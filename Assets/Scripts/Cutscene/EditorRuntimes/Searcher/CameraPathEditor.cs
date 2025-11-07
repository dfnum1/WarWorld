/********************************************************************
生成日期:	2025-07-09
类    名: 	CameraPathEditor
作    者:	HappLI
描    述:	相机路径点录制与编辑
*********************************************************************/
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Framework.Cutscene.Runtime;

namespace Framework.Cutscene.Editor
{
    public static class CameraPathEditor
    {
        private static Stack<List<CameraPathClip.PathPoint>> s_vStacks = new Stack<List<CameraPathClip.PathPoint>>();
        private static List<CameraPathClip.PathPoint> s_vPaths;
        private static System.Action<List<CameraPathClip.PathPoint>> s_OnChanged;
        private static bool s_Editing = false;
        private static int s_SelectedIndex = -1;

        // 预览相关
        private static bool s_Previewing = false;
        private static float s_PreviewTime = 0f;
        private static float s_PreviewDuration = 3f; // 路径预览总时长
        private static Camera s_PreviewCamera = null;
        private static Vector3 s_CamOrigPos;
        private static Quaternion s_CamOrigRot;
        private static float s_CamOrigFov;

        //-----------------------------------------------------
        public static void StartEdit(CameraPathClip.PathPoint[] vPaths, System.Action<IList<CameraPathClip.PathPoint>> onChanged = null, float duration = 3)
        {
            s_PreviewDuration = duration;
            s_vPaths = vPaths != null ? new List<CameraPathClip.PathPoint>(vPaths) : new List<CameraPathClip.PathPoint>();
            s_OnChanged = onChanged;
            s_Editing = true;
            s_SelectedIndex = -1;
            s_Previewing = false;
            SceneView.duringSceneGui -= OnSceneView;
            SceneView.duringSceneGui += OnSceneView;
            SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("相机曲线编辑中..."), float.MaxValue - 1);
            EditorUtility.DisplayDialog("提示", "Scene视图：\n- 按C录制当前相机点\n- 按P预览\n- 按alt可现实选中点的删除按钮\n- 拖动点可调整位置\n- 按Ctrl可编辑曲线曲率", "确定");
        }
        //-----------------------------------------------------
        public static void SetPreviewDuration(float duration)
        {
            if(s_Editing)
                s_PreviewDuration = duration;
        }
        //-----------------------------------------------------
        public static bool IsEditing()
        {
            return s_Editing;
        }
        //-----------------------------------------------------
        public static void StopEdit()
        {
            if (s_Editing)
            {
                if (s_OnChanged != null && s_vPaths != null)
                {
                    s_OnChanged(s_vPaths);
                }
                SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("退出相机曲线编辑"),0.8f);
            }
            s_Editing = false;
            s_vPaths = null;
            s_OnChanged = null;
            s_SelectedIndex = -1;
            s_Previewing = false;
            s_vStacks.Clear();
            RestorePreviewCamera();
            SceneView.duringSceneGui -= OnSceneView;
            EditorApplication.update -= OnPreviewUpdate;
            ControllerRefUtil.SetEditCameraMode(false);
        }
        //-----------------------------------------------------
        private static void OnSceneView(SceneView sceneView)
        {
            if (!s_Editing || s_vPaths == null)
                return;

            Event e = Event.current;
            Handles.color = Color.yellow;
            if(e.type == EventType.KeyDown && e.keyCode == KeyCode.P)
            {
                if (!s_Previewing && s_vPaths != null && s_vPaths.Count > 1)
                {
                    StartPreview();
                }
            }
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Z)
            {
                if(s_vStacks.Count>0)
                {
                    s_vPaths = s_vStacks.Pop();

                    e.Use();
                }
            }

            // 录制当前相机点
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.C)
            {
                var cam = Camera.main;
                if (cam)
                {

                    CameraPathClip.PathPoint pt = new CameraPathClip.PathPoint
                    {
                        position = cam.transform.position,
                        eulerAngle = cam.transform.eulerAngles,
                        fFOV = cam.fieldOfView,
                        inTan = Vector3.zero,
                        outTan = Vector3.zero
                    };
                    if (s_SelectedIndex >= 0 && s_SelectedIndex < s_vPaths.Count && (s_vPaths[s_SelectedIndex].position - pt.position).magnitude <= 1)
                    {
                        s_vStacks.Push(new List<CameraPathClip.PathPoint>(s_vPaths.ToArray()));
                        s_vPaths[s_SelectedIndex] = pt;
                    }
                    else
                    {
                        s_vStacks.Push(new List<CameraPathClip.PathPoint>(s_vPaths.ToArray()));
                        s_vPaths.Add(pt);
                    }
                }
                else
                {
                    sceneView.ShowNotification(new GUIContent("当前场景没有主相机，无法录制点位"), 1);
                }
                e.Use();
            }

            // 拖动点、显示方向
            for (int i = 0; i < s_vPaths.Count; ++i)
            {
                var pt = s_vPaths[i];

                // 拖动主点
                if (!e.control)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPos = Handles.PositionHandle(pt.position, Quaternion.Euler(pt.eulerAngle));
                    if (EditorGUI.EndChangeCheck())
                    {
                        s_vStacks.Push(new List<CameraPathClip.PathPoint>(s_vPaths.ToArray()));
                        pt.position = newPos;
                        s_vPaths[i] = pt;
                    }
                }

                // 选中高亮
                Color color = Handles.color;
                Handles.color = s_SelectedIndex == i ? Color.yellow:Color.blue;
                if (Handles.Button(pt.position, Quaternion.identity, s_SelectedIndex == i?0.5f:0.2f, 0.2f, Handles.SphereHandleCap))
                {
                    s_SelectedIndex = i;
                    if(Camera.main)
                    {
                        Camera.main.transform.position = pt.position;
                        Camera.main.transform.eulerAngles = pt.eulerAngle;
                        Camera.main.fieldOfView = pt.fFOV;
                    }
                }
                Handles.color = color;

                // Alt键时显示移除按钮
                if (!s_Previewing && Event.current.alt)
                {
                    Handles.BeginGUI();
                    Vector2 guiPos = HandleUtility.WorldToGUIPoint(pt.position);
                    Rect btnRect = new Rect(guiPos.x + 10, guiPos.y - 10, 50, 24);
                    if (GUI.Button(btnRect, "移除"))
                    {
                        s_vStacks.Push(new List<CameraPathClip.PathPoint>(s_vPaths.ToArray()) );
                        s_vPaths.RemoveAt(i);
                        s_SelectedIndex = -1;
                        s_OnChanged?.Invoke(s_vPaths);
                        e.Use();
                        Handles.EndGUI();
                        break;
                    }
                    btnRect = new Rect(guiPos.x + 10, guiPos.y - 10+24, 50, 24);
                    if (Camera.main && GUI.Button(btnRect, "更新"))
                    {
                        CameraPathClip.PathPoint newPt = new CameraPathClip.PathPoint
                        {
                            position = Camera.main.transform.position,
                            eulerAngle = Camera.main.transform.eulerAngles,
                            fFOV = Camera.main.fieldOfView,
                            inTan = Vector3.zero,
                            outTan = Vector3.zero
                        };
                        s_vPaths[i] = newPt;
                        s_OnChanged?.Invoke(s_vPaths);
                        e.Use();
                        Handles.EndGUI();
                        break;
                    }
                    btnRect = new Rect(guiPos.x + 10, guiPos.y - 10 + 48, 60, 24);
                    if (Camera.main && GUI.Button(btnRect, "预览视角"))
                    {
                        Camera.main.transform.position = s_vPaths[i].position;
                        Camera.main.transform.eulerAngles = s_vPaths[i].eulerAngle;
                        Camera.main.fieldOfView = s_vPaths[i].fFOV;
                        e.Use();
                        Handles.EndGUI();
                        break;
                    }
                    Handles.EndGUI();
                }

                // 显示欧拉角方向箭头
                Handles.color = Color.cyan;
                Vector3 dir = Quaternion.Euler(pt.eulerAngle) * Vector3.forward;
                Handles.ArrowHandleCap(0, pt.position, Quaternion.LookRotation(dir), 1.0f, EventType.Repaint);

                // 按Ctrl显示inTan/outTan
                if (e.control)
                {
                    Handles.color = Color.magenta;
                    // inTan
                    EditorGUI.BeginChangeCheck();
                    Vector3 inTanPos = pt.position + pt.inTan;
                    inTanPos = Handles.PositionHandle(inTanPos, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        s_vStacks.Push(new List<CameraPathClip.PathPoint>(s_vPaths.ToArray()));
                        pt.inTan = inTanPos - pt.position;
                        s_vPaths[i] = pt;
                    }
                    Handles.DrawLine(pt.position, pt.position + pt.inTan);
                    Handles.Label(pt.position + pt.inTan, "inTan");

                    // outTan
                    EditorGUI.BeginChangeCheck();
                    Vector3 outTanPos = pt.position + pt.outTan;
                    outTanPos = Handles.PositionHandle(outTanPos, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        s_vStacks.Push(new List<CameraPathClip.PathPoint>(s_vPaths.ToArray()));
                        pt.outTan = outTanPos - pt.position;
                        s_vPaths[i] = pt;
                    }
                    Handles.DrawLine(pt.position, pt.position + pt.outTan);
                    Handles.Label(pt.position + pt.outTan, "outTan");
                }
            }

            // 绘制贝塞尔曲线
            Handles.color = Color.green;
            for (int i = 0; i < s_vPaths.Count - 1; ++i)
            {
                var p0 = s_vPaths[i];
                var p1 = s_vPaths[i + 1];
                Vector3 start = p0.position;
                Vector3 end = p1.position;
                Vector3 tan0 = p0.position + p0.outTan;
                Vector3 tan1 = p1.position + p1.inTan;
                Handles.DrawBezier(start, end, tan0, tan1, Color.green, null, 2f);
            }

            sceneView.Repaint();

            // ESC退出
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                if (s_Previewing)
                {
                    StopPreview();
                }
                else
                {
                    StopEdit();
                }
                e.Use();
            }
        }
        //-----------------------------------------------------
        // 路径预览
        private static void StartPreview()
        {
            s_Previewing = true;
            s_PreviewTime = 0f;
            s_PreviewDuration = Mathf.Max(3f, s_vPaths.Count * 1.5f);

            s_PreviewCamera = Camera.main;
            if (s_PreviewCamera != null)
            {
                s_CamOrigPos = s_PreviewCamera.transform.position;
                s_CamOrigRot = s_PreviewCamera.transform.rotation;
                s_CamOrigFov = s_PreviewCamera.fieldOfView;
            }
            EditorApplication.update -= OnPreviewUpdate;
            EditorApplication.update += OnPreviewUpdate;
        }
        //-----------------------------------------------------
        private static void StopPreview()
        {
            s_Previewing = false;
            EditorApplication.update -= OnPreviewUpdate;
            RestorePreviewCamera();
        }
        //-----------------------------------------------------
        private static void RestorePreviewCamera()
        {
            if (s_PreviewCamera != null)
            {
                s_PreviewCamera.transform.position = s_CamOrigPos;
                s_PreviewCamera.transform.rotation = s_CamOrigRot;
                s_PreviewCamera.fieldOfView = s_CamOrigFov;
                s_PreviewCamera = null;
            }
        }
        //-----------------------------------------------------
        private static void OnPreviewUpdate()
        {
            if (!s_Previewing || s_vPaths == null || s_vPaths.Count < 2)
            {
                StopPreview();
                return;
            }

            s_PreviewTime += Time.deltaTime;
            float t = Mathf.Clamp01(s_PreviewTime / s_PreviewDuration);

            // 计算贝塞尔曲线插值
            Vector3 pos;
            Vector3 forward;
            float fov;
            Quaternion rot;
            EvaluateBezierPath(s_vPaths, t, out pos, out forward, out rot, out fov);

            if (s_PreviewCamera != null)
            {
                s_PreviewCamera.transform.position = pos;
                s_PreviewCamera.transform.rotation = rot;
                s_PreviewCamera.fieldOfView = fov;
            }

            SceneView.RepaintAll();

            if (t >= 1f)
            {
                StopPreview();
            }
        }
        //-----------------------------------------------------
        // 贝塞尔曲线插值
        private static void EvaluateBezierPath(List<CameraPathClip.PathPoint> points, float t, out Vector3 pos, out Vector3 forward, out Quaternion rot, out float fov)
        {
            int segCount = points.Count - 1;
            float segT = t * segCount;
            int segIdx = Mathf.Clamp(Mathf.FloorToInt(segT), 0, segCount - 1);
            float localT = segT - segIdx;

            var p0 = points[segIdx];
            var p1 = points[segIdx + 1];

            Vector3 start = p0.position;
            Vector3 end = p1.position;
            Vector3 tan0 = p0.position + p0.outTan;
            Vector3 tan1 = p1.position + p1.inTan;

            pos = Bezier(start, tan0, tan1, end, localT);

            // 方向
            Vector3 nextPos = Bezier(start, tan0, tan1, end, Mathf.Min(localT + 0.01f, 1f));
            forward = (nextPos - pos).normalized;
            rot = Quaternion.Euler(Vector3.Lerp(p0.eulerAngle, p1.eulerAngle, localT));
            fov = Mathf.Lerp(p0.fFOV, p1.fFOV, localT);
        }
        //-----------------------------------------------------
        private static Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            return u * u * u * p0 +
                   3 * u * u * t * p1 +
                   3 * u * t * t * p2 +
                   t * t * t * p3;
        }
    }
}
#endif