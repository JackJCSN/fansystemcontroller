using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using p = System.IO.Ports;

namespace com.JackJCSN.DataAPI
{
    /// <summary>
    /// 串口抽象
    /// </summary>
    public sealed class SerialPort : INotifyPropertyChanged, IDisposable
    {
        #region Static Functions
        /// <summary>
        /// 获取可用的串行端口列表
        /// </summary>
        /// <returns>包含可用的串行端口的列表接口</returns>
        public static IList<String> GetAvailables()
        {
            string[] ports = p.SerialPort.GetPortNames();
            return ports.ToList();
        }

        static SerialPort()
        {
#if DEBUG
            SetTracerLevel(TraceLevel.INFO);
#else
#if TRACE
            SetTracerLevel(TraceLevel.INFO);
#else
            SetTracerLevel(TraceLevel.WARRING);
#endif
#endif
        }

        private static void SetTracerLevel(TraceLevel t = TraceLevel.WARRING)
        {
            loger.TraceLevel = t;
        }

        /// <summary>
        /// 获取波特率列表
        /// </summary>
        /// <returns>包含波特率的列表接口</returns>
        public static IList<Int32> GetBaudRates()
        {
            int[] B = { 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 };
            return B.ToList();
        }
        #endregion

        private System.IO.Ports.SerialPort COM;

        /// <summary>
        /// 获取基础输入输出流
        /// </summary>
        public Stream BaseStream
        {
            get
            {
                if (COM.IsOpen)
                {
                    return COM.BaseStream;
                }
                else
                {
                    return null;
                }
            }
        }

        #region 状态指示
        public Boolean IsConnected
        {
            get
            {
                try
                {
                    return COM.IsOpen;
                }
                catch (Exception ex)
                {
                    loger.Log(new LogerMsg(ex.Message, TraceLevel.NOTICE));
                }
                return false;
            }
            internal set
            {
                if (COM.IsOpen != value)
                {
                    if (value)
                    {
                        this.Open();
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
        }

        public int AvailableByte
        {
            get;
            private set;
        }
        #endregion

        #region 事件
        /// <summary>
        /// 接收到错误
        /// </summary>
        public event SerialErrorReceivedEventHandler ErrorReceived;

        public event SerialPinChangedEventHandler PinChanged;

        public event SerialDataReceivedEventHandler DataReceived;
        #endregion

        #region 初始化和清理
        public SerialPort()
        {
            try
            {
                Initialize(GetAvailables()[0]);
            }
            catch (Exception ex)
            {
                loger.Log(new LogerMsg(ex.Message, TraceLevel.WARRING));
            }
        }

        ~SerialPort()
        {
            try
            {
                this.Dispose();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 创建串口通信
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="baudRate">波特率[可选，默认:9600]</param>
        /// <param name="parity">奇偶校验[可选，默认:无]</param>
        /// <param name="dataBits">数据位[可选，默认:8bit]</param>
        /// <param name="stopBits">停止位[可选，默认:1bit]</param>
        public SerialPort(String portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            Initialize(portName, baudRate, parity, dataBits, stopBits);
        }

        /// <summary>
        /// 日志记录器
        /// </summary>
        public static Loger loger = new Loger("SerialPort", TraceLevel.WARRING);


        /// <summary>
        /// 打开一个新的串口
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="baudRate">波特率[可选，默认:9600]</param>
        /// <param name="parity">奇偶校验[可选，默认:无]</param>
        /// <param name="dataBits">数据位[可选，默认:8bit]</param>
        /// <param name="stopBits">停止位[可选，默认:1bit]</param>
        public void Initialize(String portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            if (GetAvailables().Contains(portName))
            {
                if (COM != null)
                {
                    COM.Close();
                }
                COM = new System.IO.Ports.SerialPort(portName, baudRate, parity, dataBits, stopBits);

                COM.Encoding = Encoding.Unicode;
                COM.ErrorReceived += new SerialErrorReceivedEventHandler(COM_ErrorReceived);
                COM.PinChanged += new SerialPinChangedEventHandler(COM_PinChanged);
                COM.DataReceived += new SerialDataReceivedEventHandler(COM_DataReceived);
                COM.Disposed += new EventHandler(COM_Disposed);
            }
            else
            {
                throw new ArgumentException(RE.PortNameUnavailable, "portName");
            }
        }
        #endregion

        #region 流程控制

        /// <summary>
        /// 打开串口
        /// </summary>
        public void Open()
        {
            if (!COM.IsOpen)
            {
                COM.Open();
                AvailableByte = 0;
                Changed("AvailableByte");
                Changed("IsConnected");
            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void Close()
        {
            if (COM.IsOpen)
            {
                COM.Close();
                Changed("IsConnected");
            }
        }
        #endregion

        #region 串口事件处理

        void COM_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            System.IO.Ports.SerialPort sp = (System.IO.Ports.SerialPort)sender;
            loger.Log(new LogerMsg(String.Format(R.Notice_Fomat, R.PinChanged, sp.PortName), TraceLevel.NOTICE));
            if (PinChanged != null)
            {
                PinChanged.BeginInvoke(sender, e, null, null);
            }
        }

        void COM_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            System.IO.Ports. SerialPort sp = (System.IO.Ports.SerialPort)sender;
            loger.Log(
                new LogerMsg(
                    String.Format(R.Error_Format,
                    e.EventType.ToString(), sp.PortName), TraceLevel.ERROR));
            if (ErrorReceived != null)
            {
                ErrorReceived.BeginInvoke(sender, e, null, null);
            }
        }

        void COM_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.IO.Ports. SerialPort sp = (System.IO.Ports.SerialPort)sender;
            AvailableByte++;
            Changed("AvailableByte");
            loger.Log(
                new LogerMsg(
                    String.Format(R.Info_Fomat,
                    e.EventType.ToString(), sp.PortName), TraceLevel.INFO));
            if (DataReceived != null)
            {
                DataReceived.BeginInvoke(sender, e, null, null);
            }
        }

        void COM_Disposed(object sender, EventArgs e)
        {
            System.IO.Ports.SerialPort sp = (System.IO.Ports.SerialPort)sender;
            loger.Log(new LogerMsg(String.Format(R.Info_Fomat, R.Disconnect, sp.PortName)));
        }
        #endregion

        #region 写入流

        /// <summary>
        /// 向串口写入一个字节
        /// </summary>
        /// <param name="a">要写入的字节</param>
        public void Write(byte a)
        {
            COM.BaseStream.WriteByte(a);
        }

        /// <summary>
        /// 向串口写入一整组字节
        /// </summary>
        /// <param name="buffer">要写入的字节数组</param>
        public void Write(byte[] buffer)
        {
            foreach (var b in buffer)
            {
                this.Write(b);
                Thread.Sleep(10);
            }
            //COM.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 从字节组的指定位置向串口写入指定长度的一组字节
        /// </summary>
        /// <param name="buffer">要写入的字节组</param>
        /// <param name="offset">起始写入位置</param>
        /// <param name="count">写入长度</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            if (offset < 0 ||
                count < 1 ||
                ((long)offset + (long)count) > buffer.LongLength
                )
            {
                return;
            }
            for (int i = offset; i < count && i < buffer.Length; i++)
            {
                this.Write(buffer[i]);
                Thread.Sleep(10);
            }
            //COM.Write(buffer, offset, count);
        }

        /// <summary>
        /// 向串口写入字符组
        /// </summary>
        /// <param name="buffer">要写入的字符组</param>
        public void Write(char[] buffer)
        {
            COM.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 从字符组的指定位置向串口写入指定长度的一组字符
        /// </summary>
        /// <param name="buffer">要写入的字符组</param>
        /// <param name="offset">起始写入位置</param>
        /// <param name="count">写入长度</param>
        public void Write(char[] buffer, int offset, int count)
        {
            COM.Write(buffer, offset, count);
        }

        /// <summary>
        /// 向串口写入字符串
        /// </summary>
        /// <param name="text">要写入的字符串</param>
        public void Write(String text)
        {
            COM.Write(text);
        }

        /// <summary>
        /// 向串口写入预先格式化的字符串
        /// </summary>
        /// <param name="format">字符格式串</param>
        /// <param name="arg">格式串参数</param>
        public void Write(String format, params object[] arg)
        {
            COM.Write(String.Format(format, arg));
        }

        /// <summary>
        /// 向串口写入字符串并写入一个换行符
        /// </summary>
        /// <param name="text">要写入的字符串</param>
        public void WriteLine(String text)
        {
            COM.WriteLine(text);
        }

        /// <summary>
        /// 向串口写入预先格式化的字符串和一个换行符
        /// </summary>
        /// <param name="format">字符格式串</param>
        /// <param name="arg">格式串参数</param>
        public void WriteLine(String format, params object[] arg)
        {
            COM.WriteLine(String.Format(format, arg));
        }

        #endregion

        #region 读取流
        /// <summary>
        /// 从串口输入缓冲读取一些字节,放入指定的字节组中
        /// </summary>
        /// <param name="buffer">将输入写入其中的字节数组</param>
        /// <returns>实际读取的字节数</returns>
        public int Read(byte[] buffer)
        {
            return COM.Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 从串口输入缓冲读取一些字节放入指定的字节组中指定的偏移量处
        /// </summary>
        /// <param name="buffer">将输入写入其中的字节数组</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">要读取的字节数</param>
        /// <returns>实际读取的字节数</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            return COM.Read(buffer, offset, count);
        }

        /// <summary>
        /// 从串口输入缓冲读取大量字符,放入指定的字符数组中
        /// </summary>
        /// <param name="buffer">将输入写入其中的字符数组</param>
        /// <returns>实际读取的字符数</returns>
        public int Read(char[] buffer)
        {
            return COM.Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 从串口输入缓冲读取大量的字符放入指定的字节组中指定的偏移量处
        /// </summary>
        /// <param name="buffer">将输入写入其中的字符数组</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">要读取的字符数</param>
        /// <returns>实际读取的字符数</returns>
        public int Read(char[] buffer, int offset, int count)
        {
            return COM.Read(buffer, offset, count);
        }

        /// <summary>
        /// 从串口输入缓冲区中同步读取一个字节
        /// </summary>
        /// <returns>强制转换为Int32 的字节；或者，如果已读取到流的末尾，则为 -1。</returns>
        public int ReadByte()
        {
            return COM.ReadByte();
        }

        /// <summary>
        /// 从串口输入缓冲区中同步读取一个字符
        /// </summary>
        /// <returns>读取的字符</returns>
        public int ReadChar()
        {
            return COM.ReadChar();
        }

        /// <summary>
        /// 在编码的基础上，读取串口对象的流和输入缓冲区中所有立即可用的字节
        /// </summary>
        /// <returns>串口对象的流和输入缓冲区的内容。</returns>
        public string ReadExisting()
        {
            return COM.ReadExisting();
        }

        /// <summary>
        /// 一直读取到输入缓冲区中的指定 value 的字符串。
        /// </summary>
        /// <param name="value">一直读取到输入缓冲区中的指定 value 的字符串</param>
        /// <returns>输入缓冲区中直到指定 value 的内容。</returns>
        public string ReadTo(string value)
        {
            return COM.ReadTo(value);
        }
        #endregion

        #region IDisposable 成员
        /// <summary>
        /// 释放由创建串口所使用的非托管资源
        /// </summary>
        public void Dispose()
        {
            COM.Dispose();
        }
        #endregion

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler  PropertyChanged;

        private void Changed(String Name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(Name));
            }
        }
        #endregion
    }
}
