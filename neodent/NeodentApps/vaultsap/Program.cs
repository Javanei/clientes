using System;
using System.Collections.Generic;
using System.IO;
using NeodentUtil.util;
using DWFCore.dwf;
using PDFCore.pdf;

namespace vaultsap
{
    class Program
    {
        private static readonly string[] baseRepositoriesNormalMode = new string[] { "$/Neodent/Produção", "$/Neodent/Preset" };
        private static readonly string[] baseRepositoriesRegistroMode = new string[] { "$/Neodent/Produção", "$/Neodent/Preset", "$/Neodent/Registro" };

        private static string[] baseRepositories = null;
        private static string[] sheetPrefixes = null; // new string[] { "op_", "ps_" };

        private static IDWFConverter dwfconverter = null;
        private static IPDFConverter pdfconverter = null;

        // Configuração
        private static string vaultuser = "integracao";
        private static string vaultpass = "brasil2010";
        private static string vaultserveraddr = "br03s059.straumann.com";
        private static string vaultserver = "neodent";
        private static string storagefolder = "c:\\temp\\storage";
        private static string tempfolder = "c:\\temp";
        //private static string dwfconverterpath;
        private static string pdfconverterpath;
        private static string checkindate;
        private static bool ignorecheckout = false;
        private static bool preservetemp = false;
        private static string mode = "normal";

        // Comandos
        private static bool convertall = false;
        private static bool convert = false;
        private static bool listpropertydef = false;
        private static bool listbycheckindate = false;
        private static bool listall = false;
        private static bool list = false;
        private static bool listcheckedout = false;
        private static bool showinfo = false;
        private static bool help = false;
        private static bool stats = false;

        // Lista de desenhos a serem processandos
        private static List<string> toConvert = new List<string>();
        private static List<string> filesToConvert = new List<string>();

        private static readonly string[,] validExts = new string[,] {
            { ".idw", ".idw.dwf" },
            { ".dwg", ".dwg.dwf" },
            { ".pdf", ".pdf" }
        };
        private static readonly string confFile = "vaultsap.conf";

        [STAThread]
        static void Main(string[] args)
        {
            Dictionary<string, string> config = DictionaryUtil.ReadPropertyFile(confFile);
            string value = DictionaryUtil.GetProperty(config, "vaultuser");
            if (value != null)
            {
                vaultuser = value;
            }
            value = DictionaryUtil.GetProperty(config, "vaultpass");
            if (value != null)
            {
                vaultpass = value;
            }
            value = DictionaryUtil.GetProperty(config, "vaultserveraddr");
            if (value != null)
            {
                vaultserveraddr = value;
            }
            value = DictionaryUtil.GetProperty(config, "vaultserver");
            if (value != null)
            {
                vaultserver = value;
            }
            value = DictionaryUtil.GetProperty(config, "storagefolder");
            if (value != null)
            {
                storagefolder = value;
            }
            value = DictionaryUtil.GetProperty(config, "tempfolder");
            if (value != null)
            {
                tempfolder = value;
            }
            value = DictionaryUtil.GetProperty(config, "pdfconverterpath");
            if (value != null)
            {
                pdfconverterpath = value;
            }

            ParseParams(config, args);

            if (baseRepositories == null)
            {
                if (mode.ToLower().Equals("registro"))
                {
                    baseRepositories = baseRepositoriesRegistroMode;
                } else
                {
                    baseRepositories = baseRepositoriesNormalMode;
                }
            }
            if (mode.Equals("normal"))
            {
                sheetPrefixes = new string[] { "op_", "ps_" };
            }

            BullzipPDFTools.converter.Converter conv = new BullzipPDFTools.converter.Converter();
            dwfconverter = conv;
            pdfconverter = conv;

            DictionaryUtil.WritePropertyFile(confFile, config);

            LOG.debug("Parametros:");
            LOG.debug("===============================");
            LOG.debug("--- vaultserveraddr.=" + vaultserveraddr);
            LOG.debug("--- vaultserver.....=" + vaultserver);
            LOG.debug("--- vaultuser.......=" + vaultuser);
            LOG.debug("--- vaultpass.......=" + vaultpass);
            LOG.debug("--- tempfolder......=" + tempfolder);
            LOG.debug("--- storagefolder...=" + storagefolder);
            LOG.debug("--- pdfconverterpath=" + pdfconverterpath);
            LOG.debug("--- mode............=" + mode);

            /*
            if (toConvert.Count > 0)
            {
                ACMECadTools.converter.Converter converter = new ACMECadTools.converter.Converter(converterpath);
                foreach (string desenho in toConvert)
                {
                    // Usa um diretorio por imagem por causa dos problemas em deletar os arquivos
                    string imgTempfolder = tempfolder + "\\" + desenho;

                    // Por garantia, limpa o diretorio temporario
                    ClearDirectory(imgTempfolder);

                    // Cria o diretório temporario da imagem
                    if (!Directory.Exists(imgTempfolder))
                    {
                        Directory.CreateDirectory(imgTempfolder);
                    }

                    // Copia a imagem do Vault para a pasta temporario (equivale ao download)
                    string srcFile = "D:\\tmp\\cad\\vaultsap\\imagens\\" + desenho + ".idw.dwf";
                    string file = imgTempfolder + "\\" + desenho + ".idw.dwf";
                    File.Copy(srcFile, file);

                    // Enfim, converte as imagens
                    List<string> images = converter.DwfToJPG(file, imgTempfolder);

                    // Mergeia as imagens
                    if (images.Count > 0)
                    {
                        string destFile = storagefolder + "\\" + desenho + ".jpg";
                        NeodentUtil.util.ImageUtil.MergeImageList(images.ToArray(), destFile);
                    }

                    // Limpa o diretorio temporario
                    ClearDirectory(imgTempfolder);
                }
            }
            */

            if (dwfconverter == null)
            {
                help = true;
            }

            if (help)
            {
                Console.WriteLine("===================================");
                Console.WriteLine("Sintaxe:");
                Console.WriteLine("  vaultsap.exe [config] [comando] [desenho1] [desenho2] [desenho...]");
                Console.WriteLine("    Opcoes validas para [config]:");
                Console.WriteLine("      -vaultuser=<username>: Usuario de acesso ao Vault");
                Console.WriteLine("      -vaultpass=<password>: Senha de acesso ao Vault");
                Console.WriteLine("      -vaultserveraddr=<endereco>: Endereço do servidor vault. Default: br03s059.straumann.com");
                Console.WriteLine("      -vaultserver=<nome>: Nome do servidor no Vault. Default: neodent");
                Console.WriteLine("      -storagefolder=<caminho>: Caminho da pasta onde os desenhos convertidos serão salvos");
                Console.WriteLine("      -tempfolder=<caminho>: Caminho da pasta temporária que será usada durante a conversão");
                Console.WriteLine("      -pdfconverterpath=<caminho>: Caminho do executável que manipula PDF (Ghostscript)");
                Console.WriteLine("      -checkindate=<yyyy/MM/dd HH:mm:ss>: Uma data de checkin inicial para operações que a utilizarem");
                Console.WriteLine("      -sourcefile=<filepath>: Converte o arquivo especificado que já está em alguma pasta no disco");
                Console.WriteLine("      -sourcedir=<dir>: Converte os arquivos que estão na pasta especificada (e subpastas)");
                Console.WriteLine("      -ignorecheckout: Na conversão de desenhos específicos, converte mesmo que em checkout");
                Console.WriteLine("      -preservetemp: Não apaga os arquivos temporários usados durante a conversão");
                Console.WriteLine("      -mode=normal: Informa o modo de conversao. Opcoes validas: normal/registro");
                Console.WriteLine("      -pastas=Pastas: Lista de pastas de onde vai ser lido os desenhos");
                Console.WriteLine("    Opcoes validas para [comando]:");
                Console.WriteLine("      -convertall: Converte TODOS os desenhos que estão em checkin");
                Console.WriteLine("      -convert: Converte os desenhos a partir da data de checkin informada por -checkindate. Se não informada busca a data no arquivo vaultsap.conf. Se também não encontrar, converte TODOS os desenhos");
                Console.WriteLine("      -listbycheckindate: Lista todos os desenhos em checkin a partir da data informada por -checkindate");
                Console.WriteLine("      -listall: Lista todos os desenhos em checkin");
                Console.WriteLine("      -list: Lista os desenhos em checkin usando a data de checkin salva no arquivo vaultsap.conf. Se não tiver uma data, lista todos");
                Console.WriteLine("      -listcheckedout: Lista todos os desenhos em checkout");
                Console.WriteLine("      -stats: Mostra estatísticas sobre os desenhos");
                Console.WriteLine("      -showinfo: Mostra informações detalhadas referentes aos desenhos especificados");
                Console.WriteLine("      -help: Mostra essa tela de ajuda");
                Console.WriteLine("    [desenho1] [desenho2] [desenho...]: Converte os desenhos informados");
                Console.WriteLine("===================================");
            }
            else
            {
                try
                {
                    VaultTools.vault.Manager manager = filesToConvert.Count == 0
                        ? new VaultTools.vault.Manager(dwfconverter, pdfconverter, pdfconverterpath, vaultserveraddr, baseRepositories, validExts, sheetPrefixes, vaultserver, vaultuser, vaultpass, storagefolder, tempfolder)
                        : new VaultTools.vault.Manager(dwfconverter, pdfconverter, pdfconverterpath, baseRepositories, validExts, sheetPrefixes, storagefolder, tempfolder);
                    try
                    {
                        if (filesToConvert.Count > 0)
                        {
                            foreach (string f in filesToConvert)
                            {
                                string dir = Directory.GetParent(f).FullName;
                                string file = Path.GetFileName(f);
                                manager.ConvertAlreadyDownloadedFile(dir, file, preservetemp);
                            }
                        }
                        else if (toConvert.Count > 0)
                        {
                            foreach (string desenho in toConvert)
                            {
                                if (showinfo)
                                {
                                    manager.ShowInfoByName(desenho);
                                }
                                else
                                {
                                    manager.ConvertByFilename(desenho, ignorecheckout, preservetemp);
                                }
                            }
                        }
                        else if (convertall)
                        {
                            manager.ConvertAllInCheckin(preservetemp, ignorecheckout);
                        }
                        else if (stats)
                        {
                            manager.Stats();
                        }
                        else if (convert)
                        {
                            if (checkindate == null || checkindate == "")
                            {
                                manager.Convert(preservetemp, ignorecheckout);
                            }
                            else
                            {
                                manager.ConvertByCheckinDate(checkindate, preservetemp, ignorecheckout);
                            }
                        }
                        else if (listpropertydef)
                        {
                            manager.ListaPropertyDef();
                        }
                        else if (list)
                        {
                            manager.List(ignorecheckout);
                        }
                        else if (listbycheckindate)
                        {
                            manager.ListByCheckinDate(checkindate, ignorecheckout);
                        }
                        else if (listcheckedout)
                        {
                            manager.ListAllCheckedOut();
                        }
                        else if (listall)
                        {
                            manager.ListtAllInCheckin(ignorecheckout);
                        }
                        else if (checkindate != null)
                        {
                            manager.ConvertByCheckinDate(checkindate, preservetemp, ignorecheckout);
                        }
                    }
                    finally
                    {
                        manager.Close();
                    }
                }
                catch (Exception eManager)
                {
                    LOG.error("Não conseguiu conectar o Vault: " + eManager.Message);
                    LOG.error(eManager.StackTrace);
                }
            }
        }

        private static void ParseParams(Dictionary<string, string> config, string[] args)
        {
            if (args != null && args.Length > 0)
            {
                foreach (string s in args)
                {
                    string arg = s.ToLower().Trim();
                    if (arg.StartsWith("-vaultuser=") || arg.StartsWith("-vaultusername="))
                    {
                        vaultuser = s.Substring(s.IndexOf('=') + 1);
                        DictionaryUtil.SetProperty(config, "vaultuser", vaultuser);
                    }
                    else if (arg.StartsWith("-vaultpass") || arg.StartsWith("-vaultpassword"))
                    {
                        vaultpass = s.Substring(s.IndexOf('=') + 1);
                        DictionaryUtil.SetProperty(config, "vaultpass", vaultpass);
                    }
                    else if (arg.StartsWith("-vaultserveraddr"))
                    {
                        vaultserveraddr = s.Substring(s.IndexOf('=') + 1);
                        DictionaryUtil.SetProperty(config, "vaultserveraddr", vaultserveraddr);
                    }
                    else if (arg.StartsWith("-vaultserver"))
                    {
                        vaultserver = s.Substring(s.IndexOf('=') + 1);
                        DictionaryUtil.SetProperty(config, "vaultserver", vaultserver);
                    }
                    else if (arg.StartsWith("-tempfolder"))
                    {
                        tempfolder = s.Substring(s.IndexOf('=') + 1);
                        DictionaryUtil.SetProperty(config, "tempfolder", tempfolder);
                    }
                    else if (arg.StartsWith("-storagefolder"))
                    {
                        storagefolder = s.Substring(s.IndexOf('=') + 1);
                        DictionaryUtil.SetProperty(config, "storagefolder", storagefolder);
                    }
                    else if (arg.StartsWith("-mode"))
                    {
                        mode = s.Substring(s.IndexOf('=') + 1).ToLower();
                        if (!mode.Equals("normal") && !mode.Equals("registro"))
                        {
                            mode = "normal";
                        }
                    }
                    else if (arg.StartsWith("-pastas"))
                    {
                        string pastas = s.Substring(s.IndexOf('=') + 1);
                        string[] tmp = pastas.Split(',');
                        baseRepositories = new string[tmp.Length];
                        for(int i = 0; i < tmp.Length; i++)
                        {
                            baseRepositories[i] = "$/Neodent/" + tmp[i];
                        }
                    }
                    else if (arg.StartsWith("-pdfconverterpath"))
                    {
                        pdfconverterpath = s.Substring(s.IndexOf('=') + 1);
                        DictionaryUtil.SetProperty(config, "pdfconverterpath", pdfconverterpath);
                    }
                    else if (arg.StartsWith("-sourcefile"))
                    {
                        filesToConvert.Add(s.Substring(s.IndexOf('=') + 1));
                    }
                    else if (arg.StartsWith("-sourcedir"))
                    {
                        List<string> dirs = new List<string>
                        {
                            s.Substring(s.IndexOf('=') + 1)
                        };
                        while (dirs.Count > 0)
                        {
                            string dir = dirs[0];
                            foreach (string f in Directory.GetFiles(dir))
                            {
                                //if (f.EndsWith(".dwf") || f.EndsWith(".pdf"))
                                if (f.EndsWith(".dwf"))
                                {
                                    filesToConvert.Add(f);
                                }
                            }
                            dirs.Remove(dir);
                            foreach (string f in Directory.GetDirectories(dir))
                            {
                                dirs.Add(f);
                            }
                        }
                    }
                    else if (arg.StartsWith("-checkindate"))
                    {
                        checkindate = s.Substring(s.IndexOf('=') + 1);
                        help = help || false;
                    }
                    else if (arg.Equals("-convertall"))
                    {
                        convertall = true;
                        help = help || false;
                    }
                    else if (arg.Equals("-convert"))
                    {
                        convert = true;
                        help = help || false;
                    }
                    else if (arg.Equals("-listpropertydef"))
                    {
                        listpropertydef = true;
                        help = help || false;
                    }
                    else if (arg.Equals("-listbycheckindate"))
                    {
                        listbycheckindate = true;
                        help = help || false;
                    }
                    else if (arg.Equals("-listall"))
                    {
                        listall = true;
                        help = help || false;
                    }
                    else if (arg.Equals("-listcheckedout"))
                    {
                        listcheckedout = true;
                        help = help || false;
                    }
                    else if (arg.Equals("-showinfo"))
                    {
                        showinfo = true;
                        help = help || false;
                    }
                    else if (arg.Equals("-list"))
                    {
                        list = true;
                        help = help || false;
                    }
                    else if (arg.Equals("-stats"))
                    {
                        stats = true;
                        help = help || false;
                    }
                    else if (arg.Equals("-help"))
                    {
                        help = true;
                    }
                    else if (arg.Equals("-ignorecheckout"))
                    {
                        ignorecheckout = true;
                    }
                    else if (arg.Equals("-preservetemp"))
                    {
                        preservetemp = true;
                    }
                    else if (!arg.Contains("="))
                    {
                        toConvert.Add(s.Trim());
                        help = help || false;
                    }
                }
            }
        }
    }
}
