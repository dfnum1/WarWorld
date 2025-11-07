/********************************************************************
生成日期:	07:03:2025
类    名: 	VarOpExecutor
作    者:	HappLI
描    述:	+运算符执行器
*********************************************************************/
using UnityEngine;
namespace Framework.AT.Runtime
{
    internal class VarOpExecutor
    {
        public static bool OnExecute(AgentTree pAgent, BaseNode pNode)
        {
            int inportCnt = pNode.GetInportCount();
            int outportCnt = pNode.GetOutportCount();
            if (inportCnt != 3 || outportCnt <= 0)
            {
                Debug.LogError("condtion executor inport or outport count error!");
                return false;
            }
            var inputVarType = pAgent.GetInportVarType(pNode, 0);
            if (inputVarType == EVariableType.eNone)
            {
                Debug.LogError("condtion input var type error!");
                return false;
            }
            var conditionVarType = pAgent.GetInportVarType(pNode, 1);
            if (conditionVarType != EVariableType.eInt)
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
            EOpType opType = (EOpType)pAgent.GetInportInt(pNode, 1, 0);

            switch (inputVarType)
            {
                case EVariableType.eInt:
                    {
                        int a = pAgent.GetInportInt(pNode, 0);
                        int b = pAgent.GetInportInt(pNode, 2);
                        int result = 0;
                        switch (opType)
                        {
                            case EOpType.eAdd: result = a + b; break;
                            case EOpType.eSub: result = a - b; break;
                            case EOpType.eMul: result = a * b; break;
                            case EOpType.eDiv: result = b == 0 ? 0 : a / b; break;
                            default: Debug.LogError("Unsupported EOpType for int"); return false;
                        }
                        pAgent.SetOutportInt(pNode, 0, result);
                        return true;
                    }
                case EVariableType.eFloat:
                    {
                        float a = pAgent.GetInportFloat(pNode, 0);
                        float b = pAgent.GetInportFloat(pNode, 2);
                        float result = 0f;
                        switch (opType)
                        {
                            case EOpType.eAdd: result = a + b; break;
                            case EOpType.eSub: result = a - b; break;
                            case EOpType.eMul: result = a * b; break;
                            case EOpType.eDiv: result = Mathf.Approximately(b, 0f) ? 0f : a / b; break;
                            default: Debug.LogError("Unsupported EOpType for float"); return false;
                        }
                        pAgent.SetOutportFloat(pNode, 0, result);
                        return true;
                    }
                case EVariableType.eVec2:
                    {
                        Vector2 a = pAgent.GetInportVec2(pNode, 0);
                        Vector2 b = pAgent.GetInportVec2(pNode, 2);
                        Vector2 result = Vector2.zero;
                        switch (opType)
                        {
                            case EOpType.eAdd: result = a + b; break;
                            case EOpType.eSub: result = a - b; break;
                            case EOpType.eMul: result = Vector2.Scale(a, b); break;
                            case EOpType.eDiv:
                                result = new Vector2(
                                    Mathf.Approximately(b.x, 0f) ? 0f : a.x / b.x,
                                    Mathf.Approximately(b.y, 0f) ? 0f : a.y / b.y
                                );
                                break;
                            default: Debug.LogError("Unsupported EOpType for Vector2"); return false;
                        }
                        pAgent.SetOutportVec2(pNode, 0, result);
                        return true;
                    }
                case EVariableType.eVec3:
                    {
                        Vector3 a = pAgent.GetInportVec3(pNode, 0);
                        Vector3 b = pAgent.GetInportVec3(pNode, 2);
                        Vector3 result = Vector3.zero;
                        switch (opType)
                        {
                            case EOpType.eAdd: result = a + b; break;
                            case EOpType.eSub: result = a - b; break;
                            case EOpType.eMul: result = Vector3.Scale(a, b); break;
                            case EOpType.eDiv:
                                result = new Vector3(
                                    Mathf.Approximately(b.x, 0f) ? 0f : a.x / b.x,
                                    Mathf.Approximately(b.y, 0f) ? 0f : a.y / b.y,
                                    Mathf.Approximately(b.z, 0f) ? 0f : a.z / b.z
                                );
                                break;
                            default: Debug.LogError("Unsupported EOpType for Vector3"); return false;
                        }
                        pAgent.SetOutportVec3(pNode, 0, result);
                        return true;
                    }
                case EVariableType.eVec4:
                    {
                        Vector4 a = pAgent.GetInportVec4(pNode, 0);
                        Vector4 b = pAgent.GetInportVec4(pNode, 2);
                        Vector4 result = Vector4.zero;
                        switch (opType)
                        {
                            case EOpType.eAdd: result = a + b; break;
                            case EOpType.eSub: result = a - b; break;
                            case EOpType.eMul: result = Vector4.Scale(a, b); break;
                            case EOpType.eDiv:
                                result = new Vector4(
                                    Mathf.Approximately(b.x, 0f) ? 0f : a.x / b.x,
                                    Mathf.Approximately(b.y, 0f) ? 0f : a.y / b.y,
                                    Mathf.Approximately(b.z, 0f) ? 0f : a.z / b.z,
                                    Mathf.Approximately(b.w, 0f) ? 0f : a.w / b.w
                                );
                                break;
                            default: Debug.LogError("Unsupported EOpType for Vector4"); return false;
                        }
                        pAgent.SetOutportVec4(pNode, 0, result);
                        return true;
                    }
                case EVariableType.eString:
                    {
                        string a = pAgent.GetInportString(pNode, 0);
                        string b = pAgent.GetInportString(pNode, 2);
                        string result = "";
                        if (opType == EOpType.eAdd)
                            result = a + b;
                        else
                        {
                            Debug.LogError("String only supports Add (concat) operation.");
                            return false;
                        }
                        pAgent.SetOutportString(pNode, 0, result);
                        return true;
                    }
                default:
                    Debug.LogError("Unsupported variable type for operation.");
                    return false;
            }
        }
    }
}