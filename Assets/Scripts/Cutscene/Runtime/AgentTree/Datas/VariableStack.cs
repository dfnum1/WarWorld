/********************************************************************
生成日期:	06:30:2025
类    名: 	VariableStack
作    者:	HappLI
描    述:	变量栈
*********************************************************************/
using System;
using UnityEngine;

namespace Framework.AT.Runtime
{
    public class VariableStack
    {
        System.Collections.Generic.Stack<bool>          m_vBools = null;
        System.Collections.Generic.Stack<int>           m_vInts = null;
        System.Collections.Generic.Stack<float>         m_vFloats = null;
        System.Collections.Generic.Stack<string>        m_vStrings = null;
        System.Collections.Generic.Stack<Vector2>       m_vVec2s = null;
        System.Collections.Generic.Stack<Vector3>       m_vVec3s = null;
        System.Collections.Generic.Stack<Vector4>       m_vVec4s = null;
        System.Collections.Generic.Stack<ObjId>         m_vObjIds = null;
        System.Collections.Generic.Stack<EVariableType> m_vTypes = null;
        byte                                            m_nCapacity = 2;
        //-----------------------------------------------------
        internal VariableStack()
        {

        }
        //-----------------------------------------------------
        public static VariableStack Malloc(int capacity = 2)
        {
            VariableStack list = VariablePool.GetVariableStack();
            list.m_nCapacity = (byte)Mathf.Clamp(capacity, 1, 255);
            return list;
        }
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
            m_vTypes?.Clear();
        }
        //-----------------------------------------------------
        public EVariableType GetVarType()
        {
            if (m_vTypes == null || m_vTypes.Count == 0) return EVariableType.eNone;
            return m_vTypes.Peek();
        }
        //-----------------------------------------------------
        public void PushBool(bool value)
        {
            if (m_vBools == null) m_vBools = new System.Collections.Generic.Stack<bool>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new System.Collections.Generic.Stack<EVariableType>(m_nCapacity);
            m_vTypes.Push(EVariableType.eBool);
            m_vBools.Push(value);
        }
        //-----------------------------------------------------
        public bool PopBool(out bool value)
        {
            if (m_vBools != null && m_vBools.Count > 0 && m_vTypes != null && m_vTypes.Count > 0)
            {
                var type = m_vTypes.Pop();
                if (type != EVariableType.eBool)
                {
                    Debug.LogError($"VariableStack: PopBool type mismatch, expected {EVariableType.eBool}, got {type}");
                }
                value = m_vBools.Pop();
                return true;
            }
            value = false;
            return false;
        }
        //-----------------------------------------------------
        public void PushInt(int value)
        {
            if (m_vInts == null) m_vInts = new System.Collections.Generic.Stack<int>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new System.Collections.Generic.Stack<EVariableType>(m_nCapacity);
            m_vTypes.Push(EVariableType.eInt);
            m_vInts.Push(value);
        }
        //-----------------------------------------------------
        public bool PopInt(out int value)
        {
            if (m_vInts != null && m_vInts.Count > 0 && m_vTypes != null && m_vTypes.Count > 0)
            {
                var type = m_vTypes.Pop();
                if (type != EVariableType.eInt)
                {
                    Debug.LogError($"VariableStack: PopInt type mismatch, expected {EVariableType.eInt}, got {type}");
                }
                value = m_vInts.Pop();
                return true;
            }
            value = 0;
            return false;
        }
        //-----------------------------------------------------
        public void PushFloat(float value)
        {
            if (m_vFloats == null) m_vFloats = new System.Collections.Generic.Stack<float>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new System.Collections.Generic.Stack<EVariableType>(m_nCapacity);
            m_vTypes.Push(EVariableType.eFloat);
            m_vFloats.Push(value);
        }
        //-----------------------------------------------------
        public bool PopFloat(out float value)
        {
            if (m_vFloats != null && m_vFloats.Count > 0 && m_vTypes != null && m_vTypes.Count > 0)
            {
                var type = m_vTypes.Pop();
                if (type != EVariableType.eFloat)
                {
                    Debug.LogError($"VariableStack: PopFloat type mismatch, expected {EVariableType.eFloat}, got {type}");
                }
                value = m_vFloats.Pop();
                return true;
            }
            value = 0.0f;
            return false;
        }
        //-----------------------------------------------------
        public void PushString(string value)
        {
            if (m_vStrings == null) m_vStrings = new System.Collections.Generic.Stack<string>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new System.Collections.Generic.Stack<EVariableType>(m_nCapacity);
            m_vTypes.Push(EVariableType.eString);
            m_vStrings.Push(value);
        }
        //-----------------------------------------------------
        public bool PopString(out string value)
        {
            if (m_vStrings != null && m_vStrings.Count > 0 && m_vTypes != null && m_vTypes.Count > 0)
            {
                var type = m_vTypes.Pop();
                if (type != EVariableType.eString)
                {
                    Debug.LogError($"VariableStack: PopString type mismatch, expected {EVariableType.eString}, got {type}");
                }
                value = m_vStrings.Pop();
                return true;
            }
            value = null;
            return false;
        }
        //-----------------------------------------------------
        public void PushVec2(Vector2 value)
        {
            if (m_vVec2s == null) m_vVec2s = new System.Collections.Generic.Stack<Vector2>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new System.Collections.Generic.Stack<EVariableType>(m_nCapacity);
            m_vTypes.Push(EVariableType.eVec2);
            m_vVec2s.Push(value);
        }
        //-----------------------------------------------------
        public bool PopVec2(out Vector2 value)
        {
            if (m_vVec2s != null && m_vVec2s.Count > 0 && m_vTypes != null && m_vTypes.Count > 0)
            {
                var type = m_vTypes.Pop();
                if (type != EVariableType.eVec2)
                {
                    Debug.LogError($"VariableStack: PopVec2 type mismatch, expected {EVariableType.eVec2}, got {type}");
                }
                value = m_vVec2s.Pop();
                return true;
            }
            value = Vector2.zero;
            return false;
        }
        //-----------------------------------------------------
        public void PushVec3(Vector3 value)
        {
            if (m_vVec3s == null) m_vVec3s = new System.Collections.Generic.Stack<Vector3>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new System.Collections.Generic.Stack<EVariableType>(m_nCapacity);
            m_vTypes.Push(EVariableType.eVec3);
            m_vVec3s.Push(value);
        }
        //-----------------------------------------------------
        public bool PopVec3(out Vector3 value)
        {
            if (m_vVec3s != null && m_vVec3s.Count > 0 && m_vTypes != null && m_vTypes.Count > 0)
            {
                var type = m_vTypes.Pop();
                if (type != EVariableType.eVec3)
                {
                    Debug.LogError($"VariableStack: PopVec3 type mismatch, expected {EVariableType.eVec3}, got {type}");
                }
                value = m_vVec3s.Pop();
                return true;
            }
            value = Vector3.zero;
            return false;
        }
        //-----------------------------------------------------
        public void PushVec4(Vector4 value)
        {
            if (m_vVec4s == null) m_vVec4s = new System.Collections.Generic.Stack<Vector4>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new System.Collections.Generic.Stack<EVariableType>(m_nCapacity);
            m_vTypes.Push(EVariableType.eVec4);
            m_vVec4s.Push(value);
        }
        //-----------------------------------------------------
        public bool PopVec4(out Vector4 value)
        {
            if (m_vVec4s != null && m_vVec4s.Count > 0 && m_vTypes != null && m_vTypes.Count > 0)
            {
                var type = m_vTypes.Pop();
                if (type != EVariableType.eVec4)
                {
                    Debug.LogError($"VariableStack: PopVec4 type mismatch, expected {EVariableType.eVec4}, got {type}");
                }
                value = m_vVec4s.Pop();
                return true;
            }
            value = Vector4.zero;
            return false;
        }
        //-----------------------------------------------------
        public void PushObjId(ObjId value)
        {
            if (m_vObjIds == null) m_vObjIds = new System.Collections.Generic.Stack<ObjId>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new System.Collections.Generic.Stack<EVariableType>(m_nCapacity);
            m_vTypes.Push(EVariableType.eObjId);
            m_vObjIds.Push(value);
        }
        //-----------------------------------------------------
        public bool PopObjId(out ObjId value)
        {
            if (m_vObjIds != null && m_vObjIds.Count > 0 && m_vTypes != null && m_vTypes.Count > 0)
            {
                var type = m_vTypes.Pop();
                if (type != EVariableType.eObjId)
                {
                    Debug.LogError($"VariableStack: PopObjId type mismatch, expected {EVariableType.eObjId}, got {type}");
                }
                value = m_vObjIds.Pop();
                return true;
            }
            value = new ObjId();
            return false;
        }
    }
}