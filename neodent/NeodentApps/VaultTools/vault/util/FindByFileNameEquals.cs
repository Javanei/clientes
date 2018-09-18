using System.Collections.Generic;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault.util
{
    /// <summary>
    /// Busca de arquivos pelo seu nome exato.
    /// </summary>
    class FindByFileNameEquals
    {
        public static List<ADSK.File> FindByNameAndExtEquals(ADSKTools.WebServiceManager serviceManager, ADSK.DocumentService documentService,
            string[] baseRepositories, string filename, string[] validExt)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@ FindByFileNameEquals.FindByNameAndExtEquals - 1 - (filename=" + filename + ")");

            List<ADSK.File> fl = FindByNameEquals(serviceManager, documentService, baseRepositories, filename);
            NeodentUtil.util.LOG.debug("@@@@@@@@ FindByFileNameEquals.FindByNameAndExtEquals - 2 - encontrados=" + fl.Count);
            if (fl.Count == 0)
            {
                if (validExt != null && validExt.Length > 0)
                {
                    foreach (string ext in validExt)
                    {
                        fl = FindByNameEquals(serviceManager, documentService, baseRepositories, filename + ext);
                        NeodentUtil.util.LOG.debug("@@@@@@@@ FindByFileNameEquals.FindByNameAndExtEquals - 3 - encontrados com extensao '" + ext + "'=" + fl.Count);
                        if (fl.Count > 0)
                        {
                            break;
                        }
                    }
                }
            }
            NeodentUtil.util.LOG.debug("@@@@@@@@ FindByFileNameEquals.FindByNameAndExtEquals - 4 - resultado=" + fl.Count);
            return fl;
        }

        public static List<ADSK.File> FindByNameEquals(ADSKTools.WebServiceManager serviceManager, ADSK.DocumentService documentService,
            string[] baseRepositories, string filename)
        {
            NeodentUtil.util.LOG.debug("@@@@@@ FindByFileNameEquals.Find - 1 - (filename=" + filename + ")");
            /* Faz a pesquisa */
            string bookmark = string.Empty;
            ADSK.SrchStatus status = null;

            ADSK.PropDef propClientFileName = VaultUtil.GetPropertyDefinition(serviceManager, "ClientFileName");

            ADSK.SrchCond[] conditions = new ADSK.SrchCond[1];
            conditions[0] = new ADSK.SrchCond
            {
                SrchOper = Condition.EQUALS.Code,
                SrchTxt = filename,
                PropTyp = ADSK.PropertySearchType.SingleProperty,
                PropDefId = (int)propClientFileName.Id,
                SrchRule = ADSK.SearchRuleType.Must
            };

            long[] folderIds = GetFoldersId.Get(documentService, baseRepositories);

            NeodentUtil.util.LOG.debug("@@@@@@ FindByFileNameEquals.Find - 2 - Vai procurar os arquivos");
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
            NeodentUtil.util.LOG.debug("@@@@@@ FindByFileNameEquals.Find - 3 - arquivos encontrados=" + fileList.Count);
            return fileList;
        }
    }
}
