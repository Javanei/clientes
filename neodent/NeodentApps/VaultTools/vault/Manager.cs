using System.Collections.Generic;
using System;
using System.IO;

using DWFCore.dwf;
using NeodentUtil.util;

using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault
{
    public class Manager
    {
        private static readonly string ErrorList = "vaultsap.err.conf";

        private IDWFConverter dwfconverter;
        private string pdfconverterexecutable;
        private string[] baseRepositories;
        private string[] sheetPrefix;
        private string server;
        private string vault;
        private string user;
        private string pass;
        private string tempfolder;
        private string confFile = "vaultsap.conf";

        private ADSKTools.WebServiceManager serviceManager = null;

        public Manager(IDWFConverter dwfconverter,
            string pdfconverterexecutable,
            string[] baseRepositories,
            string[] sheetPrefix,
            string tempfolder)
        {
            this.dwfconverter = dwfconverter;
            this.pdfconverterexecutable = pdfconverterexecutable;
            this.baseRepositories = baseRepositories;
            this.sheetPrefix = sheetPrefix;
            this.tempfolder = tempfolder;
        }

        public Manager(IDWFConverter dwfconverter,
            string pdfconverterexecutable,
            string server,
            string[] baseRepositories,
            string[] sheetPrefix,
            string vault,
            string user,
            string pass,
            string tempfolder)
        {
            this.dwfconverter = dwfconverter;
            this.pdfconverterexecutable = pdfconverterexecutable;
            this.server = server;
            this.baseRepositories = baseRepositories;
            this.sheetPrefix = sheetPrefix;
            this.vault = vault;
            this.user = user;
            this.pass = pass;
            this.tempfolder = tempfolder;
            serviceManager = util.VaultUtil.Login(this.server, this.vault, this.user, this.pass);
        }

        public void Convert(string[,] validExts,
            string[] sheetPrefixes,
            string storagefolder,
            bool preservetemp,
            bool ignorecheckout)
        {
            LOG.info("Manager.Convert(preservetemp=" + preservetemp + ", ignorecheckout=" + ignorecheckout + ")");

            Dictionary<string, string> config = DictionaryUtil.ReadPropertyFile(confFile);
            string checkInDate = DictionaryUtil.GetProperty(config, "LastCheckInDate");

            if (checkInDate == null || checkInDate == "")
            {
                LOG.debug("@@@@@@ Manager.Convert - 2 - Convertendo tudo");
                ConvertAllInCheckin(validExts, sheetPrefixes, storagefolder, preservetemp, ignorecheckout);
            }
            else
            {
                LOG.debug("@@@@@@ Manager.Convert - 3 - Convertendo por data de checkin: " + checkInDate);
                ConvertByCheckinDate(checkInDate, validExts, sheetPrefix, storagefolder, preservetemp, ignorecheckout);
            }

            LOG.debug("@@@@@@ Manager.Convert - 4 - FIM");
        }

        public void ConvertByCheckinDate(string checkindate, string[,] validExts, string[] sheetPrefixes, string storagefolder,
            bool preservetemp, bool ignorecheckout)
        {
            LOG.info("Manager.ConvertByCheckinDate(checkindate=" + checkindate + ", preservetemp=" + preservetemp + ")");

            // Reprocessa os desenhos anteriores
            ProcessErrorsFromPreviousConvertion(validExts, sheetPrefixes, storagefolder, ignorecheckout, preservetemp);

            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByCheckinDate.Find(serviceManager, baseRepositories, validExts, checkindate, ignorecheckout);

            LOG.debug("@@@@@@ Manager.ConvertByCheckinDate - 2 - desenhos encontrados=" + files.Count);
            Convert(files, validExts, sheetPrefixes, storagefolder, preservetemp);

            // Reprocessa os desenhos que deram erro
            ProcessErrorsFromPreviousConvertion(validExts, sheetPrefixes, storagefolder, ignorecheckout, preservetemp);

            LOG.debug("@@@@@@ Manager.ConvertByCheckinDate - 3 - FIM");
        }

        public void ConvertAllInCheckin(string[,] validExts, string[] sheetPrefixes, string storagefolder,
            bool preservetemp, bool ignorecheckout)
        {
            LOG.debug("@@@@@@ Manager.ConvertAllInCheckin - 1");

            // Reprocessa os desenhos anteriores
            ProcessErrorsFromPreviousConvertion(validExts, sheetPrefixes, storagefolder, ignorecheckout, preservetemp);

            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindAllInCheckin.Find(serviceManager, baseRepositories, validExts, false);

            LOG.debug("@@@@@@ Manager.ConvertAllInCheckin - 2 - desenhos encontrados=" + files.Count);
            Convert(files, validExts, sheetPrefixes, storagefolder, preservetemp);

            // Reprocessa os desenhos que deram erro
            ProcessErrorsFromPreviousConvertion(validExts, sheetPrefixes, storagefolder, ignorecheckout, preservetemp);

            LOG.debug("@@@@@@ Manager.ConvertAllInCheckin - 3 - FIM");
        }

        public bool ConvertByFilename(string filename, string[,] validExts, string[] sheetPrefixes, string storagefolder,
            bool ignorecheckout, bool preservetemp)
        {
            bool result = false;
            LOG.info("Manager.ConvertByFilename(filename=" + filename + ", ignorecheckout=" + ignorecheckout + ")");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByFileNameEquals.FindByNameAndExtEquals(serviceManager, documentService,
                baseRepositories, filename, validExts, ignorecheckout);
            if (files.Count == 1)
            {
                ADSK.File file = files[0];
                string desenho = GetCode(file.Name, validExts);
                result = Convert(file, desenho, validExts, sheetPrefixes, storagefolder, preservetemp);

                LOG.debug("@@@@@@ Manager.ConvertByFilename - 2 - FIM");
            }
            else if (files.Count > 1)
            {
                throw new Exception("Mais de um desenho encontrado para o nome '" + filename + "'");
            }
            else
            {
                throw new Exception("Nenhum desenho encontrado para o nome '" + filename + "'");
            }
            return result;
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
            LOG.info("Manager.ShowInfoByName(name=" + name + ")");
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

        public void List(string[,] validExts, bool ignorecheckout)
        {
            LOG.info("Manager.List()");

            // Le o arquivo de configuração
            Dictionary<string, string> config = DictionaryUtil.ReadPropertyFile(confFile);
            DictionaryUtil.SetProperty(config, "ultimaExecucao", DateTime.Now.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));
            string checkInDate = DictionaryUtil.GetProperty(config, "LastCheckInDate");
            LOG.debug("@@@@@@ Manager.List - 2 - checkInDate=" + checkInDate);

            List<ADSK.File> files;
            if (checkInDate == null || checkInDate == "")
            {
                files = util.FindAllInCheckin.Find(serviceManager, baseRepositories, validExts, ignorecheckout);
            }
            else
            {
                files = util.FindByCheckinDate.Find(serviceManager, baseRepositories, validExts, checkInDate, ignorecheckout);
            }
            LOG.debug("@@@@@@ Manager.List - 3 - desenhos encontrados=" + files.Count);

            List(files);
            LOG.debug("@@@@@@ Manager.List - 4 - FIM");
        }

        public void ListByCheckinDate(string checkindate, string[,] validExts, bool ignorecheckout)
        {
            LOG.info("Manager.ListByCheckinDate(checkindate=" + checkindate + ")");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByCheckinDate.Find(serviceManager, baseRepositories, validExts, checkindate, ignorecheckout);

            LOG.debug("@@@@@@ Manager.ListByCheckinDate - 2 - desenhos encontrados=" + files.Count);
            List(files);
            LOG.debug("@@@@@@ Manager.ListByCheckinDate - 3 - FIM - " + files.Count);
        }

        public void ListtAllInCheckin(string[,] validExts, bool ignorecheckout)
        {
            LOG.info("Manager.ListtAllInCheckin()");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindAllInCheckin.Find(serviceManager, baseRepositories, validExts, ignorecheckout);

            LOG.debug("@@@@@@ Manager.ListtAllInCheckin - 2 - desenhos encontrados=" + files.Count);
            List(files);

            LOG.debug("@@@@@@ Manager.ListtAllInCheckin - 3 - FIM - " + files.Count);
        }

        public void ListAllCheckedOut(string[,] validExts)
        {
            LOG.info("Manager.ListAllCheckedOut()");
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            List<ADSK.File> files = util.FindByCheckedOut.Find(serviceManager, baseRepositories, validExts);

            LOG.debug("@@@@@@ Manager.ListAllCheckedOut - 2 - desenhos encontrados=" + files.Count);
            List(files);

            LOG.debug("@@@@@@ Manager.ListAllCheckedOut - 3 - FIM - " + files.Count);
        }

        private void List(List<ADSK.File> files)
        {
            foreach (ADSK.File file in files)
            {
                LOG.debug("@@@@@@@@@@ Manager.List - file: Name=" + file.Name
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

        public void ListaPropertyDef()
        {
            ADSK.PropDef[] defs = util.VaultUtil.ListPropertyDefinition(serviceManager);
            Console.WriteLine("******* Props *************");
            foreach (ADSK.PropDef prop in defs)
            {
                Console.WriteLine(prop.Id + ", SysName=" + prop.SysName + ", DispName=" + prop.DispName + ", DataType=" + prop.Typ);
            }
            Console.WriteLine("***************************");
        }

        public bool ConvertAlreadyDownloadedFile(string filedir, string filename, string[,] validExts, string[] sheetPrefixes,
            string storagefolder, bool preservetemp)
        {
            LOG.info("Manager.ConvertAlreadyDownloadedFile(filedir=" + filedir + ", filename=" + filename + ")");

            string desenho = GetCode(filename, validExts);

            bool result = false;
            try
            {
                Dictionary<string, string> fileInfo = new Dictionary<string, string>();
                LOG.debug("@@@@@@@@@@ Manager.ConvertAlreadyDownloadedFile - 2 - desenho=" + desenho);

                // Usa um diretorio por imagem para evitar problemas em deletar os arquivos
                string imgTempfolder = tempfolder + "\\" + desenho.Trim();
                LOG.debug("@@@@@@@@@@ Manager.ConvertAlreadyDownloadedFile - 3 - imgTempfolder=" + imgTempfolder);

                // Por garantia, limpa o diretorio temporario
                ClearDirectory(imgTempfolder);

                // Cria o diretório temporario da imagem
                if (!Directory.Exists(imgTempfolder))
                {
                    Directory.CreateDirectory(imgTempfolder);
                }

                string filepath = imgTempfolder + "\\" + filename;
                LOG.debug("@@@@@@@@@@ Manager.ConvertAlreadyDownloadedFile - 4 - filepath=" + filepath);

                // Copia o desenho para a pasta temporaria
                File.Copy(filedir + "\\" + filename, filepath);

                result = ConvertFile(filepath, desenho, sheetPrefixes, storagefolder);

                // Limpa o diretorio temporario
                if (!preservetemp)
                {
                    ClearDirectory(imgTempfolder);
                }
                LOG.debug("@@@@@@@@@@ Manager.ConvertAlreadyDownloadedFile - 5 - result=" + result);
            }
            catch (Exception ex)
            {
                LOG.error("Erro convertendo arquivo: " + filedir + "\\" + filename);
                LOG.error(ex.StackTrace);
                LOG.error("====================================================");
                SaveToNextConvertion(desenho, filedir + "\\" + filename);
            }
            return result;
        }

        // ==================

        private void SaveToNextConvertion(string code, string name)
        {
            Dictionary<string, string> erros = new Dictionary<string, string>();
            if (File.Exists(ErrorList))
            {
                erros = DictionaryUtil.ReadPropertyFile(ErrorList);
            }
            DictionaryUtil.SetProperty(erros, code, name);
            DictionaryUtil.WritePropertyFile(ErrorList, erros);
        }

        private void ProcessErrorsFromPreviousConvertion(string[,] validExts,
            string[] sheetPrefixes,
            string storagefolder,
            bool ignorecheckout,
            bool preservetemp)
        {
            if (File.Exists(ErrorList))
            {
                Dictionary<string, string> erros = DictionaryUtil.ReadPropertyFile(ErrorList);
                if (erros.Count > 0)
                {
                    LOG.info("Reprocessando arquivos que deram erros na conversao anterior: " + erros.Count);
                    string[] keys = new string[erros.Count];
                    int cont = 0;
                    foreach (string desenho in erros.Keys)
                    {
                        keys[cont] = desenho;
                        cont++;
                    }
                    foreach (string desenho in keys)
                    {
                        LOG.debug("@@@@@@@@@@ Manager.ProcessErrorsFromPreviousConvertion - 2 - Reprocessando desenho: " + desenho);
                        if (ConvertByFilename(desenho, validExts, sheetPrefixes, storagefolder, ignorecheckout, preservetemp))
                        {
                            LOG.debug("@@@@@@@@@@ Manager.ProcessErrorsFromPreviousConvertion - 3 - Conversao OK");
                            erros.Remove(desenho);
                        }
                        else
                        {
                            LOG.debug("@@@@@@@@@@ Manager.ProcessErrorsFromPreviousConvertion - 4 - Nao conseguiu converter. Manter na lista");
                        }
                    }
                    if (erros.Count > 0)
                    {
                        DictionaryUtil.WritePropertyFile(ErrorList, erros);
                    }
                    else
                    {
                        File.Delete(ErrorList);
                    }
                }
            }
        }

        private void Convert(List<ADSK.File> files, string[,] validExts, string[] sheetPrefixes, string storagefolder,
            bool preservetemp)
        {
            Dictionary<string, string> config = DictionaryUtil.ReadPropertyFile(confFile);
            DictionaryUtil.SetProperty(config, "ultimaExecucao", DateTime.Now.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));

            int contador = 0;
            foreach (ADSK.File file in files)
            {
                Dictionary<string, string> fileInfo = new Dictionary<string, string>();
                string desenho = GetCode(file.Name, validExts);
                //string cfgFile = storagefolder + "\\" + desenho + ".cfg";

                contador++;
                LOG.debug("===================== Vai converter arquivo [" + contador + "] de [" + files.Count + "] - " + file.Name);
                bool converteu = Convert(file, desenho, validExts, sheetPrefixes, storagefolder, preservetemp);
                LOG.debug("===================== Converteu " + file.Name + "? " + converteu);

                DictionaryUtil.SetProperty(config, "LastCheckInDate", file.CkInDate.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss"));
                DictionaryUtil.WritePropertyFile(confFile, config);
            }
        }

        private bool ConvertFile(string file, string desenho, string[] sheetPrefixes, string storagefolder)
        {
            bool result = false;
            LOG.debug("@@@@@@@@@@@@ Manager.ConvertFile - 1 - file=" + file);
            string imgTempfolder = Directory.GetParent(file).FullName;
            LOG.debug("@@@@@@@@@@@@ Manager.ConvertFile - 2 - imgTempfolder=" + imgTempfolder);

            // Enfim, converte as imagens
            List<string> images = null;
            if (file.EndsWith(".dwf"))
            {
                images = dwfconverter.DwfToPDF(file, imgTempfolder, sheetPrefixes);
                LOG.debug("@@@@@@@@@@@@ Manager.ConvertFile - 3 - imagens (DWF) para mergear: " + images.Count);
            }
            else if (file.EndsWith(".pdf"))
            {
                // Apenas copia para a pasta final
                images = new List<string>
                {
                    file
                };
                LOG.debug("@@@@@@@@@@@@ Manager.ConvertFile - 4 - imagens (PDF) para mergear: " + images.Count);
            }

            // Mergeia as imagens
            if (images.Count > 0)
            {
                string destFile = storagefolder + "\\" + desenho + ".pdf";
                GSTools.converter.Converter merger = new GSTools.converter.Converter(pdfconverterexecutable);
                merger.MergePDFs(destFile, images);
                result = true;
                File.SetAttributes(destFile, FileAttributes.Normal);
            }

            LOG.debug("@@@@@@@@@@@@ Manager.ConvertFile - 5 - result=" + result);
            return result;
        }

        private bool Convert(ADSK.File file,
            string desenho,
            string[,]
            validExts,
            string[] sheetPrefixes,
            string storagefolder,
            bool preservetemp)
        {
            bool result = false;
            try
            {
                LOG.debug("@@@@@@@@@@ Manager.Convert - 1 - Name=" + file.Name + ", CkInDate=" + file.CkInDate
                    + ", CheckedOut=" + file.CheckedOut + ", CkOutUserId=" + file.CkOutUserId);
                LOG.debug("@@@@@@@@@@ Manager.Convert - 2 - desenho=" + desenho);

                // Usa um diretorio por imagem para evitar problemas em deletar os arquivos
                string imgTempfolder = tempfolder + "\\" + desenho.Trim();
                LOG.debug("@@@@@@@@@@ Manager.Convert - 3 - imgTempfolder=" + imgTempfolder);

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
                LOG.debug("@@@@@@@@@@ Manager.Convert - 4 - Download efetuado - " + downFile + " - " + File.Exists(downFile));
                File.SetAttributes(downFile, FileAttributes.Normal);

                result = ConvertFile(downFile, desenho, sheetPrefixes, storagefolder);

                // Limpa o diretorio temporario
                if (!preservetemp)
                {
                    ClearDirectory(imgTempfolder);
                }
                LOG.debug("@@@@@@@@@@ Manager.Convert - 8 - FIM");
            }
            catch (Exception ex)
            {
                LOG.error("Erro convertendo arquivo: " + file.Name);
                LOG.error(ex.StackTrace);
                LOG.error("====================================================");
                SaveToNextConvertion(desenho, file.Name);
            }
            return result;
        }

        private string GetCode(string filename, string[,] validExts)
        {
            string code = filename;
            for (int i = 0; i < validExts.Length / 2; i++)
            {
                string ext = validExts[i, 1];
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
            LOG.debug("@@@@@@@@@@@@@@ Manager.ClearDirectory - 1 - (" + dir + ")");
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                    LOG.debug("@@@@@@@@@@@@@@ Manager.ClearDirectory - 2 - (" + dir + ") - OK");
                }
                catch (Exception ex)
                {
                    LOG.debug("@@@@@@@@@@@@@@ Manager.ClearDirectory - 3 - (" + dir + ") - ERRO: " + ex.Message);
                }
            }
        }

        private void SaveFileInfo(ADSK.File file, Dictionary<string, string> d, string cfgFile)
        {
            LOG.debug("@@@@@@@@@@@@ Manager.SaveFileInfo - 1 - Name=" + file.Name + ", cfgFile=" + cfgFile);

            DictionaryUtil.SetProperty(d, "File", file.Name);
            DictionaryUtil.SetProperty(d, "CheckedOut", file.CheckedOut.ToString());
            DictionaryUtil.SetProperty(d, "CkInDate", file.CkInDate.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss"));
            DictionaryUtil.SetProperty(d, "Cksum", file.Cksum.ToString());
            DictionaryUtil.SetProperty(d, "FileSize", file.FileSize.ToString());
            DictionaryUtil.SetProperty(d, "FileStatus", file.FileStatus.ToString());
            DictionaryUtil.SetProperty(d, "ModDate", file.ModDate.ToString());
            DictionaryUtil.SetProperty(d, "CkOutMach", file.CkOutMach);
            DictionaryUtil.SetProperty(d, "CkOutSpec", file.CkOutSpec);
            DictionaryUtil.SetProperty(d, "CkOutUserId", file.CkOutUserId.ToString());

            DictionaryUtil.WritePropertyFile(cfgFile, d);
        }
    }
}
