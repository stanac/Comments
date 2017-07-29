using System;
using System.Data;

namespace Comments
{
    internal static class DataExtensions
    {
        public static T Get<T>(this IDataReader reader, string name)
        {
            object o = reader[name];
            if (o == DBNull.Value)
            {
                return default(T);
            }
            if (typeof(T) == typeof(bool) && o is long)
            {
                int val = (int)(long)o;
                object b = val == 1;
                return (T)b;
            }
            if (typeof(T) == typeof(int) && o is long)
            {
                long lVal = (long)o;
                object val = (int)lVal;
                return (T)val;
            }
            if ((typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?)) && o is string)
            {
                string s = o as string;
                object dt = DateTime.Parse(s);
                return (T)dt;
            }
            return (T)o;
        }

        public static IDbCommand AddParamWithValue(this IDbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
            return cmd;
        }
    }
}
