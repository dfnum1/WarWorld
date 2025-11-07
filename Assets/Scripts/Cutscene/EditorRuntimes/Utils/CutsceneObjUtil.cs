/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneObjUtil
作    者:	HappLI
描    述:	监听双击cutscebeobj 资源
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    public struct CutObjActionInfo
    {
        public string name;
        public float length;
        public bool bLoop;
    }
    //-----------------------------------------------------
    public static class CutsceneObjUtil
    {
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj != null && obj is Framework.Cutscene.Runtime.CutsceneObject)
            {
                // 自动打开 CutsceneEdit
                Framework.Cutscene.Editor.CutsceneEditor.Open(obj as Framework.Cutscene.Runtime.CutsceneObject);
                return true; // 已处理
            }
            return false; // 未处理，交给其他处理
        }
        //-----------------------------------------------------
        public static void DrawCutsceneObjectsGUI(List<ICutsceneObject> vObjects)
        {
            if (vObjects == null)
                return;
            UnityEditor.EditorGUILayout.LabelField("绑定对象个数:" + vObjects.Count);
            for (int i = 0; i < vObjects.Count; ++i)
            {
                var obj = vObjects[i];
                DrawCutsceneObjectGUI(vObjects, i);
            }
        }
        //-----------------------------------------------------
        static System.Reflection.MethodInfo ms_pAnimationProviderMethod = null;
        public static List<CutObjActionInfo> GetAnimatonsProvider(this ICutsceneObject cutsceneObj)
        {
            if(ms_pAnimationProviderMethod == null)
            {
                foreach (var ass in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    System.Type[] types = ass.GetTypes();
                    for (int i = 0; i < types.Length; ++i)
                    {
                        if(types[i].IsDefined(typeof(CutsceneObjectActionProviderAttribute),false))
                        {
                            var methodAttr = types[i].GetCustomAttribute<CutsceneObjectActionProviderAttribute>();
                            if(!string.IsNullOrEmpty(methodAttr.method))
                            {
                                var method = types[i].GetMethod(methodAttr.method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                                var argvs = method.GetParameters();
                                if (method.ReturnType == typeof(List<CutObjActionInfo>) && argvs != null && argvs.Length ==1 && argvs[0].ParameterType == typeof(ICutsceneObject))
                                {
                                    ms_pAnimationProviderMethod = method;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (ms_pAnimationProviderMethod == null) return new List<CutObjActionInfo>();
            return (List<CutObjActionInfo>)ms_pAnimationProviderMethod.Invoke(null, new object[] { cutsceneObj });
        }
        //-----------------------------------------------------
        public static void DrawCutsceneObjectGUI(List<ICutsceneObject> vObjects, int index)
        {
            if (vObjects == null || vObjects.Count <= 0)
                return;
            if (index < 0) index = 0;
            if (index >= vObjects.Count) index = vObjects.Count - 1;

            var obj = vObjects[index];
            if (obj == null)
            {
                UnityEditor.EditorGUILayout.LabelField("[" + (index + 1) + "]:绑定对象为空");
                return;
            }
            UnityEditor.EditorGUILayout.LabelField("[" + (index + 1) + "]:" + obj.GetType().ToString());
            EditorGUI.indentLevel++;
            if (obj.GetUniyObject() != null)
            {
                UnityEditor.EditorGUILayout.ObjectField("", obj.GetUniyObject(), obj.GetUniyObject().GetType(), true);
            }
            if (obj.GetUniyTransform() != null)
            {
                UnityEditor.EditorGUILayout.ObjectField("", obj.GetUniyTransform(), obj.GetUniyTransform().GetType(), true);
            }
            if (obj.GetAnimator() != null)
            {
                UnityEditor.EditorGUILayout.ObjectField("", obj.GetAnimator(), obj.GetAnimator().GetType(), true);
            }
            if (obj.GetCamera() != null)
            {
                UnityEditor.EditorGUILayout.ObjectField("", obj.GetCamera(), obj.GetCamera().GetType(), true);
            }
            EditorGUI.indentLevel--;
        }
        //-----------------------------------------------------
        public static void ObjectField(this ICutsceneObject pCutsceneObj, string displayName)
        {
            if (pCutsceneObj == null)
            {
                UnityEditor.EditorGUILayout.LabelField("[" + displayName + "]:绑定对象为空");
                return;
            }
            UnityEditor.EditorGUILayout.BeginHorizontal();
            if (pCutsceneObj.GetUniyObject() != null)
            {
                UnityEditor.EditorGUILayout.ObjectField(displayName, pCutsceneObj.GetUniyObject(), pCutsceneObj.GetUniyObject().GetType(), true);
            }
            else if (pCutsceneObj.GetUniyTransform() != null)
            {
                UnityEditor.EditorGUILayout.ObjectField(displayName, pCutsceneObj.GetUniyTransform(), pCutsceneObj.GetUniyTransform().GetType(), true);
            }
            else if (pCutsceneObj.GetAnimator() != null)
            {
                UnityEditor.EditorGUILayout.ObjectField(displayName, pCutsceneObj.GetAnimator(), pCutsceneObj.GetAnimator().GetType(), true);
            }
            else if (pCutsceneObj.GetCamera() != null)
            {
                UnityEditor.EditorGUILayout.ObjectField(displayName, pCutsceneObj.GetCamera(), pCutsceneObj.GetCamera().GetType(), true);
            }
            UnityEditor.EditorGUILayout.EndHorizontal();
        }
    }
    //-----------------------------------------------------
    [InitializeOnLoad]
    public static class CutsceneObjectIconEditor
    {
        static Texture2D s_CustomIcon;
        static string ms_installPath = "";
        public static string BuildInstallPath()
        {
            if (string.IsNullOrEmpty(ms_installPath))
            {
                var scripts = UnityEditor.AssetDatabase.FindAssets("t:Script CutsceneEditor");
                if (scripts.Length > 0)
                {
                    string installPath = System.IO.Path.GetDirectoryName(UnityEditor.AssetDatabase.GUIDToAssetPath(scripts[0])).Replace("\\", "/");

                    installPath = Path.Combine(installPath, "EditorResources").Replace("\\", "/");
                    if (System.IO.Directory.Exists(installPath))
                    {
                        ms_installPath = installPath;
                    }
                }
            }
            return ms_installPath;
        }
        //-----------------------------------------------------
        static CutsceneObjectIconEditor()
        {
            string installPath = BuildInstallPath();
            if (string.IsNullOrEmpty(installPath))
                return;

            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;

            s_CustomIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(BuildInstallPath(), "Cutscene.png"));
        }
        //-----------------------------------------------------
        static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            if (s_CustomIcon == null) return;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (obj is CutsceneObject)
            {
                //      Rect iconRect = new Rect(selectionRect.x + 2, selectionRect.y + 2, 16, 16);
                //       GUI.DrawTexture(iconRect, s_CustomIcon, ScaleMode.ScaleToFit);
                if(EditorGUIUtility.GetIconForObject(obj) != s_CustomIcon)
                    EditorGUIUtility.SetIconForObject(obj, s_CustomIcon);
            }
        }
    }
}

#endif