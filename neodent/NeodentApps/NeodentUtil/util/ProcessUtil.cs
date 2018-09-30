using System;
using System.Diagnostics;

namespace NeodentUtil.util
{
    public class ProcessUtil
    {
        public static void Kill(Process process)
        {
            LOG.debug("@@@@@@@@@@ ProcessUtil.Kill - 1 - Encerrando processo, ID=" + process.Id + ", Name=" + process.ProcessName);
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("taskkill", "/F /T /PID " + process.Id)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                Process.Start(processStartInfo);
                LOG.debug("@@@@@@@@@@ ProcessUtil.Kill - 2 - Processo encerrado");
            }
            catch (Exception e)
            {
                LOG.error("ProcessUtil.Kill - Erro encerrando processo: " + e.Message);
            }
        }

        public static void PrintFile(string file, string printerName, int timeoutInMS)
        {
            LOG.debug("@@@@@@@@@@ ProcessUtil.PrintFile - 1 - Iniciando impressao do arquivo \"" + file
                + "\" na impressora \"" + printerName + "\". Tempo maximo: " + timeoutInMS + "ms");
            ProcessStartInfo info = new ProcessStartInfo(file)
            {
                Verb = "PrintTo",
                CreateNoWindow = true,
                Arguments = "\"" + printerName + "\"",
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process process = Process.Start(info);
            bool result = process.WaitForExit(timeoutInMS);
            if (!result)
            {
                throw new Exception("A impressao do arquivo \"" + file + "\" na impressora \"" + printerName + "\" nao terminou no tempo de " + timeoutInMS + "ms. Encerrando processo");
            }
            LOG.debug("@@@@@@@@@@ ProcessUtil.PrintFile - 2 - Finalizado impressao do arquivo \"" + file + "\"");
        }
    }
}
