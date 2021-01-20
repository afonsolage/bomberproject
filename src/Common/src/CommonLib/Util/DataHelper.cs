using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Util
{
    public class DataHelper
    {
        #region Type Conversion

        /// <summary>
        /// Safe conversion time
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long SafeConvertToTicks(string str)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                    return 0;

                if (!DateTime.TryParse(str, out DateTime dt))
                {
                    return 0L;
                }

                return dt.Ticks / 10000;
            }
            catch (Exception)
            {
            }

            return 0L;
        }

        /// <summary>
        /// Secure string-to-integer conversion
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int SafeConvertToInt32(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            str = str.Trim();
            if (string.IsNullOrEmpty(str))
                return 0;

            try
            {
                return Convert.ToInt32(str);
            }
            catch (Exception)
            {
            }

            return 0;
        }

        /// <summary>
        /// Secure string-to-integer conversion
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long SafeConvertToInt64(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            str = str.Trim();

            if (string.IsNullOrEmpty(str))
                return 0;

            try
            {
                return Convert.ToInt64(str);
            }
            catch (Exception)
            {
            }

            return 0;
        }

        /// <summary>
        /// Secure string to floating point conversion
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static double SafeConvertToDouble(string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0.0;

            str = str.Trim();

            if (string.IsNullOrEmpty(str))
                return 0.0;

            try
            {
                return Convert.ToDouble(str);
            }
            catch (Exception)
            {
            }

            return 0.0;
        }

        /// <summary>
        /// Converts a string to a Double array
        /// </summary>
        /// <param name="ss">Array of strings</param>
        /// <returns></returns>
        public static double[] String2DoubleArray(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            string[] sa = str.Split(',');
            return StringArray2DoubleArray(sa);
        }

        /// <summary>
        /// Converts an array of strings to an array of type double
        /// </summary>
        /// <param name="ss">Array of strings</param>
        /// <returns></returns>
        public static double[] StringArray2DoubleArray(string[] sa)
        {
            double[] da = new double[sa.Length];
            try
            {
                for (int i = 0; i < sa.Length; i++)
                {
                    string str = sa[i].Trim();
                    str = string.IsNullOrEmpty(str) ? "0.0" : str;
                    da[i] = Convert.ToDouble(str);
                }
            }
            catch (System.Exception ex)
            {
                string msg = ex.ToString();
            }

            return da;
        }

        /// <summary>
        /// Convert a string to an Int array
        /// </summary>
        /// <param name="ss">Array of strings</param>
        /// <returns></returns>
        public static int[] String2IntArray(string str, char spliter = ',')
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            string[] sa = str.Split(spliter);
            return StringArray2IntArray(sa);
        }

        /// <summary>
        /// Convert a string to an Int array
        /// </summary>
        /// <param name="ss">Array of strings</param>
        /// <returns></returns>
        public static string[] String2StringArray(string str, char spliter = '|')
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            return str.Split(spliter);
        }

        /// <summary>
        /// Converts an array of strings to an Int array
        /// </summary>
        /// <param name="ss">Array of strings</param>
        /// <returns></returns>
        public static int[] StringArray2IntArray(string[] sa)
        {
            if (sa == null)
                return null;
            return StringArray2IntArray(sa, 0, sa.Length);
        }

        public static int[] StringArray2IntArray(string[] sa, int start, int count)
        {
            if (sa == null) return null;
            if (start < 0 || start >= sa.Length) return null;
            if (count <= 0) return null;
            if (sa.Length - start < count) return null;

            int[] result = new int[count];
            for (int i = 0; i < count; ++i)
            {
                string str = sa[start + i].Trim();
                str = string.IsNullOrEmpty(str) ? "0" : str;
                result[i] = Convert.ToInt32(str);
            }

            return result;
        }
        #endregion // Type Conversion
    }
}
