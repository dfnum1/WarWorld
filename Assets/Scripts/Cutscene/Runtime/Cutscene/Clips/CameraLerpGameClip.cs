/********************************************************************
生成日期:	06:30:2025
类    名: 	CameraLerpGameClip
作    者:	HappLI
描    述:	过场相机还原游戏相机剪辑
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
#endif
using Framework.DrawProps;
using UnityEngine;
namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("相机过渡游戏视角Clip")]
    public struct CameraLerpGameClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp        baseProp;
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new CameraLerpGameDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eCameraLerpToGame;
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
        [AddInspector]
        public void AddEditorInsector()
        {
            if(baseProp.ownerTrackObject!=null && GUILayout.Button("设置当前游戏相机参数"))
            {
                var lists = baseProp.ownerTrackObject.GetDriversByType(typeof(CameraLerpGameDriver));
                if(lists != null)
                {
                    foreach(var db in lists)
                    {
                        ((CameraLerpGameDriver)db).SyncGameCameraData(baseProp.ownerTrackObject);
                    }
                }
            }
        }
#endif
    }
    //-----------------------------------------------------
    //CameraLerpGameDriver
    //-----------------------------------------------------
    public class CameraLerpGameDriver : ACutsceneDriver
    {
        private float m_fOriFov = 0.0f;
        private Vector3 m_OriPos = Vector3.zero;
        private Quaternion m_OriEulerAngle = Quaternion.identity;

        private float m_fCurFov = 0.0f;
        private Vector3 m_CurPos = Vector3.zero;
        private Quaternion m_CurEulerAngle = Quaternion.identity;

        private Camera m_pMainCamera = null;
        private Transform m_pTransform;
        private bool m_bControlled = false;
        //-----------------------------------------------------
        public override void OnDestroy()
        {
            if (m_bControlled)
            {
                ControllerRefUtil.UnControll(m_pMainCamera);
                m_bControlled = false;
            }
            m_pMainCamera = null;
            m_pTransform = null;
        }
        //-----------------------------------------------------
        public void SyncGameCameraData(CutsceneTrack pTrack)
        {
            var bindObj = pTrack.GetBindLastCutsceneObject();
            if (bindObj != null) m_pMainCamera = bindObj.GetCamera();
            if (m_pMainCamera == null) m_pMainCamera = Camera.main;
            if (m_pMainCamera)
            {
                m_pTransform = m_pMainCamera.transform;
                m_fOriFov = m_pMainCamera.fieldOfView;
                m_OriPos = m_pTransform.position;
                m_OriEulerAngle = m_pTransform.rotation;
#if UNITY_EDITOR
                if (IsEditorMode()) LockUtil.BackupCamera(m_pMainCamera, true);
#endif
            }
        }
        //-----------------------------------------------------
        public override bool OnCreateClip(CutsceneTrack pTrack, IBaseClip clip)
        {
            SyncGameCameraData(pTrack);
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData clip)
        {
            var bindObj = pTrack.GetBindLastCutsceneObject();
            if (bindObj != null) m_pMainCamera = bindObj.GetCamera();
            if(m_pMainCamera == null) m_pMainCamera = Camera.main;
            if (m_pMainCamera)
            {
                if(!m_bControlled)
                {
                    m_bControlled = true;
                    ControllerRefUtil.Controll(m_pMainCamera);
                }
                m_pTransform = m_pMainCamera.transform;
                m_fCurFov = m_pMainCamera.fieldOfView;
                m_CurPos = m_pTransform.position;
                m_CurEulerAngle = m_pTransform.rotation;
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
            if (ControllerRefUtil.ControllRef(m_pMainCamera) <= 1)
            {
                if (m_pTransform)
                {
                    m_pTransform.position = m_OriPos;
                    m_pTransform.rotation = m_OriEulerAngle;
                }
                if (m_pMainCamera)
                {
                    m_pMainCamera.fieldOfView = m_fOriFov;
                }
            }
            if (m_bControlled)
            {
                ControllerRefUtil.UnControll(m_pMainCamera);
                m_bControlled = false;
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            if (m_pMainCamera == null) return true;
#if UNITY_EDITOR
            if (IsEditorMode() && ControllerRefUtil.IsEditorObject(m_pMainCamera))
                return true;
#endif
            m_pMainCamera.fieldOfView = Mathf.Lerp(m_fCurFov, m_fOriFov, frameData.subTime / frameData.clip.GetDuration());
            if (m_pTransform)
            {
                m_pTransform.position = Vector3.Lerp(m_CurPos, m_OriPos, frameData.subTime / frameData.clip.GetDuration());
                m_pTransform.rotation = Quaternion.Lerp(m_CurEulerAngle, m_OriEulerAngle, frameData.subTime / frameData.clip.GetDuration());
            }
            return true;
        }
    }
}