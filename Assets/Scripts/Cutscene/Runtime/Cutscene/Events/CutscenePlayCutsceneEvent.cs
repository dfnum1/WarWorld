/********************************************************************
生成日期:	06:30:2025
类    名: 	CutscenePlayCutsceneEvent
作    者:	HappLI
描    述:	播放剧情
*********************************************************************/
using Framework.AT.Runtime;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneEvent("Cutscene/播放")]
    public class CutscenePlayCutsceneEvent : ACutsceneStatusCutsceneEvent
    {
        //-----------------------------------------------------
        public override ushort GetIdType()
        {
            return (ushort)EEventType.ePlayCutscene;
        }
    }
}