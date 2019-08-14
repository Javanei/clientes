using NeodentUtil.util;
using System.Collections.Generic;
using ADSK = Autodesk.Connectivity.WebServices;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace VaultTools.vault.util
{
    public class FindAll
    {
        public static List<ADSK.File> Find(VDF.Vault.Currency.Connections.Connection conn,
                                           string[] baseRepositories,
                                           string[,] validExts)
        {
            LOG.debug("@@@@@@ FindAll.Find - 1");
            ADSKTools.WebServiceManager serviceManager = conn.WebServiceManager;
            ADSK.DocumentService documentService = serviceManager.DocumentService;

            List<ADSK.File> fileList = new List<ADSK.File>();
            List<ADSK.File> fileListTmp = new List<ADSK.File>();
            List<string> allf = new List<string>();
            long[] folderIds = GetFoldersId.Get(documentService, baseRepositories);

            ADSK.PropDef propClientFileName = VaultUtil.GetPropertyDefinition(serviceManager, "ClientFileName");
            if (propClientFileName != null)
            {
                fileListTmp = new List<ADSK.File>();
                /* Faz a pesquisa */
                string bookmark = string.Empty;
                ADSK.SrchStatus status = null;

                ADSK.SrchSort[] sort = new ADSK.SrchSort[1];
                sort[0] = new ADSK.SrchSort
                {
                    SortAsc = true,
                    PropDefId = (int)propClientFileName.Id
                };
                LOG.debug("@@@@@@ FindAll.Find - 2");

                ADSK.SrchCond[] conditions = new ADSK.SrchCond[(validExts.Length / 2)];
                LOG.debug("@@@@@@ FindAll.Find - 3 - conditions=" + conditions.Length);
                for (int i = 0; i < validExts.Length / 2; i++)
                {
                    conditions[i] = new ADSK.SrchCond
                    {
                        SrchOper = Condition.CONTAINS.Code,
                        SrchTxt = validExts[i, 0],
                        PropTyp = ADSK.PropertySearchType.SingleProperty,
                        PropDefId = (int)propClientFileName.Id,
                        SrchRule = ADSK.SearchRuleType.May
                    };
                }
                LOG.debug("@@@@@@ FindAll.Find - 4 - conditions=" + conditions);

                while (status == null || fileListTmp.Count < status.TotalHits)
                {
                    ADSK.File[] files = documentService.FindFilesBySearchConditions(
                        conditions, /*SrchCond [] conditions*/
                        sort, /*SrchSort [] sortConditions*/
                        folderIds, /*Long [] folderIds*/
                        true, /*Boolean recurseFolders*/
                        true, /*Boolean latestOnly*/
                        ref bookmark, /*[out] String bookmark*/
                        out status /*[out] SrchStatus searchstatus*/
                    );
                    if (files != null)
                    {
                        foreach (ADSK.File f in files)
                        {
                            fileListTmp.Add(f);
                        }
                    }
                }
            }
            LOG.debug("@@@@@@ FindAll.Find - 5 - Arquivos encontrados=" + fileListTmp.Count);
            for (int i = 0; i < validExts.Length / 2; i++)
            {
                foreach (ADSK.File file in fileListTmp)
                {
                    if (file.Name.ToLower().EndsWith(validExts[i, 0].ToLower()))
                    {
                        fileList.Add(file);
                    }
                }
            }
            LOG.debug("@@@@@@ FindAll.Find - 6 - result=" + fileList.Count);
            return fileList;
        }
    }
}