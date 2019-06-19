using NeodentUtil.util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ADSK = Autodesk.Connectivity.WebServices;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace VaultExport
{
    class Program
    {
        private static readonly string[] baseRepositories = new string[] { "$/Neodent/Produção" };
        private static readonly string[,] validExts = new string[,] { { ".idw", ".idw" } };

        private static readonly Encoding srcEnc = Encoding.GetEncoding("Windows-1252");
        private static readonly Encoding dstEnc = Encoding.GetEncoding("ibm850");

        // Configuração
        private static string vaultuser = "integracao";
        private static string vaultpass = "brasil2010";
        private static string vaultserveraddr = "br03s059.straumann.com";
        private static string vaultserver = "neodent";
        private static string exportfile = "desenhos.xlsx";
        private static List<string> _desenhos = new List<string>();
        private static Dictionary<long, string> _folders = new Dictionary<long, string>();
        private const int SHEET_START_COL_INDEX = 15;

        private static ADSKTools.WebServiceManager serviceManager = null;
        private static VDF.Vault.Services.Connection.IPropertyManager propertyManager;
        private static VDF.Vault.Currency.Connections.Connection conn = null;

        private static ADSK.PropDef propClientFileName;
        private static ADSK.PropDef propFileExtension;
        private static ADSK.PropDef propRevNumber;
        private static ADSK.PropDef propTotalVolume;
        private static ADSK.PropDef propMaterial;
        private static VDF.Vault.Currency.Properties.PropertyDefinitionDictionary propDefsDict;
        private static VDF.Vault.Currency.Properties.PropertyDefinition propEntityIcon;

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

                serviceManager = conn.WebServiceManager;
                propertyManager = conn.PropertyManager;

                propClientFileName = GetPropertyDefinition("ClientFileName");
                propFileExtension = GetPropertyDefinition("Extension");
                propRevNumber = GetPropertyDefinition("RevNumber");
                propTotalVolume = GetPropertyDefinition("TotalVolume");
                propMaterial = GetPropertyDefinition("Material");

                propDefsDict = propertyManager.GetPropertyDefinitions(
                    VDF.Vault.Currency.Entities.EntityClassIds.Files,
                    null,
                    VDF.Vault.Currency.Properties.PropertyDefinitionFilter.IncludeAll
                );
                propEntityIcon = propDefsDict[VDF.Vault.Currency.Properties.PropertyDefinitionIds.Client.EntityIcon];

                ADSK.DocumentService documentService = serviceManager.DocumentService;

                if (_desenhos.Count == 0)
                {
                    _desenhos.Add("115.249_D");
                    _desenhos.Add("104.050_D");
                    _desenhos.Add("118.345-1_D");
                }

                List<ExportItem> items = PreparaListaHierarquia(documentService);
                ExportResult(0, 1, sheet, items);
            }
            catch (Exception eManager)
            {
                LOG.error("Não conseguiu conectar o Vault: " + eManager.Message);
                LOG.error(eManager.StackTrace);
            }
            finally
            {
                if (serviceManager != null)
                {
                    serviceManager.AuthService.SignOut();
                }
                if (conn != null)
                {
                }
                for (int i = SHEET_START_COL_INDEX + 1; i <= SHEET_START_COL_INDEX + 10; i++)
                {
                    sheet.AutoSizeColumn(i);
                }
                using (var fs = new FileStream(exportfile, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fs);
                }
            }
        }

        private static List<ExportItem> PreparaListaHierarquia(ADSK.DocumentService documentService)
        {
            List<ADSK.File> tmpFiles = new List<ADSK.File>();

            List<ExportItem> items = new List<ExportItem>();
            foreach (string desenho in _desenhos)
            {
                List<ADSK.File> files = findFiles(serviceManager, documentService, desenho);
                foreach (ADSK.File file in files)
                {
                    tmpFiles.Add(file);
                    items.Add(CalculateFileHierarchy(documentService, file.Id, 0));
                }
            }
            items.Sort();
            return items;
        }

        private static int ExportResult(int startCol, int startRow, ISheet sheet, List<ExportItem> items)
        {
            int lastRow = startRow;
            if (items != null && items.Count > 0)
            {
                foreach (ExportItem item in items)
                {
                    ExportRow(lastRow, sheet, item);
                    lastRow = ExportResult(startCol + 1, ++lastRow, sheet, item.Children);
                }
            }
            return lastRow;
        }

        private static void ExportRow(int rowIndex, ISheet sheet, ExportItem item)
        {
            IRow row = sheet.CreateRow(rowIndex);
            ICell cell = row.CreateCell(item.Level);
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

        private static ExportItem CalculateFileHierarchy(ADSK.DocumentService documentService, long fileId, int level)
        {
            ADSK.File parent = documentService.GetFileById(fileId);
            ADSK.Folder folder = documentService.GetFolderById(parent.FolderId);
            ExportItem row = new ExportItem
            {
                FileName = parent.Name,
                Level = level,
                Version = parent.VerNum,
                Path = dstEnc.GetString(srcEnc.GetBytes(folder.FullName)),
                CheckedInDate = parent.CkInDate != null ? parent.CkInDate.ToString() : "",
                EntityIcon = parent.Name.Substring(parent.Name.LastIndexOf('.')),
                Material = "",
                RevNumber = "",
                Status = parent.FileStatus.ToString(),
                TotalVolume = ""
            };

            ADSK.PropInst[] properties = serviceManager.PropertyService.GetProperties("FILE",
                new long[] { parent.Id },
                new long[] { propRevNumber.Id, propFileExtension.Id, propTotalVolume.Id, propMaterial.Id }
                );

            if (properties != null && properties.Length > 0)
            {
                foreach (ADSK.PropInst pinst in properties)
                {
                    if (pinst.PropDefId == propRevNumber.Id)
                    {
                        row.RevNumber = (string)pinst.Val;
                    }
                    else if (pinst.PropDefId == propFileExtension.Id)
                    {
                        row.FileExtension = (string)pinst.Val;
                    }
                    else if (pinst.PropDefId == propTotalVolume.Id)
                    {
                        row.TotalVolume = (string)pinst.Val;
                    }
                    else if (pinst.PropDefId == propMaterial.Id)
                    {
                        row.Material = (string)pinst.Val;
                    }
                }
            }
            VDF.Vault.Currency.Entities.FileIteration fi = new VDF.Vault.Currency.Entities.FileIteration(conn, parent);

            try
            {
                object propValue = propertyManager.GetPropertyValue(fi, propEntityIcon, null);
                if (propValue != null)
                {
                    VDF.Vault.Currency.Properties.ImageInfo img = (VDF.Vault.Currency.Properties.ImageInfo)propValue;
                    row.EntityIcon = img.Description;
                }
            }
            catch (Exception ex) { }

            ADSK.FileAssocArray[] associations = documentService.GetFileAssociationsByIds(
                        new long[] { parent.Id },
                        ADSK.FileAssociationTypeEnum.Dependency, //parentAssociationType
                        false, //parentRecurse
                        ADSK.FileAssociationTypeEnum.Dependency, //childAssociationType
                        true, //childRecurse
                        false, //includeRelatedDocuments
                        true //includeHidden
                        );
            if (associations != null && associations.Length > 0)
            {
                foreach (ADSK.FileAssocArray assoc in associations)
                {
                    ADSK.FileAssoc[] fileAssocs = assoc.FileAssocs;
                    if (fileAssocs != null && fileAssocs.Length > 0)
                    {
                        foreach (ADSK.FileAssoc fa in fileAssocs)
                        {
                            if (fa.CldFile.Id != fileId && fa.ParFile.Id == fileId)
                            {
                                /*if (fa.CldFile.Name.EndsWith(".idw")
                                    || fa.CldFile.Name.EndsWith(".ipt")
                                    || fa.CldFile.Name.EndsWith(".ipn")
                                    || fa.CldFile.Name.EndsWith(".iam"))
                                {*/
                                row.Children.Add(CalculateFileHierarchy(documentService,
                                    fa.CldFile.Id,
                                    level + 1
                                    ));
                                /*}*/
                            }
                        }
                    }
                }
            }

            return row;
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

        private static List<ADSK.File> findFiles(ADSKTools.WebServiceManager serviceManager,
            ADSK.DocumentService documentService,
            string baseFileName)
        {
            List<ADSK.File> result = new List<ADSK.File>();

            for (int i = 0; i < validExts.Length / 2; i++)
            {
                string fileName = baseFileName + validExts[i, 0];

                string bookmark = string.Empty;
                ADSK.SrchStatus status = null;

                ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
                conditions[0] = new ADSK.SrchCond
                {
                    SrchOper = Condition.EQUALS.Code,
                    SrchTxt = fileName,
                    PropTyp = ADSK.PropertySearchType.SingleProperty,
                    PropDefId = (int)propClientFileName.Id,
                    SrchRule = ADSK.SearchRuleType.Must
                };

                long[] folderIds = GetFoldersId(documentService, baseRepositories);

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

                if (fileList.Count > 0)
                {
                    result.AddRange(fileList);
                }
            }
            return result;
        }

        private static ADSK.PropDef GetPropertyDefinition(string propName)
        {
            ADSK.PropDef res = null;
            foreach (ADSK.PropDef prop in serviceManager.PropertyService.GetPropertyDefinitionsByEntityClassId("FILE"))
            {
                if (prop.SysName == propName)
                {
                    res = prop;
                }
            }
            if (res == null)
            {
                foreach (ADSK.PropDef prop in serviceManager.PropertyService.GetPropertyDefinitionsByEntityClassId("FILE"))
                {
                    if (prop.DispName == propName)
                    {
                        res = prop;
                    }
                }
            }
            return res;
        }

        private static long[] GetFoldersId(ADSK.DocumentService documentService, string[] baseRepositories)
        {
            ADSK.Folder[] fld = documentService.FindFoldersByPaths(baseRepositories);
            long[] folderIds = new long[fld != null ? fld.Length : 1];
            if (fld != null)
            {
                for (int i = 0; i < fld.Length; i++)
                {
                    folderIds[i] = fld[i].Id;
                }
            }
            return folderIds;
        }
    }
}
