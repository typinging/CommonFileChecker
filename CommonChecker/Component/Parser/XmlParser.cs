using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace CommonChecker
{
    public class XmlParser : IParser
    {
        private static readonly int startCount = 1;
        private static readonly int startRank = 0;

        public CNode Root { private set; get; } = new XmlCNode() { Name = "xml", Count = 1, };

        public CNode DirectoryNode { private set; get; }

        public CNode CurrentNode { private set; get; }

        public bool Parser(string path)
        {
            bool result = true;
            string xmlString = File.ReadAllText(path);
            CurrentNode = new XmlCNode() { Name = "xml", Count = 1, };

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xmlString);
                ParseXml(xDoc, CurrentNode, startRank);
                MergeNode(Root, CurrentNode);
            }
            catch (Exception ex)
            {
                //TODO: log4net
                result = false;
            }
            return result;
        }

        public void MergeNode(CNode root, CNode current)
        {
            if (current != null)
            {
                foreach (CNode node in current.ChildrenNode)
                {
                    CNode rnode = root.ChildrenNode.Where(rc => rc.Name == node.Name).FirstOrDefault();
                    if (rnode == null)
                    {
                        rnode = node.Clone() as CNode;
                        root.ChildrenNode.Add(rnode);
                    }
                    else
                    {
                        (rnode as XmlCNode).Count += (node as XmlCNode).Count;
                        MergeNode(rnode, node);
                    }
                }
            }
        }

        private void ParseXml(XmlNode xDoc, CNode root, int rank)
        {
            XmlNodeList Nodechildren = xDoc.ChildNodes;
            rank++;
            foreach (XmlNode xn in Nodechildren)
            {
                if (xn is XmlElement)
                {
                    XmlCNode thisNode = (XmlCNode)root.ChildrenNode.Where(cn => cn.Name == xn.Name).FirstOrDefault();
                    if (thisNode == null)
                    {
                        XmlCNode newNode = new XmlCNode();
                        newNode.Name = xn.Name;
                        newNode.Rank = rank;
                        newNode.Parent = root;
                        newNode.Count = startCount;
                        root.ChildrenNode.Add(newNode);
                        thisNode = newNode;
                    }
                    else
                    {
                        thisNode.Count++;
                    }
                    foreach (XmlAttribute xa in xn.Attributes)
                    {
                        if (thisNode.Attributes.Keys.Contains(xa.Name))
                        {
                            thisNode.Attributes[xa.Name]++;
                        }
                        else
                        {
                            thisNode.Attributes.Add(xa.Name, startCount);
                        }
                    }
                    ParseXml(xn, thisNode, rank);
                }
            }
        }

    }

    //public class XmlEditer : IEditor
    //{
    //    private void ModifyXml(string path, FileNode node, string outputPath)
    //    {
    //        string xString = File.ReadAllText(path);
    //        XmlDocument xDoc = new XmlDocument();
    //        xDoc.LoadXml(xString);
    //        foreach (FileNode fn in node.ChildrenNode)
    //        {
    //            EditXml(xDoc, fn);
    //        }
    //        using (StreamWriter sw = new StreamWriter(outputPath, false, Encoding.UTF8))
    //        {
    //            xDoc.Save(sw);
    //        }
    //    }

    //    private void EditXml(XmlNode xNode, FileNode node)
    //    {
    //        XmlNodeList xList = xNode.SelectNodes(node.Name);
    //        foreach (XmlNode xn in xList.OfType<XmlElement>())
    //        {
    //            if (node.OperationType == OperationEnum.Add)
    //            {
    //                xn.InnerText = node.EditValue;
    //            }
    //            else if (node.OperationType == OperationEnum.Delete)
    //            {
    //                if (node.DeleteConditionCollection.Count == 0)
    //                {
    //                    xNode.RemoveChild(xn);
    //                    continue;
    //                }
    //                else
    //                {
    //                    foreach (DeleteCondition dc in node.DeleteConditionCollection)
    //                    {
    //                        if (SelectedChildrenNode(xn, dc))
    //                        {
    //                            xNode.RemoveChild(xn);
    //                        }
    //                    }
    //                }
    //            }
    //            foreach (XmlNode xnc in xn.ChildNodes.OfType<XmlElement>())
    //            {
    //                EditXml(xn, node.ChildrenNode.Where(cn => cn.Name == xnc.Name).First() as FileNode);
    //            }
    //        }
    //    }

    //    private bool SelectedChildrenNode(XmlNode node, DeleteCondition dc)
    //    {
    //        if (node.Name == dc.Child.Name)
    //        {
    //            if (JudgeCondition(node, dc))
    //            {
    //                return true;
    //            }
    //        }
    //        else
    //        {
    //            foreach (XmlNode xn in node.ChildNodes.OfType<XmlElement>())
    //            {
    //                if (xn.Name == dc.Child.Name)
    //                {
    //                    if (JudgeCondition(xn, dc))
    //                    {
    //                        return true;
    //                    }
    //                }
    //                else
    //                {
    //                    if (SelectedChildrenNode(xn, dc))
    //                    {
    //                        return true;
    //                    }
    //                }
    //            }
    //        }
    //        return false;
    //    }

    //    private bool JudgeCondition(XmlNode node, DeleteCondition dc)
    //    {
    //        bool result = false;
    //        if (dc.DeleteConditionType == ConditionType.Equal)
    //        {
    //            if (string.Equals(node.InnerText, dc.Value))
    //            {
    //                result = true;
    //            }
    //        }
    //        else
    //        {
    //            if (dc.DeleteConditionType == ConditionType.Less)
    //            {
    //                int v = int.Parse(dc.Value);
    //                int d = 0;
    //                if (int.TryParse(node.InnerText, out d))
    //                {
    //                    if (v > d)
    //                    {
    //                        result = true;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                int v = int.Parse(dc.Value);
    //                int d = 0;
    //                if (int.TryParse(node.InnerText, out d))
    //                {
    //                    if (v < d)
    //                    {
    //                        result = true;
    //                    }
    //                }
    //            }
    //        }
    //        return result;
    //    }

    //    public bool EditFile(string path, FileNode node, string outputPath)
    //    {
    //        bool result = false;
    //        try
    //        {
    //            ModifyXml(path, node, outputPath);
    //            result = true;
    //        }
    //        catch (Exception ex)
    //        {
    //            //TO DO: log4net
    //        }
    //        return result;
    //    }
    //}
}
