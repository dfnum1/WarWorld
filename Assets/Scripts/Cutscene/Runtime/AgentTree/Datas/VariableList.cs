/********************************************************************
生成日期:	06:30:2025
类    名: 	VariableList
作    者:	HappLI
描    述:	变量列表存储类
*********************************************************************/
using System.Collections.Generic;
using UnityEngine;


namespace Framework.AT.Runtime
{
    public class VariableList
    {
        struct TypeIndex
        {
            public EVariableType type;
            public byte index;
            public TypeIndex(EVariableType type, byte index)
            {
                this.type = type;
                this.index = index;
            }
        }
        List<bool>          m_vBools = null;
        List<int>           m_vInts = null;
        List<float>         m_vFloats = null;
        List<string>        m_vStrings = null;
        List<Vector2>       m_vVec2s = null;
        List<Vector3>       m_vVec3s = null;
        List<Vector4>       m_vVec4s = null;
        List<ObjId>         m_vObjIds = null;
        List<TypeIndex>     m_vTypes = null;
        byte                m_nCapacity = 2;
        //-----------------------------------------------------
        internal VariableList()
        {

        }
        //-----------------------------------------------------
        public static VariableList Malloc(int capacity =2)
        {
            VariableList list= VariablePool.GetVariableList();
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
        public int GetVarCount()
        {
            if (m_vTypes == null) return 0;
            return m_vTypes.Count;
        }
        //-----------------------------------------------------
        public EVariableType GetVarType(int index)
        {
            if (index < 0 || m_vTypes == null || m_vTypes.Count == 0 || index >= m_vTypes.Count) return EVariableType.eNone;
            return m_vTypes[index].type;
        }
        //-----------------------------------------------------
        public void AddBool(bool value)
        {
            if (m_vBools == null) m_vBools = new List<bool>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new List<TypeIndex>(m_nCapacity);
            m_vTypes.Add(new TypeIndex(EVariableType.eBool, (byte)m_vBools.Count));
            m_vBools.Add(value);
        }
        //-----------------------------------------------------
        public void SetBool(int index, bool value)
        {
            if (index >= 0 && m_vBools != null && m_vBools.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eBool)
                {
                    Debug.LogError($"VariableList: SetBool type mismatch, expected {EVariableType.eBool}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vBools.Count)
                {
                    Debug.LogError($"VariableList: SetBool index out of range, index={type.index}, count={m_vBools.Count}");
                    return;
                }
                m_vBools[type.index] = value;
            }
        }
        //-----------------------------------------------------
        public bool GetBool(int index, bool bDefault = false)
        {
            if (index >= 0 && m_vBools != null && m_vBools.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eBool)
                {
                    Debug.LogError($"VariableList: GetBool type mismatch, expected {EVariableType.eBool}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vBools.Count)
                {
                    Debug.LogError($"VariableList: GetBool index out of range, index={type.index}, count={m_vBools.Count}");
                    return bDefault;
                }
                return m_vBools[type.index];
            }
            return bDefault;
        }
        //-----------------------------------------------------
        public List<bool> GetBools()
        {
            return m_vBools;
        }
        //-----------------------------------------------------
        public void AddInt(int value)
        {
            if (m_vInts == null) m_vInts = new List<int>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new List<TypeIndex>(m_nCapacity);
            m_vTypes.Add(new TypeIndex(EVariableType.eInt, (byte)m_vInts.Count));
            m_vInts.Add(value);
        }
        //-----------------------------------------------------
        public void SetInt(int index, int value)
        {
            if (index >= 0 && m_vInts != null && m_vInts.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eInt)
                {
                    Debug.LogError($"VariableList: SetInt type mismatch, expected {EVariableType.eInt}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vInts.Count)
                {
                    Debug.LogError($"VariableList: SetInt index out of range, index={type.index}, count={m_vInts.Count}");
                    return;
                }
                m_vInts[type.index] = value;
            }
        }
        //-----------------------------------------------------
        public int GetInt(int index, int defaultValue = 0)
        {
            if (index >= 0 && m_vInts != null && m_vInts.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eInt)
                {
                    Debug.LogError($"VariableList: GetInt type mismatch, expected {EVariableType.eInt}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vInts.Count)
                {
                    Debug.LogError($"VariableList: GetInt index out of range, index={type.index}, count={m_vInts.Count}");
                    return defaultValue;
                }
                return m_vInts[type.index];
            }
            return defaultValue;
        }
        //-----------------------------------------------------
        public List<int> GetInts()
        {
            return m_vInts;
        }
        //-----------------------------------------------------
        public void AddFloat(float value)
        {
            if (m_vFloats == null) m_vFloats = new List<float>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new List<TypeIndex>(m_nCapacity);
            m_vTypes.Add(new TypeIndex(EVariableType.eFloat, (byte)m_vFloats.Count));
            m_vFloats.Add(value);
        }
        //-----------------------------------------------------
        public void SetFloat(int index, float value)
        {
            if (index >= 0 && m_vFloats != null && m_vFloats.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eFloat)
                {
                    Debug.LogError($"VariableList: SetFloat type mismatch, expected {EVariableType.eFloat}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vFloats.Count)
                {
                    Debug.LogError($"VariableList: SetFloat index out of range, index={type.index}, count={m_vFloats.Count}");
                    return;
                }
                m_vFloats[type.index] = value;
            }
        }
        //-----------------------------------------------------
        public float GetFloat(int index, float defaultValue = 0f)
        {
            if (index >= 0 && m_vFloats != null && m_vFloats.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eFloat)
                {
                    Debug.LogError($"VariableList: GetFloat type mismatch, expected {EVariableType.eFloat}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vFloats.Count)
                {
                    Debug.LogError($"VariableList: GetFloat index out of range, index={type.index}, count={m_vFloats.Count}");
                    return defaultValue;
                }
                return m_vFloats[type.index];
            }
            return defaultValue;
        }
        //-----------------------------------------------------
        public List<float> GetFloats()
        {
            return m_vFloats;
        }
        //-----------------------------------------------------
        public void AddString(string value)
        {
            if (m_vStrings == null) m_vStrings = new List<string>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new List<TypeIndex>(m_nCapacity);
            m_vTypes.Add(new TypeIndex(EVariableType.eString, (byte)m_vStrings.Count));
            m_vStrings.Add(value);
        }
        //-----------------------------------------------------
        public void SetString(int index, string value)
        {
            if (index >= 0 && m_vStrings != null && m_vStrings.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eString)
                {
                    Debug.LogError($"VariableList: SetString type mismatch, expected {EVariableType.eString}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vStrings.Count)
                {
                    Debug.LogError($"VariableList: SetString index out of range, index={type.index}, count={m_vStrings.Count}");
                    return;
                }
                m_vStrings[type.index] = value;
            }
        }
        //-----------------------------------------------------
        public string GetString(int index, string defaultValue = null)
        {
            if (index >= 0 && m_vStrings != null && m_vStrings.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eString)
                {
                    Debug.LogError($"VariableList: GetString type mismatch, expected {EVariableType.eString}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vStrings.Count)
                {
                    Debug.LogError($"VariableList: GetString index out of range, index={type.index}, count={m_vStrings.Count}");
                    return defaultValue;
                }
                return m_vStrings[type.index];
            }
            return defaultValue;
        }
        //-----------------------------------------------------
        public List<string> GetStrings()
        {
            return m_vStrings;
        }
        //-----------------------------------------------------
        public void AddVec2(Vector2 value)
        {
            if (m_vVec2s == null) m_vVec2s = new List<Vector2>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new List<TypeIndex>(m_nCapacity);
            m_vTypes.Add(new TypeIndex(EVariableType.eVec2, (byte)m_vVec2s.Count));
            m_vVec2s.Add(value);
        }
        //-----------------------------------------------------
        public void SetVec2(int index, Vector2 value)
        {
            if (index >= 0 && m_vVec2s != null && m_vVec2s.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eVec2)
                {
                    Debug.LogError($"VariableList: SetVec2 type mismatch, expected {EVariableType.eVec2}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vVec2s.Count)
                {
                    Debug.LogError($"VariableList: SetVec2 index out of range, index={type.index}, count={m_vVec2s.Count}");
                    return;
                }
                m_vVec2s[type.index] = value;
            }
        }
        //-----------------------------------------------------
        public Vector2 GetVec2(int index)
        {
            if (index >= 0 && m_vVec2s != null && m_vVec2s.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eVec2)
                {
                    Debug.LogError($"VariableList: GetVec2 type mismatch, expected {EVariableType.eVec2}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vVec2s.Count)
                {
                    Debug.LogError($"VariableList: GetVec2 index out of range, index={type.index}, count={m_vVec2s.Count}");
                    return Vector2.zero;
                }
                return m_vVec2s[type.index];
            }
            return Vector2.zero;
        }
        //-----------------------------------------------------
        public List<Vector2> GetVec2s()
        {
            return m_vVec2s;
        }
        //-----------------------------------------------------
        public void AddVec3(Vector3 value)
        {
            if (m_vVec3s == null) m_vVec3s = new List<Vector3>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new List<TypeIndex>(m_nCapacity);
            m_vTypes.Add(new TypeIndex(EVariableType.eVec3, (byte)m_vVec3s.Count));
            m_vVec3s.Add(value);
        }
        //-----------------------------------------------------
        public void SetVec3(int index, Vector3 value)
        {
            if (index >= 0 && m_vVec3s != null && m_vVec3s.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eVec3)
                {
                    Debug.LogError($"VariableList: SetVec3 type mismatch, expected {EVariableType.eVec3}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vVec3s.Count)
                {
                    Debug.LogError($"VariableList: SetVec3 index out of range, index={type.index}, count={m_vVec3s.Count}");
                    return;
                }
                m_vVec3s[type.index] = value;
            }
        }
        //-----------------------------------------------------
        public Vector3 GetVec3(int index)
        {
            if (index >= 0 && m_vVec3s != null && m_vVec3s.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eVec3)
                {
                    Debug.LogError($"VariableList: GetVec3 type mismatch, expected {EVariableType.eVec3}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vVec3s.Count)
                {
                    Debug.LogError($"VariableList: GetVec3 index out of range, index={type.index}, count={m_vVec3s.Count}");
                    return Vector3.zero;
                }
                return m_vVec3s[type.index];
            }
            return Vector3.zero;
        }
        //-----------------------------------------------------
        public List<Vector3> GetVec3s()
        {
            return m_vVec3s;
        }
        //-----------------------------------------------------
        public void AddVec4(Vector4 value)
        {
            if (m_vVec4s == null) m_vVec4s = new List<Vector4>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new List<TypeIndex>(m_nCapacity);
            m_vTypes.Add(new TypeIndex(EVariableType.eVec4, (byte)m_vVec4s.Count));
            m_vVec4s.Add(value);
        }
        //-----------------------------------------------------
        public void SetVec4(int index, Vector4 value)
        {
            if (index >= 0 && m_vVec4s != null && m_vVec4s.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eVec4)
                {
                    Debug.LogError($"VariableList: SetVec4 type mismatch, expected {EVariableType.eVec4}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vVec4s.Count)
                {
                    Debug.LogError($"VariableList: SetVec4 index out of range, index={type.index}, count={m_vVec4s.Count}");
                    return;
                }
                m_vVec4s[type.index] = value;
            }
        }
        //-----------------------------------------------------
        public Vector4 GetVec4(int index)
        {
            if (index >= 0 && m_vVec4s != null && m_vVec4s.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eVec4)
                {
                    Debug.LogError($"VariableList: GetVec4 type mismatch, expected {EVariableType.eVec4}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vVec4s.Count)
                {
                    Debug.LogError($"VariableList: GetVec4 index out of range, index={type.index}, count={m_vVec4s.Count}");
                    return Vector4.zero;
                }
                return m_vVec4s[type.index];
            }
            return Vector4.zero;
        }
        //-----------------------------------------------------
        public List<Vector4> GetVec4s()
        {
            return m_vVec4s;
        }
        //-----------------------------------------------------
        public void AddObjId(ObjId value)
        {
            if (m_vObjIds == null) m_vObjIds = new List<ObjId>(m_nCapacity);
            if (m_vTypes == null) m_vTypes = new List<TypeIndex>(m_nCapacity);
            m_vTypes.Add(new TypeIndex(EVariableType.eObjId, (byte)m_vObjIds.Count));
            m_vObjIds.Add(value);
        }
        //-----------------------------------------------------
        public void SetObjId(int index, ObjId value)
        {
            if (index >= 0 && m_vObjIds != null && m_vObjIds.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eObjId)
                {
                    Debug.LogError($"VariableList: SetObjId type mismatch, expected {EVariableType.eObjId}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vObjIds.Count)
                {
                    Debug.LogError($"VariableList: SetObjId index out of range, index={type.index}, count={m_vObjIds.Count}");
                    return;
                }
                m_vObjIds[type.index] = value;
            }
        }
        //-----------------------------------------------------
        public ObjId GetObjId(int index, ObjId defaultValue = default)
        {
            if (index >= 0 && m_vObjIds != null && m_vObjIds.Count > 0 && m_vTypes != null && m_vTypes.Count > 0 && index < m_vTypes.Count)
            {
                var type = m_vTypes[index];
                if (type.type != EVariableType.eObjId)
                {
                    Debug.LogError($"VariableList: GetObjId type mismatch, expected {EVariableType.eObjId}, got {type}");
                }
                if (type.index < 0 || type.index >= m_vObjIds.Count)
                {
                    Debug.LogError($"VariableList: GetObjId index out of range, index={type.index}, count={m_vObjIds.Count}");
                    return defaultValue;
                }
                return m_vObjIds[type.index];
            }
            return defaultValue;
        }
        //-----------------------------------------------------
        public List<ObjId> GetObjIds()
        {
            return m_vObjIds;
        }
        //-----------------------------------------------------
        internal void ChangeType(int index, EVariableType type, string defaultValue = null)
        {
            if (m_vTypes == null || index < 0 || index >= m_vTypes.Count)
                return;

            var oldTypeIndex = m_vTypes[index];
            int removedDataIndex = oldTypeIndex.index;

            // 1. 移除原类型数据
            switch (oldTypeIndex.type)
            {
                case EVariableType.eBool: m_vBools?.RemoveAt(removedDataIndex); break;
                case EVariableType.eInt: m_vInts?.RemoveAt(removedDataIndex); break;
                case EVariableType.eFloat: m_vFloats?.RemoveAt(removedDataIndex); break;
                case EVariableType.eString: m_vStrings?.RemoveAt(removedDataIndex); break;
                case EVariableType.eVec2: m_vVec2s?.RemoveAt(removedDataIndex); break;
                case EVariableType.eVec3: m_vVec3s?.RemoveAt(removedDataIndex); break;
                case EVariableType.eVec4: m_vVec4s?.RemoveAt(removedDataIndex); break;
                case EVariableType.eObjId: m_vObjIds?.RemoveAt(removedDataIndex); break;
                default: break;
            }

            // 2. 修正 m_vTypes 里同类型且 index > 被移除的 index 的 TypeIndex
            for (int i = 0; i < m_vTypes.Count; ++i)
            {
                if (i == index) continue;
                if (m_vTypes[i].type == oldTypeIndex.type && m_vTypes[i].index > removedDataIndex)
                {
                    m_vTypes[i] = new TypeIndex(m_vTypes[i].type, (byte)(m_vTypes[i].index - 1));
                }
            }

            // 3. 移除 m_vTypes 的该项
            m_vTypes.RemoveAt(index);

            // 4. 插入新类型的值（优先用defaultValue，否则用类型默认值）
            switch (type)
            {
                case EVariableType.eBool:
                    {
                        bool v = false;
                        if (!string.IsNullOrEmpty(defaultValue))
                            bool.TryParse(defaultValue, out v);
                        AddBool(v);
                    }
                    break;
                case EVariableType.eInt:
                    {
                        int v = 0;
                        if (!string.IsNullOrEmpty(defaultValue))
                            int.TryParse(defaultValue, out v);
                        AddInt(v);
                    }
                    break;
                case EVariableType.eFloat:
                    {
                        float v = 0;
                        if (!string.IsNullOrEmpty(defaultValue))
                            float.TryParse(defaultValue, out v);
                        AddFloat(v);
                    }
                    break;
                case EVariableType.eString:
                    {
                        AddString(defaultValue ?? string.Empty);
                    }
                    break;
                case EVariableType.eVec2:
                    {
                        Vector2 v = Vector2.zero;
                        if (!string.IsNullOrEmpty(defaultValue))
                        {
                            var split = defaultValue.Split('|');
                            if (split.Length >= 2)
                            {
                                float.TryParse(split[0], out v.x);
                                float.TryParse(split[1], out v.y);
                            }
                        }
                        AddVec2(v);
                    }
                    break;
                case EVariableType.eVec3:
                    {
                        Vector3 v = Vector3.zero;
                        if (!string.IsNullOrEmpty(defaultValue))
                        {
                            var split = defaultValue.Split('|');
                            if (split.Length >= 3)
                            {
                                float.TryParse(split[0], out v.x);
                                float.TryParse(split[1], out v.y);
                                float.TryParse(split[2], out v.z);
                            }
                        }
                        AddVec3(v);
                    }
                    break;
                case EVariableType.eVec4:
                    {
                        Vector4 v = Vector4.zero;
                        if (!string.IsNullOrEmpty(defaultValue))
                        {
                            var split = defaultValue.Split('|');
                            if (split.Length >= 4)
                            {
                                float.TryParse(split[0], out v.x);
                                float.TryParse(split[1], out v.y);
                                float.TryParse(split[2], out v.z);
                                float.TryParse(split[3], out v.w);
                            }
                        }
                        AddVec4(v);
                    }
                    break;
                case EVariableType.eObjId:
                    {
                        ObjId v = default;
                        if (!string.IsNullOrEmpty(defaultValue))
                            int.TryParse(defaultValue, out v.id);
                        AddObjId(v);
                    }
                    break;
                default:
                    break;
            }
        }
        //-----------------------------------------------------
        internal void RemoveIndex(int index)
        {
            if (m_vTypes == null || index < 0 || index >= m_vTypes.Count)
                return;

            var typeIndex = m_vTypes[index];
            int dataIndex = typeIndex.index;

            // 1. 移除类型数据
            switch (typeIndex.type)
            {
                case EVariableType.eBool: m_vBools?.RemoveAt(dataIndex); break;
                case EVariableType.eInt: m_vInts?.RemoveAt(dataIndex); break;
                case EVariableType.eFloat: m_vFloats?.RemoveAt(dataIndex); break;
                case EVariableType.eString: m_vStrings?.RemoveAt(dataIndex); break;
                case EVariableType.eVec2: m_vVec2s?.RemoveAt(dataIndex); break;
                case EVariableType.eVec3: m_vVec3s?.RemoveAt(dataIndex); break;
                case EVariableType.eVec4: m_vVec4s?.RemoveAt(dataIndex); break;
                case EVariableType.eObjId: m_vObjIds?.RemoveAt(dataIndex); break;
                default: break;
            }

            // 2. 修正 m_vTypes 里同类型且 index > 被移除的 index 的 TypeIndex
            for (int i = 0; i < m_vTypes.Count; ++i)
            {
                if (i == index) continue;
                if (m_vTypes[i].type == typeIndex.type && m_vTypes[i].index > dataIndex)
                {
                    m_vTypes[i] = new TypeIndex(m_vTypes[i].type, (byte)(m_vTypes[i].index - 1));
                }
            }

            // 3. 移除 m_vTypes 的该项
            m_vTypes.RemoveAt(index);
        }
        //-----------------------------------------------------
        internal void SwapIndex(int index0, int index1)
        {
            if (m_vTypes == null || index0 < 0 || index1 < 0 || index0 >= m_vTypes.Count || index1 >= m_vTypes.Count || index0 == index1)
                return;

            var typeIndex0 = m_vTypes[index0];
            var typeIndex1 = m_vTypes[index1];

            if (typeIndex0.type == typeIndex1.type)
            {
                // 同类型，交换数据和TypeIndex.index
                switch (typeIndex0.type)
                {
                    case EVariableType.eBool:
                        if (m_vBools != null)
                        {
                            bool tmp = m_vBools[typeIndex0.index];
                            m_vBools[typeIndex0.index] = m_vBools[typeIndex1.index];
                            m_vBools[typeIndex1.index] = tmp;
                        }
                        break;
                    case EVariableType.eInt:
                        if (m_vInts != null)
                        {
                            int tmp = m_vInts[typeIndex0.index];
                            m_vInts[typeIndex0.index] = m_vInts[typeIndex1.index];
                            m_vInts[typeIndex1.index] = tmp;
                        }
                        break;
                    case EVariableType.eFloat:
                        if (m_vFloats != null)
                        {
                            float tmp = m_vFloats[typeIndex0.index];
                            m_vFloats[typeIndex0.index] = m_vFloats[typeIndex1.index];
                            m_vFloats[typeIndex1.index] = tmp;
                        }
                        break;
                    case EVariableType.eString:
                        if (m_vStrings != null)
                        {
                            string tmp = m_vStrings[typeIndex0.index];
                            m_vStrings[typeIndex0.index] = m_vStrings[typeIndex1.index];
                            m_vStrings[typeIndex1.index] = tmp;
                        }
                        break;
                    case EVariableType.eVec2:
                        if (m_vVec2s != null)
                        {
                            Vector2 tmp = m_vVec2s[typeIndex0.index];
                            m_vVec2s[typeIndex0.index] = m_vVec2s[typeIndex1.index];
                            m_vVec2s[typeIndex1.index] = tmp;
                        }
                        break;
                    case EVariableType.eVec3:
                        if (m_vVec3s != null)
                        {
                            Vector3 tmp = m_vVec3s[typeIndex0.index];
                            m_vVec3s[typeIndex0.index] = m_vVec3s[typeIndex1.index];
                            m_vVec3s[typeIndex1.index] = tmp;
                        }
                        break;
                    case EVariableType.eVec4:
                        if (m_vVec4s != null)
                        {
                            Vector4 tmp = m_vVec4s[typeIndex0.index];
                            m_vVec4s[typeIndex0.index] = m_vVec4s[typeIndex1.index];
                            m_vVec4s[typeIndex1.index] = tmp;
                        }
                        break;
                    case EVariableType.eObjId:
                        if (m_vObjIds != null)
                        {
                            ObjId tmp = m_vObjIds[typeIndex0.index];
                            m_vObjIds[typeIndex0.index] = m_vObjIds[typeIndex1.index];
                            m_vObjIds[typeIndex1.index] = tmp;
                        }
                        break;
                    default:
                        break;
                }
                // 交换TypeIndex.index
                m_vTypes[index0] = new TypeIndex(typeIndex0.type, typeIndex1.index);
                m_vTypes[index1] = new TypeIndex(typeIndex1.type, typeIndex0.index);
            }
            else
            {
                // 不同类型，只交换TypeIndex，不动数据
                m_vTypes[index0] = typeIndex1;
                m_vTypes[index1] = typeIndex0;
            }
        }
        //-----------------------------------------------------
        public bool AddVariable(IVariable value)
        {
            // 这里可以直接处理 IVariable
            if (value is VariableBool varB) AddBool(varB.value);
            else if (value is VariableInt varI) AddInt(varI.value);
            else if (value is VariableFloat varF) AddFloat(varF.value);
            else if (value is VariableVec2 varVec2) AddVec2(varVec2.value);
            else if (value is VariableVec3 varVec3) AddVec3(varVec3.value);
            else if (value is VariableVec4 varVec4) AddVec4(varVec4.value);
            else if (value is VariableString varStr) AddString(varStr.value);
            else if (value is VariableObjId varObj) AddObjId(varObj.value);
            else return false;
            return true;
        }
        //-----------------------------------------------------
        public bool AddVariable(VariableList value)
        {
            if (value == null || value.GetVarCount() == 0)
                return false;

            int count = value.GetVarCount();
            for (int i = 0; i < count; i++)
            {
                AddVariable(value, i);
            }
            return true;
        }
        //-----------------------------------------------------
        public bool AddVariable(VariableList value, int index)
        {
            if (value == null || value.GetVarCount() == 0)
                return false;

            int count = value.GetVarCount();
            if (index < 0 || index >= count)
                return false;

            var type = value.GetVarType(index);
            switch (type)
            {
                case EVariableType.eBool:
                    AddBool(value.GetBool(index));
                    break;
                case EVariableType.eInt:
                    AddInt(value.GetInt(index));
                    break;
                case EVariableType.eFloat:
                    AddFloat(value.GetFloat(index));
                    break;
                case EVariableType.eString:
                    AddString(value.GetString(index));
                    break;
                case EVariableType.eVec2:
                    AddVec2(value.GetVec2(index));
                    break;
                case EVariableType.eVec3:
                    AddVec3(value.GetVec3(index));
                    break;
                case EVariableType.eVec4:
                    AddVec4(value.GetVec4(index));
                    break;
                case EVariableType.eObjId:
                    AddObjId(value.GetObjId(index));
                    break;
                default:
                    // 跳过未知类型
                    break;
            }
            return true;
        }
        //-----------------------------------------------------
        public bool AddVariable(string value)
        {
            AddString(value);
            return true;
        }
        //-----------------------------------------------------
        public bool AddVariable<T>(T value) where T : struct
        {
            if (value is byte byVal) AddInt(byVal);
            else if (value is short sVal) AddInt(sVal);
            else if (value is ushort usVal) AddInt(usVal);
            else if (value is int intVal) AddInt(intVal);
            else if (value is uint uintVal) AddInt((int)uintVal);
            else if (value is float floatVal) AddFloat(floatVal);
            else if (value is bool boolVal) AddBool(boolVal);
            else if (value is Vector2 v2Val) AddVec2(v2Val);
            else if (value is Vector3 v3Val) AddVec3(v3Val);
            else if (value is Vector4 v4Val) AddVec4(v4Val);
            else if (value is ObjId objId) AddObjId(objId);
            else return false;
            return true;
        }
    }
}