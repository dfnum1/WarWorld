/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneCustomAgent
作    者:	HappLI
描    述:	自定行为参数数据
*********************************************************************/
using Framework.AT.Runtime;
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    [CreateAssetMenu]
    public class CutsceneCustomAgent : ScriptableObject
    {
        [System.Serializable]
        public class AgentUnit
        {
            [System.Serializable]
            public struct ParamData
            {
                public string name;
                public EVariableType type;
                public string defaultValue; // 默认值
                public string displayType;
                public bool canEdit; // 是否可以编辑
            }
            public string name; // 事件名称
            public string tip;
            public uint customType;
            public ushort version; // 版本号
            public ParamData[] inputVariables; // 输入变量
            public ParamData[] outputVariables; // 输出变量
#if UNITY_EDITOR
         //   [System.NonSerialized]public bool playAble = true;    // 是否可用
#endif

            public static AgentUnit DEF = new AgentUnit() { customType = 0, inputVariables = null, outputVariables = null };
            public bool IsValid()
            {
                return customType > 0 && !string.IsNullOrEmpty(name);
            }
        }

        public AgentUnit[] vEvents;
        public AgentUnit[] vClips;
    }
    //-----------------------------------------------------
    public class ParamPortMapFieldAttribute : System.Attribute
    {
#if UNITY_EDITOR
        public string fieldName;
#endif
        public ParamPortMapFieldAttribute(string fieldName)
        {
#if UNITY_EDITOR
            this.fieldName = fieldName;
#endif
    }
}
}