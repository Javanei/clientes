using System.Collections.Generic;
using ADSK = Autodesk.Connectivity.WebServices;

namespace VaultTools.vault.util
{
    public class GetFoldersId
    {
        public static long[] Get(ADSK.DocumentService documentService, string[] baseRepositories)
        {
            ADSK.Folder[] fld = documentService.FindFoldersByPaths(baseRepositories);
            long[] folderIds = new long[fld != null ? fld.Length : 1];
            if (fld != null)
            {
                for (int i = 0; i < fld.Length; i++)
                {
                    folderIds[i] = fld[i].Id;
                }
            }
            return folderIds;
        }
    }
}
