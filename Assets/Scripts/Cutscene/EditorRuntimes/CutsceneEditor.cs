/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneEditor
作    者:	HappLI
描    述:	过场动作编辑器
*********************************************************************/
#if UNITY_EDITOR
using Framework.ED;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using Framework.Cutscene.Runtime;

namespace Framework.Cutscene.Editor
{
    public class CutsceneEditor : EditorWindowBase
    {
        private const float EDGE_SNAP_OFFSET = 1;
        enum EDragEdge
        {
            None = 0,
            Asset,
            Inspector,
        }
        struct SplitData
        {
            public float rate;
            public bool bOpen;
        }

        float                           m_fToolSize = 25;
        Rect                            m_InspectorRect;
        Vector2                         m_InspectorScoller;
        Rect                            m_TimelineRect;
        Vector2                         m_TimelineScoller;
        Rect                            m_AssetRect;
        Vector2                         m_AssetScoller;

        EDragEdge                       m_eDragEdge = EDragEdge.None;

        SplitData m_ViewLeftRate;
        SplitData m_ViewRightRate;

        GUIStyle                        m_TileStyle;
        CutsceneManager                 m_CutsceneManager = null;
        CutsceneInstance                m_pCutscene = null;

        AgentTreeWindow                 m_pAgentTreeEdit;

        string m_lastContentMd5 = null;
        bool m_bRuntimeOpenPlayingCutscene = false;
        //--------------------------------------------------------
        [MenuItem("Tools/过场编辑器")]
        public static void Open()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
                return;
            }
            CutsceneEditor window = EditorWindow.GetWindow<CutsceneEditor>();
            window.titleContent = new GUIContent("过场编辑器");
        }
        //--------------------------------------------------------
        public static void Open(CutsceneObject pObject)
        {
            pObject.GetCutsceneGraph(true);
            CutsceneEditor[] editors = EditorWindow.FindObjectsOfType<CutsceneEditor>();
            if(editors!=null)
            {
                for(int i =0; i < editors.Length; ++i)
                {
                    var logic = editors[i].GetLogic<AssetDrawLogic>();
                    if (logic == null) continue;
                    if (editors[i].GetCurrentObj() == null)
                    {
                        editors[i].OnChangeSelect(pObject);
                        return;
                    }
                }
            }
            CutsceneEditor window = EditorWindow.GetWindow<CutsceneEditor>();
            window.titleContent = new GUIContent("过场编辑器-[" + pObject.name + "]");
            window.OnChangeSelect(pObject);
        }
        //--------------------------------------------------------
        internal void OnSetTime(float time)
        {
            TimelineDrawLogic logic = GetLogic<TimelineDrawLogic>();
            if (logic == null)
                return;
            logic.SetCurrentTime(time);
        }
        //--------------------------------------------------------
        protected override void OnInnerEnable()
        {
            m_CutsceneManager = new CutsceneManager();
            m_CutsceneManager.SetEditorMode(true);
            m_pCutscene = new CutsceneInstance(m_CutsceneManager);
            m_pCutscene.SetEditorMode(true, this);
            this.minSize = new Vector2(600, 400);
            this.wantsMouseMove = true;
            this.wantsMouseEnterLeaveWindow = true;
            this.autoRepaintOnSceneChange = true;
            this.wantsLessLayoutEvents = true;

            m_ViewLeftRate.bOpen = true;
            m_ViewLeftRate.rate = 0.15f;
            m_ViewRightRate.bOpen = true;
            m_ViewRightRate.rate = 0.75f;

            m_TileStyle = new GUIStyle();
            m_TileStyle.fontSize = 20;
            m_TileStyle.normal.textColor = Color.white;
            m_TileStyle.alignment = TextAnchor.MiddleCenter;
        }
        //--------------------------------------------------------
        protected override void OnInnerDisable()
        {
            base.OnInnerDisable();
            if(m_bRuntimeOpenPlayingCutscene)
            {
                SearchPathEditor.StopPick();
                CameraPathEditor.StopEdit();
                CurvePathEditor.StopEdit();
                if (m_pAgentTreeEdit != null) m_pAgentTreeEdit.Close();
                m_pAgentTreeEdit = null;
                return;
            }

            if (!m_bRuntimeOpenPlayingCutscene && m_pCurrentObj != null && m_pCurrentObj is CutsceneObject)
            {
                string jsonData = null;
                CutsceneObject obj = (CutsceneObject)m_pCurrentObj;
                var graph = obj.GetCutsceneGraph();
                if (graph != null)
                {
                    jsonData = graph.OnSerialize();
                }
                if (m_lastContentMd5 != jsonData)
                {
                    if (EditorUtility.DisplayDialog("提示", "当前数据有更改，是要保存？", "保存", "取消"))
                    {
                        SaveChanges();
                    }
                }
            }

            CutsceneCustomAgentEditor.CloseEditor();
            if (m_pCutscene != null)
            {
                m_pCutscene.SetEditorMode(false, null);
                m_pCutscene.Stop(true);
                m_pCutscene.Destroy();
            }
            SearchPathEditor.StopPick();
            CameraPathEditor.StopEdit();
            CurvePathEditor.StopEdit();
            if (m_pAgentTreeEdit != null) m_pAgentTreeEdit.Close();
            m_pAgentTreeEdit = null;
        }
        //--------------------------------------------------------
        protected override void OnInnerUpdate()
        {
            m_pTimer.m_currentSnap = EditorPreferences.GetSettings().playbackSpeedScale;
            RefreshLayout();
            if(IsRuntimeOpenPlayingCutscene())
            {
                if(m_pCutscene!=null)
                {
                    if(m_pCutscene.IsDestroyed())
                    {
                        m_bRuntimeOpenPlayingCutscene = false;
                        m_CutsceneManager = new CutsceneManager();
                        m_pCutscene = new CutsceneInstance(m_CutsceneManager);
                        m_pCutscene.SetEditorMode(true, this);
                        return;
                    }
                }
                return;
            }
            if(m_pCutscene!=null)
                m_pCutscene.Update(0);
            m_CutsceneManager.Update(m_pTimer.deltaTime, m_pCutscene);
        }
        //--------------------------------------------------------
        public CutsceneManager GetCutsceneManager()
        {
            return m_CutsceneManager;
        }
        //--------------------------------------------------------
        public CutsceneInstance GetCutsceneInstance()
        {
            return m_pCutscene;
        }
        //--------------------------------------------------------
        public Rect InspectorRect
        {
            get { return m_InspectorRect; }
        }
        //--------------------------------------------------------
        public Rect AssetRect
        {
            get { return m_AssetRect; }
        }
        //--------------------------------------------------------
        public Rect TimelineRect
        {
            get { return m_TimelineRect; }
        }
        //--------------------------------------------------------
        public GUIStyle TileStyle
        {
            get { return m_TileStyle; }
        }
        //--------------------------------------------------------
        public void OpenAgentTreeEdit()
        {
            if (m_pAgentTreeEdit == null || m_pCurrentObj !=null && m_pCurrentObj is CutsceneObject)
            {
                m_pAgentTreeEdit = AgentTreeWindow.Open(m_pCutscene, (CutsceneObject)m_pCurrentObj);
            }
            if (m_pAgentTreeEdit) m_pAgentTreeEdit.Focus();
        }
        //--------------------------------------------------------
        public AgentTreeWindow GetAgentTreeWindow()
        {
            return m_pAgentTreeEdit;
        }
        //--------------------------------------------------------
        public bool IsRuntimeOpenPlayingCutscene()
        {
            return m_bRuntimeOpenPlayingCutscene;
        }
        //--------------------------------------------------------
        public void OpenRuntimePlayingCutscene(CutsceneInstance pInstance)
        {
            var cutsceneGraphData = pInstance.GetCutsceneData();
            if (cutsceneGraphData == null)
                return;
            var cutsceneObj = cutsceneGraphData.GetOwnerObject();
            if (cutsceneObj == null)
                return;

            m_bRuntimeOpenPlayingCutscene = true;
            m_pCutscene = pInstance;
            m_CutsceneManager = pInstance.GetCutsceneManager();
            OnChangeSelect(cutsceneObj);
            this.titleContent = new GUIContent("过场编辑器-[" + cutsceneObj.name + "]");
        }
        //--------------------------------------------------------
        public override void OnChangeSelect(object pObject)
        {
            if(m_pCurrentObj != pObject)
            {
                if(m_pCurrentObj!=null && m_pCurrentObj is CutsceneObject)
                {
                    string jsonData = null;
                    CutsceneObject obj = (CutsceneObject)m_pCurrentObj;
                    var graph = obj.GetCutsceneGraph();
                    if (graph != null)
                    {
                        jsonData = graph.OnSerialize();
                    }
                    if (m_lastContentMd5 != jsonData)
                    {
                        if (EditorUtility.DisplayDialog("提示", "当前数据有更改，是要保存？", "保存", "取消"))
                        {
                            SaveChanges();
                        }
                    }
                }
            }
            base.OnChangeSelect(pObject);
            if (m_pAgentTreeEdit != null)
                m_pAgentTreeEdit.OnChangeSelect(pObject);

            m_lastContentMd5 = null;
            if (m_pCurrentObj != null && m_pCurrentObj is CutsceneObject)
            {
                CutsceneObject obj = (CutsceneObject)m_pCurrentObj;
                var graph = obj.GetCutsceneGraph();
                if (graph != null)
                {
                    m_lastContentMd5 = obj.GetJsonData();
                }
            }
        }
        //--------------------------------------------------------
        public override void SaveChanges()
        {
            if (m_bRuntimeOpenPlayingCutscene)
                return;
            base.SaveChanges();
            if (m_pAgentTreeEdit != null) m_pAgentTreeEdit.SaveChanges();
            m_lastContentMd5 = null;
            if (m_pCurrentObj != null && m_pCurrentObj is CutsceneObject)
            {
                CutsceneObject obj = (CutsceneObject)m_pCurrentObj;
                var graph = obj.GetCutsceneGraph();
                if (graph != null)
                {
                    m_lastContentMd5 = obj.GetJsonData();
                }
            }
        }
        //--------------------------------------------------------
        public void SaveAgentTreeData()
        {
            if (m_bRuntimeOpenPlayingCutscene)
                return;
            if (m_pAgentTreeEdit != null) m_pAgentTreeEdit.SaveChanges();
        }
        //--------------------------------------------------------
        protected override void OnInnerGUI()
        {
            ProcessDragEdge(Event.current);
        }
        //--------------------------------------------------------
        protected override void OnAfterInnerGUI()
        {
            //! top split toobar line
            UIDrawUtils.DrawColorLine(new Vector2(0, m_fToolSize), new Vector2(this.position.width, m_fToolSize), Color.grey);

            //! asset line
            {
                Color lineColor = Color.grey;
                if (CheckEdgeDrag(EDragEdge.Asset, new Rect(m_AssetRect.xMin, m_AssetRect.yMin, m_AssetRect.width, 10)))
                    lineColor = Color.yellow;
                UIDrawUtils.DrawColorLine(new Vector2(m_AssetRect.xMax, m_AssetRect.yMin), new Vector2(m_AssetRect.xMax, m_AssetRect.yMax), lineColor);
            }
            //! inspector
            {
                Color lineColor = Color.grey;
                if (CheckEdgeDrag(EDragEdge.Inspector, new Rect(m_InspectorRect.xMin, m_InspectorRect.yMin, m_InspectorRect.width, 10)))
                    lineColor = Color.yellow;
                UIDrawUtils.DrawColorLine(new Vector2(m_InspectorRect.xMin + 0.01f, m_InspectorRect.yMin), new Vector2(m_InspectorRect.xMin + 0.01f, m_InspectorRect.yMax), lineColor);
            }

            DrawControllBtn();
        }
        //--------------------------------------------------------
        void DrawControllBtn()
        {
            //! asset line
            {
                if (m_ViewLeftRate.bOpen)
                {
                    var btnRect = new Rect(m_AssetRect.xMax - 21, m_AssetRect.y + 1, 20, 20);
                    if (GUI.Button(btnRect, EditorGUIUtility.TrIconContent("StepLeftButton").image) || CheckBtnRectClick(btnRect))
                    {
                        m_ViewLeftRate.bOpen = false;
                    }
                }
                else
                {
                    var btnRect = new Rect(-1, m_AssetRect.y + 1, 20, 20);
                    if (GUI.Button(btnRect, (EditorGUIUtility.TrIconContent("StepButton").image)) || CheckBtnRectClick(btnRect))
                        m_ViewLeftRate.bOpen = true;
                }
            }
            //! inspector
            {
                if (m_ViewRightRate.bOpen)
                {
                    Rect btnRect = new Rect(m_InspectorRect.x, m_InspectorRect.y + 1, 20, 20);
                    if (GUI.Button(btnRect, EditorGUIUtility.TrIconContent("StepButton").image) || CheckBtnRectClick(btnRect))
                        m_ViewRightRate.bOpen = false;

                }
                else
                {
                    Rect btnRect = new Rect(position.width - 18, m_InspectorRect.y + 1, 20, 20);
                    if (GUI.Button(btnRect, EditorGUIUtility.TrIconContent("StepLeftButton").image) || CheckBtnRectClick(btnRect))
                        m_ViewRightRate.bOpen = true;
                }
            }
        }
        //--------------------------------------------------------
        bool CheckBtnRectClick(Rect rect)
        {
            if(rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
            {
                return true;
            }
            return false;
        }
        //--------------------------------------------------------
        bool CheckEdgeDrag(EDragEdge type, Rect region)
        {
            if (m_eDragEdge == type) return true;
            if (region.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDrag)
                    m_eDragEdge = type;
                return true;
            }
            return false;
        }
        //--------------------------------------------------------
        void ProcessDragEdge(Event evt)
        {
            if (m_eDragEdge == EDragEdge.None) return;
            if (evt.type == EventType.MouseUp)
            {
                m_eDragEdge = EDragEdge.None;
                return;
            }
            if (evt.button == 0)
            {
                if (evt.type != EventType.MouseDrag)
                    return;

                switch (m_eDragEdge)
                {
                    case EDragEdge.Asset:
                        {
                            m_ViewLeftRate.rate += Event.current.delta.x / this.position.width;
                            evt.Use();
                        }
                        break;
                    case EDragEdge.Inspector:
                        {
                            m_ViewRightRate.rate += Event.current.delta.x/this.position.width;
                            evt.Use();
                        }
                        break;
                }
            }
        }
        //--------------------------------------------------------
        void RefreshLayout()
        {
            float height = this.position.height - m_fToolSize;
            m_ViewLeftRate.rate = Mathf.Clamp(m_ViewLeftRate.rate, 0.1f, 0.5f);
            float leftRate = m_ViewLeftRate.bOpen? m_ViewLeftRate.rate:0;

            m_ViewRightRate.rate = Mathf.Clamp(m_ViewRightRate.rate, 0.55f, 0.8f);
            float rightRate = m_ViewRightRate.bOpen ? m_ViewRightRate.rate : 0;

            m_TimelineRect.y = m_AssetRect.y = m_InspectorRect.y = m_fToolSize;
            m_AssetRect.width = this.position.width * leftRate;
            m_AssetRect.height = height;
            if (!m_ViewLeftRate.bOpen) m_AssetRect.width = 20;

            if (m_ViewRightRate.bOpen)
                m_InspectorRect.x = this.position.width * rightRate;
            else
                m_InspectorRect.x = this.position.width ;
            m_InspectorRect.width = this.position.width - m_InspectorRect.x;
            if (!m_ViewRightRate.bOpen) m_InspectorRect.width = 20;
            m_InspectorRect.height = height;

            m_TimelineRect.height = height;
            m_TimelineRect.x = m_AssetRect.width;
            m_TimelineRect.width = this.position.width - m_InspectorRect.width - m_AssetRect.width;
        }
        //-----------------------------------------------------


        protected override void OnInnerEvent(Event evt)
        {
            if(evt.type == EventType.KeyDown)
            {
                if (evt.control && evt.keyCode == KeyCode.Z)
                {
                    GetLogic<UndoLogic>().UndoData();
                    evt.Use();
                }
                else if (evt.control && evt.keyCode == KeyCode.S)
                {
                    this.SaveChanges();
                    evt.Use();
                }
            }

        }
    }
}

#endif