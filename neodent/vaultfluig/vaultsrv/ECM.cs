using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace vaultsrv
{
    class ECM
    {
        string ecmLogin = null;
        string ecmPassword = null;
        string ecmUser = null;
        string ecmURL = null;
        int ecmCompany = 1;
        int ecmRootFolder = 5;
        string imageDir;
        Boolean upload = true;
        Boolean proxy = true;

        ECMDocService.DocumentServiceService docService = null;
        ECMFolderService.FolderServiceService folderService = null;
        //ECMSearchService.SearchDocumentServiceService searchService = null;
        ECMFolderService.documentDto rootFolder = null;

        public ECM(string pecmLogin,
            string pecmPassword,
            string pecmUser,
            string pecmURL,
            int pecmCompany,
            int pecmRootFolder,
            string pimageDir,
            Boolean pupload,
            Boolean pproxy)
        {
            this.ecmLogin = pecmLogin;
            this.ecmPassword = pecmPassword;
            this.ecmUser = pecmUser;
            this.ecmURL = pecmURL;
            this.ecmCompany = pecmCompany;
            this.ecmRootFolder = pecmRootFolder;
            this.imageDir = pimageDir;
            this.upload = pupload;
            this.proxy = pproxy;

            docService = new ECMDocService.DocumentServiceService();
            if (!ecmURL.EndsWith("/"))
            {
                ecmURL += "/";
            }
            docService.Url = ecmURL + "DocumentService";
            if (proxy)
                docService.Proxy = WebRequest.GetSystemWebProxy();
            folderService = new ECMFolderService.FolderServiceService();
            folderService.Url = ecmURL + "FolderService";
            if (proxy)
                folderService.Proxy = WebRequest.GetSystemWebProxy();
            //searchService = new ECMSearchService.SearchDocumentServiceService();
            //searchService.Url = ecmURL + "/SearchDocumentService";
            //if (proxy)
            //    searchService.Proxy = WebRequest.GetSystemWebProxy();

            // Busca o documento (pasta) raiz onde as imagens serão salvas.
            rootFolder = getECMRootFolder();
            // 2013-05-30: Por algum motivo desconhecido, as vezes não acha a pasta raiz.
            if (rootFolder == null)
            {
                LOG.imprimeLog("WARN: Algo saiu errado, nao conseguiu achar a pasta raiz, faz pausa e tenta novamente.");
                pause();
                rootFolder = getECMRootFolder();
                if (rootFolder == null)
                {
                    LOG.imprimeLog("ERRO: Algo saiu errado, nao conseguiu achar a pasta raiz, faz pausa e tenta novamente.");
                }
            }
        }

        /**
         * Atualiza um documento no ECM.
         */
        public void updateECMDocument(string itemCode, string filePath)
        {
            ECMFolderService.documentDto folder = findCreateECMFolderChild(rootFolder.documentId, itemCode);
            uploadECMDocument(folder, filePath);
        }

        /**
         * Excluir os arquivos de uma pasta no ECM.
         */
        public void deleteECMDocument(string itemCode)
        {
            LOG.imprimeLog(System.DateTime.Now + " ===== Excluindo Item do Fluig: " + itemCode);
            ECMFolderService.documentDto folder = findECMFolderChild(rootFolder.documentId, itemCode);
            if (folder != null)
            {
                ECMFolderService.documentDto[] files = getECMChildren(folder.documentId);
                if (files != null && files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].documentType.Equals("2"))
                        {
                            LOG.imprimeLog(System.DateTime.Now + " ===== Excluindo no Fluig: " + files[i].phisicalFile);
                            docService.deleteDocument(ecmUser, ecmPassword, ecmCompany, files[i].documentId, ecmLogin);
                            LOG.imprimeLog(System.DateTime.Now + " ===== Excluiu no Fluig: " + files[i].phisicalFile);
                        }
                    }
                }
            }
        }

        /**
         * Exclui um documento do ECM.
         */
        public void deleteECMDocument(string itemCode, string filePath)
        {
            string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);

            ECMFolderService.documentDto folder = findECMFolderChild(rootFolder.documentId, itemCode);
            if (folder != null)
            {
                ECMFolderService.documentDto exist = getECMChild(fileName, folder.documentId);
                if (exist != null)
                {
                    LOG.imprimeLog(System.DateTime.Now + " ======= Excluindo no Fluig: " + fileName);
                    docService.deleteDocument(ecmUser, ecmPassword, ecmCompany, exist.documentId, ecmLogin);
                    LOG.imprimeLog(System.DateTime.Now + " ======= Excluiu no Fluig: " + fileName);
                }
            }
        }

        /**
         * Faz o download do arquivo de log de um desenho.
         */
        public Boolean downloadItemLog(string itemCode, string baseDir)
        {
            Boolean result = false;
            ECMFolderService.documentDto folder = findECMFolderChild(rootFolder.documentId, itemCode);
            if (folder != null)
            {
                string fileName = itemCode + ".log";
                ECMFolderService.documentDto exist = getECMChild(fileName, folder.documentId);
                if (exist != null)
                {
                    LOG.imprimeLog(System.DateTime.Now + " ====== Arquivo a baixar do Fluig: " + exist.phisicalFile);
                    byte[] b = docService.getDocumentContent(ecmLogin, ecmPassword, ecmCompany, exist.documentId, ecmUser, exist.version, exist.phisicalFile);
                    if (b == null)
                    {
                        throw new Exception("Nao conseguiu baixar do Fluig o documento " + exist.documentId);
                    }
                    string destFile = baseDir + "\\" + fileName;

                    FileStream fs = new FileStream(destFile, FileMode.CreateNew, FileAccess.Write);
                    fs.Write(b, 0, b.Length);
                    fs.Flush();
                    fs.Close();
                    fs.Dispose();
                    result = true;
                }
            }
            return result;
        }

        /**
         * Faz o download dos arquivos de uma pasta do ECM.
         */
        public Dictionary<string, string> downloadECMDocuments(string itemCode, string baseDir, Dictionary<string, string> fileProps
            , bool getordem, bool getdes, bool getanvisa, bool getfda, bool getcheckedout)
        {
            //ECMFolderService.documentDto folder = getECMChild(itemCode, rootFolder.documentId);
            ECMFolderService.documentDto folder = findECMFolderChild(rootFolder.documentId, itemCode);
            if (folder != null)
            {
                ECMFolderService.documentDto[] files = getECMChildren(folder.documentId);
                Util.SetProperty(fileProps, "0", "False=Extract");
                if (files != null && files.Length > 0)
                {
                    string logDestFile = null;
                    bool getAll = getordem && getdes && getanvisa && getfda;

                    // Primeiro, baixa o arquivo .log
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].documentType.Equals("2"))
                        {
                            if (files[i].phisicalFile.ToLower().Equals(itemCode.ToLower() + ".log"))
                            {
                                LOG.imprimeLog(System.DateTime.Now + " ====== Arquivo a baixar do Fluig: " + files[i].phisicalFile);
                                byte[] b = docService.getDocumentContent(ecmLogin, ecmPassword, ecmCompany, files[i].documentId, ecmUser, files[i].version, files[i].phisicalFile);
                                if (b == null)
                                {
                                    throw new Exception("Nao conseguiu baixar do Fluig o documento " + files[i].documentId);
                                }
                                logDestFile = baseDir + "\\" + files[i].phisicalFile;

                                FileStream fs = new FileStream(logDestFile, FileMode.CreateNew, FileAccess.Write);
                                fs.Write(b, 0, b.Length);
                                fs.Flush();
                                fs.Close();
                                fs.Dispose();
                                break;
                            }
                        }
                    }

                    List<int> fileNumList = new List<int>();
                    bool isCheckedOut = false;

                    if (logDestFile == null)
                    {
                        getAll = true;
                    }
                    else
                    {
                        // Monta uma lista com os arquivos a serem baixados
                        Dictionary<string, string> logProps = Util.ReadPropertyFile(logDestFile);
                        foreach (KeyValuePair<string, string> pair in logProps)
                        {
                            if (pair.Key.ToLower().Equals("checkedout"))
                            {
                                isCheckedOut = pair.Value.ToLower().Equals("true");
                            }
                            else
                            {
                                int key = 0;
                                if (int.TryParse(pair.Key, out key))
                                {
                                    if (key > 0)
                                    {
                                        string[] ss = pair.Value.Split('=');
                                        if (ss[0].ToUpper().Equals("OP") && getordem)
                                        {
                                            fileNumList.Add(key);
                                        }
                                        else if (ss[0].ToUpper().Equals("PS") && getordem)
                                        {
                                            fileNumList.Add(key);
                                        }
                                        else if (ss[0].ToUpper().Equals("DES") && getdes)
                                        {
                                            fileNumList.Add(key);
                                        }
                                        else if (ss[0].ToUpper().Equals("ANVISA") && getanvisa)
                                        {
                                            fileNumList.Add(key);
                                        }
                                        else if (ss[0].ToUpper().Equals("FDA") && getfda)
                                        {
                                            fileNumList.Add(key);
                                        }
                                    }
                                }
                            }
                        }

                    }

                    if (isCheckedOut && !getcheckedout)
                    {
                        LOG.imprimeLog(System.DateTime.Now + " ====== Nenhum desenho sera baixado pois esta em checkout e nao deve ser baixado estes");
                        // Desenho esta em checkout e nao esta marcado para baixar os desenhos em checkout, entao ignora.
                        return fileProps;
                    }

                    // Agora baixa as imagens
                    for (int i = 0; i < files.Length; i++)
                    {
                        //LOG.imprimeLog(System.DateTime.Now + " ====== TIPO: " + files[i].documentType);
                        if (files[i].phisicalFile.EndsWith(".log"))
                            continue;
                        if (files[i].documentType.Equals("2"))
                        {
                            bool baixar = getAll;
                            if (!getAll)
                            {
                                foreach (int key in fileNumList)
                                {
                                    if (files[i].phisicalFile.ToLower().Equals(itemCode.ToLower() + "-" + key + ".jpg"))
                                    {
                                        baixar = true;
                                        break;
                                    }
                                }
                            }
                            if (baixar)
                            {
                                LOG.imprimeLog(System.DateTime.Now + " ====== Arquivo a baixar do Fluig: " + files[i].phisicalFile);
                                byte[] b = docService.getDocumentContent(ecmLogin, ecmPassword, ecmCompany, files[i].documentId, ecmUser, files[i].version, files[i].phisicalFile);
                                if (b == null)
                                {
                                    throw new Exception("Nao conseguiu baixar do Fluig o documento " + files[i].documentId);
                                }
                                string destFile = baseDir + "\\" + files[i].phisicalFile;

                                if (files[i].phisicalFile.IndexOf('.') > 0)
                                {
                                    string s1 = files[i].phisicalFile.Substring(0, files[i].phisicalFile.LastIndexOf('.'));
                                    if (s1.IndexOf('-') > 0)
                                    {
                                        s1 = s1.Substring(s1.LastIndexOf('-') + 1);
                                        Util.SetProperty(fileProps, s1, "OP");
                                    }
                                }

                                FileStream fs = new FileStream(destFile, FileMode.CreateNew, FileAccess.Write);
                                fs.Write(b, 0, b.Length);
                                fs.Flush();
                                fs.Close();
                                fs.Dispose();
                            }
                            else
                            {
                                LOG.imprimeLog(System.DateTime.Now + " ====== Arquivo do Fluig sendo ignorado: " + files[i].phisicalFile);
                            }
                        }
                        else
                        {
                            LOG.imprimeLog(System.DateTime.Now + " ====== Outro arquivo Fluig: " + files[i].documentType + " -> " + files[i].phisicalFile);
                        }
                    }
                }
            }
            return fileProps;
        }

        /**
         * Faz o upload de um arquivo para o ECM.
         */
        private void uploadECMDocument(ECMFolderService.documentDto folder, string filePath)
        {
            LOG.imprimeLog(System.DateTime.Now + " ======= Upload de arquivo para o fluig: Pasta=" + folder.documentId + ", Arquivo fisico=" + filePath);
            //TODO: Tratar para mandar pro ECM apenas os desenhos que devem ser impressos.
            string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);

            //TODO: ECMFolderService.documentDto exist = getECMChild(fileName, folder);
            ECMFolderService.documentDto exist = getECMChild(fileName, folder.documentId);

            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] filebytes = new byte[fs.Length];
            fs.Read(filebytes, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            fs.Dispose();
            LOG.imprimeLog(System.DateTime.Now + " ======= Upload de arquivo para o fluig: Nome do arquivo=" + fileName + ", Tamanho=" + filebytes.Length);

            //string base64String = System.Convert.ToBase64String(filebytes, 0, filebytes.Length);
            //System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            //Byte[] bytesMessage = UTF8.GetBytes(base64String);

            ECMDocService.attachment[] attach = new ECMDocService.attachment[1];
            attach[0] = new ECMDocService.attachment();
            attach[0].editing = false;
            attach[0].attach = false;
            attach[0].filecontent = filebytes;
            attach[0].fileName = fileName;
            attach[0].fileSize = filebytes.Length;
            attach[0].principal = true;

            if (exist == null)
            {
                // Arquivo novo, cria.
                LOG.imprimeLog(System.DateTime.Now + " ======= Criando um novo arquivo no Fluig: [" + fileName + "] na pasta [" + folder.documentId + "]");
                ECMDocService.webServiceMessage[] result = docService.createSimpleDocument(ecmLogin, ecmPassword, ecmCompany, folder.documentId, ecmUser, fileName, attach);
                if (result == null || result.Length == 0 || result[0].documentId == 0)
                {
                    LOG.imprimeLog(System.DateTime.Now + " ======= WARN: Falhou upload do documento. Tentando novamente.");
                    result = docService.createSimpleDocument(ecmLogin, ecmPassword, ecmCompany, folder.documentId, ecmUser, fileName, attach);
                    if (result == null || result.Length == 0 || result[0].documentId == 0)
                    {
                        LOG.imprimeLog(System.DateTime.Now + " ======= ERRO: Falhou upload do documento.");
                        throw new Exception("Nao conseguiu fazer upload para o Fluig do arquivo '" + filePath + "'");
                    }
                }
                LOG.imprimeLog(System.DateTime.Now + " ======= Arquivo criado no Fluig: [" + fileName
                    + "] na pasta [" + folder.documentId + "] com ID [" + result[0].documentId + "]");
            }
            else
            {
                // Arquivo existente, atualiza.
                LOG.imprimeLog(System.DateTime.Now + " ======= Atualizando um arquivo no Fluig: " + fileName);
                docService.updateSimpleDocument(ecmLogin, ecmPassword, ecmCompany, exist.documentId, ecmUser, fileName, attach);
                LOG.imprimeLog(System.DateTime.Now + " ======= Arquivo atualizado no Fluig: " + fileName);
            }

            //docService.getActiveDocument(ecmUser, ecmPassword, ecmCompany, 1, ecmUser);
            // 2013-05-30: Por algum motivo, as vezes não está encontrando o documento e gerando erro.
            //             Faz uma pausa e tenta novamente quando isso ocorrer.
            try
            {
                docService.getActiveDocument(ecmUser, ecmPassword, ecmCompany, 1, ecmUser);
            }
            catch (Exception ex)
            {
                LOG.imprimeLog(System.DateTime.Now + " Error : uploadECMDocument() : Falhou obtendo documento ativo. Tenta novamente");
                LOG.imprimeLog(System.DateTime.Now + " Error=" + ex.Message);
                LOG.imprimeLog("Empresa: " + ecmCompany + ", Usuario: " + ecmUser);
                LOG.imprimeLog("StackTrace: " + ex.StackTrace);
                pause();
                try
                {
                    docService.getActiveDocument(ecmUser, ecmPassword, ecmCompany, 1, ecmUser);
                }
                catch (Exception ex1)
                {
                    LOG.imprimeLog(System.DateTime.Now + " Error: uploadECMDocument() : Falhou de novo, agora lascou!!");
                    LOG.imprimeLog(System.DateTime.Now + " Error=" + ex1.Message);
                    Program.sendMail("ERRO Conversao desenho", "O Fluig falhou, nao foi possivel fazer upload de um arquivo: " + filePath);
                    throw new Exception("Nao conseguiu fazer upload para o Fluig do arquivo '" + filePath + "' [uploadECMDocument()]");
                }
            }
            // FIM
        }

        /**
         * Cria um novo documento.
         */
        private ECMFolderService.documentDto createECMFolder(string name, int parentId)
        {
            ECMFolderService.webServiceMessage[] result = folderService.createSimpleFolder(ecmLogin, ecmPassword, ecmCompany, parentId, ecmUser, name);
            if (result == null)
            {
                throw new Exception("Nao conseguiu criar uma pasta no FLuig [createECMFolder()]");
            }

            ECMFolderService.documentDto folder = new ECMFolderService.documentDto();
            folder.companyId = result[0].companyId;
            folder.documentDescription = result[0].documentDescription;
            folder.documentId = result[0].documentId;
            folder.version = result[0].version;
            return folder;
        }

        /**
         * Procura e já cria uma pasta do desenho, com suas subpastas.
         */
        private ECMFolderService.documentDto findCreateECMFolderChild(int parentId, string name)
        {
            string[] folders = parseCode(name);
            int pId = parentId;
            ECMFolderService.documentDto folder = null;

            foreach (string s in folders)
            {
                folder = getECMChild(s, pId);
                if (folder == null)
                {
                    folder = createECMFolder(s, pId);
                }
                pId = folder.documentId;
            }
            return folder;
        }

        /**
         * Procura uma pasta do desenho, com suas subpastas.
         */
        private ECMFolderService.documentDto findECMFolderChild(int parentId, string name)
        {
            string[] folders = parseCode(name);
            int pId = parentId;
            ECMFolderService.documentDto folder = null;

            foreach (string s in folders)
            {
                DateTime t = System.DateTime.Now;
                folder = getECMChild(s, pId);
                System.DateTime.Now.Subtract(t);
                LOG.imprimeLog(System.DateTime.Now + " ------ Tempo gasto buscando a pasta " + s + ": " + (System.DateTime.Now.Subtract(t).Milliseconds));
                if (folder == null)
                {
                    break;
                }
                pId = folder.documentId;
            }
            return folder;
        }

        /**
         * Busca um documento/pasta filho.
         */
        private ECMFolderService.documentDto getECMChild(string name, int parentId)
        {
            ECMFolderService.documentDto[] fdocs = folderService.getChildren(ecmLogin, ecmPassword, ecmCompany, parentId, ecmUser);
            ECMFolderService.documentDto result = null;
            // 2013-05-30: IF colocado porque as vezes está retornando NULL!!!
            if (fdocs == null)
            {
                LOG.imprimeLog(System.DateTime.Now + " Error: getECMChild() : Falhou obtendo filhos onde nao devia. Pausa e tenta novamente");
                pause();
                fdocs = folderService.getChildren(ecmLogin, ecmPassword, ecmCompany, parentId, ecmUser);
            }
            if (fdocs == null)
            {
                LOG.imprimeLog(System.DateTime.Now + " Error: getECMChild() : Falhou NOVAMENTE obtendo filhos onde nao devia. Agora lascou!");
                throw new Exception("Nao conseguiu ler a estrutura de pastas do Fluig [getECMChild()]");
            }
            // FIM
            for (int i = 0; i < fdocs.Length; i++)
            {
                if (fdocs[i].documentDescription.Equals(name))
                {
                    result = fdocs[i];
                    break;
                }
            }
            return result;
        }

        /**
         * Encontra a pasta raiz dos desenhos no ECM.
         */
        private ECMFolderService.documentDto getECMRootFolder()
        {
            ECMFolderService.documentDto[] fdocs = folderService.getFolder(ecmLogin, ecmPassword, ecmCompany, ecmRootFolder, ecmUser);
            return fdocs[0];
        }

        /**
         * Faz o parser do código do item para criar as pastas.
         */
        private string[] parseCode(string code)
        {
            LinkedList<string> result = new LinkedList<string>();
            char[] cs = code.ToLower().ToCharArray();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cs.Length; i++)
            {
                if ((cs[i] >= '0' && cs[i] <= '9') || (cs[i] >= 'a' && cs[i] <= 'z'))
                {
                    if (sb.Length > 0)
                        sb.Append("/");
                    sb.Append(new string(cs[i], 1));
                    result.AddLast(new string(cs[i], 1));
                }
            }

            for (int i = 0; i < code.Length - 1; i++)
            {
                string c = code.Substring(i, 1);
            }
            LOG.imprimeLog(System.DateTime.Now + " ===== Estrutura Pastas: " + sb.ToString());
            string[] sArray = new string[result.Count];
            result.CopyTo(sArray, 0);
            return sArray;
        }

        private ECMFolderService.documentDto[] getECMChildren(int parentId)
        {
            ECMFolderService.documentDto[] fdocs = folderService.getChildren(ecmLogin, ecmPassword, ecmCompany, parentId, ecmUser);
            return fdocs;
        }

        private void pause()
        {
            System.Threading.Thread.Sleep(5000);
        }
    }
}
