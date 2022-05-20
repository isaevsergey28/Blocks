package by.saygames.screenshot;

import android.content.ContentResolver;
import android.database.ContentObserver;
import android.os.Handler;
import android.os.HandlerThread;
import android.provider.MediaStore;

public class ScreenshotHandlerManager {
    private final HandlerThread mHandlerThread;
    private final Handler mHandler;
    private final ContentResolver mContentResolver;
    private final ContentObserver mContentObserver;

    public ScreenshotHandlerManager(ContentResolver contentResolver) {
        mHandlerThread = new HandlerThread("ShotWatch");
        mHandlerThread.start();

        mHandler = new Handler(mHandlerThread.getLooper());
        mContentResolver = contentResolver;
        mContentObserver = new ScreenshotObserver(mHandler);
    }

    public void register() {
        mContentResolver.registerContentObserver(
                MediaStore.Images.Media.EXTERNAL_CONTENT_URI,
                true,
                mContentObserver
        );
    }

    public void unregister() {
        mContentResolver.unregisterContentObserver(mContentObserver);
    }
}