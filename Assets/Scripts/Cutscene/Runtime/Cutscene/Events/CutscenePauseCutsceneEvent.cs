/********************************************************************
生成日期:	06:30:2025
类    名: 	CutscenePauseCutsceneEvent
作    者:	HappLI
描    述:	暂停剧情
*********************************************************************/
using Framework.AT.Runtime;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneEvent("Cutscene/暂停")]
    public class CutscenePauseCutsceneEvent : ACutsceneStatusCutsceneEvent
    {
        //-----------------------------------------------------
        public override ushort GetIdType()
        {
            return (ushort)EEventType.ePauseCutscene;
        }
    }
}