using System.Collections.Generic;

namespace PDFCore.pdf
{
    public interface IPDFConverter
    {
        List<string> PdfToPDF(string srcPdfFile, string imgTempfolder);
    }
}
