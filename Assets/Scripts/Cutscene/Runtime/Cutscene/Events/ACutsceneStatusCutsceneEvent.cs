/********************************************************************
生成日期:	06:30:2025
类    名: 	CutscenePlayCutsceneEvent
作    者:	HappLI
描    述:	执行行为树节点
*********************************************************************/
using Framework.AT.Runtime;
using Framework.DrawProps;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    public abstract class ACutsceneStatusCutsceneEvent : IBaseEvent
    {
        [Display("基本属性")]public BaseEventProp baseProp;

        [Display("剧情名"), RowFieldInspector] public string cutsceneName;
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new CutsceneCutsceneEventDriver();
        }
        //-----------------------------------------------------
        public abstract ushort GetIdType();
        //-----------------------------------------------------
        public string GetName()
        {
            return baseProp.name;
        }
        //-----------------------------------------------------
        public float GetTime()
        {
            return baseProp.time;
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        [NonSerialized] List<string> m_vCutscenePops = new List<string>();
        public void OnDrawFieldLineRow(System.Object pOwner, System.Reflection.FieldInfo fieldInfo)
        {
            if (fieldInfo.Name == "cutsceneName")
            {
                if(m_vCutscenePops.Count<=0)
                {
                    m_vCutscenePops.Add("None");
                    var assets = UnityEditor.AssetDatabase.FindAssets("t:CutsceneObject");
                    for (int i = 0; i < assets.Length; ++i)
                    {
                        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[i]);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CutsceneObject>(path);
                        if (asset != null)
                        {
                            var name = asset.name;
                            if (!m_vCutscenePops.Contains(name))
                                m_vCutscenePops.Add(name);
                        }
                    }
                }
                int idx = Mathf.Max(0, m_vCutscenePops.IndexOf(cutsceneName));
                idx = EditorGUILayout.Popup("", idx, m_vCutscenePops.ToArray());
                if (idx >= 0 && idx < m_vCutscenePops.Count)
                {
                    if (idx <= 0) cutsceneName = "";
                    else cutsceneName = m_vCutscenePops[idx];
                }
            }
        }
#endif
    }
}