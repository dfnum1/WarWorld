/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneCustomEvent
作    者:	HappLI
描    述:	自定义事件类型
*********************************************************************/
using Framework.AT.Runtime;
using Framework.DrawProps;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.Linq;
using Framework.Cutscene.Editor;
using UnityEditor;
#endif
namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneEvent("自定义事件")]
    public class CutsceneCustomEvent : IBaseEvent, ICustomSerialize
    {
        [Display("基本属性")]public BaseEventProp baseProp;

        [Display("事件类型"),UnEdit] public uint customType;
        [Display("输入："), ParamPortMapField("inputVariables"),SerializeField] internal Variables inputVariables;
        [Display("输出："), ParamPortMapField("outputVariables"), UnEdit, SerializeField] internal Variables outputVariables;
        [Display("将输出绑定给轨道"),DefaultValue(true)] public bool outputBindTrack;
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return null;//走回调
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EEventType.eCustom;
        }
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
        //-----------------------------------------------------
        public int GetInArgvCount()
        {
            if (inputVariables == null) return 0;
            return inputVariables.GetVarCount();
        }
        //-----------------------------------------------------
        public EVariableType GetVarType(int index)
        {
            if (inputVariables == null) return EVariableType.eNone;
            return inputVariables.GetVarType(index);
        }
        //-----------------------------------------------------
        public bool GetInBool(int index, bool defVal = false)
        {
            if (inputVariables == null) return defVal;
            return inputVariables.GetBool(index, defVal);
        }
        //-----------------------------------------------------
        public int GetInInt(int index, int defVal = 0)
        {
            if (inputVariables == null) return defVal;
            return inputVariables.GetInt(index, defVal);
        }
        //-----------------------------------------------------
        public float GetInFloat(int index, float defVal = 0)
        {
            if (inputVariables == null) return defVal;
            return inputVariables.GetFloat(index, defVal);
        }
        //-----------------------------------------------------
        public ObjId GetInObjId(int index)
        {
            if (inputVariables == null) return ObjId.DEF;
            return inputVariables.GetObjId(index);
        }
        //-----------------------------------------------------
        public Vector2 GetInVec2(int index)
        {
            if (inputVariables == null) return Vector2.zero;
            return inputVariables.GetVec2(index);
        }
        //-----------------------------------------------------
        public Vector3 GetInVec3(int index)
        {
            if (inputVariables == null) return Vector3.zero;
            return inputVariables.GetVec3(index);
        }
        //-----------------------------------------------------
        public Vector4 GetInVec4(int index)
        {
            if (inputVariables == null) return Vector4.zero;
            return inputVariables.GetVec4(index);
        }
        //-----------------------------------------------------
        public string GetInString(int index, string defValue = null)
        {
            if (inputVariables == null) return defValue;
            return inputVariables.GetString(index, defValue);
        }
        //-----------------------------------------------------
        public int GetOutArgvCount()
        {
            if (outputVariables == null) return 0;
            return outputVariables.GetVarCount();
        }
        //-----------------------------------------------------
        public EVariableType GetOutVarType(int index)
        {
            if (outputVariables == null) return EVariableType.eNone;
            return outputVariables.GetVarType(index);
        }
        //-----------------------------------------------------
        public bool SetOutArgv<T>(CutsceneTrack pOwner, int index, T value) where T : struct
        {
            if (pOwner == null || outputVariables == null) return false;
            return pOwner.SetOutputVariable(this, index, value);
        }
        //-----------------------------------------------------
        public bool SetOutArgv(CutsceneTrack pOwner, int index, string value)
        {
            if (pOwner == null || outputVariables == null) return false;
            return pOwner.SetOutputVariable(this, index, value);
        }
        //-----------------------------------------------------
        public bool OnDeserialize(string content)
        {
            var temp = JsonUtility.FromJson<CutsceneCustomEvent>(content);
            this.baseProp = temp.baseProp;
            this.customType = temp.customType;
            this.inputVariables = temp.inputVariables;
            this.outputVariables = temp.outputVariables;
            this.outputBindTrack = temp.outputBindTrack;
            inputVariables.Deserialize();
            outputVariables.Deserialize();
            return true;
        }
        //-----------------------------------------------------
        public string OnSerialize()
        {
#if UNITY_EDITOR
            this.inputVariables.Serialize();
            this.outputVariables.Serialize();
            return JsonUtility.ToJson(this);
#else
            return null;
#endif
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        internal void InitCustomAgent(CutsceneCustomAgent.AgentUnit pAgent)
        {
            baseProp.name = pAgent.name;
            customType = pAgent.customType;
            inputVariables.variables = null;
            outputVariables.variables = null;
            if(pAgent.inputVariables!=null && pAgent.inputVariables.Length>0)
            {
                inputVariables.FillCustomAgentParam(pAgent.inputVariables);
            }
            if (pAgent.outputVariables != null && pAgent.outputVariables.Length > 0)
            {
                outputVariables.FillCustomAgentParam(pAgent.outputVariables);
            }
        }
#endif
    }
}