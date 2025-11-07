/********************************************************************
生成日期:	06:30:2025
类    名: 	ActiveClip
作    者:	HappLI
描    述:	激活clip
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
#endif
using Framework.DrawProps;
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("ActiveClip")]
    public struct ActiveClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp baseProp;
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new ActiveDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eActive;
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
    //Active剪辑驱动类
    //-----------------------------------------------------
    public class ActiveDriver : ACutsceneDriver
    {
        System.Collections.Generic.List<ICutsceneObject> m_vObjects;
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
        public override bool OnCreateClip(CutsceneTrack pTrack, IBaseClip clip)
        {
            m_vObjects = pTrack.GetBindAllCutsceneObject(m_vObjects);
            if (m_vObjects != null)
            {
                var clipData = clip.Cast<ActiveClip>();
                foreach (var db in m_vObjects)
                {
                    var objectUnity = db.GetUniyObject();
                    if (objectUnity == null || !(objectUnity is GameObject)) continue;
                    ((GameObject)objectUnity).SetActive(false);
                }
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnUpdateClip(CutsceneTrack pTrack, FrameData frameData)
        {
            if (m_vObjects == null)
                m_vObjects = pTrack.GetBindAllCutsceneObject(m_vObjects);
            if (m_vObjects != null)
            {
                var clipData = frameData.clip.Cast<ActiveClip>();
                foreach (var db in m_vObjects)
                {
                    var objectUnity = db.GetUniyObject();
                    if (objectUnity == null || !(objectUnity is GameObject)) continue;
                    ((GameObject)objectUnity).SetActive(frameData.clipStatus == EDriverStatus.Framing);
                }
            }
            return base.OnUpdateClip(pTrack, frameData);
        }
    }
}