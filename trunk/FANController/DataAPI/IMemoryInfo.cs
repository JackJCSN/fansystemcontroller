using System;
using System.ComponentModel;

namespace com.JackJCSN.DataAPI
{
    public interface IMemoryInfo : INotifyPropertyChanged, IDisposable
    {
        uint MemoryLoad { get; }
        ulong TotalPhys { get; }
        ulong AvailPhys { get; }
        ulong TotalPageFile { get; }
        ulong AvailPageFile { get; }
        ulong TotalVirtual { get; }
        ulong AvailVirtual { get; }
        void Update();
    }

    public class MemoryInfo : IMemoryInfo
    {
        private MemoryInfoHelper.MemoryStatus ms = new MemoryInfoHelper.MemoryStatus();
        private MemoryInfoHelper.MemoryStatusEx mse = new MemoryInfoHelper.MemoryStatusEx();
        private bool isMemoryStatus = false;

        #region IMemoryInfo 成员

        public uint MemoryLoad
        {
            get
            {
                if (isMemoryStatus)
                {
                    return ms.dwMemoryLoad;
                }
                else
                {
                    return mse.dwMemoryLoad;
                }
            }
        }

        public ulong TotalPhys
        {
            get
            {
                if (isMemoryStatus)
                {
                    return ms.dwTotalPhys / (1024 * 1024);
                }
                else
                {
                    return mse.ullTotalPhys / (1024 * 1024);
                }
            }
        }

        public ulong AvailPhys
        {
            get
            {
                if (isMemoryStatus)
                {
                    return ms.dwAvailPhys / (1024 * 1024);
                }
                else
                {
                    return mse.ullAvailPhys / (1024 * 1024);
                }
            }
        }

        public ulong TotalPageFile
        {
            get
            {
                if (isMemoryStatus)
                {
                    return ms.dwTotalPageFile / (1024 * 1024);
                }
                else
                {
                    return mse.ullTotalPageFile / (1024 * 1024);
                }
            }
        }

        public ulong AvailPageFile
        {
            get
            {
                if (isMemoryStatus)
                {
                    return ms.dwAvailPageFile / (1024 * 1024);
                }
                else
                {
                    return mse.ullTotalPageFile / (1024 * 1024);
                }
            }
        }

        public ulong TotalVirtual
        {
            get
            {
                if (isMemoryStatus)
                {
                    return ms.dwTotalVirtual / (1024 * 1024);
                }
                else
                {
                    return mse.ullTotalVirtual / (1024 * 1024);
                }
            }
        }

        public ulong AvailVirtual
        {
            get
            {
                if (isMemoryStatus)
                {
                    return ms.dwAvailVirtual / (1024 * 1024);
                }
                else
                {
                    return mse.ullAvailVirtual / (1024 * 1024);
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged 成员

        public event PropertyChangedEventHandler  PropertyChanged;

        protected void Change(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        public void Update()
        {
            if (this.GetMemoryStatusEx(ref this.mse))
            {
                this.isMemoryStatus = false;
                Change("MemoryLoad");
                Change("TotalPhys");
                Change("AvailPhys");
                Change("TotalPageFile");
                Change("AvailPageFile");
                Change("TotalVirtual");
                Change("AvailVirtual");
                return;
            }
            if (this.GetMemoryStatus(ref this.ms))
            {
                this.isMemoryStatus = true;
                Change("MemoryLoad");
                Change("TotalPhys");
                Change("AvailPhys");
                Change("TotalPageFile");
                Change("AvailPageFile");
                Change("TotalVirtual");
                Change("AvailVirtual");
                return;
            }
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
