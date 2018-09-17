using System;
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

            ADSK.PropDef prop = VaultUtil.GetPropertyDefinition(serviceManager, "CheckoutUserName");
            if (prop != null)
            {
                propid = (int)prop.Id;
                /* Faz a pesquisa dos arquivos cujo usuario de checkout esta preenchido */
                string bookmark = string.Empty;
                ADSK.SrchStatus status = null;

                ADSK.SrchSort[] sort = new ADSK.SrchSort[1];
                sort[0] = new ADSK.SrchSort
                {
                    SortAsc = true,
                    PropDefId = propid
                };
                ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
                conditions[0] = new ADSK.SrchCond
                {
                    SrchOper = Condition.IS_NOT_EMPTY.Code,
                    PropTyp = ADSK.PropertySearchType.SingleProperty,
                    PropDefId = propid
                };

                //prop = VaultUtil.GetPropertyDefinition(serviceManager, "ClientFileName");
                NeodentUtil.util.LOG.debug("@@@@@@ FindByCheckedOut - 2 - Total encontrados=" + status.TotalHits);

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
                            foreach (string ext in validExt)
                            {
                                if (f.Name.ToLower().EndsWith(ext))
                                {
                                    string fcode = f.Name.Substring(0, f.Name.Length - ext.Length);
                                    if (!allf.Contains(fcode))
                                    {
                                        allf.Add(fcode);
                                        fileList.Add(f);
                                        NeodentUtil.util.LOG.debug("@@@@@@@@ FindByCheckedOut - 3 - adicionado checkout: code=" + fcode
                                            + ", Name=" + f.Name + ", Size=" + f.FileSize);
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
