using System;
using System.Collections.Generic;
using System.IO;

namespace NeodentUtil.util
{
    public class DictionaryUtil
    {
        public static Dictionary<string, string> ReadPropertyFile(string filename)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();

            if (!File.Exists(filename))
            {
                return d;
            }

            StreamReader SR;
            string S;
            SR = File.OpenText(filename);
            S = SR.ReadLine();
            while (S != null)
            {
                if (S.IndexOf('=') > 0)
                {
                    String key = S.Substring(0, S.IndexOf('='));
                    String value = S.Substring(S.IndexOf('=') + 1);
                    d.Add(key, value);
                }
                S = SR.ReadLine();
            }
            SR.Close();

            return d;
        }

        public static void WritePropertyFile(string filename, Dictionary<string, string> d)
        {
            StreamWriter SW;
            SW = File.CreateText(filename);

            string value = null;
            foreach (string k in d.Keys)
            {
                d.TryGetValue(k, out value);
                SW.WriteLine(k + "=" + value);
            }

            SW.Close();
        }

        public static string GetProperty(Dictionary<string, string> d, string key)
        {
            string value = null;
            d.TryGetValue(key, out value);
            return value;
        }

        public static void SetProperty(Dictionary<string, string> d, string key, string value)
        {
            if (d.ContainsKey(key))
                d.Remove(key);
            d.Add(key, value);
        }
    }
}
