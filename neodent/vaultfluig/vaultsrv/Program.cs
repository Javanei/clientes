using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Ionic.Zip;
using System.Xml;

using System.Management;

using Microsoft.Web.Services3;
using System.Net;
using System.Net.Mail;
//using Autodesk.Connectivity.WebServices;
using Autodesk.Connectivity.WebServices.DocumentSvc;
using Autodesk.Connectivity.WebServices.PropertySvc;
using Autodesk.Connectivity.WebServices.SecuritySvc;
using Autodesk.Connectivity.WebServicesTools;


//using Autodesk.Connectivity.WebServices;
using ADSK = Autodesk.Connectivity.WebServices;
using Autodesk.Connectivity.WebServices;

namespace vaultsrv
{

    public class Program
    {
        /*enum SrchOperator : long
        {
            Contains = 1,
            DoesNotContain = 2,
            IsExactly = 3,
            IsEmpty = 4,
            IsNotEmpty = 5,
            GreatherThan = 6,
            GreatherThanOrEqualTo = 7,
            LessThan = 8,
            LessThanOrEqualTo = 9,
            NotEqualTo = 10
        };*/

        private static long MAX_FILE_SIZE = 45 * 1024 * 1024; // 45 MB 

        private static string user = "cbueno";
        private static string pass = "neodent1011";
        private static string server = "vault.neodent.com.br";
        private static string vault = "neodent";
        private static string item = null;
        private static int propid = 10;
        private static int searchType = 2;
        private static string downdir = "c:\\temp\\";
        private static Boolean download = true;
        private static Boolean listall = false;
        private static String checkInDate = null;
        private static Boolean converter = false;
        private static Boolean delFiles = false;
        private static Dictionary<string, string> config;
        private static Dictionary<string, string> reconvert;
        private static Dictionary<string, string> reconvertNew = new Dictionary<string, string>();
        private static string pdfCommand = "p2iagent.exe";
        private static string dwfCommand = "C:\\Arquivos de programas\\Autodesk\\Autodesk Design Review\\DesignReview.exe";
        private static string imageDir = "C:\\Temp\\ImagePrinter";
        private static long[] folderIds = null;
        private static Boolean single = false;
        private static string printerName = "ImagePrinter";
        private static bool erro = false;
        private static string imageType = "OP";
        // Email
        private static int mailPort = 0;
        private static string mailHost = null;
        private static string mailSender = null;
        private static string mailUser = null;
        private static string mailPassword = null;
        private static string mailTO = null;
        private static bool mailEnableSSL = true;
        // ECM
        static string ecmLogin = "adm";
        static string ecmPassword = "adm";
        static string ecmUser = null;
        static string ecmURL = "http://192.168.1.106:8080/webdesk";
        static int ecmCompany = 1;
        static int ecmRootFolder = 5;
        static bool ecmEnabled = false;
        static bool ecmDownload = false;
        static bool ecmDelJpg = false;
        static ECM ecm;
        static Boolean ecmUpload = true;
        static Boolean ecmProxy = true;
        static WebServiceManager serviceManager = (WebServiceManager)null;
        // Controla se está imprimindo os desenhos de desenv
        static Boolean lDesenv = false;
        // Controla se está imprimento os desenhos de preset
        static Boolean lPreset = false;
        // Controla a lista dos desenhos que estão em desenv por estar em checkout
        List<String> lCheckout = new List<String>();

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("-d") && i < (args.Length - 1))
                    {
                        downdir = args[++i];
                        if (!downdir.EndsWith("\\"))
                        {
                            downdir = downdir + "\\";
                        }
                    }
                }
            }

            // Le o arquivo de configuração
            //config = ReadPropertyFile(Directory.GetCurrentDirectory() + "\\vaultsrv.conf");
            LOG.imprimeLog(System.DateTime.Now + " === Vai ler o arquivo de configuracao=" + downdir + "vaultsrv.conf");
            config = ReadPropertyFile(downdir + "vaultsrv.conf");
            if (config != null)
            {
                LOG.imprimeLog(System.DateTime.Now + " ============ VAULTSRV.CONF ENCONTRADO");
                mailPort = GetProperty(config, "mailPort") != null ? System.Convert.ToInt32(GetProperty(config, "mailPort"), 10) : 0;
                mailHost = GetProperty(config, "mailHost");
                mailSender = GetProperty(config, "mailSender");
                mailUser = GetProperty(config, "mailUser");
                mailPassword = GetProperty(config, "mailPassword");
                mailTO = GetProperty(config, "mailTO");
                mailEnableSSL = GetProperty(config, "mailEnableSSL") != null && GetProperty(config, "mailEnableSSL").Equals("true");
            }

            // Le arquivo com as imagens que apresentaram erro na conversao.
            reconvert = ReadPropertyFile("reconvert.conf");

            SetProperty(config, "ultimaExecucao", DateTime.Now.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));

            // Parse dos parametros
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("-ecmlogin") && i < (args.Length - 1))
                    {
                        ecmLogin = args[++i];
                        if (ecmUser == null)
                        {
                            ecmUser = ecmLogin;
                        }
                    }
                    else
                        if (args[i].Equals("-ecmuser") && i < (args.Length - 1))
                        {
                            ecmUser = args[++i];
                        }
                        else
                            if (args[i].Equals("-ecmpassword") && i < (args.Length - 1))
                            {
                                ecmPassword = args[++i];
                            }
                            else
                                if (args[i].Equals("-ecmurl") && i < (args.Length - 1))
                                {
                                    ecmURL = args[++i];
                                }
                                else
                                    if (args[i].Equals("-ecmcompany") && i < (args.Length - 1))
                                    {
                                        ecmCompany = System.Convert.ToInt32(args[++i], 10);
                                    }
                                    else
                                        if (args[i].Equals("-ecmrootfolder") && i < (args.Length - 1))
                                        {
                                            ecmRootFolder = System.Convert.ToInt32(args[++i], 10);
                                        }
                                        else
                                            if (args[i].Equals("-ecmdeljpg"))
                                            {
                                                ecmDelJpg = true;
                                            }
                                            else

                                                if (args[i].Equals("-i") && i < (args.Length - 1))
                                                {
                                                    propid = System.Convert.ToInt32(args[++i], 10);
                                                }
                                                else
                                                    if (args[i].Equals("-u") && i < (args.Length - 1))
                                                    {
                                                        user = args[++i];
                                                    }
                                                    else
                                                        if (args[i].Equals("-p") && i < (args.Length - 1))
                                                        {
                                                            pass = args[++i];
                                                        }
                                                        else
                                                            if (args[i].Equals("-imagetype") && i < (args.Length - 1))
                                                            {
                                                                if (args[++i].ToLower().Equals("anvisa"))
                                                                    imageType = "ANVISA_";
                                                                else if (args[++i].ToLower().Equals("fda"))
                                                                    imageType = "FDA_";
                                                                else if (args[++i].ToLower().Equals("des"))
                                                                    imageType = "DES_";
                                                                pass = args[++i];
                                                            }
                                                            else
                                                                if (args[i].Equals("-h") && i < (args.Length - 1))
                                                                {
                                                                    server = args[++i];
                                                                }
                                                                else
                                                                    if (args[i].Equals("-v") && i < (args.Length - 1))
                                                                    {
                                                                        vault = args[++i];
                                                                    }
                                                                    else
                                                                        if (args[i].Equals("-s") && i < (args.Length - 1))
                                                                        {
                                                                            searchType = System.Convert.ToInt32(args[++i], 10);
                                                                        }
                                                                        else
                                                                            if (args[i].Equals("-printer"))
                                                                            {
                                                                                if (i < args.Length - 1)
                                                                                {
                                                                                    printerName = args[++i];
                                                                                }
                                                                            }
                                                                            else
                                                                                if (args[i].Equals("-single"))
                                                                                {
                                                                                    single = true;
                                                                                }
                                                                                else
                                                                                    if (args[i].Equals("-d") && i < (args.Length - 1))
                                                                                    {
                                                                                        downdir = args[++i];
                                                                                        if (!downdir.EndsWith("\\"))
                                                                                        {
                                                                                            downdir = downdir + "\\";
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                        if (args[i].Equals("-nd"))
                                                                                        {
                                                                                            download = false;
                                                                                        }
                                                                                        else
                                                                                            if (args[i].Equals("-la"))
                                                                                            {
                                                                                                listall = true;
                                                                                            }
                                                                                            else
                                                                                                if (args[i].Equals("-ci"))
                                                                                                {
                                                                                                    if (i < args.Length - 1)
                                                                                                    {
                                                                                                        checkInDate = args[++i];
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        checkInDate = GetProperty(config, "LastCheckInDate");
                                                                                                        if (checkInDate == null || checkInDate == "")
                                                                                                        {
                                                                                                            checkInDate = new DateTime(1980 /*agora.Year*/, 1 /*agora.Month*/, 1 /*agora.Day*/, 0 /*agora.Hour*/, 0 /*agora.Minute*/, 0 /*agora.Second*/, 0 /*agora.Millisecond*/).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss");
                                                                                                        }
                                                                                                    }
                                                                                                    SetProperty(config, "LastCheckInDate", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                                                                                                }
                                                                                                else
                                                                                                    if (args[i].Equals("-cv"))
                                                                                                    {
                                                                                                        converter = true;
                                                                                                        download = true;
                                                                                                    }
                                                                                                    else
                                                                                                        if (args[i].Equals("-del"))
                                                                                                        {
                                                                                                            delFiles = true;
                                                                                                        }
                                                                                                        else
                                                                                                            if (args[i].Equals("-nolog"))
                                                                                                            {
                                                                                                                LOG.geraLog = false;
                                                                                                            }
                                                                                                            else
                                                                                                                if (args[i].Equals("-imgdir"))
                                                                                                                {
                                                                                                                    if (i < args.Length - 1)
                                                                                                                    {
                                                                                                                        imageDir = args[++i];
                                                                                                                    }
                                                                                                                }
                                                                                                                else
                                                                                                                    if (args[i].Equals("-dreview"))
                                                                                                                    {
                                                                                                                        if (i < args.Length - 1)
                                                                                                                        {
                                                                                                                            dwfCommand = args[++i];
                                                                                                                        }
                                                                                                                    }
                                                                                                                    else
                                                                                                                    {
                                                                                                                        item = args[i];
                                                                                                                    }
                }
            }

            if (ecmLogin != null
                && ecmPassword != null
                && ecmUser != null
                && ecmURL != null
                && ecmCompany > 0
                && ecmRootFolder > 0)
            {
                LOG.imprimeLog(System.DateTime.Now + " === Integracao com ECM habilitada");
                ecmEnabled = true;
                if (item != null && !converter)
                {
                    LOG.imprimeLog(System.DateTime.Now + " === Download do ECM habilitado");
                    ecmDownload = true;
                }
            }

            //ADSK.SecurityService ss = null;
            try
            {
                serviceManager = new WebServiceManager((IWebServiceCredentials)new UserPasswordCredentials(Program.server, Program.vault, Program.user, Program.pass, true));
                /* Efetua o Login */
                /*imprimeLog("====== Usuario.: " + user);
                imprimeLog("====== Senha...: " + pass);
                imprimeLog("====== Servidor: " + server);
                imprimeLog("====== Base....: " + vault);*/

                if (ecmEnabled)
                {
                    ecm = new ECM(ecmLogin, ecmPassword, ecmUser, ecmURL, ecmCompany, ecmRootFolder, imageDir, ecmUpload, ecmProxy);
                    if (ecm == null)
                    {
                        throw new Exception("A conexao com o ECM falhou");
                    }
                    LOG.imprimeLog(System.DateTime.Now + " ==== Instanciou o ECM");
                }

                if (ecmDownload)
                {
                    LOG.imprimeLog(System.DateTime.Now + " ==== Deve baixar do ECM");

                    try
                    {
                        string DeCodigo = item.Replace(' ', '_');
                        String ItemDir = downdir + DeCodigo;
                        string DeCodigoLog = ItemDir + "\\" + DeCodigo + ".log";
                        Dictionary<string, string> fileProps = new Dictionary<string, string>();
                        DeleteFilesFromDir(ItemDir);
                        LOG.imprimeLog(System.DateTime.Now + " ==== Limpou diretorio=" + ItemDir);
                        LOG.imprimeLog(System.DateTime.Now + " ==== Vai baixar os desenhos do ECM");
                        fileProps = ecm.downloadECMDocuments(DeCodigo, ItemDir, fileProps);
                    }
                    catch (Exception ex)
                    {
                        sendMail("ERRO baixando do ECM o desenho '" + item + "'", ex.Message);
                    }
                    //LOG.imprimeLog(System.DateTime.Now + " ==== Vai gerar o log do desenho=" + DeCodigoLog);
                    //saveInfo(DeCodigoLog, fileProps);
                }
                else
                {
                    LOG.imprimeLog(System.DateTime.Now + " === Vai baixar do Vault");
                    LoginInfo loginInfo = new LoginInfo(Program.user, Program.pass, Program.server, Program.vault);
                    DocumentService documentService = serviceManager.DocumentService;

                    ADSK.Folder[] fld = documentService.FindFoldersByPaths(new string[] { "$/Neodent/Produção" });
                    //ADSK.Folder[] fldDesenv = documentService.FindFoldersByPaths(new string[] { "$/Neodent/Engenharia" });
                    ADSK.Folder[] fldPreset = documentService.FindFoldersByPaths(new string[] { "$/Neodent/Preset" });
                    do
                    {
                        /*if (lDesenv)
                        {
                            fld = fldDesenv;
                        }*/
                        if (lPreset)
                        {
                            fld = fldPreset;
                        }
                        if (fld != null)
                        {
                            foreach (ADSK.Folder f in fld)
                            {
                                folderIds = new long[1];
                                folderIds[0] = f.Id;
                            }
                        }
                        string filename = item + ".idw";

                        List<ADSK.File> fileList = new List<ADSK.File>();
                        List<BatchConvertItem> batchList = new List<BatchConvertItem>();

                        if (listall)
                        {
                            LOG.imprimeLog(System.DateTime.Now + " ==== Listando todos");
                            List<ADSK.File> fl = findByMatches(documentService, ".idw");
                            List<string> allf = new List<string>();

                            if (fl != null && fl.Count > 0)
                            {
                                foreach (ADSK.File file in fl)
                                {
                                    if (file.Name.EndsWith(".idw"))
                                    {
                                        string fcode = file.Name.Substring(0, file.Name.Length - 4);
                                        if (!allf.Contains(fcode))
                                        {
                                            allf.Add(fcode);
                                            fileList.Add(file);
                                        }
                                    }
                                }
                            }
                            fl = findByMatches(documentService, ".dwg");
                            if (fl != null && fl.Count > 0)
                            {
                                foreach (ADSK.File file in fl)
                                {
                                    if (file.Name.EndsWith(".pdf"))
                                    {
                                        string fcode = file.Name.Substring(0, file.Name.Length - 4);
                                        if (!allf.Contains(fcode))
                                        {
                                            allf.Add(fcode);
                                            fileList.Add(file);
                                        }
                                    }
                                }
                            }
                            fl = findByMatches(documentService, ".pdf");
                            if (fl != null && fl.Count > 0)
                            {
                                foreach (ADSK.File file in fl)
                                {
                                    if (file.Name.EndsWith(".pdf"))
                                    {
                                        string fcode = file.Name.Substring(0, file.Name.Length - 4);
                                        if (!allf.Contains(fcode))
                                        {
                                            allf.Add(fcode);
                                            fileList.Add(file);
                                        }
                                    }
                                }
                            }
                            fileList = null;
                        }
                        else if (checkInDate != null)
                        {
                            fileList = findByCheckinDate(documentService, checkInDate);

                            if (reconvert != null)
                            {
                                // Adiciona as imagens que deram problemas na conversao.
                                Dictionary<string, string>.KeyCollection keyColl = reconvert.Keys;
                                foreach (string s in keyColl)
                                {
                                    List<ADSK.File> fl = findByEquals(documentService, s);
                                    if (fl != null && fl.Count > 0)
                                    {
                                        foreach (ADSK.File file in fl)
                                        {
                                            if (file.Name.Equals(filename))
                                            {
                                                fileList.Add(file);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            List<ADSK.File> fl = null;
                            if (searchType == 2)
                            {
                                LOG.imprimeLog(System.DateTime.Now + " ==== Buscando desenho: " + filename);
                                fl = findByEquals(documentService, filename);
                                /*if (fl == null || fl.Count == 0)
                                {
                                    filename = item + ".dwg.dwf";
                                    fl = findByEquals(documentService, filename);
                                }*/
                                if (fl == null || fl.Count == 0)
                                {
                                    filename = item + ".dwg";
                                    LOG.imprimeLog(System.DateTime.Now + " ==== Buscando desenho: " + filename);
                                    fl = findByEquals(documentService, filename);
                                }
                                if (fl == null || fl.Count == 0)
                                {
                                    filename = item + ".pdf";
                                    LOG.imprimeLog(System.DateTime.Now + " ==== Buscando desenho: " + filename);
                                    fl = findByEquals(documentService, filename);
                                }
                            }
                            else
                            {
                                fl = findByMatches(documentService, item);
                            }
                            if (fl != null && fl.Count > 0)
                            {
                                foreach (ADSK.File file in fl)
                                {
                                    if (file.Name.Equals(filename))
                                    {
                                        fileList.Add(file);
                                    }
                                }
                            }
                        }

                        if (fileList != null && fileList.Count > 0)
                        {
                            foreach (ADSK.File file in fileList)
                            {
                                string nomeImagem = file.Name;
                                if (isReconvert(nomeImagem))
                                {
                                    sendMail("AVISO: Reconvertendo imagem: " + nomeImagem, "Uma nova tentativa esta sendo feita para converter a imagem " + nomeImagem);
                                }
                                try
                                {
                                    ADSK.File fileDown = null;
                                    LOG.imprimeLog(System.DateTime.Now + " Processando arquivo: " + file.Name);
                                    Boolean alterou = false;
                                    Boolean ren = false;

                                    string DeCodigo = file.Name.Replace(' ', '_');
                                    //if (DeCodigo.ToLower().EndsWith(".idw.dwf") || DeCodigo.ToLower().EndsWith(".dwg.dwf"))
                                    if (DeCodigo.ToLower().EndsWith(".idw") || DeCodigo.ToLower().EndsWith(".dwg"))
                                    {
                                        //DeCodigo = DeCodigo.Substring(0, DeCodigo.Length - 8);
                                        DeCodigo = DeCodigo.Substring(0, DeCodigo.Length - 4);
                                        List<ADSK.File> fl = findByEquals(documentService, file.Name + ".dwf");
                                        if (fl != null && fl.Count > 0)
                                        {
                                            fileDown = fl.ToArray()[0];
                                        }
                                    }
                                    else
                                    {
                                        if (DeCodigo.EndsWith(".dwf"))
                                        {
                                            DeCodigo = DeCodigo.Substring(0, DeCodigo.Length - 4);
                                            fileDown = file;
                                        }
                                        else if (DeCodigo.EndsWith(".pdf"))
                                        {
                                            DeCodigo = DeCodigo.Substring(0, DeCodigo.Length - 4);
                                            fileDown = file;
                                        }
                                    }
                                    if (fileDown == null)
                                    {
                                        LOG.imprimeLog(System.DateTime.Now + " ====== Nao achou DWF, ignorando arquivo");
                                        continue;
                                    }

                                    String ItemDir = downdir + DeCodigo;
                                    LOG.imprimeLog(System.DateTime.Now + " ===== ItemDir=" + ItemDir);

                                    if (!Directory.Exists(ItemDir))
                                    {
                                        Directory.CreateDirectory(ItemDir);
                                        LOG.imprimeLog(System.DateTime.Now + " ==== Criou diretorio=" + ItemDir);
                                        alterou = true;
                                    }

                                    String ItemFile = ItemDir + "\\" + fileDown.Name;
                                    string DeCodigoLog = ItemDir + "\\" + DeCodigo + ".log";
                                    LOG.imprimeLog(System.DateTime.Now + " ==== Arquivo de log=" + DeCodigoLog);

                                    if (!System.IO.File.Exists(DeCodigoLog))
                                    {
                                        LOG.imprimeLog(System.DateTime.Now + " ==== Arquivo de log ainda nao existia=" + DeCodigoLog);
                                        alterou = true;
                                    }
                                    else
                                    {
                                        Dictionary<string, string> d = ReadPropertyFile(DeCodigoLog);
                                        if (GetProperty(d, "CkInDate") == null
                                            || !GetProperty(d, "CkInDate").Equals(file.CkInDate.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss"))
                                            || GetProperty(d, "Cksum") == null
                                            || !GetProperty(d, "Cksum").Equals(file.Cksum.ToString())
                                            || GetProperty(d, "FileSize") == null
                                            || !GetProperty(d, "FileSize").Equals(file.FileSize.ToString())
                                            )
                                        {
                                            alterou = true;
                                        }
                                    }

                                    Dictionary<string, string> fileProps = new Dictionary<string, string>();

                                    //filename = file.Name;

                                    if (download && alterou)
                                    {
                                        DeleteFilesFromDir(ItemDir);
                                        LOG.imprimeLog(System.DateTime.Now + " ==== Limpou diretorio=" + ItemDir);
                                        DownloadFile(fileDown, ItemFile, documentService);
                                        LOG.imprimeLog(System.DateTime.Now + " ==== Baixou arquivo=" + ItemFile);
                                        if (ItemFile.ToLower().EndsWith(".dwf"))
                                        {
                                            try
                                            {
                                                fileProps = extract(ItemDir, ItemFile, fileProps);
                                                if (converter)
                                                {
                                                    if (!single)
                                                    {
                                                        StringBuilder sb = new StringBuilder();
                                                        sb.Append("<configuration_file>");
                                                        sb.Append(AddFileToBpj(ItemFile));
                                                        sb.Append("</configuration_file>");

                                                        StreamWriter sw = new StreamWriter(ItemFile + ".bpj");
                                                        sw.Write(sb.ToString());
                                                        sw.Flush();
                                                        sw.Close();
                                                        LOG.imprimeLog(System.DateTime.Now + " === Arquivo bpj: " + sb.ToString());

                                                        /*if (System.IO.File.Exists(imageDir + "\\execok.exe.txt"))
                                                        {
                                                            DeleteFile(imageDir + "\\execok.exe.txt");
                                                        }*/

                                                        bool bOk = false;
                                                        System.Diagnostics.Process proc = new System.Diagnostics.Process();
                                                        proc.StartInfo.FileName = dwfCommand;
                                                        proc.StartInfo.Arguments = /*"\"" +*/ ItemFile + ".bpj" /*+ "\""*/;
                                                        proc.StartInfo.UseShellExecute = false;
                                                        proc.StartInfo.RedirectStandardOutput = false;
                                                        LOG.imprimeLog(System.DateTime.Now + " ==== Vai converter=" + dwfCommand + " " + proc.StartInfo.Arguments);
                                                        proc.Start();
                                                        proc.WaitForExit(60000);
                                                        if (!proc.HasExited)
                                                        {
                                                            LOG.imprimeLog(System.DateTime.Now + " ====== ERRO executando DesignReview, tentando fechar");
                                                            proc.Kill();
                                                        }
                                                        else
                                                        {
                                                            // Espera a imagem começar a ser gerada pelo driver.
                                                            for (int i = 0; i < 30; i++)
                                                            {
                                                                LOG.imprimeLog(System.DateTime.Now + " ====== Aguardando inicio da impressao: " + i);
                                                                if (HasPrintJobs(printerName))
                                                                {
                                                                    i = 30;
                                                                    bOk = true;
                                                                }
                                                                else
                                                                {
                                                                    String[] files = Directory.GetFiles(imageDir);
                                                                    if (files != null && files.Length > 0)
                                                                    {
                                                                        LOG.imprimeLog(System.DateTime.Now + " ====== Nao achou impressao, mas achou arquivos");
                                                                        for (int j = 0; j < files.Length; j++)
                                                                        {
                                                                            if (files[j].ToLower().EndsWith(".jpg"))
                                                                            {
                                                                                j = files.Length;
                                                                                bOk = true;
                                                                                i = 30;
                                                                            }
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        System.Threading.Thread.Sleep(1000);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        if (!bOk)
                                                        {
                                                            throw new Exception("Nao conseguiu iniciar a impressao para gerar os JPG");
                                                        }

                                                        // Espera a imagem terminar de ser gerada pelo driver
                                                        LOG.imprimeLog(System.DateTime.Now + " ===== Diretorio para salvar imagens: " + imageDir);
                                                        bOk = false;
                                                        for (int i = 0; i < 30; i++)
                                                        {
                                                            LOG.imprimeLog(System.DateTime.Now + " ====== Aguardando término da impressao: " + i);
                                                            if (!HasPrintJobs(printerName))
                                                            {
                                                                i = 30;
                                                                bOk = true;
                                                            }
                                                            else
                                                            {
                                                                System.Threading.Thread.Sleep(1000);
                                                            }
                                                        }
                                                        if (!bOk)
                                                        {
                                                            throw new Exception("Nao conseguiu concluir a impressao para gerar os JPG");
                                                        }
                                                        LOG.imprimeLog(System.DateTime.Now + " ==== Converteu");

                                                        ren = true;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                LOG.imprimeLog(System.DateTime.Now + " =========== Error extraindo arquivos=" + ex.Message);
                                                if (isReconvert(nomeImagem))
                                                {
                                                    sendMail("ERRO RECONVERTENDO desenho: " + nomeImagem, ex.Message);
                                                }
                                                else
                                                {
                                                    addReconvert(nomeImagem);
                                                    sendMail("ERRO convertendo desenho: " + nomeImagem, ex.Message);
                                                }
                                                continue;
                                            }
                                        }
                                        else if (ItemFile.ToLower().EndsWith(".pdf"))
                                        {
                                            if (converter)
                                            {
                                                LOG.imprimeLog(System.DateTime.Now + " =========== Vai converter um PDF. Comando: " + pdfCommand);
                                                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                                                proc.StartInfo.FileName = pdfCommand;
                                                proc.StartInfo.Arguments = "--dest=\"" + imageDir + "\" --format=1 --src=\"" + ItemFile + "\"";
                                                proc.StartInfo.UseShellExecute = false;
                                                proc.StartInfo.RedirectStandardOutput = false;
                                                LOG.imprimeLog(System.DateTime.Now + " =========== Iniciando conversao do PDF: " + proc.StartInfo.Arguments);
                                                proc.Start();
                                                proc.WaitForExit();
                                                LOG.imprimeLog(System.DateTime.Now + " =========== PDF convertido");
                                                int cont = 0;
                                                String[] files = Directory.GetFiles(imageDir);
                                                if (files != null && files.Length > 0)
                                                {
                                                    foreach (string f in files)
                                                    {
                                                        if (f.EndsWith(".jpg"))
                                                        {
                                                            cont++;
                                                            if (lDesenv)
                                                            {
                                                                SetProperty(fileProps, "" + cont, "DES=" + cont);
                                                            }
                                                            else if (lPreset)
                                                            {
                                                                SetProperty(fileProps, "" + cont, "PS=" + cont);
                                                            }
                                                            else
                                                            {
                                                                SetProperty(fileProps, "" + cont, "OP=" + cont);
                                                            }
                                                        }
                                                    }
                                                }

                                                ren = true;
                                            }
                                        }
                                        else
                                        {
                                            LOG.imprimeLog(System.DateTime.Now + " ======= ERRO: " + ItemFile + " NAO ESTA EM UM FORMATO CONHECIDO");
                                        }
                                    }

                                    if (single && ItemFile.ToLower().EndsWith(".dwf") && download && alterou)
                                    {
                                        BatchConvertItem it = new BatchConvertItem();
                                        it.ItemFile = ItemFile;
                                        it.file = file;
                                        it.fileDown = fileDown;
                                        it.fileLog = DeCodigoLog;
                                        it.props = fileProps;
                                        it.imageDir = imageDir;
                                        it.ItemDir = ItemDir;
                                        it.DeCodigo = DeCodigo;
                                        batchList.Add(it);
                                    }
                                    else
                                    {
                                        if (ecmEnabled && download && alterou)
                                        {
                                            ecm.deleteECMDocument(DeCodigo);
                                        }
                                        if (alterou || file.CheckedOut)
                                        {
                                            LOG.imprimeLog(System.DateTime.Now + " ==== Vai gerar o log do desenho (1)=" + DeCodigoLog);
                                            showInfo(file, fileDown, DeCodigoLog, fileProps);
                                            if (ecmEnabled)
                                            {
                                                LOG.imprimeLog(System.DateTime.Now + " ==== Vai fazer upload para o ECM");
                                                ecm.updateECMDocument(DeCodigo, DeCodigoLog);
                                            }

                                        }
                                        LOG.imprimeLog(System.DateTime.Now + " ==== download=" + download + ", alterou=" + alterou);
                                        if (download && alterou)
                                        {
                                            LOG.imprimeLog(System.DateTime.Now + " ==== Vai renomear os jpg");
                                            DeleteRenameFilesFromDir(imageDir, ItemDir, DeCodigo, delFiles, ren /*&& !fileDown.Name.EndsWith(".pdf")*/);
                                        }
                                    }


                                }
                                catch (Exception ex)
                                {
                                    if (isReconvert(nomeImagem))
                                    {
                                        sendMail("ERRO RECONVERTENDO desenho: " + nomeImagem, ex.Message);
                                    }
                                    else
                                    {
                                        addReconvert(nomeImagem);
                                        sendMail("ERRO convertendo desenho: " + nomeImagem, ex.Message);
                                    }
                                }
                            }
                        }

                        if (batchList != null && batchList.Count > 0)
                        {
                            // Divide em lotes
                            int MAX = 10;

                            while (batchList.Count > 0)
                            {
                                List<BatchConvertItem> bList = new List<BatchConvertItem>();

                                while (bList.Count < MAX && batchList.Count > 0)
                                {
                                    bList.Add(batchList.ToArray()[0]);
                                    batchList.RemoveAt(0);
                                }

                                StringBuilder sb = new StringBuilder();
                                sb.Append("<configuration_file>");
                                foreach (BatchConvertItem it in bList)
                                {
                                    sb.Append(AddFileToBpj(it.ItemFile));
                                }
                                sb.Append("</configuration_file>");

                                string convfile = downdir + "converte.bpj";

                                StreamWriter sw = new StreamWriter(convfile);
                                sw.Write(sb.ToString());
                                sw.Flush();
                                sw.Close();
                                LOG.imprimeLog(System.DateTime.Now + " === Arquivo bpj: " + sb.ToString());

                                /*if (System.IO.File.Exists(imageDir + "\\execok.exe.txt"))
                                {
                                    DeleteFile(imageDir + "\\execok.exe.txt");
                                }*/

                                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                                proc.StartInfo.FileName = dwfCommand;
                                proc.StartInfo.Arguments = convfile;
                                proc.StartInfo.UseShellExecute = false;
                                proc.StartInfo.RedirectStandardOutput = false;
                                LOG.imprimeLog(System.DateTime.Now + " ==== Vai converter=" + dwfCommand + " " + proc.StartInfo.Arguments);
                                proc.Start();
                                proc.WaitForExit();
                                // Espera a imagem ser gerada pelo driver
                                LOG.imprimeLog(System.DateTime.Now + " ===== Diretorio para salvar imagens: " + imageDir);
                                for (int i = 0; i < (30 * bList.Count); i++)
                                {
                                    LOG.imprimeLog(System.DateTime.Now + " ====== Aguardando inicio da impressao: " + i);
                                    if (HasPrintJobs(printerName))
                                    {
                                        i = 30 * bList.Count;
                                    }
                                    else
                                    {
                                        System.Threading.Thread.Sleep(1000);
                                    }
                                }
                                for (int i = 0; i < (30 * bList.Count); i++)
                                {
                                    LOG.imprimeLog(System.DateTime.Now + " ====== Aguardando término da impressao: " + i);
                                    if (!HasPrintJobs(printerName))
                                    {
                                        i = 30 * bList.Count;
                                    }
                                    else
                                    {
                                        System.Threading.Thread.Sleep(1000);
                                    }
                                }
                                LOG.imprimeLog(System.DateTime.Now + " ==== Converteu");

                                foreach (BatchConvertItem it in bList)
                                {
                                    LOG.imprimeLog(System.DateTime.Now + " ==== Vai gerar o log do desenho (2)=" + it.fileLog);
                                    /*if (ecmEnabled)
                                    {
                                        ecm.deleteECMDocument(basename);
                                    }*/
                                    showInfo(it.file, it.fileDown, it.fileLog, it.props);

                                    LOG.imprimeLog(System.DateTime.Now + " ==== Vai renomear os jpg");
                                    DeleteRenameFilesFromDir(it);
                                }
                            }
                        }
                        //lDesenv = (!lDesenv);
                        //lDesenv = false;
                        lPreset = (!lPreset);
                        //} while (lDesenv);
                    } while (lPreset);

                    //ss.SignOut();
                }
            }
            catch (Exception ex)
            {
                LOG.imprimeLog(System.DateTime.Now + " Error=" + ex.Message);
                LOG.imprimeLog("StackTrace: " + ex.StackTrace);
                erro = true;
                sendMail("ERRO convertendo desenhos", ex.Message);
            }
            if (!erro)
            {
                LOG.imprimeLog(System.DateTime.Now + " Vai salvar arquivo de configuracao=" + downdir + "vaultsrv.conf");
                WritePropertyFile(downdir + "vaultsrv.conf", config);
            }
            if (reconvertNew != null)
            {
                WritePropertyFile("reconvert.conf", reconvertNew);
            }

            //Console.WriteLine("Fim --------------------------------------");
            //Console.Write("Pressione enter para terminar");
            //Console.ReadLine();
        }

        /*private static void saveInfo(string filename, Dictionary<string, string> props)
        {
            WritePropertyFile(filename, props);
        }*/

        private static void showInfo(ADSK.File file, ADSK.File fileDown, String filename, Dictionary<string, string> props)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            SetProperty(d, "File", fileDown.Name);
            SetProperty(d, "CheckedOut", file.CheckedOut.ToString());
            SetProperty(d, "CkInDate", file.CkInDate.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss"));
            SetProperty(d, "Cksum", file.Cksum.ToString());
            //SetProperty(d, "Cloaked", file.Cloaked.ToString());
            SetProperty(d, "FileSize", file.FileSize.ToString());
            SetProperty(d, "FileStatus", file.FileStatus.ToString());
            SetProperty(d, "ModDate", file.ModDate.ToString());
            SetProperty(d, "CkOutFolderId", file.CkOutFolderId.ToString());
            SetProperty(d, "CkOutMach", file.CkOutMach);
            SetProperty(d, "CkOutSpec", file.CkOutSpec);
            SetProperty(d, "CkOutUserId", file.CkOutUserId.ToString());
            //SetProperty(d, "IsOnSite", file.IsOnSite.ToString());
            //SetProperty(d, "FileRev", file.FileRev.Label + "=" + file.FileRev.MaxFileId + "=" + file.FileRev.MaxRevId + "=" + file.FileRev.Order + "=" + file.FileRev.RevDefId + "=" + file.FileRev.RevId);

            string value = null;
            foreach (string k in props.Keys)
            {
                props.TryGetValue(k, out value);
                SetProperty(d, k, value);
            }
            LOG.imprimeLog(System.DateTime.Now + " ===== Salvando arquivo de log: " + filename);
            WritePropertyFile(filename, d);
        }

        public static List<ADSK.File> findByCheckinDate(DocumentService documentService, String checkinDate)
        {
            LOG.imprimeLog(System.DateTime.Now + " ===== findByCheckinDate: [" + checkinDate + "]");
            List<ADSK.File> fileList = new List<ADSK.File>();
            List<ADSK.File> fileListTmp = new List<ADSK.File>();
            List<string> allf = new List<string>();

            ADSK.PropDef prop = GetPropertyDefinition("CheckoutUserName", documentService);
            if (prop != null)
            {
                propid = (int)prop.Id;
                /* Faz a pesquisa */
                string bookmark = string.Empty;
                ADSK.SrchStatus status = null;

                ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
                conditions[0] = new ADSK.SrchCond();
                conditions[0].SrchOper = Condition.IS_NOT_EMPTY.Code;
                conditions[0].PropTyp = ADSK.PropertySearchType.SingleProperty;
                conditions[0].PropDefId = propid;

                prop = GetPropertyDefinition("ClientFileName", documentService);
                propid = (int)prop.Id;

                /*conditions[1] = new ADSK.SrchCond();
                conditions[1].SrchOper = Condition.CONTAINS.Code;
                conditions[1].SrchTxt = ".idw";
                conditions[1].PropTyp = ADSK.PropertySearchType.SingleProperty;
                conditions[1].PropDefId = propid;
                conditions[1].SrchRule = ADSK.SearchRuleType.May;

                conditions[2] = new ADSK.SrchCond();
                conditions[2].SrchOper = Condition.CONTAINS.Code;
                conditions[2].SrchTxt = ".pdf";
                conditions[2].PropTyp = ADSK.PropertySearchType.SingleProperty;
                conditions[2].PropDefId = propid;
                conditions[2].SrchRule = ADSK.SearchRuleType.May;

                conditions[3] = new ADSK.SrchCond();
                conditions[3].SrchOper = Condition.CONTAINS.Code;
                conditions[3].SrchTxt = ".dwg";
                conditions[3].PropTyp = ADSK.PropertySearchType.SingleProperty;
                conditions[3].PropDefId = propid;
                conditions[3].SrchRule = ADSK.SearchRuleType.May;*/

                while (status == null || fileListTmp.Count < status.TotalHits)
                {
                    ADSK.File[] files = documentService.FindFilesBySearchConditions(
                        conditions, /*SrchCond [] conditions*/
                        null, /*SrchSort [] sortConditions*/
                        folderIds, /*Long [] folderIds*/
                        true, /*Boolean recurseFolders*/
                        true, /*Boolean latestOnly*/
                        ref bookmark, /*[out] String bookmark*/
                        out status /*[out] SrchStatus searchstatus*/
                    );
                    if (files != null)
                    {
                        foreach (ADSK.File f in files)
                        {
                            fileListTmp.Add(f);
                            if (f.Name.ToLower().EndsWith(".idw"))
                            {
                                string fcode = f.Name.Substring(0, f.Name.Length - 4);
                                if (!allf.Contains(fcode))
                                {
                                    allf.Add(fcode);
                                    LOG.imprimeLog(System.DateTime.Now + " Adicionado via checkout =====-> " + f.Name + "(" + f.FileSize + ") - CkInDate: " + f.CkInDate.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));
                                    fileList.Add(f);
                                }
                            }
                        }
                        foreach (ADSK.File f in files)
                        {
                            if (f.Name.ToLower().EndsWith(".dwg"))
                            {
                                string fcode = f.Name.Substring(0, f.Name.Length - 4);
                                if (!allf.Contains(fcode))
                                {
                                    allf.Add(fcode);
                                    LOG.imprimeLog(System.DateTime.Now + " Adicionado via checkout =====-> " + f.Name + "(" + f.FileSize + ") - CkInDate: " + f.CkInDate.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));
                                    fileList.Add(f);
                                }
                            }
                        }
                        foreach (ADSK.File f in files)
                        {
                            if (f.Name.ToLower().EndsWith(".pdf"))
                            {
                                string fcode = f.Name.Substring(0, f.Name.Length - 4);
                                if (!allf.Contains(fcode))
                                {
                                    allf.Add(fcode);
                                    LOG.imprimeLog(System.DateTime.Now + " Adicionado via checkout =====-> " + f.Name + "(" + f.FileSize + ") - CkInDate: " + f.CkInDate.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));
                                    fileList.Add(f);
                                }
                            }
                        }
                        /*
                        foreach (ADSK.File f in files)
                        {
                            fileListTmp.Add(f);
                            if (f.Name.ToLower().EndsWith(".pdf") || f.Name.ToLower().EndsWith(".idw") || f.Name.ToLower().EndsWith(".dwg"))
                            {
                                LOG.imprimeLog(System.DateTime.Now + " Adicionado via checkout =====-> " + f.Name + "(" + f.FileSize + ")");
                                fileList.Add(f);
                            }
                        }
                        */
                    }
                }
            }


            /*ADSK.PropDef*/
            prop = GetPropertyDefinition("CheckInDate", documentService);
            if (prop != null)
            {
                fileListTmp = new List<ADSK.File>();
                propid = (int)prop.Id;
                /* Faz a pesquisa */
                string bookmark = string.Empty;
                ADSK.SrchStatus status = null;

                ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
                conditions[0] = new ADSK.SrchCond();
                conditions[0].SrchOper = Condition.GREATER_THAN_OR_EQUAL.Code /*(long)SrchOperator.GreatherThan*/;
                conditions[0].SrchTxt = checkinDate;
                conditions[0].PropTyp = ADSK.PropertySearchType.SingleProperty;
                conditions[0].PropDefId = propid;

                prop = GetPropertyDefinition("ClientFileName", documentService);
                propid = (int)prop.Id;

                /*conditions[1] = new ADSK.SrchCond();
                conditions[1].SrchOper = Condition.CONTAINS.Code;
                conditions[1].SrchTxt = ".idw";
                conditions[1].PropTyp = ADSK.PropertySearchType.SingleProperty;
                conditions[1].PropDefId = propid;
                conditions[1].SrchRule = ADSK.SearchRuleType.May;

                conditions[2] = new ADSK.SrchCond();
                conditions[2].SrchOper = Condition.CONTAINS.Code;
                conditions[2].SrchTxt = ".pdf";
                conditions[2].PropTyp = ADSK.PropertySearchType.SingleProperty;
                conditions[2].PropDefId = propid;
                conditions[2].SrchRule = ADSK.SearchRuleType.May;

                conditions[3] = new ADSK.SrchCond();
                conditions[3].SrchOper = Condition.CONTAINS.Code;
                conditions[3].SrchTxt = ".dwg";
                conditions[3].PropTyp = ADSK.PropertySearchType.SingleProperty;
                conditions[3].PropDefId = propid;
                conditions[3].SrchRule = ADSK.SearchRuleType.May;*/

                while (status == null || fileListTmp.Count < status.TotalHits)
                {
                    /*ADSK.FileFolder[] folders = documentService.FindFileFoldersBySearchConditions(
                        conditions,
                        null,
                        folderIds,
                        true,
                        true,
                        ref bookmark,
                        out status
                    );

                    if (folders != null)
                    {
                        foreach (ADSK.FileFolder f in folders)
                        {
                            //if (f.File != null)
                            //{
                            fileList.Add(f.File);
                            //}
                        }
                    }*/
                    ADSK.File[] files = documentService.FindFilesBySearchConditions(
                        conditions, /*SrchCond [] conditions*/
                        null, /*SrchSort [] sortConditions*/
                        folderIds, /*Long [] folderIds*/
                        true, /*Boolean recurseFolders*/
                        true, /*Boolean latestOnly*/
                        ref bookmark, /*[out] String bookmark*/
                        out status /*[out] SrchStatus searchstatus*/
                    );
                    if (files != null)
                    {
                        foreach (ADSK.File f in files)
                        {
                            fileListTmp.Add(f);
                            if (f.Name.ToLower().EndsWith(".idw"))
                            {
                                string fcode = f.Name.Substring(0, f.Name.Length - 4);
                                if (!allf.Contains(fcode))
                                {
                                    allf.Add(fcode);
                                    LOG.imprimeLog(System.DateTime.Now + " Adicionado via normal =====-> " + f.Name + "(" + f.FileSize + ") - CkInDate: " + f.CkInDate.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));
                                    fileList.Add(f);
                                }
                            }
                        }
                        foreach (ADSK.File f in files)
                        {
                            if (f.Name.ToLower().EndsWith(".dwg"))
                            {
                                string fcode = f.Name.Substring(0, f.Name.Length - 4);
                                if (!allf.Contains(fcode))
                                {
                                    allf.Add(fcode);
                                    LOG.imprimeLog(System.DateTime.Now + " Adicionado via normal =====-> " + f.Name + "(" + f.FileSize + ") - CkInDate: " + f.CkInDate.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));
                                    fileList.Add(f);
                                }
                            }
                        }
                        foreach (ADSK.File f in files)
                        {
                            if (f.Name.ToLower().EndsWith(".pdf"))
                            {
                                string fcode = f.Name.Substring(0, f.Name.Length - 4);
                                if (!allf.Contains(fcode))
                                {
                                    allf.Add(fcode);
                                    LOG.imprimeLog(System.DateTime.Now + " Adicionado via normal =====-> " + f.Name + "(" + f.FileSize + ") - CkInDate: " + f.CkInDate.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss"));
                                    fileList.Add(f);
                                }
                            }
                        }
                        /*
                        foreach (ADSK.File f in files)
                        {
                            fileListTmp.Add(f);
                            if (f.Name.ToLower().EndsWith(".pdf") || f.Name.ToLower().EndsWith(".idw") || f.Name.ToLower().EndsWith(".dwg"))
                            {
                                if (!fileList.Contains(f))
                                {
                                    LOG.imprimeLog(System.DateTime.Now + " Adicionado via normal =====-> " + f.Name + "(" + f.FileSize + ")");
                                    fileList.Add(f);
                                }
                            }
                        }
                        */
                    }

                    //ADSK.File[] files = documentService.FindFilesBySearchConditions(
                    //    conditions, /*SrchCond [] conditions*/
                    //    null, /*SrchSort [] sortConditions*/
                    //    null, /*Long [] folderIds*/
                    //    true, /*Boolean recurseFolders*/
                    //    true, /*Boolean latestOnly*/
                    //    ref bookmark, /*[out] String bookmark*/
                    //    out status /*[out] SrchStatus searchstatus*/
                    //);

                    //if (files != null)
                    //    fileList.AddRange(files);
                }
            }
            return fileList;
        }

        public static List<ADSK.File> findByEquals(_DocumentService documentService, string filename)
        {
            /* Faz a pesquisa */
            string bookmark = string.Empty;
            ADSK.SrchStatus status = null;

            ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
            conditions[0] = new ADSK.SrchCond();
            conditions[0].SrchOper = Condition.EQUALS.Code; // (long)SrchOperator.IsExactly; // 3; // Is exactly (or equals)
            conditions[0].SrchTxt = filename;
            conditions[0].PropTyp = ADSK.PropertySearchType.SingleProperty;
            conditions[0].PropDefId = propid;
            conditions[0].SrchRule = ADSK.SearchRuleType.Must;

            /*Console.WriteLine("Arquivos a procurar:");
            foreach (ADSK.SrchCond cond in conditions)
            {
                Console.WriteLine("   -> " + cond.SrchTxt);
            }*/

            //Console.WriteLine("Vai procurar os arquivos -------");
            List<ADSK.File> fileList = new List<ADSK.File>();
            while (status == null || fileList.Count < status.TotalHits)
            {
                ADSK.File[] files = documentService.FindFilesBySearchConditions(
                    conditions, /*SrchCond [] conditions*/
                    null, /*SrchSort [] sortConditions*/
                    folderIds, /*Long [] folderIds*/
                    true, /*Boolean recurseFolders*/
                    true, /*Boolean latestOnly*/
                    ref bookmark, /*[out] String bookmark*/
                    out status /*[out] SrchStatus searchstatus*/
                );

                if (files != null)
                    fileList.AddRange(files);
            }
            return fileList;
        }

        public static List<ADSK.File> findByMatches(_DocumentService documentService, String sfind)
        {
            /* Faz a pesquisa */
            string bookmark = string.Empty;
            ADSK.SrchStatus status = null;

            ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
            conditions[0] = new ADSK.SrchCond();
            conditions[0].SrchOper = Condition.CONTAINS.Code; // (long)SrchOperator.Contains; // 1; // Contains
            conditions[0].SrchTxt = sfind;
            conditions[0].PropTyp = ADSK.PropertySearchType.SingleProperty;
            conditions[0].PropDefId = propid;
            conditions[0].SrchRule = ADSK.SearchRuleType.Must;

            //Console.WriteLine("Vai procurar os arquivos -------");
            List<ADSK.File> fileList = new List<ADSK.File>();
            while (status == null || fileList.Count < status.TotalHits)
            {
                ADSK.File[] files = documentService.FindFilesBySearchConditions(
                    conditions, /*SrchCond [] conditions*/
                    null, /*SrchSort [] sortConditions*/
                    folderIds, /*Long [] folderIds*/
                    true, /*Boolean recurseFolders*/
                    true, /*Boolean latestOnly*/
                    ref bookmark, /*[out] String bookmark*/
                    out status /*[out] SrchStatus searchstatus*/
                );

                if (files != null)
                    fileList.AddRange(files);
            }
            return fileList;
        }


        /**
         * Lista todos os arquivos do Vault
         */
        public static void RunCommandListAll()
        {
            DocumentService documentService = serviceManager.DocumentService;
            try
            {
                Program.PrintFilesInFolder(((_DocumentService)documentService).GetFolderRoot(), documentService);
            }
            catch (Exception ex)
            {
                LOG.imprimeLog((string)(object)DateTime.Now + (object)" Error: " + ex.Message);
            }
        }

        /**
         * Lista os arquivos de uma pasta no Vault
         */
        private static void PrintFilesInFolder(ADSK.Folder parentFolder, ADSK.DocumentService docSvc)
        {
            ADSK.File[] files = docSvc.GetLatestFilesByFolderId(parentFolder.Id, false);
            if (files != null && files.Length > 0)
            {
                foreach (ADSK.File file in files)
                {
                    Console.WriteLine(parentFolder.FullName + "/" + file.Name);
                }
            }

            ADSK.Folder[] folders = docSvc.GetFoldersByParentId(parentFolder.Id, false);
            if (folders != null && folders.Length > 0)
            {
                foreach (ADSK.Folder folder in folders)
                {
                    PrintFilesInFolder(folder, docSvc);
                }
            }
        }

        /**
         * Faz o download de um arquivo.
         */
        public static void DownloadFile(ADSK.File file, string filePath, ADSK.DocumentService docSvc)
        {
            //Console.WriteLine("Downloading " + file.Name + " - Para o arquivo " + filePath);

            // remove the read-only attribute
            if (System.IO.File.Exists(filePath))
                System.IO.File.SetAttributes(filePath, FileAttributes.Normal);

            if (file.FileSize > MAX_FILE_SIZE)
                DownloadFileLarge(file, filePath, docSvc);
            else
                DownloadFileStandard(file, filePath, docSvc);

            // set the file to read-only
            //TODO: pq isso ta aqui??? System.IO.File.SetAttributes(filePath, FileAttributes.ReadOnly);
        }

        private static void DownloadFileStandard(ADSK.File file, string filePath, ADSK.DocumentService docSvc)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                byte[] fileData;
                ByteArray byteArray = new ByteArray();
                docSvc.DownloadFile((long)file.Id, true, out byteArray);
                fileData = (byte[])byteArray.Bytes;
                stream.Write(fileData, 0, fileData.Length);
                stream.Flush();
                stream.Close();
            }
        }

        private static void DownloadFileLarge(ADSK.File file, string filePath, ADSK.DocumentService docSvc)
        {
            if (System.IO.File.Exists(filePath))
                System.IO.File.SetAttributes(filePath, FileAttributes.Normal);

            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                long startByte = 0;
                long endByte = MAX_FILE_SIZE - 1;
                byte[] buffer;

                while (startByte < file.FileSize)
                {
                    endByte = startByte + MAX_FILE_SIZE;
                    if (endByte > file.FileSize)
                        endByte = file.FileSize;

                    buffer = docSvc.DownloadFilePart(file.Id, startByte, endByte, true).Bytes;
                    //buffer = (byte[])((_DocumentService)docSvc).DownloadFilePart((long)file.Id, startByte, endByte, true).Bytes;
                    stream.Write(buffer, 0, buffer.Length);
                    startByte += buffer.Length;
                }
                stream.Close();
            }
        }

        /**
         * Exclui um arquivo do sistema de arquivos.
         */
        private static bool DeleteFile(string filePath)
        {
            bool bOk = false;
            LOG.imprimeLog(System.DateTime.Now + " ======= DeleteFile: " + filePath + "  - Existe? " + System.IO.File.Exists(filePath));
            //Console.WriteLine("Deleting " + filePath);

            int cont = 0;
            while (++cont <= 15)
            {
                try
                {
                    try
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.SetAttributes(filePath, FileAttributes.Normal);
                            System.IO.File.Delete(filePath);
                        }
                        else
                        {
                            LOG.imprimeLog(System.DateTime.Now + " ======= WARN Nao achou mais o arquivo " + filePath);
                        }
                        bOk = true;
                        cont = 50;
                    }
                    catch (Exception ex)
                    {
                        LOG.imprimeLog(System.DateTime.Now + " ======= ** Erro excluindo arquivo (nova tentativa em 2 seg.): " + ex.Message);
                        System.Threading.Thread.Sleep(2000);
                        System.IO.File.SetAttributes(filePath, FileAttributes.Normal);
                        System.IO.File.Delete(filePath);
                    }
                }
                catch
                {
                    LOG.imprimeLog(System.DateTime.Now + " ======= **** ERRO excluindo arquivo: " + filePath);
                    if (!filePath.ToLower().EndsWith("manifest.xml"))
                    {
                        cont = 50;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(2000);
                    }
                }
            }
            return bOk;
        }

        private static void DeleteFilesFromDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                LOG.imprimeLog(System.DateTime.Now + " ==== Criou diretorio " + dir + " - " + Directory.Exists(dir));
            }
            else
            {
                String[] files = Directory.GetFiles(dir);
                if (files != null && files.Length > 0)
                {
                    foreach (string f in files)
                    {
                        if (!DeleteFile(f))
                        {
                            throw new Exception("Nao conseguiu excluir o arquivo '" + f + "'");
                        }
                    }
                }
            }
        }

        private static void DeleteRenameFilesFromDir(BatchConvertItem it)
        {
            LOG.imprimeLog(System.DateTime.Now + " ===== Processando item: " + it);
            int cont = 0;
            string[] files = Directory.GetFiles(it.imageDir);
            Boolean achou = false;
            if (files != null && files.Length > 0)
            {
                foreach (string f in files)
                {
                    if (f.ToLower().EndsWith(".log") || f.ToLower().EndsWith("execok.exe"))
                    {
                        // Ignora
                    }
                    else
                        if (f.ToLower().EndsWith(".jpg"))
                        {
                            if (f.ToLower().IndexOf(it.fileDown.Name.ToLower()) > 0)
                            {
                                achou = true;
                                //imageDir, ItemDir, DeCodigo, delFiles, ren && !fileDown.Name.EndsWith(".pdf")
                                cont++;
                                string newf = it.ItemDir + "\\" + it.DeCodigo + "-" + cont + ".jpg";
                                LOG.imprimeLog(System.DateTime.Now + " ==== Renomeando " + f + " para " + newf);
                                System.IO.File.Copy(f, newf, true);
                                DeleteFile(f);
                                //File.Move(f, newf);
                                LOG.imprimeLog(System.DateTime.Now + " ==== Renomeado?: " + System.IO.File.Exists(newf));
                            }
                        }
                }
            }
            if (!achou)
            {
                LOG.imprimeLog(System.DateTime.Now + " ===== ERRO: NAO ACHOU IMAGEM CONVERTIDA!!");
            }

            // Apaga os arquivos desnecessários
            if (delFiles)
            {
                DeleteRenameFilesFromDir(it.ItemDir, it.ItemDir, it.DeCodigo, delFiles, false);
            }
        }

        private static void DeleteRenameFilesFromDir(string dir, string destDir, string basename, Boolean del, Boolean ren)
        {
            LOG.imprimeLog(System.DateTime.Now + " ======= DeleteRenameFilesFromDir(" + dir + ", " + destDir
                + ", " + basename + ", " + del + ", " + ren + ")");
            int cont = 0;
            String[] files = Directory.GetFiles(dir);
            if (files != null && files.Length > 0)
            {
                foreach (string f in files)
                {
                    LOG.imprimeLog(System.DateTime.Now + " ========= DeleteRenameFilesFromDir - tratando arquivo: " + f);
                    if (f.ToLower().EndsWith(".log") || f.ToLower().EndsWith("execok.exe"))
                    {
                        LOG.imprimeLog(System.DateTime.Now + " =========== DeleteRenameFilesFromDir - arquivo ignorado: " + f);
                        // Ignora
                    }
                    else
                        if (f.ToLower().EndsWith(".jpg"))
                        {
                            if (ren)
                            {
                                cont++;
                                string newf = destDir + "\\" + basename + "-" + cont + ".jpg";
                                LOG.imprimeLog(System.DateTime.Now + " =========== DeleteRenameFilesFromDir - Renomeando " + f + " para " + newf);
                                Boolean bwhile = true;
                                int contErros = 0;
                                bool bOk = true;
                                while (bwhile)
                                {
                                    try
                                    {
                                        if (System.IO.File.Exists(newf))
                                        {
                                            LOG.imprimeLog(System.DateTime.Now + " =========== DeleteRenameFilesFromDir - vai excluir: " + newf);
                                            if (!DeleteFile(newf))
                                            {
                                                throw new Exception("Nao conseguiu excluir o arquivo antigo");
                                            }
                                        }
                                        System.IO.File.Copy(f, newf, true);
                                        if (!DeleteFile(f))
                                        {
                                            LOG.imprimeLog(System.DateTime.Now + " =========== WARN: DeleteRenameFilesFromDir - nao conseguiu excluir '" + f + "', pode dar problema em outro momento");
                                        }
                                        //File.Move(f, newf);
                                        bwhile = false;
                                        if (ecmEnabled)
                                        {
                                            LOG.imprimeLog(System.DateTime.Now + " =========== DeleteRenameFilesFromDir - Vai fazer upload para o ECM (" + basename + ", " + newf);
                                            ecm.updateECMDocument(basename, newf);

                                            // Exclui o jpg quando publicado no ECM.
                                            if (ecmDelJpg)
                                            {
                                                LOG.imprimeLog(System.DateTime.Now + " ============= DeleteRenameFilesFromDir - vai excluir: " + newf);
                                                if (!DeleteFile(newf))
                                                {
                                                    LOG.imprimeLog(System.DateTime.Now + " =========== WARN: DeleteRenameFilesFromDir - nao conseguiu excluir '" + newf + "', pode dar problema em outro momento");
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LOG.imprimeLog(System.DateTime.Now + " ==== " + ex.Message);
                                        contErros++;
                                        if (contErros > 30)
                                        {
                                            LOG.imprimeLog(System.DateTime.Now + " ======== Arquivo ignorado, estourou o limite de RENAME");
                                            bOk = false;
                                        }
                                        else
                                        {
                                            System.Threading.Thread.Sleep(1000);
                                        }
                                    }
                                }
                                LOG.imprimeLog(System.DateTime.Now + " ==== Renomeado?: " + System.IO.File.Exists(newf));
                                if (!bOk)
                                {
                                    throw new Exception("Nao conseguiu mover o arquivo '" + f + "' para o arquivo '" + newf + "'");
                                }
                            }
                        }
                        else
                            if (del)
                            {
                                LOG.imprimeLog(System.DateTime.Now + " =============== DeleteRenameFilesFromDir - vai excluir - f=" + f);
                                if (!DeleteFile(f))
                                {
                                    LOG.imprimeLog(System.DateTime.Now + " =========== WARN: DeleteRenameFilesFromDir - nao conseguiu excluir '" + f + "', pode dar problema em outro momento");
                                }
                            }
                }
            }

            files = Directory.GetFiles(destDir);
            foreach (string f in files)
            {
                if (f.ToLower().EndsWith(".log") || f.ToLower().EndsWith(".jpg"))
                {
                    // Ignora
                }
                else
                    if (del)
                    {
                        LOG.imprimeLog(System.DateTime.Now + " =============== DeleteRenameFilesFromDir - vai excluir restante - f=" + f);
                        if (!DeleteFile(f))
                        {
                            LOG.imprimeLog(System.DateTime.Now + " =========== WARN: DeleteRenameFilesFromDir - nao conseguiu excluir '" + f + "', pode dar problema em outro momento");
                        }
                    }
            }
        }

        /**
         * Extrai o arquivo manifest.xml do arquivo dwf
         */
        private static Dictionary<string, string> extract(String dir, String fileName, Dictionary<string, string> d)
        {
            //Console.WriteLine("0=False=Extract");
            SetProperty(d, "0", "False=Extract");
            using (ZipFile zip1 = ZipFile.Read(fileName))
            {
                foreach (ZipEntry e in zip1)
                {
                    if (e.FileName == "manifest.xml")
                    {
                        //Console.WriteLine(e.FileName);
                        e.Extract(dir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            d = parseXml(dir + "\\manifest.xml", d);
            return d;
        }

        /**
         * Interpreta o manifest.xml
         */
        private static Dictionary<string, string> parseXml(String fileName, Dictionary<string, string> d)
        {
            SetProperty(d, "0", "False=parseXml");
            int sheetNum = 0;
            XmlTextReader reader = new XmlTextReader(fileName);
            while (reader.Read())
            {
                if (reader.Name == "dwf:Section")
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            //Console.WriteLine(reader.Name);
                            if (reader.HasAttributes)
                            {
                                String sheetName = null;
                                String sheetPrefix = null;
                                Boolean processar = false;
                                for (int i = 0; i < reader.AttributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    if (reader.Name.Equals("type") && reader.Value.Equals("com.autodesk.dwf.ePlot"))
                                    {
                                        //Console.WriteLine("    " + reader.Name + " = " + reader.Value);
                                        processar = true;
                                        sheetNum++;
                                    }
                                    //if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("sheet") >= 0)
                                    if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("op_") >= 0)
                                    {
                                        sheetName = reader.Value;
                                        sheetPrefix = "OP";
                                        //Console.WriteLine("    " + reader.Name + " = " + reader.Value);
                                    }
                                    else if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("ps_") >= 0)
                                    {
                                        sheetName = reader.Value;
                                        sheetPrefix = "PS";
                                    }
                                    else if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("anvisa") >= 0)
                                    {
                                        sheetName = reader.Value;
                                        sheetPrefix = "ANVISA";
                                    }
                                    else if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("fda") >= 0)
                                    {
                                        sheetName = reader.Value;
                                        sheetPrefix = "FDA";
                                    }
                                    else if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("des") >= 0)
                                    {
                                        sheetName = reader.Value;
                                        sheetPrefix = "DES";
                                    }
                                }
                                if (sheetName != null)
                                {
                                    //Console.WriteLine(sheetNum + "=" + (sheetName != null) + "=" + sheetName);
                                    SetProperty(d, "" + sheetNum, sheetPrefix + "=" + sheetName);
                                }
                            }
                            break;
                    }
                }
            }
            return d;
        }

        public static ADSK.PropDef GetPropertyDefinition(String propName, ADSK.DocumentService docSvc)
        {
            ADSK.PropDef res = null;
            foreach (ADSK.PropDef prop in serviceManager.PropertyService.GetPropertyDefinitionsByEntityClassId("FILE"))
            {
                if (prop.SysName == propName)
                {
                    //Console.WriteLine("Prop:" + prop.SysName + "=" + prop.Id + "," + prop.DispName);
                    res = prop;
                }
            }
            return res;
        }

        /*
        public static String toUniversalTimeString(DateTime date)
        {
            //MM/dd/yyyy HH:mm:ss
            StringBuilder sb = new StringBuilder();
            if (date.Month < 10)
                sb.Append("0");
            sb.Append(date.Month);
            sb.Append("/");
            if (date.Day < 10)
                sb.Append("0");
            sb.Append(date.Day);
            sb.Append("/");
            sb.Append(date.Year);
            sb.Append(" ");
            if (date.Hour < 10)
                sb.Append("0");
            sb.Append(date.Hour);
            sb.Append(":");
            if (date.Minute < 10)
                sb.Append("0");
            sb.Append(date.Minute);
            sb.Append(":");
            if (date.Second < 10)
                sb.Append("0");
            sb.Append(date.Second);
            return sb.ToString();
        }*/

        static Dictionary<string, string> ReadPropertyFile(string filename)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();

            if (!System.IO.File.Exists(filename))
            {
                return d;
            }

            StreamReader SR;
            string S;
            SR = System.IO.File.OpenText(filename);
            S = SR.ReadLine();
            while (S != null)
            {
                if (S.IndexOf('=') > 0)
                {
                    String key = S.Substring(0, S.IndexOf('='));
                    String value = S.Substring(S.IndexOf('=') + 1);
                    d.Add(key, value);
                }
                S = SR.ReadLine();
            }
            SR.Close();

            return d;
        }

        static void WritePropertyFile(string filename, Dictionary<string, string> d)
        {
            StreamWriter SW;
            SW = System.IO.File.CreateText(filename);

            string value = null;
            foreach (string k in d.Keys)
            {
                d.TryGetValue(k, out value);
                SW.WriteLine(k + "=" + value);
            }

            SW.Close();
        }

        static string GetProperty(Dictionary<string, string> d, string key)
        {
            string value = null;
            d.TryGetValue(key, out value);
            return value;
        }

        static void SetProperty(Dictionary<string, string> d, string key, string value)
        {
            if (d.ContainsKey(key))
                d.Remove(key);
            d.Add(key, value);
        }

        static string AddFileToBpj(string ItemFile)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<DWF_File FileName=\"");
            sb.Append(ItemFile).Append("\"");
            sb.Append(" NoOfSections=\"0\"");
            sb.Append(" Print_to_scale=\"90\"");
            sb.Append(" Print_Style=\"0\"");
            sb.Append(" Print_What=\"0\"");
            sb.Append(" Fit_To_Paper=\"-1\"");
            sb.Append(" Paper_Size=\"9\"");
            sb.Append(" Paper_Size_Width=\"2100\"");
            sb.Append(" Paper_Size_Height=\"2970\"");
            sb.Append(" Orientation=\"2\"");
            sb.Append(" Number_of_copies=\"1\"");
            sb.Append(" PrinterName=\"").Append(printerName).Append("\"");
            sb.Append(" Page_Range=\"1\"");
            sb.Append(" Print_Range_Str=\"\"");
            sb.Append(" Reverse_Order=\"0\"");
            sb.Append(" Collate=\"0\"");
            sb.Append(" printColor=\"0\"");
            sb.Append(" MarkupColor=\"0\"");
            sb.Append(" printAlignment=\"4\"");
            sb.Append(" Use_DWF_Paper_Size=\"-1\"");
            sb.Append(" PrintasImage=\"0\"");
            sb.Append(" PaperName=\"A4\"");
            sb.Append(" useHPIP=\"0\"");
            sb.Append(" HPIPMediaID=\"-1\"");
            sb.Append(" HPIPExcludeEModel=\"-1\"");
            sb.Append(" HPIPPaperName=\"\"");
            sb.Append("/>");
            return sb.ToString();
        }



        public static Boolean HasPrintJobs(string printerName)
        {
            try
            {
                string PrinterJobs = "SELECT * FROM Win32_PrintJob";
                ManagementObjectSearcher FindPrintJobs =
                          new ManagementObjectSearcher(PrinterJobs);
                ManagementObjectCollection prntJobCollection = FindPrintJobs.Get();
                foreach (ManagementObject prntJob in prntJobCollection)
                {
                    System.String jobName = prntJob.Properties["Name"].Value.ToString();
                    char[] JobSplit = new char[1];
                    JobSplit[0] = Convert.ToChar(",");
                    string prnterName = jobName.Split(JobSplit)[0];
                    string documentName = "Doucment Name->" + prntJob.Properties["Document"].Value.ToString() + "               Sender Name->" + prntJob.Properties["owner"].Value.ToString();
                    if (String.Compare(prnterName, printerName, true) == 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.imprimeLog(System.DateTime.Now + " ==== " + ex.Message);
            }
            return false;
        }

        public static void sendMail(string subjectMSG, string bodyMSG)
        {
            if (mailPort > 0
                && mailHost != null
                && mailUser != null
                && mailPassword != null
                && mailSender != null
                && mailTO != null)
            {
                try
                {
                    LOG.imprimeLog(System.DateTime.Now + " === Vai enviar email: ");
                    SmtpClient client = new SmtpClient();
                    client.Port = mailPort;
                    client.Host = mailHost;
                    client.EnableSsl = mailEnableSSL;
                    client.Timeout = 10000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(mailUser, mailPassword);

                    string[] mails = mailTO.Split(';');
                    foreach (string mail in mails)
                    {
                        MailMessage mm = new MailMessage(mailSender, mail.Trim(), subjectMSG, bodyMSG);
                        mm.BodyEncoding = UTF8Encoding.UTF8;
                        mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                        client.Send(mm);
                    }
                }
                catch (Exception ex)
                {
                    LOG.imprimeLog(System.DateTime.Now + " ==== " + ex.Message);
                }
            }
        }

        private static void addReconvert(string nome)
        {
            SetProperty(reconvertNew, nome, "1");
        }

        private static Boolean isReconvert(String nome)
        {
            return GetProperty(reconvert, nome) != null;
        }
    }
}
