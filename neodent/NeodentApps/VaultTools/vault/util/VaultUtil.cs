using System.Collections.Generic;

using NeodentUtil.util;

using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault.util
{
    public class VaultUtil
    {
        public static ADSKTools.WebServiceManager Login(string server, string vault, string user, string pass)
        {
            ADSK.ServerIdentities si = new ADSK.ServerIdentities
            {
                DataServer = server,
                FileServer = server
            };
            ADSKTools.IWebServiceCredentials login = new ADSKTools.UserPasswordCredentials(si, vault, user, pass);
            ADSKTools.WebServiceManager serviceManager = new ADSKTools.WebServiceManager(login);
            return serviceManager;
        }

        public static ADSK.PropDef GetPropertyDefinition(ADSKTools.WebServiceManager serviceManager, string propName)
        {
            ADSK.PropDef res = null;
            foreach (ADSK.PropDef prop in serviceManager.PropertyService.GetPropertyDefinitionsByEntityClassId("FILE"))
            {
                if (prop.SysName == propName)
                {
                    res = prop;
                }
            }
            if (res == null)
            {
                foreach (ADSK.PropDef prop in serviceManager.PropertyService.GetPropertyDefinitionsByEntityClassId("FILE"))
                {
                    if (prop.DispName == propName)
                    {
                        res = prop;
                    }
                }
            }
            return res;
        }

        public static ADSK.PropDef[] ListPropertyDefinition(ADSKTools.WebServiceManager serviceManager)
        {
            return serviceManager.PropertyService.GetPropertyDefinitionsByEntityClassId("FILE");
        }

        public static List<ADSK.File> FindFileWithDownloadExtension(ADSKTools.WebServiceManager serviceManager,
            ADSK.DocumentService documentService,
            string[] baseRepositories,
            string filename,
            List<ADSK.File> files,
            string sourceExt,
            string destExt)
        {
            if (sourceExt.Equals(destExt))
            {
                return files;
            }

            List<ADSK.File> result = new List<ADSK.File>();
            foreach (ADSK.File f in files)
            {
                ADSK.File file = FindFileWithDownloadExtension(serviceManager, documentService, baseRepositories, filename, f, sourceExt, destExt);
                if (file != null)
                {
                    result.Add(file);
                }
            }

            return result;
        }

        public static ADSK.File FindFileWithDownloadExtension(ADSKTools.WebServiceManager serviceManager,
            ADSK.DocumentService documentService,
            string[] baseRepositories,
            string filename,
            ADSK.File f,
            string sourceExt,
            string destExt)
        {
            LOG.debug("@@@@@@@@@@ VaultUtil.FindFileWithDownloadExtension - 1 - Procurando arquivo para download do arquivo: " + f.Name);
            List<ADSK.File> fTmp = FindByFileNameEquals.FindByNameEquals(serviceManager, documentService, baseRepositories, filename + destExt);
            if (fTmp.Count > 0)
            {
                LOG.debug("@@@@@@@@@@@@ VaultUtil.FindFileWithDownloadExtension - 2 - Achou: "
                    + fTmp[0].Name + ", checkin=" + f.CkInDate);
                return fTmp[0];
            }
            else
            {
                LOG.debug("@@@@@@@@@@@@ VaultUtil.FindFileWithDownloadExtension - 3 - NAO Achou");
            }
            return null;
        }
    }
}
