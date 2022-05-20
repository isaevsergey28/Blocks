package by.saygames;

import android.content.Context;
import android.os.Handler;
import android.os.HandlerThread;
import android.util.Log;

import com.google.firebase.crashlytics.FirebaseCrashlytics;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import javax.net.ssl.SSLProtocolException;

import okhttp3.Call;
import okhttp3.Callback;
import okhttp3.CertificatePinner;
import okhttp3.Connection;
import okhttp3.EventListener;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Protocol;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import okhttp3.ResponseBody;

public class SayEndpoint {

    private static final String TAG = "SayKit";
    private static final MediaType JSON = MediaType.get("application/json; charset=utf-8");

    private static volatile Handler _globalHandler;

    public static Handler getDefaultHandler() {
        if (_globalHandler == null) {
            synchronized (SayEndpoint.class) {
                if (_globalHandler == null) {
                    HandlerThread thread = new HandlerThread("SayEndpoint Default");
                    thread.start();
                    _globalHandler = new Handler(thread.getLooper());
                }
            }
        }
        return _globalHandler;
    }

    public SayEndpoint(Context context, String name, Handler handler) {
        _context = context.getApplicationContext();
        _name = name;
        _handler = handler;
    }

    public void open() {
        _handler.post(openOp);
    }

    private final Runnable openOp = new Runnable() {
        @Override
        public void run() {
            if (_cache != null) {
                return;
            }

            _cache = new SayEndpointCache(_context, _name);
            new Thread(new Runnable() {
                @Override
                public void run() {
                    final List<StringRequestData> savedRequests = _cache.open();
                    _handler.post(new Runnable() {
                        @Override
                        public void run() {
                            onCacheOpened(savedRequests);
                        }
                    });
                }
            }).start();
        }
    };

    public void addSslPin(final String pattern, final String pin) {
        _handler.post(new Runnable() {
            @Override
            public void run() {
                _pinner.add(pattern, pin);
                _rebuildClient = true;
            }
        });
    }

    public void setUrl(final String url) {
        _url = url;
    }

    public void setContentType(String type) {
        _contentType = MediaType.get(type);
    }

    public void setDeliveryStrategy(IDeliveryStrategy delivery) {
        _delivery = delivery;
    }

    public void setAutoRetryMillis(int millis) {
        _retryDelay = millis;
    }

    public void setMaxBatchSize(int size) {
        _maxBatchSize = size;
    }

    public void setMaxRequestsCount(final int count) {
        _handler.post(new Runnable() {
            @Override
            public void run() {
                _queue.setMaxRequestsCount(count);
            }
        });
    }

    public void autoFlush(final int interval) {
        _handler.post(new Runnable() {
            @Override
            public void run() {
                if (_isAutoFlushing) {
                    return;
                }

                _isAutoFlushing = true;
                Runnable flush = new Runnable() {
                    @Override
                    public void run() {
                        doFlushRequests();
                        _handler.postDelayed(this, interval);
                    }
                };
                flush.run();
            }
        });
    }

    public void addRequest(final String body) {
        _handler.post(new Runnable() {
            @Override
            public void run() {
                doAddRequest(body, null, false);
            }
        });
    }

    public void addRequest(final String body, final boolean priority) {
        _handler.post(new Runnable() {
            @Override
            public void run() {
                doAddRequest(body, null, priority);
            }
        });
    }

    public void addBatchRequest(final String body, final IBatching batch) {
        _handler.post(new Runnable() {
            @Override
            public void run() {
                doAddRequest(body, batch, false);
            }
        });
    }

    public void addBatchRequest(final String body, final IBatching batch, final boolean priority) {
        _handler.post(new Runnable() {
            @Override
            public void run() {
                doAddRequest(body, batch, priority);
            }
        });
    }

    public void sendRequest(final String body) {
        _handler.post(new Runnable() {
            @Override
            public void run() {
                doAddRequest(body, null, false);
                doFlushRequests();
            }
        });
    }

    public void sendRequest(final String body, final boolean priority) {
        _handler.post(new Runnable() {
            @Override
            public void run() {
                doAddRequest(body, null, priority);
                doFlushRequests();
            }
        });
    }

    public void sendRequest(final String body, final IBatching batching) {
        _handler.post(new Runnable() {
            @Override
            public void run() {
                doAddRequest(body, batching, false);
                doFlushRequests();
            }
        });
    }

    public void sendRequest(final String body, final IBatching batching, final boolean priority) {
        _handler.post(new Runnable() {
            @Override
            public void run() {
                doAddRequest(body, batching, priority);
                doFlushRequests();
            }
        });
    }

    public void flushRequests() {
        _handler.post(flushRequestsOp);
    }

    private final Runnable flushRequestsOp = new Runnable() {
        @Override
        public void run() {
            doFlushRequests();
        }
    };

    //data types
    public interface IBatching {
        IStringRequest combine(List<StringRequestData> batch);
    }

    public static class AppendWithNewLineBatching implements IBatching {
        @Override
        public IStringRequest combine(List<StringRequestData> batch) {
            StringBuilder builder = new StringBuilder();
            for (IStringRequest request : batch) {
                if (builder.length() > 0) {
                    builder.append('\n');
                }
                builder.append(request.getBody());
            }
            return new BatchStringRequest(batch, this, builder.toString());
        }
    }

    public static class batch {
        public static final AppendWithNewLineBatching appendWithNewLine = new AppendWithNewLineBatching();
    }

    public enum RequestWriteStatus {
        SENDING, FLUSHED
    }

    public interface IDeliveryStrategy {
        boolean canSendNow();
        boolean isGoodResponse(Response response);
        boolean canIgnoreError(Exception error, RequestWriteStatus writeStatus);
    }

    public static class StrictServerResponseDelivery implements IDeliveryStrategy {
        final protected String _response;

        public StrictServerResponseDelivery(String response) {
            _response = response;
        }

        @Override
        public boolean canSendNow() {
            return true;
        }

        @Override
        public boolean isGoodResponse(Response response) {
            try {
                if (response.body() != null) {
                    ResponseBody body = response.peekBody(Long.MAX_VALUE);
                    return _response.equalsIgnoreCase(body.string());
                }
            } catch (Exception e) {
                Log.e(TAG, "Can't parse server response body", e);
                FirebaseCrashlytics.getInstance().recordException(e);
            }
            return false;
        }

        @Override
        public boolean canIgnoreError(Exception error, RequestWriteStatus writeStatus) {
            return false;
        }
    }

    public static class IgnoreReadErrorWithStrictServerResponseDelivery  extends StrictServerResponseDelivery {

        public IgnoreReadErrorWithStrictServerResponseDelivery(String response) {
            super(response);
        }


        @Override
        public boolean canIgnoreError(Exception error, RequestWriteStatus writeStatus) {
            return writeStatus == RequestWriteStatus.FLUSHED
                    && !(error instanceof SSLProtocolException);    //indicates write error for sure. Know it from stats.
        }
    }

    interface IStringRequest {
        String getBody();
        int getOrder();
        IBatching getBatching();
    }

    static class StringRequestData implements IStringRequest {
        private final String _body;
        private final IBatching _batching;
        private int _order;
        private final boolean _isPriority;

        public StringRequestData(String body, IBatching batching, int order, boolean isPriority) {
            _body = body;
            _batching = batching;
            _order = order;
            _isPriority = isPriority;
        }

        @Override
        public String getBody() {
            return _body;
        }

        @Override
        public IBatching getBatching() {
            return _batching;
        }

        @Override
        public int getOrder() {
            return _order;
        }

        public void setOrder(int order) {
            _order = order;
        }

        public boolean isPriority() {
            return _isPriority;
        }
    }

    private static class BatchStringRequest implements  IStringRequest {

        private final List<StringRequestData> _batch;
        private final IBatching _batching;
        private final String _body;

        public BatchStringRequest(List<StringRequestData> batch, IBatching batching, String body) {
            _batch = batch;
            _batching = batching;
            _body = body;
        }

        @Override
        public String getBody() {
            return _body;
        }

        @Override
        public int getOrder() {
            return _batch.get(_batch.size() - 1).getOrder();
        }

        @Override
        public IBatching getBatching() {
            return _batching;
        }
    }

    //fields
    private final Context _context;
    private final String _name;
    private final StringRequestQueue _queue = new StringRequestQueue();
    private final CertificatePinner.Builder _pinner = new CertificatePinner.Builder();
    private final Handler _handler;

    private OkHttpClient _httpPinnedClient;
    private boolean _rebuildClient = false;
    private SayEndpointCache _cache;
    private boolean _isOpened = false;
    private IStringRequest _pendingRequest = null;
    private volatile RequestWriteStatus _latestRequestWriteStatus = RequestWriteStatus.SENDING;
    private IDeliveryStrategy _delivery = new IgnoreReadErrorWithStrictServerResponseDelivery("ok");
    private String _url = null;
    private MediaType _contentType = JSON;
    private boolean _isAutoFlushing = false;
    private int _retryDelay = 5000;
    private int _maxBatchSize = 100;

    private void onCacheOpened(List<StringRequestData> savedRequests) {

        _isOpened = true;

        try {
            ArrayList<StringRequestData> pendingQueue = _queue.getRequests(_queue.size());
            _queue.clear();
            _queue.addAll(savedRequests);

            final int unfinishedOrder = _cache.getRequestingOrder();
            if (unfinishedOrder >= 0) {
                _cache.removeRequestsLessOrEqual(unfinishedOrder);
                _cache.removeRequestingOrder();
                _queue.removeRequestsLessOrEqual(unfinishedOrder);
            }

            int baseOrder = _queue.getLatestRequestOrder();
            for (int i = 0; i < pendingQueue.size(); ++i) {
                StringRequestData request = pendingQueue.get(i);
                request.setOrder(baseOrder + i + 1);
                _cache.cacheRequest(request);
            }


            doFlushRequests();

            _queue.addAll(pendingQueue);
        } catch (Exception e) {
            Log.e(TAG, "Error while opening endpoint cache", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        }
    }

    private StringRequestData doAddRequest(String body, IBatching batch, boolean priority) {
        StringRequestData request = new StringRequestData(
                body, batch, _queue.getLatestRequestOrder() + 1, priority);
        StringRequestData removed = _queue.addRequest(request);

        if (removed != request) {
            tryCacheRequest(request);
        }
        if (removed != null) {
            tryRemoveRequest(removed);
        }
        return request;
    }

    private void tryRemoveRequest(StringRequestData request) {
        if (!_isOpened) {
            return;
        }

        _cache.removeRequest(request);
    }

    private void tryCacheRequest(StringRequestData request) {
        if (!_isOpened) {
            return;
        }

        _cache.cacheRequest(request);
    }

    private void doFlushRequests() {
        if (_pendingRequest != null) {
            Log.w(TAG, "Skipping flush due to pending request");
            return;
        }

        if (!_isOpened) {
            Log.w(TAG, "Skipping flush. Endpoint cache is not opened");
            tryScheduleRetry();
            return;
        }

        String url = _url;
        if (url == null || url.isEmpty()) {
            Log.w(TAG, "Skipping flush due to url is not set");
            tryScheduleRetry();
            return;
        }

        if (!_delivery.canSendNow()) {
            Log.w(TAG, "Skipping flush due to _delivery forbids sending");
            tryScheduleRetry();
            return;
        }

        try {
            _pendingRequest = createPendingRequest();
        } catch (Exception e) {
            Log.e(TAG, "Can't create endpoint request", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        }

        if (_pendingRequest == null) {
            Log.d(TAG, "Nothing to send");
            return;
        }

        try {
            Request request = new Request.Builder()
                    .url(url)
                    .post(RequestBody.create(_contentType, _pendingRequest.getBody()))
                    .build();

            initPinnedClient();

            _httpPinnedClient.newCall(request).enqueue(_httpCallback);

            _cache.setRequestingOrder(_pendingRequest.getOrder());

        } catch (Exception e) {
            Log.e(TAG, "Error while sending request", e);
            handleFailure();
            FirebaseCrashlytics.getInstance().recordException(e);
        }
    }

    private final Callback _httpCallback = new Callback() {
        @Override
        public void onFailure(Call call, IOException e) {
            handleError(e);
        }

        @Override
        public void onResponse(Call call, Response response) {
            handleResponse(response);
        }
    };

    private void initPinnedClient() {
        if (_rebuildClient) {
            _httpPinnedClient = null;
            _rebuildClient = false;
        }

        if (_httpPinnedClient == null) {
            _httpPinnedClient = clientBuilder()
                    .certificatePinner(_pinner.build())
                    .build();
        }
    }

    private void tryScheduleRetry() {
        if (_isAutoFlushing) {
            return;
        }

        _handler.postDelayed(flushRequestsOp, _retryDelay);
    }

    private IStringRequest createPendingRequest() {
        ArrayList<StringRequestData> queue = _queue.getRequests(_maxBatchSize);


        if (queue.isEmpty()) {
            return null;
        }

        StringRequestData request = queue.get(0);
        IBatching batching = request.getBatching();

        if (batching == null) {
            return request;
        }

        List<StringRequestData> batch = new ArrayList<>();
        batch.add(request);
        for (int i = 1; i < queue.size(); ++i) {
            StringRequestData cur = queue.get(i);
            if (isSameBatching(cur.getBatching(), batching)) {
                batch.add(cur);
            } else {
                break;
            }
        }

        if (batch.size() == 1) {
            return batch.get(0);
        } else {
            return batching.combine(batch);
        }
    }

    private boolean isSameBatching(IBatching lhs, IBatching rhs) {
        return lhs == rhs || (lhs != null && rhs != null && lhs.getClass() == rhs.getClass());
    }

    private void handleResponse(final Response response) {
        _handler.post(new Runnable() {
            @Override
            public void run() {

                try {
                    if (_delivery.isGoodResponse(response)) {

                        finishPendingRequest();

                    } else {

                        handleFailure();

                    }
                } catch (Exception e) {
                    FirebaseCrashlytics.getInstance().recordException(e);
                    finishPendingRequest();
                }

            }
        });
    }

    private void handleError(final IOException error) {
        _handler.post(new Runnable() {
            @Override
            public void run() {

                try {

                    if (_delivery.canIgnoreError(error, _latestRequestWriteStatus)) {

                        finishPendingRequest();

                    } else {

                        handleFailure();

                    }

                } catch (Exception e) {
                    FirebaseCrashlytics.getInstance().recordException(e);
                    finishPendingRequest();
                }

            }
        });
    }

    private void finishPendingRequest() {
        try {
            int sentOrder = _pendingRequest.getOrder();
            _cache.removeRequestsLessOrEqual(sentOrder);
            _cache.removeRequestingOrder();
            _queue.removeRequestsLessOrEqual(sentOrder);
        } catch (Exception e) {
            Log.e(TAG, "Error while finishing pending request", e);
            FirebaseCrashlytics.getInstance().recordException(e);
        }
        _pendingRequest = null;
        doFlushRequests();
    }

    private void handleFailure() {
        _cache.removeRequestingOrder();
        _pendingRequest = null;
        tryScheduleRetry();
    }

    private OkHttpClient.Builder clientBuilder() {
        /*
         * OkHttp had a bug with closed sockets with Http 2.0 protocol
         * https://github.com/square/okhttp/issues/3146
         * https://github.com/square/okhttp/issues/4964
         */
        return new OkHttpClient.Builder()
                .protocols(Collections.singletonList(Protocol.HTTP_1_1))
                .retryOnConnectionFailure(false)
                .eventListener(new EventListener() {
                    @Override
                    public void callStart(Call call) {
                        _latestRequestWriteStatus = RequestWriteStatus.SENDING;
                    }

                    @Override
                    public void connectionAcquired(Call call, Connection connection) {
                        _latestRequestWriteStatus = RequestWriteStatus.SENDING;
                    }

                    @Override
                    public void requestHeadersStart(Call call) {
                        _latestRequestWriteStatus = RequestWriteStatus.SENDING;
                    }

                    @Override
                    public void responseHeadersStart(Call call) {
                        _latestRequestWriteStatus = RequestWriteStatus.FLUSHED;
                    }
                });
    }

    private static class StringRequestQueue {
        private CircularBuffer _buffer = new CircularBuffer();
        private CircularBuffer _priorityBuffer = new CircularBuffer();
        private int _maxRequestsCount = 13000;

        public void setMaxRequestsCount(int count) {
            _maxRequestsCount = count;
        }

        public int getLatestRequestOrder() {
            return Math.max(_buffer.getLatestRequestOrder(), _priorityBuffer.getLatestRequestOrder());
        }

        public void removeRequestsLessOrEqual(int order) {
            _buffer.removeRequestLessOrEqual(order);
            _priorityBuffer.removeRequestLessOrEqual(order);
        }

        public StringRequestData addRequest(StringRequestData request) {
            StringRequestData poppedRequest = null;
            if (size() >= _maxRequestsCount) {
                if (_buffer.isEmpty()) {

                    if (!request.isPriority()) {
                        return request;
                    }

                    poppedRequest = _priorityBuffer.pop();
                } else {
                    poppedRequest = _buffer.pop();
                }
            }

            if (request.isPriority()) {
                _priorityBuffer.add(request, _maxRequestsCount);
            } else {
                _buffer.add(request, _maxRequestsCount);
            }

            return poppedRequest;
        }

        //collect request from two buffers, ordered
        public ArrayList<StringRequestData> getRequests(int maxCount) {
            ArrayList<StringRequestData> list = new ArrayList<>();
            int fromSimple = 0;
            int fromPriority = 0;

            for (int i = 0; i < maxCount; ++i) {
                StringRequestData simple = null;
                StringRequestData priority = null;

                if (fromSimple < _buffer.size()) {
                    simple = _buffer.get(fromSimple);
                }
                if (fromPriority < _priorityBuffer.size()) {
                    priority = _priorityBuffer.get(fromPriority);
                }

                if (simple == null && priority == null) {
                    break;
                } else if (simple == null) {
                    list.add(priority);
                    ++fromPriority;
                } else if (priority == null) {
                    list.add(simple);
                    ++fromSimple;
                } else if (simple.getOrder() < priority.getOrder()) {
                    list.add(simple);
                    ++fromSimple;
                } else {
                    list.add(priority);
                    ++fromPriority;
                }
            }

            return list;
        }

        public int size() {
            return _buffer.size() + _priorityBuffer.size();
        }

        public void addAll(List<StringRequestData> savedRequests) {
            for (StringRequestData request: savedRequests) {
                addRequest(request);
            }
        }

        public void clear() {
            _buffer.clearBuffer();
            _priorityBuffer.clearBuffer();
        }
    }

    private static class CircularBuffer {
        private static final int SMALL_BUFFER_SIZE = 100;

        private ArrayList<StringRequestData> _buffer = new ArrayList<>();
        private int _first = -1;
        private int _last = -1;

        public int getLatestRequestOrder() {
            if (_last == -1) {
                return -1;
            } else {
                return _buffer.get(_last).getOrder();
            }
        }

        public void removeRequestLessOrEqual(int order) {
            if (_first == -1) {
                return;
            }
            while (!isEmpty()) {
                StringRequestData request = _buffer.get(_first);
                if (request.getOrder() <= order) {
                    doPop();
                } else {
                    break;
                }
            }
            optimizeBuffer();
        }

        public int size() {
            if (_buffer.isEmpty()) {
                return 0;
            } else if (_first <= _last) {
                return _last - _first + 1;
            } else {
                return _buffer.size() - _first + _last + 1;
            }
        }

        public boolean isEmpty() {
            return _buffer.isEmpty();
        }

        public StringRequestData pop() {
            StringRequestData popped = doPop();
            optimizeBuffer();
            return popped;
        }

        private void optimizeBuffer() {
            if (isEmpty() || (_first == 0 && _last == _buffer.size() - 1) || size() > SMALL_BUFFER_SIZE) {
                return;
            }

            ArrayList<StringRequestData> buffer = new ArrayList<>();
            for (int i = 0; i < size(); ++i) {
                buffer.add(get(i));
            }
            _buffer = buffer;
            _first = 0;
            _last = _buffer.size() - 1;
        }

        public StringRequestData doPop() {
            if (_first == -1) {
                return null;
            }

            StringRequestData popped = _buffer.get(_first);
            _buffer.set(_first, null);
            moveFirst();
            return popped;
        }

        private void moveFirst() {
            if (_first == _last) {
                clearBuffer();
                return;
            }

            ++_first;

            if (_first >= _buffer.size()) {
                _first = 0;
            }
        }

        public void clearBuffer() {
            if (_buffer.isEmpty()) {
                return;
            }

            _buffer = new ArrayList<>();
            _first = -1;
            _last = -1;
        }

        public void add(StringRequestData request, int maxRequestsCount) {
            if (maxRequestsCount <= 0) {
                Log.e(TAG, "SayEndpoint: maxRequestsCount <= 0");
                return;
            }

            if (_buffer.size() < maxRequestsCount && _last == _buffer.size() - 1) {
                _buffer.add(request);
                _last = _buffer.size() - 1;
                if (_first == -1) {
                    _first = 0;
                }
            } else {

                ++_last;

                if (_last >= _buffer.size()) {
                    _last = 0;
                }

                if (_last == _first) {
                    ++_first;

                    if (_first >= _buffer.size()) {
                        _first = 0;
                    }
                }

                _buffer.set(_last, request);

            }
        }

        public StringRequestData get(int index) {
            int effectiveIndex = _first + index;
            if (effectiveIndex >= _buffer.size()) {
                effectiveIndex -= _buffer.size();
            }
            return _buffer.get(effectiveIndex);
        }
    }
}
