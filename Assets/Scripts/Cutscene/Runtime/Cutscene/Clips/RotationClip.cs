/********************************************************************
生成日期:	06:30:2025
类    名: 	RotationClip
作    者:	HappLI
描    述:	转向Clip用于控制相机位置变化
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
using UnityEditor;
#endif
using Framework.DrawProps;
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("朝向Clip")]
    public class RotationClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp baseProp;
        [Display("角度")] public Vector3 eulerAngle = Vector3.zero;
        [Display("过渡时长")] public float lerpTime = 0.5f; //位置过渡时间
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new RotationDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eSetRotation;
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
        public void OnSceneView(SceneView sceneView)
        {
            if (baseProp.ownerTrackObject != null)
            {
                var binder = baseProp.ownerTrackObject.GetBindLastCutsceneObject();
                if (binder != null)
                {
                    Vector3 position = Vector3.zero;
                    if (binder.GetParamPosition(ref position))
                    {
                        EditorGUI.BeginChangeCheck();
                        var rotation = Handles.DoRotationHandle(Quaternion.Euler(eulerAngle), position);
                        Handles.Label(position, "旋转");
                        if(EditorGUI.EndChangeCheck())
                        {
                            eulerAngle = rotation.eulerAngles;
                            binder.SetParamEulerAngle(eulerAngle);
                        }
                    }
                }
            }
        }
#endif
    }
    //-----------------------------------------------------
    //转向剪辑逻辑
    //-----------------------------------------------------
    public class RotationDriver : ACutsceneDriver
    {
        private bool m_bGet = false;
        private Quaternion m_Ori = Quaternion.identity;
        private ICutsceneObject m_pHold;
        //-----------------------------------------------------
        public override void OnDestroy()
        {
            if (m_pHold != null) m_pHold.SetParamHold(false);
            m_pHold = null;
        }
        //-----------------------------------------------------
        void CheckObject(CutsceneTrack pTrack)
        {
            if (m_pHold == null)
            {
                m_pHold = pTrack.GetBindLastCutsceneObject();
                if (m_pHold != null)
                {
                    m_bGet = m_pHold.GetParamQuaternion(ref m_Ori);
                }
            }
        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData frameData)
        {
            m_bGet = false;
            CheckObject(pTrack);
            if (m_pHold == null) return true;
            m_pHold.SetParamHold(true);
            var clipData = frameData.clip.Cast<RotationClip>();
            if (clipData.lerpTime <= 0)
                m_pHold.SetParamEulerAngle(clipData.eulerAngle);
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData frameData)
        {
            if (frameData.CanRestore() || frameData.IsLeaveIn())
            {
                if (m_pHold != null && m_bGet)
                {
                    m_pHold.SetParamQuaternion(m_Ori);
                }
                if(m_pHold!=null) m_pHold.SetParamHold(false);
            }
            m_pHold = null;
            m_bGet = false;
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            CheckObject(pTrack);
            if (m_pHold == null)
                return true;
            var clipData = frameData.clip.Cast<RotationClip>();
            if (m_pHold != null)
            {
                m_pHold.SetParamHold(true);
                if (clipData.lerpTime > 0 && m_bGet)
                {
                    float lerpTime = Mathf.Min(clipData.lerpTime, clipData.GetDuration());
                    m_pHold.SetParamQuaternion(Quaternion.Lerp(m_Ori, Quaternion.Euler(clipData.eulerAngle), frameData.subTime / lerpTime));
                }
                else
                    m_pHold.SetParamEulerAngle(clipData.eulerAngle);
            }
            return true;
        }
    }
}