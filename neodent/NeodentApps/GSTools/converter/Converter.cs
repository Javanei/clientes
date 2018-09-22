using System.IO;
using System.Collections.Generic;

/*
Paper Keywords and paper size in points
---------------------------------------
Letter		 612x792
LetterSmall	 612x792
Tabloid		 792x1224
Ledger		 1224x792
Legal		 612x1008
Statement	 396x612
Executive	 540x720
A0           2384x3371
A1           1685x2384
A2		     1190x1684
A3		     842x1190
A4		     595x842
A4Small		 595x842
A5		     420x595
B4		     729x1032
B5		     516x729
Envelope	 ???x???
Folio		 612x936
Quarto		 610x780
10x14		 720x1008
*/
namespace GSTools.converter
{
    public class Converter
    {
        private string executablePath;

        public Converter(string executablePath)
        {
            this.executablePath = executablePath;
        }

        public bool MergePDFs(string destPdfFile, List<string> images)
        {
            if (images.Count > 0 && File.Exists(destPdfFile))
            {
                File.SetAttributes(destPdfFile, FileAttributes.Normal);
                File.Delete(destPdfFile);
            }

            NeodentUtil.util.LOG.debug("@@@@@@@@ MergePDFs - 1 - (destPdfFile=" + destPdfFile + ")");
            if (images.Count == 1)
            {
                NeodentUtil.util.LOG.debug("@@@@@@@@ MergePDFs - 2 - Só um arquivo, apenas copia");
                File.Copy(images[0], destPdfFile);
                return true;
            }
            else if (images.Count > 1)
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
                return true;
            }
            return false;
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
                + " -dQUIET"
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
