/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneAttributes
作    者:	HappLI
描    述:	过场动画属性
*********************************************************************/
using System;
using UnityEngine;
namespace Framework.Cutscene.Runtime
{
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CutsceneRuntimeAttribute : System.Attribute
    {
        public CutsceneRuntimeAttribute()
        {
        }
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CutsceneObjectActionProviderAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string method;
#endif
        public CutsceneObjectActionProviderAttribute(string method)
        {
#if UNITY_EDITOR
            this.method = method;
#endif
        }
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CutsceneEditorLoaderAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string method;
#endif
        public CutsceneEditorLoaderAttribute(string method)
        {
#if UNITY_EDITOR
            this.method = method;
#endif
        }
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CutsceneCustomEditorAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public System.Type type;
#endif
        public CutsceneCustomEditorAttribute(System.Type type)
        {
#if UNITY_EDITOR
            this.type = type;
#endif
        }
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Struct, AllowMultiple =true, Inherited = false)]
    public class CutsceneCustomDriverAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public EDataType dataType;
        public ushort typeId;
        public uint customType;
#endif
        public CutsceneCustomDriverAttribute(EDataType dataType, ushort typeId = 0, uint customType = 0)
        {
#if UNITY_EDITOR
            this.dataType = dataType;
            this.typeId = typeId;
            this.customType = customType;
#endif
        }
    }
    //-----------------------------------------------------
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class CutscenePresetDefaultAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public EDataType dataType;
        public ushort typeId;
        public uint customType;
        public string method;
#endif
        public CutscenePresetDefaultAttribute(string method,EDataType dataType, ushort typeId = 0, uint customType = 0)
        {
#if UNITY_EDITOR
            this.method = method;
            this.dataType = dataType;
            this.typeId = typeId;
            this.customType = customType;
#endif
        }
    }
    //-----------------------------------------------------
    public class CutsceneClipAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string name;
        public Color color;
        public string tips;
        public System.Type inputType; //输入类型
#endif
        public CutsceneClipAttribute(string name, System.Type inputType = null)
        {
#if UNITY_EDITOR
            this.name = name;
            this.color = Color.gray;
            this.inputType = inputType;
            this.tips = "";
#endif
        }
        public CutsceneClipAttribute(string name, string tips, System.Type inputType = null)
        {
#if UNITY_EDITOR
            this.name = name;
            this.color = Color.gray;
            this.inputType = inputType;
            this.tips = tips;
#endif
        }
        public CutsceneClipAttribute(string name, string tips, Color color, System.Type inputType = null)
        {
#if UNITY_EDITOR
            this.name = name;
            this.tips = tips;
            this.color = color;
            this.inputType = inputType;
#endif
        }
    }
    //-----------------------------------------------------
    public class CutsceneEditorAttribute : System.Attribute
    {
    }
    //-----------------------------------------------------
    public class CutsceneEventAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string name;
        public Color color;
        public string tips;
        public System.Type inputType; //输入类型
#endif
        public CutsceneEventAttribute(string name, System.Type inputType = null)
        {
#if UNITY_EDITOR
            this.name = name;
            this.color = Color.gray;
            this.inputType = inputType;
            this.tips = "";
#endif
        }
        public CutsceneEventAttribute(string name, string tips, System.Type inputType = null)
        {
#if UNITY_EDITOR
            this.name = name;
            this.color = Color.gray;
            this.inputType = inputType;
            this.tips = tips;
#endif
        }
        public CutsceneEventAttribute(string name, string tips, Color color, System.Type inputType = null)
        {
#if UNITY_EDITOR
            this.name = name;
            this.tips = tips;
            this.color = color;
            this.inputType = inputType;
#endif
        }
    }
    //-----------------------------------------------------
    public enum ECutsceneClipBlendType
    {
        In = 0,
        Out = 1,
    }
    public class CutsceneBlendAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public ECutsceneClipBlendType eType;
#endif
        public CutsceneBlendAttribute(ECutsceneClipBlendType eType)
        {
#if UNITY_EDITOR
            this.eType = eType;
#endif
        }
    }
}