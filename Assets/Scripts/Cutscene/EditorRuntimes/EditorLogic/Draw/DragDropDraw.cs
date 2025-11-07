/********************************************************************
生成日期:	06:30:2025
类    名: 	ClipDraw
作    者:	HappLI
描    述:	剪辑绘制
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Runtime;
using Framework.ED;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using static Framework.Cutscene.Editor.TimelineDrawLogic;

namespace Framework.Cutscene.Editor
{
    public class DragDropDraw
    {
        private TimelineDrawLogic m_pLogic;
        private TreeAssetView m_pTreeView;
        private TimelineDrawLogic.TreeItem m_DragItem = null;
        private TimelineDrawLogic.TreeItem m_HoverTargetItem = null;
        private bool m_IsDragging = false;

        private int m_InsertIndex = -1; // 箭头插入索引
        private Rect m_ArrowRect = Rect.zero; // 箭头绘制区域
        private bool m_ArrowUp = false; // true=向上箭头，false=向下箭头
        private bool m_ShowJoinGroup = false; // 是否显示加入组标志

        public bool canDrag = true;
        public DragDropDraw(TimelineDrawLogic pLogic, TreeAssetView treeView)
        {
            m_pTreeView = treeView;
            m_pLogic = pLogic;
            treeView.OnDragItem += OnDragItem;
        }
        //-----------------------------------------------------
        public void OnEvent(Event evt)
        {
            if (!canDrag)
                return;
            Vector2 localPos = evt.mousePosition;
            localPos.y -= m_pLogic.GetRect().yMin + m_pLogic.timelineRect.y + TimelineDrawLogic.TimeRulerAreaHeight;
            localPos.x -= m_pLogic.GetRect().xMin;
            localPos += m_pTreeView.state.scrollPos;
            if (!m_pLogic.leftRect.Contains(localPos))
            {
                return;
            }
            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                var hitItem = GetHitTreeItem(localPos);
                if (hitItem == m_DragItem)
                {
                    m_IsDragging = true;
                    m_DragItem = hitItem;
                    evt.Use();
                }
                else
                {
                    m_IsDragging = false;
                }
            }
            // 拖拽中
            if (m_IsDragging && evt.type == EventType.MouseDrag)
            {
                m_HoverTargetItem = GetHitTreeItem(localPos);

                UpdateInsertIndex(localPos);
                evt.Use();
            }
            // 拖拽结束
            if (m_IsDragging && evt.type == EventType.MouseUp)
            {
                m_HoverTargetItem = GetHitTreeItem(localPos);
                // 回调
                DragTreeEnd(m_DragItem, m_HoverTargetItem, m_ArrowUp, m_ShowJoinGroup);

                // 重置状态
                m_IsDragging = false;
                m_DragItem = null;
                m_HoverTargetItem = null;
                m_InsertIndex = -1;
                evt.Use();
            }
            // ...原有代码...
        }
        //-----------------------------------------------------
        public void OnGUI()
        {
            if (m_IsDragging && m_DragItem != null)
            {
                // 浮动条
                var dragRect = new Rect(
                    m_pLogic.leftRect.x + 4 + m_pLogic.GetRect().x,
                    Event.current.mousePosition.y - m_pTreeView.GetRowHeight() / 2,
                    m_pLogic.leftRect.width - 8,
                    m_pTreeView.GetRowHeight()
                );
                EditorGUI.DrawRect(dragRect, new Color(0.2f, 0.6f, 1f, 0.5f));
                GUI.Label(dragRect, m_DragItem.name, EditorStyles.boldLabel);

                // 箭头或加入组标志
                if (m_ShowJoinGroup)
                {
                    DrawJoinGroupMark(m_ArrowRect);
                }
                else if (m_InsertIndex >= 0)
                {
                    DrawInsertArrow(m_ArrowRect);
                }
            }
        }
        //-----------------------------------------------------
        private void UpdateInsertIndex(Vector2 mousePos)
        {
            var datas = m_pTreeView.GetDatas();
            m_InsertIndex = -1;
            m_ArrowRect = Rect.zero;
            m_ShowJoinGroup = false;

            // 坐标转换
            Vector2 localPos = mousePos;

            var dragRect = new Rect(
                localPos.x + 4,
                localPos.y - m_pTreeView.GetRowHeight() / 2,
                m_pLogic.leftRect.width - 8,
                m_pTreeView.GetRowHeight());

            // 找出所有交叉的item
            List<(int idx, TreeItem item)> overlaps = new List<(int, TreeItem)>();
            for (int i = 0; i < datas.Count; ++i)
            {
                if (datas[i] is EmptyItemLine) continue;
                if (datas[i] is TreeItem item && item != m_DragItem && item.rect0.Overlaps(dragRect))
                {
                    overlaps.Add((i, item));
                    break;
                }
            }

            if (overlaps.Count == 1)
            {
                int idx = overlaps[0].idx;
                var item = overlaps[0].item;
                var rect = item.rect0;

                if (item is TimelineDrawLogic.TimelineTrackGroup)
                {
                    // 分三段
                    float third = rect.height / 3f;
                    float yTop = rect.yMin;
                    float yMid = rect.yMin + third;
                    float yBot = rect.yMax - third;

                    if (localPos.y >= yMid && localPos.y <= yBot)
                    {
                        // 中间区域，显示加入组
                        m_ShowJoinGroup = true;
                        m_InsertIndex = idx;
                        m_ArrowRect = rect;
                    }
                    else if (localPos.y < yMid)
                    {
                        // 上半区，插入前
                        m_ShowJoinGroup = false;
                        m_InsertIndex = idx;
                        m_ArrowRect = new Rect(rect.x, rect.yMin - 4, m_pLogic.leftRect.width, 8);
                        m_ArrowUp = true;
                    }
                    else
                    {
                        // 下半区，插入后
                        m_ShowJoinGroup = false;
                        m_InsertIndex = idx + 1;
                        m_ArrowRect = new Rect(rect.x, rect.yMax - 4, m_pLogic.leftRect.width, 8);
                        m_ArrowUp = false;
                    }
                }
                else
                {
                    float midY = rect.yMin + rect.height / 2;
                    bool isLast = idx == datas.Count - 1;

                    if (localPos.y < midY)
                    {
                        // 上半部，插入前，向上箭头
                        m_InsertIndex = idx;
                        m_ArrowRect = new Rect(rect.x, rect.yMin - 4, m_pLogic.leftRect.width, 8);
                        m_ArrowUp = true;
                    }
                    else
                    {
                        // 下半部
                        m_InsertIndex = idx + 1;
                        m_ArrowRect = new Rect(rect.x, rect.yMax - 4, m_pLogic.leftRect.width, 8);
                        m_ArrowUp = false;
                    }
                }
            }
            else
            {
                m_InsertIndex = -1;
                m_ArrowRect = Rect.zero;
                m_ShowJoinGroup = false;
            }
            m_ArrowRect.position += Vector2.up * (m_pLogic.GetRect().yMin + m_pLogic.timelineRect.y + TimelineDrawLogic.TimeRulerAreaHeight);
            m_ArrowRect.position += Vector2.right * m_pLogic.GetRect().xMin;
        }
        //-----------------------------------------------------
        private void DrawJoinGroupMark(Rect rect)
        {
            // 高亮整个组区域
            EditorGUI.DrawRect(rect, new Color(0.2f, 1f, 0.4f, 0.25f));
            // 画一个加号
            Vector2 center = new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
            Handles.color = new Color(0.2f, 1f, 0.4f, 1f);
            Handles.DrawLine(center + new Vector2(-8, 0), center + new Vector2(8, 0));
            Handles.DrawLine(center + new Vector2(0, -8), center + new Vector2(0, 8));
        }
        //-----------------------------------------------------
        private void DrawInsertArrow(Rect rect)
        {
            if (m_ArrowRect.size.sqrMagnitude <= 0)
                return;

            Vector2 center = new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
            Vector2 p0, p1, p2;
            if (m_ArrowUp)
            {
                // 向上箭头
                p0 = center + new Vector2(-8, 4);
                p1 = center + new Vector2(8, 4);
                p2 = center + new Vector2(0, -6);
            }
            else
            {
                // 向下箭头
                p0 = center + new Vector2(-8, -4);
                p1 = center + new Vector2(8, -4);
                p2 = center + new Vector2(0, 6);
            }
            Handles.color = new Color(0.2f, 0.8f, 1f, 1f);
            Handles.DrawAAConvexPolygon(p0, p1, p2);
        }
        //-----------------------------------------------------
        private TreeItem GetHitTreeItem(Vector2 mousePos)
        {
            // 遍历所有可见 TreeItem，判断 mousePos 是否在其 rect 内
            var datas = m_pTreeView.GetDatas();
            Vector2 localPos = mousePos;
            //   Rect rect = new Rect(localPos, new Vector2(m_pLogic.leftRect.width, m_pTreeView.GetRowHeight()/2));
            var dragRect = new Rect(
                           localPos.x + 4 + m_pTreeView.DepthIndentWidth,
                           localPos.y - m_pTreeView.GetRowHeight() / 2,
                           m_pLogic.leftRect.width - 8 - m_pTreeView.DepthIndentWidth,
                           m_pTreeView.GetRowHeight());
            foreach (var db in datas)
            {
                if (db is EmptyItemLine) continue;
                if (db is TreeItem item && item.rect0.Overlaps(dragRect))
                    return item;
            }
            return null;
        }
        //--------------------------------------------------------
        bool OnDragItem(TreeAssetView.TreeItemData t)
        {
            m_IsDragging = false;
            var dragItem = t.data as TreeItem;
            if (dragItem == null)
                return false;

         //   m_IsDragging = true;
            m_DragItem = dragItem;
            return true;
        }
        //--------------------------------------------------------
        public void DragTreeEnd(TreeItem source, TreeItem target, bool bInsertBefore, bool bAddGroup)
        {
            if (source == null || target == null || source == target)
                return;

            var asset = m_pLogic.GetAsset();

            // 获取源和目标的 Group/Track 信息
            CutsceneData.Group sourceGroup = null;
            CutsceneData.Track sourceTrack = null;
            if (source is TimelineTrackGroup srcGroupItem)
                sourceGroup = srcGroupItem.group;
            else if (source is TimelineTrack srcTrackItem)
            {
                sourceGroup = srcTrackItem.groupData;
                sourceTrack = srcTrackItem.track;
            }

            CutsceneData.Group targetGroup = null;
            CutsceneData.Track targetTrack = null;
            if (target is TimelineTrackGroup tgtGroupItem)
                targetGroup = tgtGroupItem.group;
            else if (target is TimelineTrack tgtTrackItem)
            {
                targetGroup = tgtTrackItem.groupData;
                targetTrack = tgtTrackItem.track;
            }

            // 1. 源不是组（TimelineTrack）
            if (!(source is TimelineTrackGroup))
            {
                TimelineTrack trackSource = source as TimelineTrack;
                // 1.1 目标是组，且bAddGroup，加入到组
                if (target is TimelineTrackGroup && bAddGroup)
                {
                    if (sourceGroup != null && sourceGroup.tracks != null && targetGroup != null)
                    {
                        if(trackSource.ownerGroup == null)
                        {
                            // 将源group的所有track加入目标组
                            m_pLogic.RegisterUndoData(false);
                            if (targetGroup.tracks == null)
                                targetGroup.tracks = new List<CutsceneData.Track>();
                            targetGroup.tracks.AddRange(sourceGroup.tracks);
                            sourceGroup.tracks.Clear();
                            asset.groups.Remove(sourceGroup);
                            m_pLogic.RefreshTimeGraph(asset);
                        }
                        else
                        {
                            m_pLogic.RegisterUndoData(false);
                            if (targetGroup.tracks == null)
                                targetGroup.tracks = new List<CutsceneData.Track>();
                            targetGroup.tracks.Add(sourceTrack);
                            trackSource.ownerGroup.group.tracks.Remove(sourceTrack);
                            m_pLogic.RefreshTimeGraph(asset);
                        }
                    }
                    return;
                }

                // 1.2 目标是Track
                if (target is TimelineTrack)
                {
                    // 1.2.1 源有ownerGroup（在组内）
                    if (source is TimelineTrack srcTrack && srcTrack.ownerGroup != null)
                    {
                        m_pLogic.RegisterUndoData(false);
                        var srcOwnerGroup = srcTrack.ownerGroup.group;
                        srcOwnerGroup.tracks.Remove(sourceTrack);

                        // 插入到目标track前/后
                        var tgtOwnerGroup = (target as TimelineTrack).ownerGroup?.group;
                        if (tgtOwnerGroup != null)
                        {
                            int tgtIdx = tgtOwnerGroup.tracks.IndexOf(targetTrack);
                            int insertIdx = bInsertBefore ? tgtIdx : tgtIdx + 1;
                            tgtOwnerGroup.tracks.Insert(insertIdx, sourceTrack);
                        }
                        else
                        {
                            // 目标不是组，插入到asset.groups
                            int tgtIdx = asset.groups.IndexOf(targetGroup);
                            int insertIdx = bInsertBefore ? tgtIdx : tgtIdx + 1;
                            // 创建新Group包裹
                            var newGroup = new CutsceneData.Group
                            {
                                isGroup = false,
                                name = "Group" + (asset.groups.Count + 1).ToString("00"),
                                tracks = new List<CutsceneData.Track> { sourceTrack }
                            };
                            newGroup.id = asset.GeneratorGroupId();
                            asset.groups.Insert(insertIdx, newGroup);
                        }
                        m_pLogic.RefreshTimeGraph(asset);
                        return;
                    }
                    // 1.2.2 源是独立track
                    else if (source is TimelineTrack srcTrack2)
                    {
                        m_pLogic.RegisterUndoData(false);
                        asset.groups.Remove(sourceGroup);

                        var tgtOwnerGroup = (target as TimelineTrack).ownerGroup?.group;
                        if (tgtOwnerGroup != null)
                        {
                            int tgtIdx = tgtOwnerGroup.tracks.IndexOf(targetTrack);
                            int insertIdx = bInsertBefore ? tgtIdx : tgtIdx + 1;
                            tgtOwnerGroup.tracks.Insert(insertIdx, srcTrack2.track);
                        }
                        else
                        {
                            int tgtIdx = asset.groups.IndexOf(targetGroup);
                            int insertIdx = bInsertBefore ? tgtIdx : tgtIdx + 1;
                            asset.groups.Insert(insertIdx, sourceGroup);
                        }
                        m_pLogic.RefreshTimeGraph(asset);
                        return;
                    }
                }

                // 1.3 目标是组，但不是bAddGroup（创建独立组插入）
                if (target is TimelineTrackGroup && !bAddGroup)
                {
                    if (source is TimelineTrack srcTrack3 && srcTrack3.groupData != null)
                    {
                        m_pLogic.RegisterUndoData(false);
                        srcTrack3.groupData.tracks.Remove(srcTrack3.track);

                        var newGroup = new CutsceneData.Group
                        {
                            isGroup = false,
                            name = srcTrack3.track.trackName,
                            tracks = new List<CutsceneData.Track> { srcTrack3.track }
                        };
                        newGroup.id = asset.GeneratorGroupId();
                        int tgtIdx = asset.groups.IndexOf(targetGroup);
                        int insertIdx = bInsertBefore ? tgtIdx : tgtIdx + 1;
                        asset.groups.Insert(insertIdx, newGroup);
                        m_pLogic.RefreshTimeGraph(asset);
                    }
                    return;
                }
            }
            // 2. 源是组（TimelineTrackGroup）
            else
            {
                // 2.1 目标是组，且bAddGroup，加入到组
                if (target is TimelineTrackGroup)
                {
                    if(bAddGroup)
                    {
                        m_pLogic.RegisterUndoData(false);
                        if (sourceGroup != null && sourceGroup.tracks != null && targetGroup != null)
                        {
                            if (targetGroup.tracks == null)
                                targetGroup.tracks = new List<CutsceneData.Track>();
                            targetGroup.tracks.AddRange(sourceGroup.tracks);
                            asset.groups.Remove(sourceGroup);
                            m_pLogic.RefreshTimeGraph(asset);
                        }

                    }
                    else
                    {
                        //插入
                        m_pLogic.RegisterUndoData(false);
                        asset.groups.Remove(sourceGroup);
                        int tgtIdx = asset.groups.IndexOf(targetGroup);
                        int insertIdx = bInsertBefore ? tgtIdx : tgtIdx + 1;
                        asset.groups.Insert(insertIdx, sourceGroup);
                        m_pLogic.RefreshTimeGraph(asset);
                    }
                    return;
                }

                // 2.2 目标是Track
                if (target is TimelineTrack)
                {
                    var tgtOwnerGroup = (target as TimelineTrack).ownerGroup?.group;
                    if (tgtOwnerGroup != null)
                    {
                        if(sourceGroup!= tgtOwnerGroup)
                        {
                            m_pLogic.RegisterUndoData(false);
                            asset.groups.Remove(sourceGroup);
                            int tgtIdx = tgtOwnerGroup.tracks.IndexOf(targetTrack);
                            int insertIdx = bInsertBefore ? tgtIdx : tgtIdx + 1;
                            if (sourceGroup.tracks != null)
                            {
                                tgtOwnerGroup.tracks.InsertRange(insertIdx, sourceGroup.tracks);
                                sourceGroup.tracks.Clear();
                            }
                            m_pLogic.RefreshTimeGraph(asset);
                        }
                    }
                    else
                    {
                        m_pLogic.RegisterUndoData(false);
                        asset.groups.Remove(sourceGroup);
                        int tgtIdx = asset.groups.IndexOf(targetGroup);
                        int insertIdx = bInsertBefore ? tgtIdx : tgtIdx + 1;
                        asset.groups.Insert(insertIdx, sourceGroup);
                        m_pLogic.RefreshTimeGraph(asset);
                    }
                    return;
                }
            }
        }
    }
}
#endif