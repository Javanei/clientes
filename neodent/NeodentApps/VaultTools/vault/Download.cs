using System;
using System.Collections.Generic;

using ADSKTools = Autodesk.Connectivity.WebServicesTools;

using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault
{
    class Download
    {
        private string[] baseRepositories = new string[] { "$/Neodent/Produção" };
        private string server = "br03s059.straumann.com";
        private string vault = "neodent";
        private string user = "integracao";
        private string pass = "brasil2010";

        public Download()
        {
        }

        public Download(string server, string[] baseRepositories, string vault, string user, string pass)
        {
            this.server = server;
            this.baseRepositories = baseRepositories;
            this.vault = vault;
            this.user = user;
            this.pass = pass;
        }

        public string DonwloadFile(string outputFolder)
        {
            ADSKTools.WebServiceManager serviceManager = util.VaultUtil.Login(this.server, this.vault, this.user, this.pass);
            ADSK.DocumentService documentService = serviceManager.DocumentService;

            DownloadFast downloadFast = new DownloadFast(serviceManager, user, pass, vault, serviceManager.AdminService.SecurityHeader.UserId, server);

            ADSK.Folder[] fld = documentService.FindFoldersByPaths(baseRepositories);

            return null;
        }
    }
}
