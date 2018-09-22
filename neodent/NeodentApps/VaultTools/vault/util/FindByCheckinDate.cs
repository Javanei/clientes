using System;
using System.Collections.Generic;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault.util
{
    /// <summary>
    /// Busca de arquivos com data de checkin posterior a data informada.
    /// </summary>
    class FindByCheckinDate
    {
        public static List<ADSK.File> Find(ADSKTools.WebServiceManager serviceManager, string[] baseRepositories,
            string[] validExt, String checkinDate)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ FindByCheckinDate - 1 - (checkinDate=" + checkinDate + ")");
            ADSK.DocumentService documentService = serviceManager.DocumentService;

            //LOG.imprimeLog(System.DateTime.Now + " ===== findByCheckinDate: [" + checkinDate + "]");
            List<ADSK.File> fileList = new List<ADSK.File>();
            List<ADSK.File> fileListTmp = new List<ADSK.File>();
            List<string> allf = new List<string>();
            long[] folderIds = GetFoldersId.Get(documentService, baseRepositories);
            long propid;

            ADSK.PropDef propClientFileName = VaultUtil.GetPropertyDefinition(serviceManager, "ClientFileName");
            NeodentUtil.util.LOG.debug("@@@@@@ FindAllInCheckin - 4 - propClientFileName=" + propClientFileName);
            ADSK.PropDef propCheckInDate = VaultUtil.GetPropertyDefinition(serviceManager, "CheckInDate");
            NeodentUtil.util.LOG.debug("@@@@@@ FindAllInCheckin - 5 - propCheckInDate=" + propCheckInDate);
            ADSK.PropDef propCheckoutUserName = VaultUtil.GetPropertyDefinition(serviceManager, "CheckoutUserName");
            NeodentUtil.util.LOG.debug("@@@@@@ FindAllInCheckin - 6 - propCheckoutUserName=" + propCheckoutUserName);
            if (propCheckInDate != null)
            {
                fileListTmp = new List<ADSK.File>();
                propid = (int)propCheckInDate.Id;
                /* Faz a pesquisa */
                string bookmark = string.Empty;
                ADSK.SrchStatus status = null;

                ADSK.SrchSort[] sort = new ADSK.SrchSort[1];
                sort[0] = new ADSK.SrchSort
                {
                    SortAsc = true,
                    PropDefId = propid
                };
                ADSK.SrchCond[] conditions = new ADSK.SrchCond[validExt.Length + 2];
                //Condição para filtrar apenas os que não estiverem em checkout
                conditions[0] = new ADSK.SrchCond
                {
                    SrchOper = Condition.IS_EMPTY.Code,
                    PropTyp = ADSK.PropertySearchType.SingleProperty,
                    PropDefId = (int)propCheckoutUserName.Id,
                    SrchRule = ADSK.SearchRuleType.Must
                };
                conditions[1] = new ADSK.SrchCond
                {
                    SrchOper = Condition.GREATER_THAN.Code,
                    SrchTxt = checkinDate,
                    PropTyp = ADSK.PropertySearchType.SingleProperty,
                    PropDefId = propid,
                    SrchRule = ADSK.SearchRuleType.Must
                };
                for (int i = 0; i < validExt.Length; i++)
                {
                    conditions[i + 2] = new ADSK.SrchCond
                    {
                        SrchOper = Condition.CONTAINS.Code,
                        SrchTxt = validExt[i],
                        PropTyp = ADSK.PropertySearchType.SingleProperty,
                        PropDefId = (int)propClientFileName.Id,
                        SrchRule = ADSK.SearchRuleType.May
                    };
                }

                /*
                ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
                conditions[0] = new ADSK.SrchCond
                {
                    SrchOper = Condition.GREATER_THAN_OR_EQUAL.Code,
                    SrchTxt = checkinDate,
                    PropTyp = ADSK.PropertySearchType.SingleProperty,
                    PropDefId = propid,
                    SrchRule = ADSK.SearchRuleType.Must
                };
                */

                //prop = VaultUtil.GetPropertyDefinition(serviceManager, "ClientFileName");

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
                    NeodentUtil.util.LOG.debug("@@@@@@ FindByCheckinDate - 2 - Total encontrados=" + status.TotalHits);
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
                                        /*NeodentUtil.util.LOG.debug("@@@@@@@@ FindByCheckinDate - 3 - adicionado via data de checkin: code=" + fcode
                                            + ", Name=" + f.Name + ", Size=" + f.FileSize + ", CkInDate=" + f.CkInDate + ", CheckedOut=" + f.CheckedOut);*/
                                    }
                                }
                            }
                        }
                    }
                }
            }
            NeodentUtil.util.LOG.debug("@@@@@@ FindByCheckinDate - 4 - result=" + fileList.Count);
            return fileList;
        }
    }
}
