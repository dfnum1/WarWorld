/********************************************************************
生成日期:	06:30:2025
类    名: 	ClipDraw
作    者:	HappLI
描    述:	剪辑绘制
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Runtime;
using Framework.ED;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using static Framework.Cutscene.Editor.TimelineDrawLogic;

namespace Framework.Cutscene.Editor
{
    public class DragDraw
    {
        private TimelineDrawLogic m_pLogic;
        private TreeAssetView m_pTreeView;
        private TimelineDrawLogic.TreeItem m_DragItem = null;
        private TimelineDrawLogic.TreeItem m_HoverTargetItem = null;

        int m_nStarDrag = 0;
        bool m_bDragDuration = false;
        float m_fDragGrabTime = 0;
        float m_fDragOfffsetTime = 0;
        public bool canDrag = true;

        List<object> m_vTempCaches = new List<object>();
        List<IDraw> m_vSelectClips = new List<IDraw>();
        List<ClipDraw> m_vSortedTemp = new List<ClipDraw>();
        public DragDraw(TimelineDrawLogic pLogic, TreeAssetView treeView)
        {
            m_pTreeView = treeView;
            m_pLogic = pLogic;
        }    
        //--------------------------------------------------------
        public bool IsDragging()
        {
            return m_nStarDrag == 2;
        }
        //-----------------------------------------------------
        public void OnEvent(Event evt)
        {
            if (!canDrag)
                return;
            m_vTempCaches.Clear();
            for (int i = 0; i < m_vSelectClips.Count; ++i)
            {
                m_vTempCaches.Add(m_vSelectClips[i].GetOwnerTrack());
            }
            if (evt.type == EventType.MouseDown)
            {
                DragEnd();
                bool bDirtySelect = false;
                if (m_pLogic.timeZoomRect.Contains(evt.mousePosition - m_pLogic.GetRect().position))
                {
                    var datas = m_pTreeView.GetDatas();
                    int lastCnt = m_vSelectClips.Count;
                    if(!evt.control)
                    {
                        m_vSelectClips.Clear();
                    }
                    foreach (var db in datas)
                    {
                        if (db is TimelineTrack)
                        {
                            TimelineTrack timelineTrack = db as TimelineTrack;
                            foreach (var clip in timelineTrack.eventDraws)
                            {
                                if (clip.CanSelect(evt, m_pLogic))
                                {
                                    if (!m_vSelectClips.Contains(clip))
                                    {
                                        m_vSelectClips.Add(clip);
                                        bDirtySelect = true;
                                    }
                                    else
                                    {
                                        bDirtySelect = true;
                                    }
                                    break;
                                }
                            }
                            if(!bDirtySelect)
                            {
                                foreach (var clip in timelineTrack.clipDraws)
                                {
                                    if (clip.CanSelect(evt, m_pLogic))
                                    {
                                        if (!m_vSelectClips.Contains(clip))
                                        {
                                            m_vSelectClips.Add(clip);
                                            bDirtySelect = true;
                                        }
                                        break;
                                    }
                                }
                            }

                        }
                    }
                    if (lastCnt != m_vSelectClips.Count)
                    {
                        bDirtySelect = true;
                    }
                    if (bDirtySelect)
                        SelectClips(m_vSelectClips);
                }
                else
                {
                    if (!m_pLogic.IsMouseInTimeAreaHeadAndScrollBar(evt))
                    {
                        int lastCnt = m_vSelectClips.Count;
                        m_vSelectClips.Clear();
                        if (lastCnt != m_vSelectClips.Count)
                            SelectClips(m_vSelectClips);
                    }
                }
                if (m_vSelectClips != null)
                {
                    foreach (var db in m_vSelectClips)
                    {
                        if (db.CanSelect(evt, m_pLogic))
                        {
                            m_nStarDrag = 1;
                            m_bDragDuration = db.CanSelectDurationSnap(evt, m_pLogic);
                        }
                    }
                }
            }
            else if (evt.type == EventType.MouseUp)
            {
                if (m_nStarDrag == 2)
                {
                    DragEnd();
                }
                else
                {
                    if (!m_pLogic.IsMouseInTimeAreaHeadAndScrollBar(evt) && !evt.control)
                    {
                        bool bDirtySelect = false;
                        for (int i = 0; i < m_vSelectClips.Count;)
                        {
                            if (!m_vSelectClips[i].CanSelect(evt, m_pLogic))
                            {
                                m_vSelectClips.RemoveAt(i);
                                bDirtySelect = true;
                            }
                            else
                                ++i;
                        }
                        if (bDirtySelect)
                            SelectClips(m_vSelectClips);
                    }
                }
            }
            else if (evt.type == EventType.MouseDrag)
            {
                if (m_pLogic.timeZoomRect.Contains(evt.mousePosition - m_pLogic.GetRect().position) &&
                    !m_pLogic.IsMouseInTimeAreaHeadAndScrollBar(evt))
                {
                    foreach (var db in m_vSelectClips)
                        m_vTempCaches.Add(db);
                    if (m_nStarDrag == 1)
                    {
                        Rect right = new Rect(m_pLogic.rightRect.position + m_pLogic.GetRect().position, new Vector2(m_pLogic.rightRect.width, m_pLogic.rightRect.height - 30));
                        if (right.Contains(evt.mousePosition))
                        {
                            m_fDragGrabTime = m_pLogic.GetSnappedTimeAtMousePosition(evt.mousePosition);
                            m_nStarDrag = 2;
                        }
                    }
                    if (m_nStarDrag == 2)
                    {
                        float grab = m_pLogic.GetSnappedTimeAtMousePosition(evt.mousePosition);
                        m_fDragOfffsetTime = grab - m_fDragGrabTime;
                        foreach (var db in m_vSelectClips)
                        {
                            db.DragOffset(m_fDragOfffsetTime, false, false, m_bDragDuration);
                        }
                    }
                    evt.Use();
                }
            }
            else if(evt.type == EventType.KeyUp)
            {
                if(evt.keyCode == KeyCode.Delete)
                {
                    if (m_vSelectClips.Count > 0)
                    {
                        if (EditorUtility.DisplayDialog("提示", "确认是要删除当前选中","删除", "取消"))
                        {
                            List<object> vObjs = new List<object>();
                            foreach (var db in m_vSelectClips) vObjs.Add(db);
                            m_vSelectClips.Clear();
                            m_pLogic.DeleteData(vObjs);
                            m_vSelectClips.Clear();
                        }
                    }
                }
            }

                int nDirty = 0;
            for (int i = 0; i < m_vTempCaches.Count; ++i)
            {
                var track = m_vTempCaches[i] as TimelineTrack;
                if (track == null)
                    continue;
                if (track.clipDraws == null)
                    continue;
                if (track.clipDraws.Count <= 1)
                    continue;
                for (int j = 0; j < track.clipDraws.Count; ++j)
                {
                    var clip = track.clipDraws[j].clip;
                    int lastDirty = nDirty;
                    nDirty += clip.SetBlendDuration(ECutsceneClipBlendType.In, -1, false) ? 1 : 0;
                    nDirty += clip.SetBlendDuration(ECutsceneClipBlendType.Out, -1, false) ? 1 : 0;
                    if (lastDirty != nDirty)
                    {
                        break;
                    }
                }
            }
            if (nDirty != 0)
                m_pLogic.RegisterUndoData();
            for (int i = 0; i < m_vTempCaches.Count; ++i)
            {
                var track = m_vTempCaches[i] as TimelineTrack;
                if (track == null)
                    continue;
                if (track.clipDraws == null)
                    continue;
                if (track.clipDraws.Count <= 1)
                    continue;
                m_vSortedTemp.Clear();
                for (int j = 0; j < track.clipDraws.Count; ++j)
                {
                    var clip = track.clipDraws[j].clip;
                    int lastDirty = nDirty;
                    nDirty += clip.SetBlendDuration(ECutsceneClipBlendType.In, -1) ? 1 : 0;
                    nDirty += clip.SetBlendDuration(ECutsceneClipBlendType.Out, -1) ? 1 : 0;
                    m_vSortedTemp.Add(track.clipDraws[j]);
                    if (lastDirty != nDirty)
                    {
                        track.clipDraws[j].clip = clip;
                        track.track.clips[j] = clip;
                    }
                }
                m_vSortedTemp.Sort((c1, c2) =>
Math.Abs(c1.GetBegin() - c2.GetBegin()) < ClipDrawUtil.kTimeEpsilon ? c1.GetDuration().CompareTo(c2.GetDuration()) : c1.GetBegin().CompareTo(c2.GetBegin()));
                for (int j = 0; j < m_vSortedTemp.Count; ++j)
                {
                    var db = m_vSortedTemp[j];
                    if (!db.SupportsBlending())
                        continue;
                    var blendIn = db;
                    int prev = j - 1;
                    if (prev < 0) prev = 0;
                    ClipDraw blendOut = m_vSortedTemp[prev];
                    if (ClipDrawUtil.Overlaps(blendOut, blendIn))
                        ClipDrawUtil.UpdateClipIntersection(blendOut, blendIn);
                }
                m_vSortedTemp.Clear();
            }
        }
        //-----------------------------------------------------
        public void DragEnd()
        {
            if(m_nStarDrag == 2)
            {
                m_nStarDrag = 0;
                bool bDirty = false;
                foreach (var db in m_vSelectClips)
                {
                    if (db.DragOffset(m_fDragOfffsetTime, false, false, m_bDragDuration))
                        bDirty = true;
                }
                if (bDirty)
                {
                    m_pLogic.RegisterUndoData();
                }
                foreach (var db in m_vSelectClips)
                {
                    if (db.DragOffset(m_fDragOfffsetTime, true, false, m_bDragDuration))
                        bDirty = true;
                }
            }

            for (int i = 0; i < m_vSelectClips.Count; ++i)
            {
                m_vSelectClips[i].DragEnd();
            }
            m_fDragGrabTime = 0;
            m_fDragOfffsetTime = 0;
            m_nStarDrag = 0;
            m_bDragDuration = false;
        }
        //-----------------------------------------------------
        public void OnGUI()
        {
            if (m_vSelectClips.Count > 0 && m_nStarDrag == 2)
            {
                using (new GUIViewportScope(m_pLogic.timeZoomRect))
                {
                    bool bDraging = false;
                    float minTime = float.MaxValue;
                    float maxTime = float.MinValue;
                    for (int i = 0; i < m_vSelectClips.Count; ++i)
                    {
                        bDraging = true;
                        if (m_vSelectClips[i] is EventDraw)
                        {
                            minTime = Mathf.Min(m_vSelectClips[i].GetBegin(), minTime);
                            maxTime = Mathf.Max(m_vSelectClips[i].GetBegin(), maxTime);
                        }
                        else
                        {
                            minTime = Mathf.Min(m_vSelectClips[i].GetBegin(), minTime);
                            maxTime = Mathf.Max(m_vSelectClips[i].GetEnd(), maxTime);
                        }
                    }
                    if (bDraging)
                        ClipDrawUtil.Draw(m_pLogic, minTime, maxTime);
                }
            }
        }
        //--------------------------------------------------------
        internal List<IDraw> GetSelectClips()
        {
            return m_vSelectClips;
        }
        //--------------------------------------------------------
        internal bool IsSelected(IDraw draw)
        {
            return m_vSelectClips.Contains(draw);
        }
        //--------------------------------------------------------
        internal void ClearSelected()
        {
            m_vSelectClips.Clear();
        }
        //--------------------------------------------------------
        internal void AddSelected(IDraw draw)
        {
            if (m_vSelectClips.Contains(draw))
                return;
            m_vSelectClips.Add(draw);
        }
        //--------------------------------------------------------
        internal void SelectClips(List<IDraw> vSelectClips)
        {
            var logics = m_pLogic.GetOwner().GetLogics();
            foreach (var db in logics)
            {
                if (db.GetType() == this.GetType())
                    continue;
                if (db is ACutsceneLogic)
                {
                    ((ACutsceneLogic)db).OnSelectClips(vSelectClips);
                }
            }
        }
    }
}
#endif