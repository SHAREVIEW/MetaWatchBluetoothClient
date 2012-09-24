using System;
using System.Drawing;
using System.IO;

namespace Bluetooth
{
    static class Utils
    {
        public static String getContactNameFromNumber(String number)
        {
            return "Kolobokiy";
        }

        public static int getUnreadSmsCount()
        {
            return 53;
        }

        public static int getMissedCallsCount()
        {
            
            return 12;
        }

        public static int getUnreadGmailCount(String account, String label)
        {
            
            return 5;
        }

        public static String getGoogleAccountName()
        {            
            return "Virtirpir";
        }

        public static Bitmap loadBitmapFromAssets(String path)
        {
            try
            {
                return new Bitmap(Path.Combine("Assets", path));
                
            }
            catch (IOException e)
            {
                //Log.d(MetaWatch.TAG, e.toString());
                return null;
            }
        }
        /*
        public static Bitmap loadBitmapFromPath(Context context, String path) {
                return BitmapFactory.decodeFile(path);
        }
        */

        public static String getVersion()
        {
//            try
//            {
//                
//                PackageManager packageManager = context.getPackageManager();
//                PackageInfo packageInfo = packageManager.getPackageInfo(context.getPackageName(), 0);
//                return packageInfo.versionName;
//            }
//            catch (NameNotFoundException e)
//            {
//            }
            return "2.2";
        }

        public static bool isGmailAccessSupported()
        {


           

            return true;
        }
	
    }
}
