package by.saygames.screenshot;

import android.database.ContentObserver;
import android.net.Uri;
import android.os.Handler;
import android.provider.MediaStore;

import java.util.Calendar;

import by.saygames.SayKitEvents;

public class ScreenshotObserver extends ContentObserver {

    private final String MEDIA_EXTERNAL_URI_STRING = MediaStore.Images.Media.EXTERNAL_CONTENT_URI.toString();
    private long mLastTimestamp = 0;

    public ScreenshotObserver(Handler handler) {
        super(handler);
    }


    @Override
    public void onChange(boolean selfChange) {
        super.onChange(selfChange);
    }

    @Override
    public void onChange(boolean selfChange, Uri uri) {
        super.onChange(selfChange, uri);

        if (isSingleImageFile(uri)) {

            if(Calendar.getInstance().getTimeInMillis() / 1000 - mLastTimestamp > 2) {
                SayKitEvents.track("screenshot",0,0,"");

                mLastTimestamp = Calendar.getInstance().getTimeInMillis() / 1000;
            }

        }
    }

    private boolean isSingleImageFile(Uri uri) {
        return uri.toString().matches(MEDIA_EXTERNAL_URI_STRING + "/[0-9]+");
    }

    @Override
    public boolean deliverSelfNotifications() {
        return super.deliverSelfNotifications();
    }

}