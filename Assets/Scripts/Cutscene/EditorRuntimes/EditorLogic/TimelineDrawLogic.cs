/********************************************************************
生成日期:	06:30:2025
类    名: 	TimelineDrawLogic
作    者:	HappLI
描    述:	时间轴控制逻辑
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Runtime;
using Framework.ED;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using static Framework.Cutscene.Runtime.CutsceneData;

namespace Framework.Cutscene.Editor
{
    [EditorBinder(typeof(CutsceneEditor), "TimelineRect", 100)]
    public class TimelineDrawLogic : ACutsceneLogic
    {
        struct CopyDataCache
        {
            public System.Object pData;
            public string json;
            public void Clear()
            {
                pData = null;
                json = null;
            }
            public bool IsValid()
            {
                return json != null && pData != null;
            }
        }
        struct MenuData
        {
            public Vector2 mousePos;
            public object data;
            public object userData;
            public CutsceneCustomAgent.AgentUnit agentUnit;
        }
        CopyDataCache m_pStandByCopy = new CopyDataCache();
        public static float TimeRulerAreaHeight = 30;
        public static float LineItemExpand = 4.5f;
        public class TreeItem : TreeAssetView.ItemData
        {
            public Rect rect0;
            public Rect rect1;
            public Texture2D icon;
            public Color trackColor;
            public override Texture2D itemIcon()
            {
                return icon;
            }
            public override Color itemColor()
            {
                return trackColor;
            }
        }
        internal class EmptyItemLine : TreeItem
        {
        }
        internal class TimelineTrackGroup : TreeItem
        {
            public CutsceneData.Group group;
        }
        public class TimelineTrack : TreeItem
        {
            public Vector2 playableOffsetSize;
            public string playableName;
            internal TimelineTrackGroup ownerGroup;
            internal CutsceneData.Group groupData;

            public CutsceneData.Track track;
            public List<ClipDraw> clipDraws = new List<ClipDraw>();
            public List<EventDraw> eventDraws = new List<EventDraw>();
            public void RemoveClip(int index)
            {
                if (index < 0 || index >= clipDraws.Count)
                    return;
                clipDraws.RemoveAt(index);
                track.clips.RemoveAt(index);
            }
            public void AddClip(TimelineDrawLogic pLogic, IBaseClip clip)
            {
                track.clips.Add(clip);
                clipDraws.Add(new ClipDraw(pLogic,this, clip));
            }
            public void RemoveEvent(int index)
            {
                if (index < 0 || index >= eventDraws.Count)
                    return;
                eventDraws.RemoveAt(index);
                track.events.RemoveAt(index);
            }
            public void AddEvent(TimelineDrawLogic pLogic, IBaseEvent clip)
            {
                track.events.Add(clip);
                eventDraws.Add(new EventDraw(pLogic, this, clip));
            }
        }

        struct SelectDraw
        {
            public int index;
            public IDraw draw;
            public SelectDraw(IDraw draw, int index )
            {
                this.index = index;
                this.draw = draw;
            }
        }

        int m_LastFrameRate = 30;
        float m_fPlayTime = 0.0f;
        float m_fAccumoulator = 0;
        CutsceneInstance m_pCutscene = null;
        TimeArea.TimeFormat m_TimeFromat = TimeArea.TimeFormat.Seconds;

        List<ICutsceneObject> m_vCacheObjects = new List<ICutsceneObject>();
        List<SelectDraw> m_vTempDraws = new List<SelectDraw>();
        Rect m_TimelineRect;
        Rect m_TimeZoomRect;
        Rect m_RightRect;
        Rect m_LeftRect;
        private bool m_NeedSetShowRange = false;
        private Vector2 m_PendingShowRange = Vector2.zero;
        Vector2 m_TimeAreaShownRange = new Vector2(0, 10);
        bool m_bRightItemLine = false;
        bool m_TimeAreaDirty = false;
        TimelineTimeArea m_TimeArea;
        DragDropDraw m_pDragDrop = null;
        DragDraw m_pDragDraw = null;
        TreeAssetView m_pTimelineTree;
        TimeCursorDraw m_Cursor;
        TimeCursorDraw m_EndCursor;
        GUIStyle m_customToolbarButtonStyle;
        TreeAssetView.ItemData m_ItemSelected;
        bool m_bDragedAction = false;
        float m_fDragStartTime = 0.0f;

        List<IDraw> m_vGroupOffsetTimes = new List<IDraw>();
        float m_fGroupDragOffsetTime = 0;

        private bool m_MiddleMouseDragging = false;
        private Vector2 m_MiddleMouseDragLastPos;
        public int viewStateHash { get; private set; }
        public Vector2 timeAreaTranslation
        {
            get { return m_TimeArea.translation; }
        }
        //--------------------------------------------------------
        public Rect leftRect
        {
            get { return m_LeftRect; }
        }
        //--------------------------------------------------------
        public Rect rightRect
        {
            get { return m_RightRect; }
        }
        //--------------------------------------------------------
        public Rect timelineRect
        {
            get { return m_TimelineRect; }
        }
        //--------------------------------------------------------
        public Rect timeZoomRect
        {
            get { return m_TimeZoomRect; }
        }
        //--------------------------------------------------------
        public Vector2 timeAreaScale
        {
            get { return m_TimeArea.scale; }
        }
        //--------------------------------------------------------
        public Vector2 timeAreaShownRange
        {
            get { return m_TimeAreaShownRange; }
        }
        //--------------------------------------------------------
        public Vector2 ToWindowSpace(Vector2 position)
        {
            position -= m_pTimelineTree.state.scrollPos;
            position.y += GetRect().position.y;
            position.x += GetRect().position.x;
            //    position.y += m_TimelineRect.position.y;
            position.y += m_TimeArea.rect.y + TimeRulerAreaHeight;
            return position;
        }
        //--------------------------------------------------------
        protected override void OnEnable()
        {
            m_Cursor = new TimeCursorDraw(this);
            m_EndCursor = new TimeCursorDraw(this);
            m_EndCursor.drawHead = false;
            m_EndCursor.canDrag = false;
            if(m_EndCursor.style!=null)
                m_EndCursor.lineColor = m_EndCursor.style.normal.textColor;
            m_EndCursor.lineColor = new Color(m_EndCursor.lineColor.r, m_EndCursor.lineColor.g, m_EndCursor.lineColor.b,0.5f);
            m_pCutscene = GetOwner<CutsceneEditor>().GetCutsceneInstance();
            m_pTimelineTree = new TreeAssetView(new string[] { "Track", "" });
            var colomn0 = m_pTimelineTree.GetColumn(0);
            colomn0.width = colomn0.minWidth = 200;
            colomn0.maxWidth = 650;
            m_pTimelineTree.buildMutiColumnDepth = true;
            m_pTimelineTree.DepthIndentWidth = 18;
            m_pTimelineTree.scrollWhellIgnore = true;
            m_pTimelineTree.SetRowHeight(30);
            m_pTimelineTree.SetCellMargin(0);
            m_pTimelineTree.OnCellDraw += OnTreeCellLineDraw;
            m_pTimelineTree.OnSelectChange += OnItemSelectChange;
            m_pTimelineTree.OnItemRightClick += OnItemRightClick;
            m_pTimelineTree.OnViewRightClick += OnViewEmptyRightClick;
            m_pTimelineTree.Reload();

            m_TimeArea = new TimelineTimeArea(false)
            {
                hRangeLocked = false,
                vRangeLocked = true,
                margin = 10,
                scaleWithWindow = true,
                hSlider = true,
                vSlider = false,
                hBaseRangeMin = 0.0f,
                hBaseRangeMax = (float)9e6,
                hRangeMin = 0.0f,
                hScaleMax = 90000.0f,
            };
            m_TimeAreaDirty = true;
            InitTimeAreaFrameRate();
            SyncTimeAreaShownRange();

            m_pDragDrop = new DragDropDraw(this, m_pTimelineTree);
            m_pDragDraw = new DragDraw(this, m_pTimelineTree);
        }
        //--------------------------------------------------------
        protected override void OnDisable()
        {
            if (IsRuntimePlayingCutscene())
                return;

            if(m_pCutscene!=null) m_pCutscene.Destroy();
            m_pCutscene = null;
        }
        //--------------------------------------------------------
        internal List<IDraw> GetSelectClips()
        {
            return m_pDragDraw.GetSelectClips();
        }
        //--------------------------------------------------------
        public TimeArea.TimeFormat GetTimeFormat()
        {
            return m_TimeFromat;
        }
        //--------------------------------------------------------
        public float GetDuration()
        {
            if (GetAsset() == null) return 10.0f;
            return GetAsset().GetDuration();
        }
        //--------------------------------------------------------
        public float GetCurrentTime()
        {
            return m_fPlayTime;
        }
        //--------------------------------------------------------
        public void SetCurrentTime(float time)
        {
            m_fPlayTime = time;
        }
        //--------------------------------------------------------
        public void SetPlaying(bool play)
        {
            if(m_pCutscene !=null)
            {
                if (m_pCutscene.GetStatus() == EPlayableStatus.Start)
                    m_pCutscene.Pause();
            }
        }
        //--------------------------------------------------------
        public int GetFrameRate()
        {
            if (GetAsset() == null) return 30;
            return Mathf.Max(30, GetAsset().frameRate);
        }
        //--------------------------------------------------------
        public float GetInvFrame()
        {
            return 1.0f / (float)GetFrameRate();
        }
        //--------------------------------------------------------
        internal bool IsSelected(IDraw draw)
        {
            return m_pDragDraw.IsSelected(draw);
        }
        //--------------------------------------------------------
        public override void OnChangeSelect(object pAsset)
        {
            if (pAsset == null)
                return;
            m_pDragDraw.ClearSelected();
            if (pAsset is CutsceneData)
                RefreshTimeGraph((CutsceneData)pAsset);

            if(GetAsset()!=null)
            {
                if (GetAsset() != null)
                {
                    m_PendingShowRange = new Vector2(0, GetDuration() + 5.0f);
                    m_NeedSetShowRange = true;
                }
            }
            if(IsRuntimePlayingCutscene())
            {
                m_pCutscene = GetOwner<CutsceneEditor>().GetCutsceneInstance();
                if(m_pCutscene!=null) m_fPlayTime = m_pCutscene.GetTime();
            }
        }
        //--------------------------------------------------------
        public void RefreshTimeGraph(CutsceneData asset)
        {
            var selects = m_pTimelineTree.GetSelection();
            List<int> vPreSelects = new List<int>(selects);
            m_pTimelineTree.BeginTreeData();
            HashSet<int> vGroupId = new HashSet<int>();
            Dictionary<object, int> vIdLocks = new Dictionary<object, int>();
            HashSet<int> vExpandId = new HashSet<int>();

            var datas = m_pTimelineTree.GetDatas();
            foreach (var db in datas)
            {
                if (db is EmptyItemLine) continue;
                if (m_pTimelineTree.IsExpanded(db.id))
                {
                    vExpandId.Add(db.id);
                }
                var data = db as TreeItem;
                if (data is TimelineTrackGroup)
                {
                    vGroupId.Add(data.id);
                    vIdLocks[((TimelineTrackGroup)data).group] = data.id;
                }
                else if (data is TimelineTrack)
                {
                    vIdLocks[((TimelineTrack)data).track] = data.id;
                    vGroupId.Add(data.id);
                }
            }

            List<TreeAssetView.ItemData> vExpandDatas = new List<TreeAssetView.ItemData>();
            if (asset.groups != null)
            {
                for(int g =0; g < asset.groups.Count; ++g)
                {
                    var groupData = asset.groups[g];
                    TimelineTrackGroup group = null;
                    if (groupData.isGroup)
                    {
                        group = new TimelineTrackGroup();
                        if (vIdLocks.TryGetValue(groupData, out var groupId))
                            group.id = groupId;
                        else
                        {
                            int grId = 0;
                            while (vGroupId.Contains(grId))
                            {
                                grId++;
                            }
                            group.id = grId;
                            vGroupId.Add(grId);
                        }
                        group.name = groupData.name;
                        group.group = groupData;
                        group.depth = 0;
                        m_pTimelineTree.AddData(group);
                    }

                    if (groupData.tracks != null)
                    {
                        for (int i = 0; i < groupData.tracks.Count; ++i)
                        {
                            var trackData = groupData.tracks[i];
                            TimelineTrack track = new TimelineTrack();
                            if (vIdLocks.TryGetValue(trackData, out var trackId))
                                track.id = trackId;
                            else
                            {
                                int trId = 0;
                                while (vGroupId.Contains(trId))
                                {
                                    trId++;
                                }
                                track.id = trId;
                                vGroupId.Add(trId);
                            }
                            track.name = trackData.trackName;
                            track.track = trackData;
                            if (groupData.isGroup)
                                track.depth = 1;
                            else track.depth = 0;
                            track.parent = group;
                            track.ownerGroup = group;
                            track.groupData = groupData;

                            if (track.trackColor.Equals(Color.clear))
                                track.trackColor = EditorPreferences.GetTypeColor(trackData.GetType());

                            if (trackData.clips != null)
                            {
                                for (int j = 0; j < trackData.clips.Count; ++j)
                                {
                                    var clip = trackData.clips[j];
                                    track.clipDraws.Add(new ClipDraw(this, track, clip));
                                }
                            }
                            if (trackData.events != null)
                            {
                                for (int j = 0; j < trackData.events.Count; ++j)
                                {
                                    var clip = trackData.events[j];
                                    track.eventDraws.Add(new EventDraw(this, track, clip));
                                }
                            }
                            m_pTimelineTree.AddData(track);

                        }
                    }
                }
            }
            m_pTimelineTree.AddData(new EmptyItemLine() { id = int.MaxValue-1});

            m_pTimelineTree.EndTreeData();
            m_pTimelineTree.SetSelection(vPreSelects);
            foreach (var db in vExpandId)
            {
                m_pTimelineTree.SetExpanded(db, true);
            }
            RefreshRuntimePlayData();
        }
        //--------------------------------------------------------
        public override void OnRefreshData(System.Object pData)
        {
            CutsceneData cutSceneData = (CutsceneData)pData;
            if (cutSceneData == null)
                return;

            var datas = m_pTimelineTree.GetDatas();
            Dictionary<object, TreeItem> vDatas = new Dictionary<object, TreeItem>();
            foreach (var db in datas)
            {
                if (db is EmptyItemLine) continue;
                var data = db as TreeItem;
                if (data is TimelineTrackGroup)
                {
                    vDatas[((TimelineTrackGroup)data).group] = data;
                }
                else if (data is TimelineTrack)
                {
                    vDatas[((TimelineTrack)data).track] = data;
                }
            }

            if (cutSceneData.groups != null)
            {
                for (int g = 0; g < cutSceneData.groups.Count; ++g)
                {
                    var groupData = cutSceneData.groups[g];
                    if (groupData.tracks != null)
                    {
                        for (int i = 0; i < groupData.tracks.Count; ++i)
                        {
                            var trackData = groupData.tracks[i];
                            if(vDatas.TryGetValue(trackData, out var treeItem) && treeItem is TimelineTrack)
                            {
                                TimelineTrack track = (TimelineTrack)treeItem;
                                for (int j = 0; j < trackData.clips.Count; ++j)
                                {
                                    if (j < track.clipDraws.Count)
                                        track.clipDraws[j].clip = trackData.clips[j];
                                    else
                                        track.clipDraws.Add(new ClipDraw(this, track, trackData.clips[j]));
                                }
                                track.clipDraws.RemoveRange(trackData.clips.Count, track.clipDraws.Count - trackData.clips.Count);
                                for (int j = 0; j < trackData.events.Count; ++j)
                                {
                                    if (j < track.eventDraws.Count)
                                        track.eventDraws[j].clip = trackData.events[j];
                                    else
                                        track.eventDraws.Add(new EventDraw(this, track, trackData.events[j]));
                                }
                                track.eventDraws.RemoveRange(trackData.events.Count, track.eventDraws.Count - trackData.events.Count);
                            }
                        }
                    }
                }
            }
        }
        //--------------------------------------------------------
        public void UpdateViewStateHash()
        {
            viewStateHash = timeAreaTranslation.GetHashCode()
                .CombineHash(timeAreaScale.GetHashCode());
        }
        //--------------------------------------------------------
        protected override void OnGUI()
        {
            if (m_pTimelineTree == null)
                return;

            if (Event.current.type == EventType.Layout)
                UpdateViewStateHash();

            var viewRect = GetRect();
            GUILayout.BeginArea(GetRect());

            float trackWidth = 0;
            if (m_pTimelineTree != null)
            {
                trackWidth = m_pTimelineTree.GetColumn(0).width;
                m_LeftRect = new Rect(0, 0, trackWidth, viewRect.height);
                m_RightRect = new Rect(trackWidth, 0, viewRect.width - trackWidth, viewRect.height);
                m_TimelineRect = new Rect(0, 20, viewRect.width, viewRect.height - 20);
                if (m_pTimelineTree.ShowScrollVerticalBar)
                    m_pTimelineTree.discardRect = new Rect(m_TimeZoomRect.x, m_TimeZoomRect.yMax-18, m_TimeZoomRect.width,18);
                else
                    m_pTimelineTree.discardRect = Rect.zero;
                
                m_pTimelineTree.OnGUI(m_TimelineRect);

                float timelineWidth = viewRect.width - trackWidth;
                if (m_pTimelineTree.ShowScrollVerticalBar)
                    timelineWidth -= 15;
                m_pTimelineTree.GetColumn(1).width = timelineWidth;
                m_TimeZoomRect = new Rect(trackWidth, 16, timelineWidth, viewRect.height - 16);

                m_pDragDraw.OnGUI();
                m_TimeArea.hBaseRangeMax = GetDuration()+5;
                m_TimeArea.rect = m_TimeZoomRect;

                if (m_NeedSetShowRange && m_TimeArea.rect.width > 0)
                {
                    m_TimeArea.SetShownHRange(m_PendingShowRange.x, m_PendingShowRange.y);
                    m_TimeAreaDirty = false;
                    m_TimeAreaShownRange = m_PendingShowRange;
                    m_NeedSetShowRange = false;
                }

                if (m_LastFrameRate != GetFrameRate())
                    InitTimeAreaFrameRate();
                SyncTimeAreaShownRange();

                m_TimeArea.BeginViewGUI();
                m_TimeArea.TimeRuler(new Rect(trackWidth, 16, timelineWidth, TimeRulerAreaHeight), GetFrameRate(), true, false, 1.0f, m_TimeFromat);
                m_TimeArea.EndViewGUI();
            }

            EditorGUI.BeginDisabledGroup(IsRuntimePlayingCutscene());
            DrawTimeCursor();
            DrawToolBar();
            EditorGUI.EndDisabledGroup();

            GUILayout.EndArea();

            EditorGUI.BeginDisabledGroup(IsRuntimePlayingCutscene());
            GUILayout.BeginArea(new Rect(viewRect.xMin + 5+m_LeftRect.width, viewRect.y + 2, viewRect.width - 5, 18));
            DrawTimeAreaBar();
            GUILayout.EndArea();
            UIDrawUtils.DrawColorLine(new Vector2(viewRect.x + trackWidth, viewRect.yMin + 10), new Vector2(viewRect.x + trackWidth, viewRect.yMax), Color.grey);

            m_pDragDrop.OnGUI();

            EditorGUI.EndDisabledGroup();
        }
        //--------------------------------------------------------
        void InitTimeAreaFrameRate()
        {
            m_LastFrameRate = GetFrameRate();
            m_TimeArea.hTicks.SetTickModulosForFrameRate((float)m_LastFrameRate);
        }
        //--------------------------------------------------------
        public bool IsMouseInTimeAreaHeadAndScrollBar(Event evt)
        {
            Rect headRect = m_TimeZoomRect;
            headRect.y = 0;
            headRect.height = TimeRulerAreaHeight;
            if (headRect.Contains(evt.mousePosition - GetRect().position))
            {
                return true;
            }
            headRect = m_TimeZoomRect;
            headRect.y = m_TimeZoomRect.yMax-18;
            headRect.height = 18;
            if (headRect.Contains(evt.mousePosition - GetRect().position))
            {
                return true;
            }
            if(m_pTimelineTree.ShowScrollVerticalBar)
            {
                headRect = m_TimeZoomRect;
                headRect.x = m_TimeZoomRect.xMax;
                headRect.width = 18;
                if (headRect.Contains(evt.mousePosition - GetRect().position))
                {
                    return true;
                }
            }
          //  GUIUtility.GetControlID(m_pTimelineTree.treeViewControlID, FocusType.Keyboard, m_pTimelineTree.GetRect());
            return false;
        }
        //--------------------------------------------------------
        void SyncTimeAreaShownRange()
        {
            var range = m_TimeAreaShownRange;
            if (!Mathf.Approximately(range.x, m_TimeArea.shownArea.x) || !Mathf.Approximately(range.y, m_TimeArea.shownArea.xMax))
            {
                // set view data onto the time area
                if (m_TimeAreaDirty)
                {
                    m_TimeArea.SetShownHRange(range.x, range.y);
                    m_TimeAreaDirty = false;
                }
                else
                {
                    m_TimeAreaShownRange = new Vector2(m_TimeArea.shownArea.x, m_TimeArea.shownArea.xMax);
                }
            }

            m_TimeArea.hBaseRangeMax = GetDuration()+5;
        }
        //--------------------------------------------------------
        void DrawToolBar()
        {
            if (m_pTimelineTree == null)
                return;
            float trackWidth = m_pTimelineTree.GetColumn(0).width - 4;
            float barHeight = m_pTimelineTree.GetRowHeight() + 16;
            var rect = new Rect(0, 0, trackWidth, barHeight);
            GUILayout.BeginArea(rect);

            if (m_customToolbarButtonStyle == null)
            {
                m_customToolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
                {
                    fixedHeight = barHeight
                };
            }

            float buttonWidth = rect.width / 6;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (EditorUtil.DrawButton(EditorGUIUtility.TrIconContent("d_Animation.FirstKey").image, "", m_customToolbarButtonStyle, buttonWidth))
            {
                m_fPlayTime = 0.0f;
                if (m_pCutscene != null)
                    m_pCutscene.SetTime(m_fPlayTime);
            }
            if (EditorUtil.DrawButton(EditorGUIUtility.TrIconContent("d_Animation.PrevKey").image, "", m_customToolbarButtonStyle, buttonWidth))
            {
                m_fPlayTime -= GetInvFrame();
                if (m_fPlayTime < 0.0f) m_fPlayTime = 0.0f;
                if (m_pCutscene != null)
                    m_pCutscene.SetTime(m_fPlayTime);
            }

            EditorGUI.BeginChangeCheck();

            EPlayableStatus status = EPlayableStatus.Stop;
            if (m_pCutscene != null)
                status = m_pCutscene.GetStatus();

            bool isPlaying = m_pCutscene!=null? status == EPlayableStatus.Start:false;
            if (isPlaying) GUI.backgroundColor = Color.blue + Color.cyan;
            isPlaying = EditorUtil.DrawToggle(isPlaying, EditorGUIUtility.TrIconContent("d_Animation.Play").image, "", m_customToolbarButtonStyle, buttonWidth);
            GUI.backgroundColor = Color.white;
            if (EditorGUI.EndChangeCheck())
            {
                if (isPlaying)
                {
                    ForcePlay();
                }
                else
                {
                    if (m_pCutscene != null)
                    {
                        m_pCutscene.Stop();
                        m_pCutscene.Enable(false);
                        EnableCutscene(m_pCutscene, false);
                    }
                }
            }
            {
                EditorGUI.BeginChangeCheck();
                status = EPlayableStatus.Stop;
                if (m_pCutscene != null)
                    status = m_pCutscene.GetStatus();
                bool isPause = status == EPlayableStatus.Pause;
                if (isPause)
                    GUI.backgroundColor = Color.blue + Color.cyan;
                var isCurPause = EditorUtil.DrawToggle(isPause, EditorGUIUtility.TrIconContent("d_PauseButton").image, "", m_customToolbarButtonStyle, buttonWidth);
                GUI.backgroundColor = Color.white;

                if (EditorGUI.EndChangeCheck())
                {
                    if (isCurPause)
                    {
                        if (m_pCutscene != null)
                            m_pCutscene.Pause();
                    }
                    else
                    {
                        if (m_pCutscene != null)
                            m_pCutscene.Resume();
                    }
                }
            }


            if (EditorUtil.DrawButton(EditorGUIUtility.TrIconContent("d_Animation.NextKey").image, "", m_customToolbarButtonStyle, buttonWidth))
            {
                if (m_pCutscene != null)
                {
                    m_fPlayTime += GetInvFrame();
                    m_pCutscene.SetTime(m_fPlayTime);
                }
            }

            if (EditorUtil.DrawButton(EditorGUIUtility.TrIconContent("d_Animation.LastKey").image, "", m_customToolbarButtonStyle, buttonWidth))
            {
                if (m_pCutscene != null)
                {
                    m_fPlayTime = GetDuration();
                    m_pCutscene.SetTime(m_fPlayTime);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        //--------------------------------------------------------
        void ForcePlay(CutsceneData assetData = null)
        {
            if (assetData == null) assetData = GetAsset();
            m_pCutscene.Destroy();
            GetOwner<CutsceneEditor>().SaveAgentTreeData();
            m_pCutscene.SetEditorMode(true, GetOwner());
            m_pCutscene.Create(GetCutsceneGraph(), null, assetData!=null? assetData.id:-1);
            if (m_pCutscene != null)
            {
                if (m_fPlayTime >= GetDuration())
                    m_fPlayTime = 0;
                m_pCutscene.Stop();
                m_pCutscene.Enable(true);
                m_pCutscene.Play();
                EnableCutscene(m_pCutscene, true);
            }
        }
        //--------------------------------------------------------
        void RefreshRuntimePlayData(CutsceneData assetData = null)
        {
            if (IsRuntimePlayingCutscene())
            {
                m_pCutscene = GetOwner<CutsceneEditor>().GetCutsceneInstance();
                if(m_pCutscene!=null)
                    m_fPlayTime = m_pCutscene.GetTime();
                return;
            }

            bool isPlay = m_pCutscene != null && m_pCutscene.GetStatus() == EPlayableStatus.Start;
            if (assetData == null) assetData = GetAsset();
            if (m_pCutscene == null)
                return;
            m_pCutscene.Destroy();
            GetOwner<CutsceneEditor>().SaveAgentTreeData();
            m_pCutscene.SetEditorMode(true, GetOwner());
            m_pCutscene.Create(GetCutsceneGraph(), null, assetData != null ? assetData.id : -1);
            if(isPlay)
            {
                if (m_fPlayTime >= GetDuration())
                    m_fPlayTime = 0;
                m_pCutscene.Stop();
                m_pCutscene.Enable(true);
                m_pCutscene.Play();
                EnableCutscene(m_pCutscene, true);
            }
            else
            {
                m_pCutscene.Enable(true);
            }
        }
        //--------------------------------------------------------
        protected void DrawTimeAreaBar()
        {
            EditorGUI.BeginChangeCheck();
            var currentTime = m_TimeFromat.ToTimeString(m_fPlayTime, GetFrameRate(), "0.####");
            var newCurrentTime = GUILayout.TextField(currentTime, EditorStyles.toolbarTextField, new GUILayoutOption[] { GUILayout.Width(105) });
            if (EditorGUI.EndChangeCheck())
            {
                m_fPlayTime = (float)this.FromTimeString(newCurrentTime);
                if (m_pCutscene != null &&!IsRuntimePlayingCutscene())
                    m_pCutscene.SetTime(m_fPlayTime);
            }

            if (GetAsset() != null)
            {
                var nameRect = new Rect(m_RightRect.width - m_LeftRect.width, -4, 200, 25);
                GUI.Label(nameRect, "当前编辑:" + GetAsset().name);
            }
            var settingRect = new Rect(m_RightRect.xMax - 30- m_LeftRect.width, 0, 25, 25);
            if (GUI.Button(settingRect, EditorGUIUtility.TrIconContent("Settings", ""), EditorStyles.toolbarButton))
            {
                PreferencesWindow.Show(settingRect);
            }
        }
        //--------------------------------------------------------
        protected void DrawTimeCursor()
        {
            if (TimeIsInRange(GetCurrentTime()))
            {
                var colorDimFactor = EditorGUIUtility.isProSkin ? 0.7f : 0.9f;
                var c = TextUtil.timeCursor.normal.textColor * colorDimFactor;

                float time = Mathf.Max((float)GetCurrentTime(), 0);
                float duration = (float)GetDuration();

                m_TimeArea.DrawTimeOnSlider(time, c, duration, ConstUtil.kDurationGuiThickness);
            }

            m_Cursor.Draw(m_RightRect, GetCurrentTime());
            m_EndCursor.Draw(m_RightRect, GetDuration());
        }
        //--------------------------------------------------------
        protected override void OnEvent(Event evt)
        {
            base.OnEvent(evt);
            m_pDragDrop.canDrag = !IsRuntimePlayingCutscene();
            m_pDragDrop.OnEvent(evt);
            if (evt.type == EventType.MouseUp)
                m_bRightItemLine = false;

            if (evt.type == EventType.MouseUp)
            {
                m_bRightItemLine = false;
                if (evt.button == 2)
                {
                    if (evt.alt && m_MiddleMouseDragging)
                    {
                        var selections = m_pTimelineTree.GetSelection();
                        if (m_vGroupOffsetTimes.Count > 0)
                        {
                            float offset = GetSnappedTimeAtMousePosition(evt.mousePosition) - m_fGroupDragOffsetTime;
                            foreach (var db in m_vGroupOffsetTimes)
                            {
                                db.DragOffset(offset, true, true, false);
                            }
                        }
                    }
                    m_MiddleMouseDragging = false;
                    evt.Use();
                }
            }

            if (!m_RightRect.Contains(evt.mousePosition - GetRect().position))
            {
                return;
            }

            if (evt.type == EventType.ScrollWheel && evt.control)
            {
                // 轨道树的垂直滚动
                var state = m_pTimelineTree.state;
                float scrollStep = 40f; // 每次滚动的像素，可根据实际调整
                state.scrollPos.y += evt.delta.y * scrollStep;

                // 限制滚动范围
                state.scrollPos.y = Mathf.Clamp(state.scrollPos.y, 0, Mathf.Max(0, m_pTimelineTree.GetContentHeight()));

                evt.Use();
                return;
            }

            // 中键按下开始拖动
            if (evt.type == EventType.MouseDown && evt.button == 2)
            {
                m_MiddleMouseDragging = true;
                m_MiddleMouseDragLastPos = evt.mousePosition;

                m_fGroupDragOffsetTime = GetSnappedTimeAtMousePosition(evt.mousePosition);
                m_vGroupOffsetTimes.Clear();
                if (evt.alt)
                {
                    var selections = m_pTimelineTree.GetSelection();
                    if (selections != null)
                    {
                        var datas = m_pTimelineTree.GetDatas();
                        foreach (var db in datas)
                        {
                            if (db is EmptyItemLine) continue;
                            var data1 = db as TreeItem;
                            if(!selections.Contains(db.id))
                            {
                                continue;
                            }
                            if (data1 is TimelineTrack)
                            {
                                var tracks = ((TimelineTrack)data1);
                                for(int i =0; i < tracks.clipDraws.Count; ++i)
                                {
                                    var clip = tracks.clipDraws[i];
                                    if (clip.CanEdit() && !m_vGroupOffsetTimes.Contains(clip))
                                        m_vGroupOffsetTimes.Add(clip);
                                }
                                for (int i = 0; i < tracks.eventDraws.Count; ++i)
                                {
                                    var clip = tracks.eventDraws[i];
                                    if (clip.CanEdit() && !m_vGroupOffsetTimes.Contains(clip))
                                        m_vGroupOffsetTimes.Add(clip);
                                }
                            }
                            else if(data1 is TimelineTrackGroup)
                            {
                                var group = ((TimelineTrackGroup)data1);
                                foreach(var track in datas)
                                {
                                    if (track is EmptyItemLine) continue;
                                    var data2 = track as TreeItem;
                                    if (data2 is TimelineTrack)
                                    {
                                        if (((TimelineTrack)data2).ownerGroup == group)
                                        {
                                            for (int i = 0; i < ((TimelineTrack)data2).clipDraws.Count; ++i)
                                            {
                                                var clip = ((TimelineTrack)data2).clipDraws[i];
                                                if (clip.CanEdit() && !m_vGroupOffsetTimes.Contains(clip))
                                                    m_vGroupOffsetTimes.Add(clip);
                                            }
                                            for (int i = 0; i < ((TimelineTrack)data2).eventDraws.Count; ++i)
                                            {
                                                var clip = ((TimelineTrack)data2).eventDraws[i];
                                                if (clip.CanEdit() && !m_vGroupOffsetTimes.Contains(clip))
                                                    m_vGroupOffsetTimes.Add(clip);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    evt.Use();
                    return;
                }

                evt.Use();
                return;
            }

            if (evt.type == EventType.MouseDrag && evt.button == 2)
            {
                Vector2 delta = evt.mousePosition - m_MiddleMouseDragLastPos;
                m_MiddleMouseDragLastPos = evt.mousePosition;

                if(evt.alt)
                {
                    var selections = m_pTimelineTree.GetSelection();
                    if(m_vGroupOffsetTimes.Count>0)
                    {
                        float offset = GetSnappedTimeAtMousePosition(evt.mousePosition) - m_fGroupDragOffsetTime;
                        foreach (var db in m_vGroupOffsetTimes)
                        {
                            db.DragOffset(offset, false, false, false);
                        }
                    }
                    evt.Use();
                    return;
                }

                m_TimeArea.SetTransform(m_TimeArea.translation + new Vector2(delta.x, 0), m_TimeArea.scale);

                m_pTimelineTree.state.scrollPos.y -= delta.y;
                m_pTimelineTree.state.scrollPos.y = Mathf.Clamp(m_pTimelineTree.state.scrollPos.y, 0, Mathf.Max(0, m_pTimelineTree.GetContentHeight()));
                evt.Use();
                return;
            }

            var processManipulators = Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout;
            if (processManipulators)
            {
                m_Cursor.canDrag = !IsRuntimePlayingCutscene();
                m_Cursor.OnEvent(evt);
                if (m_Cursor.isDraged)
                {
                    m_pDragDraw.DragEnd();
                }
                m_pDragDraw.canDrag = !IsRuntimePlayingCutscene();
                m_pDragDraw.OnEvent(evt);
            }
        }
        //--------------------------------------------------------
        protected override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);
            if(m_pCutscene!=null)
            {
                if(IsRuntimePlayingCutscene())
                {
                    if(m_pCutscene.IsDestroyed())
                    {
                        return;
                    }
                    m_fPlayTime = m_pCutscene.GetTime();
                    return;
                }
                EPlayableStatus status = m_pCutscene.GetStatus();

                if (status == EPlayableStatus.Start)
                {
                    if(Application.isPlaying)
                    m_fAccumoulator += delta;
                    else
                        m_fAccumoulator += delta * Time.timeScale;
                    while (m_fAccumoulator >= GetInvFrame())
                    {
                        m_fAccumoulator -= GetInvFrame();
                        m_fPlayTime += GetInvFrame();
                        m_pCutscene.Evaluate(m_fPlayTime);
                    }
                    //    m_fPlayTime += GetInvFrame() * Time.timeScale;
                }
                else
                    m_pCutscene.Evaluate(m_fPlayTime);

                if (status == EPlayableStatus.Start && m_fPlayTime >= GetDuration())
                {
              //      m_fPlayTime = 0.0f;
                    m_pCutscene.Stop();
                }
            }
        }
        //--------------------------------------------------------
        void CreateGroupMenu(System.Object input = null, System.Action<GenericMenu, MenuData> onMenu = null)
        {
            var genericMenu = new GenericMenu();
            MenuData menuData = new MenuData() { mousePos = Event.current.mousePosition, userData = input };
            menuData.mousePos -= new Vector2(m_pTimelineTree.GetColumn(0).width, 0);

            bool isActive = true;
            if (input != null)
            {
                menuData.data = null;
                if (menuData.userData is ClipDraw)
                {
                    isActive = ((ClipDraw)menuData.userData).CanEdit();
                }
                else if (menuData.userData is EventDraw)
                {
                    isActive = ((EventDraw)menuData.userData).CanEdit();
                }
                else if (menuData.userData is TimelineTrack)
                {
                    isActive = !((TimelineTrack)menuData.userData).groupData.HasFlag(EGroupFlag.UnActive);
                }
                else if (menuData.userData is TimelineTrackGroup)
                {
                    isActive = !((TimelineTrackGroup)menuData.userData).group.HasFlag(EGroupFlag.UnActive);
                }
            }

            ClipAttriData customClipAttri = null;
            var clips = DataUtils.GetClipAttrs();
            if(isActive)
            {
                foreach (var db in clips)
                {
                    if (db.type == typeof(CutsceneCustomClip))
                    {
                        customClipAttri = db;
                        continue;
                    }
                    menuData.data = db;
                    genericMenu.AddItem(new GUIContent("剪辑/" + db.pAttri.name), false, (menu) => {
                        CreateClip((MenuData)menu);
                    }, menuData);
                }
                //! custom clip
                if (customClipAttri != null)
                {
                    var customClips = CustomAgentUtil.GetClipList();
                    foreach (var db in customClips)
                    {
                        menuData.data = customClipAttri;
                        menuData.agentUnit = db;
                        genericMenu.AddItem(new GUIContent("剪辑/" + db.name), false, (menu) =>
                        {
                            CreateClip((MenuData)menu);
                        }, menuData);
                    }
                }

                EventAttriData customEventAttri = null;
                var events = DataUtils.GetEventAttrs();
                foreach (var db in events)
                {
                    if (db.type == typeof(CutsceneCustomEvent))
                    {
                        customEventAttri = db;
                        continue;
                    }
                    menuData.data = db;
                    genericMenu.AddItem(new GUIContent("事件/" + db.pAttri.name), false, (menu) =>
                    {
                        CreateEvent((MenuData)menu);
                    }, menuData);
                }
                //! custom event
                if (customEventAttri != null)
                {
                    var customEvents = CustomAgentUtil.GetEventList();
                    foreach (var db in customEvents)
                    {
                        menuData.data = customEventAttri;
                        menuData.agentUnit = db;
                        genericMenu.AddItem(new GUIContent("事件/" + db.name), false, (menu) =>
                        {
                            CreateEvent((MenuData)menu);
                        }, menuData);
                    }
                }
            }

            {
                genericMenu.AddItem(new GUIContent("新建组"), false, (menu) =>
                {
                    CreateGroup((MenuData)menu);
                }, menuData);
            }

            if (input!=null)
            {
                menuData.data = null;
                if(menuData.userData is ClipDraw)
                {
                    genericMenu.AddItem(new GUIContent("删除Clip"), false, (menu) =>
                    {
                        DeleteData((MenuData)menu);
                    }, menuData);
                    genericMenu.AddItem(new GUIContent("复制"), false, (menu) =>
                    {
                        CopyData((MenuData)menu);
                    }, menuData);
                    if (isActive && m_pStandByCopy.IsValid() && m_pStandByCopy.pData != menuData.userData)
                    {
                        genericMenu.AddItem(new GUIContent("粘贴"), false, (menu) =>
                        {
                            ParseData((MenuData)menu, false);
                        }, menuData);
                    }
                }
                else if (menuData.userData is EventDraw)
                {
                    if (isActive)
                    {
                        genericMenu.AddItem(new GUIContent("删除Event"), false, (menu) =>
                        {
                            DeleteData((MenuData)menu);
                        }, menuData);
                    }

                    genericMenu.AddItem(new GUIContent("复制"), false, (menu) =>
                    {
                        CopyData((MenuData)menu);
                    }, menuData);
                    if (isActive && m_pStandByCopy.IsValid() && m_pStandByCopy.pData != menuData.userData)
                    {
                        genericMenu.AddItem(new GUIContent("粘贴"), false, (menu) =>
                        {
                            ParseData((MenuData)menu);
                        }, menuData);
                    }
                }
                else if(menuData.userData is TimelineTrack)
                {
                    if (isActive)
                    {
                        genericMenu.AddItem(new GUIContent("删除轨道"), false, (menu) =>
                        {
                            DeleteData((MenuData)menu);
                        }, menuData);
                    }

                    genericMenu.AddItem(new GUIContent("复制"), false, (menu) =>
                    {
                        CopyData((MenuData)menu);
                    }, menuData);
                    if (isActive && m_pStandByCopy.IsValid() && m_pStandByCopy.pData != menuData.userData)
                    {
                        genericMenu.AddItem(new GUIContent("粘贴"), false, (menu) =>
                        {
                            ParseData((MenuData)menu);
                        }, menuData);
                    }
                }
                else if (menuData.userData is TimelineTrackGroup)
                {
                    genericMenu.AddSeparator("");
                    foreach (Enum v in Enum.GetValues(typeof(EGroupFlag)))
                    {
                        string name = Framework.ED.EditorUtils.GetEnumDisplayName(v);
                        if(((TimelineTrackGroup)menuData.userData).group.HasFlag((EGroupFlag)v))
                        {
                            name += " √";
                        }
                        menuData.data = v;
                        genericMenu.AddItem(new GUIContent(name), false, (menu) =>
                        {
                            UpdateFlag((MenuData)menu);
                        }, menuData);
                    }
                    genericMenu.AddSeparator("");
                    genericMenu.AddItem(new GUIContent("删除组"), false, (menu) =>
                    {
                        DeleteData((MenuData)menu);
                    }, menuData);
                    genericMenu.AddItem(new GUIContent("复制"), false, (menu) =>
                    {
                        CopyData((MenuData)menu);
                    }, menuData);
                    if (isActive && m_pStandByCopy.IsValid() && m_pStandByCopy.pData != menuData.userData)
                    {
                        genericMenu.AddItem(new GUIContent("粘贴"), false, (menu) =>
                        {
                            ParseData((MenuData)menu);
                        }, menuData);
                    }
                }
            }
            if (isActive && m_pStandByCopy.IsValid() && m_pStandByCopy.pData is TimelineTrackGroup)
            {
                genericMenu.AddItem(new GUIContent("创建组并粘贴数据"), false, (menu) =>
                {
                    ParseData((MenuData)menu, true);
                }, menuData);
            }

            if (onMenu != null) onMenu(genericMenu, menuData);

            genericMenu.ShowAsContext();
        }
        //--------------------------------------------------------
        public void DeleteData(List<object> draws)
        {
            RegisterUndoData(false);
            bool bRefreshAssetDraw = false;
            for (int i =0; i < draws.Count; ++i)
            {
                DeleteData(draws[i], false, false);
                if (draws[i] is TimelineTrack || draws[i] is TimelineTrackGroup)
                    bRefreshAssetDraw = true;
            }
            if(m_pCutscene!=null)
            {

            }
            if(bRefreshAssetDraw)
            {
                RefreshTimeGraph(GetAsset());
            }
        }
        //--------------------------------------------------------
        void UpdateFlag(MenuData menuData)
        {
            if (menuData.userData == null)
                return;
            if(menuData.userData is TimelineTrackGroup)
            {
                EGroupFlag flag = (EGroupFlag)menuData.data;
                TimelineTrackGroup group = menuData.userData as TimelineTrackGroup;
                this.RegisterUndoData();
                group.group.EnableFlag(flag,!group.group.HasFlag(flag));
            }
        }
        //--------------------------------------------------------
        void DeleteData(MenuData menuData)
        {
            if (menuData.userData == null)
                return;
            DeleteData(menuData.userData, true,true);
        }
        //--------------------------------------------------------
        void DeleteData(object userData, bool bUndo = true, bool bRefreshAssetDraw = true)
        {
            if (userData == null)
                return;
            if (userData is ClipDraw)
            {
                ClipDraw clipDraw = (ClipDraw)userData;
                int index = clipDraw.ownerTrack.clipDraws.IndexOf(clipDraw);
                if (index >= 0 && index < clipDraw.ownerTrack.clipDraws.Count)
                {
                    if(bUndo) RegisterUndoData(false);
                    clipDraw.OnDelete();
                    clipDraw.ownerTrack.RemoveClip(index);
               //     RefreshRuntimePlayData();
                    if (m_pCutscene != null)
                        m_pCutscene.GetPlayable()?.Remove(clipDraw.ownerTrack.groupData, clipDraw.ownerTrack.track, clipDraw.clip);
                }
            }
            else if (userData is EventDraw)
            {
                EventDraw clipDraw = (EventDraw)userData;
                int index = clipDraw.ownerTrack.eventDraws.IndexOf(clipDraw);
                if (index >= 0 && index < clipDraw.ownerTrack.eventDraws.Count)
                {
                    if (bUndo) RegisterUndoData(false);
                    clipDraw.OnDelete();
                    clipDraw.ownerTrack.RemoveEvent(index);
                    //   RefreshRuntimePlayData();
                    if (m_pCutscene != null)
                        m_pCutscene.GetPlayable()?.Remove(clipDraw.ownerTrack.groupData, clipDraw.ownerTrack.track, clipDraw.clip);
                }
            }
            else if (userData is TimelineTrack)
            {
                TimelineTrack track = (TimelineTrack)userData;
                if (bUndo) RegisterUndoData(false);
                if (track.ownerGroup != null) track.ownerGroup.group.tracks.Remove(track.track);
                else
                {
                    GetAsset().groups.Remove(track.groupData);
                }
                if(bRefreshAssetDraw) RefreshTimeGraph(GetAsset());
            }
            else if (userData is TimelineTrackGroup)
            {
                TimelineTrackGroup track = (TimelineTrackGroup)userData;
                if (bUndo) RegisterUndoData(false);
                GetAsset().groups.Remove(track.group);
                if (bRefreshAssetDraw) RefreshTimeGraph(GetAsset());
            }
        }
        //--------------------------------------------------------
        void CopyData(MenuData menuData)
        {
            m_pStandByCopy.Clear();
            if (menuData.userData == null)
                return;
            m_pStandByCopy.pData = menuData.userData;
            if (menuData.userData is ClipDraw)
            {
                m_pStandByCopy.json =((ClipDraw)m_pStandByCopy.pData).clip.Serialize();
            }
            else if (menuData.userData is EventDraw)
            {
                m_pStandByCopy.json = ((EventDraw)m_pStandByCopy.pData).clip.Serialize();
            }
            else if (menuData.userData is TimelineTrack)
            {
                TimelineTrack track = (TimelineTrack)menuData.userData;
                track.track.OnSerialize();
                m_pStandByCopy.json = JsonUtility.ToJson(track.track);
            }
            else if (menuData.userData is TimelineTrackGroup)
            {
                TimelineTrackGroup group = (TimelineTrackGroup)menuData.userData;
                group.group.OnSerialize();
                m_pStandByCopy.json = JsonUtility.ToJson(group.group);
            }
        }
        //--------------------------------------------------------
        void ParseData(MenuData menuData, bool bCreateAndParse =false)
        {
            if (!m_pStandByCopy.IsValid())
                return;
            if (menuData.userData == null)
            {
                var curData = GetAsset();
                if (curData == null)
                {
                    GetOwner().ShowNotification(new GUIContent("请先创建一个CutsceneAsset!"), 2.0f);
                    return;
                }
                if(m_pStandByCopy.pData is ClipDraw || m_pStandByCopy.pData is EventDraw)
                {
                    if(m_ItemSelected != null)
                    {
                        TimelineTrack parseToTrack = null;
                        if (m_ItemSelected is TimelineTrackGroup)
                        {
                            GetOwner().ShowNotification(new GUIContent("事件、剪辑无法黏贴到选中的组中"), 2.0f);
                            return;
                        }
                        else
                        {
                            parseToTrack = ((TimelineTrack)m_ItemSelected);
                        }
                        if(m_pStandByCopy.pData is ClipDraw)
                        {
                            RegisterUndoData(false);
                            ClipDraw clipDraw = (ClipDraw)m_pStandByCopy.pData;
                           var clipAttr = DataUtils.GetClipAttri(clipDraw.clip.GetIdType());
                            var clipData = clipAttr.CreateClip();
                            clipData.Deserialize(m_pStandByCopy.json);
                            parseToTrack.AddClip(this, clipData);
                        }
                        else
                        {
                            RegisterUndoData(false);
                            EventDraw eventDraw = (EventDraw)m_pStandByCopy.pData;
                            var eventAttr = DataUtils.GetEventAttri(eventDraw.clip.GetIdType());
                            var eventData = eventAttr.CreateEvent();
                            eventData.Deserialize(m_pStandByCopy.json);
                            parseToTrack.AddEvent(this, eventData);
                        }
                    }
                    return;
                }
                if (m_pStandByCopy.pData is TimelineTrack)
                {
                    return;
                }
                RegisterUndoData(false);
                if (curData.groups == null)
                    curData.groups = new List<CutsceneData.Group>();

                CutsceneData.Group group = new CutsceneData.Group();
                group.isGroup = true;
                group.id = curData.GeneratorGroupId();
                group.OnDeserialize(m_pStandByCopy.json);
                group.name += "(Clone)";
                curData.groups.Add(group);
                RefreshTimeGraph(curData);
                return;
            }
            if (menuData.userData is ClipDraw && m_pStandByCopy.pData is ClipDraw)
            {
                RegisterUndoData(false);
                ClipDraw clipDraw = (ClipDraw)menuData.userData;
                clipDraw.clip.Deserialize(m_pStandByCopy.json);
            }
            else if (menuData.userData is EventDraw)
            {
                RegisterUndoData(false);
                EventDraw clipDraw = (EventDraw)menuData.userData;
                float time = clipDraw.clip.GetTime();
                clipDraw.clip.Deserialize(m_pStandByCopy.json);
                clipDraw.clip.SetTime(time);
            }
            else if (menuData.userData is TimelineTrack)
            {
                TimelineTrack track = (TimelineTrack)menuData.userData;
                RegisterUndoData(false);
                if (m_pStandByCopy.pData is ClipDraw || m_pStandByCopy.pData is EventDraw)
                {
                    if (m_pStandByCopy.pData is ClipDraw)
                    {
                        ClipDraw clipDraw = (ClipDraw)m_pStandByCopy.pData;
                        var clipAttr = DataUtils.GetClipAttri(clipDraw.clip.GetIdType());
                        var clipData = clipAttr.CreateClip();
                        clipData.Deserialize(m_pStandByCopy.json);
                        clipData.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                        track.AddClip(this, clipData);
                    }
                    else
                    {
                        EventDraw eventDraw = (EventDraw)m_pStandByCopy.pData;
                        var eventAttr = DataUtils.GetEventAttri(eventDraw.clip.GetIdType());
                        var eventData = eventAttr.CreateEvent();
                        eventData.Deserialize(m_pStandByCopy.json);
                        eventData.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                        track.AddEvent(this, eventData);
                    }
                }
                else if(m_pStandByCopy.pData is TimelineTrack)
                {
                    track.track.OnDeserialize(m_pStandByCopy.json);
                    track.clipDraws.Clear();
                    track.eventDraws.Clear();
                    if (track.track.clips != null)
                    {
                        for (int j = 0; j < track.track.clips.Count; ++j)
                        {
                            var clip = track.track.clips[j];
                            track.clipDraws.Add(new ClipDraw(this, track, clip));
                        }
                    }
                    if (track.track.events != null)
                    {
                        for (int j = 0; j < track.track.events.Count; ++j)
                        {
                            var clip = track.track.events[j];
                            track.eventDraws.Add(new EventDraw(this, track, clip));
                        }
                    }
                }
            }
            else if (menuData.userData is TimelineTrackGroup && m_pStandByCopy.pData is TimelineTrackGroup)
            {
                var curData = GetAsset();
                if (curData == null)
                {
                    GetOwner().ShowNotification(new GUIContent("请先创建一个CutsceneAsset!"), 2.0f);
                    return;
                }
                TimelineTrackGroup group = (TimelineTrackGroup)menuData.userData;
                RegisterUndoData(false);

                string lastName = group.group.name;
                group.group.OnDeserialize(m_pStandByCopy.json);
                group.group.name = lastName;
                group.group.id = curData.GeneratorGroupId();

                RefreshTimeGraph(GetAsset());
            }
        }
        //--------------------------------------------------------
        void CreateGroup(MenuData menuData)
        {
            var curData = GetAsset();
            if (curData == null)
            {
                GetOwner().ShowNotification(new GUIContent("请先创建一个CutsceneAsset!"), 2.0f);
                return;
            }
            RegisterUndoData(false);
            if (curData.groups == null)
                curData.groups = new List<CutsceneData.Group>();

            CutsceneData.Group group = new CutsceneData.Group();
            group.isGroup = true;
            group.id = curData.GeneratorGroupId();
            group.name = "Group" + (curData.groups.Count + 1).ToString("00"); ;
            group.tracks = new List<CutsceneData.Track>();
            curData.groups.Add(group);
            RefreshTimeGraph(curData);
        }
        //--------------------------------------------------------
        void CreateClip(MenuData menuData)
        {
            ClipAttriData clipAttr = (ClipAttriData)menuData.data;
            if (menuData.userData==null)
            {
                var curData = GetAsset();
                if (curData == null)
                {
                    GetOwner().ShowNotification(new GUIContent("请先创建一个CutsceneAsset!"), 2.0f);
                    return;
                }
                RegisterUndoData(false);
                if (curData.groups == null)
                    curData.groups = new List<CutsceneData.Group>();

                CutsceneData.Group group = new CutsceneData.Group();
                group.isGroup = false;
                group.id = curData.GeneratorGroupId();
                group.name = "Group" + (curData.groups.Count + 1).ToString("00"); ;
                group.tracks = new List<CutsceneData.Track>();
                curData.groups.Add(group);

                CutsceneData.Track track = new CutsceneData.Track();
                track.trackName = "Track" + (group.tracks.Count + 1).ToString("00");

                var clip = clipAttr.CreateClip();
                if (menuData.agentUnit != null && clip is CutsceneCustomClip)
                {
                    CutsceneCustomClip custom = (CutsceneCustomClip)clip;
                    custom.InitCustomAgent(menuData.agentUnit);
                    clip = custom;
                }
                clip.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                DataUtils.SetPresetDefaultValue(clip);
                track.clips.Add(clip);
                group.tracks.Add(track);
                RefreshTimeGraph(curData);
            }
            else if(menuData.userData is TimelineTrackGroup)
            {
                RegisterUndoData(false);
                var group = (TimelineTrackGroup)menuData.userData;
                if (group.group.tracks == null)
                    group.group.tracks = new List<CutsceneData.Track>();

                CutsceneData.Track track = new CutsceneData.Track();
                track.trackName = "Track" + (group.group.tracks.Count + 1).ToString("00");
                var clip = clipAttr.CreateClip();
                if (menuData.agentUnit != null && clip is CutsceneCustomClip)
                {
                    CutsceneCustomClip custom = (CutsceneCustomClip)clip;
                    custom.InitCustomAgent(menuData.agentUnit);
                    clip = custom;
                }
                clip.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                DataUtils.SetPresetDefaultValue(clip);
                track.clips.Add(clip);
                group.group.tracks.Add(track);
                RefreshTimeGraph(GetAsset());
            }
            else if (menuData.userData is TimelineTrack)
            {
                RegisterUndoData(false);
                var track = (TimelineTrack)menuData.userData;
                var clip = clipAttr.CreateClip();
                if (menuData.agentUnit != null && clip is CutsceneCustomClip)
                {
                    CutsceneCustomClip custom = (CutsceneCustomClip)clip;
                    custom.InitCustomAgent(menuData.agentUnit);
                    clip = custom;
                }
                clip.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                DataUtils.SetPresetDefaultValue(clip);
                track.AddClip(this, clip);
                //    RefreshRuntimePlayData();
                if (m_pCutscene != null)
                    m_pCutscene.GetPlayable()?.AddDataer(track.groupData, track.track, clip);
            }
            else if (menuData.userData is ClipDraw)
            {
                ClipDraw clipDraw = (ClipDraw)menuData.userData;
                RegisterUndoData(false);
                var clip = clipAttr.CreateClip();
                if (menuData.agentUnit != null && clip is CutsceneCustomClip)
                {
                    CutsceneCustomClip custom = (CutsceneCustomClip)clip;
                    custom.InitCustomAgent(menuData.agentUnit);
                    clip = custom;
                }
                clip.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                DataUtils.SetPresetDefaultValue(clip);
                clipDraw.ownerTrack.AddClip(this, clip);
                //   RefreshRuntimePlayData();
                if (m_pCutscene != null)
                    m_pCutscene.GetPlayable()?.AddDataer(clipDraw.ownerTrack.groupData, clipDraw.ownerTrack.track, clip);

            }
            else if (menuData.userData is EventDraw)
            {
                EventDraw clipDraw = (EventDraw)menuData.userData;
                RegisterUndoData(false);
                var clip = clipAttr.CreateClip();
                if (menuData.agentUnit != null && clip is CutsceneCustomClip)
                {
                    CutsceneCustomClip custom = (CutsceneCustomClip)clip;
                    custom.InitCustomAgent(menuData.agentUnit);
                    clip = custom;
                }
                clip.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                DataUtils.SetPresetDefaultValue(clip);
                clipDraw.ownerTrack.AddClip(this, clip);
                //    RefreshRuntimePlayData();
                if (m_pCutscene != null)
                    m_pCutscene.GetPlayable()?.AddDataer(clipDraw.ownerTrack.groupData, clipDraw.ownerTrack.track, clip);
            }
        }
        //--------------------------------------------------------
        void CreateEvent(MenuData menuData)
        {
            EventAttriData clipAttr = (EventAttriData)menuData.data;
            if (menuData.userData is TimelineTrack)
            {
                RegisterUndoData(false);
                var track = (TimelineTrack)menuData.userData;
                var clip = clipAttr.CreateEvent();
                if(menuData.agentUnit!=null && clip is CutsceneCustomEvent)
                {
                    CutsceneCustomEvent custom =  (CutsceneCustomEvent)clip;
                    custom.InitCustomAgent(menuData.agentUnit);
                    clip = custom;
                }
                clip.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                DataUtils.SetPresetDefaultValue(clip);
                track.AddEvent(this, clip);
                //   RefreshRuntimePlayData();
                if (m_pCutscene != null)
                    m_pCutscene.GetPlayable()?.AddDataer(track.groupData, track.track, clip);
            }
            else if (menuData.userData is ClipDraw)
            {
                ClipDraw clipDraw = (ClipDraw)menuData.userData;
                RegisterUndoData(false);
                var clip = clipAttr.CreateEvent();
                if (menuData.agentUnit != null && clip is CutsceneCustomEvent)
                {
                    CutsceneCustomEvent custom = (CutsceneCustomEvent)clip;
                    custom.InitCustomAgent(menuData.agentUnit);
                    clip = custom;
                }
                clip.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                DataUtils.SetPresetDefaultValue(clip);
                clipDraw.ownerTrack.AddEvent(this, clip);
                //    RefreshRuntimePlayData();
                if (m_pCutscene != null)
                    m_pCutscene.GetPlayable()?.AddDataer(clipDraw.ownerTrack.groupData, clipDraw.ownerTrack.track, clip);
            }
            else if (menuData.userData is EventDraw)
            {
                EventDraw clipDraw = (EventDraw)menuData.userData;
                RegisterUndoData(false);
                var clip = clipAttr.CreateEvent();
                if (menuData.agentUnit != null && clip is CutsceneCustomEvent)
                {
                    CutsceneCustomEvent custom = (CutsceneCustomEvent)clip;
                    custom.InitCustomAgent(menuData.agentUnit);
                    clip = custom;
                }
                clip.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                DataUtils.SetPresetDefaultValue(clip);
                clipDraw.ownerTrack.AddEvent(this, clip);
                //     RefreshRuntimePlayData();
                if (m_pCutscene != null)
                    m_pCutscene.GetPlayable()?.AddDataer(clipDraw.ownerTrack.groupData, clipDraw.ownerTrack.track, clip);
            }
            else if (menuData.userData is TimelineTrackGroup)
            {
                RegisterUndoData(false);
                var group = (TimelineTrackGroup)menuData.userData;
                if (group.group.tracks == null)
                    group.group.tracks = new List<CutsceneData.Track>();

                CutsceneData.Track track = new CutsceneData.Track();
                track.trackName = "Track" + (group.group.tracks.Count + 1).ToString("00");
                var clip = clipAttr.CreateEvent();
                if (menuData.agentUnit != null && clip is CutsceneCustomEvent)
                {
                    CutsceneCustomEvent custom = (CutsceneCustomEvent)clip;
                    custom.InitCustomAgent(menuData.agentUnit);
                    clip = custom;
                }
                clip.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                DataUtils.SetPresetDefaultValue(clip);
                track.events.Add(clip);
                group.group.tracks.Add(track);
                RefreshTimeGraph(GetAsset());
            }
            else
            {
                var curData = GetAsset();
                if (curData == null)
                {
                    GetOwner().ShowNotification(new GUIContent("请先创建一个CutsceneAsset!"), 2.0f);
                    return;
                }
                RegisterUndoData(false);
                if (curData.groups == null)
                    curData.groups = new List<CutsceneData.Group>();

                CutsceneData.Group group = new CutsceneData.Group();
                group.isGroup = false;
                group.id = curData.GeneratorGroupId();
                group.name = "Group" + (curData.groups.Count + 1).ToString("00"); ;
                group.tracks = new List<CutsceneData.Track>();
                curData.groups.Add(group);

                CutsceneData.Track track = new CutsceneData.Track();
                track.trackName = "Track" + (group.tracks.Count + 1).ToString("00");

                var clip = clipAttr.CreateEvent();
                if (menuData.agentUnit != null && clip is CutsceneCustomEvent)
                {
                    CutsceneCustomEvent custom = (CutsceneCustomEvent)clip;
                    custom.InitCustomAgent(menuData.agentUnit);
                    clip = custom;
                }
                clip.SetTime(Mathf.Max(0, GetSnappedTimeAtMousePosition(menuData.mousePos)));
                DataUtils.SetPresetDefaultValue(clip);
                track.events.Add(clip);
                group.tracks.Add(track);
                RefreshTimeGraph(curData);
            }
        }
        //--------------------------------------------------------
        void OnItemSelectChange(TreeAssetView.ItemData data)
        {
            if (data is EmptyItemLine)
            {
                m_pTimelineTree.SetSelection(new List<int>());
                return;
            }

            m_ItemSelected = data;
            var logics = GetLogics<ACutsceneLogic>();
            CutsceneData.Group selectGroup = null;
            if (data is TimelineTrackGroup)
            {
                selectGroup = ((TimelineTrackGroup)data).group;
            }
            else
            {
                selectGroup = ((TimelineTrack)data).groupData;
            }
            foreach (var db in logics)
            {
                db.OnSelectGroup(selectGroup);
            }

            if (data is TimelineTrackGroup)
            {
                TimelineTrackGroup group = data as TimelineTrackGroup;
                var selects = m_pTimelineTree.GetSelection();
                if (selects == null)
                    return;
                List<int> vSelects = new List<int>(selects);
                var datas = m_pTimelineTree.GetDatas();
                foreach (var db in datas)
                {
                    if (db is EmptyItemLine) continue;
                    var data1 = db as TreeItem;
                    if (data1 is TimelineTrack)
                    {
                        if(((TimelineTrack)data1).ownerGroup == group)
                        {
                            vSelects.Add(data1.id);
                        }
                    }
                }
                m_pTimelineTree.SetSelection(vSelects);
            }
        }
        //--------------------------------------------------------
        void OnItemRightClick(TreeAssetView.ItemData data)
        {
            CreateGroupMenu((data is EmptyItemLine)?null:data);
            m_bRightItemLine = true;
        }
        //--------------------------------------------------------
        void OnViewEmptyRightClick()
        {
            if (m_bRightItemLine) return;
            m_bRightItemLine = false;
            CreateGroupMenu();
        }
        //--------------------------------------------------------
        void DrawBinderGUI(Rect cellRc, CutsceneData.Group cutsceneGroup)
        {
            int binderId = cutsceneGroup.binderId;
            int cutBinder = binderId;
            var binders = ObjectBinderUtils.GetBinder(binderId);
            var obj = binders.GetBinder();
            EditorGUI.BeginChangeCheck();
            var gameObj = (GameObject)EditorGUI.ObjectField(new Rect(cellRc.xMax - 100, cellRc.y + 5, 84, cellRc.height - 5), obj?obj.gameObject:null, typeof(GameObject), true);
            bool changed = EditorGUI.EndChangeCheck();
            if (gameObj != null)
            {
                if(changed)
                {
                    CutsceneObjectBinder binder = gameObj.GetComponent<CutsceneObjectBinder>();
                    if (binder == null)
                    {
                        binder = gameObj.AddComponent<CutsceneObjectBinder>();
                    }
                    if (binder.GetBindID() == 0)
                    {
                        GetOwner().ShowNotification(new GUIContent("没有绑定CutsceneObjectBinder组件，先绑定设置好ID后，再绑定轨道"), 5);
                        Framework.Cutscene.Editor.BindIdInputPopup.Show(binder, cutsceneGroup, (binderData, gp) => {
                            if (binderData!=null && binderData.GetBindID() != gp.binderId)
                            {
                                RegisterUndoData();
                                gp.binderId = binderData.GetBindID();
                            }
                        });

                        if (binder.GetBindID() != 0)
                        {
                            obj = binder;
                            ObjectBinderUtils.BindObject(obj);
                        }
                        else
                        {
                            //  EditorUtility.DisplayDialog("错误", "绑定ID不能为0，请重新设置！", "确定");
                        }
                    }
                    else obj = binder;
                    if (obj != null)
                    {
                        cutBinder = obj.GetBindID();
                        if (obj) obj.Init();
                        if (cutBinder != cutsceneGroup.binderId)
                        {
                            RegisterUndoData();
                            cutsceneGroup.binderId = cutBinder;
                        }
                    }
                }
            }
        }
        //--------------------------------------------------------
        bool OnTreeCellLineDraw(TreeAssetView.RowArgvData agvRow)
        {
            var evt = Event.current;
            Color handleColor = Handles.color;
            Color color = GUI.color;
            GUI.color = Color.white;
            if (agvRow.itemData.data is EmptyItemLine)
                return true;

            EditorGUI.BeginDisabledGroup(IsRuntimePlayingCutscene());

            Rect cellRc = new Rect(agvRow.rowRect.x, agvRow.rowRect.y - LineItemExpand, agvRow.rowRect.width, agvRow.rowRect.height + LineItemExpand * 2);
            if (agvRow.itemData.data is TimelineTrackGroup)
            {
                TimelineTrackGroup trackGroup = agvRow.itemData.data as TimelineTrackGroup;
                if(trackGroup.group.HasFlag(EGroupFlag.UnActive))
                {
                    GUI.color = Color.yellow;
                }
                if (agvRow.column == 0)
                {
                    trackGroup.rect0 = agvRow.rowRect;
                    EditorGUI.DrawRect(cellRc, new Color(80.0f / 255.0f, 80.0f / 255.0f, 80.0f / 255.0f, 1.0f));
                    trackGroup.name = ClipDrawUtil.DelayedTextField(new Rect(cellRc.x + 10, cellRc.y, cellRc.width - 10, cellRc.height), trackGroup.name, TextUtil.titleStyle);
                    if (trackGroup.name != trackGroup.group.name)
                    {
                        RegisterUndoData(true);
                        trackGroup.group.name = trackGroup.name;
                    }
                    EditorGUI.DrawRect(new Rect(cellRc.x, cellRc.yMax - 2, cellRc.width - 16, 2), trackGroup.trackColor);

                    if (trackGroup.itemIcon() != null)
                    {
                        float iconSize = cellRc.height - 10;
                        GUI.DrawTexture(new Rect(cellRc.xMin + 5, cellRc.y + (cellRc.height - iconSize) / 2, iconSize, iconSize), (Texture2D)trackGroup.itemIcon());
                    }
                    {
                        DrawBinderGUI(cellRc, trackGroup.group);
                    }
                    if (GUI.Button(new Rect(cellRc.xMax - 16, cellRc.y + (cellRc.height - 20) / 2, 16, 20), "", EditorStyles.foldoutHeaderIcon))
                    {
                        CreateGroupMenu(trackGroup);
                    }
                }
                else if (agvRow.column == 1)
                {
                    trackGroup.rect1 = agvRow.rowRect;
                    float groupDuration = trackGroup.group.GetDuration();
                    if (groupDuration > 0.01f)
                    {
                        GUI.BeginClip(cellRc);
                        var offsetFromTimeSpaceToPixelSpace = timeAreaTranslation.x;

                        var start = 0;// trackGroup.group.GetStart();
                        var end = groupDuration;

                        Rect drawRect = Rect.MinMaxRect(
                            Mathf.Round(start * timeAreaScale.x + offsetFromTimeSpaceToPixelSpace), 0,
                            Mathf.Round(end * timeAreaScale.x + offsetFromTimeSpaceToPixelSpace), Mathf.Round(cellRc.height)
                        );
                        // 在 timeline 区域绘制一条竖线
                        EditorGUI.DrawRect(
                            drawRect,
                            new Color(0.5f, 0.5f, 0.5f, 0.5f)
                        );

                        // 也可以在该位置绘制文本
                        string durationStr = $"id={trackGroup.group.id}  {groupDuration:0.##}s";
                        Vector2 labelSize = EditorStyles.miniLabel.CalcSize(new GUIContent(durationStr));
                        Rect labelRect = new Rect(
                            drawRect.x + drawRect .width/ 2 - labelSize.x / 2,
                            drawRect.y + labelSize.y/2,
                            labelSize.x,
                            labelSize.y
                        );
                        EditorGUI.LabelField(labelRect, durationStr, EditorStyles.miniLabel);
                        GUI.EndClip();
                    }
                }
            }
            else
            {
                TimelineTrack track = agvRow.itemData.data as TimelineTrack;
                if (agvRow.column == 0)
                {
                    track.rect0 = agvRow.rowRect;
                    bool isUnActive = false;
                    if (track.ownerGroup != null && track.ownerGroup.group!=null && track.ownerGroup.group.isGroup)
                    {
                        cellRc.position += Vector2.right * m_pTimelineTree.DepthIndentWidth;
                        cellRc.width -= m_pTimelineTree.DepthIndentWidth;

                        isUnActive = track.ownerGroup.group.HasFlag(EGroupFlag.UnActive);
                    }
                    if (isUnActive)
                    {
                        GUI.color = Color.yellow;
                    }
                    EditorGUI.BeginDisabledGroup(isUnActive);
                    //Track
                    EditorGUI.DrawRect(cellRc, new Color(80.0f / 255.0f, 80.0f / 255.0f, 80.0f / 255.0f, 1.0f));
                    EditorGUI.DrawRect(new Rect(cellRc.x, cellRc.y, 2, cellRc.height), track.trackColor);

                    float labelOffset = 2;
                    if (track.itemIcon() != null)
                    {
                        float iconSize = cellRc.height - 10;
                        labelOffset += 3 + iconSize + 2;
                        GUI.DrawTexture(new Rect(cellRc.xMin + 3, cellRc.y + (cellRc.height - iconSize) / 2, iconSize, iconSize), (Texture2D)track.itemIcon());
                    }
                    track.name = ClipDrawUtil.DelayedTextField(new Rect(cellRc.x + labelOffset, cellRc.y, cellRc.width - labelOffset, cellRc.height), track.name, TextUtil.titleStyle);
                    if (track.name != track.track.trackName)
                    {
                        RegisterUndoData(true);
                        track.track.trackName = track.name;
                    }
                    if(track.ownerGroup ==null)
                    {
                        DrawBinderGUI(cellRc, track.groupData);
                    }
                    if (GUI.Button(new Rect(cellRc.xMax - 16, cellRc.y + (cellRc.height - 20) / 2, 16, 20), "", EditorStyles.foldoutHeaderIcon))
                    {
                        CreateGroupMenu(track);
                    }
                    EditorGUI.EndDisabledGroup();
                }
                else if (agvRow.column == 1)
                {
                    bool isUnActive = false;
                    if (track.ownerGroup != null && track.ownerGroup.group != null && track.ownerGroup.group.isGroup)
                    {
                        isUnActive = track.ownerGroup.group.HasFlag(EGroupFlag.UnActive);
                    }
                    EditorGUI.BeginDisabledGroup(isUnActive);

                    track.rect1 = agvRow.rowRect;
                    using (new GUIViewportScope(cellRc))
                    {
                        m_vTempDraws.Clear();
                        for (int i = 0; i < track.clipDraws.Count; ++i)
                        {
                            if (m_pDragDraw.IsSelected(track.clipDraws[i]))
                            {
                                m_vTempDraws.Add(new SelectDraw(track.clipDraws[i],i));
                                continue;
                            }
                            if (track.clipDraws[i].hasBlendIn || track.clipDraws[i].hasBlendOut)
                            {
                                m_vTempDraws.Add(new SelectDraw(track.clipDraws[i], i));
                                continue;
                            }

                            track.clipDraws[i].Draw(cellRc, this);
                            track.track.clips[i] = track.clipDraws[i].clip;
                        }
                        for (int i = 0; i < track.eventDraws.Count; ++i)
                        {
                       //     if (m_pDragDraw.IsSelected(track.eventDraws[i]))
                            {
                                m_vTempDraws.Add(new SelectDraw(track.eventDraws[i], i));
                      //          continue;
                            }
                       //     track.eventDraws[i].Draw(cellRc, this);
                       //     track.track.events[i] = track.eventDraws[i].clip;
                        }
                        for(int i =0; i < m_vTempDraws.Count; ++i)
                        {
                            var tempDraw = m_vTempDraws[i];
                            tempDraw.draw.Draw(cellRc, this);
                            if(tempDraw.draw is EventDraw)
                            {
                                track.track.events[tempDraw.index] = ((EventDraw)tempDraw.draw).clip;
                            }
                            else if (tempDraw.draw is ClipDraw)
                            {
                                track.track.clips[tempDraw.index] = ((ClipDraw)tempDraw.draw).clip;
                            }
                            if (evt.button == 1)
                            {
                                if (evt.type == EventType.MouseUp)
                                {
                                    if (cellRc.Contains(evt.mousePosition) && m_pDragDraw.IsSelected(tempDraw.draw))
                                    {
                                        CreateGroupMenu(tempDraw.draw);
                                    }
                                }
                            }
                        }
                    }

                    EditorGUI.EndDisabledGroup();
                }
            }
            EditorGUI.EndDisabledGroup();
            GUI.color = color;
            Handles.color = handleColor;
            return true;
        }
        //--------------------------------------------------------
        public double RoundToFrame(double time, double frameRate)
        {
            if (frameRate <= 0) frameRate = 30.0f;

            var frameBefore = (int)Math.Floor(time * frameRate) / frameRate;
            var frameAfter = (int)Math.Ceiling(time * frameRate) / frameRate;

            return Math.Abs(time - frameBefore) < Math.Abs(time - frameAfter) ? frameBefore : frameAfter;
        }
        //--------------------------------------------------------
        public double SnapToFrame(double time)
        {
            //if (state.timeReferenceMode == TimeReferenceMode.Global)
            //{
            //    time = state.editSequence.ToGlobalTime(time);
            //    time = TimeUtility.RoundToFrame(time, state.referenceSequence.frameRate);
            //    return state.editSequence.ToLocalTime(time);
            //}

            return RoundToFrame(time, GetFrameRate());
        }
        //--------------------------------------------------------
        public double SnapToFrameIfRequired(double currentTime, bool snapToFrame = true)
        {
            return snapToFrame ? SnapToFrame(currentTime) : currentTime;
        }
        //--------------------------------------------------------
        public float GetSnappedTimeAtMousePosition(Vector2 mousePos)
        {
            return (float)SnapToFrameIfRequired(ScreenSpacePixelToTimeAreaTime(mousePos.x));
        }
        //--------------------------------------------------------
        public float ScreenSpacePixelToTimeAreaTime(float p)
        {
            // transform into trackGroup space by offsetting the pixel by the screen-space offset of the time area
            p -= timelineRect.x;
            return TrackSpacePixelToTimeAreaTime(p);
        }
        //--------------------------------------------------------
        public float TrackSpacePixelToTimeAreaTime(float p)
        {
            p -= timeAreaTranslation.x;

            if (timeAreaScale.x > 0.0f)
                return p / timeAreaScale.x;

            return p;
        }
        //--------------------------------------------------------
        public float PixelDeltaToDeltaTime(float p)
        {
            return PixelToTime(p) - PixelToTime(0);
        }
        //--------------------------------------------------------
        public float TimeAreaPixelToTime(float pixel)
        {
            return PixelToTime(pixel);
        }
        //--------------------------------------------------------
        float ConvertTimeToGUIPos(float time)
        {
            return TimeToPixel(time);
        }
        //--------------------------------------------------------
        public float TimeToPixel(double time)
        {
            return m_TimeArea.TimeToPixel((float)time, m_TimeArea.rect);
        }
        //--------------------------------------------------------
        public float PixelToTime(float pos)
        {
            return m_TimeArea.PixelToTime(pos, m_TimeArea.rect);
        }
        //--------------------------------------------------------
        public float TimeToTimeAreaPixel(double t) // TimeToTimeAreaPixel
        {
            float pixelX = (float)t;
            pixelX *= timeAreaScale.x;
            pixelX += timeAreaTranslation.x;// + sequencerHeaderWidth;
            return pixelX;
        }
        //--------------------------------------------------------
        public float TimeToScreenSpacePixel(double time)
        {
            float pixelX = (float)time;
            pixelX *= timeAreaScale.x;
            pixelX += timeAreaTranslation.x;
            return pixelX;
        }
        //--------------------------------------------------------
        float ConvertGUIPosToTime(float pos)
        {
            float time = PixelToTime(pos);
            return time;
        }
        //--------------------------------------------------------
        public bool TimeIsInRange(float value)
        {
            Rect shownArea = m_TimeArea.shownArea;
            return value >= shownArea.x && value <= shownArea.xMax;
        }
        //--------------------------------------------------------
        void EnableCutscene(CutsceneInstance pCutscene, bool bEnable)
        {
            var logics = GetLogics<ACutsceneLogic>();
            foreach (var db in logics)
            {
                db.OnEnableCutscene(pCutscene,bEnable);
            }
            var agentWindow = GetOwner<CutsceneEditor>().GetAgentTreeWindow();
            if(agentWindow!=null)
            {
                agentWindow.GetLogics().ForEach((logic) => {
                    if(logic is ACutsceneLogic)
                        ((ACutsceneLogic)logic).OnEnableCutscene(pCutscene, bEnable);
                });
            }
        }
    }
}
#endif