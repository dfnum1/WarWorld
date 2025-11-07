/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneTrack
作    者:	HappLI
描    述:	过场动画轨道类
*********************************************************************/
using Framework.AT.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    //-----------------------------------------------------
    public enum EDriverStatus : byte
    {
        None = 0,
        Enter,
        Framing,
        Leave,
        Destroyed,
    }
    //-----------------------------------------------------
    public struct BindTrackData : ICutsceneData
    {
        public IDataer dater;
        public VariableList outputDatas;
    }
    //-----------------------------------------------------
    public class CutsceneTrack
    {
		//-----------------------------------------------------
		//! Clip 数据
		//-----------------------------------------------------
		struct ClipData
		{
            public CutsceneData.Group ownerGroup;
			public EDriverStatus eStatus;
			public IBaseClip clipData;
			public ACutsceneDriver pDriver;
        }
        //-----------------------------------------------------
        //! Event 数据
        //-----------------------------------------------------
        struct EventData
		{
            public CutsceneData.Group ownerGroup;
            public bool bTriggered;
			public IBaseEvent eventData;
            public ACutsceneDriver pDriver;
        }
        //-----------------------------------------------------
        private string                          m_strName = null;
        private List<ClipData>                  m_vClips = null;
		private List<EventData>                 m_vEvents = null;
		CutscenePlayable                        m_pOwner = null;
        CutsceneData.Group                      m_pOwnerGroup = null;
        CutsceneData.Track                      m_pOwnerTrack = null;
        //-----------------------------------------------------
        internal CutsceneTrack()
        {
        }
        //-----------------------------------------------------
        public bool Create(CutscenePlayable playeable, CutsceneData.Group pGroup, CutsceneData.Track pData)
		{
			Clear();
			m_pOwner = playeable;
            m_pOwnerGroup = pGroup;
            m_pOwnerTrack = pData;
            if (pData == null)
				return false;
            m_strName = pData.trackName;

            if (pData.clips != null)
			{
				m_vClips = new List<ClipData>(pData.clips.Count);
				for(int i =0; i < pData.clips.Count; ++i)
				{
                    AddClipData(pGroup, pData, pData.clips[i], false);
                }
                m_vClips.Sort((clip1, clip2) =>
                {
                    return clip1.clipData.GetTime() < clip2.clipData.GetTime() ? -1:1;
                });
            }
			if(pData.events != null)
			{
				m_vEvents = new List<EventData>(pData.events.Count);
				for(int i =0; i < pData.events.Count; ++i)
				{
                    AddEventData(pGroup, pData, pData.events[i]);
                }
			}
            if(m_pOwnerGroup!=null && m_pOwnerGroup.binderId!=0)
            {
                BindTrackData(new ObjId(m_pOwnerGroup.binderId), ObjectBinderUtils.GetBinder(m_pOwnerGroup.binderId));
            }
			return (m_vClips !=null && m_vClips.Count > 0) ||
				   (m_vEvents != null && m_vEvents.Count > 0);
        }
        //-----------------------------------------------------
        internal bool AddEventData(CutsceneData.Group pGroup, CutsceneData.Track pTrack, IBaseEvent pDater)
        {
            if (m_pOwner == null)
                return false;
            if (pGroup != m_pOwnerGroup)
                return false;
            if (pTrack != m_pOwnerTrack)
                return false;

            EventData eventData = new EventData();
            eventData.ownerGroup = pGroup;
            eventData.bTriggered = false;
            eventData.eventData = pDater;
            eventData.pDriver = m_pOwner.CreateDriver(EDataType.eEvent, eventData.eventData);
            if (m_vEvents == null) m_vEvents = new List<EventData>(2);
            m_vEvents.Add(eventData);
            return true;
        }
        //-----------------------------------------------------
        internal bool AddClipData(CutsceneData.Group pGroup, CutsceneData.Track pTrack, IBaseClip pDater, bool bSort = true)
        {
            if (m_pOwner == null)
                return false;
            if (pGroup != m_pOwnerGroup)
                return false;
            if (pTrack != m_pOwnerTrack)
                return false;

            ClipData clipData = new ClipData();
            clipData.ownerGroup = pGroup;
            clipData.eStatus = EDriverStatus.None;
            clipData.clipData = pDater;
            clipData.pDriver = m_pOwner.CreateDriver(EDataType.eClip, clipData.clipData);

            if (m_vClips == null) m_vClips = new List<ClipData>(2);
            m_vClips.Add(clipData);
            if (clipData.pDriver == null || !clipData.pDriver.OnCreateClip(this, clipData.clipData))
                OnCreateClip(clipData.clipData);

            if (bSort)
            {
                m_vClips.Sort((clip1, clip2) =>
                {
                    return clip1.clipData.GetTime() < clip2.clipData.GetTime() ? -1 : 1;
                });
            }
            return true;
        }
        //-----------------------------------------------------
        internal bool RemoveDataer(CutsceneData.Group pGroup, CutsceneData.Track pTrack, IDataer pDater)
        {
            if (pGroup != m_pOwnerGroup)
                return false;
            if (pTrack != m_pOwnerTrack)
                return false;

            if(pDater is IBaseEvent)
            {
                if (m_vEvents == null) return false;
                for(int i =0; i < m_vEvents.Count; ++i)
                {
                    //! value ref ,check failed
                    if (m_vEvents[i].eventData == pDater)
                    {
                        DestroyEvent(m_vEvents[i]);
                        m_vEvents.RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }
            if (pDater is IBaseClip)
            {
                if (m_vClips == null) return false;
                for (int i = 0; i < m_vClips.Count; ++i)
                {
                    var clipData = m_vClips[i];
                    //! value ref ,check failed
                    if (clipData.clipData == pDater)
                    {
                        DestroyClip(ref clipData);
                        m_vClips.RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        //-----------------------------------------------------
        public string GetName()
        {
            return m_strName;
        }
        //-----------------------------------------------------
        public string GetGroupName()
        {
            if (m_pOwnerGroup == null) return null;
            return m_pOwnerGroup.name;
        }
        //-----------------------------------------------------
        internal CutsceneData.Track GetTrackData()
        {
            return m_pOwnerTrack;
        }
        //-----------------------------------------------------
        internal CutsceneData.Group GetGroupData()
        {
            return m_pOwnerGroup;
        }
        //-----------------------------------------------------
        public CutscenePlayable GetPlayable()
        {
            return m_pOwner;
        }
        //-----------------------------------------------------
        public ushort GetGroupId()
        {
            if (m_pOwnerGroup == null) return ushort.MaxValue;
            return m_pOwnerGroup.id;
        }
        //-----------------------------------------------------
        public CutsceneInstance GetCutscene()
        {
            return m_pOwner != null ? m_pOwner.GetCutscene() : null;
        }
        //-----------------------------------------------------
        internal void CollectDrivers(List<CacheTrackDriver> vLists, IDataer pDater = null, System.Type driverType = null)
        {
            if (vLists == null) return;
            if (m_vClips != null)
            {
                for (int i = 0; i < m_vClips.Count; ++i)
                {
                    var clip = m_vClips[i];
                    if (clip.pDriver == null) continue;
                    if(driverType != null && clip.pDriver.GetType() == driverType)
                        vLists.Add(new CacheTrackDriver() { pTrack = this, pDriver =  clip.pDriver, pDater = clip.clipData });
                    else if (pDater != null && clip.clipData == pDater)
                        vLists.Add(new CacheTrackDriver() { pTrack = this, pDriver = clip.pDriver, pDater = clip.clipData });
                    else if (driverType == null && pDater == null)
                        vLists.Add(new CacheTrackDriver() { pTrack = this, pDriver = clip.pDriver, pDater = clip.clipData });
                }
            }
            if (m_vEvents != null)
            {
                for (int i = 0; i < m_vEvents.Count; ++i)
                {
                    var clip = m_vEvents[i];
                    if (clip.pDriver == null) continue;
                    if (driverType != null && clip.pDriver.GetType() == driverType)
                        vLists.Add(new CacheTrackDriver() { pTrack = this, pDriver = clip.pDriver, pDater = clip.eventData });
                    else if (pDater != null && clip.eventData == pDater)
                        vLists.Add(new CacheTrackDriver() { pTrack = this, pDriver = clip.pDriver, pDater = clip.eventData });
                    else if (driverType == null && pDater == null)
                        vLists.Add(new CacheTrackDriver() { pTrack = this, pDriver = clip.pDriver, pDater = clip.eventData });
                }
            }
        }
        //-----------------------------------------------------
        public List<CacheTrackDriver> GetCacheTrackDriversByData(IDataer pDataer = null)
        {
            if (m_pOwner == null) return null;
            return m_pOwner.GetDrivers(pDataer);
        }
        //-----------------------------------------------------
        public List<CacheTrackDriver> GetCacheTrackDriversByType(System.Type driverType)
        {
            if (m_pOwner == null) return null;
            return m_pOwner.GetDrivers(driverType);
        }
        //-----------------------------------------------------
        public List<ACutsceneDriver> GetDrivers(IDataer pDataer = null)
        {
            int cnt = 0;
            if (m_vClips != null) cnt += m_vClips.Count;
            if (m_vEvents != null) cnt += m_vEvents.Count;
            if (cnt <= 0)
                return null;

            var drivers = CutscenePool.CacheDrivers;

            if (m_vClips!=null)
            {
                for (int i = 0; i < m_vClips.Count; ++i)
                {
                    var clip = m_vClips[i];
                    if (clip.pDriver == null) continue;
                    if(pDataer == null || clip.clipData == pDataer)
                    {
                        drivers.Add(clip.pDriver);
                    }
                }
            }
            if (m_vEvents != null)
            {
                for (int i = 0; i < m_vEvents.Count; ++i)
                {
                    var clip = m_vEvents[i];
                    if (clip.pDriver == null) continue;
                    if (pDataer == null || clip.eventData == pDataer)
                    {
                        drivers.Add(clip.pDriver);
                    }
                }
            }
            return drivers;
        }
        //-----------------------------------------------------
        public List<ACutsceneDriver> GetDriversByType(System.Type driverType)
        {
            int cnt = 0;
            if (m_vClips != null) cnt += m_vClips.Count;
            if (m_vEvents != null) cnt += m_vEvents.Count;
            if (cnt <= 0)
                return null;

            var drivers = CutscenePool.CacheDrivers;
            if (m_vClips != null)
            {
                for (int i = 0; i < m_vClips.Count; ++i)
                {
                    var clip = m_vClips[i];
                    if (clip.pDriver == null) continue;
                    if (clip.pDriver.GetType() == driverType)
                    {
                        drivers.Add(clip.pDriver);
                    }
                }
            }
            if (m_vEvents != null)
            {
                for (int i = 0; i < m_vEvents.Count; ++i)
                {
                    var clip = m_vEvents[i];
                    if (clip.pDriver == null) continue;
                    if (clip.pDriver.GetType() == driverType)
                    {
                        drivers.Add(clip.pDriver);
                    }
                }
            }
            return drivers;
        }
        //-----------------------------------------------------
        public void SetObject(ObjId objId, ICutsceneObject pObject, bool bAutoDestroy = false)
        {
            if (m_pOwner == null) return;
            m_pOwner.SetObject(objId, pObject, bAutoDestroy);
        }
        //-----------------------------------------------------
        public void RemoveObject(ObjId objId)
        {
            if (m_pOwner == null) return;
            m_pOwner.RemoveObject(objId);
        }
        //-----------------------------------------------------
        public void RemoveObject(ICutsceneObject obj)
        {
            if (m_pOwner == null) return;
            m_pOwner.RemoveObject(obj);
        }
        //-----------------------------------------------------
        public ICutsceneObject GetObject(ObjId objId)
        {
            if (m_pOwner == null)
                return null;
            return m_pOwner.GetObject(objId);
        }
        //-----------------------------------------------------
        public ICutsceneObject GetObject(VariableObjId objId)
        {
            if (m_pOwner == null)
                return null;
            return m_pOwner.GetObject(objId);
        }
        //-----------------------------------------------------
        public void BindTrackData<T>(T pData) where T : struct
        {
            if (m_pOwnerGroup == null || m_pOwner == null)
                return;
            m_pOwner.BindTrackData(m_pOwnerGroup, pData);
        }
        //-----------------------------------------------------
        public void BindTrackData(ObjId objId, ICutsceneObject pObj, bool bAutoDestroy = false)
        {
            if (m_pOwnerGroup == null || m_pOwner == null)
                return;
            m_pOwner.BindTrackData(m_pOwnerGroup, objId);
            SetObject(objId, pObj, bAutoDestroy);
        }
        //-----------------------------------------------------
        public void BindTrackData(string strData)
        {
            if (m_pOwnerGroup == null || m_pOwner == null)
                return;
            m_pOwner.BindTrackData(m_pOwnerGroup, strData);
        }
        //-----------------------------------------------------
        internal void StopTrigger()
        {
            if (m_vEvents == null) return;
            for (int i = 0; i < m_vEvents.Count; ++i)
            {
                var clipData = m_vEvents[i];
                if (!clipData.bTriggered)
                {
                    clipData.bTriggered = true;
                    m_vEvents[i] = clipData;
                    if (clipData.pDriver == null || !clipData.pDriver.OnEventTrigger(this, clipData.eventData))
                        OnEventTrigger(clipData.eventData);
                    if (m_pOwner != null)
                        m_pOwner.BindEventTrackData(m_pOwnerGroup, clipData.eventData);
                }
            }
        }
        //-----------------------------------------------------
        internal bool Update(ref FrameData frameData)
		{
			bool bOver = true;
            frameData.isBlending = false;
            frameData.blendFactor = 0.0f;
            frameData.blendTime = 0.0f;

            frameData.subTime = 0.0f;
            frameData.ownerTrack = this;
            if (m_vEvents != null)
            {
                for (int i = 0; i < m_vEvents.Count; ++i)
                {
                    EventData eventData = m_vEvents[i];
                    if (!m_pOwner.CanPlayable(EDataType.eEvent, eventData.eventData))
                        continue;
                    if (eventData.bTriggered)
                        continue;
                    bOver = false;
                    float beginTime = eventData.eventData.GetTime();
                    if (beginTime > frameData.curTime)
                        continue;

                    if (!eventData.bTriggered)
                    {
                        eventData.bTriggered = true;
                        if (eventData.pDriver == null || !eventData.pDriver.OnEventTrigger(this, eventData.eventData))
                            OnEventTrigger(eventData.eventData);
                        if(m_pOwner!=null)
                            m_pOwner.BindEventTrackData(m_pOwnerGroup, eventData.eventData);
                        m_vEvents[i] = eventData;
                    }
                }
            }
            if (m_vClips != null)
			{
                var cacheIndexs = CutscenePool.GetCacheIndexs();
                for (int i = 0; i < m_vClips.Count; ++i)
				{
					ClipData clipData = m_vClips[i];
					if (clipData.clipData == null)
						continue;

                    if (!m_pOwner.CanPlayable(EDataType.eClip, clipData.clipData))
                        continue;

                    if (clipData.eStatus != EDriverStatus.Leave)
						bOver = false;

                    bool bAddCacheIndex = false;
                    frameData.clip = clipData.clipData;
                    frameData.clipStatus = clipData.eStatus;
                    float beginTime = clipData.clipData.GetTime();
					float endTime = beginTime + clipData.clipData.GetDuration();
                    switch (clipData.eStatus)
					{
						case EDriverStatus.None:
							{
								if(frameData.curTime >= beginTime && frameData.curTime <= endTime)
								{
									clipData.eStatus = EDriverStatus.Enter;
                                    frameData.clipStatus = clipData.eStatus;
                                    CalculateClipBlend(i-1,i,i+1,ref frameData);
                                    if (clipData.pDriver == null || !clipData.pDriver.OnClipEnter(this, frameData))
                                        OnFrameClipEnter(ref frameData);
                                }
                            }
							break;
                        case EDriverStatus.Enter:
                            {
                                clipData.eStatus = EDriverStatus.Framing;
                                frameData.clipStatus = clipData.eStatus;
                                UpdateFreming(ref frameData, ref clipData, i);
                            }
                            break;
                        case EDriverStatus.Framing:
                            {
                                UpdateFreming(ref frameData, ref clipData, i);
                                if (!frameData.isBlending && clipData.eStatus == EDriverStatus.Framing && 
                                    (clipData.clipData.GetEndEdgeType() == EClipEdgeType.KeepClamp || clipData.clipData.GetEndEdgeType() == EClipEdgeType.KeepState))
                                    bAddCacheIndex = true;
                            }
                            break;
                        case EDriverStatus.Leave:
                            {
                                if (frameData.curTime >= beginTime && frameData.curTime <= endTime)
                                {
                                    clipData.eStatus = EDriverStatus.Enter;
                                    frameData.clipStatus = clipData.eStatus;
                                    CalculateClipBlend(i - 1, i, i + 1, ref frameData);
                                    if (clipData.pDriver == null || !clipData.pDriver.OnClipEnter(this, frameData))
                                        OnFrameClipEnter(ref frameData);
                                }
                            }
                            break;
                    }
                    if(clipData.eStatus == EDriverStatus.Framing && cacheIndexs.Count>0)
                    {
                        long curKey = CutscenePool.GetDaterKey(EDataType.eClip, clipData.clipData);
                        for (int j = 0; j < cacheIndexs.Count;)
                        {
                            int index = cacheIndexs[j];
                            if (index >= 0 && index < m_vClips.Count)
                            {
                                var lastClip = m_vClips[index];
                                long lastKey = CutscenePool.GetDaterKey(EDataType.eClip, lastClip.clipData);
                                if (curKey == lastKey)
                                {
                                    lastClip.eStatus = EDriverStatus.Leave;
                                    m_vClips[index] = lastClip;
                                    cacheIndexs.RemoveAt(j);
                                }
                                else ++j;
                            }
                            else ++j;
                        }
                    }
                    if (bAddCacheIndex) cacheIndexs.Add(i);

                    frameData.clipStatus = clipData.eStatus;
                    if (clipData.pDriver == null || !clipData.pDriver.OnUpdateClip(this, frameData))
                        OnUpdateClip(ref frameData);

                    m_vClips[i] = clipData;
                }
                cacheIndexs.Clear();
            }
            return bOver;
        }
        //-----------------------------------------------------
		void UpdateFreming(ref FrameData frameData, ref ClipData clipData, int index)
		{
            float beginTime = clipData.clipData.GetTime();
            float endTime = beginTime + clipData.clipData.GetDuration();
            float duration = clipData.clipData.GetDuration();
            frameData.clipStatus = clipData.eStatus;
            CalculateClipBlend(index-1,index, index+1, ref frameData);
            if (clipData.clipData.GetEndEdgeType() == EClipEdgeType.KeepClamp)
            {
                frameData.clip = clipData.clipData;
                if (frameData.curTime < beginTime)
                {
                    clipData.eStatus = EDriverStatus.Leave;
                    frameData.clipStatus = clipData.eStatus;
                    if (clipData.pDriver == null || !clipData.pDriver.OnClipLeave(this, frameData))
                        OnFrameClipLeave(ref frameData);
                }
                else
                {
                    frameData.subTime = frameData.curTime - beginTime;
                    if (clipData.pDriver == null || !clipData.pDriver.OnFrameClip(this, frameData))
                        OnFrameClip(ref frameData);
                }
            }
            else if (clipData.clipData.GetEndEdgeType() == EClipEdgeType.KeepState)
            {
                frameData.clip = clipData.clipData;
                if (frameData.curTime < beginTime)
                {
                    clipData.eStatus = EDriverStatus.Leave;
                    frameData.clipStatus = clipData.eStatus;
                    if (clipData.pDriver == null || !clipData.pDriver.OnClipLeave(this, frameData))
                        OnFrameClipLeave(ref frameData);
                }
                else if (frameData.curTime <= endTime)
                {
                    frameData.subTime = frameData.curTime - beginTime;
                    if (clipData.pDriver == null || !clipData.pDriver.OnFrameClip(this, frameData))
                        OnFrameClip(ref frameData);
                }
            }
            else if (clipData.clipData.GetEndEdgeType() == EClipEdgeType.Repeat)
            {
                float elapsed = frameData.curTime - beginTime;
                if (elapsed < 0.0f)
                {
                    clipData.eStatus = EDriverStatus.Leave;
                    frameData.clipStatus = clipData.eStatus;
                    if (clipData.pDriver == null || !clipData.pDriver.OnClipLeave(this, frameData))
                        OnFrameClipLeave(ref frameData);
                }
                else if (duration > 0)
                {
                    int curRepeat = (int)(elapsed / duration);
                    if (curRepeat < clipData.clipData.GetRepeatCount())
                    {
                        float loopTime = (frameData.curTime - beginTime) % duration;
                        frameData.clip = clipData.clipData;
                        frameData.subTime = loopTime;
                        if (clipData.pDriver == null || !clipData.pDriver.OnFrameClip(this, frameData))
                            OnFrameClip(ref frameData);
                    }
                    else
                    {
                        clipData.eStatus = EDriverStatus.Leave;
                        frameData.clipStatus = clipData.eStatus;
                        if (clipData.pDriver == null || !clipData.pDriver.OnClipLeave(this, frameData))
                            OnFrameClipLeave(ref frameData);
                    }
                }
                else
                {
                    frameData.clip = clipData.clipData;
                    frameData.subTime = frameData.curTime - beginTime;
                    if (clipData.pDriver == null || !clipData.pDriver.OnFrameClip(this, frameData))
                        OnFrameClip(ref frameData);
                }
            }
            else
            {
                if (frameData.curTime < beginTime || frameData.curTime >= endTime)
                {
                    clipData.eStatus = EDriverStatus.Leave;
                    frameData.clipStatus = clipData.eStatus;
                    if (clipData.pDriver == null || !clipData.pDriver.OnClipLeave(this, frameData))
                        OnFrameClipLeave(ref frameData);
                }
                else
                {
                    frameData.clip = clipData.clipData;
                    frameData.subTime = frameData.curTime - beginTime;
                    if (clipData.pDriver == null || !clipData.pDriver.OnFrameClip(this, frameData))
                        OnFrameClip(ref frameData);
                }
            }
        }
        //-----------------------------------------------------
        void CalculateClipBlend(int indexBlendPrev, int indexBlend, int indexBlendNext, ref FrameData frameData)
        {
            frameData.isBlending = false;
            frameData.blendFactor = 1.0f;
            frameData.blendTime = 0.0f;
            if (m_vClips == null)
                return;

            // Get three clips
            IBaseClip clipPrev = (indexBlendPrev >= 0 && indexBlendPrev < m_vClips.Count) ? m_vClips[indexBlendPrev].clipData : null;
            IBaseClip clipCur = (indexBlend >= 0 && indexBlend < m_vClips.Count) ? m_vClips[indexBlend].clipData : null;
            IBaseClip clipNext = (indexBlendNext >= 0 && indexBlendNext < m_vClips.Count) ? m_vClips[indexBlendNext].clipData : null;

            float curTime = frameData.curTime;

            // Get time intervals
            float prevEnd = clipPrev != null ? (clipPrev.GetTime() + clipPrev.GetDuration()) : 0f;
            float curStart = clipCur != null ? clipCur.GetTime() : 0f;
            float curEnd = clipCur != null ? (clipCur.GetTime() + clipCur.GetDuration()) : 0f;
            float nextStart = clipNext != null ? clipNext.GetTime() : 0f;

            float blendPrevOut = clipPrev != null ? clipPrev.GetBlend(false) : 0f;
            float blendCurIn = clipCur != null ? clipCur.GetBlend(true) : 0f;
            float blendCurOut = clipCur != null ? clipCur.GetBlend(false) : 0f;
            float blendNextIn = clipNext != null ? clipNext.GetBlend(true) : 0f;

            // Early exit: no blend at all
            if (blendPrevOut <= 0f && blendCurIn <= 0f && blendCurOut <= 0f && blendNextIn <= 0f)
            {
                return;
            }

            // Calculate factors
            float factorPrev = 0f, factorCur = 0f, factorNext = 0f;

            // 1. Previous clip out blend interval
            if (clipPrev != null && blendPrevOut > 0f && curTime >= (prevEnd - blendPrevOut) && curTime < prevEnd)
            {
                float t = (curTime - (prevEnd - blendPrevOut)) / blendPrevOut;
                factorPrev = 1.0f - t;
                factorCur = t;
                frameData.blendTime = blendPrevOut;
                frameData.blendFactor = factorCur;
                frameData.isBlending = true;
                return;
            }
            // 2. Current clip in blend interval
            if (clipCur != null && blendCurIn > 0f && curTime >= curStart && curTime < (curStart + blendCurIn))
            {
                float t = (curTime - curStart) / blendCurIn;
                factorPrev = 1.0f - t;
                factorCur = t;
                frameData.blendTime = blendCurIn;
                frameData.blendFactor = factorCur;
                frameData.isBlending = true;
                return;
            }
            // 3. Current clip out blend interval
            if (clipCur != null && blendCurOut > 0f && curTime >= (curEnd - blendCurOut) && curTime < curEnd)
            {
                float t = (curTime - (curEnd - blendCurOut)) / blendCurOut;
                factorCur = 1.0f - t;
                factorNext = t;
                frameData.blendTime = blendCurOut;
                frameData.blendFactor = factorCur;
                frameData.isBlending = true;
                return;
            }
            // 4. Next clip in blend interval
            if (clipNext != null && blendNextIn > 0f && curTime >= nextStart && curTime < (nextStart + blendNextIn))
            {
                float t = (curTime - nextStart) / blendNextIn;
                factorCur = 1.0f - t;
                factorNext = t;
                frameData.blendTime = blendNextIn;
                frameData.blendFactor = factorCur;
                frameData.isBlending = true;
                return;
            }
            // 5. Not in blend interval
            frameData.blendFactor = 1.0f;
            frameData.blendTime = 0.0f;
        }
        //-----------------------------------------------------
        public List<ICutsceneData> GetBindOutputDatas()
        {
            if (m_pOwner == null || m_pOwnerGroup == null)
                return null;

            return m_pOwner.GetBindOutputDatas(m_pOwnerGroup);
        }
        //-----------------------------------------------------
        public List<ICutsceneObject> GetBindAllCutsceneObject(List<ICutsceneObject> vObjects)
        {
            if (vObjects != null) vObjects.Clear();
            if (m_pOwner == null) return vObjects;
            return m_pOwner.GetBindAllCutsceneObject(m_pOwnerGroup, vObjects);
        }
        //-----------------------------------------------------
        public ICutsceneObject GetBindLastCutsceneObject()
        {
            if (m_pOwner == null) return null;
            return m_pOwner.GetBindLastCutsceneObject(m_pOwnerGroup);
        }
        //-----------------------------------------------------
        public bool SetOutputVariable<T>(IBaseEvent pEvent, int index, T value) where T : struct
        {
            if (m_pOwner == null) return false;
            return m_pOwner.SetOutputVariable<T>(pEvent, index, value);
        }
        //-----------------------------------------------------
        public bool SetOutputVariable(IBaseEvent pEvent, int index, string value)
        {
            if (m_pOwner == null) return false;
            return m_pOwner.SetOutputVariable(pEvent, index, value);
        }
        //-----------------------------------------------------
        internal void Clear()
		{
			if (m_vClips != null)
			{
                for (int i = 0; i < m_vClips.Count; ++i)
				{
					var clipData = m_vClips[i];
                    DestroyClip(ref clipData);
                }
                m_vClips.Clear();
			}
            if (m_vEvents != null)
            {
                for (int i = 0; i < m_vEvents.Count; ++i)
                {
                    var clipData = m_vEvents[i];
                    DestroyEvent(clipData);
                }
                m_vEvents.Clear();
            }
            m_strName = null;
            m_pOwnerGroup = null;
            m_pOwnerTrack = null;
        }
        //-----------------------------------------------------
        void DestroyClip(ref ClipData clipData)
        {
            if (clipData.clipData == null)
            {
                if (clipData.pDriver != null)
                    m_pOwner.FreeDriver(clipData.pDriver);
                return;
            }
            FrameData defFrame = FrameData.DEF;
            defFrame.ownerTrack = this;
            if (clipData.eStatus == EDriverStatus.Enter ||
                clipData.eStatus == EDriverStatus.Framing)
            {
                clipData.eStatus = EDriverStatus.Leave;
                defFrame.clip = clipData.clipData;
                if (clipData.pDriver == null || !clipData.pDriver.OnClipLeave(this, defFrame))
                    OnFrameClipLeave(ref defFrame);
            }

            if (clipData.eStatus != EDriverStatus.Destroyed)
            {
                clipData.eStatus = EDriverStatus.Destroyed;
                if (clipData.pDriver == null || !clipData.pDriver.OnDestroyClip(this, clipData.clipData))
                    OnDestroyClip(clipData.clipData);
            }

            if (clipData.pDriver != null)
                m_pOwner.FreeDriver(clipData.pDriver);
        }
        //-----------------------------------------------------
        void DestroyEvent(EventData eventData)
        {
            if (eventData.pDriver != null)
                m_pOwner.FreeDriver(eventData.pDriver);
        }
        //-----------------------------------------------------
        public void Destroy()
		{
			Clear();
			m_pOwner = null;
        }
        //-----------------------------------------------------
        protected virtual void OnCreateClip(IBaseClip clip)
		{
			if (m_pOwner == null)
				return;
			m_pOwner.OnCreateClip(this, clip);
        }
        //-----------------------------------------------------
        protected virtual void OnDestroyClip(IBaseClip clip)
		{
            if (m_pOwner == null)
                return;
            m_pOwner.OnDestroyClip(this, clip);
        }
        //-----------------------------------------------------
        protected virtual void OnFrameClip(ref FrameData frameData)
		{
            if (m_pOwner == null)
                return;
			frameData.ownerTrack = this;
            m_pOwner.OnFrameClip(frameData);
        }
        //-----------------------------------------------------
        protected virtual void OnFrameClipEnter(ref FrameData frameData)
		{
            if (m_pOwner == null)
                return;
            m_pOwner.OnFrameClipEnter(this,frameData);
        }
        //-----------------------------------------------------
        protected virtual void OnUpdateClip(ref FrameData frameData)
        {
            if (m_pOwner == null)
                return;
            m_pOwner.OnUpdateClip(this, frameData);
        }
        //-----------------------------------------------------
        protected virtual void OnFrameClipLeave(ref FrameData frameData)
		{
            if (m_pOwner == null)
                return;
            m_pOwner.OnFrameClipLeave(this, frameData);
        }
        //-----------------------------------------------------
        protected virtual void OnEventTrigger(IBaseEvent pEvt)
		{
            if (m_pOwner == null)
                return;
            m_pOwner.OnEventTrigger(this, pEvt);
        }
    }
}