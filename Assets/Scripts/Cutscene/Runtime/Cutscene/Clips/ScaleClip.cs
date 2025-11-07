/********************************************************************
生成日期:	06:30:2025
类    名: 	ScaleClip
作    者:	HappLI
描    述:	缩放Clip用于控制相机位置变化
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
using UnityEditor;
#endif
using Framework.DrawProps;
using UnityEngine;
using UnityEngine.Playables;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("缩放Clip")]
    public class ScaleClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp baseProp;
        [Display("缩放")] public Vector3 scale = Vector3.one;
        [Display("过渡时长")] public float lerpTime = 0.5f; //位置过渡时间
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new ScaleDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eSetScale;
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
                        scale = Handles.DoScaleHandle(scale, position, Quaternion.identity, HandleUtility.GetHandleSize(position));
                        Handles.Label(position, "缩放");
                        if (EditorGUI.EndChangeCheck())
                        {
                            binder.SetParamScale(scale);
                        }
                    }
                }
            }
        }
#endif
    }
    //-----------------------------------------------------
    //缩放剪辑逻辑
    //-----------------------------------------------------
    public class ScaleDriver : ACutsceneDriver
    {
        private bool m_bGet = false;
        private Vector3 m_Ori = Vector3.one;
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
                    m_bGet = m_pHold.GetParamScale(ref m_Ori);
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
            var clipData = frameData.clip.Cast<ScaleClip>();
            if (clipData.lerpTime <= 0)
                m_pHold.SetParamScale(clipData.scale);
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
            if(m_pHold!=null)
            {
                if (clip.CanRestore() || clip.IsLeaveIn())
                {
                    if (m_pHold != null)
                    {
                        if (m_bGet) m_pHold.SetParamScale(m_Ori);
                    }
                }
                m_pHold.SetParamHold(false);
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            CheckObject(pTrack);
            if (m_pHold == null)
                return true;
            var clipData = frameData.clip.Cast<ScaleClip>();
            if (m_pHold != null)
            {
                m_pHold.SetParamHold(true);
                if (clipData.lerpTime > 0 && m_bGet)
                {
                    float lerpTime = Mathf.Min(clipData.lerpTime, clipData.GetDuration());
                    m_pHold.SetParamScale(Vector3.Lerp(m_Ori, clipData.scale, frameData.subTime / lerpTime));
                }
                else
                    m_pHold.SetParamScale(clipData.scale);
            }
            return true;
        }
    }
}