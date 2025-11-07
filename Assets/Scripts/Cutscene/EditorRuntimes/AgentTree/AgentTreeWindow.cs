/********************************************************************
生成日期:	06:30:2025
类    名: 	AgentTreeWindow
作    者:	HappLI
描    述:	行为树编辑器窗口
*********************************************************************/
#if UNITY_EDITOR
using Framework.ED;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using Framework.Cutscene.Runtime;

namespace Framework.Cutscene.Editor
{
    public class AgentTreeWindow : EditorWindowBase
    {
        GUIStyle                        m_TileStyle;
        CutsceneManager                 m_CutsceneManager = null;
        CutsceneInstance                m_pCutscene = null;
        //--------------------------------------------------------
        public static AgentTreeWindow Open(CutsceneInstance cutscene, CutsceneObject pObject)
        {
            pObject.GetCutsceneGraph(true);
            AgentTreeWindow[] editors = EditorWindow.FindObjectsOfType<AgentTreeWindow>();
            if(editors!=null)
            {
                for(int i =0; i < editors.Length; ++i)
                {
                    if (editors[i].m_pCutscene == cutscene || editors[i].m_pCutscene == null)
                    {
                        editors[i].m_pCutscene = cutscene;
                        editors[i].OnChangeSelect(pObject); 
                        editors[i].Focus();
                        return editors[i];
                    }
                }
            }
            AgentTreeWindow window = EditorWindow.GetWindow<AgentTreeWindow>();
            window.titleContent = new GUIContent("过场编辑器-[" + pObject.name + "]-行为树");
            window.m_pCurrentObj = cutscene;
            window.OnChangeSelect(pObject);
            return window;
        }
        //--------------------------------------------------------
        protected override void OnInnerEnable()
        {
            m_CutsceneManager = new CutsceneManager();
            m_pCutscene = new CutsceneInstance(m_CutsceneManager);
            m_pCutscene.SetEditorMode(true, this);
            this.minSize = new Vector2(600, 400);
            this.wantsMouseMove = true;
            this.wantsMouseEnterLeaveWindow = true;
            this.autoRepaintOnSceneChange = true;
            this.wantsLessLayoutEvents = true;

            m_TileStyle = new GUIStyle();
            m_TileStyle.fontSize = 20;
            m_TileStyle.normal.textColor = Color.white;
            m_TileStyle.alignment = TextAnchor.MiddleCenter;
        }
        //--------------------------------------------------------
        public Rect PreviewRect
        {
            get
            {
                return new Rect(0, 0, position.width, position.height);
            }
        }
        //--------------------------------------------------------
        protected override void OnInnerDisable()
        {
            base.OnInnerDisable();
        }
        //--------------------------------------------------------
        protected override void OnInnerUpdate()
        {
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
        public GUIStyle TileStyle
        {
            get { return m_TileStyle; }
        }
        //--------------------------------------------------------
        public override void SaveChanges()
        {
            base.SaveChanges();
        }
        //--------------------------------------------------------
        protected override void OnInnerGUI()
        {
        }
        //-----------------------------------------------------
        protected override void OnInnerEvent(Event evt)
        {
        }
    }
}

#endif