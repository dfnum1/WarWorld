/********************************************************************
生成日期:	07:03:2025
类    名: 	CutsceneResumeExecutor
作    者:	HappLI
描    述:	播放子过场动画
*********************************************************************/
namespace Framework.AT.Runtime
{
    internal class CutsceneResumeExecutor
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
            pAgent.GetCutscene().Resume();
            return true;
        }
    }
}