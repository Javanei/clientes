﻿using System;
using System.Collections.Generic;
using Ionic.Zip;
using System.Xml;

namespace DWFTools.util
{
    public class DWFUtil
    {
        public static Dictionary<string, string> Extract(String dir, String fileName, Dictionary<string, string> d, 
            string[] sheetPrefixes, string mode)
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
            d = ParseXml(dir + "\\manifest.xml", d, sheetPrefixes, mode);
            return d;
        }

        /**
         * Interpreta o manifest.xml
         */
        private static Dictionary<string, string> ParseXml(String fileName, Dictionary<string, string> d, string[] sheetPrefixes, string mode)
        {
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ ParseXml - 1 - (fileName=" + fileName + ") - Vai fazer o parser do arquivo: " + fileName);
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "0", "False=parseXml");
            int sheetNum = 0;
            XmlTextReader reader = new XmlTextReader(fileName);
            bool nonOP = false;
            bool achouOP = false;
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
                                                achouOP = true;
                                                if (!mode.ToLower().Equals("nooponly"))
                                                {
                                                    sheetName = reader.Value;
                                                    sheetPrefix = s.Substring(0, 2).ToUpper();
                                                    NeodentUtil.util.LOG.debug("@@@@@@@@@@@@@@ ParseXml - 3 - encontrou sheet: " + sheetName);
                                                } else
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        if (sheetName == null)
                                        {
                                            if (mode.ToLower().Equals("registro") || mode.ToLower().Equals("noop") || mode.ToLower().Equals("nooponly"))
                                            {
                                                sheetName = reader.Value;
                                                sheetPrefix = "ALL";
                                                NeodentUtil.util.LOG.debug("@@@@@@@@@@@@@@ ParseXml - 4 - considerando todos=" + reader.Value + ", prefix=" + sheetPrefix);
                                                nonOP = true;
                                            }
                                        } else
                                        {
                                            if (mode.ToLower().Equals("noop") || mode.ToLower().Equals("nooponly"))
                                            {
                                                NeodentUtil.util.LOG.debug("@@@@@@@@@@@@@@ ParseXml - 5 - ignorando nao op=" + reader.Value + ", prefix=" + sheetPrefix);
                                                sheetName = null;
                                                sheetPrefix = null;
                                                nonOP = true;
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
            if (mode.ToLower().Equals("nooponly"))
            {
                for(int i = 0; i <= 30; i++)
                {
                    d.Remove("" + i);
                }
            }
            if (nonOP)
            {
                NeodentUtil.util.DictionaryUtil.SetProperty(d, "-2", "NOOP");
            }
            NeodentUtil.util.DictionaryUtil.SetProperty(d, "-1", sheetNum.ToString());
            NeodentUtil.util.LOG.debug("@@@@@@@@@@ ParseXml - 6 - Fez o parser do arquivo: " + fileName);
            return d;
        }
    }
}
