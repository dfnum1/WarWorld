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
    internal class CutsceneCutsceneEventDriver : ACutsceneDriver
    {
        const byte OBJ_LABLE_TYPE = 100;
        public override bool OnEventTrigger(CutsceneTrack pTrack, IBaseEvent pEvt)
        {
            if(!(pEvt is ACutsceneStatusCutsceneEvent))
            {
                return true;
            }
            ACutsceneStatusCutsceneEvent pCutsceneEvent = (ACutsceneStatusCutsceneEvent)pEvt;
            string cutsceneName = pCutsceneEvent.cutsceneName;
            switch (pCutsceneEvent.GetIdType())
            {
                case (ushort)EEventType.ePlayCutscene:
                    {
                        if (string.IsNullOrEmpty(cutsceneName))
                            return true;
#if UNITY_EDITOR
                        if (IsEditorMode())
                        {
                            LoadAsset(cutsceneName, (obj) => {

                                CutsceneObject cutsceneObj = obj as CutsceneObject;
                                if (cutsceneObj)
                                {
                                    var instance = pTrack.GetCutscene().GetCutsceneManager().CreateCutscene(cutsceneObj.GetCutsceneGraph(), cutsceneName, true);
                                    if (instance != null)
                                    {
                                        instance.Play();
                                        pTrack.BindTrackData(new ObjId(instance.GetGUID(), OBJ_LABLE_TYPE));
                                    }
                                }

                            }, false);
                            return true;
                        }
#endif
                        int runtimeId = pTrack.GetCutscene().GetCutsceneManager().CreateCutscene(cutsceneName, -1, false, true);
                        if (runtimeId >= 0)
                        {
                            pTrack.GetCutscene().GetCutsceneManager().PlayCutscene(runtimeId);
                            pTrack.BindTrackData(new ObjId(runtimeId, OBJ_LABLE_TYPE));
                        }
                    }
                    return true;
                case (ushort)EEventType.eStopCutscene:
                case (ushort)EEventType.ePauseCutscene:
                case (ushort)EEventType.eResumeCutscene:
                    {
                        if (string.IsNullOrEmpty(cutsceneName))
                        {
                            var bindData = pTrack.GetBindOutputDatas();
                            if (bindData != null)
                            {
                                foreach (var db in bindData)
                                {
                                    if (db is BindTrackData)
                                    {
                                        var trackData = (BindTrackData)db;
                                        var objs = trackData.outputDatas.GetObjIds();
                                        if (objs != null)
                                        {
                                            foreach (var obj in objs)
                                            {
                                                if (obj.userType == OBJ_LABLE_TYPE)
                                                {
                                                    int runtimeId = obj.id;
                                                    if(pCutsceneEvent.GetIdType() == (ushort)EEventType.eStopCutscene)
                                                        pTrack.GetCutscene().GetCutsceneManager().StopCutscene(runtimeId);
                                                    else if (pCutsceneEvent.GetIdType() == (ushort)EEventType.ePauseCutscene)
                                                        pTrack.GetCutscene().GetCutsceneManager().PauseCutscene(runtimeId);
                                                    else
                                                        pTrack.GetCutscene().GetCutsceneManager().ResumeCutscene(runtimeId);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var cutsceneInstance = pTrack.GetCutscene().GetCutsceneManager().GetCutscene(cutsceneName);
                            if (cutsceneInstance!=null)
                            {
                                if (pCutsceneEvent.GetIdType() == (ushort)EEventType.eStopCutscene)
                                    cutsceneInstance.Stop();
                                else if (pCutsceneEvent.GetIdType() == (ushort)EEventType.ePauseCutscene)
                                    cutsceneInstance.Pause();
                                else
                                    cutsceneInstance.Resume();
                            }
                        }
                    }
                    return true;
            }
          
            return true;
        }
    }
}