using NeodentUtil.util;
using System;
using System.Collections.Generic;
using System.Text;
using ADSK = Autodesk.Connectivity.WebServices;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace VaultTools.vault.util
{
    public class FindHierarchy
    {
        private static readonly Encoding srcEnc = Encoding.GetEncoding("Windows-1252");
        private static readonly Encoding dstEnc = Encoding.GetEncoding("ibm850");

        private static ADSK.PropDef propClientFileName = null;
        private static ADSK.PropDef propFileExtension = null;
        private static ADSK.PropDef propRevNumber = null;
        private static ADSK.PropDef propTotalVolume = null;
        private static ADSK.PropDef propMaterial = null;
        private static VDF.Vault.Currency.Properties.PropertyDefinitionDictionary propDefsDict;
        private static VDF.Vault.Currency.Properties.PropertyDefinition propEntityIcon;

        public static List<HierarchyItem> Find(
            VDF.Vault.Currency.Connections.Connection conn,
            string[] baseRepositories,
            string[,] validExts,
            List<string> Desenhos)
        {
            ADSKTools.WebServiceManager serviceManager = conn.WebServiceManager;
            VDF.Vault.Services.Connection.IPropertyManager propertyManager = conn.PropertyManager;

            if (propRevNumber == null)
            {
                propRevNumber = VaultUtil.GetPropertyDefinition(serviceManager, "RevNumber");
                propClientFileName = VaultUtil.GetPropertyDefinition(serviceManager, "ClientFileName");
                propFileExtension = VaultUtil.GetPropertyDefinition(serviceManager, "Extension");
                propTotalVolume = VaultUtil.GetPropertyDefinition(serviceManager, "TotalVolume");
                propMaterial = VaultUtil.GetPropertyDefinition(serviceManager, "Material");
                propDefsDict = propertyManager.GetPropertyDefinitions(
                    VDF.Vault.Currency.Entities.EntityClassIds.Files,
                    null,
                    VDF.Vault.Currency.Properties.PropertyDefinitionFilter.IncludeAll
                );
                propEntityIcon = propDefsDict[VDF.Vault.Currency.Properties.PropertyDefinitionIds.Client.EntityIcon];
            }

            return PreparaListaHierarquia(conn, baseRepositories, validExts, Desenhos);
        }

        private static List<HierarchyItem> PreparaListaHierarquia(
            VDF.Vault.Currency.Connections.Connection conn,
            string[] baseRepositories,
            string[,] validExts,
            List<string> Desenhos)
        {
            ADSKTools.WebServiceManager serviceManager = conn.WebServiceManager;
            VDF.Vault.Services.Connection.IPropertyManager propertyManager = conn.PropertyManager;
            ADSK.DocumentService documentService = serviceManager.DocumentService;

            List<ADSK.File> tmpFiles = new List<ADSK.File>();

            List<HierarchyItem> items = new List<HierarchyItem>();
            foreach (string desenho in Desenhos)
            {
                List<ADSK.File> files = FindFiles(serviceManager, documentService, baseRepositories, validExts, desenho);
                foreach (ADSK.File file in files)
                {
                    tmpFiles.Add(file);
                    items.Add(CalculateFileHierarchy(
                        conn,
                        serviceManager,
                        documentService,
                        propertyManager,
                        file.Id,
                        0));
                }
            }
            items.Sort();
            return items;
        }

        private static HierarchyItem CalculateFileHierarchy(
            VDF.Vault.Currency.Connections.Connection conn,
            ADSKTools.WebServiceManager serviceManager,
            ADSK.DocumentService documentService,
            VDF.Vault.Services.Connection.IPropertyManager propertyManager,
            long fileId,
            int level)
        {
            ADSK.File parent = documentService.GetFileById(fileId);
            ADSK.Folder folder = documentService.GetFolderById(parent.FolderId);
            HierarchyItem row = new HierarchyItem
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
            VDF.Vault.Currency.Entities.FileIteration fi = new VDF.Vault.Currency.Entities.FileIteration(
                conn,
                parent);

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
                                row.Children.Add(CalculateFileHierarchy(
                                    conn,
                                    serviceManager,
                                    documentService,
                                    propertyManager,
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

        private static List<ADSK.File> FindFiles(ADSKTools.WebServiceManager serviceManager,
                ADSK.DocumentService documentService,
                string[] baseRepositories,
                string[,] validExts,
                string baseFileName)
        {
            LOG.debug("Iniciando busca no Vault do desenho: " + baseFileName);
            List<ADSK.File> result = new List<ADSK.File>();

            for (int i = 0; i < validExts.Length / 2; i++)
            {
                LOG.debug("  Buscando com extensao " + validExts[i, 0]);
                string bookmark = string.Empty;
                ADSK.SrchStatus status = null;

                ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
                if (baseFileName.Equals("*"))
                {
                    conditions[0] = new ADSK.SrchCond
                    {
                        SrchOper = Condition.CONTAINS.Code,
                        SrchTxt = validExts[i, 0],
                        PropTyp = ADSK.PropertySearchType.SingleProperty,
                        PropDefId = (int)propClientFileName.Id,
                        SrchRule = ADSK.SearchRuleType.May
                    };
                }
                else
                {
                    string fileName = baseFileName + validExts[i, 0];

                    conditions[0] = new ADSK.SrchCond
                    {
                        SrchOper = Condition.EQUALS.Code,
                        SrchTxt = fileName,
                        PropTyp = ADSK.PropertySearchType.SingleProperty,
                        PropDefId = (int)propClientFileName.Id,
                        SrchRule = ADSK.SearchRuleType.Must
                    };
                }

                long[] folderIds = GetFoldersId.Get(documentService, baseRepositories);

                LOG.debug("Total de hits: " + (status == null ? "null" : status.TotalHits.ToString()));
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
                    foreach (ADSK.File f in fileList)
                    {
                        if (f.Name.EndsWith(validExts[i, 0]))
                        {
                            result.Add(f);
                        }
                    }
                }
            }
            LOG.debug("Arquivos encontrados para o nome '" + baseFileName + "': " + result.Count);
            return result;
        }

    }
}
