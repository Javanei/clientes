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
        private static string converterpath;
        private static string checkindate;
        private static bool convertall = false;
        // Testes
        private static bool listpropertydef = false;
        private static bool listbycheckindate = false;
        private static bool listall = false;
        private static bool list = false;

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
            value = NeodentUtil.util.DictionaryUtil.GetProperty(config, "converterpath");
            if (value != null)
            {
                converterpath = value;
            }

            ParseParams(config, args);

            NeodentUtil.util.DictionaryUtil.WritePropertyFile(confFile, config);

            NeodentUtil.util.LOG.debug("Parametros:");
            NeodentUtil.util.LOG.debug("===============================");
            NeodentUtil.util.LOG.debug("--- vaultserveraddr=" + vaultserveraddr);
            NeodentUtil.util.LOG.debug("--- vaultserver....=" + vaultserver);
            NeodentUtil.util.LOG.debug("--- vaultuser......=" + vaultuser);
            NeodentUtil.util.LOG.debug("--- vaultpass......=" + vaultpass);
            NeodentUtil.util.LOG.debug("--- tempfolder.....=" + tempfolder);
            NeodentUtil.util.LOG.debug("--- storagefolder..=" + storagefolder);

            /*
            TESTE1();
            */

            if (toConvert.Count > 0)
            {
                VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories,
                    vaultserver, vaultuser, vaultpass, tempfolder);
                try
                {
                    foreach (string desenho in toConvert)
                    {
                        manager.ConvertByFilename(desenho, validExt, storagefolder, converterpath);
                    }
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
                    manager.ConvertByCheckinDate(checkindate, validExt, storagefolder, converterpath);
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
                    manager.Convert(validExt, storagefolder, converterpath);
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
                    manager.List(validExt, storagefolder, converterpath);
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
                    manager.ListByCheckinDate(checkindate, validExt, storagefolder, converterpath);
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
                    manager.ListtAllInCheckin(validExt, storagefolder, converterpath);
                }
                finally
                {
                    manager.Close();
                }
            }
            else
            {
                VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories,
                    vaultserver, vaultuser, vaultpass, tempfolder);
                try
                {
                    manager.Convert(validExt, storagefolder, converterpath);
                }
                finally
                {
                    manager.Close();
                }
            }

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
                    else if (arg.StartsWith("-converterpath"))
                    {
                        converterpath = s.Substring(s.IndexOf('=') + 1);
                        NeodentUtil.util.DictionaryUtil.SetProperty(config, "converterpath", converterpath);
                    }
                    else if (arg.StartsWith("-checkindate"))
                    {
                        checkindate = s.Substring(s.IndexOf('=') + 1);
                    }
                    else if (arg.Equals("-convertall"))
                    {
                        convertall = true;
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
