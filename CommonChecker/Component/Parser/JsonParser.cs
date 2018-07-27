using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace CommonChecker
{
    public class JsonParser : IParser
    {
        private static readonly int startCount = 1;
        private static readonly int startRank = 0;

        public CNode Root { private set; get; } = new JsonCNode() { Name = "json", Count = 1,};

        public CNode CurrentNode { private set; get; }

        public bool Parser(string path)
        {
            bool result = true;
            string jsonString = File.ReadAllText(path);
            CurrentNode = new JsonCNode() { Name = "json", Count = 1, };
            try
            {
                ParseJson((JsonCNode)this.CurrentNode, JObject.Parse(jsonString), startRank);
                MergeNode(Root, CurrentNode);
            }
            catch(Exception ex)
            {
                //TODO: log4net
                result = false;
            }
            return result;
        }

        private void MergeNode(CNode root, CNode current)
        {
            if (current != null)
            {
                foreach (CNode node in current.ChildrenNode)
                {
                    CNode rnode = root.ChildrenNode.Where(rc => rc.Name == node.Name).FirstOrDefault();
                    if (rnode == null)
                    {
                        root.ChildrenNode.Add(node);
                        rnode = root;
                    }
                    else
                    {
                        (rnode as JsonCNode).Count += (node as JsonCNode).Count;
                        MergeNode(rnode, node);
                    }
                }
            }
        }

        private void ParseJson(JsonCNode root, JObject jObject, int rank, bool isArray = false)
        {
            JToken[] jTArray = jObject.Children().ToArray();
            rank++;
            foreach (JToken jt in jTArray)
            {
                JProperty jp = jt.ToObject<JProperty>();
                if (jp.Value.Type == JTokenType.Array)
                {
                    isArray = true;
                }
                else
                {
                    isArray = false;
                }
                JsonCNode tNode = (JsonCNode)root.ChildrenNode.Where(s => s.Name == jp.Name).FirstOrDefault();
                if (tNode == null)
                {
                    JsonCNode jn = new JsonCNode();
                    jn.IsArray = isArray;
                    jn.Name = jp.Name;
                    jn.Rank = rank;
                    jn.Parent = root;
                    jn.Count = startCount;
                    root.ChildrenNode.Add(jn);
                    tNode = jn;
                }
                else
                {
                    tNode.Count++;
                }

                if (jp.Value is JObject)
                {
                    ParseJson(tNode, jp.Value as JObject, rank);
                }
                else if (jp.Value is JArray)
                {
                    JArray ja = jp.Value as JArray;
                    ParseArray(tNode, ja, rank);
                }

            }
        }

        private void ParseArray(JsonCNode root, JArray array, int rank)
        {
            if (array.Count > 0)
            {
                foreach (JToken fa in array)
                {
                    if (fa is JObject)
                    {
                        ParseJson(root, fa as JObject, rank);
                    }
                    else if (fa is JArray)
                    {
                        ParseArray(root, fa as JArray, rank);
                    }
                }
            }
            else
            {
                Debug.Assert(true);
            }
        }

    }

    //public class JsonEditor : IEditor
    //{
    //    public bool EditFile(string path, FileNode node, string outputPath)
    //    {
    //        bool result = false;
    //        try
    //        {
    //            JObject jroot = ModifyJson(path, node);
    //            if (jroot == null)
    //            {
    //                throw new ArgumentNullException("修改中出现无法预料的错误！请检查文件：" + path + "。与修改条件。");
    //            }
    //            using (StreamWriter sw = new StreamWriter(outputPath, false, Encoding.UTF8))
    //            {
    //                sw.Write(jroot.ToString());
    //            }
    //            result = true;
    //        }
    //        catch (Exception ex)
    //        {
    //            //TO DO: log4net
    //        }
    //        return result;
    //    }

    //    private JObject ModifyJson(string path, FileNode node)
    //    {
    //        string jsonString = File.ReadAllText(path);
    //        JObject jroot = JObject.Parse(jsonString);
    //        EditXml(jroot, node);
    //        return jroot;
    //    }

    //    private void EditXml(JToken jNode, FileNode node)
    //    {
    //        CheckAndEditNode(jNode, node);
    //        JToken[] jSubNodes = jNode.Children().ToArray();
    //        foreach (JToken jSub in jSubNodes)
    //        {
    //            switch (jSub.Type)
    //            {
    //                case JTokenType.Object:
    //                    JToken[] jssubNodes = (jSub as JObject).Children().ToArray();
    //                    foreach (JToken jssub in jssubNodes)
    //                    {
    //                        EditXml(jssub, node.ChildrenNode.Where(cn => cn.Name == (jssub as JProperty).Name).First() as FileNode);
    //                    }
    //                    break;
    //                case JTokenType.Array:
    //                    JToken[] jsaubNodes = (jSub as JArray).Children().ToArray();
    //                    foreach (JToken jssub in jsaubNodes)
    //                    {
    //                        EditXml(jssub, node);
    //                    }
    //                    break;
    //                case JTokenType.Property:
    //                    EditXml(jSub, node.ChildrenNode.Where(cn => cn.Name == (jSub as JProperty).Name).First() as FileNode);
    //                    break;
    //            }
    //        }
    //    }

    //    private void CheckAndEditNode(JToken jNode, FileNode node)
    //    {
    //        if (jNode.Type == JTokenType.Property)
    //        {
    //            JProperty jp = jNode as JProperty;
    //            if (node.OperationType == OperationEnum.Add)
    //            {
    //                if (jp.Name == node.Name)
    //                {
    //                    jp.Value = node.EditValue;
    //                }
    //            }
    //            else if (node.OperationType == OperationEnum.Delete)
    //            {
    //                if (jp.Name == node.Name)
    //                {
    //                    if (node.DeleteConditionCollection.Count == 0)
    //                    {
    //                        jNode.Remove();
    //                    }
    //                    else
    //                    {
    //                        foreach (DeleteCondition dc in node.DeleteConditionCollection)
    //                        {
    //                            if ((node as JsonCNode).IsArray == true)
    //                            {
    //                                JArray ja = jp.Value as JArray;
    //                                for (int i = 0; i < ja.Count; i++)
    //                                {
    //                                    if (CheckDeleteCondition(ja[i], dc))
    //                                    {
    //                                        ja.RemoveAt(i);
    //                                        i--;
    //                                    }
    //                                }
    //                            }
    //                            else
    //                            {
    //                                if (CheckDeleteCondition(jNode, dc))
    //                                {
    //                                    jNode.Remove();
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    private bool CheckDeleteCondition(JToken jNode, DeleteCondition dc)
    //    {
    //        if (jNode.Type == JTokenType.Property)
    //        {
    //            JProperty jp = jNode as JProperty;
    //            if (jp.Name == dc.Child.Name)
    //            {
    //                if (dc.DeleteConditionType == ConditionType.Equal)
    //                {
    //                    if (jp.ToString().IndexOf(dc.Value) > -1)
    //                    {
    //                        return true;
    //                    }
    //                }
    //                else
    //                {
    //                    if (dc.DeleteConditionType == ConditionType.Less)
    //                    {
    //                        double v = 0d;
    //                        if (double.TryParse(jp.Value.ToString(), out v))
    //                        {
    //                            if (v < double.Parse(dc.Value))
    //                            {
    //                                return true;
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        double v = 0d;
    //                        if (double.TryParse(jp.Value.ToString(), out v))
    //                        {
    //                            if (v > double.Parse(dc.Value))
    //                            {
    //                                return true;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        JToken[] jSubNodes = jNode.Children().ToArray();
    //        foreach (JToken jSub in jSubNodes)
    //        {
    //            switch (jSub.Type)
    //            {
    //                case JTokenType.Object:
    //                    JToken[] jssubNodes = (jSub as JObject).Children().ToArray();
    //                    foreach (JToken jssub in jssubNodes)
    //                    {
    //                        if (CheckDeleteCondition(jssub, dc))
    //                        {
    //                            return true;
    //                        }
    //                    }
    //                    break;
    //                case JTokenType.Array:
    //                    JToken[] jsaubNodes = (jSub as JArray).Children().ToArray();
    //                    foreach (JToken jssub in jsaubNodes)
    //                    {
    //                        if (ParseJArrayItem(jssub, dc))
    //                        {
    //                            return true;
    //                        }
    //                    }
    //                    break;
    //                case JTokenType.Property:
    //                    if (CheckDeleteCondition(jSub, dc))
    //                    {
    //                        return true;
    //                    }
    //                    break;
    //            }
    //        }
    //        return false;
    //    }

    //    private bool ParseJArrayItem(JToken jNode, DeleteCondition dc)
    //    {
    //        switch (jNode.Type)
    //        {
    //            case JTokenType.Object:
    //                JToken[] jssubNodes = (jNode as JObject).Children().ToArray();
    //                foreach (JToken jssub in jssubNodes)
    //                {
    //                    if (CheckDeleteCondition(jssub, dc))
    //                    {
    //                        return true;
    //                    }
    //                }
    //                break;
    //            case JTokenType.Array:
    //                JToken[] jsaubNodes = (jNode as JArray).Children().ToArray();
    //                foreach (JToken jssub in jsaubNodes)
    //                {
    //                    if (ParseJArrayItem(jssub, dc))
    //                    {
    //                        return false;
    //                    }
    //                }
    //                break;
    //            case JTokenType.Property:
    //                if (CheckDeleteCondition(jNode, dc))
    //                {
    //                    return true;
    //                }
    //                break;
    //        }
    //        return false;
    //    }

    //}

}