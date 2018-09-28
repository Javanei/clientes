using System;
using System.Collections.Generic;
using System.IO;
using NeodentUtil.util;
using DWFCore.dwf;

namespace vaultsap
{
    class Program
    {
        private static readonly string[] baseRepositories = new string[] { "$/Neodent/Produção", "$/Neodent/Preset" };
        private static readonly string[] sheetPrefixes = new string[] { "op_", "ps_" };

        private static IDWFConverter dwfconverter;

        private static string vaultuser = "integracao";
        private static string vaultpass = "brasil2010";
        private static string vaultserveraddr = "br03s059.straumann.com";
        private static string vaultserver = "neodent";
        private static string storagefolder = "c:\\temp\\storage";
        private static string tempfolder = "c:\\temp";
        private static string dwfconverterpath;
        private static string pdfconverterpath;
        private static string checkindate;
        private static bool convertall = false;
        private static bool convert = false;
        private static bool ignorecheckout = false;
        private static bool help = false;
        // Testes
        private static bool listpropertydef = false;
        private static bool listbycheckindate = false;
        private static bool listall = false;
        private static bool list = false;
        private static bool listcheckedout = false;

        private static readonly string[] validExt = new string[] { ".idw.dwf", ".dwg.dwf", ".pdf" };
        private static readonly string confFile = "vaultsap.conf";

        private static List<string> toConvert = new List<string>();

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
            value = DictionaryUtil.GetProperty(config, "dwfconverterpath");
            if (value != null)
            {
                dwfconverterpath = value;
            }
            value = DictionaryUtil.GetProperty(config, "pdfconverterpath");
            if (value != null)
            {
                pdfconverterpath = value;
            }

            ParseParams(config, args);

            dwfconverter = new ACMECadTools.converter.Converter(dwfconverterpath);

            DictionaryUtil.WritePropertyFile(confFile, config);

            LOG.debug("Parametros:");
            LOG.debug("===============================");
            LOG.debug("--- vaultserveraddr.=" + vaultserveraddr);
            LOG.debug("--- vaultserver.....=" + vaultserver);
            LOG.debug("--- vaultuser.......=" + vaultuser);
            LOG.debug("--- vaultpass.......=" + vaultpass);
            LOG.debug("--- tempfolder......=" + tempfolder);
            LOG.debug("--- storagefolder...=" + storagefolder);
            LOG.debug("--- dwfconverterpath=" + dwfconverterpath);
            LOG.debug("--- pdfconverterpath=" + pdfconverterpath);

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
                Console.WriteLine("      -dwfconverterpath=<caminho>: Caminho do executável que converte os arquivos DWF para PDF/JPG");
                Console.WriteLine("      -pdfconverterpath=<caminho>: Caminho do executável que manipula PDF (Ghostscript)");
                Console.WriteLine("      -checkindate=<yyyy/MM/dd HH:mm:ss>: Uma data de checkin inicial para operações que a utilizarem");
                Console.WriteLine("    Opcoes validas para [comando]:");
                Console.WriteLine("      -convertall: Converte TODOS os desenhos que estão em checkin");
                Console.WriteLine("      -convert: Converte os desenhos a partir da data de checkin informada por -checkindate. Se não informada busca a data no arquivo vaultsap.conf. Se também não encontrar, converte TODOS os desenhos");
                Console.WriteLine("    Opcoes de teste validas para [comando]:");
                Console.WriteLine("      -listbycheckindate: Lista todos os desenhos em checkin a partir da data informada por -checkindate");
                Console.WriteLine("      -listall: Lista todos os desenhos em checkin");
                Console.WriteLine("      -list: Lista os desenhos em checkin usando a data de checkin salva no arquivo vaultsap.conf. Se não tiver uma data, lista todos");
                Console.WriteLine("      -listcheckedout: Lista todos os desenhos em checkout");
                Console.WriteLine("    [desenho1] [desenho2] [desenho...]: Converte os desenhos informados");
                Console.WriteLine("===================================");
            }
            else
            {
                try
                {
                    VaultTools.vault.Manager manager = new VaultTools.vault.Manager(dwfconverter, vaultserveraddr, baseRepositories, sheetPrefixes, vaultserver,
                        vaultuser, vaultpass, tempfolder);
                    try
                    {
                        if (toConvert.Count > 0)
                        {
                            foreach (string desenho in toConvert)
                            {
                                manager.ConvertByFilename(desenho, validExt, sheetPrefixes, storagefolder, dwfconverterpath, pdfconverterpath, ignorecheckout);
                            }
                        }
                        else if (convertall)
                        {
                            manager.Convert(validExt, sheetPrefixes, storagefolder, dwfconverterpath, pdfconverterpath);
                        }
                        else if (convert)
                        {
                            if (checkindate == null || checkindate == "")
                            {
                                manager.Convert(validExt, sheetPrefixes, storagefolder, dwfconverterpath, pdfconverterpath);
                            }
                            else
                            {
                                manager.ConvertByCheckinDate(checkindate, validExt, sheetPrefixes, storagefolder, dwfconverterpath, pdfconverterpath);
                            }
                        }
                        else if (listpropertydef)
                        {
                            ListaPropertyDef();
                        }
                        else if (list)
                        {
                            manager.List(validExt, storagefolder);
                        }
                        else if (listbycheckindate)
                        {
                            manager.ListByCheckinDate(checkindate, validExt, storagefolder);
                        }
                        else if (listcheckedout)
                        {
                            manager.ListAllCheckedOut(validExt, storagefolder);
                        }
                        else if (listall)
                        {
                            manager.ListtAllInCheckin(validExt, storagefolder);
                        }
                        else if (checkindate != null)
                        {
                            manager.ConvertByCheckinDate(checkindate, validExt, sheetPrefixes, storagefolder, dwfconverterpath, pdfconverterpath);
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
                }
            }
        }

        private static void ClearDirectory(string dir)
        {
            LOG.debug("@@@@@@@@@@ ClearDirectory - 1 - (" + dir + ")");
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                    LOG.debug("@@@@@@@@@@ ClearDirectory - 2 - (" + dir + ") - OK");
                }
                catch (IOException ex)
                {
                    LOG.debug("@@@@@@@@@@ ClearDirectory - 3 - (" + dir + ") - ERRO: " + ex.Message);
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
                    else if (arg.StartsWith("-dwfconverterpath"))
                    {
                        dwfconverterpath = s.Substring(s.IndexOf('=') + 1);
                        DictionaryUtil.SetProperty(config, "dwfconverterpath", dwfconverterpath);
                    }
                    else if (arg.StartsWith("-pdfconverterpath"))
                    {
                        pdfconverterpath = s.Substring(s.IndexOf('=') + 1);
                        DictionaryUtil.SetProperty(config, "pdfconverterpath", pdfconverterpath);
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
                    else if (arg.Equals("-list"))
                    {
                        list = true;
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
                    else if (!arg.Contains("="))
                    {
                        toConvert.Add(s.Trim());
                        help = help || false;
                    }
                }
            }
        }

        /* --------------------- */
        private static void ListaPropertyDef()
        {
            VaultTools.vault.Manager manager = new VaultTools.vault.Manager(dwfconverter, vaultserveraddr, baseRepositories, sheetPrefixes, vaultserver,
                vaultuser, vaultpass, tempfolder);
            try
            {
                LOG.debug("===============================");
                manager.TmpListPropertyDefinition();
                LOG.debug("===============================");
            }
            finally
            {
                manager.Close();
            }
        }
    }
}
