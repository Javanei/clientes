using System;
using System.Collections.Generic;
using Ionic.Zip;
using System.Xml;

namespace DWFTools.util
{
    public class DWFUtil
    {
        public static Dictionary<string, string> Extract(String dir, String fileName, Dictionary<string, string> d)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@ Extract - 1 - (dir=" + dir + ", fileName=" + fileName + ") - Vai extrair o manifext.xml");
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "0", "False=Extract");
            using (ZipFile zip1 = ZipFile.Read(fileName))
            {
                foreach (ZipEntry e in zip1)
                {
                    if (e.FileName == "manifest.xml")
                    {
                        //Console.WriteLine(e.FileName);
                        e.Extract(dir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                zip1.Dispose();
            }
            NeodentUtil.util.LOG.debug("@@@@@@@@ Extract - 2 - Extraiu o manifext.xml");
            d = ParseXml(dir + "\\manifest.xml", d);
            return d;
        }

        /**
         * Interpreta o manifest.xml
         */
        private static Dictionary<string, string> ParseXml(String fileName, Dictionary<string, string> d)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ ParseXml - 1 - (fileName=" + fileName + ") - Vai fazer o parser do arquivo: " + fileName);
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "0", "False=parseXml");
            int sheetNum = 0;
            XmlTextReader reader = new XmlTextReader(fileName);
            while (reader.Read())
            {
                if (reader.Name == "dwf:Section")
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            //Console.WriteLine(reader.Name);
                            if (reader.HasAttributes)
                            {
                                String sheetName = null;
                                String sheetPrefix = null;
                                Boolean processar = false;
                                for (int i = 0; i < reader.AttributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    if (reader.Name.Equals("type") && reader.Value.Equals("com.autodesk.dwf.ePlot"))
                                    {
                                        //Console.WriteLine("    " + reader.Name + " = " + reader.Value);
                                        processar = true;
                                        sheetNum++;
                                    }
                                    //if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("sheet") >= 0)
                                    if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("op_") >= 0)
                                    {
                                        sheetName = reader.Value;
                                        NeodentUtil.util.LOG.debug("@@@@@@@@@@@@ ParseXml - 2 - encontrou sheet: " + sheetName);
                                        //sheetPrefix = "OP";
                                        //Console.WriteLine("    " + reader.Name + " = " + reader.Value);
                                    }
                                    /*
                                    else if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("ps_") >= 0)
                                    {
                                        sheetName = reader.Value;
                                        sheetPrefix = "PS";
                                    }
                                    else if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("anvisa") >= 0)
                                    {
                                        sheetName = reader.Value;
                                        sheetPrefix = "ANVISA";
                                    }
                                    else if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("fda") >= 0)
                                    {
                                        sheetName = reader.Value;
                                        sheetPrefix = "FDA";
                                    }
                                    else if (processar && reader.Name.Equals("title") && reader.Value.ToLower().IndexOf("des") >= 0)
                                    {
                                        sheetName = reader.Value;
                                        sheetPrefix = "DES";
                                    }
                                    */
                                }
                                if (sheetName != null)
                                {
                                    //Console.WriteLine(sheetNum + "=" + (sheetName != null) + "=" + sheetName);
                                    NeodentUtil.util.DictionaryUtil.SetProperty(d, "" + sheetNum, sheetPrefix + "=" + sheetName);
                                }
                            }
                            break;
                    }
                }
            }
            reader.Close();
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ ParseXml - 3 - Fez o parser do arquivo: " + fileName);
            return d;
        }
    }
}
