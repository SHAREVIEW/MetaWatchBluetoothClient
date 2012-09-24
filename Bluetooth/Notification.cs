using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;

namespace Bluetooth
{
    class Notification
    {
        static NotificationType lastNotification = null;
	
	public static List<NotificationType> notificationQueue = new List<Notification.NotificationType>();
	public static bool isSending = false;

        public static TimeSpan notificationTimeout = new TimeSpan(0, 0, 5);
	
	public static Object scrollRequest = new Object();
	
	public class NotificationType {
		public Bitmap bitmap;
		public int[] array;
		public byte[] buffer;
		
		public byte[] oledTop;
		public byte[] oledBottom;
		public byte[] oledScroll;
		
		public int scrollLength;
		public TimeSpan timeout;
		
		public VibratePattern vibratePattern;
	}
	
	public class VibratePattern {
		public bool _vibrate = false;
	    public int on;
	    public int off;
	    public int cycles;
				
		public VibratePattern(bool vibrate, int on, int off, int cycles) {
			this._vibrate = vibrate;
			this.on = on;
			this.off = off;
			this.cycles = cycles;
		}
	}
	
	public static void processNotificationQueue()
	{
	    if (isSending)
	        return;
	    else
	        isSending = true;

	    Thread thread = new Thread(() =>
	                                   {

	                                       if (notificationQueue.Count > 0)
	                                       {
	                                           MetaWatchService.watchState = MetaWatchService.WatchStates.NOTIFICATION;
	                                           MetaWatchService.WatchModes.NOTIFICATION = true;
	                                       }

	                                       while (notificationQueue.Count > 0)
	                                       {
	                                           NotificationType notification = notificationQueue.ElementAt(0);

	                                           //Protocol.loadTemplate(2);

	                                           if (notification.bitmap != null)
	                                               BtProtocol.sendLcdBitmap(notification.bitmap,
	                                                                        MetaWatchService.WatchBuffers.NOTIFICATION);
	                                           //						else if (notification.array != null)
	                                           //							BtProtocol.sendLcdArray(notification.array, MetaWatchService.WatchBuffers.NOTIFICATION);
	                                           //						else if (notification.buffer != null)
	                                           //							BtProtocol.sendLcdBuffer(notification.buffer, MetaWatchService.WatchBuffers.NOTIFICATION);

	                                           BtProtocol.updateDisplay(2);

	                                           if (notification.vibratePattern._vibrate)
	                                               BtProtocol.vibrate(notification.vibratePattern.on,
	                                                                  notification.vibratePattern.off,
	                                                                  notification.vibratePattern.cycles);

	                                           //						Log.d(MetaWatch.TAG, "notif bitmap sent from thread");


	                                           MetaWatchService.nap(notification.timeout);

	                                           if (MetaWatchService.WatchModes.CALL == true)
	                                           {
	                                               isSending = false;
	                                               return;
	                                           }

	                                           notificationQueue.RemoveAt(0);
	                                       }
	                                       isSending = false;

	                                       exitNotification();
	                                   }
	        );
		thread.Start();
	}

    public static void addTextNotification(String text, VibratePattern vibratePattern, TimeSpan timeout)
    {
        var notification = new NotificationType();
        notification.bitmap = BtProtocol.createTextBitmap(text);
        notification.timeout = timeout;
        if (vibratePattern == null)
            notification.vibratePattern = new VibratePattern(false, 0, 0, 0);
        else
            notification.vibratePattern = vibratePattern;
        notificationQueue.Add(notification);
        lastNotification = notification;
        processNotificationQueue();
    }

        public static void addBitmapNotification(Bitmap bitmap, VibratePattern vibratePattern, TimeSpan timeout) {
		NotificationType notification = new NotificationType();
		notification.bitmap = bitmap;
		notification.timeout = timeout;		
		if (vibratePattern == null)
			notification.vibratePattern = new VibratePattern(false, 0, 0, 0);
		else
			notification.vibratePattern	= vibratePattern;	
		notificationQueue.Add(notification);
		lastNotification = notification;
		processNotificationQueue();
	}
	
	public static void addArrayNotification(int[] array, VibratePattern vibratePattern) {
		NotificationType notification = new NotificationType();
		notification.array = array;
		notification.timeout = notificationTimeout;
		if (vibratePattern == null)
			notification.vibratePattern = new VibratePattern(false, 0, 0, 0);
		else
			notification.vibratePattern	= vibratePattern;
		notificationQueue.Add(notification);
		lastNotification = notification;
		processNotificationQueue();
	}

    public static void addBufferNotification(byte[] buffer, VibratePattern vibratePattern)
    {
        var notification = new NotificationType();
        notification.buffer = buffer;
        notification.timeout = notificationTimeout;
        if (vibratePattern == null)
            notification.vibratePattern = new VibratePattern(false, 0, 0, 0);
        else
            notification.vibratePattern = vibratePattern;
        notificationQueue.Add(notification);
        lastNotification = notification;
        processNotificationQueue();
    }


    public static void addOledNotification(byte[] top, byte[] bottom, byte[] scroll, int scrollLength, VibratePattern vibratePattern)
    {
        var notification = new NotificationType();
        notification.oledTop = top;
        notification.oledBottom = bottom;
        notification.oledScroll = scroll;
        notification.scrollLength = scrollLength;
        notification.timeout = notificationTimeout;
        if (vibratePattern == null)
            notification.vibratePattern = new VibratePattern(false, 0, 0, 0);
        else
            notification.vibratePattern = vibratePattern;
        notificationQueue.Add(notification);
        lastNotification = notification;
        processNotificationQueue();
    }



    public static void toNotification(Context context)
    {
        MetaWatchService.watchState = MetaWatchService.WatchStates.NOTIFICATION;
        MetaWatchService.WatchModes.NOTIFICATION = true;

        processNotificationQueue();
    }

        public static void exitNotification() {
		// disable notification mode
		MetaWatchService.WatchModes.NOTIFICATION = false;

        if (MetaWatchService.WatchModes.CALL == true)
            return;
        else if (MetaWatchService.WatchModes.APPLICATION == true)
            ;
        //			Application.toApp();
//        else if (MetaWatchService.WatchModes.IDLE == true)
//            Idle.toIdle();
	}
	
	public static void replay(Context context) {
		if (lastNotification != null) {
			lastNotification.vibratePattern._vibrate = false;
			notificationQueue.Add(lastNotification);
			toNotification(context);		
		}
	}
	
    }
}
