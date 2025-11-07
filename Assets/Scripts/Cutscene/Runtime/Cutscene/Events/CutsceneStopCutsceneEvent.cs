/********************************************************************
生成日期:	06:30:2025
类    名: 	CutscenePlayCutsceneEvent
作    者:	HappLI
描    述:	执行行为树节点
*********************************************************************/
using Framework.AT.Runtime;
using Framework.DrawProps;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneEvent("Cutscene/停止")]
    public class CutsceneStopCutsceneEvent : ACutsceneStatusCutsceneEvent
    {
        //-----------------------------------------------------
        public override ushort GetIdType()
        {
            return (ushort)EEventType.eStopCutscene;
        }
    }
}