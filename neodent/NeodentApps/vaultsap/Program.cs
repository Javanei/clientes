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

        private static readonly string[] validExt = new string[] { ".idw.dwf", ".dwg.dwf", ".pdf" };
        private static List<string> toConvert = new List<string>();

        [STAThread]
        static void Main(string[] args)
        {
            ParseParams(args);
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

        private static void ParseParams(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                foreach (string s in args)
                {
                    string arg = s.ToLower().Trim();
                    if (arg.StartsWith("-vaultuser=") || arg.StartsWith("-vaultusername="))
                    {
                        vaultuser = s.Substring(s.IndexOf('=') + 1);
                    }
                    else if (arg.StartsWith("-vaultpass") || arg.StartsWith("-vaultpassword"))
                    {
                        vaultpass = s.Substring(s.IndexOf('=') + 1);
                    }
                    else if (arg.StartsWith("-vaultserveraddr"))
                    {
                        vaultserveraddr = s.Substring(s.IndexOf('=') + 1);
                    }
                    else if (arg.StartsWith("-vaultserver"))
                    {
                        vaultserver = s.Substring(s.IndexOf('=') + 1);
                    }
                    else if (arg.StartsWith("-tempfolder"))
                    {
                        tempfolder = s.Substring(s.IndexOf('=') + 1);
                    }
                    else if (arg.StartsWith("-storagefolder"))
                    {
                        storagefolder = s.Substring(s.IndexOf('=') + 1);
                    }
                    else if (arg.StartsWith("-converterpath"))
                    {
                        converterpath = s.Substring(s.IndexOf('=') + 1);
                    }
                    else if (!arg.Contains("="))
                    {
                        toConvert.Add(s.Trim());
                    }
                }
            }
        }

        /* --------------------- */
        private static void TESTE1()
        {
            VaultTools.vault.Manager manager = new VaultTools.vault.Manager(vaultserveraddr, baseRepositories, vaultserver, vaultuser, vaultpass);
            try
            {
                NeodentUtil.util.LOG.debug("===============================");
                manager.TmpListPropertyDefinition();
                NeodentUtil.util.LOG.debug("===============================");
                NeodentUtil.util.LOG.debug("--- Validando arquivo: 115.111");
                manager.TmpFindFindByName("115.111", validExt);
                NeodentUtil.util.LOG.debug("--- Validando arquivo: 103.238");
                manager.TmpFindFindByName("103.238", validExt);
            }
            finally
            {
                manager.Close();
            }
        }
    }
}
