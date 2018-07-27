using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonChecker
{
    public class JsonEditor : IEditor
    {
        public bool EditFile(string path, FileNode node, string outputPath)
        {
            bool result = false;
            //try
            //{
                JObject jroot = ModifyJson(path, node);
                if (jroot == null)
                {
                    throw new ArgumentNullException("修改中出现无法预料的错误！请检查文件：" + path + "。与修改条件。");
                }
                using (StreamWriter sw = new StreamWriter(outputPath, false, Encoding.UTF8))
                {
                    sw.Write(jroot.ToString());
                }
                result = true;
            //}
            //catch
            //{
            //    throw new Exception("导出json文件错误");
            //}
            return result;
        }

        private JObject ModifyJson(string path, FileNode node)
        {
            string jsonString = File.ReadAllText(path);
            JObject jroot = JObject.Parse(jsonString);
            EditJson(jroot, node);
            //to do：检查空数组
            return jroot;
        }

        private void EditJson(JToken jNode, FileNode node)
        {
            Dictionary<FileNode, string> npMap = new Dictionary<FileNode, string>();
            GetOperatedNode(node, npMap);
            foreach (var kv in npMap)
            {
                List<JToken> destNodeList = jNode.SelectTokens(kv.Value).ToList();
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

        private void DeleteDestNode(List<JToken> destNodeList, FileNode key)
        {
            foreach (JToken jtNode in destNodeList)
            {
                if (key.DeleteConditionCollection == null || key.DeleteConditionCollection.Count == 0)
                {
                    FindPropertyAndDelete(jtNode);
                }
                else
                {
                    DeleteNodeWithCondition(jtNode, key);
                }
            }
        }

        private void FindPropertyAndDelete(JToken jNode)
        {
            if (jNode.Type == JTokenType.Property)
            {
                if(!string.IsNullOrEmpty(jNode.Path) && jNode.Parent != null)
                {
                    jNode.Remove();
                }
            }
            else
            {
                FindPropertyAndDelete(jNode.Parent);
            }
        }


        private void DeleteNodeWithCondition(JToken jtNode, FileNode key)
        {
            foreach (DeleteCondition dc in key.DeleteConditionCollection)
            {
                if (CheckDeleteCondition(jtNode, dc))
                {
                    JToken parent = jtNode.Parent;
                    jtNode.Remove();
                    if (parent.Type == JTokenType.Array)
                    {
                        if ((parent as JArray).Count == 0)
                        {
                            FindPropertyAndDelete(parent);
                        }
                    }
                }
            }
        }

        private void EditDestNodeValue(List<JToken> destNodeList, FileNode key)
        {
            foreach (JToken jtNode in destNodeList)
            {
                if (key.IsEditNodeName)
                {
                    FindPropertyAndEditName(jtNode, key);
                }
                else
                {
                    FindPropertyAndEditValue(jtNode, key);
                }
            }
        }

        private void FindPropertyAndEditName(JToken jtNode, FileNode key)
        {
            if (jtNode.Parent.Type == JTokenType.Property)
            {
                JProperty oldP = jtNode.Parent as JProperty;
                if (!string.IsNullOrEmpty(oldP.Path) && oldP.Parent != null)
                {
                    JProperty newP = new JProperty(key.EditName, oldP.Value);
                    oldP.Replace(newP);
                }
            }
            else
            {
                FindPropertyAndEditName(jtNode.Parent, key);
            }
        }

        private void FindPropertyAndEditValue(JToken jtNode, FileNode key)
        {
            if (jtNode.Parent.Type == JTokenType.Property)
            {
                (jtNode.Parent as JProperty).Value = key.EditValue;
            }
            else
            {
                FindPropertyAndEditValue(jtNode.Parent, key);
            }
        }

        private void AddNewNode(List<JToken> destNodeList, FileNode key)
        {
            foreach (JToken jtNode in destNodeList)
            {
                AddNewNodeToDestNode(jtNode, key);
            }
        }

        private void AddNewNodeToDestNode(JToken jtNode, FileNode key)
        {
            if (jtNode.Type == JTokenType.Object)
            {
                JProperty jp = new JProperty(key.AddName, key.AddContent);
                (jtNode as JObject).Add(jp);
            }
            else if (jtNode.Type == JTokenType.Property)
            {
                AddNewNodeToDestNode((jtNode as JProperty).Value, key);
            }
            else if (jtNode.Type == JTokenType.Array)
            {
                foreach (JToken sjtNode in (jtNode as JArray))
                {
                    AddNewNodeToDestNode(sjtNode, key);
                }
            }
        }

        private void GetOperatedNode(FileNode node, Dictionary<FileNode, string> map)
        {
            if (node.OperationType != OperationEnum.None)
            {
                string jPath = node.CreateJsonSearchString(string.Empty);
                map.Add(node, jPath);
            }
            foreach (FileNode sNode in node.ChildrenNode)
            {
                GetOperatedNode(sNode, map);
            }
        }

        private bool CheckDeleteCondition(JToken jNode, DeleteCondition dc)
        {
            if (jNode.Type == JTokenType.Property)
            {
                JProperty jp = jNode as JProperty;
                if (jp.Name == dc.Child.Name)
                {
                    if (dc.DeleteConditionType == ConditionType.Equal)
                    {
                        if (jp.ToString().IndexOf(dc.Value) > -1)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (dc.DeleteConditionType == ConditionType.Less)
                        {
                            double v = 0d;
                            if (double.TryParse(jp.Value.ToString(), out v))
                            {
                                if (v < double.Parse(dc.Value))
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            double v = 0d;
                            if (double.TryParse(jp.Value.ToString(), out v))
                            {
                                if (v > double.Parse(dc.Value))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            JToken[] jSubNodes = jNode.Children().ToArray();
            foreach (JToken jSub in jSubNodes)
            {
                switch (jSub.Type)
                {
                    case JTokenType.Object:
                        JToken[] jssubNodes = (jSub as JObject).Children().ToArray();
                        foreach (JToken jssub in jssubNodes)
                        {
                            if (CheckDeleteCondition(jssub, dc))
                            {
                                return true;
                            }
                        }
                        break;
                    case JTokenType.Array:
                        JToken[] jsaubNodes = (jSub as JArray).Children().ToArray();
                        foreach (JToken jssub in jsaubNodes)
                        {
                            if (ParseJArrayItem(jssub, dc))
                            {
                                return true;
                            }
                        }
                        break;
                    case JTokenType.Property:
                        if (CheckDeleteCondition(jSub, dc))
                        {
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        private bool ParseJArrayItem(JToken jNode, DeleteCondition dc)
        {
            switch (jNode.Type)
            {
                case JTokenType.Object:
                    JToken[] jssubNodes = (jNode as JObject).Children().ToArray();
                    foreach (JToken jssub in jssubNodes)
                    {
                        if (CheckDeleteCondition(jssub, dc))
                        {
                            return true;
                        }
                    }
                    break;
                case JTokenType.Array:
                    JToken[] jsaubNodes = (jNode as JArray).Children().ToArray();
                    foreach (JToken jssub in jsaubNodes)
                    {
                        if (ParseJArrayItem(jssub, dc))
                        {
                            return false;
                        }
                    }
                    break;
                case JTokenType.Property:
                    if (CheckDeleteCondition(jNode, dc))
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

    }
}
