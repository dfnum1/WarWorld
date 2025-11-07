/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneManager
作    者:	HappLI
描    述:	过场动画管理器
*********************************************************************/
using Framework.AT.Runtime;
using System.Collections.Generic;

using UnityEngine;
namespace Framework.Cutscene.Runtime
{
    public interface ICutsceneCallback
    {
        void OnCutsceneLoadAsset(string name, System.Action<UnityEngine.Object> onLoaded, bool bAsync = true);
        void OnCutsceneUnloadAsset(UnityEngine.Object pAsset);
        void OnCutsceneSpawnInstance(string name, System.Action<UnityEngine.GameObject> onLoaded, bool bAsync = true);
        void OnCutsceneDespawnInstance(GameObject pInstance, string name = null);
        void OnCutsceneStatus(int cutsceneInstanceId, EPlayableStatus status);
        bool OnCutscenePlayableCreateClip(CutscenePlayable playable, CutsceneTrack track, IBaseClip clip);
        bool OnCutscenePlayableDestroyClip(CutscenePlayable playable, CutsceneTrack track, IBaseClip clip);
        bool OnCutscenePlayableFrameClip(CutscenePlayable playable, FrameData frameData);
        bool OnCutscenePlayableFrameClipEnter(CutscenePlayable playable, CutsceneTrack track, FrameData frameData);
        bool OnCutscenePlayableFrameClipLeave(CutscenePlayable playable, CutsceneTrack track, FrameData frameData);
        bool OnCutsceneEventTrigger(CutscenePlayable pPlayablle, CutsceneTrack pTrack, IBaseEvent pEvent);
        bool OnAgentTreeExecute(AgentTree pAgentTree, BaseNode pNode);

    }
    public class CutsceneManager
    {
        private List<ICutsceneCallback>             m_vCallbacks = null;
        private Dictionary<int, CutsceneInstance>   m_vCutscenes = null;
        private LinkedList<CutsceneInstance>        m_vDoList = new LinkedList<CutsceneInstance>();

        private int                                 m_nAutoGUID = 0;

        public delegate IDataer CreateDataerDelegate(EDataType type, ushort typeId);
        public static CreateDataerDelegate          OnCreateDataer = null;
        public delegate ACutsceneDriver CreateDriverDelegate(long key);
        public static CreateDriverDelegate          OnCreateDriver = null;
#if UNITY_EDITOR
        private bool                                m_bEditMode = false;
#endif
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
        public void SetEditorMode(bool bEditor)
        {
#if UNITY_EDITOR
            m_bEditMode = bEditor;
#endif
        }
        //-----------------------------------------------------
        public bool IsEditorMode()
        {
#if UNITY_EDITOR
            return m_bEditMode;
#endif
            return false;
        }
        //-----------------------------------------------------
        public int CreateCutscene(string cutsceneName, string subName = null, bool bAsync = false, bool bEnable = true)
        {
            if (string.IsNullOrEmpty(cutsceneName))
            {
                Debug.LogError("CutsceneManager: Cutscene name cannot be null or empty.");
                return -1;
            }
            if (m_vCallbacks == null || m_vCallbacks.Count<=0)
            {
                Debug.LogError("CutsceneManager: No callbacks registered to load cutscene data.");
                return -1;
            }

            if (m_vCutscenes == null)
                m_vCutscenes = new Dictionary<int, CutsceneInstance>(4);

            CutsceneInstance cutscene = CutscenePool.MallocCutscene(this);
            cutscene.SetGUID(AutoGUID());
            cutscene.SetCutsceneManager(this);
            int playableGuid = cutscene.GetGUID();
            m_vCutscenes.Add(playableGuid, cutscene);
            if (string.IsNullOrEmpty(subName))
                Debug.Log("CutsceneManager: Creating cutscene with name " + cutsceneName);
            else
                Debug.Log("CutsceneManager: Creating cutscene with name " + cutsceneName + " and subName " + subName);

            //! lamda function gc.....
            m_vCallbacks[0].OnCutsceneLoadAsset(cutsceneName, (cutAsset) =>
                {
                    CutsceneObject pAsset = cutAsset as CutsceneObject;
                    if (pAsset == null || pAsset.GetCutsceneGraph() == null || !cutscene.Create(pAsset.GetCutsceneGraph(), subName))
                    {
                        CutscenePool.FreeCutscene(cutscene);
                    }
                    else
                    {
                        cutscene.SetName(cutsceneName);
                        EnableCutscene(cutscene.GetGUID(), bEnable);
                    }
                }, bAsync);
            cutscene.SetName(cutsceneName);
            return cutscene.GetGUID();
        }
        //-----------------------------------------------------
        public int CreateCutscene(string cutsceneName, int subId = -1, bool bAsync = false, bool bEnable = true)
        {
            if (m_vCallbacks == null || m_vCallbacks.Count <= 0)
            {
                Debug.LogError("CutsceneManager: No callbacks registered to load cutscene data.");
                return -1;
            }

            if (m_vCutscenes == null)
                m_vCutscenes = new Dictionary<int, CutsceneInstance>(4);

            CutsceneInstance cutscene = CutscenePool.MallocCutscene(this);
            cutscene.SetGUID(AutoGUID());
            cutscene.SetCutsceneManager(this);
            int playableGuid = cutscene.GetGUID();
            m_vCutscenes.Add(playableGuid, cutscene);
            Debug.Log("CutsceneManager: Creating cutscene with name " + cutsceneName + " and subId " + subId);
            //! lamda function gc.....
            m_vCallbacks[0].OnCutsceneLoadAsset(cutsceneName, (cutAsset) =>
            {
                CutsceneObject pAsset = cutAsset as CutsceneObject;
                if (pAsset == null || pAsset.GetCutsceneGraph() == null || !cutscene.Create(pAsset.GetCutsceneGraph(), null, subId))
                {
                    CutscenePool.FreeCutscene(cutscene);
                }
                else
                {
                    cutscene.SetName(cutsceneName);
                    EnableCutscene(cutscene.GetGUID(), bEnable);
                }
            }, bAsync);
            cutscene.SetName(cutsceneName);
            return cutscene.GetGUID();
        }
        //-----------------------------------------------------
        public CutsceneInstance CreateCutscene(CutsceneGraph cutsceneGraph, string cutsceneName, bool bEnable = true)
        {
            if (cutsceneGraph == null)
                return null;
            if (m_vCutscenes == null)
                m_vCutscenes = new Dictionary<int, CutsceneInstance>(4);
            CutsceneInstance cutscene = CutscenePool.MallocCutscene(this);
            cutscene.SetGUID(AutoGUID());
            cutscene.SetCutsceneManager(this);
            if (!cutscene.Create(cutsceneGraph))
            {
                CutscenePool.FreeCutscene(cutscene);
                return null;
            }
            cutscene.Enable(bEnable);
            cutscene.SetName(cutsceneName);
            m_vCutscenes.Add(cutscene.GetGUID(), cutscene);
            return cutscene;
        }
        //-----------------------------------------------------
        public void EnableCutscene(int cutsceneGuid, bool bEnable)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return;
            if (m_vCutscenes.TryGetValue(cutsceneGuid, out var cutscene))
            {
                cutscene.Enable(bEnable);
            }
        }
        //-----------------------------------------------------
        public bool ExecuteTask(int cutsceneGuid, int type, VariableList vArgvs = null, bool bAutoReleaseAgvs = true)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0 || type == 0)
                return false;
            if (m_vCutscenes.TryGetValue(cutsceneGuid, out var cutscene))
            {
                return cutscene.ExecuteTask(type, vArgvs, bAutoReleaseAgvs);
            }
            return false;
        }
        //-----------------------------------------------------
        public void PlayCutscene(int cutsceneGuid)
        {
            if(m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return;
            if (m_vCutscenes.TryGetValue(cutsceneGuid, out var cutscene))
            {
                cutscene.Play();
            }
            else
            {
                Debug.LogError("CutsceneManager: Cutscene with GUID " + cutsceneGuid + " not found.");
            }
        }
        //-----------------------------------------------------
        public bool StopCutscene(int cutsceneGuid)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return false;
            if (m_vCutscenes.TryGetValue(cutsceneGuid, out var cutscene))
            {
                cutscene.Stop();
                return true;
            }
            else
            {
                Debug.LogError("CutsceneManager: Cutscene with GUID " + cutsceneGuid + " not found.");
            }
            return false;
        }
        //-----------------------------------------------------
        public bool StopCutscene(string name)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0 || string.IsNullOrEmpty(name))
                return false;
            bool bFound = false;
            foreach (var db in m_vCutscenes)
            {
                if (name.CompareTo(db.Value.GetName()) == 0)
                {
                    db.Value.Stop();
                    bFound = true;
                }
            }
            return bFound;
        }
        //-----------------------------------------------------
        public bool PauseCutscene(int cutsceneGuid)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return false;
            if (m_vCutscenes.TryGetValue(cutsceneGuid, out var cutscene))
            {
                cutscene.Pause();
                return true;
            }
            else
            {
                Debug.LogError("CutsceneManager: Cutscene with GUID " + cutsceneGuid + " not found.");
            }
            return false;
        }
        //-----------------------------------------------------
        public bool PauseCutscene(string name)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0 || string.IsNullOrEmpty(name))
                return false;
            bool bFound = false;
            foreach (var db in m_vCutscenes)
            {
                if (name.CompareTo(db.Value.GetName()) == 0)
                {
                    db.Value.Pause();
                    bFound = true;
                }
            }
            return bFound;
        }
        //-----------------------------------------------------
        public bool ResumeCutscene(int cutsceneGuid)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return false;
            if (m_vCutscenes.TryGetValue(cutsceneGuid, out var cutscene))
            {
                cutscene.Resume();
                return true;
            }
            else
            {
                Debug.LogError("CutsceneManager: Cutscene with GUID " + cutsceneGuid + " not found.");
            }
            return false;
        }
        //-----------------------------------------------------
        public bool ResumeCutscene(string name)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0 || string.IsNullOrEmpty(name))
                return false;
            bool bFound = false;
            foreach (var db in m_vCutscenes)
            {
                if(name.CompareTo(db.Value.GetName())==0)
                {
                    db.Value.Resume();
                    bFound = true;
                }
            }
            return bFound;
        }
        //-----------------------------------------------------
        public void StopAllCutscenes()
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return;
            foreach (var cutscene in m_vCutscenes.Values)
            {
                cutscene.Stop();
            }
        }
        //-----------------------------------------------------
        public void PauseAllCutscenes()
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return;
            foreach (var cutscene in m_vCutscenes.Values)
            {
                cutscene.Pause();
            }
        }
        //-----------------------------------------------------
        public void ResumeAllCutscenes()
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return;
            foreach (var cutscene in m_vCutscenes.Values)
            {
                cutscene.Resume();
            }
        }
        //-----------------------------------------------------
        public bool IsCutscenePlaying(string name = null)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return false;
            if (!string.IsNullOrEmpty(name))
            {
                foreach (var db in m_vCutscenes)
                {
                    if (db.Value.GetPlayable() == null)
                        continue;
                    if (db.Value.IsEnable() && name.CompareTo(db.Value.GetName()) == 0)
                        return true;
                }
            }
            else
            {
                foreach (var db in m_vCutscenes)
                {
                    if (db.Value.IsEnable())
                        return true;
                }    
            }

            return false;
        }
        //-----------------------------------------------------
        public List<string> GetPlayingCutsceneNameList()
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return null;
            List<string> nameList = new List<string>();
            foreach (var db in m_vCutscenes)
            {
                if (db.Value.IsEnable())
                    nameList.Add(db.Value.GetName());
            }

            return nameList;
        }
        //-----------------------------------------------------
        public EPlayableStatus GetCutsceneStatus(int cutsceneGuid)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return EPlayableStatus.None;
            if (m_vCutscenes.TryGetValue(cutsceneGuid, out var cutscene))
            {
                return cutscene.GetStatus();
            }
            return EPlayableStatus.None;
        }
        //-----------------------------------------------------
        public CutsceneInstance GetCutscene(int cutsceneGuid)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0)
                return null;
            if (m_vCutscenes.TryGetValue(cutsceneGuid, out var cutscene))
            {
                return cutscene;
            }
            Debug.LogError("CutsceneManager: Cutscene with GUID " + cutsceneGuid + " not found.");
            return null;
        }
        //-----------------------------------------------------
        internal CutsceneInstance GetCutscene(string name)
        {
            if (m_vCutscenes == null || m_vCutscenes.Count <= 0 || string.IsNullOrEmpty(name))
                return null;
            foreach (var db in m_vCutscenes)
            {
                if (db.Value.GetPlayable() == null)
                    continue;
                if (db.Value.IsEnable() && name.CompareTo(db.Value.GetName()) == 0)
                    return db.Value;
            }
            return null;
        }
        //-----------------------------------------------------
        internal Dictionary<int, CutsceneInstance> GetAllCutscenes()
        {
            return m_vCutscenes;
        }
        //-----------------------------------------------------
        public bool LoadAsset(string file, System.Action<UnityEngine.Object> onCallback, bool bAsync= true)
        {
            if (string.IsNullOrEmpty(file))
                return false;
#if UNITY_EDITOR
            if (m_vCallbacks == null || m_vCallbacks.Count <= 0)
            {
                var asset = Editor.DataUtils.EditLoadUnityObject(file);
                if (asset != null)
                {
                    if (onCallback != null) onCallback(asset);
                    return true;
                }
            }
#endif

            if (m_vCallbacks == null || m_vCallbacks.Count<=0)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    Debug.LogError("CutsceneManager: No callbacks registered to load asset " + file);
#else
                Debug.LogError("CutsceneManager: No callbacks registered to load asset " + file);
#endif
                return false;
            }
            m_vCallbacks[0].OnCutsceneLoadAsset(file, onCallback, bAsync);
            return true;
        }
        //-----------------------------------------------------
        public void UnloadAsset(UnityEngine.Object pAsset)
        {
            if (pAsset == null)
                return;
            if (m_vCallbacks == null || m_vCallbacks.Count <= 0)
            {
#if UNITY_EDITOR
                if (!m_bEditMode && Application.isPlaying)
                    Debug.LogError("CutsceneManager: No callbacks registered to UnloadAsset");
#else
                Debug.LogError("CutsceneManager: No callbacks registered to UnloadAsset ");
#endif
                return;
            }
            m_vCallbacks[0].OnCutsceneUnloadAsset(pAsset);
        }
        //-----------------------------------------------------
        public bool SpawnInstance(string file, System.Action<UnityEngine.GameObject> onCallback, bool bAsync = true)
        {
            if (string.IsNullOrEmpty(file))
                return false;
#if UNITY_EDITOR
            if (m_vCallbacks == null || m_vCallbacks.Count <= 0)
            {
                var obj = Editor.DataUtils.EditLoadUnityObject(file);
                if (obj == null || !(obj is UnityEngine.GameObject))
                    return false;
                if (onCallback != null) onCallback(GameObject.Instantiate(obj as GameObject));
                return true;
            }
#endif
            if (m_vCallbacks == null || m_vCallbacks.Count <= 0)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    Debug.LogError("CutsceneManager: No callbacks registered to spawn instance " + file);
#else
                Debug.LogError("CutsceneManager: No callbacks registered to spawn instance " + file);
#endif
                return false;
            }
            m_vCallbacks[0].OnCutsceneSpawnInstance(file, onCallback, bAsync);
            return true;
        }
        //-----------------------------------------------------
        public void DespawnInstance(GameObject pInstance, string name = null)
        {
            if (pInstance == null)
                return;
#if UNITY_EDITOR
            if (m_vCallbacks == null || m_vCallbacks.Count <= 0)
            {
                if (Application.isPlaying) GameObject.Destroy(pInstance);
                else GameObject.DestroyImmediate(pInstance);
            }
#endif
            if (m_vCallbacks == null || m_vCallbacks.Count <= 0)
            {
#if UNITY_EDITOR
                if(!m_bEditMode && Application.isPlaying)
                    Debug.LogWarning("CutsceneManager: No callbacks registered to despawn instance " + name);
#else
                Debug.LogWarning("CutsceneManager: No callbacks registered to despawn instance " + name);
#endif
                if (pInstance) GameObject.Destroy(pInstance);
                return;
            }
            m_vCallbacks[0].OnCutsceneDespawnInstance(pInstance, name);
        }
        //-----------------------------------------------------
        public void SetBindDriver(int playableGuid, ACutsceneDriver pDriver, EDataType dataType = EDataType.eNone, int type = 0)
        {
            if (m_vCutscenes == null)
                return;
            if (m_vCutscenes.TryGetValue(playableGuid, out var playable))
            {
                playable.BindDriver(pDriver, dataType, type);
            }
        }
        //-----------------------------------------------------
        public void Update(float deltaTime, CutsceneInstance pIngore = null)
        {
            if (m_vCutscenes == null)
                return;
            m_vDoList.Clear();
            foreach (var db in m_vCutscenes)
            {
                if (pIngore!=null && pIngore == db.Value) continue;
                m_vDoList.AddLast(db.Value);
            }

            for(var node = m_vDoList.First; node != null; )
            {
                var next = node.Next;
                if (node.Value.IsEnable())
                {
                    if (!node.Value.Update(deltaTime))
                    {
                        m_vCutscenes.Remove(node.Value.GetGUID());
                        CutscenePool.FreeCutscene(node.Value);
                    }
                }
                node = next;
            }
            CutscenePool.ClearCache();
        }
        //-----------------------------------------------------
        public void Shutdown()
        {
            if (m_vCutscenes != null)
            {
                foreach (var db in m_vCutscenes)
                {
                    db.Value.Destroy();
                }
                m_vCutscenes.Clear();
            }
            m_vDoList.Clear();
            m_nAutoGUID = 0;
            if (m_vCallbacks!=null) m_vCallbacks.Clear();
        }
        //-----------------------------------------------------
        internal void OnCutsceneStatus(CutsceneInstance cutscene, EPlayableStatus status)
        {
            if (m_vCallbacks == null || m_vCallbacks.Count <= 0)
                return;
            foreach (var db in m_vCallbacks)
            {
                db.OnCutsceneStatus(cutscene.GetGUID(), status);
            }
        }
        //-----------------------------------------------------
        internal bool OnAgentTreeExecute(AgentTree pAgentTree, BaseNode pNode)
        {
            if (m_vCallbacks == null) return false;
            foreach(var db in m_vCallbacks)
            {
                if (db.OnAgentTreeExecute(pAgentTree, pNode))
                    return true;
            }
            return false;
        }
        //-----------------------------------------------------
        internal void OnCreatePlayableClip(CutscenePlayable playAble, CutsceneTrack track, IBaseClip clip)
        {
            if (m_vCallbacks == null)
                return;
            foreach (var db in m_vCallbacks)
            {
                if (db.OnCutscenePlayableCreateClip(playAble, track, clip))
                    return;
            }
        }
        //-----------------------------------------------------
        internal void OnDestroyPlayableClip(CutscenePlayable playAble, CutsceneTrack track, IBaseClip clip)
        {
            if (m_vCallbacks == null)
                return;
            foreach (var db in m_vCallbacks)
            {
                if (db.OnCutscenePlayableDestroyClip(playAble, track, clip))
                    return;
            }
        }
        //-----------------------------------------------------
        internal void OnFramePlayableClip(CutscenePlayable playAble, FrameData frameData)
        {
            if (m_vCallbacks == null)
                return;
            foreach (var db in m_vCallbacks)
            {
                if (db.OnCutscenePlayableFrameClip(playAble, frameData))
                    return;
            }
        }
        //-----------------------------------------------------
        internal void OnFramePlayableClipEnter(CutscenePlayable playAble, CutsceneTrack track, FrameData frameData)
        {
            if (m_vCallbacks == null)
                return;
            foreach (var db in m_vCallbacks)
            {
                if (db.OnCutscenePlayableFrameClipEnter(playAble, track, frameData))
                    return;
            }
        }
        //-----------------------------------------------------
        internal void OnFramePlayableClipLeave(CutscenePlayable playAble, CutsceneTrack track, FrameData frameData)
        {
            if (m_vCallbacks == null)
                return;
            foreach (var db in m_vCallbacks)
            {
                if (db.OnCutscenePlayableFrameClipLeave(playAble, track, frameData))
                    return;
            }
        }
        //-----------------------------------------------------
        internal void OnFramePlayableEventTrigger(CutscenePlayable playAble, CutsceneTrack track, IBaseEvent pEvt)
        {
            if (m_vCallbacks == null)
                return;
            foreach (var db in m_vCallbacks)
            {
                if (db.OnCutsceneEventTrigger(playAble, track, pEvt))
                    return;
            }
        }
        //-----------------------------------------------------
        int AutoGUID()
        {
            int guid = ++m_nAutoGUID;
            if (m_nAutoGUID > int.MaxValue - 1) m_nAutoGUID = 0;
            return guid;
        }
        //-----------------------------------------------------
        internal ACutsceneDriver CreateDriver(CutsceneInstance pCutscene, EDataType type, IDataer pDater)
        {
            var poolDriver = CutscenePool.MallocDriver(pCutscene, type, pDater);
            if (poolDriver != null)
            {
                poolDriver.SetCutscene(pCutscene);
                return poolDriver;
            }

            if (OnCreateDriver == null)
            {
                Debug.LogWarning("CutsceneManager: OnCreateDriver delegate is not set, cannot create driver of type " + type);
            }
            if (OnCreateDriver != null)
            {
                var driver = OnCreateDriver(CutscenePool.GetDaterKey(type, pDater));
                if(driver == null) driver = OnCreateDriver(CutscenePool.GetDaterKey(type, pDater.GetIdType(),0));
                if(driver == null) driver = OnCreateDriver(CutscenePool.GetDaterKey(type, 0, 0));
                if(driver != null)
                {
                    driver.SetKey(CutscenePool.GetDaterKey(type, pDater));
                    driver.SetCutscene(pCutscene);
                    return driver;
                }
            }
#if UNITY_EDITOR
            System.Type driverType = Editor.DataUtils.GetCustomDriver(type, pDater.GetIdType(), pDater.GetCustomTypeID());
            if(driverType == null)
            {
                driverType = Editor.DataUtils.GetCustomDriver(type, pDater.GetIdType());
            }
            if (driverType == null)
            {
                driverType = Editor.DataUtils.GetCustomDriver(type, 0);
            }
            if (driverType!=null)
            {
                ACutsceneDriver driver = System.Activator.CreateInstance(driverType) as ACutsceneDriver;
                if (driver != null)
                {
                    driver.SetKey(CutscenePool.GetDaterKey(type, pDater));
                    driver.SetCutscene(pCutscene);
                    return driver;
                }
            }
#endif
            return null;
        }
        //-----------------------------------------------------
        internal void FreeDriver(ACutsceneDriver pDater)
        {
            CutscenePool.FreeDriver(pDater);
        }
        //-----------------------------------------------------
        internal CutscenePlayable MallocPlayable()
        {
            return CutscenePool.MallocPlayable();
        }
        //-----------------------------------------------------
        internal void FreePlayable(CutscenePlayable pDater)
        {
            CutscenePool.FreePlayable(pDater);
        }
        //-----------------------------------------------------
        internal AgentTree MallocAgentTree()
        {
            return CutscenePool.MallocAgentTree();
        }
        //-----------------------------------------------------
        internal void FreeAgentTree(AgentTree pDater)
        {
            CutscenePool.FreeAgentTree(pDater);
        }
        //-----------------------------------------------------
        internal static IBaseClip CreateClip(ushort type)
        {
            if (type == (ushort)EClipType.eCustom)
                return new CutsceneCustomClip();
            else if (type == (ushort)EClipType.eCameraMove)
                return new CameraMoveClip();

            if (OnCreateDataer == null)
            {
#if !UNITY_EDITOR
                Debug.LogError("CutsceneManager: OnCreateDataer delegate is not set, cannot create event of type " + type);
                return null;// In non-editor mode, return null if delegate is not set
#else
                if(Application.isPlaying)
                    Debug.LogWarning("CutsceneManager: OnCreateDataer delegate is not set, cannot create event of type " + type);
#endif
            }
            if (OnCreateDataer != null)
            {
                IDataer dater = OnCreateDataer(EDataType.eClip, type);
                if (dater == null)
                {
                    Debug.LogError("CutsceneManager: Failed to create clip dataer of type " + type);
                    return null;
                }
                if (!(dater is IBaseClip))
                {
                    Debug.LogError("CutsceneManager: Created dataer is not an IBaseClip type for type " + type);
                    return null;
                }
                return dater as IBaseClip;
            }
#if UNITY_EDITOR
            var clipAttri = Editor.DataUtils.GetClipAttri(type);
            if (clipAttri != null)
                return clipAttri.CreateClip();
#endif
            return null;
        }
        //-----------------------------------------------------
        internal static IBaseEvent CreateEvent(ushort type)
        {
            if (type == (ushort)EEventType.eCustom)
                return new CutsceneCustomEvent();

            if (OnCreateDataer == null)
            {
#if !UNITY_EDITOR
                Debug.LogError("CutsceneManager: OnCreateDataer delegate is not set, cannot create event of type " + type);
                return null;// In non-editor mode, return null if delegate is not set
#else
                if (Application.isPlaying)
                    Debug.LogError("CutsceneManager: OnCreateDataer delegate is not set, cannot create event of type " + type);
#endif
            }
            if (OnCreateDataer != null)
            {
                IDataer dater = OnCreateDataer(EDataType.eEvent, type);
                if (dater == null)
                {
                    Debug.LogError("CutsceneManager: Failed to create event dataer of type " + type);
                    return null;
                }
                if (!(dater is IBaseEvent))
                {
                    Debug.LogError("CutsceneManager: Created dataer is not an IBaseEvent type for type " + type);
                    return null;
                }
                return dater as IBaseEvent;
            }
#if UNITY_EDITOR
            var clipAttri = Editor.DataUtils.GetEventAttri(type);
            if (clipAttri != null)
                return clipAttri.CreateEvent();
#endif
            return null;
        }
    }
}