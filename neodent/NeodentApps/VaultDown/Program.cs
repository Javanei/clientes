using NeodentUtil.util;
using System;
using System.Collections.Generic;
using VaultTools.vault.util;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace VAultDown
{
    class Program
    {
        private static readonly string[] baseRepositories = new string[]
            { "$/Neodent/Produção", "$/Neodent/Registro" };
        private static readonly string[,] validExts = new string[,] {
            { ".idw", ".idw" }
        };

        // Configuração
        private static string vaultuser = "integracao";
        private static string vaultpass = "brasil2010";
        private static string vaultserveraddr = "br03s059.straumann.com";
        private static string vaultserver = "neodent";
        private static string storagefolder = "c:\\temp\\storage";
        private static List<string> _desenhos = new List<string>();
        private static Dictionary<long, string> _folders = new Dictionary<long, string>();

        private static VDF.Vault.Currency.Connections.Connection conn = null;

        static void Main(string[] args)
        {
            ParseParams(args);

            //LOG.DEBUG = false;

            LOG.debug("Parametros:");
            LOG.debug("===============================");
            LOG.debug("--- vaultserveraddr.=" + vaultserveraddr);
            LOG.debug("--- vaultserver.....=" + vaultserver);
            LOG.debug("--- vaultuser.......=" + vaultuser);
            LOG.debug("--- vaultpass.......=" + vaultpass);
            LOG.debug("--- storagefolder...=" + storagefolder);

            try
            {
                VDF.Vault.Results.LogInResult result =
                    VDF.Vault.Library.ConnectionManager.LogIn(vaultserveraddr, vaultserver, vaultuser, vaultpass,
                    VDF.Vault.Currency.Connections.AuthenticationFlags.Standard, null);
                if (result.Success)
                {
                    conn = result.Connection;
                }
                else
                {
                    throw new Exception("Falha de login");
                }

                LOG.debug("Iniciando leitura dos desenhos");
                List<HierarchyItem> items = FindHierarchy.Find(conn, baseRepositories, validExts, _desenhos);
                LOG.debug("Desenhos lidos. Iniciando download");
                //List<string> printeds = new List<string>();
                //ExportResult(printeds, 0, 0, workbook, sheet, items);
                //LOG.debug("Planilha gerada");
            }
            catch (Exception eManager)
            {
                LOG.error("Não conseguiu conectar o Vault: " + eManager.Message);
                LOG.error(eManager.StackTrace);
            }
            finally
            {
                if (conn != null)
                {
                    VDF.Vault.Library.ConnectionManager.LogOut(conn);
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
                    else if (arg.StartsWith("-storagefolder"))
                    {
                        storagefolder = s.Substring(s.IndexOf('=') + 1);
                    }
                    else
                    {
                        _desenhos.Add(s);
                    }
                }
            }
        }
    }
}
