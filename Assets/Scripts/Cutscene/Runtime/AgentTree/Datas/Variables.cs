/********************************************************************
生成日期:	06:30:2025
类    名: 	Variables
作    者:	HappLI
描    述:	变量
*********************************************************************/
using Framework.Cutscene.Runtime;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Framework.AT.Runtime
{
    [System.Serializable]
    public  struct ObjId
    {
        public byte userType;
        public int id;
        public ObjId(int id, byte userType =0)
        {
            this.userType = userType;
            this.id = id;
        }
        public static ObjId DEF => new ObjId { id = 0, userType =0 };
    }
    public enum EVariableType : byte
    {
        [DrawProps.Disable]eNone = 0,
        [InspectorName("整数")]eInt = 1,
        [InspectorName("浮点数")] eFloat = 2,
        [InspectorName("字符串")] eString = 3,
        [InspectorName("Bool")] eBool = 4,
        [InspectorName("二维向量")] eVec2 = 5,
        [InspectorName("三维向量")] eVec3 = 6,
        [InspectorName("四维向量")] eVec4= 7,
        [InspectorName("ObjId")] eObjId = 8,
    }
    //-----------------------------------------------------
    public interface IVariable
    {
        EVariableType GetVariableType();
        short GetGuid();
    }
    //-----------------------------------------------------
    [System.Serializable]
    public struct VariableInt : IVariable
    {
        public short guid;
        public int value;
        //-----------------------------------------------------
        public VariableInt(int value)
        {
            guid = 0;
            this.value = value;
        }
        //-----------------------------------------------------
        public EVariableType GetVariableType()
        {
            return EVariableType.eInt;
        }
        //-----------------------------------------------------
        public short GetGuid() { return guid; }
    }
    //-----------------------------------------------------
    [System.Serializable]
    public struct VariableFloat : IVariable
    {
        public short guid;
        public float value;
        //-----------------------------------------------------
        public VariableFloat(float value)
        {
            guid = 0;
            this.value = value;
        }
        //-----------------------------------------------------
        public EVariableType GetVariableType()
        {
            return EVariableType.eFloat;
        }
        //-----------------------------------------------------
        public short GetGuid() { return guid; }
    }
    //-----------------------------------------------------
    [System.Serializable]
    public struct VariableVec2 : IVariable
    {
        public short guid;
        public Vector2 value;
        //-----------------------------------------------------
        public VariableVec2(Vector2 value)
        {
            guid = 0;
            this.value = value;
        }
        //-----------------------------------------------------
        public VariableVec2(float value1, float value2)
        {
            guid = 0;
            this.value = new Vector2(value1, value2);
        }
        //-----------------------------------------------------
        public EVariableType GetVariableType()
        {
            return EVariableType.eVec2;
        }
        //-----------------------------------------------------
        public short GetGuid() { return guid; }
    }
    //-----------------------------------------------------
    [System.Serializable]
    public struct VariableVec3 : IVariable
    {
        public short guid;
        public Vector3 value;
        //-----------------------------------------------------
        public VariableVec3(Vector3 value)
        {
            guid = 0;
            this.value = value;
        }
        //-----------------------------------------------------
        public VariableVec3(float value1, float value2, float value3)
        {
            guid = 0;
            this.value = new Vector3(value1, value2, value3);
        }
        //-----------------------------------------------------
        public EVariableType GetVariableType()
        {
            return EVariableType.eVec3;
        }
        //-----------------------------------------------------
        public short GetGuid() { return guid; }
    }
    //-----------------------------------------------------
    [System.Serializable]
    public struct VariableVec4 : IVariable
    {
        public short guid;
        public Vector4 value;
        //-----------------------------------------------------
        public VariableVec4(Vector4 value)
        {
            guid = 0;
            this.value = value;
        }
        //-----------------------------------------------------
        public VariableVec4(float value1, float value2, float value3, float value4)
        {
            guid = 0;
            this.value = new Vector4(value1, value2, value3, value4);
        }
        //-----------------------------------------------------
        public EVariableType GetVariableType()
        {
            return EVariableType.eVec4;
        }
        //-----------------------------------------------------
        public short GetGuid() { return guid; }
    }
    //-----------------------------------------------------
    [System.Serializable]
    public struct VariableString : IVariable
    {
        public short guid;
        public string value;
        //-----------------------------------------------------
        public VariableString(string value)
        {
            guid = 0;
            this.value = value;
        }
        //-----------------------------------------------------
        public EVariableType GetVariableType()
        {
            return EVariableType.eString;
        }
        //-----------------------------------------------------
        public short GetGuid() { return guid; }
    }
    //-----------------------------------------------------
    [System.Serializable]
    public struct VariableBool : IVariable
    {
        public short guid;
        public bool value;
        //-----------------------------------------------------
        public VariableBool(bool value)
        {
            guid = 0;
            this.value = value;
        }
        //-----------------------------------------------------
        public EVariableType GetVariableType()
        {
            return EVariableType.eBool;
        }
        //-----------------------------------------------------
        public short GetGuid() { return guid; }
    }
    //-----------------------------------------------------
    [System.Serializable]
    public struct VariableObjId : IVariable
    {
        public short guid;
        public ObjId value;
        //-----------------------------------------------------
        public VariableObjId(ObjId value)
        {
            guid = 0;
            this.value = value;
        }
        //-----------------------------------------------------
        public EVariableType GetVariableType()
        {
            return EVariableType.eObjId;
        }
        //-----------------------------------------------------
        public short GetGuid() { return guid; }
    }
    //-----------------------------------------------------
    [System.Serializable]
    struct VaribaleSerizlizeData
    {
        [System.Serializable]
        public struct Item
        {
            public byte type;
            public string value;
        }
        public Item[] items;
    }
    //-----------------------------------------------------
    [System.Serializable]
    public class Variables
    {
        [UnityEngine.SerializeField]string serializeData;
        [System.NonSerialized]public VariableList variables;
        //-----------------------------------------------------
        public int GetVarCount()
        {
            if (variables == null) return 0;
            return variables.GetVarCount();
        }
        //-----------------------------------------------------
        public EVariableType GetVarType(int index)
        {
            if (variables == null) return EVariableType.eNone;
            return variables.GetVarType(index);
        }
        //-----------------------------------------------------
        public bool GetBool(int index, bool defVal =false)
        {
            if (variables == null) return defVal;
            return variables.GetBool(index, defVal);
        }
        //-----------------------------------------------------
        public int GetInt(int index, int defVal = 0)
        {
            if (variables == null) return defVal;
            return variables.GetInt(index, defVal);
        }
        //-----------------------------------------------------
        public float GetFloat(int index, float defVal = 0)
        {
            if (variables == null) return defVal;
            return variables.GetFloat(index, defVal);
        }
        //-----------------------------------------------------
        public ObjId GetObjId(int index)
        {
            if (variables == null) return ObjId.DEF;
            return variables.GetObjId(index);
        }
        //-----------------------------------------------------
        public Vector2 GetVec2(int index)
        {
            if (variables == null) return Vector2.zero;
            return variables.GetVec2(index);
        }
        //-----------------------------------------------------
        public Vector3 GetVec3(int index)
        {
            if (variables == null) return Vector3.zero;
            return variables.GetVec3(index);
        }
        //-----------------------------------------------------
        public Vector4 GetVec4(int index)
        {
            if (variables == null) return Vector4.zero;
            return variables.GetVec4(index);
        }
        //-----------------------------------------------------
        public string GetString(int index, string defValue = null)
        {
            if (variables == null) return defValue;
            return variables.GetString(index, defValue);
        }
        //-----------------------------------------------------
        public bool Deserialize()
        {
            if (string.IsNullOrEmpty(serializeData))
                return false;
            VaribaleSerizlizeData data = UnityEngine.JsonUtility.FromJson<VaribaleSerizlizeData>(serializeData);
            if (data.items != null)
            {
                variables = VariableList.Malloc(data.items.Length);
                for (int i = 0; i < data.items.Length; ++i)
                {
                    var item = data.items[i];
                    switch ((EVariableType)item.type)
                    {
                        case EVariableType.eInt:
                            {
                                int v = 0;
                                int.TryParse(item.value, out v);
                                variables.AddInt(v);
                            }
                            break;
                        case EVariableType.eFloat:
                            {
                                float v = 0;
                                float.TryParse(item.value, out v);
                                variables.AddFloat(v);
                            }
                            break;
                        case EVariableType.eString:
                            {
                                variables.AddString(item.value);
                            }
                            break;
                        case EVariableType.eBool:
                            {
                                bool v = false;
                                bool.TryParse(item.value, out v);
                                variables.AddBool(v);
                            }
                            break;
                        case EVariableType.eVec2:
                            {
                                var splitVal = item.value.Split('|');
                                if(splitVal.Length >=2)
                                {
                                    float x = 0, y = 0;
                                    float.TryParse(splitVal[0], out x);
                                    float.TryParse(splitVal[1], out y);
                                    variables.AddVec2(new Vector2(x, y));
                                }
                            }
                            break;
                        case EVariableType.eVec3:
                            {
                                var splitVal = item.value.Split('|');
                                if (splitVal.Length >= 3)
                                {
                                    float x = 0, y = 0, z =0;
                                    float.TryParse(splitVal[0], out x);
                                    float.TryParse(splitVal[1], out y);
                                    float.TryParse(splitVal[2], out z);
                                    variables.AddVec3(new Vector3(x, y, z));
                                }
                            }
                            break;
                            case EVariableType.eVec4:
                            {
                                var splitVal = item.value.Split('|');
                                if (splitVal.Length >= 4)
                                {
                                    float x = 0, y = 0, z = 0, w = 0;
                                    float.TryParse(splitVal[0], out x);
                                    float.TryParse(splitVal[1], out y);
                                    float.TryParse(splitVal[2], out z);
                                    float.TryParse(splitVal[3], out w);
                                    variables.AddVec4(new Vector4(x, y, z,w));
                                }
                            }
                            break;
                        case EVariableType.eObjId:
                            {
                                int v = 0;
                                int.TryParse(item.value, out v);
                                ObjId obj = new ObjId();
                                JsonUtility.FromJsonOverwrite(item.value, obj);
                                variables.AddObjId(obj);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
                variables = null;
                return true;
        }
        //-----------------------------------------------------
        public void Serialize()
        {
#if UNITY_EDITOR
            VaribaleSerizlizeData data = new VaribaleSerizlizeData();
            if (variables != null)
            {
                data.items = new VaribaleSerizlizeData.Item[variables.GetVarCount()];
                for (int i = 0; i < data.items.Length; ++i)
                {
                    var type = variables.GetVarType(i);
                    var item = new VaribaleSerizlizeData.Item();
                    item.type = (byte)type;
                    switch (type)
                    {
                        case EVariableType.eInt:
                            {
                                item.value = variables.GetInt(i).ToString();
                            }
                            break;
                        case EVariableType.eFloat:
                            {
                                item.value = variables.GetFloat(i).ToString("F3");
                            }
                            break;
                        case EVariableType.eString:
                            {
                                item.value = variables.GetString(i);
                            }
                            break;
                        case EVariableType.eBool:
                            {
                                item.value = variables.GetBool(i).ToString();
                            }
                            break;
                        case EVariableType.eVec2:
                            {
                                var vv2 = variables.GetVec2(i);
                                item.value = $"{vv2.x.ToString("F3")}|{vv2.y.ToString("F3")}";
                            }
                            break;
                        case EVariableType.eVec3:
                            {
                                var vv3 = variables.GetVec3(i);
                                item.value = $"{vv3.x.ToString("F3")}|{vv3.y.ToString("F3")}|{vv3.z.ToString("F3")}";
                            }
                            break;
                        case EVariableType.eVec4:
                            {
                                var vv4 = variables.GetVec4(i);
                                item.value = $"{vv4.x.ToString("F3")}|{vv4.y.ToString("F3")}|{vv4.z.ToString("F3")}|{vv4.w.ToString("F3")}";
                            }
                            break;
                        case EVariableType.eObjId:
                            {
                                var vv4 = variables.GetObjId(i);
                                item.value = JsonUtility.ToJson(vv4);
                            }
                            break;
                    }
                    data.items[i] = item;
                }
            }
            else
            {
                data.items = new VaribaleSerizlizeData.Item[0];
            }
            serializeData = UnityEngine.JsonUtility.ToJson(data);
#else
            Debug.LogError("un support out editor");
#endif
        }
        //-----------------------------------------------------
        internal void FillCustomAgentParam(CutsceneCustomAgent.AgentUnit.ParamData[] paramValues)
        {
            if (paramValues.Length > 0)
            {
                variables = VariableList.Malloc(paramValues.Length);
                for (int j = 0; j < paramValues.Length; ++j)
                {
                    var param = paramValues[j];
                    switch (param.type)
                    {
                        case EVariableType.eInt:
                            {
                                int v = 0;
                                int.TryParse(param.defaultValue, out v);
                                variables.AddInt(v);
                            }
                            break;
                        case EVariableType.eFloat:
                            {
                                float v = 0;
                                float.TryParse(param.defaultValue, out v);
                                variables.AddFloat(v);
                            }
                            break;
                        case EVariableType.eBool:
                            {
                                bool v = false;
                                bool.TryParse(param.defaultValue, out v);
                                variables.AddBool(v);
                            }
                            break;
                        case EVariableType.eString:
                            {
                                variables.AddString(param.defaultValue ?? string.Empty);
                            }
                            break;
                        case EVariableType.eVec2:
                            {
                                Vector2 v = Vector2.zero;
                                var split = (param.defaultValue ?? "").Split('|');
                                if (split.Length >= 2)
                                {
                                    float.TryParse(split[0], out v.x);
                                    float.TryParse(split[1], out v.y);
                                }
                                variables.AddVec2(v);
                            }
                            break;
                        case EVariableType.eVec3:
                            {
                                Vector3 v = Vector3.zero;
                                var split = (param.defaultValue ?? "").Split('|');
                                if (split.Length >= 3)
                                {
                                    float.TryParse(split[0], out v.x);
                                    float.TryParse(split[1], out v.y);
                                    float.TryParse(split[2], out v.z);
                                }
                                variables.AddVec3(v);
                            }
                            break;
                        case EVariableType.eVec4:
                            {
                                Vector4 v = Vector4.zero;
                                var split = (param.defaultValue ?? "").Split('|');
                                if (split.Length >= 4)
                                {
                                    float.TryParse(split[0], out v.x);
                                    float.TryParse(split[1], out v.y);
                                    float.TryParse(split[2], out v.z);
                                    float.TryParse(split[3], out v.w);
                                }
                                variables.AddVec4(v);
                            }
                            break;
                        case EVariableType.eObjId:
                            {
                                ObjId obj = new ObjId();
                                int.TryParse(param.defaultValue, out obj.id);
                                variables.AddObjId(obj);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
    //-----------------------------------------------------
    public static class VariableUtil
    {
        public static IVariable CreateVariable(EVariableType type, short guid)
        {
            switch (type)
            {
                case EVariableType.eInt:
                    return new VariableInt { value = 0, guid = guid };
                case EVariableType.eFloat:
                    return new VariableFloat {  value = 0f, guid = guid };
                case EVariableType.eString:
                    return new VariableString { value = string.Empty, guid = guid };
                case EVariableType.eBool:
                    return new VariableBool {  value = false, guid = guid };
                case EVariableType.eVec2:
                    return new VariableVec2 { value = Vector2.zero, guid = guid };
                case EVariableType.eVec3:
                    return new VariableVec3 { value = Vector3.zero, guid = guid };
                case EVariableType.eVec4:
                    return new VariableVec4 { value = Vector4.zero, guid = guid };
                case EVariableType.eObjId:
                    return new VariableObjId { value = new ObjId(), guid = guid };
                default:
                    return null;
            }
        }
        //-----------------------------------------------------
        public static IVariable CreateVariable<T>(T value, short guid = 0)
        {
            if (value is byte byVal)
                return new VariableInt { value = byVal, guid = guid };
            if (value is short sVal)
                return new VariableInt { value = sVal, guid = guid };
            if (value is ushort usVal)
                return new VariableInt { value = usVal, guid = guid };
            if (value is int intVal)
                return new VariableInt { value = intVal, guid = guid };
            if (value is int uintVal)
                return new VariableInt { value = uintVal, guid = guid };
            if (value is float floatVal)
                return new VariableFloat { value = floatVal, guid = guid };
            if (value is string strVal)
                return new VariableString { value = strVal, guid = guid };
            if (value is bool boolVal)
                return new VariableBool { value = boolVal, guid = guid };
            if (value is Vector2 v2Val)
                return new VariableVec2 { value = v2Val, guid = guid };
            if (value is Vector3 v3Val)
                return new VariableVec3 { value = v3Val, guid = guid };
            if (value is Vector4 v4Val)
                return new VariableVec4 { value = v4Val, guid = guid };
            if (value is ObjId objId)
                return new VariableObjId { value = objId, guid = guid };
            return null;
        }
        //-----------------------------------------------------
#if UNITY_EDITOR
        public static Type GetVariableCsType(EVariableType type)
        {
            if (type == EVariableType.eBool) return typeof(bool);
            else if (type == EVariableType.eInt) return typeof(int);
            else if (type == EVariableType.eFloat) return typeof(float);
            else if (type == EVariableType.eString) return typeof(string);
            else if (type == EVariableType.eVec2) return typeof(Vector2);
            else if (type == EVariableType.eVec3) return typeof(Vector3);
            else if (type == EVariableType.eVec4) return typeof(Vector4);
            else if (type == EVariableType.eObjId) return typeof(ObjId);
            return null;
        }
        //-----------------------------------------------------
        public static IVariable CreateVariable(ArgvAttribute attri, short guid)
        {
          if (attri.argvType == typeof(bool)) return new VariableBool { value = attri.ToValue<bool>(false), guid = guid };
            else if (attri.argvType == typeof(byte) ||
                attri.argvType.IsEnum ||
                attri.argvType == typeof(short)||
                attri.argvType == typeof(ushort)||
                attri.argvType == typeof(int)||
                attri.argvType == typeof(uint))
            {
                return new VariableInt { value = attri.ToValue<int>(0), guid = guid };
            }
            else if (attri.argvType.IsEnum)
            {
                if(!string.IsNullOrEmpty(attri.strValue))
                {
                    if (Enum.TryParse(attri.argvType, attri.strValue, true, out object enumVal))
                    {
                        return new VariableInt { value = Convert.ToInt32(enumVal), guid= guid };
                    }
                    else
                    {
                        return new VariableInt { value = attri.ToValue<int>(0), guid = guid };
                    }
                }
                else
                    return new VariableInt { value = attri.ToValue<int>(0), guid = guid };
            }
            else if (attri.argvType == typeof(float))
            {
                return new VariableFloat { value = attri.ToValue<float>(0), guid = guid };
            }
            else if (attri.argvType == typeof(IVariable))
            {
                return new VariableInt { value = 0, guid = guid };
            }
            else if (attri.argvType == typeof(BaseNode))
            {
                return new VariableInt { value = 0, guid = guid };
            }
            else if (attri.argvType == typeof(string))
            {
                return new VariableString { value = attri.strValue, guid = guid };
            }
            else if (attri.argvType == typeof(Vector2))
            {
                return new VariableVec2 { value = attri.ToValue<Vector2>(Vector2.zero), guid = guid };
            }
            else if (attri.argvType == typeof(Vector3))
            {
                return new VariableVec3 { value = attri.ToValue<Vector3>(Vector3.zero), guid = guid };
            }
            else if (attri.argvType == typeof(Vector4))
            {
                return new VariableVec4 { value = attri.ToValue<Vector4>(Vector4.zero), guid = guid };
            }
            else if (attri.argvType == typeof(ObjId))
            {
                return new VariableObjId { value = new ObjId(), guid = guid };
            }
            UnityEngine.Debug.LogError($"Unsupported variable type: {attri.argvType}");
            return null;
        }
#endif
    }
}