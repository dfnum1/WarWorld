/********************************************************************
生成日期:	11:03:2023
类    名: 	CutsceneObject
作    者:	HappLI
描    述:	过场unity 存储对象
*********************************************************************/
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Framework.Cutscene.Editor;
#endif

namespace Framework.Cutscene.Runtime
{
    public class CutsceneObject : ScriptableObject/*, ISerializationCallbackReceiver*/
    {
        [System.NonSerialized] bool m_bInited = false;
        [NonSerialized]
        CutsceneGraph m_cutsceneGraph = new CutsceneGraph();

        [SerializeField, HideInInspector]
        private string cutsceneDataJson = string.Empty;
        //-----------------------------------------------------
        public CutsceneGraph GetCutsceneGraph(bool bForce = false)
        {
            if (!m_bInited || bForce)
            {
                if (!string.IsNullOrEmpty(cutsceneDataJson))
                    m_cutsceneGraph.OnDeserialize(cutsceneDataJson);
                m_bInited = true;
            }
            if (m_cutsceneGraph!=null) m_cutsceneGraph.SetOwnerObject(this);
            return m_cutsceneGraph;
        }
        //-----------------------------------------------------
#if UNITY_EDITOR
        internal void Save()
        {
            if (m_bInited)
            {
                cutsceneDataJson = m_cutsceneGraph.OnSerialize();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        //-----------------------------------------------------
        internal string GetJsonData()
        {
            return cutsceneDataJson;
        }
#endif
        /*
        //-----------------------------------------------------
        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(cutsceneDataJson))
                return;
            cutsceneGraph.OnDeserialize(cutsceneDataJson);
        }
        //-----------------------------------------------------
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            cutsceneDataJson = cutsceneGraph.OnSerialize();
#endif
        }
        */
#if UNITY_EDITOR
        //-----------------------------------------------------
        internal string GetCutsceneDataJson()
        {
            return cutsceneDataJson;
        }
        //-----------------------------------------------------
        [MenuItem("Assets/播放Cutscene", true)]
        private static bool ValidatePlayCutscene()
        {
            if (!Application.isPlaying)
                return false;
            var runtimeMgr = DataUtils.GetRuntimeCutsceneManger();
            if (runtimeMgr == null) return false;
            var obj = Selection.activeObject as CutsceneObject;
            return obj != null;
        }
        //-----------------------------------------------------

        [MenuItem("Assets/播放Cutscene", false, 0)]
        private static void PlayCutscene()
        {
            var obj = Selection.activeObject as CutsceneObject;
            if (obj != null)
            {
                var runtimeMgr = DataUtils.GetRuntimeCutsceneManger();
                if (runtimeMgr != null)
                {
                    var grpahData = ((CutsceneObject)obj).GetCutsceneGraph();
                    var cutscenes = runtimeMgr.GetAllCutscenes();
                    if(cutscenes!=null)
                    {
                        foreach(var db in cutscenes)
                        {
                            if (db.Value.GetCutsceneData() == grpahData)
                            {
                                CutsceneEditor[] editors = EditorWindow.FindObjectsOfType<CutsceneEditor>();
                                foreach(var window in editors)
                                {
                                    if(window.GetCutsceneInstance() == db.Value)
                                    {
                                        window.Focus();
                                        return;
                                    }
                                }

                                CutsceneEditor newWindow = EditorWindow.GetWindow<CutsceneEditor>();
                                newWindow.titleContent = new GUIContent("过场编辑器");
                                newWindow.OpenRuntimePlayingCutscene(db.Value);
                                return;
                            }
                        }
                    }
                    var runtime = runtimeMgr.CreateCutscene(grpahData, obj.name);
                    if (runtime != null) runtime.Play();
                }
            }
        }
#endif
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(CutsceneObject))]
    public class CutsceneObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            CutsceneObject cutsceneObject = (CutsceneObject)target;
            EditorGUILayout.TextArea(cutsceneObject.GetCutsceneDataJson(), new GUILayoutOption[] { GUILayout.Height(300) });
            if (GUILayout.Button("编辑"))
            {
                Editor.CutsceneEditor.Open(cutsceneObject);
            }
        }
    }
#endif
}