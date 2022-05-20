package by.saygames;

import android.util.Log;

public class SayKitLog {

    private static Boolean _debugFlag = false;

    public static void SetDebugFlag(int debugFlag) {
        if (debugFlag == 0) {
            _debugFlag = false;
        } else {
            _debugFlag = true;
        }

    }



    public static void Log(String type, String tag, String message) {

        if(!_debugFlag) {
            return;
        }

        switch (type) {
            case "i":
                Log.i(tag, message);
                break;
            case "w":
                Log.w(tag, message);
                break;
            case "e":
                Log.e(tag, message);
                break;
            case "d":
                Log.d(tag, message);
                break;
            default:
                Log.v(tag, message);
                break;
        }
    }

    public static void Log(String type, String tag, String message, Throwable tr) {

        if(!_debugFlag) {
            return;
        }

        switch (type) {
            case "i":
                Log.i(tag, message, tr);
                break;
            case "w":
                Log.w(tag, message, tr);
                break;
            case "e":
                Log.e(tag, message, tr);
                break;
            case "d":
                Log.d(tag, message, tr);
                break;
            default:
                Log.v(tag, message, tr);
                break;
        }
    }


}
