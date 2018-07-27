using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CommonChecker
{
    internal class SearchBase
    {
        internal readonly string errorString = "错误的节点！";
        internal readonly string errorString2 = "超出最大内容统计数量！";
    }

    internal class Searcher : SearchBase
    {
        internal Dictionary<string, int> ContentCountMap { get; private set; } = new Dictionary<string, int>();

        private int MaxMapCount = 50;

        /// <summary>
        /// 搜索指定节点的内容并归类
        /// </summary>
        /// <exception cref="FormatException">错误的节点（节点内容不是字符串）</exception>
        /// <exception cref="ArgumentException">超出最大内容统计数量</exception>
        /// <returns></returns>
        public void SearchSpecifyNode(FileNode specifyNode, List<string> filePathList)
        {
            ContentCountMap.Clear();
            ISearch searcher = GetFileSearcher(specifyNode);
            string searchString = searcher.CreateSearchString(specifyNode, string.Empty);
            foreach (string filePath in filePathList)
            {
                Dictionary<string, int> tempMap = searcher.Search(filePath, searchString);
                foreach (var kv in tempMap)
                {
                    if (ContentCountMap.Keys.Contains(kv.Key))
                    {
                        ContentCountMap[kv.Key] += kv.Value;
                    }
                    else
                    {
                        ContentCountMap.Add(kv.Key, kv.Value);
                    }
                }
                if (ContentCountMap.Count > MaxMapCount)
                {
                    throw new ArgumentException(errorString2);
                }
            }
        }

        private ISearch GetFileSearcher(FileNode specifyNode)
        {
            if (specifyNode is XmlCNode)
            {
                return new XmlSearcher();
            }
            else
            {
                return new JsonSearcher();
            }
        }
    }

    internal class JsonSearcher : SearchBase, ISearch
    {
        public string CreateSearchString(FileNode node, string searchString)
        {
            return node.CreateJsonSearchString(searchString);
        }

        //public string CreateSearchString(FileNode node, string searchString)
        //{
        //    if (node.Name == "json")
        //    {
        //        return "$" + searchString;
        //    }
        //    else
        //    {
        //        if ((node as JsonCNode).IsArray)
        //        {
        //            return CreateSearchString(node.Parent as FileNode, string.Format(@".{0}[*]{1}", node.Name, searchString));
        //        }
        //        else
        //        {
        //            return CreateSearchString(node.Parent as FileNode, string.Format(@".{0}{1}", node.Name, searchString));
        //        }
        //    }
        //}

        public Dictionary<string, int> Search(string filePath, string searchPath)
        {
            Dictionary<string, int> countMap = new Dictionary<string, int>();
            try
            {
                string jsonString = File.ReadAllText(filePath);
                JObject jroot = JObject.Parse(jsonString);
                JToken[] results = jroot.SelectTokens(searchPath).ToArray();
                if (results?.Count() > 0)
                {
                    if (results.First().Type != JTokenType.Array && results.First().Type != JTokenType.Object && results.First().Type != JTokenType.Property)
                    {
                        foreach (string i in results)
                        {
                            if (countMap.Keys.Contains(i))
                            {
                                countMap[i]++;
                            }
                            else
                            {
                                countMap.Add(i, 1);
                            }
                        }
                    }
                    else
                    {
                        throw new FormatException(errorString);
                    }
                }
            }
            catch (FormatException ex)
            {
                throw new FormatException(ex.Message);
            }
            catch (Exception ex)
            {
                //TO DO : log4net
            }
            return countMap;
        }
    }

    internal class XmlSearcher : SearchBase, ISearch
    {
        public string CreateSearchString(FileNode node, string searchString)
        {
            return node.CreateXmlSearchString(searchString);
        }

        //public string CreateSearchString(FileNode node, string searchString)
        //{
        //    if (node.Name == "xml")
        //    {
        //        return searchString;
        //    }
        //    else
        //    {
        //        return CreateSearchString(node.Parent as FileNode, string.Format(@"/{0}{1}", node.Name, searchString));
        //    }
        //}

        public Dictionary<string, int> Search(string filePath, string searchPath)
        {
            Dictionary<string, int> countMap = new Dictionary<string, int>();
            try
            {
                string xmlString = File.ReadAllText(filePath);
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xmlString);
                XmlNodeList results = xDoc.SelectNodes(searchPath);
                if (results?.Count > 0)
                {
                    foreach (XmlNode xn in results)
                    {
                        if (xn.ChildNodes.Count > 0)
                        {
                            if (xn.ChildNodes.Item(0).NodeType != XmlNodeType.Element)
                            {
                                if (countMap.Keys.Contains(xn.InnerText))
                                {
                                    countMap[xn.InnerText]++;
                                }
                                else
                                {
                                    countMap.Add(xn.InnerText, 1);
                                }
                            }
                            else
                            {
                                throw new FormatException(errorString);
                            }
                        }
                    }
                }
            }
            catch (FormatException ex)
            {
                throw new FormatException(ex.Message);
            }
            catch (Exception ex)
            {
                //TO DO : log4net
            }
            return countMap;
        }
    }

}
