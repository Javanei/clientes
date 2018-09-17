using System.Collections.Generic;
using System;

using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault
{
    public class Manager
    {
        private string[] baseRepositories = new string[] { "$/Neodent/Produção" };
        private string server = "br03s059.straumann.com";
        private string vault = "neodent";
        private string user = "integracao";
        private string pass = "brasil2010";

        private ADSKTools.WebServiceManager serviceManager = null;

        public Manager()
        {
            serviceManager = util.VaultUtil.Login(this.server, this.vault, this.user, this.pass);
        }

        public Manager(string server, string[] baseRepositories, string vault, string user, string pass)
        {
            this.server = server;
            this.baseRepositories = baseRepositories;
            this.vault = vault;
            this.user = user;
            this.pass = pass;
            serviceManager = util.VaultUtil.Login(this.server, this.vault, this.user, this.pass);
        }

        public List<ADSK.File> FindByFileName(string filename, string[] validExt)
        {
            NeodentUtil.util.LOG.debug("@@@@ FindByFileName - 1 - (filename=" + filename + ")");
            ADSK.DocumentService documentService = serviceManager.DocumentService;

            List<ADSK.File> fl = util.FindByFileNameEquals.Find(documentService, baseRepositories, filename);
            NeodentUtil.util.LOG.debug("@@@@ FindByFileName - 2 - encontrados=" + fl.Count);
            if (fl.Count == 0)
            {
                if (validExt != null && validExt.Length > 0)
                {
                    foreach (string ext in validExt)
                    {
                        fl = util.FindByFileNameEquals.Find(documentService, baseRepositories, filename + ext);
                        NeodentUtil.util.LOG.debug("@@@@@@ FindByFileName - 3 - encontrados com extensao '" + ext + "'=" + fl.Count);
                        if (fl.Count > 0)
                        {
                            break;
                        }
                    }
                }
            }
            NeodentUtil.util.LOG.debug("@@@@ FindByFileName - 4 - resultado=" + fl.Count);
            return fl;
        }

        public void Close()
        {
            if (serviceManager != null)
            {
                serviceManager.Dispose();
                serviceManager = null;
            }
        }

        //ADSK.PropDef prop = GetPropertyDefinition("CheckoutUserName", documentService);
        //prop = GetPropertyDefinition("ClientFileName", documentService);
        //prop = GetPropertyDefinition("CheckInDate", documentService);


        public void TmpListPropertyDefinition()
        {
            ADSK.PropDef[] defs = util.VaultUtil.ListPropertyDefinition(serviceManager);
            Console.WriteLine("******* Props *************");
            foreach (ADSK.PropDef prop in defs)
            {
                Console.WriteLine(prop.Id + ", SysName=" + prop.SysName + ", DispName=" + prop.DispName + ", DataType=" + prop.Typ);
            }
            Console.WriteLine("***************************");
        }

        public void TmpFindFindByName(string filename, string[] validExt)
        {
            Console.WriteLine("******* List File *************");

            List<ADSK.File> files = FindByFileName(filename, validExt);
            foreach (ADSK.File f in files)
            {
                Console.WriteLine("****** file -> Id=" + f.Id + ", Name=" + f.Name + ", CheckedOut=" + f.CheckedOut
                    + ", CkInDate=" + f.CkInDate + ", CkOutUserId=" + f.CkOutUserId + ", VerName=" + f.VerName + ", VerNum=" + f.VerNum);
            }
            Console.WriteLine("*******************************");
        }
    }
}
