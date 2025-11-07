/********************************************************************
生成日期:	06:30:2025
类    名: 	AgentTreeData
作    者:	HappLI
描    述:	过场动画行为树
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Editor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AT.Runtime
{
    //-----------------------------------------------------
    [System.Serializable]
    struct VaribaleSerizlizeGuidData
    {
        public VariableBool[] boolVariables;
        public VariableInt[] intVariables;
        public VariableFloat[] floatVariables;
        public VariableString[] stringVariables;
        public int GetVariableCnt()
        {
            int cnt = 0;
            if(boolVariables!=null) cnt += boolVariables.Length;
            if (intVariables != null) cnt += intVariables.Length;
            if (floatVariables != null) cnt += floatVariables.Length;
            if (stringVariables != null) cnt += stringVariables.Length;
            return cnt;
        }
    }
    //-----------------------------------------------------
    [System.Serializable]
    public class AgentTreeData
    {
        public EnterTask[] tasks;
        public ActionNode[] actions;
        public CutsceneEvent[] events;
        [UnityEngine.SerializeField] VaribaleSerizlizeGuidData varGuids;
        [System.NonSerialized]private Dictionary<short, IVariable> m_vVariables = null;
        [System.NonSerialized] private Dictionary<short, BaseNode> m_vNodes = null;
        [System.NonSerialized] private bool m_bInited = false;
        //-----------------------------------------------------
        public IVariable GetVariable(short guid)
        {
            if (m_vVariables == null) return null;
            IVariable variable = null;
            m_vVariables.TryGetValue(guid, out variable);
            return variable;
        }
        //-----------------------------------------------------
        internal BaseNode GetNode(short guid)
        {
            if (m_vNodes == null) return null;
            if (m_vNodes.TryGetValue(guid, out var pNode))
                return pNode;
            return null;
        }
        //-----------------------------------------------------
        public int GetNodeCnt()
        {
            int nodeCnt = 0;
            if (tasks != null) nodeCnt += tasks.Length;
            if (actions != null) nodeCnt += actions.Length;
            if (events != null) nodeCnt += events.Length;
            return nodeCnt;
        }
        //-----------------------------------------------------
        public bool IsValid()
        {
            return GetNodeCnt() > 0;
        }
        //-----------------------------------------------------
        internal void Init(bool bForce = false)
        {
            if (!bForce && m_bInited)
                return;
            m_bInited = true;
            int cnt = varGuids.GetVariableCnt();
            if (cnt > 0)
            {
                if (m_vVariables == null) m_vVariables = new Dictionary<short, IVariable>(cnt);
                else m_vVariables.Clear();
                if (varGuids.boolVariables != null)
                {
                    for (int i = 0; i < varGuids.boolVariables.Length; ++i)
                    {
                        m_vVariables[varGuids.boolVariables[i].GetGuid()] = varGuids.boolVariables[i];
                    }
                }
                if (varGuids.intVariables != null)
                {
                    for (int i = 0; i < varGuids.intVariables.Length; ++i)
                    {
                        m_vVariables[varGuids.intVariables[i].GetGuid()] = varGuids.intVariables[i];
                    }
                }
                if (varGuids.floatVariables != null)
                {
                    for (int i = 0; i < varGuids.floatVariables.Length; ++i)
                    {
                        m_vVariables[varGuids.floatVariables[i].GetGuid()] = varGuids.floatVariables[i];
                    }
                }
                if (varGuids.stringVariables != null)
                {
                    for (int i = 0; i < varGuids.stringVariables.Length; ++i)
                    {
                        m_vVariables[varGuids.stringVariables[i].GetGuid()] = varGuids.stringVariables[i];
                    }
                }
            }

            int nodeCnt = GetNodeCnt();
            if (nodeCnt > 0)
            {
                if (m_vNodes == null)
                    m_vNodes = new Dictionary<short, BaseNode>(nodeCnt);
                m_vNodes.Clear();
                if (tasks != null)
                {
                    for (int i = 0; i < tasks.Length; ++i)
                        m_vNodes[tasks[i].guid] = tasks[i];
                }
                if (actions != null)
                {
                    for (int i = 0; i < actions.Length; ++i)
                        m_vNodes[actions[i].guid] = actions[i];
                }
                if (events != null)
                {
                    for (int i = 0; i < events.Length; ++i)
                        m_vNodes[events[i].guid] = events[i];
                }
            }
            else
            {
                if (m_vNodes != null) m_vNodes.Clear();
            }
#if UNITY_EDITOR
            if (m_vNodes != null)
            {
                foreach (var db in m_vNodes)
                {
                    db.Value.GetNexts(this, true);
                }
            }
#endif
        }
        //-----------------------------------------------------
        public bool Deserialize(string content = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(content))
                    JsonUtility.FromJsonOverwrite(content, this);
                Init();
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        internal Dictionary<short, BaseNode> GetNodes()
        {
            return m_vNodes;
        }
        //-----------------------------------------------------
        internal Dictionary<short, IVariable> GetVariableGUIDs()
        {
            if (m_vVariables == null) m_vVariables = new Dictionary<short, IVariable>();
            return m_vVariables;
        }
        //-----------------------------------------------------
        internal void SetVariables(Dictionary<short,IVariable> guidVars)
        {
            if (m_vVariables == null) m_vVariables = new Dictionary<short, IVariable>();
            m_vVariables.Clear();
            foreach (var db in guidVars)
            {
                m_vVariables[db.Key] = db.Value;
            }
        }
        //-----------------------------------------------------
        internal string Serialize(bool toJson)
        {
            varGuids = new VaribaleSerizlizeGuidData();
            if (m_vVariables != null)
            {
                List<VariableBool> vBools = new List<VariableBool>();
                List<VariableInt> vInts = new List<VariableInt>();
                List<VariableFloat> vFloats = new List<VariableFloat>();
                List<VariableString> vStrs = new List<VariableString>();
                foreach (var db in m_vVariables)
                {
                    if (db.Value is VariableBool)
                    {
                        VariableBool temp = (VariableBool)db.Value;
                        temp.guid = db.Key;
                        vBools.Add(temp);
                    }
                    else if (db.Value is VariableInt)
                    {
                        VariableInt temp = (VariableInt)db.Value;
                        temp.guid = db.Key;
                        vInts.Add(temp);
                    }
                    else if (db.Value is VariableFloat)
                    {
                        VariableFloat temp = (VariableFloat)db.Value;
                        temp.guid = db.Key;
                        vFloats.Add(temp);
                    }
                    else if (db.Value is VariableString)
                    {
                        VariableString temp = (VariableString)db.Value;
                        temp.guid = db.Key;
                        vStrs.Add(temp);
                    }
                }
                if (vBools.Count > 0) varGuids.boolVariables = vBools.ToArray();
                if (vInts.Count > 0) varGuids.intVariables = vInts.ToArray();
                if (vFloats.Count > 0) varGuids.floatVariables = vFloats.ToArray();
                if (vStrs.Count > 0) varGuids.stringVariables = vStrs.ToArray();
            }
            if (toJson) return JsonUtility.ToJson(this, true);
            return null;
        }
#endif
    }

}