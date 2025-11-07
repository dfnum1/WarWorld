/********************************************************************
生成日期:	06:30:2025
类    名: 	TransformMovePathClip
作    者:	HappLI
描    述:	路径动画剪辑
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
#endif
using Framework.DrawProps;
using UnityEngine;
using UnityEngine.AI;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("路径寻路Clip")]
    public class TransformMovePathClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp baseProp;
        [Display("自动寻路")] public bool autoFindPath; //是否自动寻路
        [Display("从当前位置开始")] public bool startFromNowPosition; //是否从当前物体位置开始移动
        [Display("地表高度")] public bool terrianHeight = false; //地表高度
        [Display("路径"),RowFieldInspector] public System.Collections.Generic.List<Vector3> vPaths; //路径
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new TransformMovePathDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eTransMovePath;
        }
        //-----------------------------------------------------
        public float GetDuration()
        {
            return baseProp.duration;
        }
        //-----------------------------------------------------
        public EClipEdgeType GetEndEdgeType()
        {
            return baseProp.endEdgeType;
        }
        //-----------------------------------------------------
        public string GetName()
        {
            return baseProp.name;
        }
        //-----------------------------------------------------
        public ushort GetRepeatCount()
        {
            return baseProp.repeatCnt;
        }
        //-----------------------------------------------------
        public float GetTime()
        {
            return baseProp.time;
        }
        //-----------------------------------------------------
        public float GetBlend(bool bIn)
        {
            return baseProp.GetBlend(bIn);
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        public void OnDrawFieldLineRow(System.Reflection.FieldInfo fieldInfo)
        {
            if(fieldInfo.Name == "vPaths")
            {
                if(autoFindPath)
                {
                    if(GUILayout.Button("自动寻路"))
                    {
                        Framework.Cutscene.Editor.SearchPathEditor.StartPick(
                            vPaths,
                            (list) =>
                            {
                                if (vPaths == null) vPaths = new System.Collections.Generic.List<Vector3>();
                                vPaths.Clear();
                                vPaths.AddRange(list.ToArray());
                                UnityEditorInternal.InternalEditorUtility.RepaintAllViews(); 
                            });
                    }
                }
                else
                {
                    if (GUILayout.Button("自定义路径"))
                    {
                        Framework.Cutscene.Editor.SearchPathEditor.StartPick(
                            vPaths,
                            (list) =>
                            {
                                if (vPaths == null) vPaths = new System.Collections.Generic.List<Vector3>();
                                vPaths.Clear();
                                vPaths.AddRange(list.ToArray());
                                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                            }, null, false);
                    }
                }
            }
        }
#endif
    }
    //-----------------------------------------------------
    //相机移动驱动逻辑
    //-----------------------------------------------------
    public class TransformMovePathDriver : ACutsceneDriver
    {
        ICutsceneObject m_Object;
        System.Collections.Generic.List<float> m_vSegments;
        private System.Collections.Generic.List<Vector3> m_pathPoints;
        private float m_totalDuration;
        private bool m_useNavMesh;
        private bool m_startFromNowPosition = false; // 是否从当前物体位置开始移动
        private static NavMeshPath ms_NavPath;
        //-----------------------------------------------------
        public override void OnDestroy()
        {
#if UNITY_EDITOR
            if (m_Object != null && IsEditorMode())
            {
                var unityObj = m_Object.GetUniyObject() as GameObject;
                if (unityObj) unityObj.transform.RestoreTransform();
                m_Object.Restore();
            }
#endif
            m_Object = null;
            if (m_pathPoints != null)
            {
                UnityEngine.Pool.ListPool<Vector3>.Release(m_pathPoints);
                m_pathPoints = null;
            }
            if(m_vSegments!=null)
            {
                UnityEngine.Pool.ListPool<float>.Release(m_vSegments);
                m_vSegments = null;
            }
        }
        //-----------------------------------------------------
        void CheckApplayObject(CutsceneTrack pTrack, FrameData clip)
        {
            if (m_Object != null) return;
            m_Object = pTrack.GetBindLastCutsceneObject();
        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData clip)
        {
            CheckApplayObject(pTrack, clip);
            if (m_pathPoints != null)
                UnityEngine.Pool.ListPool<Vector3>.Release(m_pathPoints);
            m_pathPoints = null;
            m_useNavMesh = false;
            m_totalDuration = 0f;

            if (m_Object != null)
            {
                var clipData = clip.clip.Cast<TransformMovePathClip>();
                m_totalDuration = clipData.GetDuration();
                m_useNavMesh = clipData.autoFindPath;
                m_startFromNowPosition = clipData.startFromNowPosition;

                var unityObj = m_Object.GetUniyObject() as GameObject;
                if (m_useNavMesh)
                {
                    // 只取第一个点为目标
                    Vector3 end = Vector3.forward * 5;
                    if (clipData.vPaths != null && clipData.vPaths.Count > 0)
                    {
                        end = clipData.vPaths[UnityEngine.Random.Range(0, clipData.vPaths.Count)];
                    }
#if UNITY_EDITOR
                    if (IsEditorMode())
                    {
                        if(unityObj) unityObj.transform.RestoreTransform();
                        m_Object.Restore();
                    }
#endif
                    var start = unityObj.transform.position;
#if UNITY_EDITOR
                    if (IsEditorMode())
                    {
                        if(unityObj) unityObj.transform.BackupTransform();
                        m_Object.Backup();
                    }
#endif
                    if (ms_NavPath == null) ms_NavPath = new NavMeshPath();
                    ms_NavPath.ClearCorners();
                    if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, ms_NavPath) && ms_NavPath.status == NavMeshPathStatus.PathComplete)
                    {
                        m_pathPoints = UnityEngine.Pool.ListPool<Vector3>.Get();
                        m_pathPoints.Clear();
                        foreach (var pt in ms_NavPath.corners)
                            m_pathPoints.Add(pt);
                    }
                    else
                    {
                        m_pathPoints = UnityEngine.Pool.ListPool<Vector3>.Get();
                        m_pathPoints.Clear();
                        m_pathPoints.Add(start);
                        m_pathPoints.Add(end);
                    }
                }
                else
                {
                    // 非自动寻路，直接插值
                    if (clipData.vPaths != null && clipData.vPaths.Count > 0)
                    {
                        m_pathPoints = UnityEngine.Pool.ListPool<Vector3>.Get();
                        m_pathPoints.Clear();
                        if (m_startFromNowPosition)
                        {
                            if (unityObj)
                            {
                                m_pathPoints.Add(unityObj.transform.position);
                            }
                            else
                            {
                                Vector3 pos = clipData.vPaths[0];
                                m_Object.GetParamPosition(ref pos);
                                m_pathPoints.Add(pos);
                            }
                        }
                        m_pathPoints.AddRange(clipData.vPaths);
                    }
                }
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
#if UNITY_EDITOR
            if (m_Object != null && IsEditorMode() && clip.CanRestore())
            {
                var unityObj = m_Object.GetUniyObject() as GameObject;
                if (unityObj) unityObj.transform.RestoreTransform();
                m_Object.Restore();
            }
#endif
            m_Object = null;
            if (m_pathPoints != null)
            {
                UnityEngine.Pool.ListPool<Vector3>.Release(m_pathPoints);
                m_pathPoints = null;
            }
            if (m_vSegments != null)
            {
                UnityEngine.Pool.ListPool<float>.Release(m_vSegments);
                m_vSegments = null;
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
#if UNITY_EDITOR
            if (IsEditorMode() && ControllerRefUtil.IsEditorObject(m_Object))
                return true;
#endif
            CheckApplayObject(pTrack, frameData);
            if (m_Object == null)
                return true;
            float duration = m_totalDuration > 0 ? m_totalDuration : 1f;
            float curTime = frameData.subTime;

            // 路径插值
            if (m_pathPoints != null && m_pathPoints.Count > 1)
            {
                float t = Mathf.Clamp01(curTime / duration);
                Vector3 pos = EvaluatePath(m_pathPoints, t);

                // 计算朝向
                Vector3 forward = Vector3.zero;
                // 计算当前所处的段
                int segmentCount = m_pathPoints.Count - 1;
                float totalLength = 0f;
                if (m_vSegments == null)
                    m_vSegments = UnityEngine.Pool.ListPool<float>.Get();
                m_vSegments.Clear();
                for (int i = 0; i < segmentCount; ++i)
                {
                    float distance = Vector3.Distance(m_pathPoints[i], m_pathPoints[i + 1]);
                    m_vSegments.Add(distance);
                    totalLength += distance;
                }
                float targetLen = t * totalLength;
                float accum = 0f;
                for (int i = 0; i < segmentCount; ++i)
                {
                    if (accum + m_vSegments[i] >= targetLen)
                    {
                        // 朝向下一个点
                        forward = m_pathPoints[i + 1] - m_pathPoints[i];
                        break;
                    }
                    accum += m_vSegments[i];
                }
                if (forward.sqrMagnitude > 0.0001f)
                {
                    Quaternion rot = Quaternion.LookRotation(forward.normalized, Vector3.up);
                    if (!m_Object.SetParamQuaternion(rot))
                    {
                        var unityObj = m_Object.GetUniyObject() as GameObject;
                        if (unityObj != null) unityObj.transform.rotation = rot;
                    }
                }
                if(!m_Object.SetParamPosition(pos))
                {
                    var unityObj = m_Object.GetUniyObject() as GameObject;
                    if(unityObj!=null) unityObj.transform.position = pos;
                }
                var clipData = frameData.clip.Cast<TransformMovePathClip>();
                if (clipData.terrianHeight) m_Object.SetParamTerrainHeightCheck();

            }
            return true;
        }
        //-----------------------------------------------------
        // 路径分段线性插值
        Vector3 EvaluatePath(System.Collections.Generic.List<Vector3> points, float t)
        {
            if (points == null || points.Count < 2) return Vector3.zero;
            int segmentCount = points.Count - 1;
            float totalLength = 0f;

            if (m_vSegments == null)
                m_vSegments = UnityEngine.Pool.ListPool<float>.Get();
            m_vSegments.Clear();

            for (int i = 0; i < segmentCount; ++i)
            {
                float segLen = Vector3.Distance(points[i], points[i + 1]);
                m_vSegments.Add(segLen);
                totalLength += segLen;
            }
            float targetLen = t * totalLength;
            float accum = 0f;
            for (int i = 0; i < segmentCount; ++i)
            {
                if (accum + m_vSegments[i] >= targetLen)
                {
                    if(Mathf.Abs(m_vSegments[i])>0.001f)
                    {
                        float segT = (targetLen - accum) / m_vSegments[i];
                        return Vector3.Lerp(points[i], points[i + 1], segT);
                    }
                }
                accum += m_vSegments[i];
            }
            return points[points.Count - 1];
        }
    }
}