#if UNITY_EDITOR
/********************************************************************
生成日期:		29:07:2025
类    名: 	BindIdInputPopup
作    者:	HappLI
描    述:	id 输入框
*********************************************************************/
using Framework.Cutscene.Runtime;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace Framework.Cutscene.Editor
{
    public class BindIdInputPopup : EditorWindow
    {
        private CutsceneObjectBinder binder;
        private int newBindId;
        static BindIdInputPopup ms_Instance = null;

        private CutsceneData.Group m_EditGroup;
        public System.Action<CutsceneObjectBinder, CutsceneData.Group> editCallback;
        //--------------------------------------------------------
        public static void Show(CutsceneObjectBinder binder, CutsceneData.Group group, System.Action<CutsceneObjectBinder,CutsceneData.Group> callback = null)
        {
            if (ms_Instance != null)
            {
                ms_Instance.editCallback = callback;
                ms_Instance.m_EditGroup = group;
                ms_Instance.titleContent = new GUIContent("设置BindID-" + binder.gameObject.name);
                ms_Instance.binder = binder;
                ms_Instance.ShowUtility();
                ms_Instance.Focus();
                return;
            }
            var window = ScriptableObject.CreateInstance<BindIdInputPopup>();
            window.binder = binder;
            window.m_EditGroup = group;
            window.editCallback = callback;
            ms_Instance = window;
            window.newBindId = binder != null ? binder.GetBindID() : 0;
            window.titleContent = new GUIContent("设置BindID-" + binder.gameObject.name);
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 80);
         //   if (bModal)
          //      window.ShowModalUtility();
         //   else
                window.ShowUtility();
            window.Focus();
        }
        //--------------------------------------------------------
        void OnLostFocus()
        {
            this.Focus(); // 失去焦点时重新获取
        }
        //--------------------------------------------------------
        private void OnDisable()
        {
            ms_Instance = null;
        }
        //--------------------------------------------------------
        void OnGUI()
        {
            EditorGUILayout.LabelField("输入新的BindID：");
            newBindId = EditorGUILayout.IntField(newBindId);

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("确定"))
            {
                if(newBindId == 0)
                {
                    EditorUtility.DisplayDialog("错误", "BindID不能为0", "确定");
                }
                else
                {
                    if (binder != null)
                    {
                        binder.SetBindID(newBindId);
                        EditorUtility.SetDirty(binder);

                        if (editCallback != null) editCallback(binder, m_EditGroup);
                    }
                    Close();
                }
            }/*
            if (newBindId!=0 && GUILayout.Button("同步预制体"))
            {
                if (binder != null)
                {
                    var prefab = PrefabUtility.GetCorrespondingObjectFromSource(binder.gameObject);
                    if (prefab != null)
                    {
                        var binderPrefab = prefab.GetComponent<CutsceneObjectBinder>();
                        if(binderPrefab == null) 
                        {
                            binderPrefab = prefab.AddComponent<CutsceneObjectBinder>();
                        }
                        binderPrefab.SetBindID(newBindId);
                        EditorUtility.SetDirty(prefab);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", "当前对象不是预制体实例", "确定");
                    }
                }
            }*/
            if (GUILayout.Button("取消"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif