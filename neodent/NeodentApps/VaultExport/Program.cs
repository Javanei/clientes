using NeodentUtil.util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using VaultTools.vault.util;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace VaultExport
{
    class Program
    {
        private static readonly string[] baseRepositories = new string[] { "$/Neodent/Produção" };
        private static readonly string[,] validExts = new string[,] { { ".idw", ".idw" } };

        // Configuração
        private static string vaultuser = "integracao";
        private static string vaultpass = "brasil2010";
        private static string vaultserveraddr = "br03s059.straumann.com";
        private static string vaultserver = "neodent";
        private static string exportfile = "desenhos.xlsx";
        private static List<string> _desenhos = new List<string>();
        private static Dictionary<long, string> _folders = new Dictionary<long, string>();
        private const int SHEET_START_COL_INDEX = 1;

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
            ISheet sheet = workbook.CreateSheet("desenhos");
            for (int i = 0; i <= SHEET_START_COL_INDEX; i++)
            {
                sheet.SetColumnWidth(i, 1100);
            }
            IRow row = sheet.CreateRow(0);
            ICell cell = row.CreateCell(0);
            cell.SetCellValue("Parent");
            cell = row.CreateCell(1);
            cell.SetCellValue("File Name");
            cell = row.CreateCell(SHEET_START_COL_INDEX + 1);
            cell.SetCellValue("Level");
            cell = row.CreateCell(SHEET_START_COL_INDEX + 2);
            cell.SetCellValue("Version");
            cell = row.CreateCell(SHEET_START_COL_INDEX + 3);
            cell.SetCellValue("Path");
            cell = row.CreateCell(SHEET_START_COL_INDEX + 4);
            cell.SetCellValue("Checked In");
            cell = row.CreateCell(SHEET_START_COL_INDEX + 5);
            cell.SetCellValue("Entity Icon");
            cell = row.CreateCell(SHEET_START_COL_INDEX + 6);
            cell.SetCellValue("File Externsion");
            cell = row.CreateCell(SHEET_START_COL_INDEX + 7);
            cell.SetCellValue("Material");
            cell = row.CreateCell(SHEET_START_COL_INDEX + 8);
            cell.SetCellValue("Rev Number");
            cell = row.CreateCell(SHEET_START_COL_INDEX + 9);
            cell.SetCellValue("Status");
            cell = row.CreateCell(SHEET_START_COL_INDEX + 10);
            cell.SetCellValue("TotalVolume");
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

                LOG.debug("Iniciando leitura dos desenhos");
                List<HierarchyItem> items = FindHierarchy.Find(conn, baseRepositories, validExts, _desenhos);
                LOG.debug("Desenhos lidos. Iniciando geracao da planilha");
                List<string> printeds = new List<string>();
                ExportResult(printeds, 0, 0, workbook, sheet, items);
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

        private static void SaveSheet(XSSFWorkbook workbook, ISheet sheet)
        {
            for (int i = SHEET_START_COL_INDEX + 1; i <= SHEET_START_COL_INDEX + 10; i++)
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

        private static int ExportResult(List<string> printeds, int startCol, int startRow, XSSFWorkbook workbook, ISheet sheet, List<HierarchyItem> items)
        {
            int lastRow = startRow;
            if (items != null && items.Count > 0)
            {
                foreach (HierarchyItem _parent in items)
                {
                    if (!printeds.Contains(_parent.FileName) && _parent.Children != null && _parent.Children.Count > 0)
                    {
                        foreach (HierarchyItem item in _parent.Children)
                        {
                            ExportRow(++lastRow, sheet, _parent, item);
                            printeds.Add(_parent.FileName);
                        }
                        foreach (HierarchyItem item in _parent.Children)
                        {
                            lastRow = ExportResult(printeds, startCol, lastRow, workbook, sheet, _parent.Children);
                        }
                    }
                    /*
                    if (_parent.Level == 0)
                    {
                        SaveSheet(workbook, sheet);
                    }
                    */
                }
            }
            return lastRow;
        }

        private static void ExportRow(int rowIndex, ISheet sheet, HierarchyItem Parent, HierarchyItem Child)
        {
            HierarchyItem item = (Child ?? Parent);

            IRow row = sheet.CreateRow(rowIndex);
            if (Parent != null)
            {
                row.CreateCell(0).SetCellValue(Parent.FileName);
            }
            ICell cell = row.CreateCell(1);
            cell.SetCellValue(item.FileName);
            cell = row.CreateCell(SHEET_START_COL_INDEX + 1);
            cell.SetCellValue(item.Level);
            cell = row.CreateCell(SHEET_START_COL_INDEX + 2);
            cell.SetCellValue(item.Version);
            cell = row.CreateCell(SHEET_START_COL_INDEX + 3);
            cell.SetCellValue(item.Path);
            cell = row.CreateCell(SHEET_START_COL_INDEX + 4);
            cell.SetCellValue(item.CheckedInDate);
            cell = row.CreateCell(SHEET_START_COL_INDEX + 5);
            cell.SetCellValue(item.EntityIcon);
            cell = row.CreateCell(SHEET_START_COL_INDEX + 6);
            cell.SetCellValue(item.FileExtension);
            cell = row.CreateCell(SHEET_START_COL_INDEX + 7);
            cell.SetCellValue(item.Material);
            cell = row.CreateCell(SHEET_START_COL_INDEX + 8);
            cell.SetCellValue(item.RevNumber);
            cell = row.CreateCell(SHEET_START_COL_INDEX + 9);
            cell.SetCellValue(item.Status);
            cell = row.CreateCell(SHEET_START_COL_INDEX + 10);
            cell.SetCellValue(item.TotalVolume);
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
                    else
                    {
                        _desenhos.Add(s);
                    }
                }
            }
        }
    }
}
