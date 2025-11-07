/********************************************************************
生成日期:	06:30:2025
类    名: 	BaseNode
作    者:	HappLI
描    述:	行为树基础节点类
*********************************************************************/
using UnityEngine;

namespace Framework.AT.Runtime
{
    //-----------------------------------------------------
    public enum EOpType : byte
    {
        [InspectorName("加")] eAdd,
        [InspectorName("减")] eSub,
        [InspectorName("乘")] eMul,
        [InspectorName("除")] eDiv,
    }
    //-----------------------------------------------------
    public enum ECompareOpType : byte
    {
        [InspectorName("==")]eEqual,//等于
        [InspectorName("!=")] eNotEqual,//不等于
        [InspectorName(">")] eGreaterThan,//大于
        [InspectorName(">=")] eGreaterThanOrEqual,//大于等于
        [InspectorName("<")] eLessThan,//小于
        [InspectorName("<=")] eLessThanOrEqual,//小于等于
        [InspectorName("异或")] eXor,//异或
        [InspectorName("包含")] eContains,//包含
    }
    //-----------------------------------------------------
    [System.Serializable]
    public struct DummyPort
    {
        public short guid; // Unique identifier for the port
        public byte type; // 0: input, 1: output
        public byte slotIndex;

        [System.NonSerialized] internal BaseNode pNode;
        public static DummyPort DEF = new DummyPort() { guid = 0 };
        public bool IsValid()
        {
            return guid != 0 && pNode!=null;
        }
    }
    //-----------------------------------------------------
    [System.Serializable]
    public struct NodePort
    {
        public short varGuid; // Variable GUID, if applicable

        public DummyPort[] dummyPorts;

        [System.NonSerialized] internal IVariable pVariable;
    }
    //-----------------------------------------------------
    [System.Serializable]
    public abstract class BaseNode
    {
        [SerializeField]internal short guid;
        public int type;
        [SerializeField] internal short[] nextActions;

        [System.NonSerialized]BaseNode[] m_vNextNodes;
#if UNITY_EDITOR
        [SerializeField] internal float posX;
        [SerializeField] internal float posY;
#endif
        public bool IsValid()
        {
            return type > 0;
        }
        public virtual bool IsTask() { return false; }
        public int GetInportCount()
        {
            var ports = GetInports();
            return ports!=null? ports.Length:0;
        }
        public int GetOutportCount()
        {
            var ports = GetOutports();
            return ports != null ? ports.Length : 0;
        }
        internal BaseNode[] GetNexts(AgentTreeData pData, bool bReGet = false)
        {
#if UNITY_EDITOR
            if(bReGet)
            {
                m_vNextNodes = null;
            }
#endif
            if (m_vNextNodes != null) return m_vNextNodes;
            if(nextActions!=null && nextActions.Length>0)
            {
                m_vNextNodes = new BaseNode[nextActions.Length];
                for(int i =0; i < nextActions.Length; ++i)
                {
                    m_vNextNodes[i] = pData.GetNode(nextActions[i]);
                }
            }
            return m_vNextNodes;
        }
#if UNITY_EDITOR
        internal abstract NodePort[] GetInports(bool bAutoNew = false, int cnt = 0);
        internal abstract NodePort[] GetOutports(bool bAutoNew = false, int cnt =0);
#else
        internal abstract NodePort[] GetInports();
        internal abstract NodePort[] GetOutports();
#endif
        internal abstract void Init(AgentTreeData pTree);
#if UNITY_EDITOR
        internal NodePort[] CreatePorts(NodePort[] lastPorts, int portCnt)
        {
            if (portCnt <= 0)
                return  null;
            else
            {
                if (lastPorts == null)
                    return new NodePort[portCnt];
                else
                {
                    if (lastPorts.Length != portCnt)
                    {
                        NodePort[] temPorts = new NodePort[portCnt];
                        for (int i = 0; i < lastPorts.Length && i < portCnt; ++i)
                        {
                            temPorts[i] = lastPorts[i];
                        }
                        return temPorts;
                    }
                    else
                        return lastPorts;
                }
            }
        }
#endif
    }
}