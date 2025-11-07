/********************************************************************
生成日期:	2025-07-09
类    名: 	CurvePathEditor
作    者:	HappLI
描    述:	曲线路径编辑器
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Runtime;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Framework.Cutscene.Editor
{
    public static class CurvePathEditor
    {
        private static Stack<List<PathPoint>> s_vStacks = new Stack<List<PathPoint>>();
        private static List<PathPoint> s_vPaths;
        private static System.Action<List<PathPoint>> s_OnChanged;
        private static bool s_Editing = false;
        private static int s_SelectedIndex = -1;

        // 预览相关
        private static bool s_Previewing = false;
        private static float s_PreviewTime = 0f;
        private static float s_PreviewDuration = 3f; // 路径预览总时长
        private static bool s_pathFoward = true; 

        private static Vector3 s_PreviewPosition;
        private static Quaternion s_PreviewEulerangle;

        public static Transform editTest;
        public static ICutsceneObject editCutsceneObject;

        private static PathCurve m_PathCurve;
        //-----------------------------------------------------
        public static void StartEdit(PathPoint[] vPaths, System.Action<IList<PathPoint>> onChanged = null, float duration = 3)
        {
            s_PreviewDuration = duration;
            s_vPaths = vPaths != null ? new List<PathPoint>(vPaths) : new List<PathPoint>();
            s_OnChanged = onChanged;
            s_Editing = true;
            s_SelectedIndex = -1;
            s_Previewing = false;
            SceneView.duringSceneGui -= OnSceneView;
            SceneView.duringSceneGui += OnSceneView;
            if (editTest)
            {
                Selection.activeGameObject = editTest.gameObject;
                SceneView.lastActiveSceneView?.LookAt(editTest.transform.position);
            }
            if(editCutsceneObject!=null)
            {
                Vector3 paramPos = Vector3.zero;
                editCutsceneObject.GetParamPosition(ref paramPos);
                SceneView.lastActiveSceneView?.LookAt(paramPos);
            }
            SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("编辑中...."),float.MaxValue-1);

            EditorUtility.DisplayDialog("提示", "Scene视图：\n- 按C录制\n- 按P预览\n- 按alt可现实选中点的删除按钮\n- 拖动点可调整位置\n- 按Ctrl可编辑曲线曲率", "确定");
        }
        //-----------------------------------------------------
        public static void SetPreviewDuration(float duration, bool pathFoward)
        {
            if (s_Editing)
            {
                s_PreviewDuration = duration;
                s_pathFoward = pathFoward;
            }
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
                SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("退出编辑."), 0.5f);
            }
            s_Editing = false;
            s_vPaths = null;
            s_OnChanged = null;
            s_SelectedIndex = -1;
            s_Previewing = false;
            s_vStacks.Clear();
            RestorePreview();
            editTest = null;
            editCutsceneObject = null;
            m_PathCurve.Dispose();

            SceneView.duringSceneGui -= OnSceneView;
            EditorApplication.update -= OnPreviewUpdate;
        }
        //-----------------------------------------------------
        public static bool isEditing
        {
            get { return s_Editing; }
        }
        //-----------------------------------------------------
        public static bool IsEditing(Transform pTransform)
        {
            return editTest == pTransform && s_Editing;
        }
        //-----------------------------------------------------
        public static bool IsEditing(ICutsceneObject pTransform)
        {
            return editCutsceneObject == pTransform && s_Editing;
        }
        //-----------------------------------------------------
        static PathPoint LockPoint(SceneView sceneView, out bool bSucceed, bool bOffset= true)
        {
            bSucceed = false;
            if (editTest)
            {

                PathPoint pt = new PathPoint
                {
                    position = editTest.position,
                    eulerAngle = editTest.eulerAngles,
                    scale = editTest.localScale,
                    inTan = Vector3.zero,
                    outTan = Vector3.zero
                };
                if(bOffset) editTest.position += editTest.forward * 10;
                bSucceed = true;
                return pt;
            }
            else if (editCutsceneObject != null)
            {
                Vector3 paramPos = Vector3.zero;
                Vector3 paramEuler = Vector3.zero;
                Vector3 paramScale = Vector3.one;
                editCutsceneObject.GetParamPosition(ref paramPos);
                editCutsceneObject.GetParamEulerAngle(ref paramEuler);
                editCutsceneObject.GetParamScale(ref paramScale);
                PathPoint pt = new PathPoint
                {
                    position = paramPos,
                    eulerAngle = paramEuler,
                    scale = paramScale,
                    inTan = Vector3.zero,
                    outTan = Vector3.zero
                };

                if (bOffset) editCutsceneObject.SetParamPosition(paramPos + Vector3.forward * 10);
                bSucceed = true;
                return pt;
            }
            else
            {
                sceneView.ShowNotification(new GUIContent("请先设置需要录制的对象，无法录制点位"), 1);
                return new PathPoint();
            }
        }
        //-----------------------------------------------------
        private static void OnSceneView(SceneView sceneView)
        {
            if (!s_Editing || s_vPaths == null)
                return;

            if (s_Previewing && editTest == null)
            {
                Handles.SphereHandleCap(0, s_PreviewPosition, s_PreviewEulerangle, 0.5f, EventType.Repaint);
            }

            if(editCutsceneObject !=null && editTest == null)
            {
                Vector3 paramPos = Vector3.zero;
                Quaternion paramEuler = Quaternion.identity;
                Vector3 paramScale = Vector3.one;
                editCutsceneObject.GetParamPosition(ref paramPos);
                editCutsceneObject.GetParamQuaternion(ref paramEuler);
                editCutsceneObject.GetParamScale(ref paramScale);
                if (Tools.current == Tool.Scale)
                {
                    editCutsceneObject.SetParamScale(Handles.DoScaleHandle(paramScale, paramPos, paramEuler, Mathf.Min(5, HandleUtility.GetHandleSize(paramPos))));
                }
                else if (Tools.current == Tool.Rotate)
                {
                    editCutsceneObject.SetParamQuaternion(Handles.DoRotationHandle(paramEuler, paramPos));
                }
                else
                {
                    editCutsceneObject.SetParamPosition(Handles.DoPositionHandle(paramPos, paramEuler));
                }
            }

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
                var pt = LockPoint(sceneView, out var bSucceed, true);
                if(bSucceed)
                {
                    if (s_SelectedIndex >= 0 && s_SelectedIndex < s_vPaths.Count && (s_vPaths[s_SelectedIndex].position - pt.position).magnitude <= 1)
                    {
                        s_vStacks.Push(new List<PathPoint>(s_vPaths.ToArray()));
                        s_vPaths[s_SelectedIndex] = pt;
                    }
                    else
                    {
                        s_vStacks.Push(new List<PathPoint>(s_vPaths.ToArray()));
                        s_vPaths.Add(pt);
                    }
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
                    if(Tools.current == Tool.Rotate)
                    {
                        Vector3 newPos = Handles.DoRotationHandle(Quaternion.Euler(pt.eulerAngle), pt.position).eulerAngles;
                        if (EditorGUI.EndChangeCheck())
                        {
                            s_vStacks.Push(new List<PathPoint>(s_vPaths.ToArray()));
                            pt.eulerAngle = newPos;
                            s_vPaths[i] = pt;
                        }
                    }
                    else if (Tools.current == Tool.Scale)
                    {
                        Vector3 newPos = Handles.DoScaleHandle(pt.scale, pt.position, Quaternion.Euler(pt.eulerAngle), HandleUtility.GetHandleSize(pt.position));
                        if (EditorGUI.EndChangeCheck())
                        {
                            s_vStacks.Push(new List<PathPoint>(s_vPaths.ToArray()));
                            pt.scale = newPos;
                            s_vPaths[i] = pt;
                        }
                    }
                    else
                    {
                        Vector3 newPos = Handles.PositionHandle(pt.position, Quaternion.Euler(pt.eulerAngle));
                        if (EditorGUI.EndChangeCheck())
                        {
                            s_vStacks.Push(new List<PathPoint>(s_vPaths.ToArray()));
                            pt.position = newPos;
                            s_vPaths[i] = pt;
                        }
                    }
                }

                // 选中高亮
                Color color = Handles.color;
                Handles.color = s_SelectedIndex == i ? Color.yellow:Color.blue;
                if (Handles.Button(pt.position, Quaternion.identity, s_SelectedIndex == i?0.5f:0.2f, 0.2f, Handles.SphereHandleCap))
                {
                    s_SelectedIndex = i;
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
                        s_vStacks.Push(new List<PathPoint>(s_vPaths.ToArray()) );
                        s_vPaths.RemoveAt(i);
                        s_SelectedIndex = -1;
                        s_OnChanged?.Invoke(s_vPaths);
                        e.Use();
                        Handles.EndGUI();
                        break;
                    }
                    btnRect = new Rect(guiPos.x + 10, guiPos.y - 10+24, 80, 24);
                    if (GUI.Button(btnRect, "更新位置"))
                    {
                        var pt1 = LockPoint(sceneView, out var bSucceed, true);
                        if (bSucceed)
                        {
                            s_vStacks.Push(new List<PathPoint>(s_vPaths.ToArray()));
                            s_vPaths[i] = pt1;
                            s_OnChanged?.Invoke(s_vPaths);
                            e.Use();
                        }

                        Handles.EndGUI();
                        break;
                    }
                    Handles.EndGUI();
                }

                // 显示欧拉角方向箭头
                Handles.color = Color.cyan;
                Vector3 dir = Quaternion.Euler(pt.eulerAngle) * Vector3.forward;
                Handles.ArrowHandleCap(0, pt.position, Quaternion.LookRotation(dir), 2.0f, EventType.Repaint);

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
                        s_vStacks.Push(new List<PathPoint>(s_vPaths.ToArray()));
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
                        s_vStacks.Push(new List<PathPoint>(s_vPaths.ToArray()));
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
                Handles.DrawBezier(start, end, tan0, tan1, Color.green, null, 5f);
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
            if(editTest)
            {
                Selection.activeTransform = editTest;
            }
        }
        //-----------------------------------------------------
        // 路径预览
        private static void StartPreview()
        {
            s_Previewing = true;
            s_PreviewTime = 0f;
            s_PreviewDuration = Mathf.Max(3f, s_vPaths.Count * 1.5f);

            m_PathCurve.Set(s_vPaths.ToArray());

            EditorApplication.update -= OnPreviewUpdate;
            EditorApplication.update += OnPreviewUpdate;
        }
        //-----------------------------------------------------
        private static void StopPreview()
        {
            s_Previewing = false;
            EditorApplication.update -= OnPreviewUpdate;
            RestorePreview();
        }
        //-----------------------------------------------------
        private static void RestorePreview()
        {
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

            if(m_PathCurve.Evaluate(t,out var point, s_pathFoward))
            {
                s_PreviewPosition = point.position;
                if(point.useRot) s_PreviewEulerangle = point.rot;
                if (editTest != null)
                {
                    editTest.position = point.position;
                    editTest.rotation = s_PreviewEulerangle;
                    editTest.localScale = point.scale;
                }
            }

            SceneView.RepaintAll();

            if (t >= 1f)
            {
                StopPreview();
            }
        }
    }
}
#endif