package by.saygames;

import android.content.Context;
import android.content.DialogInterface;
import android.content.DialogInterface.OnClickListener;

import android.app.Activity;
import android.content.Intent;

import android.net.Uri;

import android.os.Bundle;

import androidx.appcompat.app.AlertDialog;

public class RateAppActivity extends Activity {

    @Override
    public void onCreate(Bundle instance) {
        super.onCreate(instance);
        showRateDialog();
    }

    public void showRateDialog() {


        AlertDialog.Builder builder = new AlertDialog.Builder(this);

        final Context context = this.getApplicationContext();

        builder.setTitle(context.getApplicationInfo().loadLabel(context.getPackageManager())); // "Born To Climb"
        builder.setMessage("Do you like the game? Rate it!");
        builder.setCancelable(false);

        builder.setPositiveButton("RATE NOW", new OnClickListener() {
            @Override
            public void onClick(DialogInterface dialog, int which) {
                SayKit.getActivity().startActivity(new Intent(Intent.ACTION_VIEW, Uri.parse("http://play.google.com/store/apps/details?id=" + context.getPackageName())));
                finish();
            }
        });

        builder.setNeutralButton("No, thanks", new OnClickListener() {
            @Override
            public void onClick(DialogInterface dialog, int which) {
                finish();
            }
        });

        builder.show();
    }

}
