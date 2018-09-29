using System.IO;
using System.Collections.Generic;
using DWFCore.dwf;
using NeodentUtil.util;

namespace ACMECadTools.converter
{
    public class Converter : IDWFConverter
    {
        private string executablePath;

        public Converter(string executablePath)
        {
            this.executablePath = executablePath;
        }

        public List<string> DwfToPDF(string dwfFile, string imgTempfolder, string[] sheetPrefixes)
        {
            LOG.debug("@@@@@@@@ ACMECadTools.DwfToPDF - 1 - (dwfFile=" + dwfFile + ")");
            List<string> files = new List<string>();

            //AcmeCADConverter.exe /r /ls /ad /res 400 /f 109 /a -2
            string args = "/r" //run on command line mode
                + " /ls" //Uses layout paper size if possible
                + " /ad" //detects and fits the current page size for the converted drawing
                + " /res 600" // 600 DPI
                + " /f 109" // integer Raster file format -> 109 = Um PDF por layout
                            //+ " /w 5950 /h 8410" -> A4?
                + " /a -2" //Layout Index is a interger number, -2 = todos
                + " \"" + dwfFile + "\"";

            LOG.debug("@@@@@@@@ ACMECadTools.DwfToPDF - 2 - executablePath=" + executablePath);
            LOG.debug("@@@@@@@@ ACMECadTools.DwfToPDF - 3 - args=" + args);

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(executablePath, args)
            {
                ErrorDialog = false
            };
            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };

            LOG.debug("@@@@@@@@ ACMECadTools.DwfToPDF - 4 - Vai executar");
            process.Start();
            process.WaitForExit();
            int exitCode = process.ExitCode;
            LOG.debug("@@@@@@@@ ACMECadTools.DwfToPDF - 5 - exitCode=" + exitCode);

            LOG.debug("@@@@@@@@ ACMECadTools.DwfToPDF - 6 - Executou");
            process.Dispose();
            if (exitCode > 0)
            {
                throw new System.Exception("Não foi possivel converter o arquivo usando o ACMECadConverter, exitCode=" + exitCode);
            }

            string basedir = Directory.GetParent(dwfFile).FullName;
            string[] images = Directory.GetFiles(basedir);
            foreach (string f in images)
            {
                if (f.EndsWith(".pdf"))
                {
                    LOG.debug("@@@@@@@@@@ ACMECadTools.DwfToPDF - 7 - encontrado arquivo=" + f);
                    // Tenta filtrar os arquivos incorretas pelo tamanho
                    FileInfo fi = new FileInfo(f);
                    if (fi.Length > 10000)
                    {
                        LOG.debug("@@@@@@@@@@ ACMECadTools.DwfToPDF - 8 - Ignorando arquivo=" + f);
                    }
                    else
                    {
                        LOG.debug("@@@@@@@@@@ ACMECadTools.DwfToPDF - 9 - Considerando arquivo=" + f);
                        files.Add(f);
                    }
                }
            }
            LOG.debug("@@@@@@@@ ACMECadTools.DwfToPDF - 10 - arquivos: " + files.Count);
            files.Sort();

            // Pega a lista de imagens que precisam ser consideradas.
            Dictionary<string, string> fileProps = new Dictionary<string, string>();
            DWFTools.util.DWFUtil.Extract(imgTempfolder, dwfFile, fileProps, sheetPrefixes);
            List<string> imgToConvert = new List<string>();
            foreach (var key in fileProps.Keys)
            {
                int line = int.Parse(key.ToString());
                if (line > 0)
                {
                    LOG.debug("@@@@@@@@@@ ACMECadTools.DwfToPDF - 9 - considerando arquivo: " + line + " -> " + images[line - 1]);
                    imgToConvert.Add(images[line - 1]);
                }
            }

            // Exclui as imagens nao necessarias
            foreach (string file in files)
            {
                if (!imgToConvert.Contains(file))
                {
                    LOG.debug("@@@@@@@@@@ ACMECadTools.DwfToPDF - 10 - excluindo arquivo: " + file);
                    File.Delete(file);
                }
            }

            LOG.debug("@@@@@@@@ ACMECadTools.DwfToPDF - 11 - total final de arquivos: " + imgToConvert.Count);
            return imgToConvert;
        }

        /*
        public List<string> DwfToJPG(string dwfFile, string imgTempfolder, string[] sheetPrefixes)
        {
            LOG.debug("@@@@@@@@ ACMECadTools.DwfToJPG - 1 - (dwfFile=" + dwfFile + ") - Existe? " + File.Exists(dwfFile));
            List<string> files = new List<string>();

            string args = "/r" //run on command line mode
                + " /ls" //Uses layout paper size if possible
                + " /ad" //detects and fits the current page size for the converted drawing
                + " /w 1200" // integer Raster width, default is 800.it also supports the unit flag, such as (420mm = 420 mm, 11in = 11 inch), if you do not specify the unit flag, the unit will be pixels default. 
                + " /h 800" //integer Indicate the raster height, default is 600, the unit flag is the same as the '/w' parameter.
                + " /j 10" //integer Indicate the jpeg quality, 1-lower, 10-highest, default is 10
                + " /f 2" // integer Raster file format -> 2 = JPG
                + " /a -2" //Layout Index is a interger number, -2 = todos
                           //+ " /d \"D:\tmp\\cad\\convertido\"" Não funciona!
                + " \"" + dwfFile + "\"";
            
            LOG.debug("@@@@@@@@ ACMECadTools.DwfToJPG - 2 - executablePath=" + executablePath);
            LOG.debug("@@@@@@@@ ACMECadTools.DwfToJPG - 3 - args=" + args);

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(executablePath, args)
            {
                ErrorDialog = false
            };
            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };

            LOG.debug("@@@@@@@@ ACMECadTools.DwfToJPG - 4 - Vai executar");
            process.Start();
            process.WaitForExit();
            LOG.debug("@@@@@@@@ ACMECadTools.DwfToJPG - 5 - exitCode=" + process.ExitCode);

            LOG.debug("@@@@@@@@ ACMECadTools.DwfToJPG - 6 - Executou");
            process.Dispose();

            string basedir = Directory.GetParent(dwfFile).FullName;
            string[] images = Directory.GetFiles(basedir);
            foreach (string f in images)
            {
                if (f.EndsWith(".jpg"))
                {
                    LOG.debug("@@@@@@@@@@ ACMECadTools.DwfToJPG - 7 - encontrado arquivo=" + f);
                    files.Add(f);
                }
            }
            LOG.debug("@@@@@@@@ ACMECadTools.DwfToJPG - 8 - arquivos: " + files.Count);
            files.Sort();

            // Pega a lista de imagens que precisam ser consideradas.
            Dictionary<string, string> fileProps = new Dictionary<string, string>();
            DWFTools.util.DWFUtil.Extract(imgTempfolder, dwfFile, fileProps, sheetPrefixes);
            List<string> imgToConvert = new List<string>();
            foreach (var key in fileProps.Keys)
            {
                int line = int.Parse(key.ToString());
                if (line > 0)
                {
                    LOG.debug("@@@@@@@@@@ ACMECadTools.DwfToJPG - 9 - considerando arquivo: " + line + " -> " + images[line - 1]);
                    imgToConvert.Add(images[line - 1]);
                }
            }

            // Exclui as imagens nao necessarias
            foreach (string file in files)
            {
                if (!imgToConvert.Contains(file))
                {
                    LOG.debug("@@@@@@@@@@ ACMECadTools.DwfToJPG - 10 - excluindo arquivo: " + file);
                    File.Delete(file);
                }
            }

            LOG.debug("@@@@@@@@ ACMECadTools.DwfToJPG - 11 - total final de arquivos: " + imgToConvert.Count);
            return imgToConvert;
        }
        */
    }
}
