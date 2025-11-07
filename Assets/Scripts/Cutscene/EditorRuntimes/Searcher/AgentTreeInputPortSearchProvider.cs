#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections.Generic;
using Framework.AT.Runtime;

namespace Framework.Cutscene.Editor
{
    /// <summary>
    /// 搜索AgentTreeData所有节点的输入端口，一级为节点，二级为输入端口
    /// </summary>
    public class AgentTreeInputPortSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        struct NodeData
        {
            public BaseNode pNode;
            public AT.Editor.AgentTreeAttri attri;
        }
        public System.Action<BaseNode, ArgvAttribute, int> onSelect; // 回调：节点和端口名

        List<System.Type> m_filterNodeTypes = new List<System.Type>();
        public bool onlyShowNode = true; // 仅显示节点，不显示输入端口
        private List<NodeData> m_nodes;
        private Dictionary<BaseNode, List<ArgvAttribute>> m_nodeInputPorts;

        //-----------------------------------------------------
        public void SetFilterType(params System.Type[] arrTypes)
        {
            if (arrTypes != null) m_filterNodeTypes.AddRange(arrTypes);
            else m_filterNodeTypes.Clear();
        }
        //-----------------------------------------------------
        public void SetAgentTreeData(AgentTreeData agentTreeData)
        {
            m_nodes = new List<NodeData>();
            m_nodeInputPorts = new Dictionary<BaseNode, List<ArgvAttribute>>();
            if (agentTreeData == null) return;

            foreach (var node in agentTreeData.GetNodes())
            {
                if (m_filterNodeTypes.Count>0)
                {
                    bool bInclude = false;
                    foreach(var db in m_filterNodeTypes)
                    {
                        if (node.Value.GetType() == db || node.Value.GetType().IsSubclassOf(db))
                        {
                            bInclude = true;
                            break;
                        }
                    }
                    if(!bInclude)
                    {
                        continue;
                    }
                }
                int cusomtType = 0;
                if (node.Value is CutsceneEvent)
                {
                    cusomtType = ((CutsceneEvent)node.Value).eventType;
                }
                var attr = AT.Editor.AgentTreeUtil.GetAttri(node.Value.type, cusomtType);
                if (attr == null)
                    continue;
                NodeData nodeData = new NodeData();
                nodeData.pNode = node.Value;
                nodeData.attri = attr;
                m_nodes.Add(nodeData);

                if (attr.argvs == null)
                    continue;
                m_nodeInputPorts[node.Value] = attr.argvs;
            }
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("选择节点输入端口")));

            if (m_nodes != null)
            {
                foreach (var node in m_nodes)
                {
                    if(onlyShowNode)
                    {
                        var menus = node.attri.strMenuName.Split('/');
                        for(int i =0; i < menus.Length; ++i)
                        {
                            if(i == menus.Length-1)
                            {
                                tree.Add(new SearchTreeEntry(new GUIContent(menus[i]))
                                {
                                    level = 1 + i,
                                    userData = new NodePortData { node = node.pNode }
                                });
                            }
                            else
                                tree.Add(new SearchTreeGroupEntry(new GUIContent(menus[i])) { level = 1 + i });
                        }
                    }
                    else
                    {
                        // 一级菜单：节点名
                        tree.Add(new SearchTreeGroupEntry(new GUIContent(node.attri.displayName)) { level = 1 });

                        // 二级菜单：输入端口
                        if (m_nodeInputPorts.TryGetValue(node.pNode, out var ports) && ports != null)
                        {
                            for (int i = 0; i < ports.Count; ++i)
                            {
                                tree.Add(new SearchTreeEntry(new GUIContent(ports[i].name))
                                {
                                    level = 2,
                                    userData = new NodePortData { node = node.pNode, attr = ports[i], index = i }
                                });
                            }
                        }
                    }

                }
            }
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (entry.userData is NodePortData data)
            {
                onSelect?.Invoke(data.node, data.attr, data.index);
                return true;
            }
            return false;
        }

        [System.Serializable]
        public class NodePortData
        {
            public BaseNode node;
            public ArgvAttribute attr;
            public int index;
        }
    }
}
#endif