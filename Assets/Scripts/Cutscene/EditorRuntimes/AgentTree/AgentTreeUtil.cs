/********************************************************************
生成日期:	06:30:2025
类    名: 	AgentTreeUtil
作    者:	HappLI
描    述:	行为树工具类
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using Framework.Cutscene.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework.AT.Editor
{
    public class AgentTreeAttri
    {
        public string displayName;
        public string tips;
        public string strQueueName;
        public string strMenuName;
        public int actionType;
        public bool isCutsceneCustomEvent = false;
        public int cutsceneCusomtType = 0;
        public ATActionAttribute actionAttr;
        public ATIconAttribute iconAttr;
        public List<ArgvAttribute> argvs = new List<ArgvAttribute>();
        public List<ArgvAttribute> returns = new List<ArgvAttribute>();
        public Dictionary<ArgvAttribute, LinkAttribute> linkAttributes = new Dictionary<ArgvAttribute, LinkAttribute>();

        public List<string> popArgvs = new List<string>();
        public List<string> popReturns = new List<string>();

        public System.Type graphNodeType;
    }
    public static class AgentTreeUtil
    {
        private static List<EVariableType> ms_vPopEnumTypes = new List<EVariableType>();
        private static List<string> ms_vPopEnumTypeNames = new List<string>();
        private static Dictionary<long, AgentTreeAttri> ms_Attrs = null;
        private static List<AgentTreeAttri> ms_vLists = new List<AgentTreeAttri>();
        private static List<string> ms_vPops = new List<string>();
        static string ms_installPath = null;
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
        public static Texture2D LoadIcon(string icon)
        {
            Texture2D tex = null;
            string path = AgentTreeUtil.BuildInstallPath();
            if (!string.IsNullOrEmpty(path))
            {
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(path, icon + ".png"));
            }
            if (tex == null)
            {
                tex = EditorGUIUtility.LoadRequired(icon) as Texture2D;
            }
            return tex;
        }
        //-----------------------------------------------------
        internal static void Init(bool bForce = false)
        {
            if (bForce || ms_Attrs == null)
            {
                ms_vPopEnumTypeNames.Clear();
                ms_vPopEnumTypes.Clear();
                foreach (Enum v in Enum.GetValues(typeof(EVariableType)))
                {
                    string strName = Enum.GetName(typeof(EVariableType), v);
                    FieldInfo fi = typeof(EVariableType).GetField(strName);
                    if (fi.IsDefined(typeof(Framework.DrawProps.DisableAttribute)))
                    {
                        continue;
                    }
                    if (fi.IsDefined(typeof(Framework.DrawProps.DisplayAttribute)))
                    {
                        strName = fi.GetCustomAttribute<Framework.DrawProps.DisplayAttribute>().displayName;
                    }
                    if (fi.IsDefined(typeof(InspectorNameAttribute)))
                    {
                        strName = fi.GetCustomAttribute<InspectorNameAttribute>().displayName;
                    }
                    ms_vPopEnumTypes.Add((EVariableType)v);
                    ms_vPopEnumTypeNames.Add(strName);
                }
                ms_Attrs = new Dictionary<long, AgentTreeAttri>();
                ms_vLists = new List<AgentTreeAttri>();
                ms_vPops = new List<string>();

                Dictionary<long, System.Type> vGraphNodeTypes = new Dictionary<long, Type>();
                foreach (var ass in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types = ass.GetTypes();
                    for (int i = 0; i < types.Length; ++i)
                    {
                        Type tp = types[i];
                        if (tp.IsDefined(typeof(NodeBindAttribute), false))
                        {
                            NodeBindAttribute[] atTypeAttrs = (NodeBindAttribute[])tp.GetCustomAttributes<NodeBindAttribute>();
                            for (int j = 0; j < atTypeAttrs.Length; ++j)
                            {
                                long key = ((long)atTypeAttrs[j].actionType) << 32 | (long)atTypeAttrs[j].customType;
                                vGraphNodeTypes[key] = tp;
                            }
                        }
                    }
                }
                foreach (var ass in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types = ass.GetTypes();
                    for (int i = 0; i < types.Length; ++i)
                    {
                        Type tp = types[i];
                        if (!tp.IsEnum)
                            continue;

                        if (tp.IsDefined(typeof(ATTypeAttribute), false))
                        {
                            ATTypeAttribute atTypeAttr = tp.GetCustomAttribute<ATTypeAttribute>();
                            foreach (Enum v in Enum.GetValues(tp))
                            {
                                string strName = Enum.GetName(tp, v);
                                FieldInfo fi = tp.GetField(strName);
                                if (fi.IsDefined(typeof(Framework.DrawProps.DisableAttribute)))
                                {
                                    continue;
                                }
                                if (!fi.IsDefined(typeof(ATActionAttribute)))
                                {
                                    continue;
                                }
                                var actionAttr = fi.GetCustomAttribute<ATActionAttribute>();
                                    int flagValue = Convert.ToInt32(v);
 
                                    strName = actionAttr.name;
                                AgentTreeAttri attr = new AgentTreeAttri();
                                attr.cutsceneCusomtType = 0;
                                attr.isCutsceneCustomEvent = false;
                                attr.actionAttr = actionAttr;
                                attr.iconAttr = fi.GetCustomAttribute<ATIconAttribute>();
                                attr.actionType = flagValue;
                                attr.displayName = strName;
                                attr.strQueueName = strName + v.ToString();
                                if (!string.IsNullOrEmpty(atTypeAttr.name))
                                {
                                    attr.strMenuName = atTypeAttr.name + "/" + attr.displayName;
                                    attr.strQueueName += "/" + atTypeAttr.name;
                                }
                                else
                                    attr.strMenuName = strName;
                                if (attr.actionAttr.isTask)
                                {
                                    attr.actionAttr.hasInput = false;
                                }
                                if (fi.IsDefined(typeof(ArgvAttribute), false))
                                {
                                    ArgvAttribute[] argvs = (ArgvAttribute[])fi.GetCustomAttributes<ArgvAttribute>();
                                    if (argvs != null)
                                    {
                                        if (attr.actionAttr.isTask)
                                            attr.returns.AddRange(argvs);
                                        else
                                            attr.argvs.AddRange(argvs);
                                    }
                                }
                                if (fi.IsDefined(typeof(ReturnAttribute), false))
                                {
                                    ReturnAttribute[] returns = (ReturnAttribute[])fi.GetCustomAttributes<ReturnAttribute>();
                                    if (returns != null)
                                    {
                                        for(int j =0; j < returns.Length; ++j)
                                        {
                                            attr.returns.Add(new ArgvAttribute(returns[j].name, returns[j].argvType));
                                        }
                                    }
                                }
                                if (fi.IsDefined(typeof(LinkAttribute), false))
                                {
                                    LinkAttribute[] returns = (LinkAttribute[])fi.GetCustomAttributes<LinkAttribute>();
                                    if (returns != null)
                                    {
                                        for (int j = 0; j < returns.Length; ++j)
                                        {
                                            ArgvAttribute arvgs = new ArgvAttribute(returns[j].name, typeof(int), false);
                                            arvgs.tips = returns[j].tips;
                                            attr.linkAttributes[arvgs] = returns[j];
                                            attr.returns.Add(arvgs);
                                        }
                                    }
                                }
                                long key = ((long)attr.actionType) << 32 | (long)attr.cutsceneCusomtType;
                                if (vGraphNodeTypes.TryGetValue(key, out var graphNodeType))
                                {
                                    attr.graphNodeType = graphNodeType;
                                }
                                else
                                {
                                    if(attr.actionAttr.isTask)
                                        attr.graphNodeType = typeof(TaskGraphNode);
                                    else
                                        attr.graphNodeType = typeof(GraphNode);
                                }
                                if (ms_Attrs.TryGetValue(key, out var attrD))
                                {
                                    Debug.LogError(tp.Name + " 存在重复定义:" + tp.Name + "." + v.ToString() + "=" + flagValue);
                                }
                                else
                                {
                                    foreach(var db in attr.argvs)
                                    {
                                        attr.popArgvs.Add(db.name);
                                    }
                                    foreach (var db in attr.returns)
                                    {
                                        attr.popReturns.Add(db.name);
                                    }
                                    ms_Attrs.Add(key, attr);
                                    ms_vLists.Add(attr);
                                    ms_vPops.Add(attr.displayName);
                                }
                            }
                        }
                    }
                }

                //! 添加custom data 自定义的事件
                var eventLists = CustomAgentUtil.GetEventList();
                foreach ( var db in eventLists)
                {
                    AgentTreeAttri attr = new AgentTreeAttri();
                    attr.isCutsceneCustomEvent = true;
                    attr.cutsceneCusomtType = (int)db.customType;
                    attr.actionAttr = new ATActionAttribute(db.name, false);
                    attr.actionType = (int)EActionType.eCutsceneCustomEvent;
                    attr.displayName = db.name;
                    attr.strMenuName = "自定义/" + db.name;
                    attr.strQueueName = db.name;
                    if (db.inputVariables!=null)
                    {
                        for(int j =0; j < db.inputVariables.Length; ++j)
                        {
                            var input = db.inputVariables[j];
                            attr.argvs.Add(new ArgvAttribute(input.name, VariableUtil.GetVariableCsType(input.type), input.canEdit, input.defaultValue));
                        }
                    }
                    if (db.outputVariables != null)
                    {
                        for (int j = 0; j < db.outputVariables.Length; ++j)
                        {
                            var input = db.outputVariables[j];
                            attr.returns.Add(new ArgvAttribute(input.name, VariableUtil.GetVariableCsType(input.type), input.canEdit, input.defaultValue));
                        }
                    }
                    long key = ((long)attr.actionType) << 32 | (long)attr.cutsceneCusomtType;
                    if (ms_Attrs.TryGetValue(key, out var attrD))
                    {
                        Debug.LogError(db.name + " 存在重复定义:" + attr.cutsceneCusomtType);
                    }
                    else
                    {
                        foreach (var temp in attr.argvs)
                        {
                            attr.popArgvs.Add(temp.name);
                        }
                        foreach (var temp in attr.returns)
                        {
                            attr.popReturns.Add(temp.name);
                        }
                        ms_Attrs.Add(key, attr);
                        ms_vLists.Add(attr);
                        ms_vPops.Add(attr.displayName);
                    }
                }
            }
        }
        //-----------------------------------------------------
        public static List<string> GetPops()
        {
            Init();
            return ms_vPops;
        }
        //-----------------------------------------------------
        public static List<AgentTreeAttri> GetAttrs()
        {
            Init();
            return ms_vLists;
        }
        //-----------------------------------------------------
        public static AgentTreeAttri GetAttri(int type, int customType)
        {
            Init();
            long key = ((long)type) << 32 | (long)customType;
            if (ms_Attrs.TryGetValue(key, out var tempAttr))
                return tempAttr;
            return null;
        }
        //-----------------------------------------------------
        public static List<EVariableType> GetPopEnumTypes()
        {
            return ms_vPopEnumTypes;
        }
        //-----------------------------------------------------
        public static List<string> GetPopEnumTypeNames()
        {
            return ms_vPopEnumTypeNames;
        }
    }
}

#endif