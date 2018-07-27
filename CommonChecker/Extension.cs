using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonChecker
{
    /// <summary>
    /// 扩展方法类
    /// </summary>
    internal static class Extension
    {
        public static string CreateJsonSearchString(this FileNode node, string searchString)
        {
            if (node.Name == "json")
            {
                return "$" + searchString;
            }
            else
            {
                if ((node as JsonCNode).IsArray)
                {
                    return CreateJsonSearchString(node.Parent as FileNode, string.Format(@".{0}[*]{1}", node.Name, searchString));
                }
                else
                {
                    return CreateJsonSearchString(node.Parent as FileNode, string.Format(@".{0}{1}", node.Name, searchString));
                }
            }
        }

        public static string CreateXmlSearchString(this FileNode node, string searchString)
        {
            if (node.Name == "xml")
            {
                return searchString;
            }
            else
            {
                return CreateXmlSearchString(node.Parent as FileNode, string.Format(@"/{0}{1}", node.Name, searchString));
            }
        }

        public static string CreateSearchString(this ConvertNode node, string searchString)
        {
            if (node.SchemeType == FileType.JSON)
            {
                if (node.Name == "json")
                {
                    return "$" + searchString;
                }
                else
                {
                    if (node.IsArray)
                    {
                        return (node.Parent as ConvertNode).CreateSearchString(string.Format(@".{0}[*]{1}", node.Name, searchString));
                    }
                    else
                    {
                        return (node.Parent as ConvertNode).CreateSearchString(string.Format(@".{0}{1}", node.Name, searchString));
                    }
                }
            }
            else
            {
                if (node.Name == "xml")
                {
                    return searchString;
                }
                else
                {
                    return (node.Parent as ConvertNode).CreateSearchString(string.Format(@"/{0}{1}", node.Name, searchString));
                }
            }
        }

        public static string CreateNameString(this ConvertNode node)
        {
            if (node.Parent != null)
            {
                return (node.Parent as ConvertNode).CreateNameString() + "." + node.Name;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
