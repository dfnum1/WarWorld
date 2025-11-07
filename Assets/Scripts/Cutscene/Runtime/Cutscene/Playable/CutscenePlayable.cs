/********************************************************************
生成日期:	06:30:2025
类    名: 	CutscenePlayable
作    者:	HappLI
描    述:	过场动画可播放对象
*********************************************************************/
using Framework.AT.Runtime;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
namespace Framework.Cutscene.Runtime
{
    public enum EPlayableStatus : byte
    {
        None = 0,
        Destroy,
        Create,
        Start,
        Pause,
        Stop,
    }
    //-----------------------------------------------------
    public struct CacheTrackDriver
    {
        public ACutsceneDriver pDriver;
        public CutsceneTrack pTrack;
        public IDataer pDater;
    }
    //-----------------------------------------------------
    public class CutscenePlayable
    {
        CutsceneInstance                                            m_pCutscene = null;
        ICutsceneData                                               m_BindData = null;
        EPlayableStatus                                             m_eStatus = EPlayableStatus.None;
        CutsceneData                                                m_CutsceneData = null;
        private List<CutsceneTrack>                                 m_vTracks = null;
        private float                                               m_fPlayTime = 0f;
        private float                                               m_fDuration = 0.0f;   //总时长
        private float                                               m_fInvFrameRate = 0.016667f;
        private float                                               m_fAccumoulator = 0.0f;
        private FrameData                                           m_pFrameData = new FrameData();

        Dictionary<CutsceneData.Group, List<ICutsceneData>>         m_vBindTrackDatas = null;
        Dictionary<long, VariableList>                              m_vOutputVariables = null;
        //-----------------------------------------------------
        internal CutscenePlayable()
        {
            m_pCutscene = null;
            m_eStatus = EPlayableStatus.None;
            m_CutsceneData = null;
            m_vTracks = null;
            m_fPlayTime = 0.0f;
            m_fDuration = 0.0f;
            m_fInvFrameRate = 1.0f / 60.0f; //默认60帧
            m_fAccumoulator = 0.0f;
        }
        //-----------------------------------------------------
        internal void SetCutscene(CutsceneInstance cutsceneInstance)
        {
            m_pCutscene = cutsceneInstance;
        }
        //-----------------------------------------------------
        public ushort GetId()
        {
            if (m_CutsceneData == null) return 0;
            return m_CutsceneData.id;
        }
        //-----------------------------------------------------
        public string GetName()
        {
            if (m_CutsceneData == null) return null;
            return m_CutsceneData.name;
        }
        //-----------------------------------------------------
        public CutsceneInstance GetCutscene()
        {
            return m_pCutscene;
        }
        //-----------------------------------------------------
        internal bool CanPlayable(EDataType dataType, IDataer pDater)
        {
            if (m_pCutscene == null) return false;
            return m_pCutscene.CanPlayable(dataType, pDater);
        }
        //-----------------------------------------------------
        public bool Create(CutsceneData data)
		{
            if (m_eStatus > EPlayableStatus.Destroy)
            {
                Debug.LogWarning("CutscenePlayable is already created, cannot create again.");
                return true;
            }
            Clear();
            if (data == null)
                return false;
            m_CutsceneData = data;
            m_eStatus = EPlayableStatus.Create;
            m_fInvFrameRate = 1.0f / (float)Mathf.Max(30, data.frameRate);

            int trackCnt = data.GetTrackCount();
            if (m_vTracks == null) m_vTracks = new List<CutsceneTrack>(trackCnt);
            for(int i =0; i < data.GetGroupCount(); ++i)
            {
                var group = data.groups[i];
                if (group.HasFlag(EGroupFlag.UnActive))
                    continue;
                for(int j=0; j < group.GetTrackCount(); ++j)
                {
                    if (!group.tracks[j].IsValidTrack())
                        continue;
                    CutsceneTrack track = CutscenePool.MallockTrack();
                    if (!track.Create(this, group, group.tracks[j]))
                    {
                        CutscenePool.FreeTrack(track);
                        Debug.LogError($"Create Track Failed! trackId:{group.tracks[j].trackName}");
                        continue;
                    }
                    else
                    {
                        m_vTracks.Add(track);
                    }
                }
            }
            RefreshDuration();
            OnPlayableCreate();
            ObjectBinderUtils.OnBinderCutsceneObject += OnBinderCutsceneObject;
            return m_vTracks.Count>0;
		}
        //-----------------------------------------------------
        public List<CacheTrackDriver> GetDrivers(IDataer pDataer = null)
        {
            if (m_vTracks == null) return null;
            var caches = CutscenePool.CacheTrackDrivers;
            for (int i = 0; i < m_vTracks.Count; ++i)
            {
                CutsceneTrack track = m_vTracks[i];
                track.CollectDrivers(caches, pDataer);
            }
            return caches;
        }
        //-----------------------------------------------------
        public List<CacheTrackDriver> GetDrivers(System.Type driverType)
        {
            if (m_vTracks == null) return null;
            var caches = CutscenePool.CacheTrackDrivers;
            for (int i = 0; i < m_vTracks.Count; ++i)
            {
                CutsceneTrack track = m_vTracks[i];
                track.CollectDrivers(caches, null, driverType);
            }
            return caches;
        }
        //-----------------------------------------------------
        public void SetObject(ObjId objId, ICutsceneObject pObject, bool bAutoDestroy = false)
        {
            if (m_pCutscene == null) return;
            m_pCutscene.SetObject(objId, pObject, bAutoDestroy);
        }
        //-----------------------------------------------------
        public void RemoveObject(ObjId objId)
        {
            if (m_pCutscene == null) return;
            m_pCutscene.RemoveObject(objId);
        }
        //-----------------------------------------------------
        public void RemoveObject(ICutsceneObject pObj)
        {
            if (m_pCutscene == null) return;
            m_pCutscene.RemoveObject(pObj);
        }
        //-----------------------------------------------------
        public ICutsceneObject GetObject(ObjId objId)
        {
            if (m_pCutscene == null)
                return null;
            return m_pCutscene.GetObject(objId);
        }
        //-----------------------------------------------------
        public ICutsceneObject GetObject(VariableObjId objId)
        {
            if (m_pCutscene == null)
                return null;
            return m_pCutscene.GetObject(objId);
        }
        //-----------------------------------------------------
        public void RefreshDuration()
        {
            m_fDuration = 0.0f;
            if (m_CutsceneData != null)
                m_fDuration = m_CutsceneData.GetDuration();
        }
        //-----------------------------------------------------
        void OnBinderCutsceneObject(int binderId, BinderUnityObject unityObj, bool bBind)
        {
            if (m_vTracks == null)
                return;
            if (!bBind)
                return;
            foreach (var db in m_vTracks)
            {
                var group = db.GetGroupData();
                if (group == null) continue;
                if (group.binderId == binderId)
                {
                    db.BindTrackData(new ObjId(binderId), unityObj);
                }
            }
        }
        //-----------------------------------------------------
        public void SetTime(float time)
        {
            if (m_eStatus < EPlayableStatus.Create)
            {
                Debug.LogError("CutscenePlayable is not created, cannot set time.");
                return;
            }
            if (time > m_fDuration) time = m_fDuration;
            if (time < 0.0f) time = 0.0f;
            m_fPlayTime = time;
            if(time >= m_fDuration)
            {
                Stop(true);
            }
        }
        //-----------------------------------------------------
        public float GetTime()
        {
            return m_fPlayTime;
        }
        //-----------------------------------------------------
        public float GetDuration()
        {
            return m_fDuration;
        }
        //-----------------------------------------------------
        public void Play()
        {
            if (m_eStatus < EPlayableStatus.Create)
            {
                Debug.LogError("CutscenePlayable is not created, cannot play.");
                return;
            }
            if(m_eStatus == EPlayableStatus.Start)
            {
                Debug.LogWarning("CutscenePlayable is already playing, cannot play again.");
                return;
            }
            m_eStatus = EPlayableStatus.Start;
            OnDirtyStatus();
            OnPlayablePlay();
        }
        //-----------------------------------------------------
        public void Pause()
        {
            if (m_eStatus < EPlayableStatus.Create)
            {
                Debug.LogError("CutscenePlayable is not created, cannot pause.");
                return;
            }
            if (m_eStatus == EPlayableStatus.Pause)
                return;
            m_eStatus = EPlayableStatus.Pause;
            OnDirtyStatus();
            OnPlayablePause();
        }
        //-----------------------------------------------------
        public void Resume()
        {
            if (m_eStatus != EPlayableStatus.Pause)
                return;
            m_eStatus = EPlayableStatus.Start;
            OnDirtyStatus();
            OnPlayableResume();
        }
        //-----------------------------------------------------
        public void Stop(bool bTimeOver = false)
        {
            if (m_eStatus == EPlayableStatus.Stop)
                return;
            if (bTimeOver)
            {
                if (m_vTracks != null)
                {
                    for (int i = 0; i < m_vTracks.Count; ++i)
                    {
                        CutsceneTrack track = m_vTracks[i];
                        track.StopTrigger();
                    }
                }
            }
            m_eStatus = EPlayableStatus.Stop;
            m_fPlayTime = 0.0f;
            OnDirtyStatus();
            OnPlayableStop();
        }
        //-----------------------------------------------------
        public EPlayableStatus GetStatus()
        {
            return m_eStatus;
        }
        //-----------------------------------------------------
        public void BindData(ICutsceneData pData)
        {
            m_BindData = pData;
        }
        //-----------------------------------------------------
        public void BindDriver(ACutsceneDriver pDriver, EDataType type = EDataType.eNone, int typeId = 0)
        {

        }
        //-----------------------------------------------------
        internal bool Remove(CutsceneData.Group pGroup, CutsceneData.Track pTrack, IDataer pDater)
        {
            if (m_vTracks == null) return false;
            for (int i = 0; i < m_vTracks.Count; ++i)
            {
                var runtimeTrack = m_vTracks[i];
                if (runtimeTrack.GetTrackData() == pTrack && runtimeTrack.GetGroupData() == pGroup)
                {
                    bool bRemoved = runtimeTrack.RemoveDataer(pGroup, pTrack, pDater);
                    if (bRemoved) RefreshDuration();
                    return bRemoved;
                }
            }
            return false;
        }
        //-----------------------------------------------------
        internal bool AddDataer(CutsceneData.Group pGroup, CutsceneData.Track pTrack, IDataer pDater)
        {
            if (m_vTracks == null)
                m_vTracks = new List<CutsceneTrack>(2);
            for (int i = 0; i < m_vTracks.Count; ++i)
            {
                var runtimeTrack = m_vTracks[i];
                if (runtimeTrack.GetTrackData() == pTrack && runtimeTrack.GetGroupData() == pGroup)
                {
                    if (pDater is IBaseEvent)
                    {
                        if (runtimeTrack.AddEventData(pGroup, pTrack, (IBaseEvent)pDater))
                        {
                            RefreshDuration();
                            return true;
                        }
                    }
                    else if (pDater is IBaseClip)
                    {
                        if(runtimeTrack.AddClipData(pGroup, pTrack, (IBaseClip)pDater, true))
                        {
                            RefreshDuration();
                            return true;
                        }
                    }
                    return false;
                }
            }
            CutsceneTrack track = CutscenePool.MallockTrack();
            if (!track.Create(this, pGroup, pTrack))
            {
                CutscenePool.FreeTrack(track);
                Debug.LogError($"Create Track Failed! trackId:{pTrack.trackName}");
                return false;
            }
            m_vTracks.Add(track);
            RefreshDuration();
            return true;
        }
        //-----------------------------------------------------
        public void Update(float fDelta)
        {
            if (m_pCutscene == null || m_eStatus < EPlayableStatus.Create || m_CutsceneData == null)
                return;

            if(m_eStatus == EPlayableStatus.Pause)
            {
                UpdateFrame(0);       
                return;
            }
            if (m_eStatus != EPlayableStatus.Start)
            {
#if UNITY_EDITOR
                if (m_pCutscene.IsEditorMode())
                    UpdateFrame(0);
#endif
                return;
            }
            if (fDelta > 0.0f)
            {
                bool bFixedUpdate = false;
#if UNITY_EDITOR
                if(m_pCutscene.IsEditorMode())
                {
                    bFixedUpdate = true;
                }
#endif
                if(bFixedUpdate)
                {
                    m_fAccumoulator += fDelta;
                    while (m_fAccumoulator >= m_fInvFrameRate)
                    {
                        m_fAccumoulator -= m_fInvFrameRate;
                         UpdateFrame(m_fInvFrameRate);
                    }
                }
                else
                    UpdateFrame(Time.deltaTime);
            }
        }
        //-----------------------------------------------------
        void UpdateFrame(float fDelta)
        {
            m_fPlayTime += fDelta;
            if (m_fPlayTime > m_fDuration)
            {
                m_fPlayTime = m_fDuration;
                Stop(true);
                return;
            }
            m_pFrameData.curTime = m_fPlayTime;
            m_pFrameData.totalDuration = GetDuration();
            m_pFrameData.deltaTime = fDelta;
            m_pFrameData.eStatus = m_eStatus;
            bool isOverAll = true;
            for (int i = 0; i < m_vTracks.Count; ++i)
            {
                CutsceneTrack track = m_vTracks[i];
                if (!track.Update(ref m_pFrameData))
                    isOverAll = false;
            }
            if(isOverAll)
            {
                Stop(true);
            }
        }
        //-----------------------------------------------------
        public void Evaluate(float time)
        {
            m_fPlayTime = time;
            m_pFrameData.curTime = m_fPlayTime;
            m_pFrameData.totalDuration = GetDuration();
            m_pFrameData.deltaTime = m_fInvFrameRate;
            m_pFrameData.eStatus = m_eStatus;
            if (m_vTracks == null)
                return;
            for (int i = 0; i < m_vTracks.Count; ++i)
            {
                CutsceneTrack track = m_vTracks[i];
                track.Update(ref m_pFrameData);
            }
        }
        //-----------------------------------------------------
        public void GetGroupNames(List<string> vNames, List<ushort> vIds, ushort discardGp = ushort.MaxValue)
        {
            if (m_CutsceneData == null)
                return;
            foreach(var db in m_CutsceneData.groups)
            {
                if (db.id == discardGp)
                    continue;
                vNames.Add(db.name);
                vIds.Add(db.id);
            }
        }
        //-----------------------------------------------------
        public void BindTrackData<T>(string group, T pData) where T : struct
        {
            if (string.IsNullOrEmpty(group)) return;
            if (m_CutsceneData == null || m_CutsceneData.groups == null)
            {
                Debug.LogError("CutscenePlayable.BindTrackData: CutsceneData is null or groups is null.");
                return;
            }
            CutsceneData.Group cutGroup = m_CutsceneData.GetGroup(group);
            if (cutGroup == null)
            {
                Debug.LogError($"CutscenePlayable.BindTrackData: Group '{group}' not found in CutsceneData.");
                return;
            }
            BindTrackData(cutGroup, pData);
        }
        //-----------------------------------------------------
        public void BindTrackData(string group,IVariable pData)
        {
            if (string.IsNullOrEmpty(group)) return;
            if (m_CutsceneData == null || m_CutsceneData.groups == null)
            {
                Debug.LogError("CutscenePlayable.BindTrackData: CutsceneData is null or groups is null.");
                return;
            }
            CutsceneData.Group cutGroup = m_CutsceneData.GetGroup(group);
            if (cutGroup == null)
            {
                Debug.LogError($"CutscenePlayable.BindTrackData: Group '{group}' not found in CutsceneData.");
                return;
            }
            BindTrackData(cutGroup, pData);
        }
        //-----------------------------------------------------
        public void BindTrackData(string group, string strData)
        {
            if (string.IsNullOrEmpty(group)) return;
            if (m_CutsceneData == null || m_CutsceneData.groups == null)
            {
                Debug.LogError("CutscenePlayable.BindTrackData: CutsceneData is null or groups is null.");
                return;
            }
            CutsceneData.Group cutGroup = m_CutsceneData.GetGroup(group);
            if (cutGroup == null)
            {
                Debug.LogError($"CutscenePlayable.BindTrackData: Group '{group}' not found in CutsceneData.");
                return;
            }
            BindTrackData(cutGroup, strData);
        }
        //-----------------------------------------------------
        public void BindTrackData<T>(ushort group, T pData) where T : struct
        {
            if (m_CutsceneData == null || m_CutsceneData.groups == null)
            {
                Debug.LogError("CutscenePlayable.BindTrackData: CutsceneData is null or groups is null.");
                return;
            }
            CutsceneData.Group cutGroup = m_CutsceneData.GetGroup(group);
            if (cutGroup == null)
            {
                Debug.LogError($"CutscenePlayable.BindTrackData: Group '{group}' not found in CutsceneData.");
                return;
            }
            BindTrackData(cutGroup, pData);
        }
        //-----------------------------------------------------
        public void BindTrackData(ushort group, IVariable pData)
        {
            if (m_CutsceneData == null || m_CutsceneData.groups == null)
            {
                Debug.LogError("CutscenePlayable.BindTrackData: CutsceneData is null or groups is null.");
                return;
            }
            CutsceneData.Group cutGroup = m_CutsceneData.GetGroup(group);
            if (cutGroup == null)
            {
                Debug.LogError($"CutscenePlayable.BindTrackData: Group '{group}' not found in CutsceneData.");
                return;
            }
            BindTrackData(cutGroup, pData);
        }
        //-----------------------------------------------------
        public void BindTrackData(ushort group, string strData)
        {
            if (m_CutsceneData == null || m_CutsceneData.groups == null)
            {
                Debug.LogError("CutscenePlayable.BindTrackData: CutsceneData is null or groups is null.");
                return;
            }
            CutsceneData.Group cutGroup = m_CutsceneData.GetGroup(group);
            if (cutGroup == null)
            {
                Debug.LogError($"CutscenePlayable.BindTrackData: Group '{group}' not found in CutsceneData.");
                return;
            }
            BindTrackData(cutGroup, strData);
        }
        //-----------------------------------------------------
        internal CutsceneTrack GetTrack(CutsceneData.Track track)
        {
            if (track == null) return null;
            if (m_vTracks == null) return null;
            for(int i =0; i < m_vTracks.Count; ++i)
            {
                if (m_vTracks[i].GetTrackData() == track)
                {
                    return m_vTracks[i];
                }
            }
            return null;
        }
        //-----------------------------------------------------
        internal void BindTrackData<T>(CutsceneData.Group group, T pData) where T : struct
        {
            if (m_vBindTrackDatas == null)
                m_vBindTrackDatas = new Dictionary<CutsceneData.Group, List<ICutsceneData>>(m_CutsceneData!=null?m_CutsceneData.GetGroupCount():1);
            if (!m_vBindTrackDatas.TryGetValue(group, out var tracks))
            {
                tracks = UnityEngine.Pool.ListPool<ICutsceneData>.Get();
                m_vBindTrackDatas[group] = tracks;
            }
            for (int i = 0; i < tracks.Count; ++i)
            {
                var bind = tracks[i];
                if (bind is BindTrackData)
                {
                    BindTrackData bindData = (BindTrackData)bind;
                    if (bindData.dater == null)
                    {
                        bindData.outputDatas.AddVariable(pData);
                        tracks[i] = bindData;
                        return;
                    }
                }
            }
            BindTrackData newBind = new BindTrackData();
            newBind.dater = null;
            newBind.outputDatas = VariableList.Malloc(2);
            newBind.outputDatas.AddVariable(pData);
            tracks.Add(newBind);
        }
        //-----------------------------------------------------
        internal void BindTrackData(CutsceneData.Group group, string strData)
        {
            if (m_vBindTrackDatas == null)
                m_vBindTrackDatas = new Dictionary<CutsceneData.Group, List<ICutsceneData>>(m_CutsceneData != null ? m_CutsceneData.GetGroupCount() : 1);
            if (!m_vBindTrackDatas.TryGetValue(group, out var tracks))
            {
                tracks = UnityEngine.Pool.ListPool<ICutsceneData>.Get();
                m_vBindTrackDatas[group] = tracks;
            }
            for (int i = 0; i < tracks.Count; ++i)
            {
                var bind = tracks[i];
                if (bind is BindTrackData)
                {
                    BindTrackData bindData = (BindTrackData)bind;
                    if (bindData.dater == null)
                    {
                        bindData.outputDatas.AddVariable(strData);
                        tracks[i] = bindData;
                        return;
                    }
                }
            }
            BindTrackData newBind = new BindTrackData();
            newBind.dater = null;
            newBind.outputDatas = VariableList.Malloc(2);
            newBind.outputDatas.AddVariable(strData);
            tracks.Add(newBind);
        }
        //-----------------------------------------------------
        internal void BindTrackData(CutsceneData.Group group, IVariable pData)
        {
            if (m_vBindTrackDatas == null)
                m_vBindTrackDatas = new Dictionary<CutsceneData.Group, List<ICutsceneData>>(m_CutsceneData != null ? m_CutsceneData.GetGroupCount() : 1);
            if (!m_vBindTrackDatas.TryGetValue(group, out var tracks))
            {
                tracks = UnityEngine.Pool.ListPool<ICutsceneData>.Get();
                m_vBindTrackDatas[group] = tracks;
            }
            for (int i = 0; i < tracks.Count; ++i)
            {
                var bind = tracks[i];
                if (bind is BindTrackData)
                {
                    BindTrackData bindData = (BindTrackData)bind;
                    if (bindData.dater == null)
                    {
                        bindData.outputDatas.AddVariable(pData);
                        tracks[i] = bindData;
                        return;
                    }
                }
            }
            BindTrackData newBind = new BindTrackData();
            newBind.dater = null;
            newBind.outputDatas = VariableList.Malloc(2);
            newBind.outputDatas.AddVariable(pData);
            tracks.Add(newBind);
        }
        //-----------------------------------------------------
        internal List<ICutsceneData> GetBindOutputDatas(CutsceneData.Group group)
        {
            if (group == null || m_vBindTrackDatas == null) return null;
            if (m_vBindTrackDatas.TryGetValue(group, out var traks))
                return traks;
            return null;
        }
        //-----------------------------------------------------
        public ICutsceneObject GetBindLastCutsceneObject(ushort group)
        {
            if (m_CutsceneData == null || m_CutsceneData.groups == null)
            {
                Debug.LogError("CutscenePlayable.GetBindLastCutsceneObject: CutsceneData is null or groups is null.");
                return null;
            }
            CutsceneData.Group cutGroup = m_CutsceneData.GetGroup(group);
            if (cutGroup == null)
            {
                Debug.LogError($"CutscenePlayable.GetBindLastCutsceneObject: Group '{group}' not found in CutsceneData.");
                return null;
            }
            return GetBindLastCutsceneObject(cutGroup);
        }
        //-----------------------------------------------------
        internal ICutsceneObject GetBindLastCutsceneObject(CutsceneData.Group group)
        {
            if (group.binderId != 0)
            {
                var binder = ObjectBinderUtils.GetBinder(group.binderId);
                if (binder.IsValid())
                {
                    return binder;
                }
            }
            ICutsceneObject pObj = null;
            if (m_vBindTrackDatas != null)
            {
                List<ICutsceneData> vDatas = GetBindOutputDatas(group);
                if (vDatas != null)
                {
                    foreach (var db in vDatas)
                    {
                        if (!(db is BindTrackData)) continue;
                        BindTrackData trackData = (BindTrackData)db;
                        if (trackData.outputDatas != null)
                        {
                            var objs = trackData.outputDatas.GetObjIds();
                            if (objs != null)
                            {
                                foreach (var obj in objs)
                                {
                                    var cutsceneObj = GetObject(obj);
                                    if (cutsceneObj != null)
                                    {
                                        pObj = cutsceneObj;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if(pObj == null && group == null)
            {
                var vCaches = m_pCutscene.GetObjCahces();
                if (vCaches != null && vCaches.Count > 0)
                {
                    foreach (var db in vCaches)
                    {
                        return db.Value.pObj;
                    }
                }
            }

            return pObj;
        }
        //-----------------------------------------------------
        internal List<ICutsceneObject> GetBindAllCutsceneObject(ushort group, List<ICutsceneObject> vObjects)
        {
            if (m_CutsceneData == null || m_CutsceneData.groups == null)
            {
                Debug.LogError("CutscenePlayable.GetBindAllCutsceneObject: CutsceneData is null or groups is null.");
                return vObjects;
            }
            CutsceneData.Group cutGroup = m_CutsceneData.GetGroup(group);
            if (cutGroup == null)
            {
                Debug.LogError($"CutscenePlayable.GetBindAllCutsceneObject: Group '{group}' not found in CutsceneData.");
                return vObjects;
            }
            return GetBindAllCutsceneObject(cutGroup, vObjects);
        }
        //-----------------------------------------------------
        internal List<ICutsceneObject> GetBindAllCutsceneObject(CutsceneData.Group group,List<ICutsceneObject> vObjects)
        {
            if (vObjects != null) vObjects.Clear();
            List<ICutsceneData> vDatas = GetBindOutputDatas(group);
            if (vDatas != null && m_vBindTrackDatas!=null)
            {
                foreach (var db in vDatas)
                {
                    if (!(db is BindTrackData)) continue;
                    BindTrackData trackData = (BindTrackData)db;
                    if (trackData.outputDatas != null)
                    {
                        var objs = trackData.outputDatas.GetObjIds();
                        if (objs != null)
                        {
                            foreach (var obj in objs)
                            {
                                var cutsceneObj = GetObject(obj);
                                if (cutsceneObj != null)
                                {
                                    if (vObjects == null) vObjects = UnityEngine.Pool.ListPool<ICutsceneObject>.Get();
                                    if(!vObjects.Contains(cutsceneObj)) vObjects.Add(cutsceneObj);
                                }
                            }
                        }
                    }
                }
            }
            if (group.binderId != 0)
            {
                var binder = ObjectBinderUtils.GetBinder(group.binderId);
                if (binder.IsValid())
                {
                    if (vObjects == null) vObjects = UnityEngine.Pool.ListPool<ICutsceneObject>.Get();
                    if (!vObjects.Contains(binder)) vObjects.Add(binder);
                }
            }

            return vObjects;
        }
        //-----------------------------------------------------
        internal bool SetOutputVariable<T>(IBaseEvent pEvent, int index, T value) where T : struct
        {
            long key = BuildOutputKey(pEvent, index);
            if (m_vOutputVariables == null)
            {
                m_vOutputVariables = new Dictionary<long, VariableList>(2);
            }
            if (m_vOutputVariables.TryGetValue(key, out var pVarList))
            {
                pVarList.Clear();
            }
            else
            {
                pVarList = VariableList.Malloc(1);
                m_vOutputVariables[key] = pVarList;
            }

            if (!pVarList.AddVariable(value))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError($"CutsceneTrack.SetOutputVariable: Create variable failed for type {typeof(T).Name}");
#endif
                return false;
            }
            return true;
        }
        //-----------------------------------------------------
        internal bool SetOutputVariable(IBaseEvent pEvent, int index, string value)
        {
            long key = BuildOutputKey(pEvent, index);
            if (m_vOutputVariables == null)
            {
                m_vOutputVariables = new Dictionary<long, VariableList>(2);
            }
            if (m_vOutputVariables.TryGetValue(key, out var pVarList))
            {
                pVarList.Clear();
            }
            else
            {
                pVarList = VariableList.Malloc(1);
                m_vOutputVariables[key] = pVarList;
            }

            pVarList.AddVariable(value);
            return true;
        }
        //-----------------------------------------------------
        long BuildOutputKey(IBaseEvent pEvent, int index)
        {
            ushort typeId = pEvent.GetIdType();
            int triggerTime =((int)(pEvent.GetTime() * 10000));
            return ((long)triggerTime << 32) | ((long)typeId << 16) | (long)index;
        }
        //-----------------------------------------------------
        internal void BindEventTrackData(CutsceneData.Group ownerGroup, IBaseEvent pEvt)
        {
            if (pEvt.GetIdType() == (short)EEventType.eCustom)
            {
                CutsceneCustomEvent customEvent = (CutsceneCustomEvent)pEvt;
                if (customEvent.outputBindTrack && customEvent.outputVariables != null && customEvent.outputVariables.variables != null)
                {
                    bool bHas = false;
                    for (int i = 0; i < customEvent.outputVariables.GetVarCount(); ++i)
                    {
                        if (m_vOutputVariables!=null && m_vOutputVariables.ContainsKey(BuildOutputKey(pEvt, i)))
                        {
                            bHas = true;
                            break;
                        }
                    }
                    if (bHas)
                    {
                        if (m_vBindTrackDatas == null)
                        {
                            m_vBindTrackDatas = new Dictionary<CutsceneData.Group, List<ICutsceneData>>(1);
                        }
                        if(!m_vBindTrackDatas.TryGetValue(ownerGroup, out var trackDatas))
                        {
                            trackDatas = UnityEngine.Pool.ListPool<ICutsceneData>.Get();
                            m_vBindTrackDatas[ownerGroup] = trackDatas;
                        }
                        BindTrackData bind = new BindTrackData();
                        bind.dater = customEvent;
                        bind.outputDatas =VariableList.Malloc(2);
                        for (int i = 0; i < customEvent.outputVariables.GetVarCount(); ++i)
                        {
                            long key = BuildOutputKey(pEvt,i);
                            if (m_vOutputVariables.TryGetValue(key, out var pVar))
                            {
                                bind.outputDatas.AddVariable(pVar);
                            }
                            else
                                bind.outputDatas.AddVariable(customEvent.outputVariables.variables, i);
                        }
                        trackDatas.Add(bind);
                    }

                }
            }
        }
        //-----------------------------------------------------
        internal ACutsceneDriver CreateDriver(EDataType type, IDataer pDater)
        {
            return m_pCutscene.CreateDriver(type, pDater);
        }
        //-----------------------------------------------------
        internal void FreeDriver(ACutsceneDriver pDriver)
        {
            m_pCutscene.FreeDriver(pDriver);
        }
        //-----------------------------------------------------
        void Clear()
        {
            m_fPlayTime = 0.0f;
            m_fDuration = 0.0f;
            m_fAccumoulator = 0.0f;
            ObjectBinderUtils.OnBinderCutsceneObject -= OnBinderCutsceneObject;
            ClearTracks();

            m_BindData = null;
            m_BindData = null;
            m_CutsceneData = null;

            if (m_vBindTrackDatas != null)
            {
                foreach (var db in m_vBindTrackDatas)
                {
                    foreach (var dataDb in db.Value)
                    {
                        if (dataDb is BindTrackData)
                        {
                            BindTrackData bindTrackData = (BindTrackData)dataDb;
                            if (bindTrackData.outputDatas != null)
                                bindTrackData.outputDatas.Release();
                        }
                    }
                    UnityEngine.Pool.ListPool<ICutsceneData>.Release(db.Value);
                }
                m_vBindTrackDatas.Clear();
            }
            if (m_vOutputVariables != null)
            {
                foreach(var db in m_vOutputVariables)
                {
                    db.Value.Release();
                }
                m_vOutputVariables.Clear();
            }
        }
        //-----------------------------------------------------
        internal void Destroy()
        {
            if (m_eStatus == EPlayableStatus.Destroy)
                return;
            m_eStatus = EPlayableStatus.Destroy;
            Clear();
            OnPlayableDestroy();
            OnDirtyStatus();
            m_pCutscene = null;
        }
        //-----------------------------------------------------
        void ClearTracks()
        {
            if (m_vTracks == null)
                return;
            for(int i =0; i < m_vTracks.Count; ++i)
            {
                CutscenePool.FreeTrack(m_vTracks[i]);
            }
            m_vTracks.Clear();
        }
        //-----------------------------------------------------
        public virtual void OnDirtyStatus()
        {
            m_pCutscene.OnCutsceneStatus(m_eStatus);
        }
        //-----------------------------------------------------
        void OnPlayableCreate() 
        {
            m_pCutscene.OnPlayableCreate(this);
        }
        //-----------------------------------------------------
        void OnPlayableDestroy()
        {
            m_pCutscene.OnPlayableDestroy(this);
        }
        //-----------------------------------------------------
        void OnPlayableStop() 
        {
            m_pCutscene.OnPlayableStop(this);
        }
        //-----------------------------------------------------
        void OnPlayablePlay() 
        {
            m_pCutscene.OnPlayablePlay(this);
        }
        //-----------------------------------------------------
        void OnPlayablePause()
        {
            m_pCutscene.OnPlayablePause(this);
        }
        //-----------------------------------------------------
        void OnPlayableResume() 
        {
            m_pCutscene.OnPlayableResume(this);
        }
        //-----------------------------------------------------
        public void OnCreateClip(CutsceneTrack track, IBaseClip clip) 
        {
            m_pCutscene.OnCreateClip(track, clip);
        }
        //-----------------------------------------------------
        public void OnDestroyClip(CutsceneTrack track, IBaseClip clip)
        {
            m_pCutscene.OnDestroyClip(track, clip);
        }
        //-----------------------------------------------------
        public void OnFrameClip(FrameData frameData)
        {
            m_pCutscene.OnFrameClip(frameData);
        }
        //-----------------------------------------------------
        public void OnFrameClipEnter(CutsceneTrack track, FrameData frameData)
        {
            m_pCutscene.OnFrameClipEnter(track, frameData);
        }
        //-----------------------------------------------------
        public void OnUpdateClip(CutsceneTrack track, FrameData frameData)
        {
            m_pCutscene.OnUpdateClip(track, frameData);
        }
        //-----------------------------------------------------
        public void OnFrameClipLeave(CutsceneTrack track, FrameData frameData)
        {
            m_pCutscene.OnFrameClipLeave(track, frameData);
        }
        //-----------------------------------------------------
        public void OnEventTrigger(CutsceneTrack track, IBaseEvent pEvt)
        {
            m_pCutscene.OnEventTrigger(track, pEvt);
        }
    }
}