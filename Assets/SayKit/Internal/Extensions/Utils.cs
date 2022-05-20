using System;

namespace SayKitInternal {

    class Utils {
        static public int currentTimestamp {
            get {
                return Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            }
        }
    }

}