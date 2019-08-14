using NeodentUtil.util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VaultTools.vault.util;
using ADSK = Autodesk.Connectivity.WebServices;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace ListaUnidade
{
    class Program
    {
        private static readonly string[] baseRepositories = new string[]
            { "$/Neodent/Produção", "$/Neodent/Registro", "$/Neodent/Alterações" };
        private static readonly string[,] validExts = new string[,] {
            { ".idw", ".idw" }
        };
        private static readonly Encoding srcEnc = Encoding.GetEncoding("Windows-1252");
        private static readonly Encoding dstEnc = Encoding.GetEncoding("ibm850");

        // Configuração
        private static string vaultuser = "integracao";
        private static string vaultpass = "brasil2010";
        private static string vaultserveraddr = "br03s059.straumann.com";
        private static string vaultserver = "neodent";
        private static string exportfile = "unidade.xlsx";
        //private static ADSK.PropDef propUnidade;
        private static VDF.Vault.Currency.Properties.PropertyDefinition propDefUnidade;

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
            LOG.debug("--- exportfile......=" + exportfile);

            XSSFWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("unidades");
            IRow row = sheet.CreateRow(0);
            ICell cell = row.CreateCell(0);
            cell.SetCellValue("File Name");
            cell = row.CreateCell(1);
            cell.SetCellValue("Unidade");
            cell = row.CreateCell(2);
            cell.SetCellValue("Path");

            LOG.info("Cabecalho da planilha inicializado");
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

                VDF.Vault.Currency.Properties.PropertyDefinitionDictionary propDefsDict = conn.PropertyManager.GetPropertyDefinitions(
                    VDF.Vault.Currency.Entities.EntityClassIds.Files,
                    null,
                    VDF.Vault.Currency.Properties.PropertyDefinitionFilter.IncludeAll
                );
                ICollection<VDF.Vault.Currency.Properties.PropertyDefinition> propertyDefinitions = propDefsDict.Values;
                foreach (VDF.Vault.Currency.Properties.PropertyDefinition p in propertyDefinitions)
                {
                    if (p.DisplayName == "UNIDADE")
                    {
                        propDefUnidade = p;
                        break;
                    }
                }

                if (propDefUnidade == null)
                {
                    throw new Exception("Não encontrado a propriedade Unidade");
                }

                LOG.debug("Iniciando leitura dos desenhos");

                List<ADSK.File> files = FindAll.Find(conn, baseRepositories, validExts);
                LOG.debug("Desenhos encontrados: " + files.Count);

                ExportResult(files, sheet);

                LOG.debug("Planilha gerada");
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
                SaveSheet(workbook, sheet);
            }
        }

        private static void ExportResult(List<ADSK.File> files, ISheet sheet)
        {
            ADSKTools.WebServiceManager serviceManager = conn.WebServiceManager;
            ADSK.DocumentService documentService = serviceManager.DocumentService;
            VDF.Vault.Services.Connection.IPropertyManager propertyManager = conn.PropertyManager;

            LOG.debug("Exportando resultado");

            int iRow = 0;
            foreach (ADSK.File file in files)
            {
                IRow row = sheet.CreateRow(++iRow);
                ICell cell = row.CreateCell(0);
                cell.SetCellValue(file.Name);
                string value = "";
                if (propDefUnidade != null)
                {
                    VDF.Vault.Currency.Entities.FileIteration fi = new VDF.Vault.Currency.Entities.FileIteration(
                            conn,
                            file);
                    object propValue = propertyManager.GetPropertyValue(fi, propDefUnidade, null);
                    if (propValue != null)
                    {
                        value = propValue.ToString();
                    }
                }
                cell = row.CreateCell(1);
                cell.SetCellValue(value);
                cell = row.CreateCell(2);
                ADSK.Folder folder = documentService.GetFolderById(file.FolderId);
                cell.SetCellValue(dstEnc.GetString(srcEnc.GetBytes(folder.FullName)));
            }
        }

        private static void SaveSheet(XSSFWorkbook workbook, ISheet sheet)
        {
            for (int i = 0; i <= 2; i++)
            {
                sheet.AutoSizeColumn(i);
            }
            using (var fs = new FileStream(exportfile, FileMode.Create, FileAccess.Write))
            {
                LOG.debug("Iniciando gravacao da planilha em disco: " + exportfile);
                workbook.Write(fs);
                LOG.debug("Planilha salva, tudo OK");
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
                    else if (arg.StartsWith("-exportfile"))
                    {
                        exportfile = s.Substring(s.IndexOf('=') + 1);
                    }
                }
            }
        }
    }
}
