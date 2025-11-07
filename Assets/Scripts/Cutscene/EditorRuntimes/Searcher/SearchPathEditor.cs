/********************************************************************
生成日期:	7:9:2025  10:37
类    名: 	SearchPathEditor
作    者:	HappLI
描    述:	寻路路径编辑
*********************************************************************/
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Framework.Cutscene.Editor
{
    public class SearchPathEditor
    {
        private static bool s_Picking = false;
        private static Vector3 s_startPos = Vector3.zero;
        private static List<Vector3> s_TargetList = null;
        private static List< List<Vector3>> s_PathList = new List<List<Vector3>>();
        private static System.Action<List<Vector3>> s_OnChanged = null;
        private static string s_Tip = "请在Scene视图中点击目标点，按Ctrl可添加多个点，ESC退出。";
        private static Vector3 s_Start = Vector3.zero;
        private static List<Color> s_ColorList = new List<Color>();
        private static bool s_bAutoPath = true;

        /// <summary>
        /// 开始拾取路径点
        /// </summary>
        /// <param name="targetList">目标点列表（会被直接修改）</param>
        /// <param name="onChanged">每次修改后回调（可为null）</param>
        /// <param name="start">起点（默认0）</param>
        public static void StartPick(IList<Vector3> targetList, System.Action<List<Vector3>> onChanged = null, Vector3? start = null, bool bAutoPath = true)
        {
            s_Picking = true;
            s_bAutoPath = bAutoPath;
            if (targetList == null) s_TargetList = new List<Vector3>();
            else  s_TargetList = new List<Vector3>(targetList);
            s_PathList.Clear();
            s_OnChanged = onChanged;
            s_Start = start ?? Vector3.zero;
            SceneView.duringSceneGui -= OnSceneView;
            SceneView.duringSceneGui += OnSceneView;
            if(s_bAutoPath)
                s_Tip = "请在Scene视图中点击目标点，按Ctrl可添加多个点，ESC退出。";
            else
                s_Tip = "请在Scene视图中点击目标点，按Ctrl可添加多个点，ESC退出。";
            EditorUtility.DisplayDialog("提示", s_Tip, "确定");
        }

        /// <summary>
        /// 退出拾取
        /// </summary>
        public static void StopPick()
        {
            if(s_Picking)
            {
                s_OnChanged?.Invoke(s_TargetList);
            }
            s_Picking = false;
            s_PathList.Clear();
            s_TargetList = null;
            s_OnChanged = null;
            SceneView.duringSceneGui -= OnSceneView;
        }

        private static void OnSceneView(SceneView sceneView)
        {
            if (!s_Picking || s_TargetList == null)
                return;

            if(s_bAutoPath)
            {
                s_startPos = Handles.PositionHandle(s_startPos, Quaternion.identity);
                Handles.Label(s_startPos, "测试起点", EditorStyles.boldLabel);
            }

            Event e = Event.current;
            // 拾取目标点
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                Plane plane = new Plane(Vector3.up, s_Start);
                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 hit = ray.GetPoint(enter);
                    if(s_bAutoPath)
                    {
                        if (e.control)
                        {
                            s_TargetList.Add(hit);
                        }
                        else
                        {
                            s_TargetList.Clear();
                            s_TargetList.Add(hit);
                        }
                        NavMeshPath navPath = new NavMeshPath();
                        s_PathList.Clear();
                        for (int i = 0; i < s_TargetList.Count; i++)
                        {
                            if (NavMesh.CalculatePath(s_startPos, s_TargetList[i], NavMesh.AllAreas, navPath) && navPath.status == NavMeshPathStatus.PathComplete)
                            {
                                List<Vector3> vPaths = new List<Vector3>();
                                foreach (var pt in navPath.corners)
                                    vPaths.Add(pt);
                                s_PathList.Add(vPaths);
                            }
                        }
                    }
                    else
                    {
                        if (e.control)
                        {
                            s_TargetList.Add(hit);
                        }
                        else
                        {
                            if (s_TargetList.Count <= 0)
                                s_TargetList.Add(hit);
                            else
                                s_TargetList[s_TargetList.Count - 1] = hit;
                        }
                        s_PathList.Clear();
                        s_PathList.Add(s_TargetList);
                    }

                    EditorUtility.SetDirty(sceneView);
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
                e.Use();
            }
            // 右键或ESC退出
            if ((e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape))
            {
                StopPick();
                e.Use();
            }

            // 可视化路径
            if (s_PathList != null && s_PathList.Count > 0)
            {
                for(int i =0; i < s_PathList.Count; ++i)
                {
                    if(i >= s_ColorList.Count)
                    {
                        s_ColorList.Add(new Color(Random.value, Random.value, Random.value, 1f));
                    }
                    Handles.color = s_ColorList[i];
                    var path = s_PathList[i];
                    Vector3 prev = path[0];
                    foreach (var pt in path)
                    {
                        Handles.DrawLine(prev, pt, 2);
                        Handles.SphereHandleCap(0, pt, Quaternion.identity, 0.2f, EventType.Repaint);
                        prev = pt;
                    }
                }

            }
            Handles.SphereHandleCap(0, s_Start, Quaternion.identity, 0.2f, EventType.Repaint);

            sceneView.Repaint();
        }
    }
}
#endif