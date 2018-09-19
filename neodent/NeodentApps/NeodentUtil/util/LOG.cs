using System;

namespace NeodentUtil.util
{
    public class LOG
    {
        public static bool geraLog = true;

        public static void debug(string s)
        {
            if (geraLog)
            {
                Console.WriteLine(DateTime.Now + " " + s);
            }
        }
    }
}
