#if UNITY_EDITOR


using Framework.ED;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    public static class EditorUtil
    {
        public static bool HasAny(this ClipCaps caps, ClipCaps flags)
        {
            return (caps & flags) != 0;
        }
        public static int CombineHash(this int h1, int h2)
        {
            return h1 ^ (int)(h2 + 0x9e3779b9 + (h1 << 6) + (h1 >> 2)); // Similar to c++ boost::hash_combine
        }
        public static bool DrawButton(Texture image, string tooltip, GUIStyle style, float width =0)
        {
            if(width<=0)
                return GUILayout.Button(new GUIContent(image, tooltip),  style);
            else
                return GUILayout.Button(new GUIContent(image, tooltip),style, GUILayout.Width(width));
        }
        public static bool DrawToggle(bool value, Texture image, string tooltip, GUIStyle style, float width = 0)
        {
            if (width <= 0)
                return GUILayout.Toggle(value, new GUIContent(image, tooltip), style);
            else
                return GUILayout.Toggle(value, new GUIContent(image, tooltip), style, GUILayout.Width(width));
        }
        public static void DrawPolygonAA(Color color, Vector3[] vertices)
        {
            var prevColor = Handles.color;
            Handles.color = color;
            Handles.DrawAAConvexPolygon(vertices);
            Handles.color = prevColor;
        }
        public static void DrawLine(Vector3 p1, Vector3 p2, Color color)
        {
            var c = Handles.color;
            Handles.color = color;
            Handles.DrawLine(p1, p2);
            Handles.color = c;
        }
        public static void DrawDottedLine(Vector3 p1, Vector3 p2, float segmentsLength, Color col)
        {
            UIDrawUtils.ApplyWireMaterial();

            GL.Begin(GL.LINES);
            GL.Color(col);

            var length = Vector3.Distance(p1, p2); // ignore z component
            var count = Mathf.CeilToInt(length / segmentsLength);
            for (var i = 0; i < count; i += 2)
            {
                GL.Vertex((Vector3.Lerp(p1, p2, i * segmentsLength / length)));
                GL.Vertex((Vector3.Lerp(p1, p2, (i + 1) * segmentsLength / length)));
            }

            GL.End();
        }
        public static void ShadowLabel(Rect rect, GUIContent content, GUIStyle style, Color textColor, Color shadowColor)
        {
            var shadowRect = rect;
            shadowRect.xMin += 2.0f;
            shadowRect.yMin += 2.0f;
            style.normal.textColor = shadowColor;
            style.hover.textColor = shadowColor;
            GUI.Label(shadowRect, content, style);

            style.normal.textColor = textColor;
            style.hover.textColor = textColor;
            GUI.Label(rect, content, style);
        }
        //-----------------------------------------------------
        public static void AddAllChildPaths(Transform parent, string parentPath, List<string> pathList)
        {
            foreach (Transform child in parent)
            {
                string path = string.IsNullOrEmpty(parentPath) ? child.name : parentPath + "/" + child.name;
                pathList.Add(path);
                AddAllChildPaths(child, path, pathList);
            }
        }
    }
    struct GUIViewportScope : IDisposable
    {
        bool m_open;
        public GUIViewportScope(Rect position)
        {
            m_open = false;
            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
            {
                GUI.BeginClip(position, -position.min, Vector2.zero, false);
                m_open = true;
            }
        }

        public void Dispose()
        {
            CloseScope();
        }

        void CloseScope()
        {
            if (m_open)
            {
                GUI.EndClip();
                m_open = false;
            }
        }
    }
}
#endif