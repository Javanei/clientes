using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Management;
using System.IO;

namespace ecmfiles
{
    class Program
    {
        static string ecmLogin = "adm";
        static string ecmPassword = "adm";
        static string ecmUser = null;
        static string ecmURL = "http://192.168.1.106:8080/webdesk";
        static int ecmCompany = 1;
        static int ecmRootFolder = 0;
        static int ecmDocumentId = 0;
        static string inputFile = null;
        static string outputFile = null;
        static string outputFolder = null;
        static string ecmFolder = null;
        static string action = null;
        static string ecmFile = null;
        static string ecmProxyServer = null;
        static int ecmProxyPort = 0;
        static string ecmProxyUser = null;
        static string ecmProxyPass = null;
        static ECM ecm;

        [STAThread]
        static void Main(string[] args)
        {
            LOG.imprimeLog(System.DateTime.Now + " ===== Versao: 2013/10/09 12:20");
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower().Equals("-ecmlogin") && i < (args.Length - 1))
                    {
                        ecmLogin = args[++i];
                        if (ecmUser == null)
                        {
                            ecmUser = ecmLogin;
                        }
                    }
                    else
                        if (args[i].ToLower().Equals("-ecmuser") && i < (args.Length - 1))
                        {
                            ecmUser = args[++i];
                        }
                        else
                            if (args[i].ToLower().Equals("-ecmpassword") && i < (args.Length - 1))
                            {
                                ecmPassword = args[++i];
                            }
                            else
                                if (args[i].ToLower().Equals("-ecmurl") && i < (args.Length - 1))
                                {
                                    ecmURL = args[++i];
                                }
                                else
                                    if (args[i].ToLower().Equals("-ecmcompany") && i < (args.Length - 1))
                                    {
                                        ecmCompany = System.Convert.ToInt32(args[++i], 10);
                                    }
                                    else
                                        if (args[i].ToLower().Equals("-ecmrootfolder") && i < (args.Length - 1))
                                        {
                                            ecmRootFolder = System.Convert.ToInt32(args[++i], 10);
                                        }
                                        else
                                            if (args[i].ToLower().Equals("-action") && i < (args.Length - 1))
                                            {
                                                action = args[++i];
                                            }
                                            else
                                                if (args[i].ToLower().Equals("-inputfile") && i < (args.Length - 1))
                                                {
                                                    inputFile = args[++i];
                                                }
                                                else
                                                    if (args[i].ToLower().Equals("-outputfile") && i < (args.Length - 1))
                                                    {
                                                        outputFile = args[++i];
                                                    }
                                                    else
                                                        if (args[i].ToLower().Equals("-outputfolder") && i < (args.Length - 1))
                                                        {
                                                            outputFolder = args[++i];
                                                        }
                                                        else
                                                            if (args[i].ToLower().Equals("-ecmfolder") && i < (args.Length - 1))
                                                            {
                                                                ecmFolder = args[++i];
                                                            }
                                                            else
                                                                if (args[i].ToLower().Equals("-ecmfile") && i < (args.Length - 1))
                                                                {
                                                                    ecmFile = args[++i];
                                                                }
                                                                else
                                                                    if (args[i].ToLower().Equals("-ecmdocumentid") && i < (args.Length - 1))
                                                                    {
                                                                        ecmDocumentId = System.Convert.ToInt32(args[++i], 10);
                                                                    }
                                                                    else
                                                                        if (args[i].ToLower().Equals("-ecmproxyserver") && i < (args.Length - 1))
                                                                        {
                                                                            ecmProxyServer = args[++i];
                                                                        }
                                                                        else
                                                                            if (args[i].ToLower().Equals("-ecmproxyport") && i < (args.Length - 1))
                                                                            {
                                                                                ecmProxyPort = System.Convert.ToInt32(args[++i], 10);
                                                                            }
                                                                            else
                                                                                if (args[i].ToLower().Equals("-ecmproxyuser") && i < (args.Length - 1))
                                                                                {
                                                                                    ecmProxyUser = args[++i];
                                                                                }
                                                                                else
                                                                                    if (args[i].ToLower().Equals("-ecmproxypass") && i < (args.Length - 1))
                                                                                    {
                                                                                        ecmProxyPass = args[++i];
                                                                                    }

                }
            }

            LOG.imprimeLog(System.DateTime.Now + " ===== URL ECM: " + ecmURL);
            LOG.imprimeLog(System.DateTime.Now + " ===== Action: " + action);
            LOG.imprimeLog(System.DateTime.Now + " ===== ecmRootFolder: " + ecmRootFolder);
            LOG.imprimeLog(System.DateTime.Now + " ===== ecmFolder: " + ecmFolder);
            LOG.imprimeLog(System.DateTime.Now + " ===== ecmProxyServer: " + ecmProxyServer);
            LOG.imprimeLog(System.DateTime.Now + " ===== ecmProxyPort: " + ecmProxyPort);
            LOG.imprimeLog(System.DateTime.Now + " ===== ecmUser: " + ecmUser);
            LOG.imprimeLog(System.DateTime.Now + " ===== ecmProxyUser: " + ecmProxyUser);
            ecm = new ECM(ecmLogin, ecmPassword, ecmUser, ecmURL, ecmCompany, ecmFolder, ecmRootFolder, ecmProxyServer, ecmProxyPort, ecmProxyUser, ecmProxyPass);
            LOG.imprimeLog(System.DateTime.Now + " ===== Instanciou ECM");

            Boolean erro = false;
            try
            {
                if (action != null)
                {
                    if (action.Equals("listFiles"))
                    {
                        if (ecmFolder == null)
                        {
                            throw new Exception("A pasta base precisa ser informada");
                        }
                        listECMFiles();
                    }
                    else if (action.Equals("upload"))
                    {
                        if (ecmFolder == null)
                        {
                            throw new Exception("A pasta base precisa ser informada");
                        }
                        if (inputFile == null)
                        {
                            throw new Exception("O arquivo a ser feito upload (inputFile) precisa ser informado");
                        }
                        ecm.uploadDocument(inputFile);
                        listECMFiles();
                    }
                    else if (action.Equals("delete"))
                    {
                        if (ecmFolder == null)
                        {
                            throw new Exception("A pasta base precisa ser informada");
                        }
                        if (ecmFile == null && ecmDocumentId <= 0)
                        {
                            throw new Exception("O arquivo a ser excluido precisa ser informado");
                        }
                        if (ecmFile != null)
                        {
                            ecm.deleteDocument(ecmFile);
                        }
                        else
                        {
                            ecm.deleteDocument(ecmDocumentId);
                        }
                        listECMFiles();
                    }
                    else if (action.Equals("view"))
                    {
                        if (ecmFolder == null)
                        {
                            throw new Exception("A pasta base precisa ser informada");
                        }
                        if (ecmFile == null && ecmDocumentId <= 0)
                        {
                            throw new Exception("O arquivo a ser visualizado precisa ser informado");
                        }
                        if (ecmFile != null)
                        {
                            ecm.viewDocument(ecmFile);
                        }
                        else
                        {
                            ecm.viewDocument(ecmDocumentId);
                        }
                    }
                    else if (action.Equals("convert"))
                    {
                        ecm.convert();
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.imprimeLog(System.DateTime.Now + " Error=" + ex.Message);
                LOG.imprimeLog("StackTrace: " + ex.StackTrace);
                erro = true;
            }
        }

        private static void listECMFiles()
        {
            ECMFolderService.documentDto[] files = ecm.listFolder();
            StreamWriter SW = null;
            if (outputFile != null)
            {
                SW = System.IO.File.CreateText(outputFile);
                SW.WriteLine("id|version|type|name");
            }
            if (files != null && files.Length > 0)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    if (!files[i].documentType.Equals("2"))
                        continue;

                    LOG.imprimeLog(System.DateTime.Now + " ===== Arquivo: "
                        + "id=" + files[i].documentId
                        + ", desc=" + files[i].documentDescription
                        + ", version=" + files[i].version
                        + ", documentType=" + files[i].documentType
                        + ", documentTypeId=" + files[i].documentTypeId
                        );
                    if (SW != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(files[i].documentId);
                        sb.Append("|").Append(files[i].version);
                        sb.Append("|").Append(files[i].documentType);
                        sb.Append("|").Append(files[i].documentDescription);
                        SW.WriteLine(sb.ToString());
                    }
                }
            }
            else
            {
                LOG.imprimeLog(System.DateTime.Now + " ===== NENHUM documento encontrado");
            }

            if (SW != null)
            {
                SW.Close();
            }
        }

        /*private static string decode(String name)
        {
            Encoding enc1 = System.Text.Encoding.Default;
            Encoding enc2 = System.Text.Encoding.GetEncoding(850);
            byte[] b = enc1.GetBytes(name);
            string result = enc2.GetString(b);
            LOG.imprimeLog(System.DateTime.Now + " ============= " + name + " = " + result);
            return result;
        }*/
    }
}
