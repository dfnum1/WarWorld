/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneData
作    者:	HappLI
描    述:	过场数据
*********************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    public interface ICustomSerialize
    {
        string OnSerialize();
        bool OnDeserialize(string content);
    }
    //-----------------------------------------------------
    public interface IDataer
    {
        ushort GetIdType();
        float GetTime();
        string GetName();
        ACutsceneDriver CreateDriver();
    }
    //-----------------------------------------------------
    public static class IDataerUtl
    {
        public static uint GetCustomTypeID(this IDataer pDater)
        {
            uint cusomtType = 0;
            if (pDater != null && pDater is CutsceneCustomEvent)
            {
                cusomtType = ((CutsceneCustomEvent)pDater).customType;
            }
            else if (pDater != null && pDater is CutsceneCustomClip)
            {
                cusomtType = ((CutsceneCustomClip)pDater).customType;
            }
            return cusomtType;
        }
        public static T Cast<T>(this IBaseClip clip) where T : IBaseClip
        {
            return (T)clip;
        }
        public static T Cast<T>(this IBaseEvent evt) where T : IBaseEvent
        {
            return (T)evt;
        }
    }
    public enum EDataType
    {
        eNone = 0,
        eClip = 1,   //剪辑
        eEvent = 2,  //事件
    }

    [System.Serializable]
    internal struct TrackData
    {
        public byte type;   //1-Clip, 2-Event
        public ushort typeId;
        public string content;
    }

    public enum EGroupFlag
    {
        [InspectorName("不生效")] UnActive= 1<<0,
    }

    [System.Serializable]
    public class CutsceneData
    {
        public string name;
        public ushort id;//唯一标识符
        public int frameRate = 60; //帧率
        public List<Group> groups;

        [System.Serializable]
        public class Group
        {
            public string name;
            public ushort id;   //唯一标识符
            public int binderId = -1;
            public bool isGroup = true;
            public ushort groupFlag;
            public List<Track> tracks;
            //-----------------------------------------------------
            public int GetTrackCount()
            {
                return tracks != null ? tracks.Count : 0;
            }
            //-----------------------------------------------------
            public bool HasFlag(EGroupFlag flag)
            {
                return (groupFlag & (ushort)flag) != 0;
            }
            //-----------------------------------------------------
            internal void EnableFlag(EGroupFlag flag, bool bEnable)
            {
                if (bEnable)
                {
                    groupFlag |= (ushort)flag;
                }
                else
                {
                    groupFlag &= (ushort)~flag;
                }
            }
            //-----------------------------------------------------
            public float GetStart()
            {
                float start = float.MaxValue-1.0f;
                if (tracks != null)
                {
                    foreach (var track in tracks)
                    {
                        if (track.clips != null)
                        {
                            foreach (var clip in track.clips)
                            {
                                if (clip != null && clip.GetTime() < start)
                                {
                                    start = Mathf.Min(start, clip.GetTime());
                                }
                            }
                        }
                        if (track.events != null)
                        {
                            foreach (var evt in track.events)
                            {
                                if (evt != null && evt.GetTime() < start)
                                {
                                    start = Mathf.Min(start, evt.GetTime());
                                }
                            }
                        }
                    }
                }
                if (start >= float.MaxValue - 1) start = 0.0f;
                return start;
            }
            //-----------------------------------------------------
            public float GetDuration()
            {
                float duration = 0.0f;
                if (tracks != null)
                {
                    foreach (var track in tracks)
                    {
                        if (track.clips != null)
                        {
                            foreach (var clip in track.clips)
                            {
                                if (clip != null && clip.GetTime() + clip.GetDuration() > duration)
                                {
                                    duration = Mathf.Max(duration, clip.GetTime() + clip.GetDuration());
                                }
                            }
                        }

                        if (track.events != null)
                        {
                            foreach (var evt in track.events)
                            {
                                if (evt != null && evt.GetTime() > duration)
                                {
                                    duration = Mathf.Max(duration, evt.GetTime());
                                }
                            }
                        }
                    }
                }
                return duration;
            }
            //-----------------------------------------------------
            public bool OnDeserialize(string jsonContent = null)
            {
                try
                {
                    if(!string.IsNullOrEmpty(jsonContent))
                    {
                        JsonUtility.FromJsonOverwrite(jsonContent, this);
                    }
                    if (tracks != null)
                    {
                        for (int i = 0; i < tracks.Count; ++i)
                        {
                            Track track = tracks[i];
                            track.OnDeserialize();
                        }
                    }
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    return false;
                }
            }
#if UNITY_EDITOR
            //-----------------------------------------------------
            public void OnSerialize()
            {
                if (tracks != null)
                {
                    foreach (var track in tracks)
                    {
                        track.OnSerialize();
                    }
                }
            }
#endif
        }

        [System.Serializable]
        public class Track
        {
            public string trackName;
            [SerializeField]internal List<TrackData> datas;
            //-----------------------------------------------------

            [System.NonSerialized]private List<IBaseClip> m_vClips;
            public List<IBaseClip> clips
            {
                get
                {
                    if (m_vClips == null) m_vClips = new List<IBaseClip>(4);
                    return m_vClips;
                }
            }
            //-----------------------------------------------------
            [System.NonSerialized] private List<IBaseEvent> m_vEvents;
            public List<IBaseEvent> events
            {
                get
                {
                    if (m_vEvents == null) m_vEvents = new List<IBaseEvent>(4);
                    return m_vEvents;
                }
            }
            //-----------------------------------------------------
            public bool IsValidTrack()
            {
                return (m_vClips != null && m_vClips.Count > 0) || (m_vEvents != null && m_vEvents.Count > 0);
            }
            //-----------------------------------------------------
            public void Clear()
            {
                if (m_vClips != null) m_vClips.Clear();
                if (m_vEvents != null) m_vEvents.Clear();
            }
            //-----------------------------------------------------
            public bool OnDeserialize(string jsonContent = null)
            {
                try
                {
                    Clear();
                    if(!string.IsNullOrEmpty(jsonContent))
                    {
                        JsonUtility.FromJsonOverwrite(jsonContent,this);
                    }
                    if (datas == null)
                        return true;
                    for (int j = 0; j < datas.Count; ++j)
                    {
                        TrackData data = datas[j];
                        if (data.type == 0 || data.typeId == 0)
                            continue;

                        switch (data.type)
                        {
                            case (byte)EDataType.eClip: //Clip
                                {
                                    IBaseClip clip = CutsceneManager.CreateClip(data.typeId);
                                    if (clip != null)
                                    {
                                        if (clip is ICustomSerialize)
                                        {
                                            ((ICustomSerialize)clip).OnDeserialize(data.content);
                                        }
                                        else
                                            JsonUtility.FromJsonOverwrite(data.content, clip);
                                        clips.Add(clip);
                                    }
                                    else
                                    {
                                        Debug.LogError($"Create Clip Failed! typeId:{data.typeId}");
                                        continue;
                                    }
                                }
                                break;
                            case (byte)EDataType.eEvent: //Event
                                {
                                    IBaseEvent evt = CutsceneManager.CreateEvent(data.typeId);
                                    if (evt != null)
                                    {
                                        if (evt is ICustomSerialize)
                                        {
                                            ((ICustomSerialize)evt).OnDeserialize(data.content);
                                        }
                                        else
                                            JsonUtility.FromJsonOverwrite(data.content, evt);
                                        events.Add(evt);
                                    }
                                    else
                                    {
                                        Debug.LogError($"Create Event Failed! typeId:{data.typeId}");
                                        continue;
                                    }
                                }
                                break;
                        }
                    }
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    return false;
                }
            }
#if UNITY_EDITOR
            //-----------------------------------------------------
            public void OnSerialize()
            {
                datas = new List<TrackData>();
                // 序列化 clips
                if (m_vClips!=null)
                {
                    foreach (var clip in m_vClips)
                    {
                        if (clip == null) continue;
                        var data = new TrackData
                        {
                            type = (byte)EDataType.eClip,
                            typeId = clip.GetIdType(),
                        };
                        if (clip is ICustomSerialize)
                            data.content = ((ICustomSerialize)clip).OnSerialize();
                        else
                            data.content = JsonUtility.ToJson(clip);
                        datas.Add(data);
                    }
                }
                
                // 序列化 events
                if (m_vEvents != null)
                {
                    foreach (var evt in m_vEvents)
                    {
                        if (evt == null) continue;
                        var data = new TrackData
                        {
                            type = (byte)EDataType.eEvent,
                            typeId = evt.GetIdType()
                        };
                        if (evt is ICustomSerialize)
                            data.content = ((ICustomSerialize)evt).OnSerialize();
                        else
                            data.content = JsonUtility.ToJson(evt);
                        datas.Add(data);
                    }
                }
            }
#endif
        }
        //-----------------------------------------------------
        public int GetGroupCount()
        {
            return groups != null ? groups.Count : 0;
        }
        //-----------------------------------------------------
        public int GetTrackCount()
        {
            int count = 0;
            if(groups!=null)
            {
                for (int i = 0; i < groups.Count; ++i)
                {
                    count += groups[i].GetTrackCount();
                }
            }

            return count;
        }
        //-----------------------------------------------------
        public Group GetGroup(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (groups != null)
            {
                foreach (var group in groups)
                {
                    if (name.CompareTo(group.name) ==0 )
                        return group;
                }
            }
            return null;
        }
        //-----------------------------------------------------
        public Group GetGroup(ushort groupId)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (groups != null)
            {
                foreach (var group in groups)
                {
                    if (group.id == groupId)
                        return group;
                }
            }
            return null;
        }
        //-----------------------------------------------------
        public float GetDuration()
        {
            float duration = 0.0f;
            if (groups != null)
            {
                foreach (var gp in groups)
                {
                    duration = Mathf.Max(duration, gp.GetDuration());
                }
            }
            return duration;
        }
        //-----------------------------------------------------
        public bool OnDeserialize(string content = null)
        {
            try
            {
                if(!string.IsNullOrEmpty(content))
                    JsonUtility.FromJsonOverwrite(content, this);
                if(groups!=null)
                {
                    for (int i =0; i < groups.Count; ++i)
                    {
                        Group group = groups[i];
                        group.OnDeserialize();
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        internal string OnSerialize()
        {
            if (groups != null)
            {
                foreach (var group in groups)
                {
                    if (group.id == 0)
                        group.id = GeneratorGroupId();
                    group.OnSerialize();
                }
            }

            // 最终序列化整个 CutsceneData
            return JsonUtility.ToJson(this, true);
        }
        //-----------------------------------------------------
        internal ushort GeneratorGroupId()
        {
            ushort id = 1;
            HashSet<ushort> vGp = new HashSet<ushort>();
            if (groups!=null)
            {
                foreach (var db in groups)
                {
                    vGp.Add(db.id);
                }
            }
            int stackCnt = 65535;
            while(vGp.Contains(id))
            {
                id++;
                stackCnt--;
                if (stackCnt <= 0) break;
            }
            return id;
        }
#endif
    }

}