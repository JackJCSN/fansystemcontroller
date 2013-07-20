using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace FANController
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private Mutex mutex = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            bool createNew = false;
            mutex = new Mutex(true, "com.JackJCSN.FANController", out createNew);
            if (createNew)
            {
                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show("程序已经在运行。", "智能实时散热系统",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        ~App()
        {
            if (mutex != null)
            {
                try
                {
                    mutex.Dispose();
                }
                catch
                {
                }
            }
        }
    }
}
