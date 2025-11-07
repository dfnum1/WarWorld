/********************************************************************
生成日期:	06:30:2025
类    名: 	PreferencesWindow
作    者:	HappLI
描    述:	编辑器偏好设置窗口
*********************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    class PreferencesWindow : PopupWindowContent
    {
        //private static Rect _myRect;
        //private bool firstPass = true;
        private Vector2 m_scroll;
        private static Vector2 win_size = new Vector2(400, 160);
        public static void Show(Rect rect)
        {
            rect.x = rect.x - win_size.x + rect.width;
            //_myRect = rect;
            PopupWindow.Show(rect, new PreferencesWindow());
        }

        public override Vector2 GetWindowSize() => win_size;

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label("设置", new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize=22
            });
            GUILayout.Space(2);

            m_scroll = EditorGUILayout.BeginScrollView(m_scroll);
            EditorPreferences.PreferencesGUI();
            EditorGUILayout.EndScrollView();
        }


    }
}
#endif