package by.saygames;

import java.util.concurrent.ScheduledThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

public class SayExceptionHandler {

    public static void initUncaughtExceptionHandler() {

        final ScheduledThreadPoolExecutor c = new ScheduledThreadPoolExecutor(1);
        c.schedule(new Runnable() {
            @Override
            public void run() {
                final Thread.UncaughtExceptionHandler defaultHandler = Thread.getDefaultUncaughtExceptionHandler();

                Thread.setDefaultUncaughtExceptionHandler(new Thread.UncaughtExceptionHandler() {
                    @Override
                    public void uncaughtException(final Thread paramThread,final Throwable paramThrowable) {
                        // do my amazing stuff here

                        StringBuilder extra = new StringBuilder();

                        extra.append(paramThrowable.getLocalizedMessage());


                        StackTraceElement[] trace = paramThrowable.getStackTrace();
                        for (StackTraceElement traceElement : trace)
                        {
                            extra.append("\tat ").append(traceElement);
                        }

                        by.saygames.SayKitEvents.trackImmediately("crash", 0, 0, extra.toString());

                        defaultHandler.uncaughtException(paramThread, paramThrowable);
                    }
                });
            }

            //added delay, because we need to wait for crashlytic.
        }, 5, TimeUnit.SECONDS);
    }
}
