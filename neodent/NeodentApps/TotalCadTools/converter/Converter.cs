using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;

using DWFCore.dwf;
using NeodentUtil.util;

namespace TotalCadTools.converter
{
    public class Converter : IDWFConverter
    {
        private string executablePath;
        private string pdfExecutablePath;
        private Type oType;

        public Converter(string executablePath, string pdfExecutablePath)
        {
            this.executablePath = executablePath;
            this.pdfExecutablePath = pdfExecutablePath;

            oType = Type.GetTypeFromProgID("CADConverter.CADConverterX");
            if (oType == null)
            {
                throw new Exception("Não conseguiu instanciar o CADConverterX");
            }
        }

        public List<string> DwfToPDF(string dwfFile, string imgTempfolder, string[] sheetPrefixes)
        {
            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 1 - (dwfFile=" + dwfFile + ")");
            List<string> files = new List<string>();

            object Conv = Activator.CreateInstance(oType);

            string sFormat = "PDF";
            string pdfToConvert = Path.ChangeExtension(dwfFile, "." + sFormat);
            string sCmd = "-fo -cm n -c " + sFormat;
            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 2 - pdfToConvert: " + pdfToConvert);
            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 3 - sCmd: " + sCmd);

            object[] parameters;
            parameters = new object[3];
            parameters[0] = dwfFile;
            parameters[1] = pdfToConvert;
            parameters[2] = sCmd;

            object convResult = Conv.GetType().InvokeMember("Convert", BindingFlags.InvokeMethod, null, Conv, parameters);
            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 4 - convResult: " + convResult);

            List<string> imgToConvert = new List<string>();

            Dictionary<string, string> fileProps = new Dictionary<string, string>();
            DWFTools.util.DWFUtil.Extract(imgTempfolder, dwfFile, fileProps, sheetPrefixes);
            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 5 - fileProps: " + fileProps.Count);
            if (fileProps.Count > 1)
            {
                //Encontrou desenhos
                GSTools.converter.Converter pdfsplit = new GSTools.converter.Converter(pdfExecutablePath);
                files = pdfsplit.SplitPDF(pdfToConvert);
                LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 6 - files: " + files.Count);

                if (files.Count > 0)
                {
                    files.Sort();

                    foreach (var key in fileProps.Keys)
                    {
                        int v;
                        if (int.TryParse(key, out v))
                        {
                            if (v > 0)
                            {
                                LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 7 - desenho valido: "
                                    + key + " -> " + v + "=" + DictionaryUtil.GetProperty(fileProps, key));
                                imgToConvert.Add(files[v - 1]);
                            }
                        }
                    }
                }

                if (fileProps.Count == (imgToConvert.Count + 1))
                {
                    LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 8 - Todos os desenhos sao validos, entao usa o original");
                    imgToConvert.Clear();
                    imgToConvert.Add(pdfToConvert);
                }
            }

            // Exclui as imagens nao necessarias
            foreach (string file in files)
            {
                if (!imgToConvert.Contains(file))
                {
                    LOG.debug("@@@@@@@@@@ TotalCadTools.DwfToPDF - 9 - excluindo arquivo: " + file);
                    File.Delete(file);
                }
            }

            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 9 - total final de arquivos: " + imgToConvert.Count);
            return imgToConvert;
        }

        /*
        public List<string> DwfToPDF(string dwfFile, string imgTempfolder, string[] sheetPrefixes)
        {
            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 1 - (dwfFile=" + dwfFile + ")");
            List<string> files = new List<string>();

            string args = "\"" + dwfFile + "\""
                + " \"" + imgTempfolder + "\""
                + " -fo"
                + " -c PDF"
                //+ " -combine:off"
                //+ " -cimt none"
                //+ " -po Portrait"
                //+ " -ps A4" Ferra tudo
                //+ " -pc F"
                //+ " -pvl sp"
                ;

            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 2 - executablePath=" + executablePath);
            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 3 - args=" + args);

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(executablePath, args)
            {
                ErrorDialog = false
            };
            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };

            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 4 - Vai executar");
            process.Start();
            process.WaitForExit();
            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 5 - exitCode=" + process.ExitCode);

            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 6 - Executou");
            process.Dispose();

            string basedir = Directory.GetParent(dwfFile).FullName;
            string[] images = Directory.GetFiles(basedir);
            string pdfToConvert = null;
            foreach (string f in images)
            {
                if (f.EndsWith(".pdf"))
                {
                    LOG.debug("@@@@@@@@@@ TotalCadTools.DwfToPDF - 7 - encontrado arquivo=" + f);
                    pdfToConvert = f;
                    break;
                }
            }
            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 8 - pdfToConvert: " + pdfToConvert);

            List<string> imgToConvert = new List<string>();

            Dictionary<string, string> fileProps = new Dictionary<string, string>();
            DWFTools.util.DWFUtil.Extract(imgTempfolder, dwfFile, fileProps, sheetPrefixes);
            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 9 - fileProps: " + fileProps.Count);
            if (fileProps.Count > 1)
            {
                //Encontrou desenhos
                GSTools.converter.Converter pdfsplit = new GSTools.converter.Converter(pdfExecutablePath);
                files = pdfsplit.SplitPDF(pdfToConvert);
                LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 10 - files: " + files.Count);

                if (files.Count > 0)
                {
                    files.Sort();

                    foreach (var key in fileProps.Keys)
                    {
                        int v;
                        if (int.TryParse(key, out v))
                        {
                            if (v > 0)
                            {
                                LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 11 - desenho valido: "
                                    + key + " -> " + v + "=" + DictionaryUtil.GetProperty(fileProps, key));
                                imgToConvert.Add(files[v - 1]);
                            }
                        }
                    }
                }

                if (fileProps.Count == (imgToConvert.Count + 1))
                {
                    LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 12 - Todos os desenhos sao validos, entao usa o original");
                    imgToConvert.Clear();
                    imgToConvert.Add(pdfToConvert);
                }
            }

            // Exclui as imagens nao necessarias
            foreach (string file in files)
            {
                if (!imgToConvert.Contains(file))
                {
                    LOG.debug("@@@@@@@@@@ TotalCadTools.DwfToPDF - 13 - excluindo arquivo: " + file);
                    File.Delete(file);
                }
            }

            LOG.debug("@@@@@@@@ TotalCadTools.DwfToPDF - 14 - total final de arquivos: " + imgToConvert.Count);
            return imgToConvert;
        }
        */
    }
}
