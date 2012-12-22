using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;

namespace Bluetooth
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Thread _readThread;
        private BluetoothClient _bluetoothClient;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button1Click(object sender, RoutedEventArgs e)
        {


            var addr = BluetoothAddress.Parse("d0:37:61:c4:cb:25");


            var serviceClass = BluetoothService.SerialPort;
            var ep = new BluetoothEndPoint(addr, serviceClass);
            _bluetoothClient = new BluetoothClient();

            _bluetoothClient.Connect(ep);
            //            Stream peerStream = cli.GetStream();

            BtProtocol.OutStream = _bluetoothClient.GetStream();
            MetaWatchService.inputOutputStream = BtProtocol.OutStream;
            BtProtocol.SendRtcNow();
            BtProtocol.GetDeviceType();
            BtProtocol.OutStream.Flush();


            _readThread = new Thread(() =>
            {
                while (true)
                {
                    BtProtocol.OutStream.ReadTimeout = 2000000;
                    var startByte = BtProtocol.OutStream.ReadByte();
                    if (startByte != 1)
                        continue;
                    var msgLen = BtProtocol.OutStream.ReadByte();
                    if (msgLen < 6)
                    {
                        MessageBox.Show("Ошибка приёма");
                        continue;
                    }
                    var msgType = BtProtocol.OutStream.ReadByte();
                    var msgOptions = BtProtocol.OutStream.ReadByte();
                    var msgDataLen = msgLen - 6;

                    var data = new byte[msgDataLen];
                    BtProtocol.OutStream.Read(data, 0, msgDataLen);

                    //CRC
                    BtProtocol.OutStream.ReadByte();
                    BtProtocol.OutStream.ReadByte();

                    switch (msgType)
                    {

                        case (int)BtMessage.Message.GetDeviceTypeResponse:
                            switch (data[0])
                            {
                                case 2:
                                    Dispatcher.Invoke(new Action(delegate
                                    {

                                        sbiConnect.Content = "Подключено!";

                                    }), System.Windows.Threading.DispatcherPriority.Normal);

                                    //MessageBox.Show("Цифровые!");
                                    break;
                            }
                            break;
                        case (int)BtMessage.Message.ButtonEventMsg:
                            if ((data[0] & 0x1) == 0x1)
                                MessageBox.Show("A!");
                            if ((data[0] & 0x2) == 0x2)
                                MessageBox.Show("B!");
                            if ((data[0] & 0x4) == 0x4)
                                MessageBox.Show("C!");
                            if ((data[0] & 0x8) == 0x8)
                                MessageBox.Show("D!");

                            if ((data[0] & 0x20) == 0x20)
                                MessageBox.Show("E!");
                            if ((data[0] & 0x40) == 0x40)
                                MessageBox.Show("F!");
                            if ((data[0] & 0x80) == 0x80)
                                MessageBox.Show("S!");
                            break;
                        case (int)BtMessage.Message.ReadBatteryVoltageResponse:
                            var powerGood = data[0];
                            var batteryCharging = data[1];
                            var voltage = (data[3] << 8) | data[2];
                            var voltageAverage = (data[5] << 8) | data[4];
                            MessageBox.Show(string.Format("volt:{0} avg:{1} powerGood:{2} batteryCharging: {3}",
                                                          voltage / 1000f, voltageAverage / 1000f, powerGood, batteryCharging));
                            break;
                    }


                }
            });
            _readThread.Start();
            //            var buf = new byte[2];
            //            var readLen = peerStream.Read(buf, 0, buf.Length);
            //            if (readLen == 2)
            //            {
            //                MessageBox.Show(buf[1].ToString());
            //            }


            //    peerStream.Write/Read ...
            //
            //e.g.
            /*var buf = new byte[1000];
            var readLen = peerStream.Read(buf, 0, buf.Length);
            if (readLen == 0)
            {
                Console.WriteLine("Connection is closed");
            }
            else

            {

                Console.WriteLine("Recevied {0} bytes", readLen);

            }*/

        }

        private static class BtMessage
        {
            public enum Message
            {
                InvalidMessage = 0x00,

                GetDeviceType = 0x01,
                GetDeviceTypeResponse = 0x02,
                GetInfoString = 0x03,
                GetInfoStringResponse = 0x04,
                DiagnosticLoopback = 0x05,
                EnterShippingModeMsg = 0x06,
                SoftwareResetMsg = 0x07,
                ConnectionTimeoutMsg = 0x08,
                TurnRadioOnMsg = 0x09,
                TurnRadioOffMsg = 0x0a,
                SppReserved = 0x0b,
                PariringControlMsg = 0x0c,
                EnterSniffModeMsg = 0x0d,
                xxReEnterSniffModeMsg = 0x0e,
                LinkAlarmMsg = 0x0f,

                /*
       * OLED display related commands
       */
                OledWriteBufferMsg = 0x10,
                OledConfigureModeMsg = 0x11,
                OledChangeModeMsg = 0x12,
                OledWriteScrollBufferMsg = 0x13,
                OledScrollMsg = 0x14,
                OledShowIdleBufferMsg = 0x15,
                OledCrownMenuMsg = 0x16,
                OledCrownMenuButtonMsg = 0x17,

                /* 
       * Status and control
       */

                /* move the hands hours, mins and seconds */
                AdvanceWatchHandsMsg = 0x20,

                /* config and (dis)enable vibrate */
                SetVibrateMode = 0x23,

                /* Sets the RTC */
                SetRealTimeClock = 0x26,
                GetRealTimeClock = 0x27,
                GetRealTimeClockResponse = 0x28,

                /* osal nv */
                NvalOperationMsg = 0x30,
                NvalOperationResponseMsg = 0x31,

                /* status of the current display operation */
                StatusChangeEvent = 0x33,

                ButtonEventMsg = 0x34,

                GeneralPurposePhoneMsg = 0x35,
                GeneralPurposeWatchMsg = 0x36,
                /*
       * LCD display related commands
       */
                WriteBuffer = 0x40,
                ConfigureMode = 0x41,
                ConfigureIdleBufferSize = 0x42,
                UpdateDisplay = 0x43,
                LoadTemplate = 0x44,
                UpdateMyDisplaySram = 0x45,
                EnableButtonMsg = 0x46,
                DisableButtonMsg = 0x47,
                ReadButtonConfigMsg = 0x48,
                ReadButtonConfigResponse = 0x49,
                UpdateMyDisplayLcd = 0x4a,

                /* */
                BatteryChargeControl = 0x52,
                BatteryConfigMsg = 0x53,
                LowBatteryWarningMsgHost = 0x54,
                LowBatteryBtOffMsgHost = 0x55,
                ReadBatteryVoltageMsg = 0x56,
                ReadBatteryVoltageResponse = 0x57,
                ReadLightSensorMsg = 0x58,
                ReadLightSensorResponse = 0x59,
                LowBatteryWarningMsg = 0x5a,
                LowBatteryBtOffMsg = 0x5b,

                /*****************************************************************************
       *
       * User Reserved 0x60-0x70-0x80-0x90
       *
       ****************************************************************************/


                /*****************************************************************************
       *
       * Watch/Internal Use Only
       *
       ****************************************************************************/
                IdleUpdate = 0xa0,
                xxxInitialIdleUpdate = 0xa1,
                WatchDrawnScreenTimeout = 0xa2,
                ClearLcdSpecial = 0xa3,
                WriteLcd = 0xa4,
                ClearLcd = 0xa5,
                ChangeModeMsg = 0xa6,
                ModeTimeoutMsg = 0xa7,
                WatchStatusMsg = 0xa8,
                MenuModeMsg = 0xa9,
                BarCode = 0xaa,
                ListPairedDevicesMsg = 0xab,
                ConnectionStateChangeMsg = 0xac,
                ModifyTimeMsg = 0xad,
                MenuButtonMsg = 0xae,
                ToggleSecondsMsg = 0xaf,
                SplashTimeoutMsg = 0xb0,


                // My
                CalendarMsg = 0xba,
                ShowDiaryMsg = 0xbb,

                // Diary
                DiaryIsEmptyRecord = 0xbc,
                DiaryWriteRecord = 0xbd,
                DiaryWriteEnd = 0xbe,

                LedChange = 0xc0,

                QueryMemoryMsg = 0xd0,

                AccelerometerSteps = 0xea,
                AccelerometerRawData = 0xeb
            }

            public static byte Start = 0x01;

        }




        public static void GetDeviceType(Stream outStream)
        {

            var bytes = new byte[4];

            bytes[0] = BtMessage.Start;
            bytes[1] = (byte)(bytes.Length + 2); // length
            bytes[2] = (byte)BtMessage.Message.GetDeviceType;
            bytes[3] = 0;

            outStream.Write(bytes, 0, bytes.Length);
        }

        private void Button2Click(object sender, RoutedEventArgs e)
        {
            BtProtocol.ReadBatteryVoltage();
            BtProtocol.OutStream.Flush();

        }

        private void Button3Click(object sender, RoutedEventArgs e)
        {
            BtProtocol.EnableAccelerometer();
            BtProtocol.OutStream.Flush();
        }

        private void Button4Click(object sender, RoutedEventArgs e)
        {
            BtProtocol.DisableAccelerometer();
            BtProtocol.OutStream.Flush();

        }

        private void Button5Click(object sender, RoutedEventArgs e)
        {
            BtProtocol.AccelerometerAccess();
            BtProtocol.OutStream.Flush();

        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (_readThread != null && _readThread.IsAlive)
            {
                _readThread.Abort();
            }
        }

        private Diary _diary;
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _diary = Diary.LoadFromFile("Diary.xml");
            dataGrid1.ItemsSource = _diary.Rec;
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            _diary.SaveToFile("Diary.xml");
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            BtProtocol.UploadDiary(_diary);
        }

        private static long MkFullTime(int year, int month, int day, int hour, int min)
        {
            // до 2047 года будет работать, потом надо что-нить придумать
            return ((long)year << 21) | ((long)month << 17) | ((long)day << 12) | ((long)hour << 6) | min;
        }


        int[] eventIndexes = new int[3]; // 3 индекса событий, которые отображаются на экране
        int[] diaryEventStatus = new int[4]; // 0 - текущий, Остальные для 3х событий на экране
        int[] diaryEventYear = new int[4];
        int[] diaryEventMonth = new int[4];
        int[] diaryEventDay = new int[4];
        DayOfWeek[] diaryEventDow = new DayOfWeek[4];
        int[] diaryEventHour = new int[4];
        int[] diaryEventMin = new int[4];
        string[] diaryEventText = new string[4];
        // RTCYEAR RTCMON  RTCDOW RTCDAY
        // Если событие в эту дату, возвращается 1
        // hour, min - событие должно быть после этогов ремени
        int LoadDiaryEvent(int eventIndex, int year,
                                  int month, DayOfWeek weekday,
                                  int day,
                                  int hour, int min)
        {
            var curFullTime = MkFullTime(year, month, day, hour, min);
            // NUMBER_OF_DIARY_RECORDS записей по 32 символа
            // 0 байт - статус (3)  0 - пустая ячейка 1 - одноразово 2 - каждый год
            //                      3 - День рождения
            //                      4 - каждый месяц 5 - каждую неделю
            //                      6 - каждый день
            //   вид напоминания (2) 0 - без напоминания
            //                      1 - вибро
            //   начало года     (3)
            // 1,2 - дата - [год(4бита от 1900 года), мес(4 бита)], день[(5бит) день недели(3 бита)]
            // 3,4 - время
            diaryEventStatus[0] = (int)_diary.Rec[eventIndex].Status;
            if (diaryEventStatus[0] == 0)
            {
                return 0;
            }

            var tempYear = _diary.Rec[eventIndex].DateTime.Year;
            diaryEventYear[0] = tempYear;
            diaryEventMonth[0] = _diary.Rec[eventIndex].DateTime.Month;
            diaryEventDay[0] = _diary.Rec[eventIndex].DateTime.Day;
            diaryEventDow[0] = _diary.Rec[eventIndex].DateTime.DayOfWeek;
            diaryEventHour[0] = _diary.Rec[eventIndex].DateTime.Hour;
            diaryEventMin[0] = _diary.Rec[eventIndex].DateTime.Minute;
            diaryEventText[0] = _diary.Rec[eventIndex].Value;

            var eventFullTime = MkFullTime(
                diaryEventYear[0], 
                diaryEventMonth[0], 
                diaryEventDay[0],
                diaryEventHour[0],
                diaryEventMin[0]);


            // Если повторы, то год текущий ставим
            if (diaryEventStatus[0] >= 2 && diaryEventStatus[0] <= 6)
            {
                // Если год события меньше текущего, то выравниваем
                // То есть событие началось раньше
                if (eventFullTime <= curFullTime)
                    diaryEventYear[0] = year;
            }
            // Если повторы чаще, чем раз в год, то месяц текущий
            if (diaryEventStatus[0] >= 4 && diaryEventStatus[0] <= 6)
            {
                // Если месяц события меньше текущего и год этот же, то месяц выравниваем
                if (eventFullTime <= curFullTime)
                    diaryEventMonth[0] = month;
            }
            // Если повторы каждую неделю и день недели совпадает, то день текущий
            if (diaryEventStatus[0] == 5)
            {
                // день недели совпадает и месяц и год равны текущим и дата меньше сегодняшней
                if (diaryEventDow[0] == weekday
                    && (eventFullTime <= curFullTime))
                    diaryEventDay[0] = day;
            }
            // Если повторы каждый день, то день текущий
            // Если год и месяц сегодняшние и дата раньше
            if (diaryEventStatus[0] == 6)
            {
                if (eventFullTime <= curFullTime)
                    diaryEventDay[0] = day;
            }
            if (diaryEventYear[0] == year && diaryEventMonth[0] == month
               && diaryEventDay[0] == day)// && diaryEventHour[0]
            {
                if (diaryEventHour[0] > hour || (diaryEventHour[0] == hour && diaryEventMin[0] >= min))
                {
//                    if (diaryEventStatus[0] == 3) // Если тип - день рождения
//                    { // То год ставим рождения
//                        diaryEventYear[0] = tempYear;
//                    }
                    return 1;
                }
                else
                    return 0;
            }
            else return 0;
        }

        void Sort()
        {
            var curDate = DateTime.Now;
            var curYear = curDate.Year;
            var curMonth = curDate.Month;

            var startDay = curDate.Day;
            var startHour = curDate.Hour;
            var startMin = curDate.Minute;
            
              // Очистка
              for(var i = 0; i < 3; i++)
              {
                diaryEventYear[i + 1] = 0;
                eventIndexes[i] = 0;
              }

            // Сортируем события

            for (var month = 0; month <= 3; month++)
            {
                // Сколько дней в месяце
                var days = DateTime.DaysInMonth(curYear, curMonth);
                for (var day = startDay; day <= days; day++)
                {
                    // Получаем день недели
                    var curDOW = (new DateTime(curYear, curMonth, day)).DayOfWeek;
                    for (var i = 0; i < _diary.Rec.Count; i++)
                    {
                        if (LoadDiaryEvent(i, curYear, curMonth, curDOW, day, startHour, startMin) == 1)
                        {
                            for (var j = 0; j < 3; j++)
                            {
                                // Если ячейка не занята, занять!
                                if (diaryEventYear[j + 1] == 0)
                                {
                                    diaryEventYear[j + 1] = diaryEventYear[0];
                                    diaryEventMonth[j + 1] = diaryEventMonth[0];
                                    diaryEventDay[j + 1] = diaryEventDay[0];
                                    eventIndexes[j] = i;
                                    break;
                                }
                                else
                                {
                                    // Если событие раньше, чем записанное, занять ячейку и переместить записанное событие дальше
                                    if (diaryEventYear[0] < diaryEventYear[j + 1]
                                       || (diaryEventYear[0] == diaryEventYear[j + 1] && diaryEventMonth[0] < diaryEventMonth[j + 1])
                                       || (diaryEventYear[0] == diaryEventYear[j + 1] && diaryEventMonth[0] == diaryEventMonth[j + 1] && diaryEventDay[0] < diaryEventDay[j + 1]))
                                    {
                                        if (j < 2) // сдвигаем остальные ячейки вправо
                                        {
                                            for (var k = 2; k > j; k--)
                                            {
                                                diaryEventYear[k + 1] = diaryEventYear[k + 1 - 1];
                                                diaryEventMonth[k + 1] = diaryEventMonth[k + 1 - 1];
                                                diaryEventDay[k + 1] = diaryEventDay[k + 1 - 1];
                                                eventIndexes[k] = eventIndexes[k - 1];
                                            }
                                        }
                                        // Записываем событие в освободившуюся ячейку
                                        diaryEventYear[j + 1] = diaryEventYear[0];
                                        diaryEventMonth[j + 1] = diaryEventMonth[0];
                                        diaryEventDay[j + 1] = diaryEventDay[0];
                                        eventIndexes[j] = i;
                                    }
                                }
                            }
                        }

                    }
                    // Если в третьей ячейке записано год больше 0, то заканчиваем
                    if (diaryEventYear[3] > 0)
                        return;

                    // После текущего дня ищем события, которые могут быть в любое время
                    startHour = 0;
                    startMin = 0;
                } // day
                if (curMonth == 12)
                {
                    curYear++;
                    curMonth = 1;
                }
                else
                {
                    curMonth++;
                }
                startDay = 1;
            } // month

        }

        private void ButtonClick1(object sender, RoutedEventArgs e)
        {
            Sort();

            // Вывод
            lbQueue.Items.Clear();
            for (var i = 0; i < 3; i++)
            {
                if (diaryEventYear[i + 1] > 0)
                {
                    var str = string.Format("{0}.{1}.{2} {3}",
                                            diaryEventDay[i + 1],
                                            diaryEventMonth[i + 1],
                                            diaryEventYear[i + 1],
                                            _diary.Rec[eventIndexes[i]].Value);

                    lbQueue.Items.Add(str);
                }
                
            }
        }

    }


}
