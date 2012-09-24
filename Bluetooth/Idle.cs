using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Bluetooth
{
    class Idle
    {
        public static byte[] overridenButtons = null;

        static Bitmap createLcdIdle()
        {
            Bitmap bitmap = new Bitmap(96, 96, PixelFormat.Format1bppIndexed);
            var g = Graphics.FromImage(bitmap);

            


/*            Paint paintSmall = new Paint();
            paintSmall.setColor(Color.BLACK);
            paintSmall.setTextSize(8);
            Typeface typefaceSmall = Typeface.createFromAsset(context.getAssets(), "metawatch_8pt_5pxl_CAPS.ttf");
            paintSmall.setTypeface(typefaceSmall);

            Paint paintLarge = new Paint();
            paintLarge.setColor(Color.BLACK);
            paintLarge.setTextSize(16);
            Typeface typefaceLarge = Typeface.createFromAsset(context.getAssets(), "metawatch_16pt_11pxl.ttf");
            paintLarge.setTypeface(typefaceLarge);

            canvas.drawColor(Color.WHITE);*/

            DrawLine(g, 32);

            if (true)//WeatherData.received
            {
                // condition
//                canvas.save();
//                TextPaint paint = new TextPaint(paintSmall);
//                StaticLayout layout = new StaticLayout(WeatherData.condition, paint, 36, android.text.Layout.Alignment.ALIGN_NORMAL, 1.3f, 0, false);
//                canvas.translate(3, 40); //position the text
//                layout.draw(canvas);
//
//                canvas.restore();

                // icon
                var image = Utils.loadBitmapFromAssets("weather_rain.bmp");
                g.DrawImage(image, 37, 35);
                    

                // temperatures
                using (var brush = new SolidBrush(Color.Black))
                {
                    using (var font = new Font(MetaWatchService.Pfc.Families[0], 8, FontStyle.Regular, GraphicsUnit.Point))
                    {
                        g.DrawString("22", font, brush, 64, 46);
//                        canvas.drawText(WeatherData.temp, 64, 46, paintLarge);
//                        canvas.drawText(WeatherData.tempHigh, 64, 54, paintSmall);
//                        canvas.drawText(WeatherData.tempLow, 64, 62, paintSmall);
//                        canvas.drawText(WeatherData.city, 3, 62, paintSmall);

                    }
                }

            }
            else
            {
//                canvas.drawText("no data", 34, 50, paintSmall);
            }

            DrawLine(g, 64);

            // icons row
            //Bitmap imageI = Utils.loadBitmapFromAssets(context, "idle_icons_row.bmp");
            //canvas.drawBitmap(imageI, 0, 66, null);

            int rows = 3;
            /*
            if (Utils.isGmailAccessSupported(context))
                rows = 3;
            else
                rows = 2;
            */

            // icons
            for (int i = 0; i < rows; i++)
            {
                int slotSpace = 96 / rows;
                int slotX = slotSpace / 2 - 12;
                int iconX = slotSpace * i + slotX;
                switch (i)
                {
                    case 0:
                        g.DrawImage(Utils.loadBitmapFromAssets("idle_call.bmp"), iconX, 67);
                        break;
                    case 1:
                        g.DrawImage(Utils.loadBitmapFromAssets("idle_sms.bmp"), iconX, 67);
                        break;
                    case 2:
                        g.DrawImage(Utils.loadBitmapFromAssets("idle_gmail.bmp"), iconX, 67);
                        break;
                }
            }

            // unread counters
//            for (int i = 0; i < rows; i++)
//            {
//                String count = "";
//                switch (i)
//                {
//                    case 0:
//                        count = Integer.toString(Utils.getMissedCallsCount(context));
//                        break;
//                    case 1:
//                        count = Integer.toString(Utils.getUnreadSmsCount(context));
//                        break;
//                    case 2:
//                        if (Utils.isGmailAccessSupported(context))
//                            count = Integer.toString(Utils.getUnreadGmailCount(context, Utils.getGoogleAccountName(context), "^i"));
//                        else
//                            count = Integer.toString(Monitors.getGmailUnreadCount());
//                        break;
//                }
//
//                int slotSpace = 96 / rows;
//                int slotX = (int)(slotSpace / 2 - paintSmall.measureText(count) / 2);
//                int countX = slotSpace * i + slotX;
//
//                canvas.drawText(count, countX, 92, paintSmall);
//            }

            /*
            FileOutputStream fos = new FileOutputStream("/sdcard/test.png");
            image.compress(Bitmap.CompressFormat.PNG, 100, fos);
            fos.close();
            Log.d("ow", "bmp ok");
            */
            return bitmap;
        }

        public static void DrawLine(Graphics canvas, int y)
        {
            var pen = new Pen(Color.Black);

            int left = 3;

            for (int i = 0 + left; i < 96 - left; i += 3)
                canvas.DrawLine(pen, i, y, i + 2, y);
        }

        public static void sendLcdIdle()
        {
            var bitmap = createLcdIdle();
            //Protocol.loadTemplate(0);		
            BtProtocol.sendLcdBitmap(bitmap, MetaWatchService.WatchBuffers.IDLE);
            //Protocol.activateBuffer();
            BtProtocol.updateDisplay(0);
        }

        public static bool toIdle(Context context)
        {
            // check for parent modes

            MetaWatchService.WatchModes.IDLE = true;
            MetaWatchService.watchState = MetaWatchService.WatchStates.IDLE;

            sendLcdIdle();
            //Protocol.updateDisplay(0);

            return true;
        }

        public static void updateLcdIdle()
        {
            if (MetaWatchService.watchState == MetaWatchService.WatchStates.IDLE)
                sendLcdIdle();
        }

        public static bool isIdleButtonOverriden(byte button)
        {
            if (overridenButtons != null)
                for (int i = 0; i < overridenButtons.Length; i++)
                    if (overridenButtons[i] == button)
                        return true;
            return false;
        }
	
    }
}
