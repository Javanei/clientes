using System.IO;
using System.Collections.Generic;
using NeodentUtil.util;

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

        public List<string> SplitPDF(string sourcePdfFile)
        {
            LOG.debug("(@@@@@@@@ SplitPDF - 1 - (sourcePdfFile=" + sourcePdfFile + ")=" + File.Exists(sourcePdfFile));
            string basedir = Directory.GetParent(sourcePdfFile).FullName;
            LOG.debug("@@@@@@@@ SplitPDF - 2 - basedir=" + basedir);
            string args = "-dNOPAUSE -dBATCH -dQUIET -sDEVICE=pdfwrite -o \"" + basedir + "\\out-%03d.pdf\" -f \"" + sourcePdfFile + "\"";
            LOG.debug("@@@@@@@@ SplitPDF - 3 - args=" + args);

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(executablePath, args)
            {
                ErrorDialog = false
            };
            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };

            LOG.debug("@@@@@@@@ SplitPDF - 4 - Vai executar");
            process.Start();
            if (!process.WaitForExit(30000))
            {
                process.Kill();
                throw new System.Exception("Timeout fazendo split do PDF: " + sourcePdfFile);
            }
            int exitCode = process.ExitCode;
            LOG.debug("@@@@@@@@ SplitPDF - 5 - Executou - exitCode=" + exitCode);
            process.Dispose();
            if (exitCode > 0)
            {
                throw new System.Exception("Erro fazendo split do PDF: " + sourcePdfFile + " exitCode=" + exitCode);
            }

            List<string> result = new List<string>();
            string[] files = Directory.GetFiles(basedir);
            foreach (string f in files)
            {
                if (f.ToLower().EndsWith(".pdf") && f.ToLower().Contains("out-"))
                {
                    LOG.debug("@@@@@@@@@@ SplitPDF - 6 - pdf=" + f);
                    result.Add(f);
                }
            }

            return result;
        }

        public bool MergePDFs(string destPdfFile, List<string> images)
        {
            LOG.debug("@@@@@@@@@@@@@@ MergePDFs - 1 - (destPdfFile=" + destPdfFile + ", images=" + images.Count + ")");
            if (images.Count > 0 && File.Exists(destPdfFile))
            {
                LOG.debug("@@@@@@@@@@@@@@ MergePDFs - 2 - Ja existia, deletando versao anterior");
                File.SetAttributes(destPdfFile, FileAttributes.Normal);
                File.Delete(destPdfFile);
                LOG.debug("@@@@@@@@@@@@@@ MergePDFs - 3 - Conseguiu deletar? =" + (!File.Exists(destPdfFile)));
            }

            if (images.Count == 1)
            {
                LOG.debug("@@@@@@@@@@@@@@ MergePDFs - 4 - So um arquivo, apenas copia");
                File.Copy(images[0], destPdfFile);
                if (!File.Exists(destPdfFile))
                {
                    throw new System.Exception("Nao conseguiu criar o arquivo destino: " + destPdfFile);
                }
                LOG.debug("@@@@@@@@@@@@@@ MergePDFs - 5 - Copia efetuada");
                return true;
            }
            else if (images.Count > 1)
            {
                string args = "-dNOPAUSE -dBATCH -dQUIET -sDEVICE=pdfwrite -sOUTPUTFILE=\"" + destPdfFile + "\"";
                foreach (string img in images)
                {
                    args = args + " \"" + img + "\"";
                }

                LOG.debug("@@@@@@@@@@@@@@ MergePDFs - 6 - executablePath=" + executablePath);
                LOG.debug("@@@@@@@@@@@@@@ MergePDFs - 7 - args=" + args);

                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(executablePath, args)
                {
                    ErrorDialog = false
                };
                System.Diagnostics.Process process = new System.Diagnostics.Process
                {
                    StartInfo = startInfo
                };

                LOG.debug("@@@@@@@@@@@@@@ MergePDFs - 8 - Vai executar");
                process.Start();
                if (!process.WaitForExit(30000))
                {
                    process.Kill();
                    throw new System.Exception("Timeout fazendo merge para o PDF: " + destPdfFile);
                }
                int exitCode = process.ExitCode;
                LOG.debug("@@@@@@@@@@@@@@ MergePDFs - 9 - Executou - exitCode=" + exitCode);
                process.Dispose();
                if (exitCode > 0)
                {
                    throw new System.Exception("Erro fazendo merge para o PDF " + destPdfFile + ", exitCode=" + exitCode);
                }
                if (!File.Exists(destPdfFile))
                {
                    throw new System.Exception("Nao conseguiu criar o arquivo destino: " + destPdfFile);
                }
                return true;
            }
            return false;
        }

        public List<string> PDFToJPG(string pdfFile, string imgTempfolder)
        {
            LOG.debug("@@@@@@@@ PDFToJPG - 1 - (pdfFile=" + pdfFile + ")");
            List<string> files = new List<string>();

            string args = "-dNOPAUSE"
                + " -dBATCH"
                + " -sDEVICE=jpeg"
                + " -dJPEGQ=100"
                + " -r100x100"
                + " -dQUIET"
                + " -f \"" + pdfFile + "\"";
            ;
            LOG.debug("@@@@@@@@ PDFToJPG - 2 - executablePath=" + executablePath);
            LOG.debug("@@@@@@@@ PDFToJPG - 3 - args=" + args);

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(executablePath, args)
            {
                ErrorDialog = false
            };
            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };

            LOG.debug("@@@@@@@@ PDFToJPG - 4 - Vai executar");
            process.Start();
            if (!process.WaitForExit(30000))
            {
                process.Kill();
                throw new System.Exception("Timeout convertendo para JPG o arquivo: " + pdfFile);
            }
            LOG.debug("@@@@@@@@ PDFToJPG - 5 - exitCode=" + process.ExitCode);

            LOG.debug("@@@@@@@@ PDFToJPG - 6 - Executou");
            process.Dispose();

            string basedir = Directory.GetParent(pdfFile).FullName;
            string[] images = Directory.GetFiles(basedir);
            foreach (string f in images)
            {
                if (f.EndsWith(".jpg"))
                {
                    LOG.debug("@@@@@@@@@@ PDFToJPG - 7 - encontrado arquivo=" + f);
                    files.Add(f);
                }
            }
            LOG.debug("@@@@@@@@ DwfToJPG - 8 - arquivos: " + files.Count);
            files.Sort();

            return files;
        }


        public List<string> PDFToPDF(string pdfFile, string imgTempfolder)
        {
            LOG.debug("@@@@@@@@ PDFToPDF - 1 - (pdfFile=" + pdfFile + ")");
            List<string> files = new List<string>();

            string destPdfFile = imgTempfolder + "\\"
                + Path.GetFileNameWithoutExtension(pdfFile) + "-out.pdf";
            LOG.debug("@@@@@@@@ PDFToPDF - 2 - destPdfFile=" + destPdfFile);

            string args = "-dNOPAUSE"
                + " -dBATCH"
                + " -dQUIET"
                + " -sDEVICE=pdfwrite"
                + " -sOUTPUTFILE=\"" + destPdfFile + "\""
                + " -f \"" + pdfFile + "\"";

            LOG.debug("@@@@@@@@ PDFToPDF - 3 - executablePath=" + executablePath);
            LOG.debug("@@@@@@@@ PDFToPDF - 4 - args=" + args);

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(executablePath, args)
            {
                ErrorDialog = false
            };
            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };

            LOG.debug("@@@@@@@@ PDFToPDF - 5 - Vai executar");
            process.Start();
            if (!process.WaitForExit(30000))
            {
                process.Kill();
                throw new System.Exception("Timeout convertendo para PDF o arquivo: " + pdfFile);
            }
            LOG.debug("@@@@@@@@ PDFToPDF - 6 - exitCode=" + process.ExitCode);

            LOG.debug("@@@@@@@@ PDFToPDF - 7 - Executou");
            process.Dispose();

            files.Add(destPdfFile);

            return files;
        }
    }
}
