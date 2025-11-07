/********************************************************************
生成日期:	06:30:2025
类    名: 	CameraPathClip
作    者:	HappLI
描    述:	路径相机剪辑
*********************************************************************/
using Framework.DrawProps;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using Framework.Cutscene.Editor;
#endif

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("相机路径动画Clip")]
    public class CameraPathClip : IBaseClip
    {
        [System.Serializable]
        public struct PathPoint
        {
            [Display("位置")] public Vector3                  position;
            [Display("角度")] public Vector3                  eulerAngle;
            [Display("广角"), DefaultValue(60)] public float  fFOV;
            [Display("入切角")] public Vector3                inTan;
            [Display("出切角")] public Vector3                outTan;
        }
        [Display("基本属性")] public BaseClipProp   baseProp;
        [Display("路径点")] public PathPoint[]      pathPoints; //路径点
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new CameraPathDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eCameraPath;
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
        //-----------------------------------------------------
#if UNITY_EDITOR
        [AddInspector]
        public void OnCameraEditor()
        {
            if (GUILayout.Button("编辑相机运行曲线"))
            {
                ControllerRefUtil.SetEditCameraMode(true);
                CameraPathEditor.StartEdit(pathPoints, (paths) => {
                    pathPoints = paths.ToArray();
                }, GetDuration());
            }
            CameraPathEditor.SetPreviewDuration(GetDuration());
        }
#endif
    }
    //-----------------------------------------------------
    //相机移动驱动逻辑
    //-----------------------------------------------------
    public class CameraPathDriver : ACutsceneDriver
    {
        private Camera m_pMainCamera = null;
        private Transform m_pTransform;
        private Vector3 m_OrigPos;
        private Quaternion m_OrigRot;
        private float m_OrigFov;
        private bool m_bControlled = false;
        //-----------------------------------------------------
        public override void OnDestroy()
        {
#if UNITY_EDITOR
            if (IsEditorMode() && m_pMainCamera != null)
                m_pMainCamera.RestoreCamera();
#endif
            if (m_bControlled)
            {
                ControllerRefUtil.UnControll(m_pMainCamera);
                m_bControlled = false;
            }
            m_pMainCamera = null;
            m_pTransform = null;
        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData clip)
        {
            var bindObj = pTrack.GetBindLastCutsceneObject();
            if (bindObj != null) m_pMainCamera = bindObj.GetCamera();
            if (m_pMainCamera == null) m_pMainCamera = Camera.main;
            if (m_pMainCamera)
            {
#if UNITY_EDITOR
                if(IsEditorMode() && !ControllerRefUtil.IsControlling(m_pMainCamera)) m_pMainCamera.RestoreCamera();
#endif
                m_pTransform = m_pMainCamera.transform;
                m_OrigPos = m_pTransform.position;
                m_OrigRot = m_pTransform.rotation;
                m_OrigFov = m_pMainCamera.fieldOfView;
#if UNITY_EDITOR
                if (IsEditorMode()) m_pMainCamera.BackupCamera();
#endif
            }
            if(!m_bControlled)
            {
                ControllerRefUtil.Controll(m_pMainCamera);
                m_bControlled = true;
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
            if (m_pMainCamera != null && clip.CanRestore())
            {
                if (ControllerRefUtil.ControllRef(m_pMainCamera) <= 1)
                {
                    m_pTransform.position = m_OrigPos;
                    m_pTransform.rotation = m_OrigRot;
                    m_pMainCamera.fieldOfView = m_OrigFov;

#if UNITY_EDITOR
                    if (IsEditorMode()) m_pMainCamera.RestoreCamera();
#endif
                }
            }
            if(m_bControlled)
            {
                ControllerRefUtil.UnControll(m_pMainCamera);
                m_bControlled = false;
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            if (m_pMainCamera == null)
            {
                var bindObj = pTrack.GetBindLastCutsceneObject();
                if (bindObj != null) m_pMainCamera = bindObj.GetCamera();
                if (m_pMainCamera == null) m_pMainCamera = Camera.main;
                if(m_pMainCamera == null)
                    return true;
                else
                {
                    m_pTransform = m_pMainCamera.transform;
                    m_OrigPos = m_pTransform.position;
                    m_OrigRot = m_pTransform.rotation;
                    m_OrigFov = m_pMainCamera.fieldOfView;
                }
            }
#if UNITY_EDITOR
            if (IsEditorMode() && ControllerRefUtil.IsEditorObject(m_pMainCamera))
                return true;
#endif
            var clipData = frameData.clip.Cast<CameraPathClip>();
            var points = clipData.pathPoints;
            if (points == null || points.Length < 2) return true;

            float duration = clipData.GetDuration();
            float t = Mathf.Clamp01(frameData.subTime / Mathf.Max(duration, 0.01f));

            // 计算分段
            int segCount = points.Length - 1;
            float segT = t * segCount;
            int segIdx = Mathf.Clamp(Mathf.FloorToInt(segT), 0, segCount - 1);
            float localT = segT - segIdx;

            var p0 = points[segIdx];
            var p1 = points[segIdx + 1];

            Vector3 start = p0.position;
            Vector3 end = p1.position;
            Vector3 tan0 = p0.position + p0.outTan;
            Vector3 tan1 = p1.position + p1.inTan;

            // 贝塞尔插值
            Vector3 pos = Bezier(start, tan0, tan1, end, localT);

            // 方向插值
            Vector3 nextPos = Bezier(start, tan0, tan1, end, Mathf.Min(localT + 0.01f, 1f));
            Vector3 forward = (nextPos - pos).sqrMagnitude > 0.0001f ? (nextPos - pos).normalized : m_pTransform.forward;
            Quaternion rot = Quaternion.Lerp(Quaternion.Euler(p0.eulerAngle), Quaternion.Euler(p1.eulerAngle), localT);

            // FOV插值
            float fov = Mathf.Lerp(p0.fFOV, p1.fFOV, localT);

            // 应用到相机
            m_pTransform.position = pos;
            m_pTransform.rotation = rot;
            m_pMainCamera.fieldOfView = fov;

            return true;
        }
        //-----------------------------------------------------
        private Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            return u * u * u * p0 +
                   3 * u * u * t * p1 +
                   3 * u * t * t * p2 +
                   t * t * t * p3;
        }
    }
}