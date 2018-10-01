using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWFCore.dwf
{
    public interface IDWFConverter
    {
        List<string> DwfToPDF(string dwfFile, string imgTempfolder, string[] sheetPrefixes);
    }
}
