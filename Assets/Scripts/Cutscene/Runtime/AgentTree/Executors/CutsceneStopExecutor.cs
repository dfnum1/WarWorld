/********************************************************************
生成日期:	07:03:2025
类    名: 	CutsceneStopExecutor
作    者:	HappLI
描    述:	停止过场动画
*********************************************************************/
namespace Framework.AT.Runtime
{
    internal class CutsceneStopExecutor
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
            int instanceId = pAgent.GetInportInt(pNode, 0);
            pAgent.GetCutscene().Stop();
            return true;
        }
    }
}