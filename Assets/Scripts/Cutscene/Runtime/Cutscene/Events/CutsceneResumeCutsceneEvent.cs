/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneResumeCutsceneEvent
作    者:	HappLI
描    述:	恢复继续剧情
*********************************************************************/
using Framework.AT.Runtime;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneEvent("Cutscene/恢复继续")]
    public class CutsceneResumeCutsceneEvent : ACutsceneStatusCutsceneEvent
    {
        //-----------------------------------------------------
        public override ushort GetIdType()
        {
            return (ushort)EEventType.eResumeCutscene;
        }
    }
}