using System;
using System.Collections.Generic;
using Ionic.Zip;
using System.Xml;

namespace DWFTools.util
{
    public class DWFUtil
    {
        public static Dictionary<string, string> Extract(String dir, String fileName, Dictionary<string, string> d, string[] sheetPrefixes)
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
            d = ParseXml(dir + "\\manifest.xml", d, sheetPrefixes);
            return d;
        }

        /**
         * Interpreta o manifest.xml
         */
        private static Dictionary<string, string> ParseXml(String fileName, Dictionary<string, string> d, string[] sheetPrefixes)
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
                                string sheetName = null;
                                string sheetPrefix = null;
                                bool processar = false;
                                for (int i = 0; i < reader.AttributeCount; i++)
                                {
                                    reader.MoveToAttribute(i);
                                    if (reader.Name.Equals("type") && reader.Value.Equals("com.autodesk.dwf.ePlot"))
                                    {
                                        //Console.WriteLine("    " + reader.Name + " = " + reader.Value);
                                        processar = true;
                                        sheetNum++;
                                    }
                                    if (processar && reader.Name.Equals("title"))
                                    {
                                        foreach (string s in sheetPrefixes)
                                        {
                                            NeodentUtil.util.LOG.debug("@@@@@@@@@@@@@@ ParseXml - 2 - validando sheet=" + reader.Value + ", prefix=" + s);
                                            if (reader.Value.ToLower().IndexOf(s) >= 0)
                                            {
                                                sheetName = reader.Value;
                                                sheetPrefix = s.Substring(0, 2).ToUpper();
                                                NeodentUtil.util.LOG.debug("@@@@@@@@@@@@@@ ParseXml - 3 - encontrou sheet: " + sheetName);
                                            }
                                        }
                                    }
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
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "-1=", sheetNum.ToString());
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ ParseXml - 4 - Fez o parser do arquivo: " + fileName);
            return d;
        }
    }
}
