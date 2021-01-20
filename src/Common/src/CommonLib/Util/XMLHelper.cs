using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CommonLib.Util
{
    public class XMLHelper
    {
        /// <summary>
        /// Get the node path for the specified xml node
        /// </summary>
        /// <param name="element"></param>
        public static string GetXElementNodePath(XElement element)
        {
            try
            {
                string path = element.Name.ToString();
                element = element.Parent;

                while (null != element)
                {
                    path = element.Name.ToString() + "/" + path;
                    element = element.Parent;
                }

                return path;
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// Get XML file tree node segment XElement
        /// </summary>
        /// <param name="XML">XML file carrier</param>
        /// <param name="newroot">Independent node to find</param>
        ///<returns></returns>
        public static XElement GetXElement(XElement XML, string newroot)
        {
            return XML.DescendantsAndSelf(newroot).Single();
        }

        /// <summary>
        /// Get XML file tree node segment XElement
        /// </summary>
        /// <param name="XML">XML file carrier</param>
        /// <param name="newroot">Independent node to find</param>
        /// <returns></returns>
        public static XElement GetSafeXElement(XElement XML, string newroot)
        {
            try
            {
                return XML.DescendantsAndSelf(newroot).Single();
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read: {0} failed, xml node name: {1}", newroot, GetXElementNodePath(XML)));
            }
        }

        /// <summary>
        /// Get XML file tree node segment XElement
        /// </summary>
        /// <param name="xml">XML file carrier</param>
        /// <param name="mainnode">Primary node to find</param>
        /// <param name="attribute">Primary node condition attribute name</param>
        /// <param name="value">Primary node condition attribute value</param>
        /// <returns></returns>
        public static XElement GetXElement(XElement XML, string newroot, string attribute, string value)
        {
            return XML.DescendantsAndSelf(newroot).Single(X => X.Attribute(attribute).Value == value);
        }

        /// <summary>
        /// Get XML file tree node segment XElement
        /// </summary>
        /// <param name="xml">XML file carrier</param>
        /// <param name="mainnode">Primary node to find</param>
        /// <param name="attribute">Primary node condition attribute name</param>
        /// <param name="value">Primary node condition attribute value</param>
        /// <returns></returns>
        public static XElement GetSafeXElement(XElement XML, string newroot, string attribute, string value)
        {
            try
            {
                return XML.DescendantsAndSelf(newroot).Single(X => X.Attribute(attribute).Value == value);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Read: {0}/{1}={2} failed, xml node name: {3} ", newroot, attribute, value, GetXElementNodePath(XML)) + ex.Message);
            }
        }

        /// <summary>
        /// Get the property value
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static XAttribute GetSafeAttribute(XElement XML, string attribute)
        {
            try
            {
                XAttribute attrib = XML.Attribute(attribute);
                if (null == attrib)
                {
                    throw new Exception(string.Format("Read attribute: {0} failed, xml node name: {1}", attribute, GetXElementNodePath(XML)));
                }

                return attrib;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read attribute: {0} failed, xml node name: {1}", attribute, GetXElementNodePath(XML)));
            }
        }

        /// <summary>
        /// Get property value (string)
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static string GetSafeAttributeStr(XElement XML, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, attribute);
            return (string)attrib;
        }

        /// <summary>
        /// Get Property Value (String) Returns the default value if it can not be retrieved
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static string GetDefAttributeStr(XElement XML, string attribute, string strdef)
        {
            XAttribute attrib = XML.Attribute(attribute);
            if (null == attrib)
                return strdef;

            return (string)attrib;
        }

        /// <summary>
        /// Get property value (integer)
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static long GetSafeAttributeLong(XElement XML, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, attribute);
            string str = (string)attrib;
            if (null == str || str == "")
                return -1;

            try
            {
                return (long)Convert.ToDouble(str);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read attribute: {0} failed, xml node name: {1}", attribute, GetXElementNodePath(XML)));
            }
        }

        /// <summary>
        /// Get a 32-bit integer array of attributes
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="attribute"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int[] GetSafeAttributeIntArray(XElement XML, string attribute, int length = -1)
        {
            XAttribute attrib = GetSafeAttribute(XML, attribute);
            string str = (string)attrib;
            if (null == str || str == "")
                return null;
            try
            {
                string[] args = str.Split(',');
                if (length > args.Length && length != args.Length)
                {
                    return null;
                }
                return DataHelper.StringArray2IntArray(args);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read attribute: {0} failed, xml node name: {1}", attribute, GetXElementNodePath(XML)));
            }
        }

        /// <summary>
        /// Get property value (integer array)
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static long[] GetSafeAttributeLongArray(XElement XML, string attribute, int length = -1)
        {
            XAttribute attrib = GetSafeAttribute(XML, attribute);
            string str = (string)attrib;
            if (null == str || str == "")
                return null;

            try
            {
                string[] args = str.Split(',');
                if (length > args.Length && length != args.Length)
                {
                    return null;
                }

                long[] result = new long[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    result[i] = DataHelper.SafeConvertToInt64(args[i]);
                }

                return result;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read attribute: {0} failed, xml node name: {1}", attribute, GetXElementNodePath(XML)));
            }
        }

        /// <summary>
        /// Get property value (integer array)
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static double[] GetSafeAttributeDoubleArray(XElement XML, string attribute, int length = -1)
        {
            XAttribute attrib = GetSafeAttribute(XML, attribute);
            string str = (string)attrib;
            if (null == str || str == "")
                return null;

            try
            {
                string[] args = str.Split(',');
                if (length > args.Length && length != args.Length)
                {
                    return null;
                }

                double[] result = new double[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    result[i] = DataHelper.SafeConvertToDouble(args[i]);
                }

                return result;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read attribute: {0} failed, xml node name: {1}", attribute, GetXElementNodePath(XML)));
            }
        }

        /// <summary>
        /// Get property value (floating point)
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static double GetSafeAttributeDouble(XElement XML, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, attribute);
            string str = (string)attrib;
            if (null == str || str == "")
                return 0.0;

            try
            {
                return Convert.ToDouble(str);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read attribute: {0} failed, xml node name: {1}", attribute, GetXElementNodePath(XML)));
            }
        }

        /// <summary>
        /// Get xml property value
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="root"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static XAttribute GetSafeAttribute(XElement XML, string root, string attribute)
        {
            try
            {
                XAttribute attrib = XML.Element(root).Attribute(attribute);
                if (null == attrib)
                {
                    throw new Exception(string.Format("Read property: {0}/{1} failed, xml node name: {2}", root, attribute, GetXElementNodePath(XML)));
                }

                return attrib;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read property: {0}/{1} failed, xml node name: {2}", root, attribute, GetXElementNodePath(XML)));
            }
        }

        /// <summary>
        /// Get xml attribute value (string)
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="root"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static string GetSafeAttributeStr(XElement XML, string root, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, root, attribute);
            return (string)attrib;
        }

        /// <summary>
        /// Get xml property value (integer value)
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="root"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static long GetSafeAttributeLong(XElement XML, string root, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, root, attribute);
            string str = (string)attrib;
            if (null == str || str == "")
                return -1;

            try
            {
                return (long)Convert.ToDouble(str);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read property: {0}/{1} failed, xml node name: {2}", root, attribute, GetXElementNodePath(XML)));
            }
        }

        /// <summary>
        /// Get xml attribute value (floating point)
        /// </summary>
        /// <param name="XML"></param>
        /// <param name="root"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static double GetSafeAttributeDouble(XElement XML, string root, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, root, attribute);
            string str = (string)attrib;
            if (null == str || str == "")
                return -1;

            try
            {
                return Convert.ToDouble(str);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read property: {0}/{1} failed, xml node name: {2}", root, attribute, GetXElementNodePath(XML)));
            }
        }
    }
}
