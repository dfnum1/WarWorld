/********************************************************************
生成日期:	06:30:2025
类    名: 	CameraShakeClip
作    者:	HappLI
描    述:	相机抖动用于模拟相机震动效果，使用简单的正弦函数来模拟震动效果。
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
#endif
using Framework.DrawProps;
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("相机抖动Clip")]
    public class CameraShakeClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp       baseProp;
        [Display("震动强度")] public Vector3            shakeIntense = new Vector3(0.1f, 0.25f,0.0f);
        [Display("震动频率")] public Vector3            shakeHertz = new Vector3(60,50,1);
        [Display("衰减曲线")] public AnimationCurve     decayCurve = AnimationCurve.Linear(0, 1, 1, 0);
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new CameraShakeDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eCameraShake;
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
        [AddInspector]
        public void OnCameraEditor()
        {
            if (GUILayout.Button("模拟"))
            {
            }
        }
#endif
    }
    //-----------------------------------------------------
    //相机抖动逻辑
    //-----------------------------------------------------
    public class CameraShakeDriver : ACutsceneDriver
    {
        private Vector3 m_CurPos = Vector3.zero;
        private Camera m_pMainCamera = null;
        private Transform m_pTransform;
        //-----------------------------------------------------
        public override void OnDestroy()
        {
#if UNITY_EDITOR
            if (IsEditorMode() && m_pMainCamera) m_pMainCamera.RestoreCamera();
#endif
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
                if (IsEditorMode() && !ControllerRefUtil.IsControlling(m_pMainCamera)) m_pMainCamera.RestoreCamera();
#endif
                m_pTransform = m_pMainCamera.transform;
                m_CurPos = m_pTransform.position;
#if UNITY_EDITOR
                if (IsEditorMode()) m_pMainCamera.BackupCamera();
#endif
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
            if (ControllerRefUtil.ControllRef(m_pMainCamera) <=0)
            {
                if (m_pTransform)
                {
                    m_pTransform.position = m_CurPos;
                }

#if UNITY_EDITOR
                if (IsEditorMode() && m_pMainCamera) m_pMainCamera.RestoreCamera();
#endif
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            if (frameData.eStatus == EPlayableStatus.Pause)
                return true;
            if (m_pMainCamera == null) return true;
            var clipData = frameData.clip.Cast<CameraShakeClip>();
            if (m_pTransform)
            {
                float dampping = 1;
                if (clipData.decayCurve != null && clipData.decayCurve.length > 0)
                {
                    float maxTime = clipData.decayCurve[clipData.decayCurve.length - 1].time;
                    if(maxTime>0)
                        dampping = clipData.decayCurve.Evaluate(frameData.subTime / frameData.clip.GetDuration() * maxTime);
                }
                float fShakeX = clipData.shakeIntense.x * ((float)Mathf.Sin(clipData.shakeHertz.x * frameData.subTime))* dampping;
                float fShakeY = clipData.shakeIntense.y * ((float)Mathf.Sin(clipData.shakeHertz.y * frameData.subTime))* dampping;
                float fShakeZ = clipData.shakeIntense.z * ((float)Mathf.Sin(clipData.shakeHertz.z * frameData.subTime))* dampping;

                var offset = fShakeX * m_pTransform.forward + fShakeY * m_pTransform.up + fShakeZ * m_pTransform.right;

                m_pTransform.position = m_CurPos + offset;
            }
            return true;
        }
    }
}