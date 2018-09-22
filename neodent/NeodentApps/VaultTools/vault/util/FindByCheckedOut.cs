using System.Collections.Generic;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault.util
{
    /// <summary>
    /// Busca dos arquivos em Checkout
    /// </summary>
    class FindByCheckedOut
    {
        public static List<ADSK.File> Find(ADSKTools.WebServiceManager serviceManager, string[] baseRepositories, string[] validExt)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ FindByCheckedOut - 1");
            ADSK.DocumentService documentService = serviceManager.DocumentService;

            List<ADSK.File> fileList = new List<ADSK.File>();
            List<ADSK.File> fileListTmp = new List<ADSK.File>();
            List<string> allf = new List<string>();
            long[] folderIds = GetFoldersId.Get(documentService, baseRepositories);
            long propid;

            ADSK.PropDef propClientFileName = VaultUtil.GetPropertyDefinition(serviceManager, "ClientFileName");
            ADSK.PropDef propCheckInDate = VaultUtil.GetPropertyDefinition(serviceManager, "CheckInDate");
            ADSK.PropDef propCheckoutUserName = VaultUtil.GetPropertyDefinition(serviceManager, "CheckoutUserName");
            if (propCheckoutUserName != null)
            {
                propid = (int)propCheckoutUserName.Id;
                /* Faz a pesquisa dos arquivos cujo usuario de checkout esta preenchido */
                string bookmark = string.Empty;
                ADSK.SrchStatus status = null;

                ADSK.SrchSort[] sort = new ADSK.SrchSort[1];
                sort[0] = new ADSK.SrchSort
                {
                    SortAsc = true,
                    PropDefId = propid
                };
                /*
                ADSK.SrchCond[] conditions = new ADSK.SrchCond[validExt.Length + 1];
                conditions[0] = new ADSK.SrchCond
                {
                    SrchOper = Condition.IS_NOT_EMPTY.Code,
                    PropTyp = ADSK.PropertySearchType.SingleProperty,
                    PropDefId = propid,
                    SrchRule = ADSK.SearchRuleType.Must
                };
                for (int i = 0; i < validExt.Length; i++)
                {
                    conditions[i + 1] = new ADSK.SrchCond
                    {
                        SrchOper = Condition.CONTAINS.Code,
                        SrchTxt = validExt[i],
                        PropTyp = ADSK.PropertySearchType.SingleProperty,
                        PropDefId = (int)propClientFileName.Id,
                        SrchRule = ADSK.SearchRuleType.May
                    };
                }
                */

                ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
                conditions[0] = new ADSK.SrchCond
                {
                    SrchOper = Condition.IS_NOT_EMPTY.Code,
                    PropTyp = ADSK.PropertySearchType.SingleProperty,
                    PropDefId = propid,
                    SrchRule = ADSK.SearchRuleType.Must
                };

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
                    NeodentUtil.util.LOG.debug("@@@@@@ FindByCheckedOut - 2 - Total encontrados=" + status.TotalHits);
                    if (files != null)
                    {
                        foreach (ADSK.File f in files)
                        {
                            fileListTmp.Add(f);
                            foreach (string ext in validExt)
                            {
                                if (f.Name.ToLower().EndsWith(ext))
                                {
                                    string fcode = f.Name.Substring(0, f.Name.Length - ext.Length);
                                    if (!allf.Contains(fcode))
                                    {
                                        allf.Add(fcode);
                                        fileList.Add(f);
                                        /*NeodentUtil.util.LOG.debug("@@@@@@@@ FindByCheckedOut - 3 - adicionado checkout: code=" + fcode
                                            + ", Name=" + f.Name + ", Size=" + f.FileSize);*/
                                    }
                                }
                            }
                        }
                    }
                }
            }
            NeodentUtil.util.LOG.debug("@@@@@@ FindByCheckedOut - 4 - result=" + fileList.Count);
            return fileList;
        }
    }
}
