using System.Collections.Generic;
using System;
using System.IO;

using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault
{
    public class Manager
    {
        private string[] baseRepositories;
        private string server;
        private string vault;
        private string user;
        private string pass;
        private string tempfolder;
        private string confFile = "vaultsap.conf";

        private ADSKTools.WebServiceManager serviceManager = null;

        public Manager(string server, string[] baseRepositories, string vault, string user, string pass, string tempfolder)
        {
            this.server = server;
            this.baseRepositories = baseRepositories;
            this.vault = vault;
            this.user = user;
            this.pass = pass;
            this.tempfolder = tempfolder;
            serviceManager = util.VaultUtil.Login(this.server, this.vault, this.user, this.pass);
        }

        public void Convert(string[] validExt, string storagefolder, string converterexecutable)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ Convert - 1");

            // Le o arquivo de configuração
            Dictionary<string, string> config = NeodentUtil.util.DictionaryUtil.ReadPropertyFile(confFile);
            NeodentUtil.util.DictionaryUtil.SetProperty(config, "ultimaExecucao", DateTime.Now.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));
            string checkInDate = NeodentUtil.util.DictionaryUtil.GetProperty(config, "LastCheckInDate");
            if (checkInDate == null || checkInDate == "")
            {
                checkInDate = new DateTime(
                    1980, //agora.Year
                    1, //agora.Month
                    1, //agora.Day
                    0, //agora.Hour
                    0, //agora.Minute
                    0, //agora.Second
                    0 //agora.Millisecond
                    ).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss");
            }
            NeodentUtil.util.LOG.debug("@@@@@@ Convert - 2 - checkInDate=" + checkInDate);

            NeodentUtil.util.DictionaryUtil.SetProperty(config, "LastCheckInDate", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));

            List<ADSK.File> files = util.FindByCheckinDate.Find(serviceManager, baseRepositories, validExt, checkInDate);
            NeodentUtil.util.LOG.debug("@@@@@@ Convert - 3 - desenhos encontrados=" + files.Count);

            Convert(files, validExt, storagefolder, converterexecutable);
            NeodentUtil.util.LOG.debug("@@@@@@ Convert - 4 - FIM");
        }

        public void ConvertByCheckinDate(string checkindate, string[] validExt, string storagefolder, string converterexecutable)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ ConvertByCheckinDate - 1 - (checkindate=" + checkindate + ")");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByCheckinDate.Find(serviceManager, baseRepositories, validExt, checkindate);

            NeodentUtil.util.LOG.debug("@@@@@@ ConvertByCheckinDate - 2 - desenhos encontrados=" + files.Count);
            Convert(files, validExt, storagefolder, converterexecutable);

            NeodentUtil.util.LOG.debug("@@@@@@ ConvertByCheckinDate - 3 - FIM");
        }

        public void ConvertAllInCheckin(string[] validExt, string storagefolder, string converterexecutable)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ ConvertAllInCheckin - 1");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindAllInCheckin.Find(serviceManager, baseRepositories, validExt);

            NeodentUtil.util.LOG.debug("@@@@@@ ConvertAllInCheckin - 2 - desenhos encontrados=" + files.Count);
            Convert(files, validExt, storagefolder, converterexecutable);

            NeodentUtil.util.LOG.debug("@@@@@@ ConvertAllInCheckin - 3 - FIM");
        }

        public void ConvertByFilename(string filename, string[] validExt, string storagefolder, string converterexecutable)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ ConvertByFilename - 1 - (filename=" + filename + ")");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByFileNameEquals.FindByNameAndExtEquals(serviceManager, documentService,
                baseRepositories, filename, validExt);
            if (files.Count == 1)
            {
                ADSK.File file = files[0];
                string desenho = GetCode(file.Name, validExt);
                Convert(file, desenho, validExt, storagefolder, converterexecutable);

                NeodentUtil.util.LOG.debug("@@@@@@ ConvertByFilename - 2 - FIM");
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

        /*
         * Testes
         */
        public void List(string[] validExt, string storagefolder, string converterexecutable)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ List - 1");

            // Le o arquivo de configuração
            Dictionary<string, string> config = NeodentUtil.util.DictionaryUtil.ReadPropertyFile(confFile);
            NeodentUtil.util.DictionaryUtil.SetProperty(config, "ultimaExecucao", DateTime.Now.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));
            string checkInDate = NeodentUtil.util.DictionaryUtil.GetProperty(config, "LastCheckInDate");
            if (checkInDate == null || checkInDate == "")
            {
                checkInDate = new DateTime(
                    1980, //agora.Year
                    1, //agora.Month
                    1, //agora.Day
                    0, //agora.Hour
                    0, //agora.Minute
                    0, //agora.Second
                    0 //agora.Millisecond
                    ).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss");
            }
            NeodentUtil.util.LOG.debug("@@@@@@ List - 2 - checkInDate=" + checkInDate);

            List<ADSK.File> files = util.FindByCheckinDate.Find(serviceManager, baseRepositories, validExt, checkInDate);
            NeodentUtil.util.LOG.debug("@@@@@@ List - 3 - desenhos encontrados=" + files.Count);

            List(files, validExt, storagefolder, converterexecutable);
            NeodentUtil.util.LOG.debug("@@@@@@ List - 4 - FIM");
        }

        public void ListByCheckinDate(string checkindate, string[] validExt, string storagefolder, string converterexecutable)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ ListByCheckinDate - 1 - (checkindate=" + checkindate + ")");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByCheckinDate.Find(serviceManager, baseRepositories, validExt, checkindate);

            NeodentUtil.util.LOG.debug("@@@@@@ ListByCheckinDate - 2 - desenhos encontrados=" + files.Count);
            List(files, validExt, storagefolder, converterexecutable);

            NeodentUtil.util.LOG.debug("@@@@@@ ListByCheckinDate - 3 - FIM");
        }

        public void ListtAllInCheckin(string[] validExt, string storagefolder, string converterexecutable)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ ListtAllInCheckin - 1");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindAllInCheckin.Find(serviceManager, baseRepositories, validExt);

            NeodentUtil.util.LOG.debug("@@@@@@ ListtAllInCheckin - 2 - desenhos encontrados=" + files.Count);
            List(files, validExt, storagefolder, converterexecutable);

            NeodentUtil.util.LOG.debug("@@@@@@ ListtAllInCheckin - 3 - FIM");
        }

        private void List(List<ADSK.File> files, string[] validExt, string storagefolder, string converterexecutable)
        {
            foreach (ADSK.File file in files)
            {
                NeodentUtil.util.LOG.debug("@@@@@@@@@@ file: Name=" + file.Name
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

        private void Convert(List<ADSK.File> files, string[] validExt, string storagefolder, string converterexecutable)
        {
            Dictionary<string, string> config = NeodentUtil.util.DictionaryUtil.ReadPropertyFile(confFile);
            NeodentUtil.util.DictionaryUtil.SetProperty(config, "ultimaExecucao", DateTime.Now.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));

            foreach (ADSK.File file in files)
            {
                Dictionary<string, string> fileInfo = new Dictionary<string, string>();
                string desenho = GetCode(file.Name, validExt);
                string cfgFile = storagefolder + "\\" + desenho + ".cfg";

                Convert(file, desenho, validExt, storagefolder, converterexecutable);

                NeodentUtil.util.DictionaryUtil.SetProperty(config, "LastCheckInDate", file.CkInDate.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss"));
                NeodentUtil.util.DictionaryUtil.WritePropertyFile(confFile, config);

                //TODO: Precisa mesmo salvar as configurações?
                SaveFileInfo(file, fileInfo, cfgFile);
            }
        }

        private void Convert(ADSK.File file, string desenho, string[] validExt, string storagefolder, string converterexecutable)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ Convert - 1 - Name=" + file.Name + ", CkInDate=" + file.CkInDate);
            ACMECadTools.converter.Converter converter = new ACMECadTools.converter.Converter(converterexecutable);
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ Convert - 2 - desenho=" + desenho);

            // Usa um diretorio por imagem para evitar problemas em deletar os arquivos
            string imgTempfolder = tempfolder + "\\" + desenho;
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ Convert - 3 - imgTempfolder=" + imgTempfolder);

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
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ Convert - 4 - Download efetuado");

            // Enfim, converte as imagens
            List<string> images = null;
            if (file.Name.EndsWith(".dwf"))
            {
                images = converter.DwfToJPG(imgTempfolder + "\\" + file.Name, imgTempfolder);
                NeodentUtil.util.LOG.debug("@@@@@@@@@@ Convert - 5 - imagens para mergear: " + images.Count);
            }
            else if (file.Name.EndsWith(".pdf"))
            {
                //TODO: Implementar
                throw new Exception("Conversao de PDF ainda nao implementado");
            }

            // Mergeia as imagens
            if (images.Count > 0)
            {
                string destFile = storagefolder + "\\" + desenho + ".jpg";
                NeodentUtil.util.ImageUtil.MergeImageList(images.ToArray(), destFile);
                NeodentUtil.util.LOG.debug("@@@@@@@@@@@@ Convert - 6 - merge realizado: " + destFile);
            }

            // Limpa o diretorio temporario
            ClearDirectory(imgTempfolder);
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ Convert - 7 - FIM");
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
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ ClearDirectory - 1 - (" + dir + ")");
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                    NeodentUtil.util.LOG.debug("@@@@@@@@@@ ClearDirectory - 2 - (" + dir + ") - OK");
                }
                catch (IOException ex)
                {
                    NeodentUtil.util.LOG.debug("@@@@@@@@@@ ClearDirectory - 3 - (" + dir + ") - ERRO: " + ex.Message);
                }
            }
        }

        private void SaveFileInfo(ADSK.File file, Dictionary<string, string> d, string cfgFile)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@@@@@ SaveFileInfo - 1 - Name=" + file.Name + ", cfgFile=" + cfgFile);

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
