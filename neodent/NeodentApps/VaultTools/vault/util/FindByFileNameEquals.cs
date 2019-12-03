using System.Collections.Generic;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;
using NeodentUtil.util;

namespace VaultTools.vault.util
{
    /// <summary>
    /// Busca de arquivos pelo seu nome exato.
    /// </summary>
    public class FindByFileNameEquals
    {
        public static List<ADSK.File> FindByNameAndExtEquals(ADSKTools.WebServiceManager serviceManager,
            ADSK.DocumentService documentService,
            string[] baseRepositories,
            string filename,
            string[,] validExts,
            bool ignoreCheckout)
        {
            string dir = "";
            foreach (string s in baseRepositories)
            {
                if (dir.Length > 0) dir = dir + ",";
                dir = dir + s;
            }
            LOG.debug("@@@@@@@@ FindByFileNameEquals.FindByNameAndExtEquals - 1 - (filename=" + filename
                + ", ignoreCheckout=" + ignoreCheckout + ", baseRepo=" + dir + ")");

            List<ADSK.File> fl = ignoreCheckout
                ? FindByNameEquals(serviceManager, documentService, baseRepositories, filename)
                : FindByNameEqualsCheckinOnly(serviceManager, documentService, baseRepositories, filename);
            LOG.debug("@@@@@@@@ FindByFileNameEquals.FindByNameAndExtEquals - 2 - encontrados=" + fl.Count);
            if (fl.Count == 0)
            {
                if (validExts != null && validExts.Length > 0)
                {
                    for (int i = 0; i < validExts.Length / 2; i++)
                    {
                        List<ADSK.File> fls = ignoreCheckout
                            ? FindByNameEquals(serviceManager, documentService, baseRepositories, filename + validExts[i, 0])
                            : FindByNameEqualsCheckinOnly(serviceManager, documentService, baseRepositories, filename + validExts[i, 0]);
                        LOG.debug("@@@@@@@@ FindByFileNameEquals.FindByNameAndExtEquals - 3 - encontrados com extensao '" + validExts[i, 0] + "'=" + fl.Count);
                        if (fls.Count > 0)
                        {
                            fl = VaultUtil.FindFileWithDownloadExtension(serviceManager, documentService, baseRepositories, filename, fls, validExts[i, 0], validExts[i, 1]);
                        }
                    }
                }
            }
            LOG.debug("@@@@@@@@ FindByFileNameEquals.FindByNameAndExtEquals - 4 - resultado=" + fl.Count);
            return fl;
        }

        public static List<ADSK.File> FindByNameEqualsCheckinOnly(ADSKTools.WebServiceManager serviceManager,
            ADSK.DocumentService documentService,
            string[] baseRepositories,
            string filename)

        {
            LOG.debug("@@@@@@ FindByFileNameEquals.FindByNameEqualsCheckinOnly - 1 - (filename=" + filename + ")");
            /* Faz a pesquisa */
            string bookmark = string.Empty;
            ADSK.SrchStatus status = null;

            ADSK.PropDef propClientFileName = VaultUtil.GetPropertyDefinition(serviceManager, "ClientFileName");
            ADSK.PropDef propCheckoutUserName = VaultUtil.GetPropertyDefinition(serviceManager, "CheckoutUserName");

            ADSK.SrchCond[] conditions = new ADSK.SrchCond[2];
            conditions[0] = new ADSK.SrchCond
            {
                SrchOper = Condition.EQUALS.Code,
                SrchTxt = filename,
                PropTyp = ADSK.PropertySearchType.SingleProperty,
                PropDefId = (int)propClientFileName.Id,
                SrchRule = ADSK.SearchRuleType.Must
            };
            conditions[1] = new ADSK.SrchCond
            {
                SrchOper = Condition.IS_EMPTY.Code,
                PropTyp = ADSK.PropertySearchType.SingleProperty,
                PropDefId = (int)propCheckoutUserName.Id,
                SrchRule = ADSK.SearchRuleType.Must
            };

            long[] folderIds = GetFoldersId.Get(documentService, baseRepositories);

            LOG.debug("@@@@@@ FindByFileNameEquals.FindByNameEqualsCheckinOnly - 2 - Vai procurar os arquivos");
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
            LOG.debug("@@@@@@ FindByFileNameEquals.FindByNameEqualsCheckinOnly - 3 - arquivos encontrados=" + fileList.Count);
            return fileList;
        }

        public static List<ADSK.File> FindByNameEquals(ADSKTools.WebServiceManager serviceManager,
            ADSK.DocumentService documentService,
            string[] baseRepositories,
            string filename)
        {
            LOG.debug("@@@@@@ FindByFileNameEquals.FindByNameEquals - 1 - (filename=" + filename + ")");
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

            LOG.debug("@@@@@@ FindByFileNameEquals.FindByNameEquals - 2 - Vai procurar os arquivos");
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
            LOG.debug("@@@@@@ FindByFileNameEquals.FindByNameEquals - 3 - arquivos encontrados=" + fileList.Count);
            return fileList;
        }
    }
}
