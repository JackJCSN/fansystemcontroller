using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using com.JackJCSN.DataAPI;
using FANController.Properties;

namespace FANController
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        //帧开始码
        private const byte OpenByte = 0xFE;
        //帧结束码
        private const byte StopByte = 0xFF;
        //时间同步码
        private const byte TimeFrame = 0x01;
        //普通同步码
        private const byte NomalFrame = 0x02;

        IMemoryInfo memory = null;
        IGetCPUInfo cpuinfo = null;
        SerialPort com = null;
        bool sendfull = true;
        private static Loger loger;

        private ArrayList buffer =null;

        public MainWindow()
        {
            InitializeComponent();
            if (loger == null)
            {
                loger = new Loger("MainWindow");
            }
            try
            {
                buffer = ArrayList.Synchronized(new ArrayList(new Byte[0]));
                cpuinfo = new GetCPUInfo();
                cpuinfo.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PropertyChanged);
                memory = new MemoryInfo();
                com = new SerialPort();
                com.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(com_DataReceived);
                connectBtn.DataContext = com;
                hardwareStat.DataContext = com;
                cupGroup.DataContext = cpuinfo;
                memoryGroup.DataContext = memory;
                hardWareGroup.DataContext = Settings.Default;
                ComportBox.ItemsSource = SerialPort.GetAvailables();
                RateBox.ItemsSource = SerialPort.GetBaudRates();
                comLostFocus(this, null);
                Binding b = new Binding("IsConnected");
                b.Source = com;
                b.Converter = this.FindResource("BooleanNotConverter1") as BooleanNotConverter;
                b.Mode = BindingMode.OneWay;
                ComportBox.SetBinding(UIElement.IsEnabledProperty, b);
                UseDefaultRate.SetBinding(UIElement.IsEnabledProperty, b);
            }
            catch (Exception ex)
            {
                try
                {
                    loger.ErrorMSG(ex);
                    MessageBoxResult r = MessageBox.Show(
                        String.Format(R.ErrorMsg_Format, RE.ProcessNotFind, R.StopApplication), R.Error,
                         MessageBoxButton.YesNo, MessageBoxImage.Stop);
                    switch (r)
                    {
                        case MessageBoxResult.Yes:
                            this.Close();
                            break;
                        default:
                            break;
                    }
                }
                catch(Exception ex2)
                {
                    loger.ErrorMSG(ex2);
                }
            }
        }

        private void com_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                byte be = (byte)com.ReadByte();
                switch (be)
                {
                    case OpenByte:
                        #region CS1
                        buffer.Clear();
                        buffer.Add(OpenByte);
                        #endregion
                        break;
                    case StopByte:
                        #region CS2
                        byte bo = 0;
                        if (buffer.Count == 2)
                        {
                            bo = (byte)buffer[0];
                            byte bc = (byte)buffer[1];
                            if (bo != OpenByte || bc != TimeFrame)
                            {
                                throw new DecoderFallbackException(RE.DataFormatError);
                            }
                            sendfull = true;
                            buffer.Clear();
                        }
                        else
                        {
                            throw new DecoderFallbackException(RE.DataFormatError);
                        }
                        #endregion
                        break;
                    default:
                        #region CS3
                        if (buffer.Count == 0)
                        {
                            throw new DecoderFallbackException(RE.DataFormatError);
                        }
                        buffer.Add(be);
                        #endregion
                        break;
                }
            }
            catch (Exception ex)
            {
                buffer.Clear();
                loger.WarringMSG(ex);
            }
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CPUTemplater":
                    MemoryUpdate();
                    SendTempMsg();
                    break;
                default:
                    break;
            }
        }

        private void MemoryUpdate()
        {
            memory.Update();
        }

        private void SendTempMsg()
        {
            IList<byte> sendbuffer = new List<byte>();
            sendbuffer.Add(OpenByte);
            if (sendfull)
            {
                sendbuffer.Add(TimeFrame);
                DateTime n = DateTime.Now;
                if (n.Millisecond > 500)
                {
                    n.AddSeconds(1);
                }
                sendbuffer.Add((byte)(n.Year - 2000));
                sendbuffer.Add((byte)n.Month);
                sendbuffer.Add((byte)n.Day);
                sendbuffer.Add((byte)n.Hour);
                sendbuffer.Add((byte)n.Minute);
                sendbuffer.Add((byte)(n.Second));
                //StartFrom Sunday:0 End On Saturday:6
                //OR Can (n.DayOfWeek + 6)%7 + 1
                sendbuffer.Add((byte)(n.DayOfWeek));
                sendfull = false;
            }
            else
            {
                sendbuffer.Add(NomalFrame);
                sendbuffer.Add((byte)cpuinfo.CPUTemplater);
                sendbuffer.Add((byte)cpuinfo.CPULoad);
                //Memory Usage Now Set 200:0xC8
                //2013-13-15 Use GlobMemoryStatusEx to Do Test
                sendbuffer.Add((byte)memory.MemoryLoad);
            }
            //Check SUM Now Include Header 0xFE
            sendbuffer.Add(CheckSum(sendbuffer));
            sendbuffer.Add(StopByte);
            if (com.IsConnected)
            {
                try
                {
                    com.Write(sendbuffer.ToArray());
                }
                catch(Exception ex)
                {
                    loger.WarringMSG(ex);
                    try
                    {
                        StatError.Foreground = Brushes.Red;
                        StatError.Content = String.Format("{0}: {1}", R.Error, R.InnerError).Replace("\r\n", " ");
                    }
                    catch (Exception ex2)
                    {
                        loger.WarringMSG(ex2);
                    }
                    try
                    {
                        com.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static byte CheckSum(IList<byte> list)
        {
            uint cksum=0;
            foreach (byte a in list)
            {
                cksum += a;
            }

            //将32位转换为8位
            while ((cksum >> 8) > 0)
            {
                cksum = (cksum >> 8) + (cksum & 0xff);
            }
            return (byte)(~cksum);
        }

        private void ComportBoxDropDownOpened(object sender, EventArgs e)
        {
            ComportBox.ItemsSource = SerialPort.GetAvailables();
        }

        private void RateBoxDropDownOpened(object sender, EventArgs e)
        {
            RateBox.ItemsSource = SerialPort.GetBaudRates();
        }

        #region IDisposable 成员

        public void Dispose()
        {
            if (cpuinfo != null)
            {
                cpuinfo.Dispose();
            }
            if (com != null)
            {
                com.Dispose();
            }
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            this.Dispose();
            base.OnClosed(e);
        }

        private void connctBtn_Click(object sender, RoutedEventArgs e)
        {
            if (com.IsConnected)
            {
                try
                {
                    com.Close();
                }
                catch (IOException ex)
                {
                    StatError.Foreground = Brushes.Red;
                    StatError.Content = String.Format("{0}: {1}", R.Error, ex.Message).Replace("\r\n", " ");
                }
                catch (Exception ex)
                {
                    loger.WarringMSG(ex);
                    StatError.Foreground = Brushes.Red;
                    StatError.Content = String.Format("{0}: {1}", R.Error, R.InnerError).Replace("\r\n", " ");
                }
            }
            else
            {
                StatError.Foreground = Brushes.Black;
                StatError.Content = "正在连接……";
                try
                {
                    Int32 rate = 0;
                    if (!Int32.TryParse(RateBox.Text, out rate))
                    {
                        rate = 9600;
                    }
                    com.Initialize(ComportBox.Text, rate);
                    com.Open();
                    StatError.Content = "";
                    Settings.Default.Save();
                    Settings.Default.Reload();
                    StatError.Content = "";
                }
                catch (IOException ex)
                {
                    StatError.Foreground = Brushes.Red;
                    StatError.Content = String.Format("{0}: {1}", R.Error, ex.Message).Replace("\r\n", " ");
                }
                catch (Exception ex)
                {
                    loger.WarringMSG(ex);
                    StatError.Foreground = Brushes.Red;
                    StatError.Content = String.Format("{0}: {1}", R.Error, R.InnerError).Replace("\r\n", " ");
                }
            }
        }

        private void comLostFocus(object sender, RoutedEventArgs e)
        {
            String Tip =  "使用端口{0}@{1}Hz进行通信";
            lblTip.Content = String.Format(Tip, ComportBox.Text, RateBox.Text);
        }

        private void OnNotifyIconDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Visibility = Visibility.Hidden;
                setItemEable(true);
            }
            else
            {
                Visibility = Visibility.Visible;
                setItemEable(false);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Visibility = Visibility.Hidden;
                setItemEable(true);
            }
            else
            {
                Visibility = Visibility.Visible;
                setItemEable(false);
            }
        }

        private void setItemEable(bool IsEnable)
        {
            ShowItem.IsEnabled = IsEnable;
            HiddenItem.IsEnabled = !IsEnable;
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            notifyIcon.Visibility = Visibility.Hidden;
        }
    }
}
