/********************************************************************
生成日期:	07:03:2025
类    名: 	VectorOpExecutor
作    者:	HappLI
描    述:	向量运算执行器
*********************************************************************/
using UnityEngine;
namespace Framework.AT.Runtime
{
    internal class VectorOpExecutor
    {
        public static bool OnExecutor(AgentTree pAgent, BaseNode pNode)
        {
            int inportCnt = pNode.GetInportCount();
            int outportCnt = pNode.GetOutportCount();
            if (inportCnt != 2 || outportCnt <= 0)
            {
                Debug.LogError("condtion executor inport or outport count error!");
                return false;
            }
            var varType1 = pAgent.GetInportVarType(pNode, 0);
            if (varType1 == EVariableType.eNone)
            {
                Debug.LogError("condtion input var type error!");
                return false;
            }
            var varType2 = pAgent.GetInportVarType(pNode, 1);
            if (varType2 != varType1)
            {
                Debug.LogError("condtion op type error!");
                return false;
            }
            switch ((EActionType)pNode.type)
            {
                case EActionType.eDotVariable:
                    {
                        switch (varType1)
                        {
                            case EVariableType.eVec2:
                                {
                                    Vector2 a = pAgent.GetInportVec2(pNode, 0);
                                    Vector2 b = pAgent.GetInportVec2(pNode, 1);
                                    float result = Vector2.Dot(a, b);
                                    pAgent.SetOutportFloat(pNode, 0, result);
                                    return true;
                                }
                            case EVariableType.eVec3:
                                {
                                    Vector3 a = pAgent.GetInportVec3(pNode, 0);
                                    Vector3 b = pAgent.GetInportVec3(pNode, 1);
                                    float result = Vector3.Dot(a, b);
                                    pAgent.SetOutportFloat(pNode, 0, result);
                                    return true;
                                }
                            case EVariableType.eVec4:
                                {
                                    Vector4 a = pAgent.GetInportVec4(pNode, 0);
                                    Vector4 b = pAgent.GetInportVec4(pNode, 1);
                                    float result = Vector4.Dot(a, b);
                                    pAgent.SetOutportFloat(pNode, 0, result);
                                    return true;
                                }
                            default:
                                Debug.LogError("Dot only supports Vec2/Vec3/Vec4");
                                return true;
                        }
                    }
                    break;
                case EActionType.eCrossVariable:
                    {
                        switch (varType1)
                        {
                            case EVariableType.eVec2:
                                {
                                    Vector2 a = pAgent.GetInportVec2(pNode, 0);
                                    Vector2 b = pAgent.GetInportVec2(pNode, 1);
                                    float result = a.x * b.y - a.y * b.x;
                                    pAgent.SetOutportFloat(pNode, 0, result);
                                    return true;
                                }
                            case EVariableType.eVec3:
                                {
                                    Vector3 a = pAgent.GetInportVec3(pNode, 0);
                                    Vector3 b = pAgent.GetInportVec3(pNode, 1);
                                    Vector3 result = Vector3.Cross(a, b);
                                    pAgent.SetOutportVec3(pNode, 0, result);
                                    return true;
                                }
                            default:
                                Debug.LogError("Cross only supports Vec2/Vec3");
                                return true;
                        }
                    }
                    break;
                case EActionType.eDistanceVariable:
                    {
                        switch (varType1)
                        {
                            case EVariableType.eVec2:
                                {
                                    Vector2 a = pAgent.GetInportVec2(pNode, 0);
                                    Vector2 b = pAgent.GetInportVec2(pNode, 1);
                                    float result = Vector2.Distance(a, b);
                                    pAgent.SetOutportFloat(pNode, 0, result);
                                    return true;
                                }
                            case EVariableType.eVec3:
                                {
                                    Vector3 a = pAgent.GetInportVec3(pNode, 0);
                                    Vector3 b = pAgent.GetInportVec3(pNode, 1);
                                    float result = Vector3.Distance(a, b);
                                    pAgent.SetOutportFloat(pNode, 0, result);
                                    return true;
                                }
                            case EVariableType.eVec4:
                                {
                                    Vector4 a = pAgent.GetInportVec4(pNode, 0);
                                    Vector4 b = pAgent.GetInportVec4(pNode, 1);
                                    float result = (a - b).magnitude;
                                    pAgent.SetOutportFloat(pNode, 0, result);
                                    return true;
                                }
                            default:
                                Debug.LogError("Distance only supports Vec2/Vec3/Vec4");
                                return true;
                        }
                    }
                    break;
                case EActionType.eLerp:
                    {
                        switch (varType1)
                        {
                            case EVariableType.eVec2:
                                {
                                    Vector2 a = pAgent.GetInportVec2(pNode, 0);
                                    Vector2 b = pAgent.GetInportVec2(pNode, 1);
                                    Vector2 result = Vector2.Lerp(a, b, Time.deltaTime * pAgent.GetInportFloat(pNode, 2));
                                    pAgent.SetOutportVec2(pNode, 0, result);
                                    return true;
                                }
                            case EVariableType.eVec3:
                                {
                                    Vector3 a = pAgent.GetInportVec3(pNode, 0);
                                    Vector3 b = pAgent.GetInportVec3(pNode, 1);
                                    Vector3 result = Vector3.Lerp(a, b, Time.deltaTime* pAgent.GetInportFloat(pNode, 2));
                                    pAgent.SetOutportVec3(pNode, 0, result);
                                    return true;
                                }
                            case EVariableType.eVec4:
                                {
                                    Vector4 a = pAgent.GetInportVec4(pNode, 0);
                                    Vector4 b = pAgent.GetInportVec4(pNode, 1);
                                    Vector4 result = Vector4.Lerp(a, b, Time.deltaTime * pAgent.GetInportFloat(pNode, 2));
                                    pAgent.SetOutportVec4(pNode, 0, result);
                                    return true;
                                }
                            default:
                                Debug.LogError("Lerp only supports Vec2/Vec3/Vec4");
                                return true;
                        }
                    }
                    break;
                default:
                    Debug.LogError("Unsupported variable type for operation.");
                    return false;
            }
            return true;
        }
    }
}