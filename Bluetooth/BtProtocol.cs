using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Bluetooth
{
    class BtProtocol
    {
                

        	public static byte REPLAY = 30;
	
	public static List<byte[]> sendQueue = new List<byte[]>();
	public static bool isSending = false;

        public static Stream OutStream;


        public static void sendLcdBitmap(Bitmap bitmap, MetaWatchService.WatchBuffers bufferType)
	{
//	    var pixelArray = new byte[96*96];
//        bitmap.pi
//	    bitmap.getPixels(pixelArray, 0, 96, 0, 0, 96, 96);
//
        sendLcdArray(bitmap, bufferType);
	}


    public static void sendLcdArray(Bitmap bitmap, MetaWatchService.WatchBuffers bufferType)
	{
	    var send = new byte[1152];

        var i = 0;
        for(var y = 0; y < 96; y++, i++)
        {
            var p = new byte[8];
            for(var x = 0; x < 96; x += 8)
            {

	            for (int j = 0; j < 8; j++)
	            {
                    if (bitmap.GetPixel(y, x) == System.Drawing.Color.White)
	                    p[j] = 0;
	                else

	                    p[j] = 1;
	            }

            }
	        send[i] = (byte) (p[7] << 7 | p[6] << 6 | p[5] << 5 | p[4] << 4 | p[3] << 3 | p[2] << 2 | p[1] << 1 | p[0]);
        }
	    sendLcdBuffer(send, bufferType);
	}


    static void sendLcdBuffer(byte[] buffer, MetaWatchService.WatchBuffers bufferType)
    {
		if (MetaWatchService.connectionState != MetaWatchService.ConnectionState.CONNECTED)
			return;
		
		int i = 0;
		if (bufferType == MetaWatchService.WatchBuffers.IDLE)
			i = 30;
		
		for (; i < 96; i += 2) {
			byte[] bytes = new byte[30];

			bytes[0] = BtMessage.Start;
			bytes[1] = (byte) (bytes.Length+2); // packet length
			bytes[2] = (byte) BtMessage.Message.WriteBuffer; 
			bytes[3] = (byte) bufferType; 

			bytes[4] = (byte) i; // row A
			for (int j = 0; j < 12; j++)
				bytes[j + 5] = buffer[i * 12 + j];
			
			bytes[4+13] = (byte) (i+1); // row B
			for (int j = 0; j < 12; j++)
				bytes[j + 5 + 13] = buffer[i * 12 + j + 12];

			sendQueue.Add(bytes);
		}
				
		processSendQueue();
	}
		


	public static void processSendQueue() {
		if (isSending)
			return;
		else
			isSending = true;
		
		var thread = new Thread(()=> {
			
//				Log.d(MetaWatch.TAG, "entering send queue");
				while (sendQueue.Count > 0) 
                {
					try {
						send(sendQueue[0]);
						//Log.d(MetaWatch.TAG, "  part sent");
						sendQueue.RemoveAt(0);
						Thread.Sleep(10);//Preferences.packetWait
					} catch (IOException e) {
						sendQueue.Clear();
					} 
				}
//				Log.d(MetaWatch.TAG, "send queue finished");
				isSending = false;
			
		});
		thread.Start();
	}
	
	public static void send(byte[] bytes) 
    {
		if (bytes == null)
			return;

        if (OutStream == null)
			throw new IOException("OutputStream is null");

        OutStream.Write(bytes, 0, bytes.Length);
	    var crc = Crc(bytes);
        OutStream.Write(crc, 0, crc.Length);
        OutStream.Flush();

	}
	
	public static void SendAdvanceHands() {
		try
		{
		    var date = DateTime.Now;

			var hour = date.Hour;
			var minute = date.Minute;
			var second = date.Second;

			var bytes = new byte[7];

			bytes[0] = BtMessage.Start;
			bytes[1] = 9; // length
			bytes[2] = (byte) BtMessage.Message.AdvanceWatchHandsMsg;
			bytes[3] = 0x04; // complete

			bytes[4] = (byte) hour;
			bytes[5] = (byte) minute;
			bytes[6] = (byte) second;

			//send(bytes);
		} catch (Exception x) {
		}
	}
	
        public static void SendRtcNow()
        {
            try
            {
               /* bool isMMDD = true;
                char[] ch = DateFormat.getDateFormatOrder(context);

                for (int i = 0; i < ch.length; i++)
                {
                    if (ch[i] == DateFormat.DATE)
                    {
                        isMMDD = false;
                        break;
                    }
                    if (ch[i] == DateFormat.MONTH)
                    {
                        isMMDD = true;
                        break;
                    }
                }*/
                var isMMDD = true;
                var date = DateTime.Now;
//                Calendar calendar = Calendar.getInstance();
//                calendar.setTime(date);
                int year = date.Year;

                var bytes = new byte[14];

                bytes[0] = BtMessage.Start;
                bytes[1] = (byte)(bytes.Length + 2); // length
                bytes[2] = (byte) BtMessage.Message.SetRealTimeClock;
                bytes[3] = 0x00; // not used

                bytes[4] = (byte)(year / 256);
                bytes[5] = (byte)(year % 256);
                bytes[6] = (byte)(date.Month);
                bytes[7] = (byte)date.Day;
                bytes[8] = (byte)(date.DayOfWeek);
                bytes[9] = (byte)date.Hour;
                bytes[10] = (byte)date.Minute;
                bytes[11] = (byte)date.Second;
                if (false)//DateFormat.is24HourFormat(context)
                    bytes[12] = (byte)1; // 24hr
                else
                    bytes[12] = (byte)0; // 12hr
                if (isMMDD)
                    bytes[13] = (byte)0; // mm/dd
                else
                    bytes[13] = (byte)1; // dd/mm
                var crc = Crc(bytes);
                //send(bytes);

                MetaWatchService.inputOutputStream.Write(bytes, 0, bytes.Length);
                MetaWatchService.inputOutputStream.Write(crc, 0, crc.Length);
//                sendQueue.Add(bytes);
//                processSendQueue();

            }
            catch (Exception x)
            {
            }
        }

	
	
        public static byte[] Crc(byte[] bytes)
        {
            var result = new byte[2];
            var crc = (ushort)0xFFFF;
            for (var j = 0; j < bytes.Length; j++)
            {
                var c = bytes[j];
                for (var i = 7; i >= 0; i--)
                {
                    var c15 = ((crc >> 15 & 1) == 1);
                    var bit = ((c >> (7 - i) & 1) == 1);
                    crc <<= 1;
                    if (c15 ^ bit)
                        crc ^= 0x1021; // 0001 0000 0010 0001 (0, 5, 12)
                }
            }
            var crc2 = crc - 0xffff0000;
            result[0] = (byte)(crc2 % 256);
            result[1] = (byte)(crc2 / 256);
            return result;
        }

        public enum FontSize
        {
            Small = 1,
            Medium = 2,
            Large = 3
        }


        public static Bitmap createTextBitmap(String text)
        {



            String font = null;
            int size = 8;

            switch (FontSize.Medium) //Preferences.fontSize
            {
                case FontSize.Small:
                    font = "metawatch_8pt_5pxl_CAPS.ttf";
                    break;
                case FontSize.Medium:
                    font = "metawatch_8pt_7pxl_CAPS.ttf";
                    break;
                case FontSize.Large:
                    font = "metawatch_16pt_11pxl.ttf";
                    size = 16;
                    break;
            }

            const int width = 96;
            const int height = width;
            const int stride = width/8;
            //        var pixels = new byte[height * stride];

            var retBmp = new Bitmap(width, height, PixelFormat.Format1bppIndexed);

            //            var g = Graphics.FromImage(retBmp);





            //		Canvas canvas = new Canvas(bitmap);
            //		Paint paint = new Paint();
            //		paint.setColor(Color.BLACK);		
            //		paint.setTextSize(size);
            //		Typeface typeface = Typeface.createFromAsset(context.getAssets(), font);		
            //		paint.setTypeface(typeface);		
            //		canvas.drawColor(Color.WHITE);
            //		canvas = breakText(canvas, text, paint, 0, 0);

            return retBmp;
        }



/*
	public static Canvas breakText(Canvas canvas, String text, Paint pen, int x, int y) {		
		TextPaint textPaint = new TextPaint(pen);
		StaticLayout staticLayout = new StaticLayout(text, textPaint, 98, android.text.Layout.Alignment.ALIGN_NORMAL, 1.3f, 0, false);
		canvas.translate(x, y); // position the text
		staticLayout.draw(canvas);		
		return canvas;		
	}
	*/
        public static void loadTemplate(int mode)
        {
            byte[] bytes = new byte[5];

            bytes[0] = BtMessage.Start;
            bytes[1] = (byte) (bytes.Length + 2); // length
            bytes[2] = (byte) BtMessage.Message.LoadTemplate; // load template
            bytes[3] = (byte) mode;

            bytes[4] = (byte) 0; // write all "0"

            sendQueue.Add(bytes);
            processSendQueue();
        }

        public static void activateBuffer(int mode)
        {

            byte[] bytes = new byte[4];

            bytes[0] = BtMessage.Start;
            bytes[1] = (byte) (bytes.Length + 2); // length
            bytes[2] = (byte) BtMessage.Message.ConfigureIdleBufferSize; // activate buffer
            bytes[3] = (byte) mode;

            sendQueue.Add(bytes);
            processSendQueue();
        }

        public static void updateDisplay(int bufferType)
        {

            byte[] bytes = new byte[4];

            bytes[0] = BtMessage.Start;
            bytes[1] = (byte) (bytes.Length + 2); // length
            bytes[2] = (byte) BtMessage.Message.UpdateDisplay; // update display
            bytes[3] = (byte) (bufferType + 16);

            sendQueue.Add(bytes);
            processSendQueue();
        }

        public static void vibrate(int on, int off, int cycles)
        {

            byte[] bytes = new byte[10];

            bytes[0] = BtMessage.Start;
            bytes[1] = 12; // delka
            bytes[2] = (byte) BtMessage.Message.SetVibrateMode; // set vibrate
            bytes[3] = 0x00; // unused

            bytes[4] = 0x01; // enabled
            bytes[5] = (byte) (on%256);
            bytes[6] = (byte) (on/256);
            bytes[7] = (byte) (off%256);
            bytes[8] = (byte) (off/256);
            bytes[9] = (byte) cycles;

            sendQueue.Add(bytes);
            processSendQueue();
        }

        public static void writeBuffer()
        {

            //for (int j = 0; j < 96; j = j+2) {		
            byte[] bytes = new byte[17];

            bytes[0] = BtMessage.Start;
            bytes[1] = (byte) (bytes.Length + 2); // length
            bytes[2] = (byte) BtMessage.Message.WriteBuffer;
            //bytes[3] = 0x02; // notif, two lines
            //bytes[3] = 18;
            bytes[3] = 0;
            //bytes[3] = 16;

            bytes[4] = 31;

            bytes[5] = 15;
            bytes[6] = 15;
            bytes[7] = 15;
            bytes[8] = 15;
            bytes[9] = 15;
            bytes[10] = 15;
            bytes[11] = 15;
            bytes[12] = 15;
            bytes[13] = 15;
            bytes[14] = 15;
            bytes[15] = 15;
            bytes[16] = 15;

            sendQueue.Add(bytes);
            processSendQueue();
            //}
        }

        public static void enableButton(int button, int type, int code) {
		byte[] bytes = new byte[9];
		
		bytes[0] = BtMessage.Start;
		bytes[1] = (byte) (bytes.Length+2); // length
		bytes[2] = (byte) BtMessage.Message.EnableButtonMsg; 
		bytes[3] = 0; // not used
		
		bytes[4] = 0; // idle
		bytes[5] = (byte) button; 
		bytes[6] = (byte) type; // immediate
		bytes[7] = 0x34;
		bytes[8] = (byte) code;
		
		sendQueue.Add(bytes);
		processSendQueue();
	}
	
	public static void disableButton(int button, int type) {
		byte[] bytes = new byte[7];
		
		bytes[0] = BtMessage.Start;
		bytes[1] = (byte) (bytes.Length+2); // length
		bytes[2] = (byte) BtMessage.Message.DisableButtonMsg;
		bytes[3] = 0; // not used
		
		bytes[4] = 0; // idle
		bytes[5] = (byte) button; 
		bytes[6] = (byte) type; // immediate
		
		sendQueue.Add(bytes);
		processSendQueue();
	}
	
	public static void enableReplayButton() {
		enableButton(1, 0, REPLAY);		
	}
	
	public static void disableReplayButton() {
		disableButton(1, 0);		
	}
	
	public static void enableMediaButtons() {
		//enableMediaButton(0); // right top
		//enableMediaButton(1); // right middle
		//enableMediaButton(2); // right bottom
		
		enableButton(3, 0, 0); // left bottom
/*
		enableButton(3, 1, MediaControl.VOLUME_DOWN); // left bottom
		enableButton(3, 2, MediaControl.PREVIOUS); // left bottom
		
		//enableMediaButton(5, 0, 0); // left middle
		enableButton(5, 0, MediaControl.TOGGLE); // left middle
		
		enableButton(6, 0, 0); // left top
		enableButton(6, 1, MediaControl.VOLUME_UP); // left top
		enableButton(6, 2, MediaControl.NEXT); // left top
*/
	}
	
	public static void disableMediaButtons() {
		disableButton(3, 0);
		disableButton(3, 1);
		disableButton(3, 2);
		
		disableButton(5, 0);
		
		disableButton(6, 0);
		disableButton(6, 1);
		disableButton(6, 2);
	}
	
	public static void readButtonConfiguration() {
		byte[] bytes = new byte[9];
		
		bytes[0] = BtMessage.Start;
		bytes[1] = (byte) (bytes.Length+2); // length
		bytes[2] = (byte) BtMessage.Message.ReadButtonConfigMsg; 
		bytes[3] = 0; // not used
		
		bytes[4] = 0; 
		bytes[5] = 1; 
		bytes[6] = 2; // press type 
		bytes[7] = 0x34; 
		bytes[8] = 0; 
		
		sendQueue.Add(bytes);
		processSendQueue();
	}
	
        
	public static void configureMode() {
		
		var bytes = new byte[6];

		bytes[0] = BtMessage.Start;
		bytes[1] = (byte) (bytes.Length+2); // length
		bytes[2] = (byte) BtMessage.Message.ConfigureMode; 
		bytes[3] = 0;
		
		bytes[4] = 10; 
		bytes[5] = (byte) (MetaWatchService.Preferences.invertLCD ? 1 : 0); // invert

		sendQueue.Add(bytes);
		processSendQueue();
	}

    public static void GetDeviceType()
    {

        var bytes = new byte[4];

        bytes[0] = BtMessage.Start;
        bytes[1] = (byte) (bytes.Length + 2); // length
        bytes[2] = (byte) BtMessage.Message.GetDeviceType;
        bytes[3] = 0;

        var crc = Crc(bytes);

        MetaWatchService.inputOutputStream.Write(bytes, 0, bytes.Length);
        MetaWatchService.inputOutputStream.Write(crc, 0, crc.Length);

//        sendQueue.Add(bytes);
//        processSendQueue();
    }

    public static void UploadDiary(Diary diary)
    {
        //  DiaryWriteRecord = 0xbd,
        // Options = 0 (3-8 бит - номер ячейки)
        // Запись первой части данных в ячейку (статус и настройки)
        // 0 - Статус
        // 1 - вид
        // 2,3,4 год(от 12 года), мес, день
        // 5, 6 - время

        //  DiaryWriteRecord = 0xbd,
        // Options = 1 (3-8 бит - номер ячейки)
        // Запись второй части данных (текст)

        byte[] bytes, crc;

        for (var i = 0; i < 13 && i < diary.Rec.Count; i++)//!diary.Rec.Count
        {
            bytes = new byte[12];

            bytes[0] = BtMessage.Start;
            bytes[1] = (byte)(bytes.Length + 2); // length
            bytes[2] = (byte)BtMessage.Message.DiaryWriteRecord;
            bytes[3] = (byte) i;
            bytes[4] = (byte) diary.Rec[i].Status;
            bytes[5] = (byte) diary.Rec[i].Alarm;
            bytes[6] = (byte) (diary.Rec[i].DateTime.Year - 1900);
            bytes[7] = (byte) (diary.Rec[i].DateTime.Month); 
            bytes[8] = (byte) (diary.Rec[i].DateTime.Day);
            bytes[9] = (byte) (diary.Rec[i].DateTime.DayOfWeek);
            bytes[10] = (byte) (diary.Rec[i].DateTime.Hour);
            bytes[11] = (byte) (diary.Rec[i].DateTime.Minute);

            crc = Crc(bytes);

            MetaWatchService.inputOutputStream.Write(bytes, 0, bytes.Length);
            MetaWatchService.inputOutputStream.Write(crc, 0, crc.Length);

            Thread.Sleep(200);
            // Запись текста
            bytes = new byte[27];

            bytes[0] = BtMessage.Start;
            bytes[1] = (byte)(bytes.Length + 2); // length
            bytes[2] = (byte)BtMessage.Message.DiaryWriteRecord;
            bytes[3] = (byte) (0x80 | (byte)i);

            //var encoding = new ASCIIEncoding();
            var tmpTxt = diary.Rec[i].Value.Trim().ToUpper();
            var messageBytes = Encoding.GetEncoding(1251).GetBytes(tmpTxt); //encoding.GetBytes(diary.Rec[i].Value);
            for (var j = 0; j < 21 && j < messageBytes.Length; j++)
            {
                bytes[4 + j] = messageBytes[j];
            }

            crc = Crc(bytes);

            MetaWatchService.inputOutputStream.Write(bytes, 0, bytes.Length);
            MetaWatchService.inputOutputStream.Write(crc, 0, crc.Length);
            Thread.Sleep(200);


            /*bytes = new byte[12];

            bytes[0] = BtMessage.Start;
            bytes[1] = (byte)(bytes.Length + 2); // length
            bytes[2] = (byte)BtMessage.Message.DiaryWriteRecord;
            bytes[3] = (byte)(i + 1);
            bytes[4] = (byte)diary.Rec[i + 1].Status;
            bytes[5] = (byte)diary.Rec[i + 1].Alarm;
            bytes[6] = (byte)(diary.Rec[i + 1].DateTime.Year - 2012);
            bytes[7] = (byte)(diary.Rec[i + 1].DateTime.Month);
            bytes[8] = (byte)(diary.Rec[i + 1].DateTime.Day);
            bytes[9] = (byte)(diary.Rec[i + 1].DateTime.DayOfWeek);
            bytes[10] = (byte)(diary.Rec[i + 1].DateTime.Hour);
            bytes[11] = (byte)(diary.Rec[i + 1].DateTime.Minute);

            crc = Crc(bytes);

            MetaWatchService.inputOutputStream.Write(bytes, 0, bytes.Length);
            MetaWatchService.inputOutputStream.Write(crc, 0, crc.Length);
            */
        }

            bytes = new byte[7];

            bytes[0] = BtMessage.Start;
            bytes[1] = (byte)(bytes.Length + 2); // length
            bytes[2] = (byte)BtMessage.Message.DiaryWriteEnd;
            bytes[3] = 0;

            crc = Crc(bytes);

            MetaWatchService.inputOutputStream.Write(bytes, 0, bytes.Length);
            MetaWatchService.inputOutputStream.Write(crc, 0, crc.Length);
    }


    public static void ReadBatteryVoltage()
    {

        var bytes = new byte[4];

        bytes[0] = BtMessage.Start;
        bytes[1] = (byte)(bytes.Length + 2); // length
        bytes[2] = (byte)BtMessage.Message.ReadBatteryVoltageMsg;
        bytes[3] = 0;

        var crc = Crc(bytes);

        MetaWatchService.inputOutputStream.Write(bytes, 0, bytes.Length);
        MetaWatchService.inputOutputStream.Write(crc, 0, crc.Length);

        //        sendQueue.Add(bytes);
        //        processSendQueue();
    }

    public static void EnableAccelerometer()
    {

        var bytes = new byte[4];

        bytes[0] = BtMessage.Start;
        bytes[1] = (byte)(bytes.Length + 2); // length
        bytes[2] = (byte)BtMessage.Message.AccelerometerEnableMsg;
        bytes[3] = 0;

        var crc = Crc(bytes);

        MetaWatchService.inputOutputStream.Write(bytes, 0, bytes.Length);
        MetaWatchService.inputOutputStream.Write(crc, 0, crc.Length);

        //        sendQueue.Add(bytes);
        //        processSendQueue();
    }

    public static void DisableAccelerometer()
    {

        var bytes = new byte[4];

        bytes[0] = BtMessage.Start;
        bytes[1] = (byte)(bytes.Length + 2); // length
        bytes[2] = (byte)BtMessage.Message.AccelerometerDisableMsg;
        bytes[3] = 0;

        var crc = Crc(bytes);

        MetaWatchService.inputOutputStream.Write(bytes, 0, bytes.Length);
        MetaWatchService.inputOutputStream.Write(crc, 0, crc.Length);

        //        sendQueue.Add(bytes);
        //        processSendQueue();
    }

    public static void AccelerometerAccess()
    {
        const byte accelerometerAccessWriteOption = 0;
        const byte accelerometerAccessReadOption = 1;
        var bytes = new byte[4];

        bytes[0] = BtMessage.Start;
        bytes[1] = (byte)(bytes.Length + 2); // length
        bytes[2] = (byte)BtMessage.Message.AccelerometerAccessMsg;
        bytes[3] = accelerometerAccessReadOption;

        var crc = Crc(bytes);

        MetaWatchService.inputOutputStream.Write(bytes, 0, bytes.Length);
        MetaWatchService.inputOutputStream.Write(crc, 0, crc.Length);

        //        sendQueue.Add(bytes);
        //        processSendQueue();
    }




        public static void queryNvalTime() {
		
		byte[] bytes = new byte[7];

		bytes[0] = BtMessage.Start;
		bytes[1] = (byte) (bytes.Length+2); // length
		bytes[2] = (byte) BtMessage.Message.NvalOperationMsg;
		bytes[3] = 0x01; // read
		
		bytes[4] = 0x09; 
		bytes[5] = 0x20;
		bytes[6] = 0x01; // size

		sendQueue.Add(bytes);
		processSendQueue();
	}
	
        /*
	public static void setNvalTime(boolean militaryTime) {
		
		byte[] bytes = new byte[8];

		bytes[0] = BtMessage.Start;
		bytes[1] = (byte) (bytes.Length+2); // length
		bytes[2] = (byte) BtMessage.Message.NvalOperationMsg;
		bytes[3] = 0x02; // write
		
		bytes[4] = 0x09; 
		bytes[5] = 0x20;
		bytes[6] = 0x01; // size
		if (militaryTime)
			bytes[7] = 0x01; // 24 hour mode
		else
			bytes[7] = 0x00; // 12 hour mode

		sendQueue.Add(bytes);
		processSendQueue();
	}*/
	
	
    }
}
