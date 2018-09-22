using System;
using System.Collections.Generic;
using System.IO;

namespace vaultsap
{
    class Program
    {
        private static readonly string[] baseRepositories = new string[] { "$/Neodent/Produção" };

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
            Dictionary<string, string> config = NeodentUtil.util.DictionaryUtil.ReadPropertyFile(confFile);
            string value = NeodentUtil.util.DictionaryUtil.GetProperty(config, "vaultuser");
            if (value != null)
            {
                vaultuser = value;
            }
            value = NeodentUtil.util.DictionaryUtil.GetProperty(config, "vaultpass");
            if (value != null)
            {
                vaultpass = value;
            }
            value = NeodentUtil.util.DictionaryUtil.GetProperty(config, "vaultserveraddr");
            if (value != null)
            {
                vaultserveraddr = value;
            }
            value = NeodentUtil.util.DictionaryUtil.GetProperty(config, "vaultserver");
            if (value != null)
            {
                vaultserver = value;
            }
            value = NeodentUtil.util.DictionaryUtil.GetProperty(config, "storagefolder");
            if (value != null)
            {
                storagefolder = value;
            }
            value = NeodentUtil.util.DictionaryUtil.GetProperty(config, "tempfolder");
            if (value != null)
            {
                tempfolder = value;
            }
            value = NeodentUtil.util.DictionaryUtil.GetProperty(config, "dwfconverterpath");
            if (value != null)
            {
                dwfconverterpath = value;
            }
            value = NeodentUtil.util.DictionaryUtil.GetProperty(config, "pdfconverterpath");
            if (value != null)
            {
                pdfconverterpath = value;
            }

            ParseParams(config, args);

            NeodentUtil.util.DictionaryUtil.WritePropertyFile(confFile, config);

            NeodentUtil.util.LOG.debug("Parametros:");
            NeodentUtil.util.LOG.debug("===============================");
            NeodentUtil.util.LOG.debug("--- vaultserveraddr.=" + vaultserveraddr);
            NeodentUtil.util.LOG.debug("--- vaultserver.....=" + vaultserver);
            NeodentUtil.util.LOG.debug("--- vaultuser.......=" + vaultuser);
            NeodentUtil.util.LOG.debug("--- vaultpass.......=" + vaultpass);
            NeodentUtil.util.LOG.debug("--- tempfolder......=" + tempfolder);
            NeodentUtil.util.LOG.debug("--- storagefolder...=" + storagefolder);
            NeodentUtil.util.LOG.debug("--- dwfconverterpath=" + dwfconverterpath);
            NeodentUtil.util.LOG.debug("--- pdfconverterpath=" + pdfconverterpath);


            if (toConvert.Count > 0)
            {
                VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories,
                    vaultserver, vaultuser, vaultpass, tempfolder);
                try
                {
                    foreach (string desenho in toConvert)
                    {
                        manager.ConvertByFilename(desenho, validExt, storagefolder, dwfconverterpath, pdfconverterpath);
                    }
                }
                finally
                {
                    manager.Close();
                }
            }
            else if (convertall)
            {
                VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories,
                    vaultserver, vaultuser, vaultpass, tempfolder);
                try
                {
                    manager.Convert(validExt, storagefolder, dwfconverterpath, pdfconverterpath);
                }
                finally
                {
                    manager.Close();
                }
            }
            else if (convert)
            {
                VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories,
                    vaultserver, vaultuser, vaultpass, tempfolder);
                try
                {
                    if (checkindate == null || checkindate == "")
                    {
                        manager.Convert(validExt, storagefolder, dwfconverterpath, pdfconverterpath);
                    } else
                    {
                        manager.ConvertByCheckinDate(checkindate, validExt, storagefolder, dwfconverterpath, pdfconverterpath);
                    }
                }
                finally
                {
                    manager.Close();
                }
            }
            else if (listpropertydef)
            {
                ListaPropertyDef();
            }
            else if (list)
            {
                VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories,
                    vaultserver, vaultuser, vaultpass, tempfolder);
                try
                {
                    manager.List(validExt, storagefolder);
                }
                finally
                {
                    manager.Close();
                }
            }
            else if (listbycheckindate)
            {
                VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories,
                    vaultserver, vaultuser, vaultpass, tempfolder);
                try
                {
                    manager.ListByCheckinDate(checkindate, validExt, storagefolder);
                }
                finally
                {
                    manager.Close();
                }
            }
            else if (listall)
            {
                VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories,
                    vaultserver, vaultuser, vaultpass, tempfolder);
                try
                {
                    manager.ListtAllInCheckin(validExt, storagefolder);
                }
                finally
                {
                    manager.Close();
                }
            }
            else if (listcheckedout)
            {
                VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories,
                    vaultserver, vaultuser, vaultpass, tempfolder);
                try
                {
                    manager.ListAllCheckedOut(validExt, storagefolder);
                }
                finally
                {
                    manager.Close();
                }
            }
            else if (checkindate != null)
            {
                VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories,
                    vaultserver, vaultuser, vaultpass, tempfolder);
                try
                {
                    manager.ConvertByCheckinDate(checkindate, validExt, storagefolder, dwfconverterpath, pdfconverterpath);
                }
                finally
                {
                    manager.Close();
                }
            }
            else
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
                        NeodentUtil.util.DictionaryUtil.SetProperty(config, "vaultuser", vaultuser);
                    }
                    else if (arg.StartsWith("-vaultpass") || arg.StartsWith("-vaultpassword"))
                    {
                        vaultpass = s.Substring(s.IndexOf('=') + 1);
                        NeodentUtil.util.DictionaryUtil.SetProperty(config, "vaultpass", vaultpass);
                    }
                    else if (arg.StartsWith("-vaultserveraddr"))
                    {
                        vaultserveraddr = s.Substring(s.IndexOf('=') + 1);
                        NeodentUtil.util.DictionaryUtil.SetProperty(config, "vaultserveraddr", vaultserveraddr);
                    }
                    else if (arg.StartsWith("-vaultserver"))
                    {
                        vaultserver = s.Substring(s.IndexOf('=') + 1);
                        NeodentUtil.util.DictionaryUtil.SetProperty(config, "vaultserver", vaultserver);
                    }
                    else if (arg.StartsWith("-tempfolder"))
                    {
                        tempfolder = s.Substring(s.IndexOf('=') + 1);
                        NeodentUtil.util.DictionaryUtil.SetProperty(config, "tempfolder", tempfolder);
                    }
                    else if (arg.StartsWith("-storagefolder"))
                    {
                        storagefolder = s.Substring(s.IndexOf('=') + 1);
                        NeodentUtil.util.DictionaryUtil.SetProperty(config, "storagefolder", storagefolder);
                    }
                    else if (arg.StartsWith("-dwfconverterpath"))
                    {
                        dwfconverterpath = s.Substring(s.IndexOf('=') + 1);
                        NeodentUtil.util.DictionaryUtil.SetProperty(config, "dwfconverterpath", dwfconverterpath);
                    }
                    else if (arg.StartsWith("-pdfconverterpath"))
                    {
                        pdfconverterpath = s.Substring(s.IndexOf('=') + 1);
                        NeodentUtil.util.DictionaryUtil.SetProperty(config, "pdfconverterpath", pdfconverterpath);
                    }
                    else if (arg.StartsWith("-checkindate"))
                    {
                        checkindate = s.Substring(s.IndexOf('=') + 1);
                    }
                    else if (arg.Equals("-convertall"))
                    {
                        convertall = true;
                    }
                    else if (arg.Equals("-convert"))
                    {
                        convert = true;
                    }
                    else if (arg.Equals("-listpropertydef"))
                    {
                        listpropertydef = true;
                    }
                    else if (arg.Equals("-listbycheckindate"))
                    {
                        listbycheckindate = true;
                    }
                    else if (arg.Equals("-listall"))
                    {
                        listall = true;
                    }
                    else if (arg.Equals("-listcheckedout"))
                    {
                        listcheckedout = true;
                    }
                    else if (arg.Equals("-list"))
                    {
                        list = true;
                    }
                    else if (!arg.Contains("="))
                    {
                        toConvert.Add(s.Trim());
                    }
                }
            }
        }

        /* --------------------- */
        private static void ListaPropertyDef()
        {
            VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories, vaultserver,
                vaultuser, vaultpass, tempfolder);
            try
            {
                NeodentUtil.util.LOG.debug("===============================");
                manager.TmpListPropertyDefinition();
                NeodentUtil.util.LOG.debug("===============================");
            }
            finally
            {
                manager.Close();
            }
        }
    }
}
