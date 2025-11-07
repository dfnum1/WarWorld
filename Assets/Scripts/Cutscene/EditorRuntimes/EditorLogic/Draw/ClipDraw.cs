/********************************************************************
生成日期:	06:30:2025
类    名: 	ClipDraw
作    者:	HappLI
描    述:	剪辑绘制
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    public class ClipDraw : IDraw
    {

        bool m_ClipViewDirty = true;
        int m_PreviousLayerStateHash = -1;
        public Rect parentViewRect { get; set; }
        public Rect clippedRect { get; private set; }
        Rect m_ClipCenterSection;
        readonly List<Rect> m_LoopRects = new List<Rect>();
        Rect m_MixOutRect;
        Rect m_MixInRect;

        ClipCaps m_ClipCap = ClipCaps.All;
        public ClipCaps clipCaps
        {
            get
            {
                return m_ClipCap;
            }
        }
        int m_MinLoopIndex = -1;
        int m_nStarDrag = 0;
        bool m_bDragDuration = false;
        float m_fDragGrabTime = 0;
        float m_fDragOfffsetTime = 0;
        ClipDrawData m_ClipDrawData;
        Color m_SwatchColor = Color.white;
        TimelineDrawLogic.TimelineTrack m_pTrack;
        List<Texture2D> m_vIcons;
        TimelineDrawLogic m_pLogic;
        public TimelineDrawLogic.TimelineTrack ownerTrack { get { return m_pTrack; }}
        public TimelineDrawLogic.TimelineTrack GetOwnerTrack() { return m_pTrack; }

        private ACutsceneCustomEditor m_pEditor = null;
        public bool expandProp { get; set; }
        public IBaseClip clip { get; set; }
        //--------------------------------------------------------
        Rect m_TreeViewRect;
        public Rect treeViewRect
        {
            get { return m_TreeViewRect; }
            protected set
            {
                m_TreeViewRect = value;
                if (value.width < 0.0f)
                    m_TreeViewRect.width = 1.0f;
            }
        }
        //--------------------------------------------------------
        public Rect mixOutRect
        {
            get
            {
                var percent = mixOutPercentage;
                var x = Mathf.Round(treeViewRect.width * (1 - percent));
                var width = Mathf.Round(treeViewRect.width * percent);
                m_MixOutRect.Set(x, 0.0f, width, treeViewRect.height);
                return m_MixOutRect;
            }
        }
        //--------------------------------------------------------
        public Rect mixInRect
        {
            get
            {
                var width = Mathf.Round(treeViewRect.width * mixInPercentage);
                m_MixInRect.Set(0.0f, 0.0f, width, treeViewRect.height);
                return m_MixInRect;
            }
        }
        public bool hasBlendIn { get { return clipCaps.HasAny(ClipCaps.Blending) && clip.GetBlendDuration( ECutsceneClipBlendType.In) > 0; } }

        public bool hasBlendOut { get { return clipCaps.HasAny(ClipCaps.Blending) && clip.GetBlendDuration( ECutsceneClipBlendType.Out) > 0; } }

        //--------------------------------------------------------
        public ClipDraw(TimelineDrawLogic pLogic, TimelineDrawLogic.TimelineTrack track, IBaseClip clip)
        {
            expandProp = true;
            m_pLogic = pLogic;
            m_pTrack = track;
            this.clip = clip;
            m_ClipCap = ClipCaps.All;
            if(!clip.CanBlend()) m_ClipCap &= ~(ClipCaps.Blending);
            m_SwatchColor = track.itemColor();
        }
        //--------------------------------------------------------
        public IDataer GetData() { return this.clip; }
        //--------------------------------------------------------
        public void SetData(IDataer pDater)
        {
            if (pDater == null || !(pDater is IBaseClip))
                return;
            this.clip = pDater as IBaseClip;
        }
        //--------------------------------------------------------
        public bool CanEdit()
        {
            if (ownerTrack == null || ownerTrack.groupData == null) return false;
            return !ownerTrack.groupData.HasFlag(EGroupFlag.UnActive);
        }
        //--------------------------------------------------------
        public void RegisterUndo(bool bDirtyData=true)
        {
            m_pLogic.RegisterUndoData(bDirtyData);
        }
        //--------------------------------------------------------
        public bool IsDragging()
        {
            return m_nStarDrag==2;
        }
        //--------------------------------------------------------
        public bool CanSelect(Event evt, TimelineDrawLogic stateLogic)
        {
            ClipBlends clipBlends = GetClipBlends();
            Vector2 mousePos = evt.mousePosition - stateLogic.ToWindowSpace(treeViewRect.position);
            return m_ClipCenterSection.Contains(mousePos) || IsPointLocatedInClipBlend(mousePos, clipBlends);
        }
        //--------------------------------------------------------
        public bool CanSelectDurationSnap(Event evt, TimelineDrawLogic stateLogic)
        {
            ClipBlends clipBlends = GetClipBlends();
            Vector2 mousePos = evt.mousePosition - stateLogic.ToWindowSpace(treeViewRect.position);
            float durSnap = Mathf.Min(10, Mathf.Max(m_ClipCenterSection.width * 0.1f, 3));
            Rect right = new Rect(m_ClipCenterSection.position + Vector2.right * (m_ClipCenterSection.width - durSnap), new Vector2(durSnap, m_ClipCenterSection.height));
            return right.Contains(mousePos);
        }
        //--------------------------------------------------------
        bool IsPointLocatedInClipBlend(Vector2 pt, ClipBlends blends)
        {
            if (blends.inRect.Contains(pt))
            {
                if (blends.inKind == BlendKind.Mix)
                    return Sign(pt, blends.inRect.min, blends.inRect.max) < 0;
                return true;
            }

            if (blends.outRect.Contains(pt))
            {
                if (blends.outKind == BlendKind.Mix)
                    return Sign(pt, blends.outRect.min, blends.outRect.max) >= 0;
                return true;
            }

            return false;
        }
        //--------------------------------------------------------
        float Sign(Vector2 point, Vector2 linePoint1, Vector2 linePoint2)
        {
            return (point.x - linePoint2.x) * (linePoint1.y - linePoint2.y) - (linePoint1.x - linePoint2.x) * (point.y - linePoint2.y);
        }
        //--------------------------------------------------------
        public float mixInPercentage
        {
            get { return (float)(mixInDuration / GetDuration()); }
        }
        //--------------------------------------------------------
        public float mixInDuration
        {
            get { return hasBlendIn ? GetBlendIn() : 0; }
        }
        public float mixOutTime
        {
            get { return GetDuration() - mixOutDuration + GetBegin(); }
        }
        public float mixOutDuration
        {
            get { return hasBlendOut ? GetBlendOut() : 0; }
        }
        public float mixOutPercentage
        {
            get { return (float)(mixOutDuration / GetDuration()); }
        }
        //--------------------------------------------------------
        public ClipBlends GetClipBlends()
        {
            var _mixInRect = mixInRect;
            var _mixOutRect = mixOutRect;

            var blendInKind = BlendKind.None;
            if (_mixInRect.width > ConstUtil.k_MinMixWidth && hasBlendIn)
                blendInKind = BlendKind.Mix;
            else if (_mixInRect.width > ConstUtil.k_MinMixWidth)
                blendInKind = BlendKind.Ease;

            var blendOutKind = BlendKind.None;
            if (_mixOutRect.width > ConstUtil.k_MinMixWidth && hasBlendOut)
                blendOutKind = BlendKind.Mix;
            else if (_mixOutRect.width > ConstUtil.k_MinMixWidth)
                blendOutKind = BlendKind.Ease;

            return new ClipBlends(blendInKind, _mixInRect, blendOutKind, _mixOutRect);
        }
        //--------------------------------------------------------
        public float GetBegin()
        {
            if (m_bDragDuration)
                return clip.GetTime();
            return clip.GetTime() + m_fDragOfffsetTime;
        }
        //--------------------------------------------------------
        public void SetBegin(float begin)
        {
            DragEnd();
            if (clip.GetTime() == begin)
                return;
            m_pLogic.RegisterUndoData();
            clip.SetTime(begin);
        }
        //--------------------------------------------------------
        public float GetEnd()
        {
            return clip.GetTime() + clip.GetDuration() + m_fDragOfffsetTime;
        }
        //--------------------------------------------------------
        public void SetEnd(float end)
        {
            DragEnd();
            if (clip.GetDuration() == (end - clip.GetTime()))
                return;
            m_pLogic.RegisterUndoData();
            clip.SetDuration(end-clip.GetTime());
        }
        //--------------------------------------------------------
        public float GetBlendIn()
        {
            return clip.GetBlendDuration( ECutsceneClipBlendType.In);
        }
        //--------------------------------------------------------
        public float GetBlendOut()
        {
            return clip.GetBlendDuration( ECutsceneClipBlendType.Out);
        }
        //--------------------------------------------------------
        public float GetDuration()
        {
            return GetEnd() - GetBegin();
        }
        //--------------------------------------------------------
        public int GetLoop()
        {
            return clip.GetLoopCnt();
        }
        //--------------------------------------------------------
        public EClipEdgeType GetEdgeType()
        {
            return clip.GetEndEdgeType();
        }
        //--------------------------------------------------------
        public float GetTimeScale()
        {
            return 1.0f;
        }
        //--------------------------------------------------------
        public bool DragOffset(float timeOffset, bool bUsed, bool bUndo = true, bool bDuration = true)
        {
            if (this.ownerTrack != null && ownerTrack.groupData!=null && ownerTrack.groupData.HasFlag(EGroupFlag.UnActive))
                return false;

            bool bDirty = false;
            m_fDragGrabTime = 0;
            m_nStarDrag = 0;
            m_bDragDuration = false;

            if (bUsed) m_fDragOfffsetTime = 0;
            else
            {
                m_bDragDuration = bDuration;
                m_fDragOfffsetTime = timeOffset;
            }
            if (bDuration)
            {
                float time = clip.GetDuration() + timeOffset;
                if (clip.GetDuration() != time)
                {
                    bDirty = true;
                    if (bUsed)
                    {
                        if (bUndo) m_pLogic.RegisterUndoData();
                        clip.SetDuration(time);
                    }
                }
            }
            else
            {
                float time = clip.GetTime() + timeOffset;
                if (clip.GetTime() != time && (time+clip.GetDuration())>1.0f)
                {
                    if (bUsed)
                    {
                        if (bUndo) m_pLogic.RegisterUndoData();
                        clip.SetTime(time);
                    }

                    bDirty = true;
                }
            }
            return bDirty;
        }
        //--------------------------------------------------------
        public void DragEnd()
        {
            if (m_nStarDrag==2)
            {
                if(m_bDragDuration)
                {
                    float time = clip.GetDuration() + m_fDragOfffsetTime;
                    if (clip.GetDuration() != time)
                    {
                        m_pLogic.RegisterUndoData();
                        clip.SetDuration(time);
                    }
                }
                else
                {
                    float time = clip.GetTime() + m_fDragOfffsetTime;
                    if (clip.GetTime() != time)
                    {
                        m_pLogic.RegisterUndoData();
                        clip.SetTime(time);
                    }
                }

            }
            m_fDragGrabTime = 0;
            m_fDragOfffsetTime = 0;
            m_nStarDrag = 0;
            m_bDragDuration = false;
        }
        //--------------------------------------------------------
        public void OnEvent(Event evt, TimelineDrawLogic stateLogic, bool bForceDrag = false)
        {
            if (evt.type == EventType.MouseDrag)
            {
                if(m_nStarDrag== 1)
                {
                    Rect right = new Rect(stateLogic.rightRect.position + stateLogic.GetRect().position, new Vector2(stateLogic.rightRect.width, stateLogic.rightRect.height-30));
                    if(bForceDrag ||(right.Contains(evt.mousePosition) && this.CanSelect(evt, stateLogic) && stateLogic.IsSelected(this)))
                    {
                        m_fDragGrabTime = stateLogic.GetSnappedTimeAtMousePosition(evt.mousePosition);
                        m_nStarDrag = 2;
                        m_bDragDuration = CanSelectDurationSnap(evt, stateLogic);
                    }
                }
                if(m_nStarDrag == 2)
                {
                    float grab = stateLogic.GetSnappedTimeAtMousePosition(evt.mousePosition);
                    m_fDragOfffsetTime = grab - m_fDragGrabTime;
                }
            }
            else if (evt.type == EventType.MouseDown)
            {
                m_fDragGrabTime = 0;
                m_fDragOfffsetTime = 0;
                m_nStarDrag = 0;
                m_bDragDuration = false;
                Rect right = new Rect(stateLogic.rightRect.position + stateLogic.GetRect().position, new Vector2(stateLogic.rightRect.width, stateLogic.rightRect.height - 30));
                if (right.Contains(evt.mousePosition))
                {
                    m_nStarDrag = 1;
                    m_bDragDuration = CanSelectDurationSnap(evt, stateLogic);
                }
            }
            else if(evt.type == EventType.MouseUp)
            {
                if (m_nStarDrag == 2)
                    stateLogic.RegisterUndoData(stateLogic.GetAsset());
                DragEnd();
            }
        }
        //--------------------------------------------------------
        public void Draw(Rect rect, TimelineDrawLogic statLogic)
        {
            parentViewRect = rect;
            float onePixelTime = statLogic.PixelDeltaToDeltaTime(ConstUtil.kVisibilityBufferInPixels);
            var visibleTime = statLogic.timeAreaShownRange + new Vector2(-onePixelTime, onePixelTime);
            var layerViewStateHasChanged = GetLayerViewStateChanged(rect, statLogic);

            float startTime = GetBegin();
            float endTime = GetEnd();
            bool bVisible = endTime > visibleTime.x && startTime < visibleTime.y;
            if (!bVisible)
                return;
            OnDraw(rect, layerViewStateHasChanged, statLogic);
        }
        //--------------------------------------------------------
        protected virtual void OnDraw(Rect trackRect, bool tracRectChanged, TimelineDrawLogic statLogic)
        {
            DetectClipChanged(tracRectChanged);
            CalculateClipRectangle(trackRect, statLogic);

            CalculateBlendRect();
            CalculateLoopRects(trackRect, statLogic);

            DrawInto(treeViewRect, statLogic);
            ResetClipChanged();
        }
        //--------------------------------------------------------
        void DrawInto(Rect drawRect, TimelineDrawLogic state)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            float clipWidth = treeViewRect.width;
            if (GetLoop() > 1) clipWidth = treeViewRect.width * GetLoop();
            GUI.BeginClip(new Rect(treeViewRect.position, new Vector2(clipWidth, treeViewRect.height)));

            bool selected = state.IsSelected(this);
            var originRect = new Rect(0.0f, 0.0f, drawRect.width, drawRect.height);

            bool previousClipSelected = false;
            string clipTile = this.clip.GetName();
            UpdateDrawData(state, originRect, clipTile, selected, previousClipSelected, drawRect.x);
            ClipDrawUtil.DrawClip(m_ClipDrawData);

            GUI.EndClip();

            //if (clip.GetParentTrack() != null && !clip.GetParentTrack().lockedInHierarchy)
            //{
            //    if (selected && supportResize)
            //    {
            //        var cursorRect = rect;
            //        cursorRect.xMin += leftHandle.boundingRect.width;
            //        cursorRect.xMax -= rightHandle.boundingRect.width;
            //        EditorGUIUtility.AddCursorRect(cursorRect, MouseCursor.MoveArrow);
            //    }

            //    if (supportResize)
            //    {
            //        var handleWidth = Mathf.Clamp(drawRect.width * 0.3f, k_MinHandleWidth, k_MaxHandleWidth);

            //        leftHandle.Draw(drawRect, handleWidth, state);
            //        rightHandle.Draw(drawRect, handleWidth, state);
            //    }
            //}
        }
        //--------------------------------------------------------
        void UpdateDrawData(TimelineDrawLogic state, Rect drawRect, string title, bool selected, bool previousClipSelected, float rectXOffset)
        {
            m_ClipDrawData.clip = clip;
            m_ClipDrawData.targetRect = drawRect;
            m_ClipDrawData.clipCenterSection = m_ClipCenterSection;
            m_ClipDrawData.unclippedRect = treeViewRect;
            m_ClipDrawData.title = title;
            var clipAttr = DataUtils.GetClipAttri(clip.GetIdType());
            if(clipAttr!=null) m_ClipDrawData.tips = clipAttr.pAttri.tips;
            m_ClipDrawData.selected = selected;
           // m_ClipDrawData.inlineCurvesSelected = inlineCurvesSelected;
           // m_ClipDrawData.previousClip = previousClip != null ? previousClip.clip : null;
            m_ClipDrawData.previousClipSelected = previousClipSelected;

            Vector2 shownAreaTime = state.timeAreaShownRange;
            m_ClipDrawData.localVisibleStartTime = clip.ToLocalTimeUnbound(Math.Max(GetBegin(), shownAreaTime.x));
            m_ClipDrawData.localVisibleEndTime = clip.ToLocalTimeUnbound(Math.Min(GetEnd(), shownAreaTime.y));

            m_ClipDrawData.clippedRect = new Rect(clippedRect.x - rectXOffset, 0.0f, clippedRect.width, clippedRect.height);

            m_ClipDrawData.swatchColor = m_SwatchColor;

            m_ClipDrawData.minLoopIndex = m_MinLoopIndex;
            m_ClipDrawData.loopRects = m_LoopRects;
            m_ClipDrawData.supportsLooping = GetLoop() == 0 || GetLoop() > 1;
            m_ClipDrawData.clipBlends = GetClipBlends();
            // m_ClipDrawData.clipEditor = m_ClipEditor;
            // m_ClipDrawData.ClipDrawOptions = UpdateClipDrawOptions(m_ClipEditor, clip);

            m_ClipDrawData.overlapWithClip = IsOverlapWithClip();
            UpdateClipIcons(state);
        }
        //--------------------------------------------------------
        bool IsOverlapWithClip()
        {
            if (ownerTrack == null || ownerTrack.clipDraws == null || !m_pLogic.IsSelected(this)) return false;
            for(int i =0; i < ownerTrack.clipDraws.Count; ++i)
            {
                if (ownerTrack.clipDraws[i] == this) continue;
                if (ClipDrawUtil.Overlaps(this, ownerTrack.clipDraws[i]) && !ownerTrack.clipDraws[i].SupportsBlending())
                {
                    return true;
                }
            }
            return false;
        }
        //--------------------------------------------------------
        public void OnDelete()
        {
            // 1. 如果有 blendIn，处理交集 clip 的 blendOut
            if (hasBlendIn)
            {
                foreach (var other in ownerTrack.clipDraws)
                {
                    if (other == this) continue;
                    if (ClipDrawUtil.Overlaps(this, other))
                    {
                        // 设置交集clip的blendOut为0
                        if (other.clip.GetBlendDuration(ECutsceneClipBlendType.Out) > 0)
                        {
                            other.clip.SetBlendDuration(ECutsceneClipBlendType.Out, 0);
                        }
                    }
                }
            }

            // 2. 如果有 blendOut，处理交集 clip 的 blendIn
            if (hasBlendOut)
            {
                foreach (var other in ownerTrack.clipDraws)
                {
                    if (other == this) continue;
                    if (ClipDrawUtil.Overlaps(this, other))
                    {
                        // 设置交集clip的blendIn为0
                        if (other.clip.GetBlendDuration(ECutsceneClipBlendType.In) > 0)
                        {
                            other.clip.SetBlendDuration(ECutsceneClipBlendType.In, 0);
                        }
                    }
                }
            }
            if (m_pEditor != null) m_pEditor.OnDisable();
        }
        //--------------------------------------------------------
        void UpdateClipIcons(TimelineDrawLogic state)
        {
            //// Pass 1 - gather size
            bool requiresDigIn = false;
            int required = 0;
            if (m_vIcons == null) m_vIcons = new List<Texture2D>();
            m_vIcons.Clear();

            if(GetEdgeType() == EClipEdgeType.KeepClamp)
            {
                m_vIcons.Add(TextUtil.LoadIcon("StepButton On"));
            }
            else if (GetEdgeType() == EClipEdgeType.KeepState)
            {
                m_vIcons.Add(TextUtil.LoadIcon("SaveFromPlay"));
            }
            else if( GetEdgeType() == EClipEdgeType.Repeat)
            {
                if (GetLoop() == 0 || GetLoop()>1)
                {
                    //! loop
                    m_vIcons.Add(TextUtil.LoadIcon("d_playLoopOn"));
                }
            }
            if (m_vIcons != null)
            {
                foreach (var icon in m_vIcons)
                {
                    if (icon != null)
                        required++;
                }
            }



            // Pass 2 - copy icon data
            if (required == 0)
            {
                m_ClipDrawData.rightIcons = null;
                return;
            }

            if (m_ClipDrawData.rightIcons == null || m_ClipDrawData.rightIcons.Length != required)
                m_ClipDrawData.rightIcons = new IconData[required];

            int index = 0;
            if (requiresDigIn)
                m_ClipDrawData.rightIcons[index++] = ClipDrawUtil.k_DiggableClipIcon;

            foreach (var icon in m_vIcons)
            {
                if (icon != null)
                    m_ClipDrawData.rightIcons[index++] = new IconData(icon);
            }
        }
        //--------------------------------------------------------
        void DetectClipChanged(bool tracRectChanged)
        {
            if (Event.current.type != EventType.Layout)
                return;
            m_ClipViewDirty = true;
          //  OnClipChanged();
        }
        //--------------------------------------------------------
        void ResetClipChanged()
        {
            if (Event.current.type == EventType.Repaint)
                m_ClipViewDirty = false;
        }
        //--------------------------------------------------------
        void CalculateClipRectangle(Rect trackRect, TimelineDrawLogic stateLogic)
        {
            if (!m_ClipViewDirty) return;
            var clipRect = RectToTimeline(trackRect, stateLogic);
            treeViewRect = clipRect;

            // calculate clipped rect
            clipRect.xMin = Mathf.Max(clipRect.xMin, trackRect.xMin);
            clipRect.xMax = Mathf.Min(clipRect.xMax, trackRect.xMax);
            if (clipRect.width > 0 && clipRect.width < 2)
            {
                clipRect.width = 5.0f;
            }

            clippedRect = clipRect;
        }
        //--------------------------------------------------------
        void CalculateBlendRect()
        {
            m_ClipCenterSection = treeViewRect;
            m_ClipCenterSection.x = 0;
            m_ClipCenterSection.y = 0;

            m_ClipCenterSection.xMin = mixInRect.xMax;
            m_ClipCenterSection.width = Mathf.Round(treeViewRect.width - mixInRect.width - mixOutRect.width);
            m_ClipCenterSection.xMax = m_ClipCenterSection.xMin + m_ClipCenterSection.width;
        }
        //--------------------------------------------------------
        void CalculateLoopRects(Rect trackRect, TimelineDrawLogic state)
        {
            if (!m_ClipViewDirty)
                return;

            m_LoopRects.Clear();
            if (GetLoop() == 1)
                return;
            var totalDuration = GetDuration() * GetLoop();
            if (totalDuration < ConstUtil.kTimeEpsilon)
                return;
            Vector2 pos = Vector2.zero;
            m_MinLoopIndex = 1;
            for (int i = 1; i < GetLoop(); i++)
            {
                pos += Vector2.right * treeViewRect.width;
                Vector2 size = treeViewRect.size;
                m_LoopRects.Add(new Rect(pos, size));
            }
        }
        //--------------------------------------------------------
        public virtual Rect RectToTimeline(Rect trackRect, TimelineDrawLogic state)
        {
            var offsetFromTimeSpaceToPixelSpace = state.timeAreaTranslation.x + trackRect.xMin;

            var start = this.GetBegin();
            var end = GetEnd();

            return Rect.MinMaxRect(
                Mathf.Round(start * state.timeAreaScale.x + offsetFromTimeSpaceToPixelSpace), Mathf.Round(trackRect.yMin),
                Mathf.Round(end * state.timeAreaScale.x + offsetFromTimeSpaceToPixelSpace), Mathf.Round(trackRect.yMax)
            );
        }
        //--------------------------------------------------------
        static Rect ProjectRectOnTimeline(Rect rect, Rect trackRect, TimelineDrawLogic state)
        {
            Rect newRect = rect;
            // transform clipRect into pixel-space
            newRect.x *= state.timeAreaScale.x;
            newRect.width *= state.timeAreaScale.x;

            newRect.x += state.timeAreaTranslation.x + trackRect.xMin;

            // adjust clipRect height and vertical centering
            const int clipPadding = 2;
            newRect.y = trackRect.y + clipPadding;
            newRect.height = trackRect.height - (2 * clipPadding);
            return newRect;
        }
        //--------------------------------------------------------
        bool GetLayerViewStateChanged(Rect rect, TimelineDrawLogic state)
        {
            var layerStateHash = rect.GetHashCode().CombineHash(state.viewStateHash);
            var layerViewStateHasChanged = layerStateHash != m_PreviousLayerStateHash;

            if (Event.current.type == EventType.Layout && layerViewStateHasChanged)
                m_PreviousLayerStateHash = layerStateHash;

            return layerViewStateHasChanged;
        }
        //--------------------------------------------------------
        public bool SupportsLooping()
        {
            return  (clipCaps & ClipCaps.Looping) != ClipCaps.None;
        }
        //--------------------------------------------------------
        public bool SupportsExtrapolation()
        {
            return (clipCaps & ClipCaps.Extrapolation) != ClipCaps.None;
        }
        //--------------------------------------------------------
        public bool SupportsClipIn()
        {
            return (clipCaps & ClipCaps.ClipIn) != ClipCaps.None;
        }
        //--------------------------------------------------------
        public bool SupportsSpeedMultiplier()
        {
            return (clipCaps & ClipCaps.SpeedMultiplier) != ClipCaps.None;
        }
        //--------------------------------------------------------
        public bool SupportsBlending()
        {
            return (clipCaps & ClipCaps.Blending) != ClipCaps.None;
        }
        //--------------------------------------------------------
        public bool HasAll(ClipCaps flags)
        {
            return (clipCaps & flags) == flags;
        }
        //--------------------------------------------------------
        public ACutsceneCustomEditor GetCustomEditor()
        {
            if (m_pEditor == null) m_pEditor = DataUtils.CreateCustomEvent(clip.GetType());
            if (m_pEditor != null) m_pEditor.SetDraw(this);
            return m_pEditor;
        }
    }
}
#endif