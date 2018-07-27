using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace CommonChecker
{
    public class XmlEditer : IEditor
    {
        private void ModifyXml(string path, FileNode node, string outputPath)
        {
            string xString = File.ReadAllText(path);
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xString);
            EditXml(xDoc, node);
            using (StreamWriter sw = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                xDoc.Save(sw);
            }
        }

        private void EditXml(XmlNode xNode, FileNode node)
        {
            Dictionary<FileNode, string> npMap = new Dictionary<FileNode, string>();
            GetOperatedNode(node, npMap);
            foreach (var kv in npMap)
            {
                XmlNodeList destNodeList = xNode.SelectNodes(kv.Value);
                if (destNodeList?.Count > 0)
                {
                    switch (kv.Key.OperationType)
                    {
                        case OperationEnum.Add:
                            AddNewNode(destNodeList, kv.Key);
                            break;
                        case OperationEnum.Edit:
                            EditDestNodeValue(destNodeList, kv.Key);
                            break;
                        case OperationEnum.Delete:
                            DeleteDestNode(destNodeList, kv.Key);
                            break;
                    }
                }
            }
        }

        private void DeleteDestNode(XmlNodeList destNodeList, FileNode node)
        {
            foreach (XmlNode xNode in destNodeList)
            {
                if (node.DeleteConditionCollection == null || node.DeleteConditionCollection.Count == 0)
                {
                    xNode.ParentNode.RemoveChild(xNode);
                    continue;
                }
                else
                {
                    foreach (DeleteCondition dc in node.DeleteConditionCollection)
                    {
                        if (SelectedChildrenNode(xNode, dc))
                        {
                            xNode.ParentNode.RemoveChild(xNode);
                        }
                    }
                }
            }
        }

        private void EditDestNodeValue(XmlNodeList destNodeList, FileNode node)
        {
            foreach (XmlNode xn in destNodeList)
            {
                if (!node.IsEditNodeName)
                {
                    xn.InnerText = node.EditValue;
                }
                else
                {
                    XmlNode newNode = xn.OwnerDocument.CreateElement(node.EditName);
                    newNode.InnerText = xn.InnerText;
                    newNode.InnerXml = xn.InnerXml;
                    xn.ParentNode.ReplaceChild(newNode, xn);
                }
            }
        }

        private void AddNewNode(XmlNodeList destNodeList, FileNode node)
        {
            foreach (XmlNode xn in destNodeList)
            {
                XmlNode newNode = xn.OwnerDocument.CreateElement(node.AddName);
                newNode.InnerText = node.AddContent;
                xn.AppendChild(newNode);
            }
        }

        private void GetOperatedNode(FileNode node, Dictionary<FileNode, string> map)
        {
            if (node.OperationType != OperationEnum.None)
            {
                string xPath = node.CreateXmlSearchString(string.Empty);
                map.Add(node, xPath);
            }
            foreach (FileNode sNode in node.ChildrenNode)
            {
                GetOperatedNode(sNode, map);
            }
        }

        private bool SelectedChildrenNode(XmlNode node, DeleteCondition dc)
        {
            if (node.Name == dc.Child.Name)
            {
                if (JudgeCondition(node, dc))
                {
                    return true;
                }
            }
            else
            {
                foreach (XmlNode xn in node.ChildNodes.OfType<XmlElement>())
                {
                    if (xn.Name == dc.Child.Name)
                    {
                        if (JudgeCondition(xn, dc))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (SelectedChildrenNode(xn, dc))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool JudgeCondition(XmlNode node, DeleteCondition dc)
        {
            bool result = false;
            if (dc.DeleteConditionType == ConditionType.Equal)
            {
                if (string.Equals(node.InnerText, dc.Value))
                {
                    result = true;
                }
            }
            else
            {
                if (dc.DeleteConditionType == ConditionType.Less)
                {
                    int v = int.Parse(dc.Value);
                    int d = 0;
                    if (int.TryParse(node.InnerText, out d))
                    {
                        if (v > d)
                        {
                            result = true;
                        }
                    }
                }
                else
                {
                    int v = int.Parse(dc.Value);
                    int d = 0;
                    if (int.TryParse(node.InnerText, out d))
                    {
                        if (v < d)
                        {
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        public bool EditFile(string path, FileNode node, string outputPath)
        {
            bool result = false;
            //try
            //{
                ModifyXml(path, node, outputPath);
                result = true;
            //}
            //catch (Exception ex)
            //{
            //    //TO DO: log4net
            //    throw new Exception("导出文件错误");
            //}
            return result;
        }
    }
}
