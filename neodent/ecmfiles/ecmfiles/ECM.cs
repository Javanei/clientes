using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace ecmfiles
{
    class ECM
    {
        string ecmLogin = null;
        string ecmPassword = null;
        string ecmUser = null;
        string ecmURL = null;
        int ecmCompany = 1;
        int ecmRootFolder = 0;
        string ecmProxyServer = null;
        int ecmProxyPort = 0;
        string ecmProxyUser;
        string ecmProxyPass;
        string ecmFolder = null;

        ECMDocService.DocumentServiceService docService = null;
        ECMFolderService.FolderServiceService folderService = null;
        ECMFolderService.documentDto _rootFolder = null;

        public ECM(string pecmLogin,
            string pecmPassword,
            string pecmUser,
            string pecmURL,
            int pecmCompany,
            string pecmFolder,
            int pecmRootFolder,
            string pecmProxyServer,
            int pecmProxyPort,
            string pecmProxyUser,
            string pecmProxyPass)
        {
            this.ecmLogin = pecmLogin;
            this.ecmPassword = pecmPassword;
            this.ecmUser = pecmUser;
            this.ecmURL = pecmURL;
            this.ecmCompany = pecmCompany;
            this.ecmFolder = pecmFolder;
            this.ecmRootFolder = pecmRootFolder;
            this.ecmProxyServer = pecmProxyServer;
            this.ecmProxyPort = pecmProxyPort;
            this.ecmProxyUser = pecmProxyUser;
            this.ecmProxyPass = pecmProxyPass;

            docService = new ECMDocService.DocumentServiceService();
            docService.Url = ecmURL + "/DocumentService";
            if (this.ecmProxyServer == null || this.ecmProxyServer.Trim().Length == 0)
            {
                docService.Proxy = WebRequest.GetSystemWebProxy();
            }
            folderService = new ECMFolderService.FolderServiceService();
            folderService.Url = ecmURL + "/FolderService";
            if (this.ecmProxyServer == null || this.ecmProxyServer.Trim().Length == 0)
            {
                folderService.Proxy = WebRequest.GetSystemWebProxy();
                LOG.imprimeLog(System.DateTime.Now + " @@@@@@@ Usando proxy do IE");
            }

            if (this.ecmProxyServer != null && this.ecmProxyServer.Trim().Length > 0)
            {
                if (this.ecmProxyServer.Equals("noproxy"))
                {
                    LOG.imprimeLog(System.DateTime.Now + " @@@@@@@ Nao usando proxy");
                }
                else
                {
                    WebProxy oproxy = this.ecmProxyPort > 0 ? new WebProxy(this.ecmProxyServer, this.ecmProxyPort) : new WebProxy(this.ecmProxyServer);
                    oproxy.BypassProxyOnLocal = true;
                    if (this.ecmProxyUser != null && this.ecmProxyUser.Trim().Length > 0)
                    {
                        oproxy.Credentials = new NetworkCredential(this.ecmProxyUser, this.ecmProxyPass, "neodent");
                    }
                    folderService.Proxy = oproxy;
                    docService.Proxy = oproxy;
                    LOG.imprimeLog(System.DateTime.Now + " @@@@@@@ Usando servidor de proxy: " + this.ecmProxyServer + ":" + this.ecmProxyPort);
                }
            }

            //LOG.imprimeLog(System.DateTime.Now + " @@@@@@@ Vai buscar root folder");
            /*this.rootFolder = getRootFolder();
            if (this.rootFolder == null)
            {
                LOG.imprimeLog("WARN: Algo saiu errado, nao conseguiu achar a pasta raiz, faz pausa e tenta novamente.");
                pause();
                rootFolder = getRootFolder();
                if (rootFolder == null)
                {
                    LOG.imprimeLog("ERRO: Algo saiu errado, nao conseguiu achar a pasta raiz, faz pausa e tenta novamente.");
                }
                throw new Exception("Nao foi possivel achar a pasta raiz no ECM");
            }*/
        }

        /**
         * Lista os arquivos de uma pasta.
         */
        public ECMFolderService.documentDto[] listFolder()
        {
            LOG.imprimeLog(System.DateTime.Now + " ======= listFolder - 1");
            ECMFolderService.documentDto[] files = folderService.getChildren(this.ecmLogin, this.ecmPassword, this.ecmCompany, this.getRootFolder().documentId, this.ecmUser);
            LOG.imprimeLog(System.DateTime.Now + " ======= listFolder - 2 - Arquivo encontrados: " + files.Length);
            return files;
        }

        /**
         * Atualiza um documento no ECM.
         */
        public void uploadDocument(string filePath)
        {
            LOG.imprimeLog(System.DateTime.Now + " ======= uploadDocument - 1: " + filePath);
            uploadDocument(this.getRootFolder(), filePath);
            LOG.imprimeLog(System.DateTime.Now + " ======= uploadDocument - 2 -  upload terminado");
        }

        /**
         * Exclui um documento.
         */
        public void deleteDocument(int docId)
        {
            LOG.imprimeLog(System.DateTime.Now + " ======= deleteDocument - 1: " + docId);
            ECMFolderService.documentDto exist = getDocument(docId, this.getRootFolder().documentId);
            if (exist != null)
            {
                //LOG.imprimeLog(System.DateTime.Now + " ======= deleteDocument - 2 - Excluindo documento: " + exist.documentId + " - " + exist.documentDescription);
                docService.deleteDocument(this.ecmLogin, this.ecmPassword, this.ecmCompany, exist.documentId, this.ecmUser);
                LOG.imprimeLog(System.DateTime.Now + " ======= deleteDocument - 3 - Exclusao efetuada");
            }
            else
            {
                LOG.imprimeLog(System.DateTime.Now + " ===== deleteDocument - 4 - Lascou!!! Nao achou o documento " + docId + "!!");
            }
        }

        /**
         * Exclui um documento.
         */
        public void deleteDocument(String name)
        {
            //string decodedName = decodeFileName(name);
            LOG.imprimeLog(System.DateTime.Now + " ======= deleteDocument - 1: " + name);
            ECMFolderService.documentDto exist = getDocument(name, this.getRootFolder().documentId);
            if (exist != null)
            {
                //LOG.imprimeLog(System.DateTime.Now + " ======= deleteDocument - 2 - Excluindo documento: " + exist.documentId + " - " + exist.documentDescription);
                docService.deleteDocument(this.ecmLogin, this.ecmPassword, this.ecmCompany, exist.documentId, this.ecmUser);
                LOG.imprimeLog(System.DateTime.Now + " ======= deleteDocument - 3 - Exclusao efetuada");
            }
            else
            {
                LOG.imprimeLog(System.DateTime.Now + " ======= deleteDocument - 4 - Lascou!!! Nao achou o documento " + name + "!!");
            }
        }

        /**
         * Exclui um documento.
         */
        public void viewDocument(int docId)
        {
            LOG.imprimeLog(System.DateTime.Now + " ======= viewDocument - 1 - Visualizacao do documento: " + docId);
            ECMFolderService.documentDto exist = getDocument(docId, this.getRootFolder().documentId);
            viewDocument(exist);
            //LOG.imprimeLog(System.DateTime.Now + " ======= viewDocument - 2 - Visualizacao aberta");
        }

        /**
         * Exclui um documento.
         */
        public void viewDocument(String name)
        {
            //string decodedName = decodeFileName(name);
            LOG.imprimeLog(System.DateTime.Now + " ======= viewDocument - 1 - Visualizacao do arquivo: " + name);
            ECMFolderService.documentDto exist = getDocument(name, this.getRootFolder().documentId);
            viewDocument(exist);
            //LOG.imprimeLog(System.DateTime.Now + " ======= viewDocument - 2 - Visualizacao aberta");
        }

        public void convert()
        {
            LOG.imprimeLog(System.DateTime.Now + " ======= convert - 1 - Iniciando a conversao");
            ECMFolderService.documentDto[] fdocs = folderService.getFolder(this.ecmLogin, this.ecmPassword, this.ecmCompany, this.ecmRootFolder, this.ecmUser);
            ECMFolderService.documentDto fromRoot = fdocs[0];
            ECMFolderService.documentDto[] folders = folderService.getSubFolders(this.ecmLogin, this.ecmPassword, this.ecmCompany, fromRoot.documentId, this.ecmUser);
            for (int i = 0; i < folders.Length; i++)
            {
                ECMFolderService.documentDto folder = folders[i];
                LOG.imprimeLog(System.DateTime.Now + " ======= convert - 2 - " + folder.documentId + "(" + folder.documentDescription + ")");
                if (folder.documentDescription.Length > 1)
                {
                    this._rootFolder = null;
                    this.ecmFolder = folder.documentDescription;
                    ECMFolderService.documentDto root = this.getRootFolder();
                    LOG.imprimeLog(System.DateTime.Now + " ======= convert - 3 - Nome da pasta destino: " + this.ecmFolder);
                    LOG.imprimeLog(System.DateTime.Now + " ======= convert - 4 - mover de " + folder.documentId + "("
                            + folder.documentDescription + ") para " + root.documentId + "(" + root.documentDescription + ")");
                    ECMFolderService.documentDto[] files = folderService.getChildren(this.ecmLogin, this.ecmPassword, this.ecmCompany, folder.documentId, this.ecmUser);
                    int?[] ids = new int?[files.Length];
                    for (int j = 0; j < files.Length; j++)
                    {
                        ECMFolderService.documentDto file = files[j];
                        LOG.imprimeLog(System.DateTime.Now + " ======= convert - 5 - preparando para mover arquivo: " + file.documentId + "(" + file.documentDescription + ")");
                        ids[j] = file.documentId;
                    }
                    if (ids.Length > 0)
                    {
                        LOG.imprimeLog(System.DateTime.Now + " ======= convert - 6 - vai mover: " + ids.Length + " arquivos");
                        docService.moveDocument(this.ecmLogin, this.ecmPassword, this.ecmCompany, ids, this.ecmUser, root.documentId);
                        LOG.imprimeLog(System.DateTime.Now + " ======= convert - 7 - Pronto!");
                    }
                    else
                    {
                        LOG.imprimeLog(System.DateTime.Now + " ======= convert - 8 - A pasta esta vazia");
                    }
                    folderService.deleteDocument(this.ecmLogin, this.ecmPassword, this.ecmCompany, folder.documentId, this.ecmUser);
                    LOG.imprimeLog(System.DateTime.Now + " ======= convert - 9 - Removeu a pasta origem");
                }
                else
                {
                    LOG.imprimeLog(System.DateTime.Now + " ======= convert - 10 - Ignorando por ser tamanho 1: " + folder.documentId + " - " + folder.documentDescription);
                }
                LOG.imprimeLog(System.DateTime.Now + " =========================================");
            }
        }

        private void viewDocument(ECMFolderService.documentDto doc)
        {
            if (doc != null)
            {
                //LOG.imprimeLog(System.DateTime.Now + " ========= Exibindo documento: " + doc.documentId + " - " + doc.version + " = " + doc.documentDescription);
                SHDocVw.InternetExplorer ie = new SHDocVw.InternetExplorer();
                string url = ecmURL;
                if (!url.EndsWith("/"))
                {
                    url = url + "/";
                }
                url = url + "documentviewer?WDNrDocto=" + doc.documentId + "&WDNrVersao=" + doc.version;
                //LOG.imprimeLog(System.DateTime.Now + " ========= URL: " + url);
                ie.Navigate(url);
                ie.ToolBar = 0;
                ie.AddressBar = false;
                ie.Visible = true;
            }
            else
            {
                LOG.imprimeLog(System.DateTime.Now + " ========= Lascou!!! Nao achou o documento!!");
            }
        }

        /**
         * Faz o parser do caminho.
         */
        private string[] parseFolder(String basePath)
        {
            LinkedList<string> result = new LinkedList<string>();
            String s;
            if (basePath.StartsWith("/"))
                s = basePath.Substring(1);
            else
                s = basePath;
            while (s.Length > 0 && s.EndsWith("/"))
            {
                s = s.Substring(0, s.Length - 1);
            }
            while (s.Length > 0)
            {
                if (s.IndexOf("/") > 0)
                {
                    result.AddLast(s.Substring(0, s.IndexOf("/")));
                    s = s.Substring(s.IndexOf("/") + 1);
                }
                else
                {
                    result.AddLast(s);
                    s = "";
                }
            }
            string s1 = result.ElementAt(result.Count - 1);
            result.RemoveLast();
            string[] s2 = parseCode(s1);
            for (int i = 0; i < s2.Length; i++)
            {
                result.AddLast(s2[i]);
            }

            string[] sArray = new string[result.Count];
            result.CopyTo(sArray, 0);
            return sArray;
        }

        /**
         * Busca uma pasta na raiz com o nome informado.
         */
        private ECMFolderService.documentDto getRootFolder(String name)
        {
            LOG.imprimeLog(System.DateTime.Now + " ========= getRootFolder(name) - 1 - Buscando ROOT folder: " + name);
            ECMFolderService.documentDto result = null;

            ECMFolderService.documentDto[] fs = folderService.getRootFolders(this.ecmLogin, this.ecmPassword, this.ecmCompany, this.ecmUser);
            if (fs != null)
            {
                for (int i = 0; i < fs.Length; i++)
                {
                    if (fs[i].documentDescription.Equals(name))
                    {
                        result = fs[i];
                        break;
                    }
                }
            }
            if (result == null)
            {
                LOG.imprimeLog(System.DateTime.Now + " ========= getRootFolder(name) - 2 - Lascou!!! Nao encontrou!!");
                throw new Exception("Pasta raiz " + name + " nao encontrada");
            }
            LOG.imprimeLog(System.DateTime.Now + " ========= getRootFolder(name) - 3 - ROOT folder: " + result.documentId + "=" + result.documentDescription);
            return result;
        }

        /**
          * Cria um novo folder.
          */
        private ECMFolderService.documentDto createFolder(string name, int parentId)
        {
            LOG.imprimeLog(System.DateTime.Now + " ========= createFolder - 1 - Vai criar folder: " + name + " (parent=" + parentId + ")");
            ECMFolderService.webServiceMessage[] result = folderService.createSimpleFolder(this.ecmLogin, this.ecmPassword, this.ecmCompany, parentId, this.ecmUser, name);

            ECMFolderService.documentDto folder = new ECMFolderService.documentDto();
            folder.companyId = result[0].companyId;
            folder.documentDescription = result[0].documentDescription;
            folder.documentId = result[0].documentId;
            folder.version = result[0].version;
            LOG.imprimeLog(System.DateTime.Now + " ========= createFolder - 2 - Criado folder: " + folder.documentId + "=" + folder.documentDescription);
            return folder;
        }


        /**
         * Faz o upload de um arquivo para o ECM.
         */
        private void uploadDocument(ECMFolderService.documentDto folder, string filePath)
        {
            string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
            //string decodedFileName = decodeFileName(fileName);
            ECMFolderService.documentDto exist = getDocument(fileName, folder.documentId);

            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] filebytes = new byte[fs.Length];
            fs.Read(filebytes, 0, Convert.ToInt32(fs.Length));

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
                LOG.imprimeLog(System.DateTime.Now + " ========= Criando um novo arquivo no ECM: " + fileName);
                docService.createSimpleDocument(this.ecmLogin, this.ecmPassword, this.ecmCompany, folder.documentId, this.ecmUser, fileName, attach);
            }
            else
            {
                // Arquivo existente, atualiza.
                LOG.imprimeLog(System.DateTime.Now + " ========= Atualizando um arquivo no ECM: " + fileName);
                docService.updateSimpleDocument(this.ecmLogin, this.ecmPassword, this.ecmCompany, exist.documentId, this.ecmUser, fileName, attach);
                //LOG.imprimeLog(System.DateTime.Now + " ========= Arquivo atualizado no ECM: " + fileName);
            }

            try
            {
                docService.getActiveDocument(this.ecmLogin, this.ecmPassword, this.ecmCompany, 1, this.ecmUser);
            }
            catch (Exception ex)
            {
                LOG.imprimeLog(System.DateTime.Now + " Error : uploadDocument() : Falhou obtendo documento ativo. Tenta novamente");
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
                    LOG.imprimeLog(System.DateTime.Now + " Error: uploadDocument() : Falhou de novo, agora lascou!!");
                    LOG.imprimeLog(System.DateTime.Now + " Error=" + ex1.Message);
                }
            }
        }

        /**
         * Busca um documento/pasta filho.
         */
        private ECMFolderService.documentDto getDocument(string name, int parentId)
        {
            ECMFolderService.documentDto[] fdocs = folderService.getChildren(this.ecmLogin, this.ecmPassword, this.ecmCompany, parentId, this.ecmUser);
            ECMFolderService.documentDto result = null;
            if (fdocs == null)
            {
                LOG.imprimeLog(System.DateTime.Now + " Error: getDocument() : Falhou obtendo filhos onde nao devia. Pausa e tenta novamente");
                pause();
                fdocs = folderService.getChildren(this.ecmLogin, this.ecmPassword, this.ecmCompany, parentId, this.ecmUser);
            }
            if (fdocs == null)
            {
                LOG.imprimeLog(System.DateTime.Now + " Error: getDocument() : Falhou NOVAMENTE obtendo filhos onde nao devia. Agora lascou!");
            }
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
         * Busca um documento/pasta filho.
         */
        private ECMFolderService.documentDto getDocument(int docId, int parentId)
        {
            ECMFolderService.documentDto[] fdocs = folderService.getChildren(this.ecmLogin, this.ecmPassword, this.ecmCompany, parentId, this.ecmUser);
            ECMFolderService.documentDto result = null;
            if (fdocs == null)
            {
                LOG.imprimeLog(System.DateTime.Now + " Error: getDocument() : Falhou obtendo filhos onde nao devia. Pausa e tenta novamente");
                pause();
                fdocs = folderService.getChildren(this.ecmLogin, this.ecmPassword, this.ecmCompany, parentId, this.ecmUser);
            }
            if (fdocs == null)
            {
                LOG.imprimeLog(System.DateTime.Now + " Error: getDocument() : Falhou NOVAMENTE obtendo filhos onde nao devia. Agora lascou!");
            }
            for (int i = 0; i < fdocs.Length; i++)
            {
                if (fdocs[i].documentId == docId)
                {
                    result = fdocs[i];
                    break;
                }
            }
            return result;
        }

        private ECMFolderService.documentDto getRootFolder()
        {
            if (this._rootFolder != null)
            {
                return this._rootFolder;
            }
            this._rootFolder = _getRootFolder();
            if (this._rootFolder == null)
            {
                LOG.imprimeLog("WARN: Algo saiu errado, nao conseguiu achar a pasta raiz, faz pausa e tenta novamente.");
                pause();
                _rootFolder = getRootFolder();
                if (_rootFolder == null)
                {
                    throw new Exception("Nao foi possivel achar a pasta raiz no ECM");
                }
            }
            return this._rootFolder;
        }

        /**
         * Encontra a pasta raiz dos desenhos no ECM.
         */
        private ECMFolderService.documentDto _getRootFolder()
        {
            LOG.imprimeLog(System.DateTime.Now + " ========= getRootFolder() - 1 - Buscando ROOT folder: " + this.ecmRootFolder);
            ECMFolderService.documentDto result = null;
            if (this.ecmRootFolder > 0)
            {
                LOG.imprimeLog(System.DateTime.Now + " ========= getRootFolder() - 2 - Buscando ROOT folder: " + this.ecmRootFolder);
                ECMFolderService.documentDto[] fdocs = folderService.getFolder(this.ecmLogin, this.ecmPassword, this.ecmCompany, this.ecmRootFolder, this.ecmUser);
                LOG.imprimeLog(System.DateTime.Now + " ========= getRootFolder() - 3 - Encontrou ROOT folder: " + fdocs[0].documentDescription);
                result = fdocs[0];
            }
            LOG.imprimeLog(System.DateTime.Now + " ========= getRootFolder() - 4");

            if (this.ecmFolder != null && this.ecmFolder.Trim().Length > 0)
            {
                LOG.imprimeLog(System.DateTime.Now + " ========= getRootFolder() - 5 - Vai criar estrutura: " + this.ecmFolder);
                result = findCreateLastFolderInPath(result, this.ecmFolder);
            }

            LOG.imprimeLog(System.DateTime.Now + " ========= getRootFolder() - 6 - root folder: " + result.documentId + " - " + result.documentDescription);
            return result;
        }

        private ECMFolderService.documentDto findCreateLastFolderInPath(ECMFolderService.documentDto startFolder, String basePath)
        {
            LOG.imprimeLog(System.DateTime.Now + " =========== findCreateLastFolderInPath - 1 - basePath: " + basePath);
            string[] folders = parseFolder(basePath);
            ECMFolderService.documentDto result = startFolder;
            int ini = 0;
            if (startFolder == null)
            {
                LOG.imprimeLog(System.DateTime.Now + " =========== findCreateLastFolderInPath - 2");
                startFolder = getRootFolder(folders[0]);
                ini = 1;
            }
            LOG.imprimeLog(System.DateTime.Now + " =========== findCreateLastFolderInPath - 3 - ini: " + ini);
            if (startFolder == null)
            {
                LOG.imprimeLog(System.DateTime.Now + " =========== findCreateLastFolderInPath - 4: Lascou, nao achou o ROOT folder!");
                return null;
            }
            LOG.imprimeLog(System.DateTime.Now + " =========== findCreateLastFolderInPath - 5 - root folder: " + startFolder.documentId + " = " + startFolder.documentDescription);

            for (int i = ini; i < folders.Length; i++)
            {
                LOG.imprimeLog(System.DateTime.Now + " =========== findCreateLastFolderInPath - 6 - tratando folder: " + folders[i]);
                ECMFolderService.documentDto folder = null;
                ECMFolderService.documentDto[] fdocs = folderService.getSubFolders(this.ecmLogin, this.ecmPassword, this.ecmCompany, result.documentId, this.ecmUser);
                for (int j = 0; j < fdocs.Length; j++)
                {
                    if (fdocs[j].documentDescription.Equals(folders[i]))
                    {
                        folder = fdocs[j];
                        LOG.imprimeLog(System.DateTime.Now + " =========== findCreateLastFolderInPath - 7 - Folder encontrado: " + folder.documentId + "=" + folder.documentDescription);
                        break;
                    }
                }
                if (folder == null)
                {
                    folder = createFolder(folders[i], result.documentId);
                    LOG.imprimeLog(System.DateTime.Now + " =========== findCreateLastFolderInPath - 8 - Folder criado: " + folder.documentId + "=" + folder.documentDescription);
                }
                result = folder;
            }
            //LOG.imprimeLog(System.DateTime.Now + " =========== findCreateLastFolderInPath - 9 - result: " + result);
            return result;
        }

        /**
         * Faz o parser do código do cliente para criar as pastas.
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
            LOG.imprimeLog(System.DateTime.Now + " ============= Estrutura Pastas: " + sb.ToString());
            string[] sArray = new string[result.Count];
            result.CopyTo(sArray, 0);
            return sArray;
        }

        private void pause()
        {
            System.Threading.Thread.Sleep(5000);
        }
    }
}
