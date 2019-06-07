using NeodentUtil.util;
using System;
using System.Collections.Generic;
using System.Text;
//using VaultTools.vault;
//using VaultTools.vault.util;
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
        private static List<string> _desenhos = new List<string>();
        private static Dictionary<long, string> _folders = new Dictionary<long, string>();

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

            LOG.DEBUG = false;

            LOG.debug("Parametros:");
            LOG.debug("===============================");
            LOG.debug("--- vaultserveraddr.=" + vaultserveraddr);
            LOG.debug("--- vaultserver.....=" + vaultserver);
            LOG.debug("--- vaultuser.......=" + vaultuser);
            LOG.debug("--- vaultpass.......=" + vaultpass);

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

                //serviceManager = VaultUtil.Login(vaultserveraddr, vaultserver, vaultuser, vaultpass);
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

                //TesteListaTudo(documentService);
                TesteListaHierarquia(documentService);
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
                    //serviceManager.Dispose();
                    serviceManager.AuthService.SignOut();
                }
                if (conn != null)
                {

                }
            }
        }

        private static void TesteListaHierarquia(ADSK.DocumentService documentService)
        {
            List<ADSK.File> tmpFiles = new List<ADSK.File>();

            List<ExportItem> items = new List<ExportItem>();
            foreach (string desenho in _desenhos)
            {
                //List<ADSK.File> files = FindByFileNameEquals.FindByNameAndExtEquals(serviceManager, documentService, baseRepositories, desenho, validExts, true);
                List<ADSK.File> files = findFiles(serviceManager, documentService, desenho);
                foreach (ADSK.File file in files)
                {
                    tmpFiles.Add(file);
                    items.Add(CalculateFileHierarchy(documentService, file.Id, 0));
                }
            }
            Console.WriteLine("File Name,,,,,,,,,,,Level,Version,Path,Checked In,Entity Icon,File Extension,Material,Rev Number,Status,Total Volume");
            items.Sort();
            PrintResult(items);

        }

        private static void PrintResult(List<ExportItem> items)
        {
            foreach (ExportItem item in items)
            {
                Console.WriteLine(item.ToString());
                PrintResult(item.Children);
            }
        }

        private static ExportItem CalculateFileHierarchy(ADSK.DocumentService documentService, long fileId, int level)
        {
            long[] propsDef = new long[]
            {
                2, //SysName=Status,DispName=Status,DataType=String
                4, //SysName=VersionNumber,DispName=Version,DataType=Numeric
                33, //SysName=RevNumber,DispName=Rev Number,DataType=String
                66, //SysName=Doc Type-{9FD0432E-7422-4E30-B7DD-5E0944F413F2},DispName=Doc Type,DataType=String
                74, //SysName=Design Status-{4D877B8E-1EDF-48B9-B6BC-68EA30BBAA9E},DispName=Design Status,DataType=String
                76, //SysName=Doc Sub Type-{18F13CF6-4E72-4230-B96B-D4BC53E5E467},DispName=Doc Sub Type,DataType=String
                77, //SysName=Doc Sub Type Name-{5A493546-7382-434F-AB4F-003F7811333E},DispName=Doc Sub Type Name,DataType=String
                81, //SysName=External Property Revision-{71F9A929-B943-456C-829E-AAD852A1,DispName=External Property Revision,DataType=String
                83, //SysName=Material,DispName=Material,DataType=String
                86, //SysName=PartNumber,DispName=Part Number,DataType=String
                87, //SysName=Part Property Revision-{DD36BD46-6556-4AC5-AB6F-28F53F2F9EE5,DispName=Part Property Revision,DataType=String
                91, //SysName=Revision Id-{4C65E173-1CB3-4B03-A441-9487B4E1B3D9},DispName=Revision Id,DataType=String
                92, //SysName=Database Id-{9EF1D3E5-6947-485E-833C-A4CB8151BD25},DispName=Database Id,DataType=String
                93, //SysName=Part Icon-{F470C8EE-A1A3-4921-BAAA-411FFFCCBC91},DispName=Part Icon,DataType=Image
                98, //SysName=TotalMass-{A2644BE1-F601-45F1-88E2-252B85557870},DispName=TotalMass,DataType=String
                99, //SysName=TotalVolume-{DF6BD00D-D244-4995-B892-E3AE55AA826E},DispName=TotalVolume,DataType=String
                103, //SysName=UNIDADE-{EA004DF1-45D6-427D-A520-8FEFFD6B7C8B},DispName=UNIDADE,DataType=String
                136, //SysName=Materials-{1FCE51C2-1BD5-4B7A-84EF-E169C0427A1B},DispName=Materials,DataType=String
                168, //SysName=RevisÆo-{65F91A3B-D350-41E4-8D4C-2D73B3C22043},DispName=RevisÆo,DataType=String
                186 //,SysName=Extension,DispName=File Extension,DataType=String
            };
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
