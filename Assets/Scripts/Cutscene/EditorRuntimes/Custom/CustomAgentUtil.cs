/********************************************************************
生成日期:	06:30:2025
类    名: 	CustomAgentUtil
作    者:	HappLI
描    述:	自定义事件、剪辑参数工具
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Editor;
using Framework.Cutscene.Runtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    public class CustomAgentUtil
    {
        private static Dictionary<uint, CutsceneCustomAgent.AgentUnit> ms_vEvents = null;
        private static Dictionary<uint, CutsceneCustomAgent.AgentUnit> ms_vClips = null;
        private static List<CutsceneCustomAgent.AgentUnit> ms_vEventLists = null;
        private static List<CutsceneCustomAgent.AgentUnit> ms_vClipLists = null;
        private static List<string> ms_vEventPops = null;
        private static List<string> ms_vClipPops = null;
        private static CutsceneCustomAgent ms_Agent;
        public static void Init(bool bForce = false)
        {
            if(!bForce && ms_vEvents != null && ms_vClips != null && ms_Agent!=null)
            {
                return;
            }
            ms_vEvents = new Dictionary<uint, CutsceneCustomAgent.AgentUnit>();
            ms_vClips = new Dictionary<uint, CutsceneCustomAgent.AgentUnit>();
            ms_vEventLists = new List<CutsceneCustomAgent.AgentUnit>();
            ms_vClipLists = new List<CutsceneCustomAgent.AgentUnit>();
            ms_vEventPops = new List<string>();
            ms_vClipPops = new List<string>();
            string[] cutscenes = AssetDatabase.FindAssets("t:CutsceneCustomAgent");
            for (int i = 0; i < cutscenes.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(cutscenes[i]);
                CutsceneCustomAgent agents = AssetDatabase.LoadAssetAtPath<CutsceneCustomAgent>(path);
                if (agents == null)
                    continue;
                if(agents.vEvents!=null)
                {
                    foreach (var item in agents.vEvents)
                    {
                        if (!item.IsValid()) continue;
                        if (!ms_vEvents.ContainsKey(item.customType))
                        {
                            ms_vEvents.Add(item.customType, item);
                            ms_vEventLists.Add(item);
                            ms_vEventPops.Add(item.name + "[" + item.customType + "]");
                        }
                        else
                        {
                            Debug.LogError($"Custom event type {item.customType} already exists in {path}");
                        }
                    }
                }
                if(agents.vClips!=null)
                {
                    foreach (var item in agents.vClips)
                    {
                        if (!item.IsValid()) continue;
                        if (!ms_vClips.ContainsKey(item.customType))
                        {
                            ms_vClips.Add(item.customType, item);
                            ms_vClipLists.Add(item);
                            ms_vClipPops.Add(item.name + "[" + item.customType + "]");
                        }
                        else
                        {
                            Debug.LogError($"Custom clip type {item.customType} already exists in {path}");
                        }
                    }
                }
                ms_Agent = agents;
                break;//only load the first one
            }
        }
        //-----------------------------------------------------
        internal static void RefreshData(List<CutsceneCustomAgent.AgentUnit> vEvents, List<CutsceneCustomAgent.AgentUnit> vClips)
        {
            ms_vEvents = new Dictionary<uint, CutsceneCustomAgent.AgentUnit>();
            ms_vClips = new Dictionary<uint, CutsceneCustomAgent.AgentUnit>();
            ms_vEventLists = new List<CutsceneCustomAgent.AgentUnit>();
            ms_vClipLists = new List<CutsceneCustomAgent.AgentUnit>();
            ms_vEventPops = new List<string>();
            ms_vClipPops = new List<string>();
            if (vEvents != null)
            {
                foreach (var item in vEvents)
                {
                    if (!item.IsValid()) continue;
                    if (!ms_vEvents.ContainsKey(item.customType))
                    {
                        ms_vEvents.Add(item.customType, item);
                        ms_vEventLists.Add(item);
                        ms_vEventPops.Add(item.name + "[" + item.customType + "]");
                    }
                    else
                    {
                        Debug.LogError($"Custom event type {item.customType} already exists");
                    }
                }
            }
            if (vClips != null)
            {
                foreach (var item in vClips)
                {
                    if (!item.IsValid()) continue;
                    if (!ms_vClips.ContainsKey(item.customType))
                    {
                        ms_vClips.Add(item.customType, item);
                        ms_vClipLists.Add(item);
                        ms_vClipPops.Add(item.name + "[" + item.customType + "]");
                    }
                    else
                    {
                        Debug.LogError($"Custom clip type {item.customType} already exists");
                    }
                }
            }
            if(ms_Agent == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:CutsceneCustomAgent");
                if(guids.Length>0)
                {
                    ms_Agent = AssetDatabase.LoadAssetAtPath<CutsceneCustomAgent>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
                if(ms_Agent==null)
                {
                    CutsceneCustomAgent customAgents = ScriptableObject.CreateInstance<CutsceneCustomAgent>();
                    string saveFile = EditorUtility.SaveFilePanel("保存自定义行为参数配置", Application.dataPath, "CutsceneCustomAgent", "asset");
                    saveFile = saveFile.Replace("\\", "/");
                    if (!string.IsNullOrEmpty(saveFile) && saveFile.StartsWith(Application.dataPath.Replace("\\", "/")))
                    {
                        saveFile = saveFile.Replace("\\", "/").Replace(Application.dataPath.Replace("\\", "/"), "Assets");
                        if (saveFile.StartsWith("/")) saveFile = saveFile.Substring(1);
                        AssetDatabase.CreateAsset(customAgents, saveFile);
                        AssetDatabase.SaveAssets();
                        ms_Agent = AssetDatabase.LoadAssetAtPath<CutsceneCustomAgent>(saveFile);
                    }
                }
            }
            if(ms_Agent!=null)
            {
                ms_Agent.vEvents = vEvents.ToArray();
                ms_Agent.vClips = vClips.ToArray();
                EditorUtility.SetDirty(ms_Agent);
                AssetDatabase.SaveAssetIfDirty(ms_Agent);
            }
            AgentTreeUtil.Init(true);
        }
        //-----------------------------------------------------
        public static void CommitGit()
        {
            if (ms_Agent == null) return;
            var file = AssetDatabase.GetAssetPath(ms_Agent);
            if (string.IsNullOrEmpty(file))
                return;
            Framework.ED.EditorUtils.CommitGit(file, bWait:false);
        }
        //-----------------------------------------------------
        public static void AddEvent(CutsceneCustomAgent.AgentUnit unit)
        {
            Init();
            if (ms_vEvents.ContainsKey(unit.customType) || HasEvent(unit.name))
            {
                Debug.LogError("Invalid or duplicate custom event unit.");
                return;
            }
            ms_vEvents.Add(unit.customType, unit);
            ms_vEventLists.Add(unit);
            ms_vEventPops.Add(unit.name + "[" + unit.customType + "]");
            if (ms_Agent != null)
            {
                ms_Agent.vEvents = ms_vEventLists.ToArray();
                EditorUtility.SetDirty(ms_Agent);
                AssetDatabase.SaveAssetIfDirty(ms_Agent);
            }
        }
        //-----------------------------------------------------
        public static void RemoveEvent(uint customType)
        {
            Init();
            if (ms_vEvents.Remove(customType))
            {
                bool bFound = false;
                for (int i = 0; i < ms_vEventLists.Count; ++i)
                {
                    if (ms_vEventLists[i].customType == customType)
                    {
                        ms_vEventLists.RemoveAt(i);
                        ms_vEventPops.RemoveAt(i);
                        bFound = true;
                        break;
                    }
                }
                if (bFound && ms_Agent != null)
                {
                    ms_Agent.vEvents = ms_vEventLists.ToArray();
                    EditorUtility.SetDirty(ms_Agent);
                    AssetDatabase.SaveAssetIfDirty(ms_Agent);
                }
            }
        }
        //-----------------------------------------------------
        public static bool HasEvent(uint customType)
        {
            Init();
            return ms_vEvents.ContainsKey(customType);
        }
        //-----------------------------------------------------
        public static bool HasEvent(string name)
        {
            Init();
            foreach (var item in ms_vEvents.Values)
            {
                if (item.name == name)
                    return true;
            }
            return false;
        }
        //-----------------------------------------------------
        public static void AddClip(CutsceneCustomAgent.AgentUnit unit)
        {
            Init();
            if (ms_vClips.ContainsKey(unit.customType) || HasClip(unit.name))
            {
                Debug.LogError("Invalid or duplicate custom clip unit.");
                return;
            }
            ms_vClips.Add(unit.customType, unit);
            ms_vClipLists.Add(unit);
            ms_vClipPops.Add(unit.name + "[" + unit.customType + "]");
            if (ms_Agent != null)
            { 
                ms_Agent.vClips = ms_vClipLists.ToArray();
                EditorUtility.SetDirty(ms_Agent);
                AssetDatabase.SaveAssetIfDirty(ms_Agent);
            }
        }
        //-----------------------------------------------------
        public static void RemoveClip(uint customType)
        {
            Init();
            if (ms_vClips.Remove(customType))
            {
                bool bFound = false;
                for (int i = 0; i < ms_vClipLists.Count; ++i)
                {
                    if (ms_vClipLists[i].customType == customType)
                    {
                        ms_vClipLists.RemoveAt(i);
                        ms_vClipPops.RemoveAt(i);
                        bFound = true;
                        break;
                    }
                }
                if (bFound && ms_Agent != null)
                {
                    ms_Agent.vClips = ms_vClipLists.ToArray();
                    EditorUtility.SetDirty(ms_Agent);
                    AssetDatabase.SaveAssetIfDirty(ms_Agent);
                }
            }
        }
        //-----------------------------------------------------
        public static bool HasClip(uint customType)
        {
            Init();
            return ms_vClips.ContainsKey(customType);
        }
        //-----------------------------------------------------
        public static bool HasClip(string name)
        {
            Init();
            foreach (var item in ms_vClips.Values)
            {
                if (item.name == name)
                    return true;
            }
            return false;
        }
        //-----------------------------------------------------
        public static CutsceneCustomAgent.AgentUnit GetEvent(uint customType)
        {
            Init();
            if (ms_vEvents.TryGetValue(customType, out CutsceneCustomAgent.AgentUnit unit))
            {
                return unit;
            }
            return default;
        }
        //-----------------------------------------------------
        public static List<CutsceneCustomAgent.AgentUnit> GetEventList()
        {
            Init();
            return ms_vEventLists;
        }
        //-----------------------------------------------------
        public static List<string> GetEventPopList()
        {
            Init();
            return ms_vEventPops;
        }
        //-----------------------------------------------------
        public static CutsceneCustomAgent.AgentUnit GetClip(uint customType)
        {
            Init();
            if (ms_vClips.TryGetValue(customType, out CutsceneCustomAgent.AgentUnit unit))
            {
                return unit;
            }
            return default;
        }
        //-----------------------------------------------------
        public static List<CutsceneCustomAgent.AgentUnit> GetClipList()
        {
            Init();
            return ms_vClipLists;
        }
        //-----------------------------------------------------
        public static List<string> GetClipPopList()
        {
            Init();
            return ms_vClipPops;
        }
    }
}

#endif