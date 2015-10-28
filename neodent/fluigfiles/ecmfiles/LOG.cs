using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ecmfiles
{
    class LOG
    {
        public static Boolean geraLog = true;

        public static void imprimeLog(string s)
        {
            if (geraLog)
            {
                Console.WriteLine(s);
            }
        }
    }
}
