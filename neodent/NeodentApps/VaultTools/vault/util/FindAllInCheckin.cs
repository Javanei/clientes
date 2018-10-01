using System.Collections.Generic;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;

using NeodentUtil.util;

namespace VaultTools.vault.util
{
    /// <summary>
    /// Lista todos os desenhos em checkin
    /// </summary>
    class FindAllInCheckin
    {
        public static List<ADSK.File> Find(ADSKTools.WebServiceManager serviceManager,
            string[] baseRepositories,
            string[,] validExts,
            bool ignorecheckout)
        {
            LOG.debug("@@@@@@ FindAllInCheckin.Find - 1 - (ignorecheckout=" + ignorecheckout + ")");
            ADSK.DocumentService documentService = serviceManager.DocumentService;

            List<ADSK.File> fileList = new List<ADSK.File>();
            List<ADSK.File> fileListTmp = new List<ADSK.File>();
            List<string> allf = new List<string>();
            long[] folderIds = GetFoldersId.Get(documentService, baseRepositories);

            ADSK.PropDef propClientFileName = VaultUtil.GetPropertyDefinition(serviceManager, "ClientFileName");
            ADSK.PropDef propCheckInDate = VaultUtil.GetPropertyDefinition(serviceManager, "CheckInDate");
            ADSK.PropDef propCheckoutUserName = VaultUtil.GetPropertyDefinition(serviceManager, "CheckoutUserName");
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
                    PropDefId = (int)propCheckInDate.Id
                };
                LOG.debug("@@@@@@ FindAllInCheckin.Find - 2");

                ADSK.SrchCond[] conditions = new ADSK.SrchCond[(validExts.Length / 2) + (ignorecheckout ? 1 : 0)];
                LOG.debug("@@@@@@ FindAllInCheckin.Find - 3 - conditions=" + conditions.Length);
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
                //Condição para filtrar apenas os que não estiverem em checkout
                if (ignorecheckout)
                {
                    conditions[conditions.Length - 1] = new ADSK.SrchCond
                    {
                        SrchOper = Condition.IS_EMPTY.Code,
                        PropTyp = ADSK.PropertySearchType.SingleProperty,
                        PropDefId = (int)propCheckoutUserName.Id,
                        SrchRule = ADSK.SearchRuleType.Must
                    };
                }
                LOG.debug("@@@@@@ FindAllInCheckin.Find - 4 - conditions=" + conditions);

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
                    LOG.debug("@@@@@@ FindAllInCheckin.Find - 5 - Total encontrados=" + status.TotalHits);
                    if (files != null)
                    {
                        foreach (ADSK.File f in files)
                        {
                            fileListTmp.Add(f);
                            for (int i = 0; i < validExts.Length / 2; i++)
                            {
                                if (f.Name.ToLower().EndsWith(validExts[i, 0]))
                                {
                                    string fcode = f.Name.Substring(0, f.Name.Length - validExts[i, 0].Length);
                                    if (!allf.Contains(fcode))
                                    {
                                        allf.Add(fcode);
                                        ADSK.File file = VaultUtil.FindFileWithDownloadExtension(serviceManager,
                                            documentService,
                                            baseRepositories,
                                            fcode,
                                            f,
                                            validExts[i, 0],
                                            validExts[i, 1]);
                                        if (file != null)
                                        {
                                            fileList.Add(file);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            LOG.debug("@@@@@@ FindAllInCheckin.Find - 6 - result=" + fileList.Count);
            return fileList;
        }
    }
}
