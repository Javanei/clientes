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

        public void MergePDFs(string destPdfFile, List<string> images)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@ MergePDFs - 1 - (destPdfFile=" + destPdfFile + ")");
            if (images.Count == 1)
            {
                NeodentUtil.util.LOG.debug("@@@@@@@@ MergePDFs - 2 - Só um arquivo, apenas copia");
                File.Copy(images[0], destPdfFile);
            } else if (images.Count > 1)
            {
                string args = "-dNOPAUSE -dBATCH -dQUIET -sDEVICE=pdfwrite -sOUTPUTFILE=\"" + destPdfFile + "\"";
                foreach (string img in images)
                {
                    args = args + " \"" + img + "\"";
                }

                NeodentUtil.util.LOG.debug("@@@@@@@@ MergePDFs - 3 - executablePath=" + executablePath);
                NeodentUtil.util.LOG.debug("@@@@@@@@ MergePDFs - 4 - args=" + args);

                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(executablePath, args)
                {
                    ErrorDialog = false
                };
                System.Diagnostics.Process process = new System.Diagnostics.Process
                {
                    StartInfo = startInfo
                };

                NeodentUtil.util.LOG.debug("@@@@@@@@ MergePDFs - 5 - Vai executar");
                process.Start();
                process.WaitForExit();
                NeodentUtil.util.LOG.debug("@@@@@@@@ MergePDFs - 6 - Executou - exitCode=" + process.ExitCode);
                process.Dispose();
            }
        }

        public List<string> PDFToJPG(string pdfFile, string imgTempfolder)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@ PDFToJPG - 1 - (pdfFile=" + pdfFile + ")");
            List<string> files = new List<string>();

            string args = "-dNOPAUSE"
                + " -dBATCH"
                          + " -dQUIET"
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
