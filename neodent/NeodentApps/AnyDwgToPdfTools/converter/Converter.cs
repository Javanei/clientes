using System.IO;
using System.Collections.Generic;

namespace AnyDwgToPdfTools.converter
{
    public class Converter
    {
        private string executablePath;

        public Converter(string executablePath)
        {
            this.executablePath = executablePath;
        }

        public List<string> DwfToPDF(string dwfFile, string imgTempfolder, string[] sheetPrefixes)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@ AnyDwgToPdfTools.DwfToPDF - 1 - (dwfFile=" + dwfFile + ")");
            List<string> files = new List<string>();

            string args = "/InFile \"" + dwfFile + "\""
                + " /OutFolder \"" + imgTempfolder + "\""
                + " /ConvertType DWF2PDF"
                + " /PDFColor TrueColors"
                + " /PDFQuality Highest"
                + " /OutMode ByLayout"
                + " /Overwrite"
                //+ " /Outlayout:"
                + " /LineWidth Default"
                + " /LineScale 1" //Testar valores
                                  //+ " /DisablePDFBookmark"
                                  //+ " /DisableFillTTF"
                ;

            NeodentUtil.util.LOG.debug("@@@@@@@@ AnyDwgToPdfTools.DwfToPDF - 2 - executablePath=" + executablePath);
            NeodentUtil.util.LOG.debug("@@@@@@@@ AnyDwgToPdfTools.DwfToPDF - 3 - args=" + args);

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(executablePath, args)
            {
                ErrorDialog = false
            };
            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };

            NeodentUtil.util.LOG.debug("@@@@@@@@ AnyDwgToPdfTools.DwfToPDF - 4 - Vai executar");
            process.Start();
            process.WaitForExit();
            NeodentUtil.util.LOG.debug("@@@@@@@@ AnyDwgToPdfTools.DwfToPDF - 5 - exitCode=" + process.ExitCode);

            NeodentUtil.util.LOG.debug("@@@@@@@@ AnyDwgToPdfTools.DwfToPDF - 6 - Executou");
            process.Dispose();

            string basedir = Directory.GetParent(dwfFile).FullName;
            string[] images = Directory.GetFiles(basedir);
            foreach (string f in images)
            {
                if (f.EndsWith(".pdf"))
                {
                    NeodentUtil.util.LOG.debug("@@@@@@@@@@ AnyDwgToPdfTools.DwfToPDF - 7 - encontrado arquivo=" + f);
                    files.Add(f);
                }
            }
            NeodentUtil.util.LOG.debug("@@@@@@@@ AnyDwgToPdfTools.DwfToPDF - 8 - arquivos: " + files.Count);
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
                    NeodentUtil.util.LOG.debug("@@@@@@@@@@ AnyDwgToPdfTools.DwfToPDF - 9 - considerando arquivo: " + line + " -> " + images[line - 1]);
                    imgToConvert.Add(images[line - 1]);
                }
            }

            // Exclui as imagens nao necessarias
            foreach (string file in files)
            {
                if (!imgToConvert.Contains(file))
                {
                    NeodentUtil.util.LOG.debug("@@@@@@@@@@ AnyDwgToPdfTools.DwfToPDF - 10 - excluindo arquivo: " + file);
                    File.Delete(file);
                }
            }

            NeodentUtil.util.LOG.debug("@@@@@@@@ AnyDwgToPdfTools.DwfToPDF - 11 - total final de arquivos: " + imgToConvert.Count);
            return imgToConvert;
        }
    }
}
