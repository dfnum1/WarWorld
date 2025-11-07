/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneObjectBinder
作    者:	HappLI
描    述:	cutscene对象绑定器
*********************************************************************/
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    //-----------------------------------------------------
    [ExecuteAlways,ExecuteInEditMode]
    public class CutsceneObjectBinder : MonoBehaviour
    {
        [SerializeField] private int bindId;
        [SerializeField]private Animator pAnimator;
        [SerializeField] private Camera pCamera;

        private Transform m_pTransform;
        private void Awake()
        {
            Init();
            ObjectBinderUtils.BindObject(this);
        }
        //-----------------------------------------------------
        public void Init()
        {
            m_pTransform = this.transform;
            if (pAnimator == null)
            {
                pAnimator = GetComponent<Animator>();
                if (pAnimator == null) pAnimator = GetComponentInChildren<Animator>();
            }
        }
        //-----------------------------------------------------
        private void OnDestroy()
        {
            ObjectBinderUtils.UnBindObject(this);
        }
        //-----------------------------------------------------
        public int GetBindID()
        {
            return bindId;
        }
        //-----------------------------------------------------
        public void SetBindID(int id)
        {
            if (id == bindId)
                return;
            ObjectBinderUtils.UnBindObject(this);
            bindId = id;
            ObjectBinderUtils.BindObject(this);
        }
        //-----------------------------------------------------
        public Animator GetAnimator()
        {
            return pAnimator;
        }
        //-----------------------------------------------------
        public Transform GetTransform()
        {
            return m_pTransform;
        }
        //-----------------------------------------------------
        public Camera GetCamera()
        {
            return pCamera;
        }
    }
    //-----------------------------------------------------
#if UNITY_EDITOR
    [CustomEditor(typeof(CutsceneObjectBinder))]
    public class CutsceneObjectBinderEditor : UnityEditor.Editor
    {
        //-----------------------------------------------------
        void OnEnable()
        {
            CutsceneObjectBinder cutsceneObject = (CutsceneObjectBinder)target;
            //帮我检测pAnimator和pCamera是否有设置,如果则尝试自动绑定
            if (cutsceneObject.GetAnimator() == null)
            {
                serializedObject.FindProperty("pAnimator").objectReferenceValue = cutsceneObject.GetComponent<Animator>();
            }
            if (cutsceneObject.GetCamera() == null)
            {
                serializedObject.FindProperty("pCamera").objectReferenceValue = cutsceneObject.GetComponent<Camera>();
            }
        }
        //-----------------------------------------------------
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CutsceneObjectBinder cutsceneObject = (CutsceneObjectBinder)target;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pAnimator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pCamera"));
            if (cutsceneObject.GetBindID() == 0)
            {
                EditorGUILayout.BeginHorizontal();
                cutsceneObject.SetBindID( EditorGUILayout.IntField("识别Id", cutsceneObject.GetBindID()));
                EditorGUILayout.HelpBox("不能为0", MessageType.Error);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                cutsceneObject.SetBindID( EditorGUILayout.IntField("识别Id", cutsceneObject.GetBindID()));
            }
            if(serializedObject.ApplyModifiedProperties())
                EditorUtility.SetDirty(target);
        }
    }
#endif
    //-----------------------------------------------------
    internal struct BinderUnityObject : ICutsceneObject
    {
        CutsceneObjectBinder m_pBinder;
        //-----------------------------------------------------
        internal BinderUnityObject(CutsceneObjectBinder binder)
        {
            m_pBinder = binder;
        }
        //-----------------------------------------------------
        public bool IsValid()
        {
            return m_pBinder != null;
        }
        //-----------------------------------------------------
        public void Destroy()
        {                
        }
        //-----------------------------------------------------
        public Animator GetAnimator()
        {
            if (m_pBinder) return m_pBinder.GetAnimator();
            return null;
        }
        //-----------------------------------------------------
        public CutsceneObjectBinder GetBinder()
        {
            return m_pBinder;
        }
        //-----------------------------------------------------
        public Object GetUniyObject()
        {
            if (m_pBinder == null) return null;
            return m_pBinder.gameObject;
        }
        //-----------------------------------------------------
        public Transform GetUniyTransform()
        {
            if (m_pBinder == null) return null;
            return m_pBinder.GetTransform();
        }
        //-----------------------------------------------------
        public Camera GetCamera()
        {
            if (m_pBinder == null) return null;
            return m_pBinder.GetCamera();
        }
        //-----------------------------------------------------
        public bool SetParameter(EParamType type, CutsceneParam paramData)
        {
            return false;
        }
        //-----------------------------------------------------
        public bool GetParameter(EParamType type, ref CutsceneParam paramData)
        {
            return false;
        }
    }
    //-----------------------------------------------------
    [DrawProps.Inspector(typeof(BinderUnityObject), "DrawBinderInspector")]
    internal static class ObjectBinderUtils
    {
        internal static System.Action<int, BinderUnityObject, bool> OnBinderCutsceneObject;
        static Dictionary<int, BinderUnityObject> ms_Binders = null;
#if UNITY_EDITOR
        static long ms_LastBindTime = 0;
#endif
        //-----------------------------------------------------
        internal static BinderUnityObject GetBinder(int bindId)
        {
            if (ms_Binders == null)
            {
#if UNITY_EDITOR
                CutsceneObjectBinder[] binders = GameObject.FindObjectsOfType<CutsceneObjectBinder>(true);
                for (int i = 0; i < binders.Length; ++i)
                    BindObject(binders[i]);
#endif
            }
            if (ms_Binders == null || !ms_Binders.TryGetValue(bindId, out BinderUnityObject binder))
            {
                return default;
            }
            return binder;
        }
        //-----------------------------------------------------
        internal static void BindObject(CutsceneObjectBinder gameObject)
        {
            if (gameObject.GetBindID() == 0)
                return;
            if (ms_Binders == null)
                ms_Binders = new Dictionary<int, BinderUnityObject>(8);
            BinderUnityObject unityObj = new BinderUnityObject(gameObject);
            ms_Binders[gameObject.GetBindID()] = unityObj;
            if (OnBinderCutsceneObject != null)
                OnBinderCutsceneObject(gameObject.GetBindID(), unityObj, true);
        }
        //-----------------------------------------------------
        internal static void UnBindObject(CutsceneObjectBinder gameObject)
        {
            if (ms_Binders == null) return;
            if(ms_Binders.TryGetValue(gameObject.GetBindID(), out var binder))
            {
                ms_Binders.Remove(gameObject.GetBindID());
                if (OnBinderCutsceneObject != null)
                    OnBinderCutsceneObject(gameObject.GetBindID(), binder, false);
            }
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        internal static int DrawBinderInspector(int bindId, string name)
        {
            var binders = ObjectBinderUtils.GetBinder(bindId);
            var obj = binders.GetBinder();
            EditorGUI.BeginChangeCheck();
            var gameObj = (GameObject)EditorGUILayout.ObjectField(name,obj?obj.gameObject:null, typeof(GameObject), true);
            bool changed = EditorGUI.EndChangeCheck();
            if (gameObj!=null)
            {
                if(changed)
                {
                    CutsceneObjectBinder binder = gameObj.GetComponent<CutsceneObjectBinder>();
                    if (binder == null)
                    {
                        binder = gameObj.AddComponent<CutsceneObjectBinder>();
                    }
                    if (binder.GetBindID() == 0)
                    {
                        Framework.Cutscene.Editor.BindIdInputPopup.Show(binder, null);

                        if (binder.GetBindID() != 0)
                        {
                            obj = binder;
                        }
                        else
                        {
                          //  EditorUtility.DisplayDialog("错误", "绑定ID不能为0，请重新设置！", "确定");
                        }
                    }
                    else obj = binder;
                    if (obj) obj.Init();
                }
            }
            if (obj != null)
            {
                bindId = obj.GetBindID();
                ObjectBinderUtils.BindObject(obj);
            }
            else
            {
                bindId = 0;
            }
            return bindId;

        }
#endif
    }
}