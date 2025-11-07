/********************************************************************
生成日期:	06:30:2025
类    名: 	Attribute
作    者:	HappLI
描    述:	变量
*********************************************************************/
using System;
using UnityEngine;
namespace Framework.AT.Runtime
{
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, Inherited = false)]
    public class ATIconAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string name;
#endif
        public ATIconAttribute(string name)
        {
#if UNITY_EDITOR
            this.name = name;
#endif
        }
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class ATActionAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string name;
        public string tips;
        public bool isTask; // 是否为任务节点
        public bool hasInput;
        public bool hasOutput;
        public bool bShow;
#endif
        public ATActionAttribute(string name, bool isTask = false, bool hasInput = true, bool hasOutput = true, bool bShow = true)
        {
#if UNITY_EDITOR
            this.name = name;
            this.tips = null;
            this.isTask = isTask;
            this.hasInput = hasInput;
            this.hasOutput = hasOutput;
            this.bShow = bShow;
#endif
        }
        public ATActionAttribute(string name, string tips, bool isTask = false, bool hasInput = true, bool hasOutput = true, bool bShow = true)
        {
#if UNITY_EDITOR
            this.name = name;
            this.tips = tips;
            this.isTask = isTask;
            this.hasInput = hasInput;
            this.hasOutput = hasOutput;
            this.bShow = bShow;
#endif
        }
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class ArgvAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string name;
        public string tips;
        public System.Type argvType;
        public string strValue; // 默认值
        public bool canEdit;
        public EVariableType[] limitVarTypes;
#endif
        //-----------------------------------------------------
        public ArgvAttribute(string name, System.Type argvType, bool canEdit = false, System.Object defVal = null, params EVariableType[] limitTypes)
        {
#if UNITY_EDITOR
            this.name = name;
            this.tips = null;
            this.argvType = argvType;
            this.strValue = defVal != null ? defVal.ToString() : "";
            this.canEdit = canEdit;
            this.limitVarTypes = limitTypes;
#endif
        }
        //-----------------------------------------------------
        public ArgvAttribute(string name, string tips, System.Type argvType, bool canEdit = false, System.Object defVal = null, params EVariableType[] limitTypes)
        {
#if UNITY_EDITOR
            this.name = name;
            this.tips = tips;
            this.argvType = argvType;
            this.strValue = defVal != null ? defVal.ToString() : "";
            this.canEdit = canEdit;
            this.limitVarTypes = limitTypes;
#endif
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        public T ToValue<T>(T defVal = default)
        {
            try
            {
                if (string.IsNullOrEmpty(strValue))
                    return defVal;

                Type targetType = typeof(T);

                // 先特殊处理枚举
                if (targetType.IsEnum)
                {
                    // 支持名称和数字
                    if (Enum.TryParse(targetType, strValue, true, out object enumVal))
                        return (T)enumVal;
                    // 尝试数字转枚举
                    if (int.TryParse(strValue, out int intVal))
                        return (T)Enum.ToObject(targetType, intVal);
                    return defVal;
                }

                // 支持所有常见数值类型
                if (targetType == typeof(byte))
                    return (T)(object)byte.Parse(strValue);
                if (targetType == typeof(bool))
                    return (T)(object)strValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                if (targetType == typeof(short))
                    return (T)(object)short.Parse(strValue);
                if (targetType == typeof(ushort))
                    return (T)(object)ushort.Parse(strValue);
                if (targetType == typeof(int))
                    return (T)(object)int.Parse(strValue);
                if (targetType == typeof(uint))
                    return (T)(object)uint.Parse(strValue);
                if (targetType == typeof(long))
                    return (T)(object)long.Parse(strValue);
                if (targetType == typeof(ulong))
                    return (T)(object)ulong.Parse(strValue);
                if (targetType == typeof(float))
                    return (T)(object)float.Parse(strValue);
                if (targetType == typeof(double))
                    return (T)(object)double.Parse(strValue);
                if (targetType == typeof(decimal))
                    return (T)(object)decimal.Parse(strValue);
                if (targetType == typeof(Vector2))
                {
                    var splitVal = strValue.Split(new char[] { '|', ',' });
                    if (splitVal.Length == 2 && float.TryParse(splitVal[0], out float x) && float.TryParse(splitVal[1], out float y))
                        return (T)(object)(new Vector2(x, y));
                }
                if (targetType == typeof(Vector3))
                {
                    var splitVal = strValue.Split(new char[] { '|', ',' });
                    if (splitVal.Length == 3 && float.TryParse(splitVal[0], out float x) && float.TryParse(splitVal[1], out float y) && float.TryParse(splitVal[2], out float z))
                        return (T)(object)(new Vector3(x, y, z));
                }
                if (targetType == typeof(Vector4))
                {
                    var splitVal = strValue.Split(new char[] { '|', ',' });
                    if (splitVal.Length == 4 && float.TryParse(splitVal[0], out float x) && float.TryParse(splitVal[1], out float y) && float.TryParse(splitVal[2], out float z) && float.TryParse(splitVal[3], out float w))
                        return (T)(object)(new Vector4(x, y, z, w));
                }

                // 其它类型尝试通用转换
                return (T)Convert.ChangeType(strValue, targetType);
            }
            catch
            {
                return defVal;
            }
        }
#endif
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class ReturnAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string name;
        public string tips;
        public System.Type argvType;
#endif
        public ReturnAttribute(string name, System.Type argvType)
        {
#if UNITY_EDITOR
            this.name = name;
            this.tips = null;
            this.argvType = argvType;
#endif
        }
        //-----------------------------------------------------
        public ReturnAttribute(string name, string tips, System.Type argvType)
        {
#if UNITY_EDITOR
            this.name = name;
            this.tips = tips;
            this.argvType = argvType;
#endif
        }
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class LinkAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string name;
        public string tips;
        public bool linkIn;
#endif
        public LinkAttribute(string name, bool linkIn)
        {
#if UNITY_EDITOR
            this.name = name;
            this.tips = null;
            this.linkIn = linkIn;
#endif
        }
        //-----------------------------------------------------
        public LinkAttribute(string name, string tips, bool linkIn)
        {
#if UNITY_EDITOR
            this.name = name;
            this.tips = tips;
            this.linkIn = linkIn;
#endif
        }
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = true, Inherited = false)]
    public class ATTypeAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string name;
#endif
        public ATTypeAttribute(string name)
        {
#if UNITY_EDITOR
            this.name = name;
#endif
        }
    }
}