using System;
using System.Collections.Generic;
using System.Text;

namespace vaultsrv
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
