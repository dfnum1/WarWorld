/********************************************************************
生成日期:	06:30:2025
类    名: 	IconUtils
作    者:	HappLI
描    述:	
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Editor;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    public struct IconData
    {
        public enum Side { Left = -1, Right = 1 }

        public Texture2D icon;
        public Color tint;

        public float width { get { return 16; } }
        public float height { get { return 16; } }

        public IconData(Texture2D icon)
        {
            this.icon = icon;
            tint = Color.white;
        }
    }
    public class IconUtils
    {
        public static readonly IconData[] k_ClipErrorIcons = { new IconData { icon = iconWarn, tint = TextUtil.kClipErrorColor } };
        public static Texture2D m_iconWarn = null;
        public static Texture2D iconWarn
        {
            get
            {
                if (m_iconWarn == null)
                {
                    m_iconWarn = EditorGUIUtility.LoadRequired("console.warnicon.inactive.sml") as Texture2D;
                }
                return m_iconWarn;
            }
        }
        private static Texture2D LoadIcon(string icon)
        {
            Texture2D tex = null;
            string path = AgentTreeUtil.BuildInstallPath();
            if (!string.IsNullOrEmpty(path))
            {
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(path, icon + ".png"));
            }
            if (tex == null)
            {
                tex = EditorGUIUtility.LoadRequired(icon) as Texture2D;
            }
            return tex;
        }
        public static Texture2D dot { get { return _dot != null ? _dot : _dot = LoadIcon("xnode_dot"); } }
        private static Texture2D _dot;
        public static Texture2D dotOuter 
        {
            get 
            { return _dotOuter != null ? _dotOuter : _dotOuter = LoadIcon("xnode_dot_outer"); } 
        }
        private static Texture2D _dotOuter;
        public static Texture2D linkOuter { get { return _linkOuter != null ? _linkOuter : _linkOuter = LoadIcon("xnode_link"); } }
        private static Texture2D _linkOuter;
        public static Texture2D nodeBody { get { return _nodeBody != null ? _nodeBody : _nodeBody = LoadIcon("xnode_node"); } }
        private static Texture2D _nodeBody;
        public static Texture2D nodeHighlight { get { return _nodeHighlight != null ? _nodeHighlight : _nodeHighlight = LoadIcon("xnode_node_highlight"); } }
        private static Texture2D _nodeHighlight;
    }
}

#endif