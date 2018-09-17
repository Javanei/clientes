using System.IO;
using System.Collections.Generic;

namespace ACMECadTools.converter
{
    public class Converter
    {
        private string executablePath;

        public Converter(string executablePath)
        {
            this.executablePath = executablePath;
        }

        public List<string> DwfToJPG(string dwfFile, string imgTempfolder)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@ DwfToJPG - 1 - (dwfFile=" + dwfFile + ")");
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
            ;
            NeodentUtil.util.LOG.debug("@@@@@@@@ DwfToJPG - 2 - executablePath=" + executablePath);
            NeodentUtil.util.LOG.debug("@@@@@@@@ DwfToJPG - 3 - args=" + args);

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(executablePath, args)
            {
                ErrorDialog = false
            };
            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };

            NeodentUtil.util.LOG.debug("@@@@@@@@ DwfToJPG - 4 - Vai executar");
            process.Start();
            process.WaitForExit();
            NeodentUtil.util.LOG.debug("@@@@@@@@ DwfToJPG - 5 - exitCode=" + process.ExitCode);

            NeodentUtil.util.LOG.debug("@@@@@@@@ DwfToJPG - 6 - Executou");
            process.Dispose();

            string basedir = Directory.GetParent(dwfFile).FullName;
            string[] images = Directory.GetFiles(basedir);
            foreach (string f in images)
            {
                if (f.EndsWith(".jpg"))
                {
                    NeodentUtil.util.LOG.debug("@@@@@@@@@@ DwfToJPG - 7 - encontrado arquivo=" + f);
                    files.Add(f);
                }
            }
            NeodentUtil.util.LOG.debug("@@@@@@@@ DwfToJPG - 8 - arquivos: " + files.Count);
            files.Sort();

            // Pega a lista de imagens que precisam ser consideradas.
            Dictionary<string, string> fileProps = new Dictionary<string, string>();
            DWFTools.util.DWFUtil.Extract(imgTempfolder, dwfFile, fileProps);
            List<string> imgToConvert = new List<string>();
            foreach (var key in fileProps.Keys)
            {
                int line = int.Parse(key.ToString());
                if (line > 0)
                {
                    NeodentUtil.util.LOG.debug("@@@@@@@@@@ DwfToJPG - 9 - considerando arquivo: " + line + " -> " + images[line - 1]);
                    imgToConvert.Add(images[line - 1]);
                }
            }

            // Exclui as imagens nao necessarias
            foreach (string file in files)
            {
                if (!imgToConvert.Contains(file))
                {
                    NeodentUtil.util.LOG.debug("@@@@@@@@@@ DwfToJPG - 10 - excluindo arquivo: " + file);
                    File.Delete(file);
                }
            }

            NeodentUtil.util.LOG.debug("@@@@@@@@ DwfToJPG - 11 - total final de arquivos: " + imgToConvert.Count);
            return imgToConvert;
        }
    }
}
