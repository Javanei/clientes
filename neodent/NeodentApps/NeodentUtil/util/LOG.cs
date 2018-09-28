using System;

namespace NeodentUtil.util
{
    public class LOG
    {
        public static bool DEBUG = true;
        public static bool INFO = true;
        public static bool ERROR = true;

        public static void debug(string s)
        {
            if (DEBUG)
            {
                Console.WriteLine(DateTime.Now + " DEBUG: " + s);
            }
        }

        public static void info(string s)
        {
            if (INFO)
            {
                Console.WriteLine(DateTime.Now + "  INFO: " + s);
            }
        }

        public static void error(string s)
        {
            if (ERROR)
            {
                Console.WriteLine(DateTime.Now + " ERROR: " + s);
            }
        }
    }
}
