using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bluetooth
{
    /*class TestSmsLoop : Thread
    {
        bool runLoop;

        public void run()
        {
            runLoop = true;
            for (int i = 1; runLoop; i++)
            {
                NotificationBuilder.createSMS(context, "123-456-789", "\n  Test SMS #" + i);
                try
                {
                    Thread.sleep(MetaWatchService.Preferences.smsLoopInterval * 1000);
                }
                catch (InterruptedException e)
                {
                }
            }
        }

        public void stop()
        {
            runLoop = false;
        }

    }*/
}
