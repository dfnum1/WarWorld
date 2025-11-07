/********************************************************************
生成日期:	06:30:2025
类    名: 	AgentTreeData
作    者:	HappLI
描    述:	过场动画行为树
*********************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AT.Runtime
{
    //-----------------------------------------------------
    [System.Serializable]
    public class ActionNode : BaseNode
    {
        [SerializeField]internal NodePort[] inPorts;
        [SerializeField]internal NodePort[] outPorts;
#if UNITY_EDITOR
        internal override NodePort[] GetInports(bool bAutoNew = false, int cnt=0)
#else
        internal override NodePort[] GetInports()
#endif
        {
#if UNITY_EDITOR
            if (bAutoNew)
            {
                inPorts= CreatePorts(inPorts, cnt);
            }
#endif
            return inPorts;
        }

#if UNITY_EDITOR
        internal override NodePort[] GetOutports(bool bAutoNew = false, int cnt = 0)
#else
        internal override NodePort[] GetOutports()
#endif
        {
#if UNITY_EDITOR
            if (bAutoNew)
            {
                outPorts = CreatePorts(outPorts, cnt);
            }
#endif
            return outPorts;
        }
        internal override void Init(AgentTreeData pTree)
        {
            if (inPorts!=null)
            {
                for(int i =0; i < inPorts.Length; ++i)
                {
                    inPorts[i].pVariable = pTree.GetVariable(inPorts[i].varGuid);
                }
            }
            if (outPorts != null)
            {
                for (int i = 0; i < outPorts.Length; ++i)
                {
                    outPorts[i].pVariable = pTree.GetVariable(outPorts[i].varGuid);
                }
            }
        }
    }
}