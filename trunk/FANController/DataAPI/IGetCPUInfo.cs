using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GetCoreTempInfoNET;

namespace com.JackJCSN.DataAPI
{
    /// <summary>
    /// 定义一组获取CPU温度等信息的方法
    /// </summary>
    public interface IGetCPUInfo : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// CPU系统名称
        /// </summary>
        String CPUName { get; }
        /// <summary>
        /// CPU当前运行主频频率
        /// </summary>
        String Frequency { get; }
        /// <summary>
        /// CPU当前运行时钟频率
        /// </summary>
        String InnerFrequency { get; }
        /// <summary>
        /// CPU当前运行倍频
        /// </summary>
        String FrequencyLock { get; }
        /// <summary>
        /// 当前CPU的核心数量
        /// </summary>
        Int32 CoreCount { get; }
        /// <summary>
        /// 当前CPU的负载信息
        /// </summary>
        Int32 CPULoad { get; }
        /// <summary>
        /// 当前CPU的核心平均温度
        /// </summary>
        Double CPUTemplater { get; }
    }

    /// <summary>
    /// 从CoreTemp获取CPU温度等信息
    /// </summary>
    public class GetCPUInfo : IGetCPUInfo, INotifyPropertyChanged, IDisposable
    {
        Thread thread;
        Process coretemp;
        private bool isCreate = false;
        private static Loger loger;
        /// <summary>
        /// 初始化CoreTemp
        /// </summary>
        public GetCPUInfo()
        {
            if (loger == null)
            {
                loger = new Loger("GetCPUInfo");
            }            
            if (coretemp == null)
            {
                try
                {
                    coretemp = Process.GetProcessesByName("Core Temp")[0];
                }
                catch (IndexOutOfRangeException)
                {                    
                    try
                    {
                        coretemp = new Process();
                        coretemp.StartInfo = new ProcessStartInfo(".\\Core Temp.exe");
                        coretemp.StartInfo.CreateNoWindow = true;
                        coretemp.StartInfo.Verb = "runas";
                        coretemp.Start();
                        isCreate = true;
                    }
                    catch(Exception ex)
                    {
                        loger.ErrorMSG(ex);
                        throw new InvalidOperationException(RE.ProcessNotFind);
                    }
                }
            }
            thread = new Thread(() =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                    this.Update();
                }
            });
            thread.Name = "CoreTemp UpDating";
            thread.Start();
        }

        ~GetCPUInfo()
        {
            try
            {
                this.Dispose();
            }
            catch (Exception)
            {
            }
        }

        #region INotifyPropertyChanged 成员
        public event PropertyChangedEventHandler  PropertyChanged;

        public void Changed(String PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        #endregion

        #region 更新
        CoreTempInfo c = new CoreTempInfo();

        /// <summary>
        /// 强制更新CPU信息
        /// </summary>
        public void Update()
        {
            if (c.GetData())
            {
                Changed("CPUTemplater");
                Changed("CPULoad");
                Changed("Frequency");
                Changed("InnerFrequency");
                Changed("CPUName");
                Changed("FrequencyLock");
                Changed("CoreCount");
            }
        }

        #endregion

        #region IGetCPUInfo 成员

        /// <summary>
        /// 当前CPU的名字
        /// </summary>
        public string CPUName
        {
            get { return c.GetCPUName; }
        }

        /// <summary>
        /// 当前CPU运行频率
        /// </summary>
        public string Frequency
        {
            get { return c.GetCPUSpeed.ToString(); }
        }

        /// <summary>
        /// 当前CPU的主频
        /// </summary>
        public string InnerFrequency
        {
            get { return c.GetFSBSpeed.ToString(); }
        }

        /// <summary>
        /// 当前CPU的倍频
        /// </summary>
        public string FrequencyLock
        {
            get { return c.GetMultiplier.ToString(); }
        }

        /// <summary>
        /// 当前CPU的核心数量
        /// </summary>
        public int CoreCount
        {
            get { return (int)c.GetCoreCount; }
        }

        /// <summary>
        /// 当前CPU的负载
        /// </summary>
        public int CPULoad
        {
            get
            {
                try
                {
                    decimal sum = c.GetCoreLoad.Sum((a) => { return (decimal)a; });
                    return (int)(sum / c.GetCoreCount);
                }
                catch (Exception ex)
                {
                    loger.WarringMSG(ex);
                    return 0;
                }
            }
        }

        /// <summary>
        /// 当前CPU的核心平均温度
        /// </summary>
        public double CPUTemplater
        {
            get
            {
                float dc = 0;
                double re = 0;
                try
                {
                    foreach (float a in c.GetTemp)
                    {
                        dc = dc + a;
                    }

                    if (c.GetCoreCount != 0)
                    {
                        re = (dc / c.GetCoreCount);
                    }

                    if (c.IsFahrenheit)
                    {
                        re = (double)((5.0 / 9.0) * (re - 32.0));
                    }
                    return re;
                }
                catch (Exception ex)
                {
                    loger.WarringMSG(ex);
                    return 0;
                }
            }
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            thread.Abort();
            if (isCreate)
            {
                coretemp.Close();
            }
        }

        #endregion
    }
}
