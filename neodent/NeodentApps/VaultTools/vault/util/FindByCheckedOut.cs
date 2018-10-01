using System.Collections.Generic;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;

using NeodentUtil.util;

namespace VaultTools.vault.util
{
    /// <summary>
    /// Busca dos arquivos em Checkout
    /// </summary>
    class FindByCheckedOut
    {
        public static List<ADSK.File> Find(ADSKTools.WebServiceManager serviceManager,
            string[] baseRepositories,
            string[,] validExts)
        {
            LOG.debug("@@@@@@ FindByCheckedOut.Find - 1");
            ADSK.DocumentService documentService = serviceManager.DocumentService;

            List<ADSK.File> fileList = new List<ADSK.File>();
            List<ADSK.File> fileListTmp = new List<ADSK.File>();
            List<string> allf = new List<string>();
            long[] folderIds = GetFoldersId.Get(documentService, baseRepositories);
            long propid;

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
                    LOG.debug("@@@@@@ FindByCheckedOut.Find - 2 - Total encontrados=" + status.TotalHits);
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
            LOG.debug("@@@@@@ FindByCheckedOut.Find - 3 - result=" + fileList.Count);
            return fileList;
        }
    }
}
