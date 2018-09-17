using System.Collections.Generic;
using ADSKTools = Autodesk.Connectivity.WebServicesTools;
using ADSK = Autodesk.Connectivity.WebServices;
using VDF = Autodesk.DataManagement.Client.Framework;
using Autodesk.DataManagement.Client.Framework.Vault.Currency.Connections;

namespace VaultTools.vault
{
    class DownloadFast
    {
        private Connection m_conn;

        public DownloadFast(ADSKTools.WebServiceManager mgr, string userName, string password, string vault, long userID, string server)
        {
            this.m_conn = new Connection(mgr, userName, password, vault, userID, server, AuthenticationFlags.ServerOnly);
        }

        public void DownlodFiles(ICollection<VDF.Vault.Currency.Entities.FileIteration> fileIters)
        {
            // download individual files to a temp location
            VDF.Vault.Settings.AcquireFilesSettings settings = new VDF.Vault.Settings.AcquireFilesSettings(m_conn);
            settings.LocalPath = new VDF.Currency.FolderPathAbsolute(@"c:\temp");
            foreach (VDF.Vault.Currency.Entities.FileIteration fileIter in fileIters)
            {
                settings.AddFileToAcquire(fileIter, VDF.Vault.Settings.AcquireFilesSettings.AcquisitionOption.Download);
            }

            VDF.Vault.Results.AcquireFilesResults results = m_conn.FileManager.AcquireFiles(settings);
        }

        public void DownloadFile(ADSK.File file, string filePath)
        {
            VDF.Vault.Currency.Entities.FileIteration fileIter = new VDF.Vault.Currency.Entities.FileIteration(this.m_conn, file);
            VDF.Vault.Settings.AcquireFilesSettings settings = new VDF.Vault.Settings.AcquireFilesSettings(m_conn)
            {
                LocalPath = new VDF.Currency.FolderPathAbsolute(filePath)
            };
            settings.OptionsResolution.SyncWithRemoteSiteSetting = VDF.Vault.Settings.AcquireFilesSettings.SyncWithRemoteSite.Always;
            settings.AddFileToAcquire(fileIter, VDF.Vault.Settings.AcquireFilesSettings.AcquisitionOption.Download);

            // Baixa o arquivo
            VDF.Vault.Results.AcquireFilesResults results = m_conn.FileManager.AcquireFiles(settings);
        }

        public void DownloadAssembly(VDF.Vault.Currency.Entities.FileIteration topLevelAssembly)
        {
            // download the latest version of the assembly to working folders
            VDF.Vault.Settings.AcquireFilesSettings settings =
                new VDF.Vault.Settings.AcquireFilesSettings(m_conn);
            settings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeChildren = true;
            settings.OptionsRelationshipGathering.FileRelationshipSettings.RecurseChildren = true;
            settings.OptionsRelationshipGathering.FileRelationshipSettings.VersionGatheringOption =
                VDF.Vault.Currency.VersionGatheringOption.Latest;
            settings.AddFileToAcquire(topLevelAssembly,
               VDF.Vault.Settings.AcquireFilesSettings.AcquisitionOption.Download);

            VDF.Vault.Results.AcquireFilesResults results =
                m_conn.FileManager.AcquireFiles(settings);
        }
    }
}
