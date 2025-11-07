/********************************************************************
生成日期:	06:30:2025
类    名: 	TimeCursorDraw
作    者:	HappLI
描    述:	事件时间游标绘制类
*********************************************************************/
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace Framework.Cutscene.Editor
{
    internal class TimeCursorDraw
    {
        public Color headColor { get; set; }
        public Color lineColor { get; set; }
        public bool drawLine { get; set; }
        public bool drawHead { get; set; }
        public bool canMoveHead { get; set; }
        public string tooltip { get; set; }
        public Vector2 boundOffset { get; set; }

        readonly GUIContent m_HeaderContent = new GUIContent();
        GUIStyle m_Style;

        Rect m_BoundingRect;
        TimelineDrawLogic m_Logic;

        bool m_bCanDrag = true;
        bool m_bDragged = false;
        public bool canDrag { get { return m_bCanDrag; } set { m_bCanDrag = value; } }
        public bool isDraged { get { return m_bDragged; } }
        float widgetHeight { get { return style.fixedHeight; } }
        float widgetWidth { get { return style.fixedWidth; } }

        public Rect bounds
        {
            get
            {
                Rect r = m_BoundingRect;
                r.y = m_Logic.timelineRect.yMin;
                r.position += boundOffset;

                return r;
            }
        }

        public GUIStyle style
        {
            get 
            {
                if (m_Style == null)
                {
                    m_Style = TextUtil.timeCursor;
                    if(m_Style!=null) lineColor = m_Style.normal.textColor;
                }
                return m_Style; 
            }
        }


        public bool showTooltip { get; set; }

        // is this the first frame the drag callback is being invoked
        public bool firstDrag { get; private set; }

        public TimeCursorDraw(TimelineDrawLogic logic)
        {
            m_Logic = logic;
            m_Style = null;
            headColor = Color.white;
            if(m_Style!=null)
                lineColor = m_Style.normal.textColor;
            drawLine = true;
            drawHead = true;
            canMoveHead = false;
            tooltip = string.Empty;
            boundOffset = Vector2.zero;
        }
        //--------------------------------------------------------
        public void Draw(Rect rect, double time)
        {
            using (new GUIViewportScope(rect))
            {
                Vector2 windowCoordinate = rect.min;
                windowCoordinate.y += 4.0f;

                windowCoordinate.x = m_Logic.TimeToPixel(time);

                m_BoundingRect = new Rect((windowCoordinate.x - widgetWidth / 2.0f), windowCoordinate.y, widgetWidth, widgetHeight);

                // Do not paint if the time cursor goes outside the timeline bounds...
                if (Event.current.type == EventType.Repaint)
                {
                    if (m_BoundingRect.xMax < m_Logic.timeZoomRect.xMin)
                        return;
                    if (m_BoundingRect.xMin > m_Logic.timeZoomRect.xMax)
                        return;
                }

                var top = new Vector3(windowCoordinate.x, rect.y +m_Logic.timelineRect.yMin);
                var bottom = new Vector3(windowCoordinate.x, rect.yMax-11);

                if (drawLine)
                {
                    Rect lineRect = Rect.MinMaxRect(top.x - 0.5f, top.y, bottom.x + 0.5f, bottom.y);
                    EditorGUI.DrawRect(lineRect, lineColor);
                }

                if (drawHead && Event.current.type == EventType.Repaint)
                {
                    Color c = GUI.color;
                    GUI.color = headColor;
                    style.Draw(bounds, m_HeaderContent, false, false, false, false);
                    GUI.color = c;

                    if (canMoveHead)
                        EditorGUIUtility.AddCursorRect(bounds, MouseCursor.MoveArrow);
                }
            }
        }
        //--------------------------------------------------------
        public void OnEvent(Event evt)
        {
            if (!m_bCanDrag)
                return;
            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                Vector2 mousePos = evt.mousePosition - m_Logic.timelineRect.position - m_Logic.GetRect().position;
                Rect rect = new Rect(m_Logic.rightRect.position, new Vector2(m_Logic.timelineRect.width, TimelineDrawLogic.TimeRulerAreaHeight));
                if (rect.Contains(mousePos))
                {
                    m_Logic.SetPlaying(false);
                    mousePos -= m_Logic.rightRect.position;
                    m_Logic.SetCurrentTime((float)Math.Max(0.0, m_Logic.GetSnappedTimeAtMousePosition(mousePos)));

                    m_bDragged = true;
                    evt.Use();
                }
            }
            else if (evt.type == EventType.MouseDrag)
            {
                if (m_bDragged)
                {
                    Vector2 mousePos = evt.mousePosition - m_Logic.timelineRect.position - m_Logic.GetRect().position;
                    mousePos -= m_Logic.rightRect.position;
                    m_Logic.SetPlaying(false);
                    m_Logic.SetCurrentTime((float)Math.Max(0.0, m_Logic.GetSnappedTimeAtMousePosition(mousePos)));
                    evt.Use();
                }
            }
            else if (evt.type == EventType.MouseUp)
            {
                if (m_bDragged) evt.Use();
                m_bDragged = false;
            }
        }
    }
}
#endif