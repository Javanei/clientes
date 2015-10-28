using System;
using System.Collections.Generic;
using System.Xml;
using System.Web.Services.Protocols;

namespace vaultsrv
{
    /// <summary>
    /// Parses the data from a server error.
    /// </summary>
    public class ErrorHandler
    {

        /// <summary>
        ///  Returns the ADMS error in string format.  Null is returned if it's not an ADMS error.
        /// </summary>
        public static string GetErrorCodeString(Exception e)
        {
            SoapException se = e as SoapException;

            if (se != null)
            {
                try
                {
                    return se.Detail["sl:sldetail"]["sl:errorcode"].InnerText;
                }
                catch
                { }
            }

            return null;
        }

        public static void HandleError(Exception e)
        {
            SoapException soapEx = e as SoapException;

            if (soapEx == null)
            {
                // error was not thrown by the server
                Console.WriteLine(e.ToString());
                return;
            }

            // parse the XML data returned from the server.
            XmlElement elementDetail = soapEx.Detail["sl:sldetail"];

            long errorCode = -1;
            List<int> restrictions = new List<int>();


            if (elementDetail == null || elementDetail.ChildNodes == null)
            {
                // catch cases where the error code is 0
                Console.WriteLine(e.ToString());
                return;
            }

            foreach (XmlNode node in elementDetail.ChildNodes)
            {
                if (node.Name == "sl:errorcode")
                    errorCode = long.Parse(node.InnerText);
                else if (node.Name == "sl:restrictions")
                {
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        if (node2.Name != "sl:restriction")
                            continue;

                        restrictions.Add(int.Parse(node2.Attributes["sl:code"].Value));
                    }
                }
            }

            if (errorCode == 1387 && restrictions.Count > 0)
            {
                // print restriction data
                string msgString = "";

                foreach (int restricCode in restrictions)
                {
                    msgString += "Restriction Code = " + restricCode.ToString() + Environment.NewLine;
                }

                Console.WriteLine("Server Error: " + msgString);
            }
            else
                Console.WriteLine("Server Error : Error Code = " + errorCode);

        }
    }
}
