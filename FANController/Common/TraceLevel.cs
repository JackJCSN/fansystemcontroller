using System;
using System.Diagnostics;
using System.IO;

namespace com.JackJCSN.DataAPI
{
    public enum TraceLevel
    {
        ERROR,
        WARRING,
        NOTICE,
        INFO
    }

    public struct LogerMsg
    {
        public String msg;
        public TraceLevel Level;

        public LogerMsg(String m, TraceLevel t = TraceLevel.INFO)
        {
            msg = m;
            Level = t;
        }
    }

    public class Loger
    {
        public String ModleName { get; private set; }
        public TraceLevel TraceLevel { get; set; }
        private FileStream fs;
        private StreamWriter output = null;
        private string Format = "[{0}]\r\n{1}\r\n{2}";

        public Loger(string modleName, TraceLevel Level = TraceLevel.ERROR)
        {
            ModleName = modleName;
            TraceLevel = Level;
            try
            {
                fs = new FileStream(modleName+".log", FileMode.Append | FileMode.OpenOrCreate);
                if (fs != null)
                {
                    output = new StreamWriter(fs);
                }
            }
            catch
            {
            }            
        }

        ~Loger()
        {
            try
            {
                if (output != null)
                {
                    output.Close();
                }
            }
            catch
            {
            }
            finally
            {
                try
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }
                catch
                {
                }
            }
        }

        public void Log(LogerMsg msg)
        {
            if (TraceLevel > msg.Level)
            {
                switch (msg.Level)
                {
                    case TraceLevel.ERROR:
                        ErrorMSG(msg.msg);
                        break;
                    case TraceLevel.WARRING:
                        ErrorMSG(msg.msg);
                        break;
                    case TraceLevel.NOTICE:
                        ErrorMSG(msg.msg);
                        break;
                    case TraceLevel.INFO:
                        ErrorMSG(msg.msg);
                        break;
                    default: break;
                }
            }
        }

        #region 记录
        public void ErrorMSG(string msg)
        {
            output.WriteLine(Format,TraceLevel.ERROR,DateTime.Now,msg);
            Trace.WriteLine(msg);
        }

        public void ErrorMSG(Exception ex)
        {
            output.WriteLine(Format, TraceLevel.ERROR, DateTime.Now, ex.ToString());
            Trace.WriteLine(ex);
        }

        public void WarringMSG(string msg)
        {
            Trace.WriteLine(msg);
        }

        public void WarringMSG(Exception ex)
        {
            Trace.WriteLine(ex);
        }

        public void NoticeMSG(string msg)
        {
            Trace.WriteLine(msg);
        }

        public void InfoMSG(string msg)
        {
            Trace.WriteLine(msg);
        }
        #endregion
    }
}
