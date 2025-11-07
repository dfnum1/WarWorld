/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneAnimationClip
作    者:	HappLI
描    述:	CutsceneAnimationClip
*********************************************************************/
using Framework.DrawProps;
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("AnimationClip")]
    public class CutsceneAnimationClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp           baseProp;
        [Display("动作剪辑"),StringViewPlugin("OnDrawSelectAnimationInspector")] public string action;                               
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new AnimationDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eAnimation;
        }
        //-----------------------------------------------------
        public float GetDuration()
        {
            return baseProp.duration;
        }
        //-----------------------------------------------------
        public EClipEdgeType GetEndEdgeType()
        {
            return baseProp.endEdgeType;
        }
        //-----------------------------------------------------
        public string GetName()
        {
            return baseProp.name;
        }
        //-----------------------------------------------------
        public ushort GetRepeatCount()
        {
            return baseProp.repeatCnt;
        }
        //-----------------------------------------------------
        public float GetTime()
        {
            return baseProp.time;
        }
        //-----------------------------------------------------
        public float GetBlend(bool bIn)
        {
            return baseProp.GetBlend(bIn);
        }
    }
    //-----------------------------------------------------
    //相机移动驱动逻辑
    //-----------------------------------------------------
    public class AnimationDriver : ACutsceneDriver
    {
        System.Collections.Generic.List<ICutsceneObject> m_vObjects;
        UnityEngine.AnimationClip m_pAnimationClip = null;
        string m_strLoadName = null;
        //-----------------------------------------------------
        public override void OnDestroy()
        {
            if (m_vObjects != null)
            {
                UnityEngine.Pool.ListPool<ICutsceneObject>.Release(m_vObjects);
                m_vObjects = null;
            }
        }
        //-----------------------------------------------------
        void CheckLoad(CutsceneAnimationClip clipData)
        {
            if (m_pAnimationClip == null || clipData.action.CompareTo(m_strLoadName) != 0)
            {
                if (m_pAnimationClip != null) UnloadAsset(m_pAnimationClip);
                m_pAnimationClip = null;
                m_strLoadName = clipData.action;
                LoadAsset(clipData.action, (ani) =>
                {
                    if (ani != null)
                        m_pAnimationClip = ani as UnityEngine.AnimationClip;
                }, false);
            }

        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData clip)
        {
            m_vObjects = pTrack.GetBindAllCutsceneObject(m_vObjects);
            CheckLoad(clip.clip.Cast<CutsceneAnimationClip>());
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
            if (m_vObjects != null)
            {
                foreach (var db in m_vObjects)
                    db.SetParamHold(false);

                UnityEngine.Pool.ListPool<ICutsceneObject>.Release(m_vObjects);
                m_vObjects = null;
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            if (frameData.eStatus == EPlayableStatus.Pause)
                return true;
            var clipData = frameData.clip.Cast<CutsceneAnimationClip>();
#if UNITY_EDITOR
            CheckLoad(clipData);
#endif
            if (m_pAnimationClip == null)
                return true;
            if (m_vObjects != null && m_vObjects.Count>0)
            {
                foreach (var db in m_vObjects)
                {
                    db.SetParamHold(true);
                    var obj = db.GetUniyObject();
                    if (obj == null) continue;
                    if (!(obj is GameObject)) continue;
                    GameObject pGo = obj as GameObject;
                    m_pAnimationClip.SampleAnimation(pGo, frameData.subTime);
                }
            }
            else
            {
                m_vObjects = pTrack.GetBindAllCutsceneObject(m_vObjects);
            }
            return true;
        }
    }
}