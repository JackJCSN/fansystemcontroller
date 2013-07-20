using System;
using System.ComponentModel;

namespace FANController.Properties
{
    public partial class Settings : INotifyPropertyChanged
    {

        public String PortName
        {
            get { return this.硬件通讯端口; }
            set
            {
                if (value != this.硬件通讯端口)
                {
                    this.硬件通讯端口 = value;
                    Changed("PortName");
                }
            }
        }

        public Boolean UseDefaultRate
        {
            get { return this.使用默认通讯频率; }
            set
            {
                if (value != this.使用默认通讯频率)
                {
                    if (value == true)
                    {
                        this.Rate = this.DefaultRate;
                    }
                    this.使用默认通讯频率 = value;
                    Changed("UseDefaultRate");
                }
            }
        }

        public int Rate
        {
            get { return this.硬件通讯频率; }
            set
            {
                if (value != this.硬件通讯频率)
                {
                    this.硬件通讯频率 = value;
                    Changed("Rate");
                }
            }
        }

        public int DefaultRate
        {
            get { return this.默认硬件通讯频率; }
        }

        #region INotifyPropertyChanged 成员

        public new event PropertyChangedEventHandler PropertyChanged;

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
