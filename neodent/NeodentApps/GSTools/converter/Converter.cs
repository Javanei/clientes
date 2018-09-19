using System.IO;
using System.Collections.Generic;

namespace GSTools.converter
{
    public class Converter
    {
        private string executablePath;

        public Converter(string executablePath)
        {
            this.executablePath = executablePath;
        }

        public List<string> PDFToJPG(string pdfFile, string imgTempfolder)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@ PDFToJPG - 1 - (pdfFile=" + pdfFile + ")");
            List<string> files = new List<string>();

            //     -f 114.419.pdf
            string args = "-dNOPAUSE"
                + " -dBATCH"
                + " -sDEVICE=jpeg"
                + " -dJPEGQ=100"
                + " -r100x100"
                + " -f \"" + pdfFile + "\"";
            ;
            NeodentUtil.util.LOG.debug("@@@@@@@@ PDFToJPG - 2 - executablePath=" + executablePath);
            NeodentUtil.util.LOG.debug("@@@@@@@@ PDFToJPG - 3 - args=" + args);

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(executablePath, args)
            {
                ErrorDialog = false
            };
            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };

            NeodentUtil.util.LOG.debug("@@@@@@@@ PDFToJPG - 4 - Vai executar");
            process.Start();
            process.WaitForExit();
            NeodentUtil.util.LOG.debug("@@@@@@@@ PDFToJPG - 5 - exitCode=" + process.ExitCode);

            NeodentUtil.util.LOG.debug("@@@@@@@@ PDFToJPG - 6 - Executou");
            process.Dispose();

            string basedir = Directory.GetParent(pdfFile).FullName;
            string[] images = Directory.GetFiles(basedir);
            foreach (string f in images)
            {
                if (f.EndsWith(".jpg"))
                {
                    NeodentUtil.util.LOG.debug("@@@@@@@@@@ PDFToJPG - 7 - encontrado arquivo=" + f);
                    files.Add(f);
                }
            }
            NeodentUtil.util.LOG.debug("@@@@@@@@ DwfToJPG - 8 - arquivos: " + files.Count);
            files.Sort();

            return files;
        }
    }
}
