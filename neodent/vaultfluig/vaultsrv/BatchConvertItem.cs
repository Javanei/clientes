using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ADSK = Autodesk.Connectivity.WebServices;

namespace vaultsrv
{
    class BatchConvertItem
    {
        public string ItemFile = null;
        public ADSK.File file = null;
        public ADSK.File fileDown = null;
        public String fileLog = null;
        public Dictionary<string, string> props = null;
        public string imageDir = null;
        public string ItemDir = null;
        public string DeCodigo = null;

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<BatchConvertItem");
            sb.Append(" ItemFile=\"").Append(ItemFile).Append("\"");
            sb.Append(" file=\"").Append(file.Name).Append("\"");
            sb.Append(" fileDown=\"").Append(fileDown.Name).Append("\"");
            sb.Append(" fileLog=\"").Append(fileLog).Append("\"");
            sb.Append(" imageDir=\"").Append(imageDir).Append("\"");
            sb.Append(" ItemDir=\"").Append(ItemDir).Append("\"");
            sb.Append(" DeCodigo=\"").Append(DeCodigo).Append("\"");
            sb.Append(" />");
            return sb.ToString();
        }
    }
}
