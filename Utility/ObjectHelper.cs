using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Management;

namespace QA_LISSummary.Utility
{
    public static class ObjectHelper
    {
        public static int ToTypeInteger(this object obj, int defaultValue = 0)
        {
            int result = defaultValue;
            if (obj != null && obj != DBNull.Value)
            {
                if (!int.TryParse(obj.ToString(), out result))
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        public static double ToTypeDouble(this object obj, double defaultValue = 0.00)
        {
            double result = defaultValue;
            if (obj != null && obj != DBNull.Value)
            {
                if (!double.TryParse(obj.ToString(), out result))
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        public static string ToTypeString(this object obj, string defaultValue = "")
        {
            string result = defaultValue;
            if (obj != null && obj != DBNull.Value)
            {
                result = obj.ToString();
            }
            return result;
        }

        public static DateTime ToTypeDateTIme(this object obj)
        {
            DateTime result = new DateTime(1991, 1, 1);
            if (obj != null && obj != DBNull.Value)
            {
                DateTime.TryParse(obj.ToString(), out result);
            }
            return result;
        }

    }
}