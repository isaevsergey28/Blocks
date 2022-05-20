package by.saygames;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteFullException;
import android.database.sqlite.SQLiteOpenHelper;
import android.util.Base64;
import android.util.Log;


import com.google.firebase.crashlytics.FirebaseCrashlytics;

import java.io.UnsupportedEncodingException;
import java.lang.reflect.Constructor;
import java.nio.ByteBuffer;
import java.security.GeneralSecurityException;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.security.SecureRandom;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;

import javax.crypto.Cipher;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

public class SayEndpointCache {

    private static final String TAG = "SayKit";

    private static class DB extends SQLiteOpenHelper {

        public static final int DB_VERSION = 1;

        private static final String CREATE_SQL =
                "CREATE TABLE IF NOT EXISTS requests ( " +
                        "ordering INTEGER PRIMARY KEY, "+
                        "body BLOB, " +
                        "batching TEXT, " +
                        "priority INTEGER " +
                        ")";
        private static final String CREATE_KV_INT_SQL =
                "CREATE TABLE IF NOT EXISTS kv_int ( " +
                        "k TEXT PRIMARY KEY, " +
                        "v INTEGER " +
                        ")";

        public DB(Context context, String name) {
            super(context, obfuscatedDbName(name), null, DB_VERSION);
        }

        @Override
        public void onCreate(SQLiteDatabase db) {
            db.execSQL(CREATE_SQL);
        }

        @Override
        public void onOpen(SQLiteDatabase db) {
            super.onOpen(db);
            db.execSQL(CREATE_SQL);
            db.execSQL(CREATE_KV_INT_SQL);
        }

        @Override
        public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
            //upgrade logic if needed
        }
    }

    private final SecureRandom _secureRandom = new SecureRandom();
    private DB _db = null;
    private SQLiteDatabase _rw;
    private final HashMap<String, SayEndpoint.IBatching> _batchingMap = new HashMap<>();
    private final Context _context;
    private final String _name;

    public SayEndpointCache(Context context, String name) {
        _context = context;
        _name = name;
        newDbInstance();
    }

    private void newDbInstance() {
        try {
            _db = new DB(_context, _name);
        } catch (Exception e) {
            Log.e(TAG, "Can't create SayEndpointCache", e);
            FirebaseCrashlytics.getInstance().recordException(e);

        }
    }

    public List<SayEndpoint.StringRequestData> open() {
        ArrayList<SayEndpoint.StringRequestData> request = new ArrayList<>();
        Cursor cursor = null;

        try {
            _rw = _db.getWritableDatabase();

            cursor = _rw.rawQuery("SELECT ordering, body, batching, priority FROM requests ORDER BY ordering", null);

            while (cursor.moveToNext()) {
                int ordering = cursor.getInt(0);
                byte[] cipheredBody = cursor.getBlob(1);
                String body = decipher(cipheredBody);

                if (body == null) {
                    Log.e(TAG, "Can't decipher request body");
                    continue;
                }

                String batchingClass = cursor.getString(2);
                boolean priority = cursor.getInt(3) > 0;
                SayEndpoint.IBatching batching = getBatching(batchingClass);

                SayEndpoint.StringRequestData data = new SayEndpoint.StringRequestData(body, batching, ordering, priority);
                request.add(data);
            }

        } catch (Exception e) {
            Log.e(TAG, "Failed to open endpoint db", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        } finally {
            if (cursor != null) {
                cursor.close();
            }
        }

        return request;
    }

    private SayEndpoint.IBatching getBatching(String batchingClass) {
        if (batchingClass == null || batchingClass.isEmpty()) {
            return null;
        }

        if (_batchingMap.containsKey(batchingClass)) {
            return  _batchingMap.get(batchingClass);
        } else {

            SayEndpoint.IBatching batching = null;

            try {
                String effectiveClass = batchingClass;
                if ("by.saygames.SayEndpoint$AppendWithNewLineBatchingNoPin".equals(effectiveClass)) {
                    effectiveClass = "by.saygames.SayEndpoint$AppendWithNewLineBatching";
                }

                Class<?> clazz = Class.forName(effectiveClass);
                Constructor<?> ctor = clazz.getConstructor();
                Object object = ctor.newInstance();
                batching = (SayEndpoint.IBatching)object;
            } catch (Exception e) {
                Log.w(TAG, "Can't get create instance of " + batchingClass, e);
            }

            _batchingMap.put(batchingClass, batching);
            return batching;
        }
    }

    public void cacheRequest(SayEndpoint.StringRequestData data) {
        if (_rw == null) {
            Log.e(TAG, "SayEndpointCache is not opened");
            return;
        }

        try {
            byte[] encryptedBody = cipher(data.getBody());

            ContentValues values = new ContentValues();

            values.put("ordering", data.getOrder());
            values.put("body", encryptedBody);
            values.put("batching", getBatching(data));
            values.put("priority", data.isPriority() ? 1 : 0);

            _rw.insertOrThrow("requests", null, values);
        } catch (Exception e) {
            Log.e(TAG, "SayEndpointCache.cacheRequest error", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        }
    }

    private static SecretKeySpec key(String left, String right) {
        byte[] key = left.getBytes();
        byte[] mask = right.getBytes();
        for (int i = 0; i < key.length; ++i) {
            key[i] = (byte) (key[i] ^ mask[i]);
        }
        return new SecretKeySpec(key, "AES");
    }

    private byte[] cipher(String body) throws GeneralSecurityException, UnsupportedEncodingException {
        byte[] bodyBytes = body.getBytes("UTF-8");

        byte[] iv = new byte[12];
        _secureRandom.nextBytes(iv);
        final Cipher cipher = Cipher.getInstance("AES/GCM/NoPadding");
        IvParameterSpec parameterSpec = new IvParameterSpec(iv); //128 bit auth tag length
        SecretKeySpec secretKey = key(
                "\"E#&@p,<,u+eE(]r(:(c`&h[88Z'u$4;",
                "p'N[)sy3Vh9h4K(VazA7_w#BmA?e!D4j");
        cipher.init(Cipher.ENCRYPT_MODE, secretKey, parameterSpec);

        byte[] cipherText = cipher.doFinal(body.getBytes("UTF-8"));
        ByteBuffer byteBuffer = ByteBuffer.allocate(16 + 4 + iv.length + cipherText.length);
        byteBuffer.put(getMd5Digest(bodyBytes));
        byteBuffer.putInt(iv.length);
        byteBuffer.put(iv);
        byteBuffer.put(cipherText);

        return byteBuffer.array();
    }

    private String decipher(byte[] cipheredBody) {
        try {
            ByteBuffer byteBuffer = ByteBuffer.wrap(cipheredBody);
            byte[] digest = new byte[16];
            byteBuffer.get(digest);
            int ivLength = byteBuffer.getInt();
            if(ivLength < 12 || ivLength >= 16) { // check input parameter
                throw new IllegalArgumentException("invalid iv length");
            }
            byte[] iv = new byte[ivLength];
            byteBuffer.get(iv);
            byte[] cipherText = new byte[byteBuffer.remaining()];
            byteBuffer.get(cipherText);

            final Cipher cipher = Cipher.getInstance("AES/GCM/NoPadding");
            SecretKeySpec secretKey = key(
                    "\"E#&@p,<,u+eE(]r(:(c`&h[88Z'u$4;",
                    "p'N[)sy3Vh9h4K(VazA7_w#BmA?e!D4j");
            cipher.init(Cipher.DECRYPT_MODE, secretKey, new IvParameterSpec(iv));
            byte[] plainText= cipher.doFinal(cipherText);
            byte[] md5 = getMd5Digest(plainText);

            if (!Arrays.equals(digest, md5)) {
                throw new Exception("Body digests are not equal");
            }

            return new String(plainText);
        } catch (Exception e) {
            Log.e(TAG, "Can't decipher request body", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        }

        return null;
    }
    
    private static String obfuscatedDbName(String name) {
        try {
            return Base64.encodeToString(("sayendpoint_" + name).getBytes("UTF-8"),
                    Base64.NO_PADDING | Base64.NO_WRAP | Base64.URL_SAFE) + ".db";
        } catch (UnsupportedEncodingException e) {
            FirebaseCrashlytics.getInstance().recordException(e);
            return Base64.encodeToString(("sayendpoint_" + name).getBytes(),
                    Base64.NO_PADDING | Base64.NO_WRAP | Base64.URL_SAFE) + ".db";
        }
    }

    private static byte[] getMd5Digest(byte[] data) throws NoSuchAlgorithmException {
        MessageDigest md = MessageDigest.getInstance("MD5");
        md.update(data);
        return md.digest();
    }

    private String getBatching(SayEndpoint.StringRequestData data) {
        SayEndpoint.IBatching batching = data.getBatching();
        if (batching == null) {
            return null;
        } else {
            return batching.getClass().getName();
        }
    }

    public void removeRequestsLessOrEqual(int order) {
        if (_rw == null) {
            Log.e(TAG, "SayEndpointCache is not opened");
            return;
        }

        try {
            int deleted = _rw.delete("requests",
                    "ordering <= ?", new String[]{Integer.toString(order)});
            Log.d(TAG, deleted + "  requests were sent successfully and removed from db");
        } catch (Exception e) {

            purgeDb();

            Log.e(TAG, "SayEndpointCache.removeRequestsLessOrEqual error", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        }
    }

    public void removeRequest(SayEndpoint.StringRequestData request) {
        if (_rw == null) {
            Log.e(TAG, "SayEndpointCache is not opened");
            return;
        }

        try {
            _rw.delete("requests",
                    "ordering = ?", new String[]{Integer.toString(request.getOrder())});
        } catch (Exception e) {

            purgeDb();

            Log.e(TAG, "SayEndpointCache.removeRequestsLessOrEqual error", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        }
    }

    private void purgeDb() {
        if (_db == null) return;

        close();
        try {
            _context.deleteDatabase(obfuscatedDbName(_name));
        } catch (Exception e) {
            Log.e(TAG, "SayEndpointCache.purgeDb error", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        }
        newDbInstance();
        prepareDb();
    }

    private void prepareDb() {
        try {
            _rw = _db.getWritableDatabase();
        } catch (Exception e) {
            Log.e(TAG, "SayEndpointCache.prepareDb error", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        }
    }

    private void close() {
        try {
            if (_rw != null) {
                _rw.close();
            }
            _db.close();
        } catch (Exception e) {
            Log.e(TAG, "SayEndpointCache.removeRequestsLessOrEqual error", e);
        }
        _db = null;
        _rw = null;
    }

    public void setRequestingOrder(int order) {
        if (_rw == null) {
            Log.e(TAG, "SayEndpointCache is not opened");
            return;
        }

        try {
            ContentValues values = new ContentValues();
            values.put("k", "requestingOrder");
            values.put("v", order);
            _rw.insertWithOnConflict("kv_int", null, values, SQLiteDatabase.CONFLICT_REPLACE);
        } catch (Exception e) {
            Log.e(TAG, "SayEndpointCache.setRequestingOrder error", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        }
    }

    public void removeRequestingOrder() {
        if (_rw == null) {
            Log.e(TAG, "SayEndpointCache is not opened");
            return;
        }

        try {
            _rw.delete("kv_int", "k = ?", new String[]{"requestingOrder"});
        } catch (Exception e) {
            Log.e(TAG, "SayEndpointCache.removeRequestingOrder error", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        }
    }

    public int getRequestingOrder() {
        if (_rw == null) {
            Log.e(TAG, "SayEndpointCache is not opened");
            return -1;
        }

        Cursor cursor = null;
        try {
             cursor = _rw.rawQuery("SELECT v FROM kv_int WHERE k = 'requestingOrder'", null);

             if (cursor.moveToNext()) {
                 return cursor.getInt(0);
             }

        } catch (Exception e) {
            Log.e(TAG, "SayEndpointCache.getRequestingOrder error", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        } finally {
            if (cursor != null) {
                try {
                    cursor.close();
                } catch (Exception e) {
                    FirebaseCrashlytics.getInstance().recordException(e);
                }
            }
        }
        return -1;
    }
}
