/********************************************************************
生成日期:	07:03:2025
类    名: 	ConditionExecutor
作    者:	HappLI
描    述:	条件判断执行器
*********************************************************************/
using UnityEngine;
namespace Framework.AT.Runtime
{
    internal class ConditionExecutor
    {
        public static bool OnExecute(AgentTree pAgent, BaseNode pNode)
        {
            int inportCnt = pNode.GetInportCount();
            if (inportCnt != 3)
                return false;
            var inputVarType = pAgent.GetInportVarType(pNode,0);
            if (inputVarType == EVariableType.eNone)
            {
                Debug.LogError("condtion executor input type var error!");
                return false;
            }
            var conditionVarType = pAgent.GetInportVarType(pNode, 1);
            if(conditionVarType != EVariableType.eInt)
            {
                Debug.LogError("condtion op type error!");
                return false;
            }
            var opVarType = pAgent.GetInportVarType(pNode, 2);
            if (opVarType != inputVarType)
            {
                Debug.LogError("condtion op type type var error!");
                return false;
            }
            int nDoSucceed = 0;
            ECompareOpType opType = (ECompareOpType)pAgent.GetInportInt(pNode, 1, 0);
            switch (inputVarType)
            {
                case EVariableType.eBool:
                    {
                        if (opType == ECompareOpType.eEqual)
                        {
                            nDoSucceed = pAgent.GetInportBool(pNode, 0) == pAgent.GetInportBool(pNode, 2)?1:2;
                        }
                        else if (opType == ECompareOpType.eNotEqual) nDoSucceed = pAgent.GetInportBool(pNode, 0) != pAgent.GetInportBool(pNode, 2)?1:2;
                        else return true;
                    }
                    break;
                case EVariableType.eInt:
                    {
                        if (opType == ECompareOpType.eEqual) nDoSucceed = pAgent.GetInportInt(pNode, 0) == pAgent.GetInportInt(pNode, 2) ? 1 : 2;
                        else if (opType == ECompareOpType.eNotEqual) nDoSucceed = pAgent.GetInportInt(pNode, 0) != pAgent.GetInportInt(pNode, 2) ? 1 : 2;
                        else if (opType == ECompareOpType.eGreaterThan) nDoSucceed = pAgent.GetInportInt(pNode, 0) > pAgent.GetInportInt(pNode, 2) ? 1 : 2;
                        else if (opType == ECompareOpType.eGreaterThanOrEqual) nDoSucceed = pAgent.GetInportInt(pNode, 0) >= pAgent.GetInportInt(pNode, 2) ? 1 : 2;
                        else if (opType == ECompareOpType.eLessThan) nDoSucceed = pAgent.GetInportInt(pNode, 0) < pAgent.GetInportInt(pNode, 2) ? 1 : 2;
                        else if (opType == ECompareOpType.eLessThanOrEqual) nDoSucceed = pAgent.GetInportInt(pNode, 0) <= pAgent.GetInportInt(pNode, 2) ? 1 : 2;
                        else if (opType == ECompareOpType.eXor) nDoSucceed = ((pAgent.GetInportInt(pNode, 0) & pAgent.GetInportInt(pNode, 2))!=0) ? 1 : 2;
                        else return true;
                    }
                    break;
                case EVariableType.eFloat:
                    {
                        if (opType == ECompareOpType.eEqual) nDoSucceed = Mathf.Abs(pAgent.GetInportFloat(pNode, 0) - pAgent.GetInportFloat(pNode, 2))<=0.0001f ? 1 : 2;
                        else if (opType == ECompareOpType.eNotEqual) nDoSucceed = Mathf.Abs(pAgent.GetInportFloat(pNode, 0) - pAgent.GetInportFloat(pNode, 2))>0.0001f ? 1 : 2;
                        else if (opType == ECompareOpType.eGreaterThan) nDoSucceed = pAgent.GetInportFloat(pNode, 0) > pAgent.GetInportFloat(pNode, 2) ? 1 : 2;
                        else if (opType == ECompareOpType.eGreaterThanOrEqual) nDoSucceed = pAgent.GetInportFloat(pNode, 0) >= pAgent.GetInportFloat(pNode, 2) ? 1 : 2;
                        else if (opType == ECompareOpType.eLessThan) nDoSucceed = pAgent.GetInportFloat(pNode, 0) < pAgent.GetInportFloat(pNode, 2) ? 1 : 2;
                        else if (opType == ECompareOpType.eLessThanOrEqual) nDoSucceed = pAgent.GetInportFloat(pNode, 0) <= pAgent.GetInportFloat(pNode, 2) ? 1 : 2;
                        else return true;
                    }
                    break;
                case EVariableType.eString:
                    {
                        if (opType == ECompareOpType.eEqual) nDoSucceed = pAgent.GetInportString(pNode, 0).CompareTo(pAgent.GetInportString(pNode, 2)) == 0 ? 1 : 2;
                        else if (opType == ECompareOpType.eNotEqual) nDoSucceed = pAgent.GetInportString(pNode, 0).CompareTo(pAgent.GetInportString(pNode, 2))!=0 ? 1 : 2;
                        else if (opType == ECompareOpType.eContains) nDoSucceed = pAgent.GetInportString(pNode, 0).Contains(pAgent.GetInportString(pNode, 2)) ? 1 : 2;
                        else return true;
                    }
                    break;
                    case EVariableType.eVec2:
                    {
                        if (opType == ECompareOpType.eEqual) nDoSucceed = pAgent.GetInportVec2(pNode, 0).Equals(pAgent.GetInportVec2(pNode, 2)) ? 1 : 2;
                        else if (opType == ECompareOpType.eNotEqual) nDoSucceed = !pAgent.GetInportVec2(pNode, 0).Equals(pAgent.GetInportVec2(pNode, 2)) ? 1 : 2;
                        else return true;
                    }
                    break;
                case EVariableType.eVec3:
                    {
                        if (opType == ECompareOpType.eEqual) nDoSucceed = pAgent.GetInportVec3(pNode, 0).Equals(pAgent.GetInportVec3(pNode, 2)) ? 1 : 2;
                        else if (opType == ECompareOpType.eNotEqual) nDoSucceed = !pAgent.GetInportVec3(pNode, 0).Equals(pAgent.GetInportVec3(pNode, 2)) ? 1 : 2;
                        else return true;
                    }
                    break;
                case EVariableType.eVec4:
                    {
                        if (opType == ECompareOpType.eEqual) nDoSucceed = pAgent.GetInportVec4(pNode, 0).Equals(pAgent.GetInportVec4(pNode, 2)) ? 1 : 2;
                        else if (opType == ECompareOpType.eNotEqual) nDoSucceed = !pAgent.GetInportVec4(pNode, 0).Equals(pAgent.GetInportVec4(pNode, 2)) ? 1 : 2;
                        else return true;
                    }
                    break;
            }
            if(nDoSucceed == 2)
            {
                //! else check false
                var outports = pNode.GetOutports();
                if (outports != null && outports.Length > 0)
                {
                   int failedDoNode =  pAgent.GetOutportInt(pNode, outports.Length - 1);
                    if(failedDoNode!=0)
                    {
                        pAgent.PushDoNode(pNode, (short)failedDoNode);
                    }
                }
            }
            return nDoSucceed == 1;
        }
    }
}