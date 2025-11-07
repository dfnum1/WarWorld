/********************************************************************
生成日期:	07:03:2025
类    名: 	CutsceneBindTrackExecutor
作    者:	HappLI
描    述:	轨道绑定
*********************************************************************/
namespace Framework.AT.Runtime
{
    internal class CutsceneBindTrackExecutor
    {
        static internal bool OnExecute(AgentTree pAgent, BaseNode pNode)
        {
            if (pAgent.GetCutscene() == null)
                return false;
            int inportCnt = pNode.GetInportCount();
            if (inportCnt != 3)
                return false;

            int instanceId = pAgent.GetInportInt(pNode, 0);
            int cutsceneAndGroupId = pAgent.GetInportInt(pNode, 1);

            int groupId = cutsceneAndGroupId & 0xFFFF; // 低16位为组ID
            int cutsceneId = cutsceneAndGroupId >> 16; // 高16位为过场动画ID

            if (pAgent.GetCutscene().GetCutsceneId() != cutsceneId)
                return true;

            var varType = pAgent.GetInportVarType(pNode, 2);
            switch (varType)
            {
                case EVariableType.eBool:
                    pAgent.GetCutscene().BindGroupTrackData((ushort)groupId, pAgent.GetInportBool(pNode, 2));
                    break;
                case EVariableType.eInt:
                    pAgent.GetCutscene().BindGroupTrackData((ushort)groupId, pAgent.GetInportInt(pNode, 2));
                    break;
                case EVariableType.eFloat:
                    pAgent.GetCutscene().BindGroupTrackData((ushort)groupId, pAgent.GetInportFloat(pNode, 2));
                    break;
                case EVariableType.eVec2:
                    pAgent.GetCutscene().BindGroupTrackData((ushort)groupId, pAgent.GetInportVec2(pNode, 2));
                    break;
                case EVariableType.eVec3:
                    pAgent.GetCutscene().BindGroupTrackData((ushort)groupId, pAgent.GetInportVec3(pNode, 2));
                    break;
                case EVariableType.eVec4:
                    pAgent.GetCutscene().BindGroupTrackData((ushort)groupId, pAgent.GetInportVec4(pNode, 2));
                    break;
                case EVariableType.eString:
                    pAgent.GetCutscene().BindGroupTrackData((ushort)groupId, pAgent.GetInportString(pNode, 2));
                    break;
                case EVariableType.eObjId:
                    pAgent.GetCutscene().BindGroupTrackData((ushort)groupId, pAgent.GetInportObjId(pNode, 2));
                    break;
                default:
                    UnityEngine.Debug.LogError($"CutsceneBindTrackExecutor: Unsupported variable type {varType} for binding track data.");
                    break;
            }
            return true;
        }
    }
}