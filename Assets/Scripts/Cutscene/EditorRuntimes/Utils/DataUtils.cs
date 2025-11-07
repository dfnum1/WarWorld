/********************************************************************
生成日期:	06:30:2025
类    名: 	DataUtils
作    者:	HappLI
描    述:	数据工具类
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
using Framework.Cutscene.Runtime;
using Framework.DrawProps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Application;

namespace Framework.Cutscene.Editor
{
    public class ClipAttriData
    {
        public int typeId;
        public Type type;
        public CutsceneClipAttribute pAttri;
        public IBaseClip CreateClip()
        {
            var dater = Activator.CreateInstance(type) as IBaseClip;
            dater = DataUtils.SetDefaultValue(dater);
            if (string.IsNullOrEmpty(dater.GetName()))
                dater.SetName(pAttri.name);
            return dater;
        }
        public Color drawColor
        {
            get
            {
                return pAttri.color;
            }
        }
    }
    public class EventAttriData
    {
        public int typeId;
        public Type type;
        public CutsceneEventAttribute pAttri;
        public IBaseEvent CreateEvent()
        {
            var dater = Activator.CreateInstance(type) as IBaseEvent;
            dater = DataUtils.SetDefaultValue(dater);
            if (string.IsNullOrEmpty(dater.GetName()))
                dater.SetName(pAttri.name);
            return dater;
        }
        public Color drawColor
        {
            get
            {
                return pAttri.color;
            }
        }
    }
    public class DriverAttriData
    {
        public long key;
        public Type type;
        public CutsceneCustomDriverAttribute pAttri;
    }
    public static class DataUtils
    {
        private static Dictionary<int, ClipAttriData> ms_ClipAttrs = null;
        private static List<ClipAttriData> ms_vClipsLists = new List<ClipAttriData>();
        private static List<string> ms_vClipsPops = new List<string>();

        private static Dictionary<int, EventAttriData> ms_EventAttrs = null;
        private static List<EventAttriData> ms_vEventsLists = new List<EventAttriData>();
        private static List<string> ms_vEventsPops = new List<string>();

        private static List<FieldInfo> ms_vTempFields = new List<FieldInfo>();

        private static System.Reflection.MethodInfo ms_pLoadUnityPlugin = null;
        private static Dictionary<long, DriverAttriData> ms_vCustomDriverTypes = new Dictionary<long, DriverAttriData>();
        private static Dictionary<System.Type, System.Type> ms_vCustomEventTypes = new Dictionary<Type, Type>();
        private static Dictionary<System.Type, System.Reflection.MethodInfo> ms_vSceneViewMethods = new Dictionary<System.Type, System.Reflection.MethodInfo>();
        private static System.Type ms_pCutsceneRuntimeType = null;
        private static Dictionary<long, System.Reflection.MethodInfo> ms_vDefaultValueFunctions = new Dictionary<long, MethodInfo>();
        //-----------------------------------------------------
        static void Init()
        {
            if (ms_ClipAttrs == null || ms_EventAttrs == null)
            {
                ms_ClipAttrs = new Dictionary<int, ClipAttriData>();
                ms_EventAttrs = new Dictionary<int, EventAttriData>();
                ms_vSceneViewMethods.Clear();
                ms_vClipsLists.Clear();
                ms_vClipsPops.Clear();
                ms_vEventsLists.Clear();
                ms_vEventsPops.Clear();
                ms_vCustomDriverTypes.Clear();
                ms_vCustomEventTypes.Clear();
                ms_vDefaultValueFunctions.Clear();
                foreach (var ass in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types = ass.GetTypes();
                    for (int i = 0; i < types.Length; ++i)
                    {
                        Type tp = types[i];
                        if (tp.IsDefined(typeof(CutsceneRuntimeAttribute), false) && tp.GetInterface(typeof(ICutsceneCallback).FullName.Replace("+","."))!=null)
                        {
                            ms_pCutsceneRuntimeType = tp;
                        }
                        if (tp.IsDefined(typeof(CutsceneCustomEditorAttribute), false) && tp.IsSubclassOf(typeof(ACutsceneCustomEditor)))
                        {
                            var clipAttri = tp.GetCustomAttributes<CutsceneCustomEditorAttribute>();
                            foreach(var evt in clipAttri)
                            {
                                ms_vCustomEventTypes[evt.type] = tp;
                            }
                        }
                        if (tp.IsDefined(typeof(CutsceneEditorLoaderAttribute), false))
                        {
                            var clipAttri = tp.GetCustomAttribute<CutsceneEditorLoaderAttribute>();
                            ms_pLoadUnityPlugin = tp.GetMethod(clipAttri.method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                        }
                        if (tp.IsDefined(typeof(CutsceneClipAttribute), false))
                        {
                            var clipAttri = tp.GetCustomAttribute<CutsceneClipAttribute>();
                            IDataer instType = Activator.CreateInstance(tp) as IDataer;
                            int typeId = instType.GetIdType();
                            if (ms_ClipAttrs.TryGetValue(typeId, out var attriD))
                            {
                                Debug.LogError(tp.Name + " 存在重复定义:" + typeId + "   ->" + attriD.type.Name);
                            }
                            else
                            {
                                ClipAttriData data = new ClipAttriData();
                                data.type = tp;
                                data.typeId = typeId;
                                data.pAttri = clipAttri;
                                ms_ClipAttrs.Add(typeId, data);
                                ms_vClipsLists.Add(data);
                                ms_vClipsPops.Add(clipAttri.name);
                            }

                            var methods = tp.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
                            if (methods!=null)
                            {
                                for(int j = 0; j < methods.Length; ++j)
                                {
                                    if (methods[j].GetParameters().Length == 1 && methods[j].GetParameters()[0].ParameterType == typeof(UnityEditor.SceneView))
                                    {
                                        ms_vSceneViewMethods[tp] = methods[j];
                                    }
                                }
                            }
                        }
                        else if (tp.IsDefined(typeof(CutsceneEventAttribute), false))
                        {
                            var clipAttri = tp.GetCustomAttribute<CutsceneEventAttribute>();
                            IDataer instType = Activator.CreateInstance(tp) as IDataer;
                            int typeId = instType.GetIdType();
                            if (ms_EventAttrs.TryGetValue(typeId, out var attriD))
                            {
                                Debug.LogError(tp.Name + " 存在重复定义:" + typeId + "   ->" + attriD.type.Name);
                            }
                            else
                            {
                                EventAttriData data = new EventAttriData(); 
                                data.type = tp;
                                data.typeId = typeId;
                                data.pAttri = clipAttri;
                                ms_EventAttrs.Add(typeId, data);
                                ms_vEventsLists.Add(data);
                                ms_vEventsPops.Add(clipAttri.name);
                            }
                            var methods = tp.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
                            if (methods != null)
                            {
                                for (int j = 0; j < methods.Length; ++j)
                                {
                                    if (methods[j].GetParameters().Length == 1 && methods[j].GetParameters()[0].ParameterType == typeof(UnityEditor.SceneView))
                                    {
                                        ms_vSceneViewMethods[tp] = methods[j];
                                    }
                                }
                            }
                        }
                        else if(tp.IsDefined(typeof(CutsceneCustomDriverAttribute), false))
                        {
                            var driverAttris = (CutsceneCustomDriverAttribute[])tp.GetCustomAttributes<CutsceneCustomDriverAttribute>();
                            if(tp.IsSubclassOf(typeof(ACutsceneDriver)))
                            {
                                for(int j =0; j < driverAttris.Length; ++j)
                                {
                                    long key = CutscenePool.GetDaterKey(driverAttris[j].dataType, driverAttris[j].typeId, driverAttris[j].customType);
                                    DriverAttriData driverAttriData = new DriverAttriData();
                                    driverAttriData.key = key;
                                    driverAttriData.type = tp;
                                    driverAttriData.pAttri = driverAttris[j];

                                    ms_vCustomDriverTypes[key] = driverAttriData;
                                }
                            }
                            var methods = tp.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
                            if (methods != null)
                            {
                                for (int j = 0; j < methods.Length; ++j)
                                {
                                    if (methods[j].GetParameters().Length == 1 && methods[j].GetParameters()[0].ParameterType == typeof(UnityEditor.SceneView))
                                    {
                                        ms_vSceneViewMethods[tp] = methods[j];
                                    }
                                }
                            }
                        }
                        if(tp.IsDefined(typeof(CutscenePresetDefaultAttribute), false))
                        {
                            var defautlAttrs = (CutscenePresetDefaultAttribute[])tp.GetCustomAttributes<CutscenePresetDefaultAttribute>();
                            for(int j =0; j < defautlAttrs.Length; ++j)
                            {
                                var method = tp.GetMethod(defautlAttrs[j].method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
                                if (method == null) continue;
                                var vParams = method.GetParameters();
                                if (vParams == null || vParams.Length != 1)
                                    continue;
                                if (vParams.Length <= 0) continue;
                                if(defautlAttrs[j].dataType == EDataType.eClip)
                                {
                                    if (vParams[0].ParameterType.GetInterface(typeof(IBaseClip).FullName.Replace("+", ".")) == null)
                                        continue;
                                }
                                else if (defautlAttrs[j].dataType == EDataType.eEvent)
                                {
                                    if (vParams[0].ParameterType.GetInterface(typeof(IBaseEvent).FullName.Replace("+", ".")) == null)
                                        continue;
                                }
                                if (vParams[0].ParameterType != method.ReturnType)
                                    continue;

                                long key = CutscenePool.GetDaterKey(defautlAttrs[j].dataType, defautlAttrs[j].typeId, defautlAttrs[j].customType);
                                ms_vDefaultValueFunctions[key] = method;
                            }
                        }
                    }
                }
            }
        }
        //-----------------------------------------------------
        public static UnityEngine.Object EditLoadUnityObject(string file)
        {
            if(ms_pLoadUnityPlugin != null)
            {
                var returnObj = ms_pLoadUnityPlugin.Invoke(null, new object[] { file });
                if (returnObj != null && returnObj is UnityEngine.Object)
                    return returnObj as UnityEngine.Object;
            }
           return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file);
        }
        //-----------------------------------------------------
        public static System.Type GetCustomDriver(EDataType dataType, int typeId, uint customType =0)
        {
            long key = CutscenePool.GetDaterKey(dataType, typeId, customType);
            if (ms_vCustomDriverTypes.TryGetValue(key, out var driverType))
                return driverType.type;
            return null;
        }
        //-----------------------------------------------------
        public static void GeneratorCode(string strPath)
        {
            Init();
            string dataName = typeof(IDataer).FullName.Replace("+", ".");
            string dataTypeName = typeof(EDataType).FullName.Replace("+", ".");
            string driverTypeName = typeof(ACutsceneDriver).FullName.Replace("+", ".");
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("//auto generator");
            sb.AppendLine("namespace Framework.Cutscene.Runtime");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic class CutsceneUtil");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tpublic static "+ dataName + " CreateDataer("+ dataTypeName + " type, ushort typeId)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t    switch(type)");
            sb.AppendLine("\t\t    {");
            sb.AppendLine("\t\t        case "+ dataTypeName + ".eClip:");
            sb.AppendLine("\t\t            switch(typeId)");
            sb.AppendLine("\t\t            {");
            foreach (var db in ms_ClipAttrs)
            {
                var typeId = db.Key;
                var type = db.Value.type;
                sb.AppendLine($"\t\t                case {typeId}: return new {type.FullName.Replace("+",".")}();");
            }
            sb.AppendLine("\t\t                default: return null;");
            sb.AppendLine("\t\t            }");
            sb.AppendLine("\t\t        case "+ dataTypeName + ".eEvent:");
            sb.AppendLine("\t\t            switch(typeId)");
            sb.AppendLine("\t\t            {");
            foreach (var db in ms_EventAttrs)
            {
                var typeId = db.Key;
                var type = db.Value.type;
                sb.AppendLine($"\t\t                case {typeId}: return new {type.FullName.Replace("+",".")}();");
            }
            sb.AppendLine("\t\t                default: return null;");
            sb.AppendLine("\t\t            }");
            sb.AppendLine("\t\t        default: return null;");
            sb.AppendLine("\t\t    }");
            sb.AppendLine("\t\t}");

            sb.AppendLine("\t\t//-----------------------------------------------------");
            sb.AppendLine("\t\tpublic static "+ driverTypeName + " CreateDriver(long key)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t    switch(key)");
            sb.AppendLine("\t\t    {");
            foreach (var db in ms_vCustomDriverTypes)
            {
                var key = db.Key;
                var type = db.Value.type;
                var attr = db.Value.pAttri;
                sb.AppendLine($"\t\t        case {key}: // {dataTypeName}.{attr.dataType} typeid={attr.typeId} customtype={attr.customType}");
                sb.AppendLine($"\t\t            return new {type.FullName.Replace("+", ".")}();");
            }
            sb.AppendLine("\t\t        default: return null;");
            sb.AppendLine("\t\t    }");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            if (!Directory.Exists(Path.GetDirectoryName(strPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(strPath));
            FileStream fs = new FileStream(strPath, FileMode.OpenOrCreate);
            StreamWriter writer = new StreamWriter(fs, System.Text.Encoding.UTF8);
            fs.Position = 0;
            fs.SetLength(0);
            writer.Write(sb.ToString());
            writer.Close();
        }
        //-----------------------------------------------------
        public static ACutsceneCustomEditor CreateCustomEvent(System.Type type)
        {
            Init();
            if(ms_vCustomEventTypes.TryGetValue(type, out var eventType))
            {
                ACutsceneCustomEditor pEditor =(ACutsceneCustomEditor)Activator.CreateInstance(eventType);
                return pEditor;
            }
            return null;
        }
        //-----------------------------------------------------
        public static List<string> GetPopClips()
        {
            Init();
            return ms_vClipsPops;
        }
        //-----------------------------------------------------
        public static List<ClipAttriData> GetClipAttrs()
        {
            Init();
            return ms_vClipsLists;
        }
        //-----------------------------------------------------
        public static ClipAttriData GetClipAttri(int type)
        {
            Init();
            if (ms_ClipAttrs.TryGetValue(type, out var tempAttr))
                return tempAttr;
            return null;
        }
        //-----------------------------------------------------
        public static List<string> GetPopEvents()
        {
            Init();
            return ms_vEventsPops;
        }
        //-----------------------------------------------------
        public static List<EventAttriData> GetEventAttrs()
        {
            Init();
            return ms_vEventsLists;
        }
        //-----------------------------------------------------
        public static EventAttriData GetEventAttri(int type)
        {
            Init();
            if (ms_EventAttrs.TryGetValue(type, out var tempAttr))
                return tempAttr;
            return null;
        }
        //-----------------------------------------------------
        public static Dictionary<int, EventAttriData> GetEvents()
        {
            Init();
            return ms_EventAttrs;
        }
        //-----------------------------------------------------
        public static Color GetDrawColor(IDataer dater)
        {
            if(dater is IBaseClip)
            {
                var attri = GetClipAttri(dater.GetIdType());
                if (attri != null) return attri.drawColor;
            }
            else if (dater is IBaseEvent)
            {
                var attri = GetEventAttri(dater.GetIdType());
                if (attri != null) return attri.drawColor;
            }
            return Color.gray;
        }
        //-----------------------------------------------------
        static List<FieldInfo> GetDaterFields(this IDataer dater, Type fieldType)
        {
            ms_vTempFields.Clear();
             var fields = dater.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            for (int i = 0; i < fields.Length; ++i)
            {
                if (fields[i].FieldType == fieldType)
                {
                    ms_vTempFields.Add(fields[i]);
                    break;
                }
            }
            return ms_vTempFields;
        }
        //-----------------------------------------------------
        static FieldInfo GetDaterField(this IDataer dater, Type fieldType)
        {
            var fields = dater.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            for (int i = 0; i < fields.Length; ++i)
            {
                if (fields[i].FieldType == fieldType)
                {
                    return fields[i];
                }
            }
            return null;
        }
        //-----------------------------------------------------
        public static float GetDuration(this IDataer dater)
        {
            var fields = dater.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (dater is IBaseClip)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseClipProp));
                if (basePropField != null)
                {
                    BaseClipProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        propData = new BaseClipProp();
                    }
                    else propData = (BaseClipProp)propObj;

                    return propData.duration;
                }
            }
            else
                return 0.5f;
            return 0.0f;
        }
        //-----------------------------------------------------
        public static bool CanBlend(this IDataer dater)
        {
            if (dater is IBaseClip)
            {
                return true;
                //var fields = dater.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                //if (fields != null)
                //{
                //    for (int i = 0; i < fields.Length; ++i)
                //    {
                //        if (fields[i].IsDefined(typeof(CutsceneBlendAttribute)) && fields[i].FieldType == typeof(float))
                //        {
                //            return true;
                //        }
                //    }
                //}
            }
            return false;
        }
        //-----------------------------------------------------
        public static float GetBlendDuration(this IDataer dater, ECutsceneClipBlendType type )
        {
            if (dater is IBaseClip)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseClipProp));
                if (basePropField != null)
                {
                    BaseClipProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        propData = new BaseClipProp();
                    }
                    else propData = (BaseClipProp)propObj;

                    if (type == ECutsceneClipBlendType.In)
                    {
                        return propData.GetBlend(true);
                    }
                    else if (type == ECutsceneClipBlendType.Out)
                    {
                        return propData.GetBlend(false);
                    }
                    return 0;
                }
                //var fields = dater.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                //if (fields != null)
                //{
                //   for(int i =0; i < fields.Length; ++i)
                //    {
                //        if(fields[i].IsDefined(typeof(CutsceneBlendAttribute)) && fields[i].FieldType == typeof(float))
                //        {
                //            var blendAttr = fields[i].GetCustomAttribute<CutsceneBlendAttribute>();
                //            if(blendAttr.eType == type)
                //            {
                //                return (float)fields[i].GetValue(dater);
                //            }
                //        }
                //    }
                //}
            }
            return 0.0f;
        }
        //-----------------------------------------------------
        public static bool SetBlendDuration(this IDataer dater, ECutsceneClipBlendType type, float duration, bool bApplay = true)
        {
            if (dater is IBaseClip)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseClipProp));
                if (basePropField != null)
                {
                    BaseClipProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        propData = new BaseClipProp();
                    }
                    else propData = (BaseClipProp)propObj;

                    bool bDirty = false;
                    if (type == ECutsceneClipBlendType.In)
                    {
                        if (propData.GetBlend(true) != duration)
                        {
                            if(bApplay) propData.blendIn = duration;
                            bDirty = true;
                        }
                    }
                    else if (type == ECutsceneClipBlendType.Out)
                    {
                        if (propData.GetBlend(false) != duration)
                        {
                            if (bApplay) propData.blendOut = duration;
                            bDirty = true;
                        }
                    }
                    if (bApplay) basePropField.SetValue(dater, propData);
                    return bDirty;
                }
                //var fields = dater.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                //if (fields != null)
                //{
                //    for (int i = 0; i < fields.Length; ++i)
                //    {
                //        if (fields[i].IsDefined(typeof(CutsceneBlendAttribute)) && fields[i].FieldType == typeof(float))
                //        {
                //            var blendAttr = fields[i].GetCustomAttribute<CutsceneBlendAttribute>();
                //            if (blendAttr.eType == type)
                //            {
                //                float val = (float)fields[i].GetValue(dater);
                //                if(val != duration)
                //                {
                //                    if(bApplay) fields[i].SetValue(dater, duration);
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
            }
            return false;
        }
        //-----------------------------------------------------
        public static int GetLoopCnt(this IDataer dater)
        {
            if (dater is IBaseClip)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseClipProp));
                if (basePropField == null) return 1;
                var loopVal = basePropField.GetValue(dater);
                if (loopVal == null) return 1;
                BaseClipProp prop = (BaseClipProp)loopVal;
                return prop.repeatCnt;
            }
            return 1;
        }
        //-----------------------------------------------------
        public static bool SetDuration(this IDataer dater, float duration)
        {
            bool bDirty = false;
            if (dater is IBaseClip)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseClipProp));
                if (basePropField != null)
                {
                    BaseClipProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        propData = new BaseClipProp();
                    }
                    else propData = (BaseClipProp)propObj;

                    if (propData.duration != duration)
                    {
                        propData.duration = duration;
                        bDirty = true;
                    }

                    basePropField.SetValue(dater, propData);
                }
            }
            return bDirty;
        }
        //-----------------------------------------------------
        public static float ToLocalTimeUnbound(this IDataer clip, float time)
        {
            return 0;
            // return (time - clip.GetStartTime()) * timeScale + clipIn;
        }
        //-----------------------------------------------------
        public static bool SetOwnerObject(this IDataer dater, System.Object pObject, CutsceneTrack pSubObject)
        {
            bool bDirty = false;
            if (dater is IBaseClip)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseClipProp));
                if (basePropField != null)
                {
                    BaseClipProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        propData = new BaseClipProp();
                    }
                    else propData = (BaseClipProp)propObj;
                    propData.ownerObject = pObject;
                    propData.ownerTrackObject = pSubObject;
                    basePropField.SetValue(dater, propData);
                }
            }
            else if (dater is IBaseEvent)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseEventProp));
                if (basePropField != null)
                {
                    BaseEventProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        propData = new BaseEventProp();
                    }
                    else propData = (BaseEventProp)propObj;

                    propData.ownerObject = pObject;
                    propData.ownerTrackObject = pSubObject;
                    basePropField.SetValue(dater, propData);
                }
            }
            return bDirty;
        }
        //-----------------------------------------------------
        public static bool SetName(this IDataer dater, string name)
        {
            bool bDirty = false;
            if (dater is IBaseClip)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseClipProp));
                if (basePropField != null)
                {
                    BaseClipProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        propData = new BaseClipProp();
                    }
                    else propData = (BaseClipProp)propObj;

                    if (propData.name != name)
                    {
                        propData.name = name;
                        bDirty = true;
                    }

                    basePropField.SetValue(dater, propData);
                }
            }
            else if (dater is IBaseEvent)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseEventProp));
                if (basePropField != null)
                {
                    BaseEventProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        propData = new BaseEventProp();
                    }
                    else propData = (BaseEventProp)propObj;

                    if (propData.name != name)
                    {
                        propData.name = name;
                        bDirty = true;
                    }

                    basePropField.SetValue(dater, propData);
                }
            }
            return bDirty;
        }
        //-----------------------------------------------------
        public static bool IsDefaultName(this IDataer dater)
        {
            if (dater is IBaseClip)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseClipProp));
                if (basePropField != null)
                {
                    BaseClipProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        return true;
                    }
                    var clipData= GetClipAttri(dater.GetIdType());
                    if (clipData == null)
                        return true;
                    propData = (BaseClipProp)propObj;
                    if (propData.name != clipData.pAttri.name)
                    {
                        return false;
                    }
                }
            }
            else if (dater is IBaseEvent)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseEventProp));
                if (basePropField != null)
                {
                    BaseEventProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        return true;
                    }
                    var clipData = GetEventAttri(dater.GetIdType());
                    if (clipData == null)
                        return true;
                    propData = (BaseEventProp)propObj;
                    if (propData.name != clipData.pAttri.name)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        //-----------------------------------------------------
        public static string Serialize(this IDataer dater)
        {
            if (dater is ICustomSerialize)
                return ((ICustomSerialize)dater).OnSerialize();
            else
                return JsonUtility.ToJson(dater);
        }
        //-----------------------------------------------------
        public static void Deserialize(this IDataer dater, string jsonContent)
        {
            if (dater is ICustomSerialize)
            {
                ((ICustomSerialize)dater).OnDeserialize(jsonContent);
            }
            else
                JsonUtility.FromJsonOverwrite(jsonContent, dater);
        }
        //-----------------------------------------------------
        public static bool SetTime(this IDataer dater, float time)
        {
            bool bDirty = false;
            var fields = dater.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if(dater is IBaseClip)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseClipProp));
                if(basePropField!=null)
                {
                    BaseClipProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        propData = new BaseClipProp();
                    }
                    else propData = (BaseClipProp)propObj;

                    if(propData.time != time)
                    {
                        propData.time = time;
                        bDirty = true;
                    }

                    basePropField.SetValue(dater, propData);
                }
            }
            else if (dater is IBaseEvent)
            {
                FieldInfo basePropField = dater.GetDaterField(typeof(BaseEventProp));
                if (basePropField != null)
                {
                    BaseEventProp propData;
                    var propObj = basePropField.GetValue(dater);
                    if (propObj == null)
                    {
                        propData = new BaseEventProp();
                    }
                    else propData = (BaseEventProp)propObj;

                    if (propData.time != time)
                    {
                        propData.time = time;
                        bDirty = true;
                    }

                    basePropField.SetValue(dater, propData);
                }
            }
            return bDirty;
        }
        //-----------------------------------------------------
        static ACutsceneLogic ms_pCurrentLogic = null;
        internal static void OnSceneView(this IDataer dataer, UnityEditor.SceneView sceneView,ACutsceneLogic logic)
        {
            if (dataer == null) return;
            Init();
            if(ms_vSceneViewMethods.TryGetValue(dataer.GetType(), out var method))
            {
                ms_pCurrentLogic = logic;
                method.Invoke(dataer, new object[] { sceneView });
                ms_pCurrentLogic = null;
            }
        }
        //-----------------------------------------------------
        internal static void SetCurrentLogic(ACutsceneLogic logic)
        {
            ms_pCurrentLogic = logic;
        }
        //-----------------------------------------------------
        public static void RegisterUndo(this IDataer data)
        {
            if (data == null || ms_pCurrentLogic == null)
                return;
            ms_pCurrentLogic.RegisterUndoData();
        }
        //-----------------------------------------------------
        public static T SetDefaultValue<T>(T dater) where T : IDataer
        {
            dater = (T)SetDefaultValueRecursive(dater, new HashSet<object>());
            return dater;
        }
        //-----------------------------------------------------
        private static object SetDefaultValueRecursive(object obj, HashSet<object> visited)
        {
            if (obj == null || visited.Contains(obj))
                return obj;
            visited.Add(obj);

            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                // 跳过只读字段
                if (field.IsInitOnly) continue;

                var defValueAttr = field.GetCustomAttribute<DefaultValueAttribute>();
                if (defValueAttr != null)
                {
                    var fieldType = field.FieldType;

                    try
                    {
                        object value = null;
                        if (fieldType.IsEnum)
                        {
                            value = Enum.Parse(fieldType, defValueAttr.strValue);
                        }
                        else if (fieldType == typeof(string))
                        {
                            value = defValueAttr.strValue;
                        }
                        else if (fieldType == typeof(bool))
                        {
                            value = defValueAttr.ToValue<bool>();
                        }
                        else if (fieldType == typeof(byte))
                        {
                            value = defValueAttr.ToValue<byte>();
                        }
                        else if (fieldType == typeof(short))
                        {
                            value = defValueAttr.ToValue<short>();
                        }
                        else if (fieldType == typeof(ushort))
                        {
                            value = defValueAttr.ToValue<ushort>();
                        }
                        else if (fieldType == typeof(int))
                        {
                            value = defValueAttr.ToValue<int>();
                        }
                        else if (fieldType == typeof(uint))
                        {
                            value = defValueAttr.ToValue<uint>();
                        }
                        else if (fieldType == typeof(long))
                        {
                            value = defValueAttr.ToValue<long>();
                        }
                        else if (fieldType == typeof(ulong))
                        {
                            value = defValueAttr.ToValue<ulong>();
                        }
                        else if (fieldType == typeof(float))
                        {
                            value = defValueAttr.ToValue<float>();
                        }
                        else if (fieldType == typeof(double))
                        {
                            value = defValueAttr.ToValue<double>();
                        }
                        else if (fieldType == typeof(decimal))
                        {
                            value = defValueAttr.ToValue<decimal>();
                        }
                        else if (fieldType.IsValueType || fieldType.IsClass)
                        {
                            // 结构体或类，递归赋值
                            object nestedObj = field.GetValue(obj);
                            if (nestedObj == null)
                            {
                                nestedObj = Activator.CreateInstance(fieldType);
                            }
                            nestedObj = SetDefaultValueRecursive(nestedObj, visited);
                            field.SetValue(obj, nestedObj);
                            continue;
                        }
                        else
                        {
                            // 其它类型尝试通用转换
                            value = Convert.ChangeType(defValueAttr.strValue, fieldType);
                        }

                        field.SetValue(obj, value);
                    }
                    catch
                    {
                        // 忽略异常，防止单个字段出错影响整体
                    }
                }
                else
                {
                    // 没有DefaultValueAttribute，但如果是自定义类/结构体，也递归
                    var fieldType = field.FieldType;
                    if ((fieldType.IsClass || (fieldType.IsValueType && !fieldType.IsPrimitive && !fieldType.IsEnum))
                        && fieldType != typeof(string))
                    {
                        if (field.IsDefined(typeof(System.NonSerializedAttribute)))
                            continue;
                        object nestedObj = field.GetValue(obj);
                        if (nestedObj == null && !fieldType.IsGenericType && !fieldType.IsArray)
                        {
                            nestedObj = Activator.CreateInstance(fieldType);
                        }
                        if (nestedObj != null)
                        {
                            nestedObj = SetDefaultValueRecursive(nestedObj, visited);
                            field.SetValue(obj, nestedObj);
                        }
                    }
                }
            }

            var refObj = SetPresetDefaultValue(obj);
            if (refObj != null) obj = refObj;
            return obj;
        }
        //-----------------------------------------------------
        public static object SetPresetDefaultValue(object pObj)
        {
            if (pObj == null) return pObj;
            var type = pObj.GetType();
            var method = type.GetMethod("SetDefault", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null) method.Invoke(pObj, null);
            else if (pObj != null && pObj.GetType().GetInterface(typeof(IDataer).FullName.Replace("+", ".")) != null)
            {
                IDataer pDater = (IDataer)pObj;
                if (pDater != null)
                {
                    long key = 0;
                    if(pDater is IBaseClip)
                    {
                        key = CutscenePool.GetDaterKey(EDataType.eClip, pDater);
                    }
                    else if(pDater is IBaseEvent)
                    {
                        key = CutscenePool.GetDaterKey(EDataType.eEvent, pDater);
                    }
                    if (ms_vDefaultValueFunctions.TryGetValue(key, out var callMethod))
                    {
                        pObj = callMethod.Invoke(null, new[] { pObj });
                    }
                }
            }
            return pObj;
        }
        //-----------------------------------------------------
        public static CutsceneManager GetRuntimeCutsceneManger()
        {
            if (ms_pCutsceneRuntimeType == null) return null;
            var fields = ms_pCutsceneRuntimeType.GetFields(BindingFlags.Public| BindingFlags.NonPublic| BindingFlags.Static| BindingFlags.Instance);
            if (fields == null) return null;
            object instancePt = null;
            for (int i =0; i < fields.Length; ++i)
            {
                if(fields[i].FieldType == ms_pCutsceneRuntimeType && fields[i].IsStatic)
                {
                    instancePt = fields[i].GetValue(null);
                    break;
                }
            }
            if (instancePt == null)
                return null;

            for (int i = 0; i < fields.Length; ++i)
            {
                if (fields[i].FieldType == typeof(CutsceneManager))
                {
                    if(fields[i].IsStatic)
                        return (CutsceneManager)fields[i].GetValue(null);
                    else
                    {
                        return (CutsceneManager)fields[i].GetValue(instancePt);
                    }
                }
            }
            return null;
        }
    }
}

[UnityEditor.InitializeOnLoad]
public static class DataUtilsAutoGenerator
{
    static DataUtilsAutoGenerator()
    {
        // 你可以自定义生成文件路径
        string path = EditorPreferences.GetSettings().generatorCodePath;
        if (string.IsNullOrEmpty(path))
            return;
        string outputPath = Path.Combine(path,"Generated/CreateDataerDelegate.gen.cs");
        Framework.Cutscene.Editor.DataUtils.GeneratorCode(outputPath);
    }
}

#endif