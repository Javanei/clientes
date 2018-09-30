using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using Bullzip.PdfWriter;

using DWFCore.dwf;
using NeodentUtil.util;

namespace BullzipPDFTools.converter
{
    public class Converter : IDWFConverter
    {
        public Converter()
        {
        }

        public List<string> DwfToPDF(string dwfFile, string imgTempfolder, string[] sheetPrefixes)
        {
            LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 1 - (dwfFile=" + dwfFile + ")");
            List<string> imgToConvert = new List<string>();

            Dictionary<string, string> fileProps = new Dictionary<string, string>();
            DWFTools.util.DWFUtil.Extract(imgTempfolder, dwfFile, fileProps, sheetPrefixes);
            LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 2 - fileProps: " + fileProps.Count);
            if (fileProps.Count <= 2)
            {
                LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 3 - Não ha nenhum desenho a ser impresso");
                return imgToConvert;
            }
            LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 4 - imagens a serem impressas: " + (fileProps.Count - 2));
            int.TryParse(DictionaryUtil.GetProperty(fileProps, "-1"), out int totalPages);
            LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 5 - total de imagens no desenho: " + totalPages);
            bool singleFile = totalPages == (fileProps.Count - 2);
            LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 6 - Imprimir em arquivo unico?: " + singleFile);
            string codigoDesenho = Directory.GetParent(dwfFile).Name;
            LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 7 - Codigo do desenho: " + codigoDesenho);

            PdfSettings settings = new PdfSettings();
            settings.LoadSettings(settings.GetSettingsFilePath(false));
            settings.SetValue(BullzipPDFOptions.ConfirmOverwrite.ToString(), "no");
            settings.SetValue(BullzipPDFOptions.ConfirmNewFolder.ToString(), "no");
            settings.SetValue(BullzipPDFOptions.ShowSaveAS.ToString(), "never");
            settings.SetValue(BullzipPDFOptions.ShowSettings.ToString(), "never");
            settings.SetValue(BullzipPDFOptions.ShowPDF.ToString(), "no");
            settings.SetValue(BullzipPDFOptions.OpenFolder.ToString(), "no");
            settings.SetValue(BullzipPDFOptions.ShowProgress.ToString(), "no");
            settings.SetValue(BullzipPDFOptions.ShowProgressFinished.ToString(), "no");
            settings.SetValue(BullzipPDFOptions.RememberLastFileName.ToString(), "no");
            settings.SetValue(BullzipPDFOptions.RememberLastFolderName.ToString(), "yes");
            settings.SetValue(BullzipPDFOptions.SuppressErrors.ToString(), "yes");
            settings.SetValue(BullzipPDFOptions.ImageCompression.ToString(), "no");
            settings.SetValue(BullzipPDFOptions.Target.ToString(), "default");
            settings.SetValue(BullzipPDFOptions.Zoom.ToString(), "default");
            settings.SetValue(BullzipPDFOptions.UseThumbs.ToString(), "no");
            settings.SetValue(BullzipPDFOptions.Device.ToString(), "pdfwrite");
            settings.SetValue(BullzipPDFOptions.TextAlphaBits.ToString(), "4");
            settings.SetValue(BullzipPDFOptions.GraphicsAlphaBits.ToString(), "4");
            settings.SetValue(BullzipPDFOptions.CompatibilityLevel.ToString(), "1.5");
            settings.SetValue(BullzipPDFOptions.AppendIfExists.ToString(), "no");
            settings.SetValue(BullzipPDFOptions.EmbedAllFonts.ToString(), "yes");
            settings.SetValue(BullzipPDFOptions.DeleteOutput.ToString(), "no");

            string logFile = imgTempfolder + "\\" + codigoDesenho + ".log";
            string outPdf = imgTempfolder + "\\" + codigoDesenho + ".pdf";
            settings.SetValue(BullzipPDFOptions.StatusFile.ToString(), logFile);
            if (singleFile)
            {
                settings.SetValue(BullzipPDFOptions.Output.ToString(), outPdf);
            }
            else
            {
                settings.SetValue(BullzipPDFOptions.Output.ToString(), imgTempfolder + "\\" + codigoDesenho + "-<pageno>.pdf");
            }

            settings.WriteSettings(settings.GetSettingsFilePath(false));

            string printerName = settings.PrinterName;

            ProcessUtil.PrintFile(dwfFile, printerName, 60000);

            // Aguarda a criaçao do arquivo de LOG
            LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 8 - Aguardando a geracao do arquivo de log: " + logFile);
            bool result = PdfUtil.WaitForFile(logFile, 60000);
            if (!result)
            {
                throw new System.Exception("Não conseguiu gerar os PDFs no tempo esperado");
            }

            // Imprimiu, mas imprimiu tudo mesmo?
            if (singleFile)
            {
                if (!File.Exists(outPdf))
                {
                    LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 9 - Nao gerou o arquivo: " + outPdf);
                    throw new System.Exception("A impressao do desenho falhou");
                }
            }
            else
            {
                FileInfo[] printedFils = Directory.GetParent(dwfFile).GetFiles();
                if (printedFils.Length < totalPages)
                {
                    LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 10 - Era esperado " + totalPages + " arquivos, porém foram gerados " + printedFils.Length);

                    throw new System.Exception("A impressao do desenho falhou");
                }
            }

            PdfStatus pdfStatus = new PdfStatus(logFile);
            LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 11 - Arquivos PDF gerados: " + pdfStatus.FileCount);
            if (pdfStatus.FileCount == 0)
            {
                throw new System.Exception("Não gerou nenhum PDF");
            }

            if (singleFile)
            {
                imgToConvert.Add(pdfStatus.Files[0]);
            }
            else
            {
                List<string> files = pdfStatus.Files;
                foreach (var key in fileProps.Keys)
                {
                    if (int.TryParse(key, out int v))
                    {
                        if (v > 0)
                        {
                            LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 12 - desenho valido: "
                                + key + " -> " + v + "=" + DictionaryUtil.GetProperty(fileProps, key));
                            imgToConvert.Add(files[v - 1]);
                        }
                    }
                }

                LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 13 - Vai mergear para o arquivo destino: " + outPdf);
                PdfUtil.Merge(imgToConvert.ToArray(), outPdf, printerName, 120000);
                LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 14 - Mergeou para o arquivo destino: " + outPdf + ", existe?: " + File.Exists(outPdf));
                imgToConvert.Clear();
                imgToConvert.Add(outPdf);
                foreach (string file in pdfStatus.Files)
                {
                    LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 15 - excluindo arquivo: " + file);
                    File.Delete(file);
                }
            }
            LOG.debug("@@@@@@@@ BullzipPDFTools.DwfToPDF - 16 - FIM: " + imgToConvert.Count);
            return imgToConvert;
        }
    }
}
