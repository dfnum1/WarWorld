/********************************************************************
生成日期:	06:30:2025
类    名: 	EventDraw
作    者:	HappLI
描    述:	事件绘制
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Runtime;
using UnityEditor;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    public class EventDraw : IDraw
    {

        bool m_ClipViewDirty = true;
        int m_PreviousLayerStateHash = -1;
        public Rect parentViewRect { get; set; }
        public Rect clippedRect { get; private set; }
        Rect m_ClipCenterSection;
        Rect m_MixOutRect;
        Rect m_MixInRect;
        int m_nStarDrag = 0;
        float m_fDragGrabTime = 0;
        float m_fDragOfffsetTime = 0;
        Color m_SwatchColor = Color.white;
        TimelineDrawLogic.TimelineTrack m_pTrack;
        TimelineDrawLogic m_pLogic;
        public TimelineDrawLogic.TimelineTrack ownerTrack { get { return m_pTrack; }}
        public TimelineDrawLogic.TimelineTrack GetOwnerTrack() { return m_pTrack; }
        public bool expandProp { get; set; }
        public IBaseEvent clip { get; set; }
        private ACutsceneCustomEditor m_pEditor = null;
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
        public float mixInPercentage
        {
            get { return (float)(mixInDuration / GetDuration()); }
        }
        //--------------------------------------------------------
        public float mixInDuration
        {
            get { return 0; }
        }
        public float mixOutTime
        {
            get { return GetDuration() - mixOutDuration + GetBegin(); }
        }
        public float mixOutDuration
        {
            get { return 0; }
        }
        public float mixOutPercentage
        {
            get { return (float)(mixOutDuration / GetDuration()); }
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
        //--------------------------------------------------------
        public EventDraw(TimelineDrawLogic pLogic, TimelineDrawLogic.TimelineTrack track, IBaseEvent clip)
        {
            expandProp = true;
            m_pLogic = pLogic;
            m_pTrack = track;
            this.clip = clip;
            m_SwatchColor = track.itemColor();
        }
        //--------------------------------------------------------
        public IDataer GetData() { return this.clip; }
        //--------------------------------------------------------
        public void SetData(IDataer pDater)
        {
            if (pDater == null || !(pDater is IBaseEvent))
                return;
            this.clip = pDater as IBaseEvent;
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
            Vector2 mousePos = evt.mousePosition - stateLogic.ToWindowSpace(treeViewRect.position);
            return m_ClipCenterSection.Contains(mousePos);
        }
        //--------------------------------------------------------
        float Sign(Vector2 point, Vector2 linePoint1, Vector2 linePoint2)
        {
            return (point.x - linePoint2.x) * (linePoint1.y - linePoint2.y) - (linePoint1.x - linePoint2.x) * (point.y - linePoint2.y);
        }
        //--------------------------------------------------------
        public float GetBegin()
        {
            return clip.GetTime() + m_fDragOfffsetTime;
        }
        //--------------------------------------------------------
        public float GetEnd()
        {
            return GetBegin() + GetDuration();
        }    
        //--------------------------------------------------------
        public float GetDuration()
        {
            return 0.1f;
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
        public bool DragOffset(float timeOffset, bool bUsed = true, bool bUndo = true, bool bDuration = true)
        {
            if (this.ownerTrack != null && ownerTrack.groupData != null && ownerTrack.groupData.HasFlag(EGroupFlag.UnActive))
                return false;

            bool bDirty = false;
            if (bUsed) m_fDragOfffsetTime = 0;
            else m_fDragOfffsetTime = timeOffset;

            float time = clip.GetTime() + timeOffset;
            if (clip.GetTime() != time && time>= 0.0f)
            {
                if (bUsed)
                {
                    if (bUndo) m_pLogic.RegisterUndoData();
                    clip.SetTime(time);
                }
                bDirty = true;
            }

            m_fDragGrabTime = 0;
            m_nStarDrag = 0;
            return bDirty;
        }
        //--------------------------------------------------------
        public void DragEnd()
        {
            if (m_nStarDrag == 2)
            {
                float time = clip.GetTime() + m_fDragOfffsetTime;
                if (clip.GetTime() != time)
                {
                    m_pLogic.RegisterUndoData();
                    clip.SetTime(time);
                }
            }
            m_fDragGrabTime = 0;
            m_fDragOfffsetTime = 0;
            m_nStarDrag = 0;
        }
        //--------------------------------------------------------
        public void OnEvent(Event evt, TimelineDrawLogic stateLogic, bool bForceDrag = false)
        {
            if (evt.type == EventType.MouseDrag)
            {
                if(m_nStarDrag== 1)
                {
                    Rect right = new Rect(stateLogic.rightRect.position + stateLogic.GetRect().position, new Vector2(stateLogic.rightRect.width, stateLogic.rightRect.height - 30));
                    if (bForceDrag || (right.Contains(evt.mousePosition) && this.CanSelect(evt, stateLogic) && stateLogic.IsSelected(this)))
                    {
                        m_fDragGrabTime = stateLogic.GetSnappedTimeAtMousePosition(evt.mousePosition);
                        m_nStarDrag = 2;
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
                Rect right = new Rect(stateLogic.rightRect.position + stateLogic.GetRect().position, new Vector2(stateLogic.rightRect.width, stateLogic.rightRect.height - 30));
                if (right.Contains(evt.mousePosition))
                {
                    m_nStarDrag = 1;
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
        public bool CanSelectDurationSnap(Event evt, TimelineDrawLogic stateLogic)
        {
            return false;
        }
        //--------------------------------------------------------
        public void Draw(Rect rect, TimelineDrawLogic statLogic)
        {
            parentViewRect = rect;
            float onePixelTime = statLogic.PixelDeltaToDeltaTime(ConstUtil.kVisibilityBufferInPixels);
            var visibleTime = statLogic.timeAreaShownRange + new Vector2(-onePixelTime, onePixelTime);
            var layerViewStateHasChanged = GetLayerViewStateChanged(rect, statLogic);

            float startTime = GetBegin();
            bool bVisible = startTime < visibleTime.y;
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

            DrawInto(treeViewRect, statLogic);
            ResetClipChanged();
        }
        //--------------------------------------------------------
        void DrawInto(Rect drawRect, TimelineDrawLogic state)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            float clipWidth = treeViewRect.width;
            GUI.BeginClip(new Rect(treeViewRect.position, new Vector2(clipWidth, treeViewRect.height)));

            if (Event.current.type == EventType.Repaint)
            {
                bool selected = state.IsSelected(this);
                // 绘制事件图片
                Texture2D eventIcon = null;
                if (eventIcon == null)
                    eventIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(System.IO.Path.Combine(CutsceneObjectIconEditor.BuildInstallPath(), "EventIcon.png"));
                if (eventIcon != null)
                {
                    float iconW = eventIcon.width;
                    float iconH = eventIcon.height;
                    float maxW = m_ClipCenterSection.width;
                    float maxH = m_ClipCenterSection.height;
                    float scale = 1;// Mathf.Min(maxW / iconW, maxH / iconH, 1f);
                    float drawW = iconW * 0.5f;
                    float drawH = iconH ;
                    Rect iconRect = new Rect(
                        m_ClipCenterSection.x + (maxW - drawW) * 0.5f,
                        m_ClipCenterSection.y + (maxH - drawH) * 0.5f,
                        drawW, drawH
                    );
                    GUI.DrawTexture(iconRect, eventIcon, ScaleMode.ScaleToFit, true);
                }
                else
                    EditorGUI.DrawRect(m_ClipCenterSection, Color.red);
                //GUI.DrawTexture(new Rect(m_ClipCenterSection.x, m_ClipCenterSection.y, 20, m_ClipCenterSection.height), EditorGUIUtility.TrIconContent("Animation.AddEvent").image);
                if (selected)
                {
                    var selectionBorder = ClipBorder.Selection();
                    ClipDrawUtil.DrawClipSelectionBorder(m_ClipCenterSection, selectionBorder, ClipBlends.kNone);
                }
            }

            GUI.EndClip();
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
            m_ClipCenterSection.y = treeViewRect.height/2;

            m_ClipCenterSection.xMin = mixInRect.xMax;
            m_ClipCenterSection.width = Mathf.Round(treeViewRect.width - mixInRect.width - mixOutRect.width);
            m_ClipCenterSection.height = treeViewRect.height / 2;
            m_ClipCenterSection.xMax = m_ClipCenterSection.xMin + m_ClipCenterSection.width;
        }
        //--------------------------------------------------------
        public virtual Rect RectToTimeline(Rect trackRect, TimelineDrawLogic state)
        {
            var offsetFromTimeSpaceToPixelSpace = state.timeAreaTranslation.x + trackRect.xMin;

            var start = this.GetBegin()-GetDuration()/2;
            var end = this.GetBegin() + GetDuration()/2;

            return Rect.MinMaxRect(
                Mathf.Round(start * state.timeAreaScale.x + offsetFromTimeSpaceToPixelSpace), Mathf.Round(trackRect.yMin),
                Mathf.Round(end * state.timeAreaScale.x + offsetFromTimeSpaceToPixelSpace), Mathf.Round(trackRect.yMax)
            );
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
        public ACutsceneCustomEditor GetCustomEditor()
        {
            if(m_pEditor == null) m_pEditor = DataUtils.CreateCustomEvent(clip.GetType());
            if (m_pEditor != null) m_pEditor.SetDraw(this);
            return m_pEditor;
        }
        //--------------------------------------------------------
        public void OnDelete()
        {
            if (m_pEditor != null) m_pEditor.OnDisable();
        }
    }
}
#endif