using System.Collections.Generic;
using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault.util
{
    /// <summary>
    /// Busca de arquivos pelo seu nome exato.
    /// </summary>
    class FindByFileNameEquals
    {
        public static List<ADSK.File> Find(ADSK.DocumentService documentService, string[] baseRepositories, string filename)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ FindByFileNameEquals - 1 - (filename=" + filename + ")");
            /* Faz a pesquisa */
            string bookmark = string.Empty;
            ADSK.SrchStatus status = null;

            ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
            conditions[0] = new ADSK.SrchCond();
            conditions[0].SrchOper = Condition.EQUALS.Code; // (long)SrchOperator.IsExactly; // 3; // Is exactly (or equals)
            conditions[0].SrchTxt = filename;
            conditions[0].PropTyp = ADSK.PropertySearchType.SingleProperty;
            conditions[0].PropDefId = 10; //TODO: Está certo?
            conditions[0].SrchRule = ADSK.SearchRuleType.Must;

            long[] folderIds = GetFoldersId.Get(documentService, baseRepositories);

            NeodentUtil.util.LOG.debug("@@@@@@ FindByFileNameEquals - 2 - Vai procurar os arquivos");
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
            NeodentUtil.util.LOG.debug("@@@@@@ FindByFileNameEquals - 3 - arquivos encontrados=" + fileList.Count);
            return fileList;
        }
    }
}
