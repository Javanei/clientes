﻿using System;
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
            docService.Url = ecmURL + "/DocumentService";
            if (proxy)
                docService.Proxy = WebRequest.GetSystemWebProxy();
            folderService = new ECMFolderService.FolderServiceService();
            folderService.Url = ecmURL + "/FolderService";
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
                if (rootFolder == null) {
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
            LOG.imprimeLog(System.DateTime.Now + " ===== Excluindo Item do ECM: " + itemCode);
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
                            LOG.imprimeLog(System.DateTime.Now + " ===== Excluindo no ECM: " + files[i].phisicalFile);
                            docService.deleteDocument(ecmUser, ecmPassword, ecmCompany, files[i].documentId, ecmLogin);
                            LOG.imprimeLog(System.DateTime.Now + " ===== Excluiu no ECM: " + files[i].phisicalFile);
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
                    LOG.imprimeLog(System.DateTime.Now + " ===== Excluindo no ECM: " + fileName);
                    docService.deleteDocument(ecmUser, ecmPassword, ecmCompany, exist.documentId, ecmLogin);
                    LOG.imprimeLog(System.DateTime.Now + " ===== Excluiu no ECM: " + fileName);
                }
            }
        }

        /**
         * Faz o download dos arquivos de uma pasta do ECM.
         */
        public Dictionary<string, string> downloadECMDocuments(string itemCode, string baseDir, Dictionary<string, string> fileProps)
        {
            //ECMFolderService.documentDto folder = getECMChild(itemCode, rootFolder.documentId);
            ECMFolderService.documentDto folder = findECMFolderChild(rootFolder.documentId, itemCode);
            if (folder != null)
            {
                ECMFolderService.documentDto[] files = getECMChildren(folder.documentId);
                SetProperty(fileProps, "0", "False=Extract");
                if (files != null && files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        //LOG.imprimeLog(System.DateTime.Now + " ====== TIPO: " + files[i].documentType);
                        if (files[i].documentType.Equals("2")) {
                            LOG.imprimeLog(System.DateTime.Now + " ====== Arquivo a baixar do ECM: " + files[i].phisicalFile);
                            byte[] b = docService.getDocumentContent(ecmLogin, ecmPassword, ecmCompany, files[i].documentId, ecmUser, files[i].version, files[i].phisicalFile);
                            if (b == null)
                            {
                                throw new Exception("Nao conseguiu baixar do ECM o documento " + files[i].documentId);
                            }
                            string destFile = baseDir + "\\" + files[i].phisicalFile;

                            if (files[i].phisicalFile.IndexOf('.') > 0)
                            {
                                string s1 = files[i].phisicalFile.Substring(0, files[i].phisicalFile.LastIndexOf('.'));
                                if (s1.IndexOf('-') > 0)
                                {
                                    s1 = s1.Substring(s1.LastIndexOf('-') + 1);
                                    SetProperty(fileProps, s1, "OP");
                                }
                            }

                            FileStream fs = new FileStream(destFile, FileMode.CreateNew, FileAccess.Write);
                            fs.Write(b, 0, b.Length);
                            fs.Flush();
                            fs.Close();
                        } else {
                            LOG.imprimeLog(System.DateTime.Now + " ====== Outro arquivo ECM: " + files[i].documentType + " -> " + files[i].phisicalFile);
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
            //TODO: Tratar para mandar pro ECM apenas os desenhos que devem ser impressos.
            string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);

            //TODO: ECMFolderService.documentDto exist = getECMChild(fileName, folder);
            ECMFolderService.documentDto exist = getECMChild(fileName, folder.documentId);

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
                LOG.imprimeLog(System.DateTime.Now + " ======= Criando um novo arquivo no ECM: " + fileName);
                docService.createSimpleDocument(ecmLogin, ecmPassword, ecmCompany, folder.documentId, ecmUser, fileName, attach);
            }
            else
            {
                // Arquivo existente, atualiza.
                LOG.imprimeLog(System.DateTime.Now + " ======= Atualizando um arquivo no ECM: " + fileName);
                docService.updateSimpleDocument(ecmLogin, ecmPassword, ecmCompany, exist.documentId, ecmUser, fileName, attach);
                LOG.imprimeLog(System.DateTime.Now + " ======= Arquivo atualizado no ECM: " + fileName);
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
                    Program.sendMail("ERRO Conversao desenho", "O ECM falhou, nao foi possivel fazer upload de um arquivo: " + filePath);
                    throw new Exception("Nao conseguiu fazer upload para o ECM do arquivo '" + filePath + "' [uploadECMDocument()]");
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
                throw new Exception("Nao conseguiu criar uma pasta no ECM [createECMFolder()]");
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
                folder = getECMChild(s, pId);
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
                throw new Exception("Nao conseguiu ler a estrutura de pastas do ECM [getECMChild()]");
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
        private void SetProperty(Dictionary<string, string> d, string key, string value)
        {
            if (d.ContainsKey(key))
                d.Remove(key);
            d.Add(key, value);
        }

        private void pause()
        {
            System.Threading.Thread.Sleep(5000);
        }
    }
}
