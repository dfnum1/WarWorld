/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneCustomClip
作    者:	HappLI
描    述:	自定义剪辑类型
*********************************************************************/
using Framework.AT.Runtime;
using Framework.DrawProps;
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("自定义Clip")]
    public class CutsceneCustomClip : IBaseClip, ICustomSerialize
    {
        [Display("基本属性")]public BaseClipProp    baseProp;

        [Display("剪辑类型"),UnEdit] public uint    customType;
        [Display("输入："), 
        ParamPortMapField("inputVariables"),
        SerializeField] 
        internal Variables                          inputVariables;
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return null;//走回调
        }
        //-----------------------------------------------------
        public float GetDuration()
        {
            return baseProp.duration;
        }
        //-----------------------------------------------------
        public EClipEdgeType GetEndEdgeType()
        {
            return baseProp.endEdgeType;
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eCustom;
        }
        //-----------------------------------------------------
        public string GetName()
        {
            return baseProp.name;
        }
        //-----------------------------------------------------
        public ushort GetRepeatCount()
        {
            return baseProp.repeatCnt;
        }
        //-----------------------------------------------------
        public float GetTime()
        {
            return baseProp.time;
        }
        //-----------------------------------------------------
        public float GetBlend(bool bIn)
        {
            return baseProp.GetBlend(bIn);
        }
        //-----------------------------------------------------
        public bool OnDeserialize(string content)
        {
            var temp = JsonUtility.FromJson<CutsceneCustomClip>(content);
            this.baseProp = temp.baseProp;
            this.customType = temp.customType;
            this.inputVariables = temp.inputVariables;
            inputVariables.Deserialize();
            return true;
        }
        //-----------------------------------------------------
        public string OnSerialize()
        {
#if UNITY_EDITOR
            this.inputVariables.Serialize();
            return JsonUtility.ToJson(this);
#else
            return null;
#endif
        }
        //-----------------------------------------------------
        public int GetArgvCount()
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
#if UNITY_EDITOR
        //-----------------------------------------------------
        internal void InitCustomAgent(CutsceneCustomAgent.AgentUnit pAgent)
        {
            baseProp.name = pAgent.name;
            customType = pAgent.customType;
            inputVariables.variables = null;
            if(pAgent.inputVariables!=null && pAgent.inputVariables.Length>0)
            {
                inputVariables.variables = new VariableList();
                inputVariables.FillCustomAgentParam(pAgent.inputVariables);
            }
        }
#endif
    }
}