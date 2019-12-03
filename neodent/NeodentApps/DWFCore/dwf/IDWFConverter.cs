using System.Collections.Generic;

namespace DWFCore.dwf
{
    public interface IDWFConverter
    {
        List<string> DwfToPDF(string dwfFile, string imgTempfolder, string[] sheetPrefixes, string mode);
    }
}
