package by.saygames;

import androidx.annotation.NonNull;

import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.Task;
import com.google.firebase.messaging.FirebaseMessaging;

public class SayKitNotifications {

    private static final String TAG = "SayKitNotifications";

    static String token = "";

    static public String getToken() {
        return token;
    } 

    static public void init() {

        FirebaseMessaging.getInstance().getToken()
                .addOnCompleteListener(new OnCompleteListener<String>() {
                    @Override
                    public void onComplete(@NonNull Task<String> task) {
                        if (!task.isSuccessful()) {
                            SayKitLog.Log("w", TAG, "getInstanceId failed", task.getException());
                            return;
                        }

                        // Get new Instance ID token
                        String token = task.getResult();

                        SayKitLog.Log("d", TAG, token);

                        SayKitNotifications.token = token;
                    }

                });

    } 
}