package by.saygames;

import android.os.Bundle;

import com.facebook.FacebookSdk;
import com.unity3d.player.UnityPlayerActivity;

import java.util.Calendar;

//import by.saygames.screenshot.ScreenshotHandlerManager;

public class SayKitActivity extends UnityPlayerActivity {

//    ScreenshotHandlerManager mScreenshotHandlerManager;

    private boolean isApplicationStarted = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        if (!isApplicationStarted) {
            isApplicationStarted = true;

            SayKitEvents.ApplicationStartTimestamp = (int)(System.currentTimeMillis()/1000);
        }

//        mScreenshotHandlerManager = new ScreenshotHandlerManager(getContentResolver());
    }

    @Override
    protected void onDestroy ()
    {
        super.onDestroy();

//        mScreenshotHandlerManager.unregister();
    }

    @Override protected void onResume()
    {
        super.onResume();

//        mScreenshotHandlerManager.register();
    }

}
