using System;
using System.Diagnostics;

namespace NeodentUtil.util
{
    public class ProcessUtil
    {
        public static void Kill(Process process)
        {
            LOG.debug("@@@@@@@@@@ ProcessUtil.Kill - 1 - Encerrando processo, ID=" + process.Id + ", Name=" + process.ProcessName);
            KillByPid(process.Id);
        }

        public static void KillByPid(int pid)
        {
            LOG.debug("@@@@@@@@@@ ProcessUtil.KillByPid - 1 - Encerrando processo com PID=" + pid);
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("taskkill", "/F /T /PID " + pid)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                Process.Start(processStartInfo);
                LOG.debug("@@@@@@@@@@ ProcessUtil.KillByPid - 2 - Processo encerrado");
            }
            catch (Exception e)
            {
                LOG.error("ProcessUtil.KillByPid - Erro encerrando processo: " + e.Message);
            }
        }

        public static void KillByImageName(string imageName)
        {
            LOG.debug("@@@@@@@@@@ ProcessUtil.KillByImageName - 1 - Encerrando processo com ImageName=" + imageName);
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("taskkill", "/F /T /IM \"" + imageName + "\"")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                Process.Start(processStartInfo);
                LOG.debug("@@@@@@@@@@ ProcessUtil.KillByImageName - 2 - Processo encerrado");
            }
            catch (Exception e)
            {
                LOG.error("ProcessUtil.KillByImageName - Erro encerrando processo: " + e.Message);
            }
        }

        public static void KillByWindowTitle(string windowTitle)
        {
            LOG.debug("@@@@@@@@@@ ProcessUtil.KillByWindowTitle - 1 - Encerrando processo com WindowTitle=" + windowTitle);
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("taskkill", "/F /T /fi \"WINDOWTITLE eq " + windowTitle + "\"")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                Process.Start(processStartInfo);
                LOG.debug("@@@@@@@@@@ ProcessUtil.KillByWindowTitle - 2 - Processo encerrado");
            }
            catch (Exception e)
            {
                LOG.error("ProcessUtil.KillByWindowTitle - Erro encerrando processo: " + e.Message);
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
            bool result = false;
            Process process = Process.Start(info);
            LOG.debug("@@@@@@@@@@ ProcessUtil.PrintFile - 2 - Criado processo com PID=" + process.Id + ", Name=" + process.ProcessName);
            try
            {
                result = process.WaitForExit(timeoutInMS);
            }
            catch (Exception ex)
            {
                Kill(process);
                throw new Exception("A impressao do arquivo \"" + file + "\" na impressora \"" + printerName + "\" falhou com o erro: " + ex.Message);
            }
            if (!result)
            {
                Kill(process);
                throw new Exception("A impressao do arquivo \"" + file + "\" na impressora \"" + printerName + "\" nao terminou no tempo de " + timeoutInMS + "ms. Encerrando processo");
            }
            LOG.debug("@@@@@@@@@@ ProcessUtil.PrintFile - 3 - Finalizado impressao do arquivo \"" + file + "\"");
        }
    }
}
