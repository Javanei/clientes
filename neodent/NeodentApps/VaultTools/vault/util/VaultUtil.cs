﻿using ADSKTools = Autodesk.Connectivity.WebServicesTools;

using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault.util
{
    class VaultUtil
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
            return res;
        }

        public static ADSK.PropDef[] ListPropertyDefinition(ADSKTools.WebServiceManager serviceManager)
        {
            return serviceManager.PropertyService.GetPropertyDefinitionsByEntityClassId("FILE");
        }
    }
}