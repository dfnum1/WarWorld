/********************************************************************
生成日期:	07:03:2025
类    名: 	CutscenePlayExecutor
作    者:	HappLI
描    述:	播放子过场动画
*********************************************************************/
namespace Framework.AT.Runtime
{
    internal class CutscenePlayExecutor
    {
        static internal bool OnExecute(AgentTree pAgent, BaseNode pNode)
        {
            if (pAgent.GetCutscene() == null)
                return false;
            int inportCnt = pNode.GetInportCount();
            if (inportCnt <= 0)
            {
                return false;
            }
            int subsceneId = pAgent.GetInportInt(pNode, 0);
            if (pAgent.GetCutscene().GetCutsceneId() == subsceneId)
            {
                pAgent.GetCutscene().Play();
                return true;
            }
            int instanceId = pAgent.GetCutscene().CreatePlayable((short)subsceneId);
            pAgent.SetOutportInt(pNode, 0, instanceId);
            return true;
        }
    }
    internal class CutsceneSeekExecutor
    {
        static internal bool OnExecute(AgentTree pAgent, BaseNode pNode)
        {
            if (pAgent.GetCutscene() == null)
                return false;
            int inportCnt = pNode.GetInportCount();
            if (inportCnt <= 2)
            {
                return false;
            }
            float time = pAgent.GetInportFloat(pNode, 1);
            pAgent.GetCutscene().SetTime(time);
            return true;
        }
    }
}