/********************************************************************
生成日期:	06:30:2025
类    名: 	VariableKV
作    者:	HappLI
描    述:	变量Key-Value存储类
*********************************************************************/
using UnityEngine;

namespace Framework.AT.Runtime
{
    internal class VariableKV
    {
        System.Collections.Generic.Dictionary<short, bool>      m_vBools = null;
        System.Collections.Generic.Dictionary<short, int>       m_vInts = null;
        System.Collections.Generic.Dictionary<short, float>     m_vFloats = null;
        System.Collections.Generic.Dictionary<short, string>    m_vStrings = null;
        System.Collections.Generic.Dictionary<short, Vector2>   m_vVec2s = null;
        System.Collections.Generic.Dictionary<short, Vector3>   m_vVec3s = null;
        System.Collections.Generic.Dictionary<short, Vector4>   m_vVec4s = null;
        System.Collections.Generic.Dictionary<short, ObjId>     m_vObjIds = null;
        //-----------------------------------------------------
        public void Clear()
        {
            m_vBools?.Clear();
            m_vInts?.Clear();
            m_vFloats?.Clear();
            m_vStrings?.Clear();
            m_vVec2s?.Clear();
            m_vVec3s?.Clear();
            m_vVec4s?.Clear();
            m_vObjIds?.Clear();
        }
        //-----------------------------------------------------
        public void SetBool(short key, bool value)
        {
            if (m_vBools == null) m_vBools = new System.Collections.Generic.Dictionary<short, bool>(2);
            m_vBools[key] = value;
        }
        //-----------------------------------------------------
        public bool GetBool(short key, bool defaultValue = false)
        {
            if (m_vBools != null && m_vBools.TryGetValue(key, out var val))
                return val;
            return defaultValue;
        }
        //-----------------------------------------------------
        public bool GetBool(short key, out bool value)
        {
            if (m_vBools != null && m_vBools.TryGetValue(key, out value))
                return true;
            value = false;
            return false;
        }
        //-----------------------------------------------------
        public void SetInt(short key, int value)
        {
            if (m_vInts == null) m_vInts = new System.Collections.Generic.Dictionary<short, int>(2);
            m_vInts[key] = value;
        }
        //-----------------------------------------------------
        public int GetInt(short key, int defaultValue = 0)
        {
            if (m_vInts != null && m_vInts.TryGetValue(key, out var val))
                return val;
            return defaultValue;
        }
        //-----------------------------------------------------
        public bool GetInt(short key, out int value)
        {
            if (m_vInts != null && m_vInts.TryGetValue(key, out value))
                return true;
            value = 0;
            return false;
        }
        //-----------------------------------------------------
        public void SetFloat(short key, float value)
        {
            if (m_vFloats == null) m_vFloats = new System.Collections.Generic.Dictionary<short, float>(2);
            m_vFloats[key] = value;
        }
        //-----------------------------------------------------
        public float GetFloat(short key, float defaultValue = 0f)
        {
            if (m_vFloats != null && m_vFloats.TryGetValue(key, out var val))
                return val;
            return defaultValue;
        }
        //-----------------------------------------------------
        public bool GetFloat(short key, out float value)
        {
            if (m_vFloats != null && m_vFloats.TryGetValue(key, out value))
                return true;
            value = 0.0f;
            return false;
        }
        //-----------------------------------------------------
        public void SetString(short key, string value)
        {
            if (m_vStrings == null) m_vStrings = new System.Collections.Generic.Dictionary<short, string>(2);
            m_vStrings[key] = value;
        }
        //-----------------------------------------------------
        public string GetString(short key, string defaultValue = null)
        {
            if (m_vStrings != null && m_vStrings.TryGetValue(key, out var val))
                return val;
            return defaultValue;
        }
        //-----------------------------------------------------
        public bool GetString(short key, out string value)
        {
            if (m_vStrings != null && m_vStrings.TryGetValue(key, out value))
                return true;
            value = null;
            return false;
        }
        //-----------------------------------------------------
        public void SetVec2(short key, Vector2 value)
        {
            if (m_vVec2s == null) m_vVec2s = new System.Collections.Generic.Dictionary<short, Vector2>(2);
            m_vVec2s[key] = value;
        }
        //-----------------------------------------------------
        public Vector2 GetVec2(short key, Vector2 defaultValue = default)
        {
            if (m_vVec2s != null && m_vVec2s.TryGetValue(key, out var val))
                return val;
            return defaultValue;
        }
        //-----------------------------------------------------
        public bool GetVec2(short key, out Vector2 value)
        {
            if (m_vVec2s != null && m_vVec2s.TryGetValue(key, out value))
                return true;
            value = Vector2.zero;
            return false;
        }
        //-----------------------------------------------------
        public void SetVec3(short key, Vector3 value)
        {
            if (m_vVec3s == null) m_vVec3s = new System.Collections.Generic.Dictionary<short, Vector3>(2);
            m_vVec3s[key] = value;
        }
        //-----------------------------------------------------
        public Vector3 GetVec3(short key)
        {
            if (m_vVec3s != null && m_vVec3s.TryGetValue(key, out var val))
                return val;
            return Vector3.zero;
        }
        //-----------------------------------------------------
        public bool GetVec3(short key, out Vector3 value)
        {
            if (m_vVec3s != null && m_vVec3s.TryGetValue(key, out value))
                return true;
            value = Vector3.zero;
            return false;
        }
        //-----------------------------------------------------
        public void SetVec4(short key, Vector4 value)
        {
            if (m_vVec4s == null) m_vVec4s = new System.Collections.Generic.Dictionary<short, Vector4>(2);
            m_vVec4s[key] = value;
        }
        //-----------------------------------------------------
        public Vector4 GetVec4(short key)
        {
            if (m_vVec4s != null && m_vVec4s.TryGetValue(key, out var val))
                return val;
            return Vector4.one;
        }
        //-----------------------------------------------------
        public bool GetVec4(short key, out Vector4 value)
        {
            if (m_vVec4s != null && m_vVec4s.TryGetValue(key, out value))
                return true;
            value = Vector4.zero;
            return false;
        }
        //-----------------------------------------------------
        public void SetObjId(short key, ObjId value)
        {
            if (m_vObjIds == null) m_vObjIds = new System.Collections.Generic.Dictionary<short, ObjId>(2);
            m_vObjIds[key] = value;
        }
        //-----------------------------------------------------
        public ObjId GetObjId(short key, ObjId defaultValue = default)
        {
            if (m_vObjIds != null && m_vObjIds.TryGetValue(key, out var val))
                return val;
            return defaultValue;
        }
        //-----------------------------------------------------
        public bool GetObjId(short key, out ObjId value)
        {
            if (m_vObjIds != null && m_vObjIds.TryGetValue(key, out value))
                return true;
            value =new ObjId() { id =0};
            return false;
        }
        //-----------------------------------------------------
        public void SetVariable(IVariable variable)
		{
			if (variable is VariableInt vInt)
				SetVariable(vInt);
			else if (variable is VariableBool vBool)
				SetVariable(vBool);
			else if (variable is VariableFloat vFloat)
				SetVariable(vFloat);
			else if (variable is VariableString vString)
				SetVariable(vString);
			else if (variable is VariableVec2 vVec2)
				SetVariable(vVec2);
			else if (variable is VariableVec3 vVec3)
				SetVariable(vVec3);
			else if (variable is VariableVec4 vVec4)
				SetVariable(vVec4);
			else if (variable is VariableObjId vObjId)
				SetVariable(vObjId);
		}
    }
}