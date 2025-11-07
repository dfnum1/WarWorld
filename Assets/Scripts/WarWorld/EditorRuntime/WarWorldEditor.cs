/********************************************************************
生成日期:	11:07:2025
类    名: 	WarWorld
作    者:	HappLI
描    述:	战争世界编辑器
*********************************************************************/
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Framework.ED;

namespace Framework.War.Editor
{
    public class WarWorldEditor : EditorWindowBase
    {
        //--------------------------------------------------------
        [MenuItem("Tools/战争编辑器")]
        public static void Open()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
                return;
            }
            WarWorldEditor window = EditorWindow.GetWindow<WarWorldEditor>();
            window.titleContent = new GUIContent("战争编辑器");
        }
    }
}
#endif