/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneManager
作    者:	HappLI
描    述:	过场动画管理器
*********************************************************************/
using Framework.AT.Runtime;
using System.Collections.Generic;
namespace Framework.Cutscene.Runtime
{
    internal static class CutscenePool
    {
        private static int MAX_POOL = 32;
        private static Stack<CutsceneInstance> ms_vCutscenePool = null;
        private static Stack<CutscenePlayable> ms_vPlayablePool = null;
        private static Stack<AgentTree> ms_vAgentTreePool = null;
        private static Stack<CutsceneTrack> ms_vTrackPool = null;
        private static Dictionary<long, Stack<ACutsceneDriver>> ms_vDriverPools = null;
        static List<ACutsceneDriver> ms_vCacheDrivers = null;
        static List<CacheTrackDriver> ms_vCacheTrackDrivers = null;
        static List<int> ms_vCacheIndexs = null;
        //-----------------------------------------------------
        internal static List<CacheTrackDriver> CacheTrackDrivers
        {
            get
            {
                if (ms_vCacheTrackDrivers == null)
                    ms_vCacheTrackDrivers = new List<CacheTrackDriver>(8);
                ms_vCacheTrackDrivers.Clear();
                return ms_vCacheTrackDrivers;
            }
        }
        //-----------------------------------------------------
        internal static List<ACutsceneDriver> CacheDrivers
        {
            get
            {
                if (ms_vCacheDrivers == null)
                    ms_vCacheDrivers = new List<ACutsceneDriver>(8);
                ms_vCacheDrivers.Clear();
                return ms_vCacheDrivers;
            }
        }
        //-----------------------------------------------------
        internal static CutsceneInstance MallocCutscene(CutsceneManager pMgr)
        {
            CutsceneInstance cutsceneInstance = null;
            if (ms_vCutscenePool != null && ms_vCutscenePool.Count > 0)
                cutsceneInstance= ms_vCutscenePool.Pop();
            else
                cutsceneInstance = new CutsceneInstance(pMgr);
            cutsceneInstance.SetCutsceneManager(pMgr);
            return cutsceneInstance;
        }
        //-----------------------------------------------------
        internal static void FreeCutscene(CutsceneInstance cutscene)
        {
            if (cutscene == null) return;
            cutscene.Destroy();
            if (ms_vCutscenePool != null && ms_vCutscenePool.Count >= MAX_POOL)
                return;
            if (ms_vCutscenePool == null) ms_vCutscenePool = new Stack<CutsceneInstance>(MAX_POOL);
            ms_vCutscenePool.Push(cutscene);
        }
        //-----------------------------------------------------
        internal static CutscenePlayable MallocPlayable()
        {
            if (ms_vPlayablePool!=null && ms_vPlayablePool.Count > 0)
                return ms_vPlayablePool.Pop();
            return new CutscenePlayable();
        }
        //-----------------------------------------------------
        internal static void FreePlayable(CutscenePlayable playable)
        {
            if (playable == null) return;
            playable.Destroy();
            if (ms_vPlayablePool != null && ms_vPlayablePool.Count >= MAX_POOL)
                return;
            if (ms_vPlayablePool == null) ms_vPlayablePool = new Stack<CutscenePlayable>(MAX_POOL);
            ms_vPlayablePool.Push(playable);
        }
        //-----------------------------------------------------
        internal static AgentTree MallocAgentTree()
        {
            if (ms_vAgentTreePool != null && ms_vAgentTreePool.Count > 0)
                return ms_vAgentTreePool.Pop();
            return new AgentTree();
        }
        //-----------------------------------------------------
        internal static void FreeAgentTree(AgentTree agentTree)
        {
            if (agentTree == null) return;
            agentTree.Destroy();
            if (ms_vAgentTreePool != null && ms_vAgentTreePool.Count >= MAX_POOL)
                return;
            if (ms_vAgentTreePool == null) ms_vAgentTreePool = new Stack<AgentTree>(MAX_POOL);
            ms_vAgentTreePool.Push(agentTree);
        }
        //-----------------------------------------------------
        internal static CutsceneTrack MallockTrack()
        {
            if (ms_vTrackPool!=null && ms_vTrackPool.Count > 0)
                return ms_vTrackPool.Pop();
            return new CutsceneTrack();
        }
        //-----------------------------------------------------
        internal static void FreeTrack(CutsceneTrack track)
        {
            if (track == null)
                return;
            track.Destroy();
            if (ms_vTrackPool != null && ms_vTrackPool.Count >= MAX_POOL*4)
                return;
            if (ms_vTrackPool == null) ms_vTrackPool = new Stack<CutsceneTrack>(MAX_POOL*4);
            ms_vTrackPool.Push(track);
        }
        //-----------------------------------------------------
        internal static long GetDaterKey(EDataType type, IDataer pDater)
        {
            return GetDaterKey(type, pDater.GetIdType(), pDater.GetCustomTypeID());
        }
        //-----------------------------------------------------
        internal static long GetDaterKey(EDataType type, int typeId, uint customType =0)
        {
            long key = (long)type << 32 | (long)typeId << 16 | (long)customType;
            return key;
        }
        //-----------------------------------------------------
        internal static List<int> GetCacheIndexs()
        {
            if (ms_vCacheIndexs == null) ms_vCacheIndexs = new List<int>(2);
            ms_vCacheIndexs.Clear();
            return ms_vCacheIndexs;
        }
        //-----------------------------------------------------
        internal static ACutsceneDriver MallocDriver(CutsceneInstance pCutscene, EDataType type, IDataer pDater)
        {
            long key = GetDaterKey(type, pDater);
            if (ms_vDriverPools!=null)
            {
                if (ms_vDriverPools.TryGetValue(key, out var pools) && pools.Count > 0)
                {
                    ACutsceneDriver pPopDriver = pools.Pop();
                    pPopDriver.SetCutscene(pCutscene);
                    pPopDriver.SetKey(key);
                    return pPopDriver;
                }
            }
            ACutsceneDriver pDriver = pDater.CreateDriver();
            if (pDriver == null)
                return null;
            pDriver.SetCutscene(pCutscene);
            pDriver.SetKey(key);
            return pDriver;
        }
        //-----------------------------------------------------
        internal static void FreeDriver(ACutsceneDriver driver)
        {
            if (driver == null) return;
            long key = driver.GetKey();
            driver.OnDestroy();
            driver.SetCutscene(null);
            driver.SetKey(0);
            //key is 0, means this driver is not from pool, so we don't need to free it.
            if (key == 0)
                return;
            if (ms_vDriverPools != null && ms_vDriverPools.TryGetValue(key, out var pools))
            {
                if (pools.Count >= MAX_POOL * 2)
                    return;
                pools.Push(driver);
            }
            else
            {
                if (ms_vDriverPools == null)
                {
                    ms_vDriverPools = new Dictionary<long, Stack<ACutsceneDriver>>(8);
                }

                var tempPools = new Stack<ACutsceneDriver>(MAX_POOL * 2);
                tempPools.Push(driver);
                ms_vDriverPools[key] = tempPools;
            }
        }
        //-----------------------------------------------------
        internal static void ClearCache()
        {
            if (ms_vCacheDrivers != null) ms_vCacheDrivers.Clear();
            if (ms_vCacheTrackDrivers != null) ms_vCacheTrackDrivers.Clear();
            if (ms_vCacheIndexs != null) ms_vCacheIndexs.Clear();
        }
    }
}