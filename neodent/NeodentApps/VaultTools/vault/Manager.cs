using System.Collections.Generic;
using System;
using System.IO;

using DWFCore.dwf;

using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault
{
    public class Manager
    {
        private IDWFConverter dwfconverter;
        private string[] baseRepositories;
        private string[] sheetPrefix;
        private string server;
        private string vault;
        private string user;
        private string pass;
        private string tempfolder;
        private string confFile = "vaultsap.conf";

        private ADSKTools.WebServiceManager serviceManager = null;

        public Manager(IDWFConverter dwfconverter, string server, string[] baseRepositories, string[] sheetPrefix, string vault, string user, string pass, string tempfolder)
        {
            this.dwfconverter = dwfconverter;
            this.server = server;
            this.baseRepositories = baseRepositories;
            this.sheetPrefix = sheetPrefix;
            this.vault = vault;
            this.user = user;
            this.pass = pass;
            this.tempfolder = tempfolder;
            serviceManager = util.VaultUtil.Login(this.server, this.vault, this.user, this.pass);
        }

        public void Convert(string[] validExt, string[] sheetPrefixes, string storagefolder, string dwfconverterexecutable, string pdfconverterexecutable)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.Convert - 1");

            // Le o arquivo de configuração
            Dictionary<string, string> config = NeodentUtil.util.DictionaryUtil.ReadPropertyFile(confFile);
            NeodentUtil.util.DictionaryUtil.SetProperty(config, "ultimaExecucao", DateTime.Now.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));
            string checkInDate = NeodentUtil.util.DictionaryUtil.GetProperty(config, "LastCheckInDate");
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.Convert - 2 - checkInDate=" + checkInDate);

            List<ADSK.File> files;
            if (checkInDate == null || checkInDate == "")
            {
                files = util.FindAllInCheckin.Find(serviceManager, baseRepositories, validExt);
            }
            else
            {
                files = util.FindByCheckinDate.Find(serviceManager, baseRepositories, validExt, checkInDate);
            }
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.Convert - 3 - desenhos encontrados=" + files.Count);

            Convert(files, validExt, sheetPrefixes, storagefolder, dwfconverterexecutable, pdfconverterexecutable);
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.Convert - 4 - FIM");
        }

        public void ConvertByCheckinDate(string checkindate, string[] validExt, string[] sheetPrefixes, string storagefolder, string dwfconverterexecutable, string pdfconverterexecutable)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ConvertByCheckinDate - 1 - (checkindate=" + checkindate + ")");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByCheckinDate.Find(serviceManager, baseRepositories, validExt, checkindate);

            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ConvertByCheckinDate - 2 - desenhos encontrados=" + files.Count);
            Convert(files, validExt, sheetPrefixes, storagefolder, dwfconverterexecutable, pdfconverterexecutable);

            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ConvertByCheckinDate - 3 - FIM");
        }

        public void ConvertAllInCheckin(string[] validExt, string[] sheetPrefixes, string storagefolder, string dwfconverterexecutable, string pdfconverterexecutable)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ConvertAllInCheckin - 1");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindAllInCheckin.Find(serviceManager, baseRepositories, validExt);

            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ConvertAllInCheckin - 2 - desenhos encontrados=" + files.Count);
            Convert(files, validExt, sheetPrefixes, storagefolder, dwfconverterexecutable, pdfconverterexecutable);

            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ConvertAllInCheckin - 3 - FIM");
        }

        public void ConvertByFilename(string filename, string[] validExt, string[] sheetPrefixes, string storagefolder,
            string dwfconverterexecutable, string pdfconverterexecutable, bool ignorecheckout)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ConvertByFilename - 1 - (filename=" + filename + ", ignorecheckout=" + ignorecheckout + ")");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByFileNameEquals.FindByNameAndExtEquals(serviceManager, documentService,
                baseRepositories, filename, validExt, ignorecheckout);
            if (files.Count == 1)
            {
                ADSK.File file = files[0];
                string desenho = GetCode(file.Name, validExt);
                Convert(file, desenho, validExt, sheetPrefixes, storagefolder, dwfconverterexecutable, pdfconverterexecutable);

                NeodentUtil.util.LOG.debug("@@@@@@ Manager.ConvertByFilename - 2 - FIM");
            }
            else if (files.Count > 1)
            {
                throw new Exception("Mais de um desenho encontrado para o nome '" + filename + "'");
            }
            else
            {
                throw new Exception("Nenhum desenho encontrado para o nome '" + filename + "'");
            }
        }

        public void Close()
        {
            if (serviceManager != null)
            {
                serviceManager.Dispose();
                serviceManager = null;
            }
        }

        public void ShowInfoByName(string name)
        {
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByFileNameMatches.Find(serviceManager, documentService, baseRepositories, name);
            foreach (ADSK.File file in files)
            {
                Console.WriteLine("Arquivo: " + file.Name);
                Console.WriteLine(" - Id............=" + file.Id);
                Console.WriteLine(" - Hidden........=" + file.Hidden);
                Console.WriteLine(" - CheckedOut....=" + file.CheckedOut);
                Console.WriteLine(" - CkInDate......=" + file.CkInDate);
                Console.WriteLine(" - CkOutUserId...=" + file.CkOutUserId);
                Console.WriteLine(" - CreateDate....=" + file.CreateDate);
                Console.WriteLine(" - CreateUserId..=" + file.CreateUserId);
                Console.WriteLine(" - CreateUserName=" + file.CreateUserName);
                Console.WriteLine(" - ModDate.......=" + file.ModDate);
                Console.WriteLine(" - FileRev.......=" + file.FileRev);
                Console.WriteLine(" - FileSize......=" + file.FileSize);
                Console.WriteLine(" - FileStatus....=" + file.FileStatus);
                Console.WriteLine(" - VerName.......=" + file.VerName);
                Console.WriteLine(" - VerNum........=" + file.VerNum);
                Console.WriteLine("==============================================================");
            }
        }

        public void List(string[] validExt, string storagefolder)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.List - 1");

            // Le o arquivo de configuração
            Dictionary<string, string> config = NeodentUtil.util.DictionaryUtil.ReadPropertyFile(confFile);
            NeodentUtil.util.DictionaryUtil.SetProperty(config, "ultimaExecucao", DateTime.Now.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));
            string checkInDate = NeodentUtil.util.DictionaryUtil.GetProperty(config, "LastCheckInDate");
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.List - 2 - checkInDate=" + checkInDate);

            List<ADSK.File> files;
            if (checkInDate == null || checkInDate == "")
            {
                files = util.FindAllInCheckin.Find(serviceManager, baseRepositories, validExt);
            }
            else
            {
                files = util.FindByCheckinDate.Find(serviceManager, baseRepositories, validExt, checkInDate);
            }
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.List - 3 - desenhos encontrados=" + files.Count);

            List(files);
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.List - 4 - FIM");
        }

        public void ListByCheckinDate(string checkindate, string[] validExt, string storagefolder)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ListByCheckinDate - 1 - (checkindate=" + checkindate + ")");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByCheckinDate.Find(serviceManager, baseRepositories, validExt, checkindate);

            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ListByCheckinDate - 2 - desenhos encontrados=" + files.Count);
            List(files);
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ListByCheckinDate - 3 - FIM - " + files.Count);
        }

        public void ListtAllInCheckin(string[] validExt, string storagefolder)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ListtAllInCheckin - 1");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindAllInCheckin.Find(serviceManager, baseRepositories, validExt);

            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ListtAllInCheckin - 2 - desenhos encontrados=" + files.Count);
            List(files);

            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ListtAllInCheckin - 3 - FIM - " + files.Count);
        }

        public void ListAllCheckedOut(string[] validExt, string storagefolder)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ListAllCheckedOut - 1");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByCheckedOut.Find(serviceManager, baseRepositories, validExt);

            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ListAllCheckedOut - 2 - desenhos encontrados=" + files.Count);
            List(files);

            NeodentUtil.util.LOG.debug("@@@@@@ Manager.ListAllCheckedOut - 3 - FIM - " + files.Count);
        }

        private void List(List<ADSK.File> files)
        {
            foreach (ADSK.File file in files)
            {
                NeodentUtil.util.LOG.debug("@@@@@@@@@@ Manager.List - file: Name=" + file.Name
                    + ", CkInDate=" + file.CkInDate
                    + ", Cksum=" + file.Cksum
                    + ", FileSize=" + file.FileSize
                    + ", FileStatus=" + file.FileStatus
                    + ", ModDate=" + file.ModDate
                    + ", CheckedOut=" + file.CheckedOut
                    + ", CkOutUserId=" + file.CkOutUserId
                    //+ ", CkOutMach=" + file.CkOutMach
                    //+ ", CkOutSpec=" + file.CkOutSpec
                    );
            }
        }

        // ==================

        private void Convert(List<ADSK.File> files, string[] validExt, string[] sheetPrefixes, string storagefolder, string dwfconverterexecutable, string pdfconverterexecutable)
        {
            Dictionary<string, string> config = NeodentUtil.util.DictionaryUtil.ReadPropertyFile(confFile);
            NeodentUtil.util.DictionaryUtil.SetProperty(config, "ultimaExecucao", DateTime.Now.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));

            int contador = 0;
            foreach (ADSK.File file in files)
            {
                Dictionary<string, string> fileInfo = new Dictionary<string, string>();
                string desenho = GetCode(file.Name, validExt);
                //string cfgFile = storagefolder + "\\" + desenho + ".cfg";

                contador++;
                NeodentUtil.util.LOG.debug("===================== Vai converter arquivo [" + contador + "] de [" + files.Count + "] - " + file.Name);
                bool converteu = Convert(file, desenho, validExt, sheetPrefixes, storagefolder, dwfconverterexecutable, pdfconverterexecutable);
                NeodentUtil.util.LOG.debug("===================== Converteu " + file.Name + "? " + converteu);

                /*
                if (converteu)
                {
                    //TODO: Precisa mesmo salvar as configurações?
                    SaveFileInfo(file, fileInfo, cfgFile);
                } else
                {
                    if (File.Exists(cfgFile))
                    {
                        File.Delete(cfgFile);
                    }
                }
                */
                NeodentUtil.util.DictionaryUtil.SetProperty(config, "LastCheckInDate", file.CkInDate.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss"));
                NeodentUtil.util.DictionaryUtil.WritePropertyFile(confFile, config);
            }
        }

        private bool Convert(ADSK.File file, string desenho, string[] validExt, string[] sheetPrefixes, string storagefolder, string dwfconverterexecutable, string pdfconverterexecutable)
        {
            bool result = false;
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ Manager.Convert - 1 - Name=" + file.Name + ", CkInDate=" + file.CkInDate
                + ", CheckedOut=" + file.CheckedOut + ", CkOutUserId=" + file.CkOutUserId);
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ Manager.Convert - 2 - desenho=" + desenho);

            // Usa um diretorio por imagem para evitar problemas em deletar os arquivos
            string imgTempfolder = tempfolder + "\\" + desenho.Trim();
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ Manager.Convert - 3 - imgTempfolder=" + imgTempfolder);

            // Por garantia, limpa o diretorio temporario
            ClearDirectory(imgTempfolder);

            // Cria o diretório temporario da imagem
            if (!Directory.Exists(imgTempfolder))
            {
                Directory.CreateDirectory(imgTempfolder);
            }

            // Faz o download do arquivo
            DownloadFast downloadFast = new DownloadFast(serviceManager, user, pass, vault,
                serviceManager.AdminService.SecurityHeader.UserId, server);
            downloadFast.DownloadFile(file, imgTempfolder);
            string downFile = imgTempfolder + "\\" + file.Name;
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ Manager.Convert - 4 - Download efetuado - " + downFile + " - " + File.Exists(downFile));
            File.SetAttributes(downFile, FileAttributes.Normal);

            // Enfim, converte as imagens
            List<string> images = null;
            if (file.Name.EndsWith(".dwf"))
            {
                //ACMECadTools.converter.Converter dwfconverter = new ACMECadTools.converter.Converter(dwfconverterexecutable);
                //images = dwfconverter.DwfToJPG(imgTempfolder + "\\" + file.Name, imgTempfolder);
                // Converte para PDF
                images = dwfconverter.DwfToPDF(imgTempfolder + "\\" + file.Name, imgTempfolder, sheetPrefixes);
                NeodentUtil.util.LOG.debug("@@@@@@@@@@ Manager.Convert - 5 - imagens (DWF) para mergear: " + images.Count);
            }
            else if (file.Name.EndsWith(".pdf"))
            {
                // Apenas copia para a pasta final
                images = new List<string>
                {
                    imgTempfolder + "\\" + file.Name
                };
                /*
                GSTools.converter.Converter pdfconverter = new GSTools.converter.Converter(pdfconverterexecutable);
                images = pdfconverter.PDFToJPG(imgTempfolder + "\\" + file.Name, imgTempfolder);
                */
                NeodentUtil.util.LOG.debug("@@@@@@@@@@ Manager.Convert - 6 - imagens (PDF) para mergear: " + images.Count);
            }

            // Mergeia as imagens
            if (images.Count > 0)
            {
                /*
                string destFile = storagefolder + "\\" + desenho + ".jpg";
                NeodentUtil.util.ImageUtil.MergeImageList(images.ToArray(), destFile);
                NeodentUtil.util.LOG.debug("@@@@@@@@@@@@ Manager.Convert - 7 - merge realizado: " + destFile);
                */
                string destFile = storagefolder + "\\" + desenho + ".pdf";
                GSTools.converter.Converter merger = new GSTools.converter.Converter(pdfconverterexecutable);
                merger.MergePDFs(destFile, images);
                result = true;
                File.SetAttributes(destFile, FileAttributes.Normal);
            }

            // Limpa o diretorio temporario
            ClearDirectory(imgTempfolder);
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ Manager.Convert - 8 - FIM");
            return result;
        }

        private string GetCode(string filename, string[] validExt)
        {
            string code = filename;
            foreach (string ext in validExt)
            {
                if (filename.EndsWith(ext))
                {
                    code = filename.Substring(0, filename.Length - ext.Length);
                    break;
                }
            }
            return code;
        }

        private static void ClearDirectory(string dir)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@@@@@@@ Manager.ClearDirectory - 1 - (" + dir + ")");
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                    NeodentUtil.util.LOG.debug("@@@@@@@@@@@@@@ Manager.ClearDirectory - 2 - (" + dir + ") - OK");
                }
                catch (Exception ex)
                {
                    NeodentUtil.util.LOG.debug("@@@@@@@@@@@@@@ Manager.ClearDirectory - 3 - (" + dir + ") - ERRO: " + ex.Message);
                }
            }
        }

        private void SaveFileInfo(ADSK.File file, Dictionary<string, string> d, string cfgFile)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@@@@@ Manager.SaveFileInfo - 1 - Name=" + file.Name + ", cfgFile=" + cfgFile);

            NeodentUtil.util.DictionaryUtil.SetProperty(d, "File", file.Name);
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "CheckedOut", file.CheckedOut.ToString());
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "CkInDate", file.CkInDate.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss"));
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "Cksum", file.Cksum.ToString());
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "FileSize", file.FileSize.ToString());
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "FileStatus", file.FileStatus.ToString());
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "ModDate", file.ModDate.ToString());
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "CkOutMach", file.CkOutMach);
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "CkOutSpec", file.CkOutSpec);
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "CkOutUserId", file.CkOutUserId.ToString());

            NeodentUtil.util.DictionaryUtil.WritePropertyFile(cfgFile, d);
        }

        public void TmpListPropertyDefinition()
        {
            ADSK.PropDef[] defs = util.VaultUtil.ListPropertyDefinition(serviceManager);
            Console.WriteLine("******* Props *************");
            foreach (ADSK.PropDef prop in defs)
            {
                Console.WriteLine(prop.Id + ", SysName=" + prop.SysName + ", DispName=" + prop.DispName + ", DataType=" + prop.Typ);
            }
            Console.WriteLine("***************************");
        }
    }
}
