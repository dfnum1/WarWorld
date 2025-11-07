/********************************************************************
生成日期:	06:30:2025
类    名: 	AgentTreeSearcher
作    者:	HappLI
描    述:	节点搜索器
*********************************************************************/
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Framework.AT.Editor
{
    public class AgentTreeSearcher : ScriptableObject, ISearchWindowProvider
    {
        public AgentTreeGraphView ownerGraphView;
        public delegate bool SerchMenuWindowOnSelectEntryDelegate(SearchTreeEntry searchTreeEntry,            //声明一个delegate类
        SearchWindowContext context);
        public SerchMenuWindowOnSelectEntryDelegate OnSelectEntryHandler;
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("添加节点")) { level = 0 }
            };

            // 路径到分组的映射，key为完整路径，value为分组entry   
            var groupMap = new Dictionary<string, SearchTreeGroupEntry>
            {
                { "", (SearchTreeGroupEntry)tree[0] }
            };

            foreach (var item in AgentTreeUtil.GetAttrs())
            {
                if (!item.actionAttr.bShow)
                    continue;

                // 解析菜单路径
                string menuPath = item.strMenuName ?? item.displayName;
                string[] pathParts = menuPath.Split('/');
                string curPath = "";
                int level = 1;
                SearchTreeGroupEntry parentGroup = groupMap[""];

                // 逐级创建分组
                for (int i = 0; i < pathParts.Length - 1; ++i)
                {
                    string part = pathParts[i].Trim();
                    if (string.IsNullOrEmpty(part)) continue;
                    curPath = curPath == "" ? part : curPath + "/" + part;
                    if (!groupMap.TryGetValue(curPath, out var group))
                    {
                        group = new SearchTreeGroupEntry(new GUIContent(part, item.tips)) { level = level };
                        tree.Add(group);
                        groupMap[curPath] = group;
                    }
                    parentGroup = group;
                    level++;
                }

                // 创建最终的节点
                string displayName = pathParts[^1];
                var entry = item.iconAttr != null
                    ? new SearchTreeEntry(new GUIContent(displayName, AgentTreeUtil.LoadIcon(item.iconAttr.name), item.tips))
                    : new SearchTreeEntry(new GUIContent(displayName, item.tips));
                entry.level = level;
                entry.userData = item;
                tree.Add(entry);
            }

            if (ownerGraphView!=null)
            {
                SearchTreeGroupEntry callBack = new SearchTreeGroupEntry(new GUIContent("节点返回值"))
                {
                    level = 1
                };
                tree.Add(callBack);
                var ports = ownerGraphView.GetArvgPorts();
                foreach(var db in ports)
                {
                    if (db.isInput)
                        continue;
                    SearchTreeGroupEntry nodeGp = new SearchTreeGroupEntry(new GUIContent(db.grapNode.title))
                    {
                        level = 2
                    };
                    tree.Add(nodeGp);
                    string name = db.GetName();
                    tree.Add(new SearchTreeEntry(new GUIContent(name))
                    {
                        level = 3,
                        userData = db
                    });
                }
            }
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (OnSelectEntryHandler == null)
            {
                return false;
            }
            return OnSelectEntryHandler(entry, context);
        }
    }
    /*
    public struct AgentTreeNodeParam
    {
        public Vector2 mousePos;
        public Vector2 gridPos;
        public AgentTreeAttri Data;
    }
    //------------------------------------------------------
    public partial class AgentTreeSearcher
    {
        public struct ItemEvent
        {
            public AgentTreeNodeParam param;
            public System.ActionNode<object> callback;
        }
        protected enum EState
        {
            Open,
            Close,
        }
        public Rect inspectorRect = new Rect(1, 22, 120, 50);
        protected Texture2D m_pBTest = null;

        protected EState m_nState = EState.Close;
        protected Vector2 m_scrollPosition;
        protected string m_searchString = "";
        protected AssetTree m_assetTree;
        protected AssetTreeIMGUI m_assetTreeGUI;
        protected AgentTreeLogic m_pLogic;
        System.Object m_pBindData = null;
        protected Dictionary<string, ItemEvent> m_vEvents = new Dictionary<string, ItemEvent>();
        //------------------------------------------------------
        public void Open(AgentTreeLogic pLogic, Rect rect, System.Object bindData = null)
        {
            m_pLogic = pLogic;
            m_pBindData = bindData;
            inspectorRect = rect;
            if (m_nState == EState.Open) return;
            m_nState = EState.Open;
            Init();
            Search(m_searchString);
            OnOpen();
        }
        //------------------------------------------------------
        protected virtual void OnOpen() { }

        //------------------------------------------------------
        public void Close()
        {
            if (m_nState == EState.Close) return;
            m_nState = EState.Close;
            OnClose();
        }
        //------------------------------------------------------
        protected virtual void OnClose() { }
        //------------------------------------------------------
        public bool IsOpen()
        {
            return m_nState == EState.Open;
        }
        //--------------------------------------------------
        public bool IsMouseIn(Vector2 mouse)
        {
            if (m_nState == EState.Open && inspectorRect.Contains(mouse)) return true;
            return false;
        }

        //------------------------------------------------------
        void Init()
        {
            if (m_assetTree == null)
            {
                m_assetTree = new AssetTree();
                m_assetTreeGUI = new AssetTreeIMGUI(m_assetTree.Root);
                m_assetTreeGUI.onSelected = OnSelected;
                m_assetTreeGUI.onDoubleClick = OnDoubleSelected;
                m_assetTreeGUI.onDraw = OnDrawItem;
            }
        }//------------------------------------------------------
        protected virtual void OnSelected(AssetData data, bool bSelected)
        {
            if (data.guid == null || !bSelected) return;
            if (data.guid == null) return;

            ItemEvent evt;
            if (m_vEvents.TryGetValue(data.guid, out evt) && evt.callback != null)
            {
                evt.callback(evt.param);
            }
            Close();
        }
        //------------------------------------------------------
        protected virtual void OnDoubleSelected(AssetData data)
        {
            if (data.guid == null) return;

            //             ItemEvent evt;
            //             if (m_vEvents.TryGetValue(data.guid, out evt) && evt.callback != null)
            //             {
            //                 evt.callback(evt.param);
            //             }
            //             Close();
        }
        //------------------------------------------------------
        protected virtual void OnGUI()
        {
            DrawList(inspectorRect);
        }
        //------------------------------------------------------
        protected void DrawList(Rect rect)
        {
            if (m_pBTest == null)
            {
                m_pBTest = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                for (int x = 0; x < 2; ++x)
                {
                    for (int z = 0; z < 2; ++z)
                    {
                        m_pBTest.SetPixel(x, z, new Color(1, 1, 1, 0.8f));
                    }
                }
                m_pBTest.Apply();
            }
            // draw search
            GUILayout.BeginArea(rect, m_pBTest);
            GUI.Box(new Rect(0, 0, rect.width, rect.height), m_pBTest);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUILayout.LabelField("Search:", EditorStyles.miniLabel, GUILayout.Width(40));

            EditorGUI.BeginChangeCheck();

            //搜索栏聚焦
            GUI.SetNextControlName("search");
            m_searchString = EditorGUILayout.TextField(m_searchString, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
            GUI.FocusControl("search");

            if (EditorGUI.EndChangeCheck())
            {
                Search(m_searchString);
            }

            EditorGUILayout.EndHorizontal();

            // draw tree
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

            m_assetTreeGUI.DrawTreeLayout();

            EditorGUILayout.EndScrollView();

            GUILayout.EndArea();
        }
        //------------------------------------------------------
        void OnDrawItem(AssetData data)
        {
        //    if (data.icon == null)
       //         data.icon = Resources.Load<Texture2D>("at_tr_function");
        }
        //------------------------------------------------------
        public void OnDraw()
        {
            if (m_nState != EState.Open) return;
            Init();

            OnGUI();
        }
        //------------------------------------------------------
        public void Update(float fTime)
        {

        }
        //------------------------------------------------------
        protected void Search(string query)
        {
            HashSet<char> vQuery = new HashSet<char>();
            for (int i = 0; i < query.Length; ++i)
                vQuery.Add(query[i]);

            m_vEvents.Clear();
            int id = 0;
            {
                foreach (var item in AgentTreeUtil.GetAttrs())
                {
                    bool bQuerty = IsQuery(query, item.strQueueName);
                    if (!bQuerty) continue;

                    AgentTreeNodeParam param = new AgentTreeNodeParam();
                    param.Data = item;
                    param.mousePos = inspectorRect.position;
                    param.gridPos = m_pLogic.WindowToGridPosition(param.mousePos);

                    m_assetTree.AddUser(id.ToString(), item.displayName, vQuery.Count > 0);

                    m_vEvents.Add(id.ToString(), new ItemEvent() { param = param, callback = m_pLogic.CreateActionNode });
                    id++;
                }
            }
        }  
        //------------------------------------------------------
        bool IsQuery(string query, string strContext)
        {
            if (string.IsNullOrEmpty(query)) return true;
            if (string.IsNullOrEmpty(strContext)) return false;
            if (strContext.Length > query.Length)
            {
                return strContext.ToLower().Contains(query.ToLower());
            }
            return query.ToLower().Contains(strContext.ToLower());
        }
    }*/
}
#endif