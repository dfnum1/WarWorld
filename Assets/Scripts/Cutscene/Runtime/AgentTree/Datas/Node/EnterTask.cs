/********************************************************************
生成日期:	06:30:2025
类    名: 	EnterTask
作    者:	HappLI
描    述:	任务回调类节点
*********************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AT.Runtime
{
    //-----------------------------------------------------
    [System.Serializable]
    public class EnterTask : BaseNode
    {
        [SerializeField] internal NodePort[] argvs;
        public override bool IsTask() { return true; }
#if UNITY_EDITOR
        internal override NodePort[] GetInports(bool bAutoNew = false, int cnt = 0)
#else
        internal override NodePort[] GetInports()
#endif
        {
            return null;
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
                argvs = CreatePorts(argvs, cnt);
            }
#endif
            return argvs;
        }
        internal override void Init(AgentTreeData pTree)
        {
            if (argvs != null)
            {
                for (int i = 0; i < argvs.Length; ++i)
                {
                    argvs[i].pVariable = pTree.GetVariable(argvs[i].varGuid);
                }
            }
        }
    }
}