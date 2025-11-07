/********************************************************************
生成日期:	06:30:2025
类    名: 	CameraMoveClip
作    者:	HappLI
描    述:	过场相机动画剪辑
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
using UnityEditor;
#endif
using Framework.DrawProps;
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("相机移动Clip")]
    public struct CameraMoveClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp baseProp;
        [Display("移动到")] public Vector3 targetPos;
        [Display("角度")] public Vector3 eulerAngle;
        [Display("广角"), DefaultValue(60), Disable] public float fFOV;
        [Display("过渡曲线")] public AnimationCurve speedCurve;
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new CameraMoveDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eCameraMove;
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
            fFOV = UnityEditor.EditorGUILayout.Slider("广角", fFOV, 10.0f, 179.0f);
            if (GUILayout.Button("设置当前相机参数"))
            {
                var mainCamera = Camera.main;
                if (mainCamera)
                {
                    targetPos = mainCamera.transform.position;
                    eulerAngle = mainCamera.transform.eulerAngles;
                    fFOV = mainCamera.fieldOfView;
                }
            }
            if (ControllerRefUtil.CanSetCameraEditMode())
            {
                if(ControllerRefUtil.IsEdingCamera())
                {
                    if (GUILayout.Button("退出相机编辑器模式"))
                    {
                        ControllerRefUtil.SetEditCameraMode(false);
                    }
                }
                else
                {
                    if (GUILayout.Button("进入相机编辑器模式"))
                    {
                        ControllerRefUtil.SetEditCameraMode(true);
                    }
                }

            }
        }
        //-----------------------------------------------------
        public void OnSceneView(SceneView view)
        {
            if(Event.current.type == EventType.KeyDown)
            {
                if(Event.current.keyCode == KeyCode.Escape)
                {
                    ControllerRefUtil.SetEditCameraMode(false);
                    Event.current.Use();
                }
            }
        }
        //-----------------------------------------------------
        void SetDefault()
        {
            if (Camera.main == null)
                return;
            targetPos = Camera.main.transform.position;
            eulerAngle = Camera.main.transform.eulerAngles;
            fFOV = Camera.main.fieldOfView;
        }
#endif
    }
    //-----------------------------------------------------
    //相机移动驱动逻辑
    //-----------------------------------------------------
    public class CameraMoveDriver : ACutsceneDriver
    {
        private float m_fCurFov = 0.0f;
        private Vector3 m_CurPos = Vector3.zero;
        private Quaternion m_CurEulerAngle = Quaternion.identity;
        private Camera m_pMainCamera = null;
        private Transform m_pTransform;
        private bool m_bControlled = false;
        //-----------------------------------------------------
        public override void OnDestroy()
        {
#if UNITY_EDITOR
       //     if (IsEditorMode() && m_pMainCamera) m_pMainCamera.RestoreCamera();
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
            if(m_pMainCamera == null) m_pMainCamera = Camera.main;
            if (m_pMainCamera)
            {
#if UNITY_EDITOR
        //        if(IsEditorMode() && !ControllerRefUtil.IsControlling(m_pMainCamera)) m_pMainCamera.RestoreCamera();
#endif
                m_pTransform = m_pMainCamera.transform;
                m_fCurFov = m_pMainCamera.fieldOfView;
                m_CurPos = m_pTransform.position;
                m_CurEulerAngle = m_pTransform.rotation;
#if UNITY_EDITOR
         //       if (IsEditorMode()) m_pMainCamera.BackupCamera();
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
            if (clip.CanRestore())
            {
                if(ControllerRefUtil.ControllRef(m_pMainCamera) <=1)
                {
                    if (m_pTransform)
                    {
                        m_pTransform.position = m_CurPos;
                        m_pTransform.rotation = m_CurEulerAngle;
                    }
                    if (m_pMainCamera) m_pMainCamera.fieldOfView = m_fCurFov;
                }
#if UNITY_EDITOR
          //      if (IsEditorMode() && m_pMainCamera) m_pMainCamera.RestoreCamera();
#endif
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
            if (m_pMainCamera == null)
            {
                var bindObj = pTrack.GetBindLastCutsceneObject();
                if (bindObj != null) m_pMainCamera = bindObj.GetCamera();
                if (m_pMainCamera == null) m_pMainCamera = Camera.main;
                if (m_pMainCamera == null)
                    return true;
                else
                {
                    m_pTransform = m_pMainCamera.transform;
                    m_fCurFov = m_pMainCamera.fieldOfView;
                    m_CurPos = m_pTransform.position;
                    m_CurEulerAngle = m_pTransform.rotation;
                }
            }
#if UNITY_EDITOR
            if (IsEditorMode() && ControllerRefUtil.IsEditorObject(m_pMainCamera))
                return true;
#endif
            float factor = frameData.subTime / frameData.clip.GetDuration();
            var clipData = frameData.clip.Cast<CameraMoveClip>();
            if (clipData.speedCurve != null && clipData.speedCurve.length > 0)
                factor = Mathf.Clamp01(clipData.speedCurve.Evaluate(factor));
            else
                factor = Mathf.Clamp01(factor);
            m_pMainCamera.fieldOfView = Mathf.Lerp(m_fCurFov, clipData.fFOV, factor);
            if (m_pTransform)
            {
                m_pTransform.position = Vector3.Lerp(m_CurPos, clipData.targetPos, factor);
                m_pTransform.rotation = Quaternion.Lerp(m_CurEulerAngle, Quaternion.Euler(clipData.eulerAngle), factor);
            }
            return true;
        }
    }
}