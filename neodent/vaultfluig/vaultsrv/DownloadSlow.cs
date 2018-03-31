using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Connectivity.WebServicesTools;
using Autodesk.Connectivity.WebServices;

namespace vaultsrv
{
    class DownloadSlow
    {
        private static int MAX_FILE_PART_SIZE = 45 * 1024 * 1024;   // 45 MB

        public static File CheckoutFile(WebServiceManager mgr, long folderId, long fileId, 
            CheckoutFileOptions option, string machine, string localPath, string comment, 
            bool downloadFile, bool allowSync, out byte[] fileContents)
        {
            ByteArray downloadTicket;
            fileContents = null;

            File checkedOutFile = mgr.DocumentService.CheckoutFile(fileId, option, machine, localPath, comment, out downloadTicket);

            if (downloadFile)
                DownloadFile(mgr, checkedOutFile.Id, allowSync, out fileContents);

            return checkedOutFile;
        }

        public static void DownloadFile(WebServiceManager mgr, long fileId, bool allowSync, out byte[] fileContents)
        {
            ByteArray[] tickets = mgr.DocumentService.GetDownloadTicketsByFileIds(new long[] { fileId });
            DownloadFile(mgr, out fileContents, tickets[0], true);
        }

        private static void DownloadFile(WebServiceManager mgr, out byte[] fileContents, ByteArray downloadTicket, bool allowSync)
        {
            mgr.FilestoreService.CompressionHeaderValue = new CompressionHeader();
            mgr.FilestoreService.CompressionHeaderValue.Supported = Compression.None;
            mgr.FilestoreService.FileTransferHeaderValue = new FileTransferHeader();
            mgr.FilestoreService.FileTransferHeaderValue = null;

            System.IO.MemoryStream stream = new System.IO.MemoryStream();

            long bytesRead = 0;
            while (mgr.FilestoreService.FileTransferHeaderValue == null || !mgr.FilestoreService.FileTransferHeaderValue.IsComplete)
            {
                //byte[] tempBytes = mgr.FilestoreService.DownloadFilePart(downloadTicket.Bytes, bytesRead, bytesRead + MAX_FILE_PART_SIZE - 1, allowSync);
                System.IO.Stream sOut = mgr.FilestoreService.DownloadFilePart(downloadTicket.Bytes, bytesRead, bytesRead + MAX_FILE_PART_SIZE - 1, allowSync);

                int chunkSize = mgr.FilestoreService.FileTransferHeaderValue.UncompressedSize;
                byte[] tempBytes = new byte[chunkSize];
                stream.Write(tempBytes, 0, chunkSize);
                bytesRead += chunkSize;
            }

            fileContents = new byte[stream.Length];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.Read(fileContents, 0, (int)stream.Length);
            stream.Close();
        }
    }
}
