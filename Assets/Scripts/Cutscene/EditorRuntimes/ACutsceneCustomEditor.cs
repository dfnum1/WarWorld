/********************************************************************
生成日期:	06:30:2025
类    名: 	ACutsceneCustomEditor
作    者:	HappLI
描    述:	自定义编辑
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using Framework.Cutscene.Runtime;
using Framework.ED;
using System.Collections.Generic;
using UnityEditor;

namespace Framework.Cutscene.Editor
{
    public class ACutsceneCustomEditor
    {
        private IDraw m_pDraw;
        System.Action<IDraw> m_pDefaultDraw = null;
        //-----------------------------------------------------
        public ACutsceneCustomEditor()
        {

        }
        //-----------------------------------------------------
        internal void SetDraw(IDraw pDraw)
        {
            m_pDraw = pDraw;
        }
        //-----------------------------------------------------
        internal void SetDefaultDraw(System.Action<IDraw> onDraw)
        {
            m_pDefaultDraw = onDraw;
        }
        //-----------------------------------------------------
        public T GetData<T>() where T : IDataer
        {
            if (m_pDraw == null) return default;
            var pData = m_pDraw.GetData();
            if (pData == null) return default(T);
            if (pData is T) return ((T)pData);
            return default(T);
        }
        //-----------------------------------------------------
        public void ApplayModify(IDataer pDater)
        {
            if (m_pDraw == null)
                return;
            m_pDraw.SetData(pDater);
        }
        //-----------------------------------------------------
        internal bool DrawInspector()
        {
            if (m_pDraw == null)
                return false;
            return OnInspector();
        }
        //-----------------------------------------------------
        public void DrawDefaultInspector()
        {
            if (m_pDefaultDraw != null && m_pDraw!=null)
                m_pDefaultDraw(m_pDraw);
        }
        //-----------------------------------------------------
        protected virtual bool OnInspector()
        {
            return false;
        }
        //-----------------------------------------------------
        public virtual void OnEnable()
        {
            if (m_pDraw == null)  return;
            var method = GetType().GetMethod(nameof(OnSceneView), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (method != null && method.DeclaringType != typeof(ACutsceneCustomEditor))
            {
                SceneView.duringSceneGui += OnSceneView;
            }
        }
        //-----------------------------------------------------
        public virtual void OnDisable()
        {
            if (m_pDraw == null) return;
            SceneView.duringSceneGui -= OnSceneView;
        }
        //-----------------------------------------------------
        public virtual void OnUpdate(float deltaTime) { }
        //-----------------------------------------------------
        public virtual void OnSceneView(SceneView sceneView)
        {

        }
        //-----------------------------------------------------
        public void ShowNotification(string content, float fadeout = 1.0f)
        {
            if(SceneView.currentDrawingSceneView)
            {
                SceneView.currentDrawingSceneView.ShowNotification(new UnityEngine.GUIContent(content), fadeout);
            }
            if(SceneView.lastActiveSceneView)
            {
                SceneView.lastActiveSceneView.ShowNotification(new UnityEngine.GUIContent(content), fadeout);
            }
        }
    }
}

#endif