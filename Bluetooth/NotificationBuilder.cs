using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace Bluetooth
{
    class NotificationBuilder
    {
        public enum FontSize
        {
            SMALL,
            MEDIUM,
            LARGE
        }

        public static void createSMS(String number, String text)
        {
            String name = Utils.getContactNameFromNumber(number);
                var bitmap = smartLines("message.bmp", new String[] {"SMS from", name});
                Notification.addBitmapNotification(bitmap, new Notification.VibratePattern(true, 500, 500, 3), new TimeSpan(0,0,4));
                Notification.addTextNotification(text, new Notification.VibratePattern(false, 0, 0, 0),
                                                 Notification.notificationTimeout);
        }

        public static void createK9(String sender, String subject)
        {
            var bitmap = smartLines("email.bmp", new[] {"K9 mail from", sender, subject});
            Notification.addBitmapNotification(bitmap, new Notification.VibratePattern(true, 500, 500, 3),
                                               Notification.notificationTimeout);
        }

        public static void createGmail(String sender, String email, String subject, String snippet)
        {
            Bitmap bitmap = smartLines("email.bmp", new[] {"Gmail from", sender, email, subject});
            Notification.addBitmapNotification(bitmap, new Notification.VibratePattern(true, 500, 500, 3),
                                               Notification.notificationTimeout);
            Notification.addTextNotification(snippet, new Notification.VibratePattern(false, 0, 0, 0),
                                             Notification.notificationTimeout);
        }

        public static void createGmailBlank(String recipient)
        {
            Bitmap bitmap = smartLines("email.bmp", new[] {"Gmail for", recipient});
            Notification.addBitmapNotification(bitmap, new Notification.VibratePattern(true, 500, 500, 3),
                                               Notification.notificationTimeout);
        }

        public static void createAlarm()
        {
            var bitmap = smartLines("timer.bmp", new[] {"Alarm Clock"});
            Notification.addBitmapNotification(bitmap, new Notification.VibratePattern(true, 500, 500, 3),
                                               Notification.notificationTimeout);
        }

        public static void createMusic(String artist, String track)
        {
            Bitmap bitmap = smartLines("play.bmp", new[] {track, artist});
            Notification.addBitmapNotification(bitmap, new Notification.VibratePattern(true, 150, 0, 1),
                                               Notification.notificationTimeout);
        }


        static Bitmap smartLines(String iconPath, String[] lines)
        {

            String font = null;
            int size = 8;
            int realSize = 7;

            switch (MetaWatchService.Preferences.fontSize)
            {
                case FontSize.SMALL:
                    font = "metawatch_8pt_5pxl_CAPS.ttf";
                    realSize = 5;
                    break;
                case FontSize.MEDIUM:
                    font = "metawatch_8pt_7pxl_CAPS.ttf";
                    realSize = 7;
                    break;
                case FontSize.LARGE:
                    font = "metawatch_16pt_11pxl.ttf";
                    realSize = 11;
                    size = 16;
                    break;
            }

            var bitmap = new Bitmap(96, 96, PixelFormat.Format1bppIndexed);
            var g = Graphics.FromImage(bitmap);

//            Canvas canvas = new Canvas(bitmap);
//            Paint paint = new Paint();
//            paint.setColor(Color.BLACK);
//            paint.setTextSize(size);
//            Typeface typeface = Typeface.createFromAsset(context.getAssets(), font);
//            paint.setTypeface(typeface);
//            canvas.drawColor(Color.WHITE);

            Bitmap icon = Utils.loadBitmapFromAssets(iconPath);

            int spaceForItem = 96/(1 + lines.Length);

            g.DrawImage(icon, 96 / 2 - icon.Width / 2, spaceForItem / 2 - icon.Height / 2);

            for (int i = 0; i < lines.Length; i++)
            {
                /*g.MeasureString(lines[i])
                int x = (int) (96/2 - paint.measureText(lines[i])/2);
                if (x < 0)
                    x = 0;
                int y = spaceForItem*(i + 1) + spaceForItem/2 + realSize/2;
                canvas.drawText(lines[i], x, y, paint);*/
            }

            return bitmap;
        }
    }
}
