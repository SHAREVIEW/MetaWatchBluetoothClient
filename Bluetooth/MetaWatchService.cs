using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace Bluetooth
{
    class MetaWatchService
    {
	public static BluetoothClient bluetoothAdapter;
	//BluetoothServerSocket bluetoothServerSocket;
	public static Stream inputOutputStream;
		
	public static ConnectionState connectionState;
	public static WatchStates watchState;
	
//	public static TestSmsLoop testSmsLoop;
    public static readonly PrivateFontCollection Pfc = new PrivateFontCollection();

        public enum ConnectionState
        {
            DISCONNECTED,
            CONNECTING,
            CONNECTED,
            DISCONNECTING
        }

        public enum WatchBuffers
        {
            IDLE,
            APPLICATION,
            NOTIFICATION
        }

        public enum WatchStates
        {
            OFF = 0,
            IDLE = 1,
            APPLICATION = 2,
            NOTIFICATION = 3,
            CALL = 3
        }

        public static class WatchModes {
		public static bool IDLE = false;
		public static bool APPLICATION = false;
		public static bool NOTIFICATION = false;
		public static bool CALL = false;
	}
	
	public static class Preferences {
		public static bool startOnBoot = false;
		public static bool notifyCall = true;
		public static bool notifySMS = true;
		public static bool notifyGmail = true;
		public static bool notifyK9 = true;
		public static bool notifyAlarm = true;
		public static bool notifyMusic = true;
		public static String watchMacAddress = "";
		public static int packetWait = 10;
		public static bool skipSDP = false;
		public static bool invertLCD = false;
		public static String weatherCity = "Dallas,US";
		public static bool weatherCelsius = false;
        public static NotificationBuilder.FontSize fontSize = NotificationBuilder.FontSize.MEDIUM;
		public static int smsLoopInterval = 15;
		public static bool idleMusicControls = false;
		public static bool idleReplay = false;
	}
		
	public static void loadPreferences() 
    {
//		SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context);


	    Preferences.startOnBoot = false; // sharedPreferences.getbool("StartOnBoot", Preferences.startOnBoot);
	    Preferences.notifyCall = false; // sharedPreferences.getbool("NotifyCall", Preferences.notifyCall);
		Preferences.notifySMS = false; // sharedPreferences.getbool("NotifySMS", Preferences.notifySMS);
		Preferences.notifyGmail = false; // sharedPreferences.getbool("NotifyGmail", Preferences.notifyGmail);
		Preferences.notifyK9 = false; // sharedPreferences.getbool("NotifyK9", Preferences.notifyK9);
		Preferences.notifyAlarm = false; // sharedPreferences.getbool("NotifyAlarm", Preferences.notifyAlarm);
		Preferences.notifyMusic = false; // sharedPreferences.getbool("NotifyMusic", Preferences.notifyMusic);
	    Preferences.watchMacAddress = "d0:37:61:c4:cb:25";// sharedPreferences.getString("MAC", Preferences.watchMacAddress);		
	    Preferences.skipSDP = true; // sharedPreferences.getbool("SkipSDP", Preferences.skipSDP);
	    Preferences.invertLCD = false; // sharedPreferences.getbool("InvertLCD", Preferences.invertLCD);
	    Preferences.weatherCity = "Moscow";// sharedPreferences.getString("WeatherCity", Preferences.weatherCity);
	    Preferences.weatherCelsius = true; // sharedPreferences.getbool("WeatherCelsius", Preferences.weatherCelsius);
	    Preferences.idleMusicControls = true; // sharedPreferences.getbool("IdleMusicControls", Preferences.idleMusicControls);
	    Preferences.idleReplay = true;// sharedPreferences.getbool("IdleReplay", Preferences.idleReplay);
		
		try
		{
		    Preferences.fontSize = NotificationBuilder.FontSize.MEDIUM;// Integer.valueOf(sharedPreferences.getString("FontSize", Integer.toString(Preferences.fontSize)));
		    Preferences.packetWait = 1;// Integer.valueOf(sharedPreferences.getString("PacketWait", Integer.toString(Preferences.packetWait)));
		    Preferences.smsLoopInterval = 1;// Integer.valueOf(sharedPreferences.getString("SmsLoopInterval", Integer.toString(Preferences.smsLoopInterval)));
		} catch (Exception e) {			
		}
		
	}

        public void onCreate()
        {

            loadPreferences();

            LoadFont("metawatch_8pt_5pxl_CAPS.ttf");
            LoadFont("metawatch_8pt_7pxl_CAPS.ttf");
            LoadFont("metawatch_16pt_11pxl.ttf");

		connectionState = ConnectionState.CONNECTING;
		watchState = WatchStates.OFF;

            bluetoothAdapter = new BluetoothClient();
				

		
		start();
		
	}

        void LoadFont(string fontName)
        {
            Pfc.AddFontFile(Path.Combine("Assets", fontName));
           
        }

	public void onDestroy()
	{
	    disconnectExit();

	}


        void connect() {
		
		try {
			

						
			
			//Log.d(MetaWatch.TAG, "got Bluetooth socket");
			//if (bluetoothSocket == null)
				//Log.d(MetaWatch.TAG, "Bluetooth socket is null");
            var addr = BluetoothAddress.Parse("d0:37:61:c4:cb:25");


            var serviceClass = BluetoothService.SerialPort;
            var ep = new BluetoothEndPoint(addr, serviceClass);
            bluetoothAdapter.Connect(ep);

            inputOutputStream = bluetoothAdapter.GetStream();

			connectionState = ConnectionState.CONNECTED;

            BtProtocol.SendRtcNow();
            BtProtocol.GetDeviceType();			
			
		} 
        catch (IOException ioexception) 
        {
//			Log.d(MetaWatch.TAG, ioexception.toString());
//			sendToast(ioexception.toString());
		} /*catch (SecurityException e) {
			Log.d(MetaWatch.TAG, e.toString());
		} catch (NoSuchMethodException e) {
			Log.d(MetaWatch.TAG, e.toString());
		} catch (IllegalArgumentException e) {
			Log.d(MetaWatch.TAG, e.toString());
		} catch (IllegalAccessException e) {
			Log.d(MetaWatch.TAG, e.toString());
		} catch (InvocationTargetException e) {
			Log.d(MetaWatch.TAG, e.toString());
		}*/

		
	}
//	
//	public void sendToast(String text) {
//		var m = new BtMessage.Message();
//		m.what = 1;
//		m.obj = text;
//		messageHandler.sendMessage(m);
//	}
//
//	private Handler messageHandler = new Handler() {
//		public void handleMessage(Message msg) {
//			switch (msg.what) {
//			case 1:
//				Toast.makeText(context, (CharSequence) msg.obj, Toast.LENGTH_SHORT).show();
//				break;
//			}
//		}
//
//	};
	
	void disconnect()
	{
	    try
	    {
            bluetoothAdapter.Dispose();
	    }
	    catch (IOException e)
	    {
	    }
	    
	}

    void disconnectExit()
	{
	    connectionState = ConnectionState.DISCONNECTING;
	    disconnect();
	}

        public static void nap(TimeSpan time) {
		try {
			Thread.Sleep(time);
		} catch (Exception e) {
		}
	}
	
	void start()
	{
	    var thread = new Thread(() =>
	                                {

	                                    bool run = true;


	                                    while (run)
	                                    {
	                                        switch (connectionState)
	                                        {
	                                            case ConnectionState.DISCONNECTED:
	                                                //	                                                   Log.d(MetaWatch.TAG, "state: disconnected");
	                                                break;
	                                            case ConnectionState.CONNECTING:
	                                                //	                                                   Log.d(MetaWatch.TAG, "state: connecting");
	                                                // create initial connection or reconnect
	                                                connect();
	                                                nap(new TimeSpan(0, 0, 2));
	                                                break;
	                                            case ConnectionState.CONNECTED:
	                                                //	                                                   Log.d(MetaWatch.TAG, "state: connected");
	                                                // read from input stream
	                                                readFromDevice();
	                                                break;
	                                            case ConnectionState.DISCONNECTING:
	                                                //	                                                   Log.d(MetaWatch.TAG, "state: disconnecting");
	                                                // exit
	                                                run = false;
	                                                break;
	                                        }
	                                    }
	                                });
	
		thread.Start();
	}
	
	void readFromDevice() {
		
		try
		{
		    var bytes = new byte[256];
		    //Log.d(MetaWatch.TAG, "before blocking read");
		    inputOutputStream.Read(bytes, 0, 256);
//			wakeLock.acquire(5000);

		    // print received
		    var str = "received: ";
		    int len = (bytes[1] & 0xFF);
//			Log.d(MetaWatch.TAG, "packet length: " + len);

/*
			for (int i = 0; i < len; i ++) {
				//str+= Byte.toString(bytes[i]) + ", ";
				str+= "0x" + ((bytes[i] & 0xff) + 0x100, 16).substring(1) + ", ";
			}
			Log.d(MetaWatch.TAG, str);
*/

		    switch (bytes[2])
		    {
		        case 0x02:
//					Log.d(MetaWatch.TAG, "received: device type response");
		            break;
		        case 0x31:
//					Log.d(MetaWatch.TAG, "received: nval response");
		            break;
		        case 0x33:
//					Log.d(MetaWatch.TAG, "received: status change event");
		            break;
		    }
		    /*
			if (bytes[2] == 0x31) { // nval response
				if (bytes[3] == 0x00) // success
					if (bytes[4] == 0x00) // set to 12 hour format
					Protocol.setNvalTime(true);
			}
			*/

		    if (bytes[2] == 0x33)
		    {
		        // status change event
		        if (bytes[4] == 0x11)
		        {
//					Log.d(MetaWatch.TAG, "notify scroll request");

//					synchronized (Notification.scrollRequest) 
//						Notification.scrollRequest.notify();
		        }
		    }

		    if (bytes[4] == 0x10)
		    {
//					Log.d(MetaWatch.TAG, "scroll completed");										
		    }
		

	    if (bytes[2] == 0x34) { // button press
//				Log.d(MetaWatch.TAG, "button event");
				pressedButton(bytes[3]);
			}
			
			if (bytes[2] == 0x02) { // device type
				{
					
					
					if (Preferences.idleMusicControls)
						BtProtocol.enableMediaButtons();
					//else 
						//Protocol.disableMediaButtons();
					
					if (Preferences.idleReplay)
                        BtProtocol.enableReplayButton();
					//else
						//Protocol.disableReplayButton();

                    BtProtocol.configureMode();
					////Idle.toIdle(this);
					////Idle.updateLcdIdle(this);

                    BtProtocol.queryNvalTime();
					
				}
			}
			
		} catch (IOException e) 
        {
			if (connectionState != ConnectionState.DISCONNECTING)
				connectionState = ConnectionState.CONNECTING;			
		}
		
	}
	
	void pressedButton(byte button) {
//		Log.d(MetaWatch.TAG, "button code: " + Byte.toString(button));
		
		switch (watchState) {
			case WatchStates.IDLE: {
				
				/*
				if (Idle.isIdleButtonOverriden(button)) {
						Log.d(MetaWatch.TAG, "this button is overriden");
						broadcastButton(button, watchState);
				}
				*/				
			}
				break;
			case WatchStates.APPLICATION:
//				broadcastButton(button, watchState);
				break;
			case WatchStates.NOTIFICATION:				
				break;
		}
		
	}


	


    }
}
