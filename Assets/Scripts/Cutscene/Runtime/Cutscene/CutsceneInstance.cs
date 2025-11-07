/********************************************************************
生成日期:	06:30:2025
类    名: 	Cutscene
作    者:	HappLI
描    述:	过场动画管理器
*********************************************************************/
using Framework.AT.Runtime;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
namespace Framework.Cutscene.Runtime
{
    struct ObjCache
    {
        public bool autoDestroy; //是否自动销毁
        public ObjId objId;
        public ICutsceneObject pObj; //绑定的对象
    }
    //-----------------------------------------------------
    public interface ICutsceneData
    {

    }
    //-----------------------------------------------------
    public class CutsceneInstance
    {
        bool                                        m_bEnable = false;
        bool                                        m_bEditorMode = false;
        string                                      m_strName = null;
#if UNITY_EDITOR
        UnityEditor.EditorWindow                    m_pOwnerEditor = null; //编辑器模式下的编辑器窗口
        Dictionary<long, bool>                      m_vPlayableToggle = null;
#endif
        int                                         m_nGUID = 0;
        CutsceneGraph                               m_pData = null;
        ICutsceneData                               m_BindData = null;
        CutsceneManager                             m_pMgr;
        private List<ICutsceneCallback>             m_vCallbacks = null;
        private CutscenePlayable                    m_pPlayable = null;
        private AgentTree                           m_pAgentTree = null;

        private Dictionary<long, ACutsceneDriver>   m_vBindKeyDrivers = null; //绑定具体参数驱动器
        private ACutsceneDriver                     m_pBindDriver = null;
        private int                                 m_nPlayableGuid = 0;

        private Dictionary<int, ObjCache>           m_vObjIds = null;
        //-----------------------------------------------------
        internal CutsceneInstance()
        {
            Debug.Assert(false, "CutsceneInstance must be created by CutsceneManager!");
        }
        //-----------------------------------------------------
        internal CutsceneInstance(CutsceneManager mgr)
        {
            m_pMgr = mgr;
            m_pBindDriver = null;
            m_vBindKeyDrivers = null;
            m_BindData = null;
            m_bEnable = false;
        }
        //-----------------------------------------------------
        public void SetName(string name)
        {
            m_strName = name;
        }
        //-----------------------------------------------------
        public string GetName()
        {
            return m_strName;
        }
        //-----------------------------------------------------
        public string GetSubName()
        {
            if (m_pPlayable == null) return null;
            return m_pPlayable.GetName();
        }
        //-----------------------------------------------------
        internal CutsceneManager GetCutsceneManager()
        {
            return m_pMgr;
        }
        //-----------------------------------------------------
        internal void SetCutsceneManager(CutsceneManager cutsceneMgr)
        {
            m_pMgr = cutsceneMgr;
        }
        //-----------------------------------------------------
        public int GetGUID()
        {
            return m_nGUID;
        }
        //-----------------------------------------------------
        internal void SetGUID(int guid)
        {
            m_nGUID = guid;
        }
        //-----------------------------------------------------
        public bool IsEditorMode()
        {
#if UNITY_EDITOR
            return m_pOwnerEditor!=null || m_bEditorMode;
#else
            return m_bEditorMode;
#endif
        }
        //-----------------------------------------------------
        internal void SetPlayableToggle(EDataType dataType, int typeId, uint customType, bool bEnable)
        {
#if UNITY_EDITOR
            if (m_vPlayableToggle == null) m_vPlayableToggle = new Dictionary<long, bool>(32);
            m_vPlayableToggle[CutscenePool.GetDaterKey(dataType, typeId, customType)] = bEnable;
#endif
        }
        //-----------------------------------------------------
        internal void SetPlayableToggle(EDataType dataType, int typeId, bool bEnable)
        {
#if UNITY_EDITOR
            if (m_vPlayableToggle == null) m_vPlayableToggle = new Dictionary<long, bool>(32);
            m_vPlayableToggle[CutscenePool.GetDaterKey(dataType, typeId)] = bEnable;
#endif
        }
        //-----------------------------------------------------
        internal bool CanPlayable(EDataType type, IDataer pDater)
        {
#if UNITY_EDITOR
            if(IsEditorMode())
            {
                if(m_vPlayableToggle !=null)
                {
                    if(m_vPlayableToggle.TryGetValue(CutscenePool.GetDaterKey(type,pDater), out var bEnable))
                    {
                        return bEnable;
                    }
                }
            }
#endif
            return true;
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        public void SetEditorMode(bool bEditorMode, UnityEditor.EditorWindow pOwnerEditor)
        {
            m_bEditorMode = bEditorMode;
            m_pOwnerEditor = pOwnerEditor;
        }
        //-----------------------------------------------------
        public UnityEditor.EditorWindow GetOwnerEditor()
        {
            return m_pOwnerEditor;
        }
#endif
        //-----------------------------------------------------
        public bool Create(CutsceneGraph data, string subName = null, int subId = -1)
        {
            Clear();
            if (data == null)
            {
                Debug.LogError("CutsceneInstance.Create data is null!");
                return false;
            }

            m_pData = data;
            //! create playable
            CutsceneData playableCutsceneData = null;
            if (!string.IsNullOrEmpty(subName))  playableCutsceneData = data.GetSubCutscene(subName);
            else if(subId>=0 && subId < ushort.MaxValue) playableCutsceneData = data.GetSubCutscene((ushort)subId);
            else playableCutsceneData = data.GetEnterCutscene();
            CreatePlayable(playableCutsceneData);

            //! create agent tree
            CreateAgentTree(data);
            
            return true;
        }
        //-----------------------------------------------------
        internal CutsceneGraph GetCutsceneData()
        {
            return m_pData;
        }
        //-----------------------------------------------------
        public AgentTree GetAgentTree()
        {
            return m_pAgentTree;
        }
        //-----------------------------------------------------
        public CutscenePlayable GetPlayable()
        {
            return m_pPlayable;
        }
        //-----------------------------------------------------
        public ushort GetCutsceneId()
        {
            if (m_pPlayable == null) return 0;
            return m_pPlayable.GetId();
        }
        //-----------------------------------------------------
        public bool CreateAgentTree(CutsceneGraph graphData)
        {
            if(m_pAgentTree!=null)
            {
                m_pMgr.FreeAgentTree(m_pAgentTree);
                m_pAgentTree = null;
            }
            if (graphData.agentTree != null && graphData.agentTree.IsValid())
            {
                m_pAgentTree = m_pMgr.MallocAgentTree();
                m_pAgentTree.SetCutscene(this);
                if (!m_pAgentTree.Create(graphData))
                {
                    m_pMgr.FreeAgentTree(m_pAgentTree);
                    m_pAgentTree = null;
                }
            }
            return m_pAgentTree != null;
        }
        //-----------------------------------------------------
        public bool ExecuteTask(int type, VariableList vArgvs = null, bool bAutoReleaseAgvs = true)
        {
            if (m_pAgentTree == null) return false;
            return m_pAgentTree.ExecuteTask(type, vArgvs, bAutoReleaseAgvs);
        }
        //-----------------------------------------------------
        public bool LoadAsset(string file, System.Action<UnityEngine.Object> onCallback, bool bAsync = true)
        {
            if (m_pMgr == null)
            {
                return false;
            }
            m_pMgr.LoadAsset(file, onCallback, bAsync);
            return true;
        }
        //-----------------------------------------------------
        public bool UnloadAsset(UnityEngine.Object pObj)
        {
            if (m_pMgr == null)
            {
                return false;
            }
            m_pMgr.UnloadAsset(pObj);
            return true;
        }
        //-----------------------------------------------------
        public bool SpawnInstance(string file, System.Action<UnityEngine.GameObject> onCallback, bool bAsync = true)
        {
            if (m_pMgr == null)
            {
                return false;
            }
            return m_pMgr.SpawnInstance(file, onCallback, bAsync);
        }
        //-----------------------------------------------------
        public void DespawnInstance(GameObject pInstance, string name = null)
        {
            if (m_pMgr == null)
            {
                return;
            }
            m_pMgr.DespawnInstance(pInstance, name);
        }
        //-----------------------------------------------------
        public void RefreshDuration()
        {
            if (m_pPlayable == null) return;
            m_pPlayable.RefreshDuration();
        }
        //-----------------------------------------------------
        public void SetTime(float time)
        {
            if (m_pPlayable == null) return;
            m_pPlayable.SetTime(time);
#if UNITY_EDITOR
            if (m_pOwnerEditor != null && m_pOwnerEditor is Editor.CutsceneEditor)
            {
                ((Editor.CutsceneEditor)m_pOwnerEditor).OnSetTime(time);
            }
#endif
        }
        //-----------------------------------------------------
        public float GetTime()
        {
            if (m_pPlayable == null) return 0.0f;
            return m_pPlayable.GetTime();
        }
        //-----------------------------------------------------
        public float GetDuration()
        {
            if (m_pPlayable == null) return 0.0f;
            return m_pPlayable.GetDuration();
        }
        //-----------------------------------------------------
        public void Enable(bool bEnable)
        {
            if (m_bEnable == bEnable)
                return;
            m_bEnable = bEnable;
            if (m_pAgentTree != null)
                m_pAgentTree.Enable(bEnable);
        }
        //-----------------------------------------------------
        public bool IsEnable()
        {
            return m_bEnable;
        }
        //-----------------------------------------------------
        public void Play()
        {
            if (m_pPlayable == null) return;
            m_pPlayable.Play();
        }
        //-----------------------------------------------------
        public void Pause()
        {
            if (m_pPlayable == null) return;
            m_pPlayable.Pause();
        }
        //-----------------------------------------------------
        public void Resume()
        {
            if (m_pPlayable == null) return;
            m_pPlayable.Resume();
        }
        //-----------------------------------------------------
        public void Stop(bool bTimeOver = false)
        {
            if (m_pPlayable == null) return;
            m_pPlayable.Stop(bTimeOver);
        }
        //-----------------------------------------------------
        public EPlayableStatus GetStatus()
        {
            if (m_pPlayable == null) return EPlayableStatus.Stop;
            return m_pPlayable.GetStatus();
        }
        //-----------------------------------------------------
        public void SetObject(ObjId objId, ICutsceneObject pObject, bool bAutoDestroy = false)
        {
            if (m_vObjIds == null)
                m_vObjIds = new Dictionary<int, ObjCache>(2);
            if (m_vObjIds.TryGetValue(objId.id, out var objCache))
            {
                if (objCache.autoDestroy && objCache.pObj != null)
                    objCache.pObj.Destroy();
            }
            objCache.objId = objId;
            objCache.autoDestroy = bAutoDestroy;
            objCache.pObj = pObject;
            m_vObjIds[objId.id] = objCache;
        }
        //-----------------------------------------------------
        public void RemoveObject(ObjId objId)
        {
            if (m_vObjIds == null) return;
            m_vObjIds.Remove(objId.id);
        }
        //-----------------------------------------------------
        public void RemoveObject(ICutsceneObject pObj)
        {
            if (m_vObjIds == null) return;
            foreach(var db in m_vObjIds)
            {
                if(db.Value.pObj == pObj)
                {
                    m_vObjIds.Remove(db.Key);
                    break;
                }
            }
        }
        //-----------------------------------------------------
        public ICutsceneObject GetObject(ObjId objId)
        {
            if (m_vObjIds == null || !m_vObjIds.TryGetValue(objId.id, out var objCache))
                return null;
            return objCache.pObj;
        }
        //-----------------------------------------------------
        public ICutsceneObject GetObject(VariableObjId objId)
        {
            return GetObject(objId.value);
        }
        //-----------------------------------------------------
        internal Dictionary<int, ObjCache> GetObjCahces()
        {
            return m_vObjIds;
        }
        //-----------------------------------------------------
        public void BindData(ICutsceneData pData)
        {
            m_BindData = pData;
        }
        //-----------------------------------------------------
        public ICutsceneData GetBindData()
        {
            return m_BindData;
        }
        //-----------------------------------------------------
        public bool BindGroupTrackData<T>(string group, T pData) where T : struct
        {
            if (m_pPlayable == null) return false;
            if (string.IsNullOrEmpty(group)) return false;

            m_pPlayable.BindTrackData(group, pData);
            return true;
        }
        //-----------------------------------------------------
        public bool BindGroupTrackData(string group, IVariable pData)
        {
            if (m_pPlayable == null) return false;
            if (string.IsNullOrEmpty(group)) return false;

            m_pPlayable.BindTrackData(group, pData);
            return true;
        }
        //-----------------------------------------------------
        public bool BindGroupTrackData(string trackName, string strData)
        {
            if (m_pPlayable == null) return false;
            if (string.IsNullOrEmpty(trackName)) return false;

            m_pPlayable.BindTrackData(trackName, strData);
            return true;
        }
        //-----------------------------------------------------
        public bool BindGroupTrackData<T>(ushort groupId, T pData) where T : struct
        {
            if (m_pPlayable == null) return false;
            m_pPlayable.BindTrackData(groupId, pData);
            return true;
        }
        //-----------------------------------------------------
        public bool BindGroupTrackData(ushort groupId, IVariable pData)
        {
            if (m_pPlayable == null) return false;

            m_pPlayable.BindTrackData(groupId, pData);
            return true;
        }
        //-----------------------------------------------------
        public bool BindGroupTrackData(ushort groupId, string strData)
        {
            if (m_pPlayable == null) return false;
            m_pPlayable.BindTrackData(groupId, strData);
            return true;
        }
        //-----------------------------------------------------
        public List<ICutsceneObject> GetGroupBindAllCutsceneObject(ushort groupId, List<ICutsceneObject> vList)
        {
            if (m_pPlayable == null)
            {
                return vList;
            }
            if (vList == null) vList = new List<ICutsceneObject>(2);
            m_pPlayable.GetBindAllCutsceneObject(groupId, vList);
            return vList;
        }
        //-----------------------------------------------------
        public ICutsceneObject GetGroupBindLastCutsceneObject(ushort groupId)
        {
            if (m_pPlayable == null)
            {
                return null;
            }
            return m_pPlayable.GetBindLastCutsceneObject(groupId);
        }
        //-----------------------------------------------------
        public void RegisterCallback(ICutsceneCallback callback)
        {
            if (callback == null) return;
            if (m_vCallbacks == null)
                m_vCallbacks = new List<ICutsceneCallback>(1);
            if (!m_vCallbacks.Contains(callback))
                m_vCallbacks.Add(callback);
        }
        //-----------------------------------------------------
        public void UnregisterCallback(ICutsceneCallback callback)
        {
            if (callback == null) return;
            if (m_vCallbacks == null) return;
            if (m_vCallbacks.Contains(callback))
                m_vCallbacks.Remove(callback);
        }
        //-----------------------------------------------------
        internal int CreatePlayable(short cutsceneId)
        {
            if (m_pData == null || m_pData.vCutscenes == null)
                return -1;
            for(int i =0; i < m_pData.vCutscenes.Count; ++i)
            {
                if (m_pData.vCutscenes[i].cutSceneData.id == cutsceneId)
                {
                    return CreatePlayable(m_pData.vCutscenes[i].cutSceneData);
                }
            }
            return -1;
        }
        //-----------------------------------------------------
        internal int CreatePlayable(CutsceneData cutsceneData)
        {
            if (cutsceneData == null)
                return -1;
            if(m_pPlayable!=null)
            {
                m_pMgr.FreePlayable(m_pPlayable);
                m_pPlayable = null;
            }
            CutscenePlayable playable = m_pMgr.MallocPlayable();
            playable.SetCutscene(this);
            if (!playable.Create(cutsceneData))
            {
                m_pMgr.FreePlayable(playable);
                return -1;
            }
            m_pPlayable = playable;
            return playable.GetId();
        }
        //-----------------------------------------------------
        public void BindDriver(ACutsceneDriver pDriver, EDataType type = EDataType.eNone, int typeId = 0, uint cusomtType =0)
        {
            if (pDriver == null)
                return;
            if (type == EDataType.eNone && typeId == 0)
                m_pBindDriver = pDriver;
            else
            {
                long key = CutscenePool.GetDaterKey(type, typeId, cusomtType);
                if (m_vBindKeyDrivers == null)
                {
                    m_vBindKeyDrivers = new Dictionary<long, ACutsceneDriver>(2);
                }
                m_vBindKeyDrivers[key] = pDriver;
            }
        }
        //-----------------------------------------------------
        ACutsceneDriver GetKeyDriver(EDataType type , IDataer typeId)
        {
            if (m_vBindKeyDrivers == null) return null;
            if (m_vBindKeyDrivers.TryGetValue(CutscenePool.GetDaterKey(EDataType.eClip, typeId), out var dirver))
                return dirver;
            if (m_vBindKeyDrivers.TryGetValue(CutscenePool.GetDaterKey(EDataType.eClip, typeId.GetIdType(),0), out dirver))
                return dirver;
            if (m_vBindKeyDrivers.TryGetValue(CutscenePool.GetDaterKey(EDataType.eClip, 0), out dirver))
                return dirver;
            return null;
        }
        //-----------------------------------------------------
        internal bool Update(float deltaTime)
        {
            if (!m_bEnable)
                return true;

            if (m_pPlayable == null && m_pAgentTree == null)
                return false;

            if(m_pPlayable != null)
            {
                m_pPlayable.Update(deltaTime);
                if(!IsEditorMode())
                {
                    if (m_pPlayable.GetStatus() == EPlayableStatus.Stop ||
                        m_pPlayable.GetStatus() == EPlayableStatus.Destroy)
                    {
                        m_pMgr.FreePlayable(m_pPlayable);
                        m_pPlayable = null;
                    }
                }
            }
            if(m_pAgentTree!=null)
            {
                if(!m_pAgentTree.Update(deltaTime))
                {
                    m_pMgr.FreeAgentTree(m_pAgentTree);
                    m_pAgentTree = null;
                }
            }
            return !(m_pPlayable == null && m_pAgentTree == null);
        }
        //-----------------------------------------------------
        public void Evaluate(float time)
        {
            if (m_pPlayable == null)
                return;
            m_pPlayable.Evaluate(time);
        }
        //-----------------------------------------------------
        public void RegisterAgentTreeCallback(IAgentTreeCallback callback)
        {
            if (m_pAgentTree == null) return;
            m_pAgentTree.RegisterCallback(callback);
        }
        //-----------------------------------------------------
        public void UnregisterAgentTreeCallback(IAgentTreeCallback callback)
        {
            if (m_pAgentTree == null) return;
            m_pAgentTree.UnregisterCallback(callback);
        }
        //-----------------------------------------------------
        internal bool OnAgentTreeExecute(AgentTree pAgentTree, BaseNode pNode)
        {
            if (m_vBindKeyDrivers != null)
            {
                foreach (var db in m_vBindKeyDrivers)
                {
                    if (db.Value.OnAgentTreeExecute(pAgentTree, pNode))
                        return true;
                }
            }

            if (m_pBindDriver != null)
            {
                if (m_pBindDriver.OnAgentTreeExecute(pAgentTree, pNode))
                    return true;
            }
            return m_pMgr.OnAgentTreeExecute(pAgentTree, pNode);
        }
        //-----------------------------------------------------
        internal void OnCutsceneStatus(EPlayableStatus eStatus)
        {
            m_pMgr.OnCutsceneStatus(this, eStatus);
        }
        //-----------------------------------------------------
        public virtual void OnPlayableCreate(CutscenePlayable playable) { }
        //-----------------------------------------------------
        public virtual void OnPlayableDestroy(CutscenePlayable playable) { }
        //-----------------------------------------------------
        public virtual void OnPlayableStop(CutscenePlayable playable)
        {
            if (m_pAgentTree != null)
            {
                var argvs = VariableList.Malloc();
                argvs.AddInt(GetGUID());
                argvs.AddInt(playable.GetId());
                m_pAgentTree.ExecuteTask((int)ETaskType.eCutscenePlayableStopedCallback, argvs);
            }
        }
        //-----------------------------------------------------
        public virtual void OnPlayablePlay(CutscenePlayable playable) 
        {
            if(m_pAgentTree!=null)
            {
                var argvs = VariableList.Malloc();
                argvs.AddInt(GetGUID());
                argvs.AddInt(playable.GetId());
                m_pAgentTree.ExecuteTask((int)ETaskType.eCutscenePlayablePlayedCallback, argvs);
            }
        }
        //-----------------------------------------------------
        public virtual void OnPlayablePause(CutscenePlayable playable) 
        {
            if (m_pAgentTree != null)
            {
                var argvs = VariableList.Malloc();
                argvs.AddInt(GetGUID());
                argvs.AddInt(playable.GetId());
                m_pAgentTree.ExecuteTask((int)ETaskType.eCutscenePlayablePauseCallback, argvs);
            }
        }
        //-----------------------------------------------------
        public virtual void OnPlayableResume(CutscenePlayable playable) 
        {
            if (m_pAgentTree != null)
            {
                var argvs = VariableList.Malloc();
                argvs.AddInt(GetGUID());
                argvs.AddInt(playable.GetId());
                m_pAgentTree.ExecuteTask((int)ETaskType.eCutscenePlayableResumeCallback, argvs);
            }
        }
        //-----------------------------------------------------
        public virtual void OnCreateClip(CutsceneTrack track, IBaseClip clip)
        {
            var driver = GetKeyDriver(EDataType.eClip, clip);
            if (driver != null && driver.OnCreateClip(track, clip))
                return;

            if (m_pBindDriver != null)
            {
                if (m_pBindDriver.OnCreateClip(track, clip))
                    return;
            }
            m_pMgr.OnCreatePlayableClip(m_pPlayable, track, clip);
        }
        //-----------------------------------------------------
        public virtual void OnDestroyClip(CutsceneTrack track, IBaseClip clip)
        {
            var driver = GetKeyDriver(EDataType.eClip, clip);
            if (driver != null && driver.OnDestroyClip(track, clip))
                return;
            if (m_pBindDriver != null)
            {
                if (m_pBindDriver.OnDestroyClip(track, clip))
                    return;
            }
            m_pMgr.OnDestroyPlayableClip(m_pPlayable, track, clip);
        }
        //-----------------------------------------------------
        public virtual void OnFrameClip(FrameData frameData)
        {
            var driver = GetKeyDriver(EDataType.eClip, frameData.clip);
            if (driver != null && driver.OnFrameClip(frameData.ownerTrack, frameData))
                return;
            if (m_pBindDriver != null)
            {
                if (m_pBindDriver.OnFrameClip(frameData.ownerTrack, frameData))
                    return;
            }

            m_pMgr.OnFramePlayableClip(m_pPlayable, frameData);
        }
        //-----------------------------------------------------
        public virtual void OnFrameClipEnter(CutsceneTrack track, FrameData frameData)
        {
            var driver = GetKeyDriver(EDataType.eClip, frameData.clip);
            if (driver != null && driver.OnClipEnter(track, frameData))
                return;
            if (m_pBindDriver != null)
            {
                if (m_pBindDriver.OnClipEnter(track, frameData))
                    return;
            }

            m_pMgr.OnFramePlayableClipEnter(m_pPlayable, track, frameData);
        }
        //-----------------------------------------------------
        public virtual void OnUpdateClip(CutsceneTrack track, FrameData frameData)
        {

        }
        //-----------------------------------------------------
        public virtual void OnFrameClipLeave(CutsceneTrack track, FrameData frameData)
        {
            var driver = GetKeyDriver(EDataType.eClip, frameData.clip);
            if (driver != null && driver.OnClipLeave(track, frameData))
                return;
            if (m_pBindDriver != null)
            {
                if (m_pBindDriver.OnClipLeave(track, frameData))
                    return;
            }
            m_pMgr.OnFramePlayableClipLeave(m_pPlayable, track, frameData);
        }
        //-----------------------------------------------------
        public virtual void OnEventTrigger(CutsceneTrack track, IBaseEvent pEvt)
        {
            var driver = GetKeyDriver(EDataType.eEvent, pEvt);
            if (driver != null && driver.OnEventTrigger(track, pEvt))
                return;
            if (m_pBindDriver != null)
            {
                if (m_pBindDriver.OnEventTrigger(track, pEvt))
                    return;
            }
            m_pMgr.OnFramePlayableEventTrigger(m_pPlayable, track, pEvt);
        }
        //-----------------------------------------------------
        internal ACutsceneDriver CreateDriver(EDataType type, IDataer pDater)
        {
            return m_pMgr.CreateDriver(this,type, pDater);
        }
        //-----------------------------------------------------
        internal void FreeDriver(ACutsceneDriver pDriver)
        {
            m_pMgr.FreeDriver(pDriver);
        }
        //-----------------------------------------------------
        void Clear()
        {
            m_bEnable = false;
            m_BindData = null;
            if (m_pPlayable != null)
            {
                m_pMgr.FreePlayable(m_pPlayable);
                m_pPlayable = null;
            }
            if(m_pAgentTree!=null)
            {
                m_pMgr.FreeAgentTree(m_pAgentTree);
                m_pAgentTree = null;
            }
            m_pData = null;
            m_nPlayableGuid = 0;
            if (m_vObjIds != null)
            {
                foreach (var db in m_vObjIds)
                {
                    var objCache = db.Value;
                    if (objCache.autoDestroy)
                        objCache.pObj.Destroy();
                }
                m_vObjIds.Clear();
            }
          //  m_bEditorMode = false;
//#if UNITY_EDITOR
//            m_pOwnerEditor = null;
//#endif
        }
        //-----------------------------------------------------
        internal void Destroy()
        {
            Clear();
            m_strName = null;
            m_BindData = null;
            m_nGUID = 0;
            m_pBindDriver = null;
            if (m_vBindKeyDrivers != null)
                m_vBindKeyDrivers.Clear();
            if (m_vCallbacks != null)
                m_vCallbacks.Clear();
        }
        //-----------------------------------------------------
        public bool IsDestroyed()
        {
            return m_nGUID == 0 && m_pPlayable == null && m_pAgentTree == null;
        }
    }
}