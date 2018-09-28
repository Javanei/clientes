using System.Collections.Generic;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault.util
{
    /// <summary>
    /// Busca de arquivos pelo nome parcial (match).
    /// </summary>
    class FindByFileNameMatches
    {
        public static List<ADSK.File> Find(ADSKTools.WebServiceManager serviceManager, ADSK.DocumentService documentService,
            string[] baseRepositories, string sfind)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ FindByFileNameMatches.Find - 1 - (sfind=" + sfind + ")");

            ADSK.PropDef propClientFileName = VaultUtil.GetPropertyDefinition(serviceManager, "ClientFileName");

            /* Faz a pesquisa */
            string bookmark = string.Empty;
            ADSK.SrchStatus status = null;

            ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
            conditions[0] = new ADSK.SrchCond
            {
                SrchOper = Condition.CONTAINS.Code, // (long)SrchOperator.Contains; // 1; // Contains
                SrchTxt = sfind,
                PropTyp = ADSK.PropertySearchType.SingleProperty,
                PropDefId = (int)propClientFileName.Id,
                SrchRule = ADSK.SearchRuleType.Must
            };

            long[] folderIds = GetFoldersId.Get(documentService, baseRepositories);

            NeodentUtil.util.LOG.debug("@@@@@@ FindByFileNameMatches.Find - 2 - Vai procurar os arquivos");
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
            NeodentUtil.util.LOG.debug("@@@@@@ FindByFileNameMatches.Find - 3 - arquivos encontrados=" + fileList.Count);
            return fileList;
        }
    }
}
