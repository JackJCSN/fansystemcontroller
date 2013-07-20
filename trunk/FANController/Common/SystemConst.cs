using System.Windows.Media;
using com.JackJCSN.DataAPI.Properties;

namespace com.JackJCSN.DataAPI
{
    /// <summary>
    /// System Consts & ReadOnlys
    /// </summary>
    public class SC
    {
        #region CONTS

        #endregion

        #region Readonly
        public static readonly Color rgbTemplateStart;
        public static readonly Color rgbTemplateEnd;
        public static readonly Color rgbTemplateMid;
        public static readonly Color rgbLoadStart;
        public static readonly Color rgbLoadEnd;
        public static readonly Color rgbLoadMid;
        public static readonly Color rgbTemplateError;
        public static readonly Color rgbLoadError;
        public static readonly int TempHight;
        public static readonly int TempLowest;
        public static readonly int TempWarring;
        public static readonly int LoadWarring;

        static SC()
        {
            rgbTemplateStart = Settings.Default.TC1;
            rgbTemplateMid = Settings.Default.TC2;
            rgbTemplateEnd = Settings.Default.TC3;
            rgbLoadStart = Settings.Default.LC1;
            rgbLoadMid = Settings.Default.LC2;
            rgbLoadEnd = Settings.Default.LC3;
            rgbLoadError = Settings.Default.LE;
            rgbTemplateError = Settings.Default.TE;
            TempHight = Settings.Default.TH;
            TempLowest = Settings.Default.TL < TempHight ? Settings.Default.TL : TempHight - 100;
            var t = Settings.Default.TW;
            TempWarring = (t > TempHight || t < TempLowest) ? t : (TempHight - TempLowest) / 2;
            var a = Settings.Default.LW;
            LoadWarring = (a > 100 || a < 0) ? a : 75;
        }
        #endregion
    }
}
