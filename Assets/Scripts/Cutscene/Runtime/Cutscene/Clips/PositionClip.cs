/********************************************************************
生成日期:	06:30:2025
类    名: 	PositionClip
作    者:	HappLI
描    述:	坐标位置Clip用于控制相机位置变化
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
using UnityEditor;
using static UnityEditor.Handles;
#endif
using Framework.DrawProps;
using UnityEngine;
using UnityEngine.Playables;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("位置Clip")]
    public class PositionClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp       baseProp;
        [Display("位置")] public Vector3 position = Vector3.zero;
        [Display("过渡时长")] public float lerpTime = 0.5f; //位置过渡时间
        [Display("地表高度")] public bool terrianHeight = false; //地表高度检测
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new PositionDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eSetPosition;
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
            EditorGUI.BeginChangeCheck();
            position = Handles.DoPositionHandle(position, Quaternion.identity);
            Handles.Label(position, "位置");
            if(EditorGUI.EndChangeCheck())
            {
                if (baseProp.ownerTrackObject != null)
                {
                    var binder = baseProp.ownerTrackObject.GetBindLastCutsceneObject();
                    if (binder != null)
                    {
                        binder.SetParamPosition(position);
                    }
                }
            }
        }
#endif
    }
    //-----------------------------------------------------
    //位置剪辑逻辑
    //-----------------------------------------------------
    public class PositionDriver : ACutsceneDriver
    {
        private bool m_bGetPos = false;
        private Vector3 m_Ori = Vector3.zero;
        private ICutsceneObject m_pHold;
        //-----------------------------------------------------
        public override void OnDestroy()
        {
            if (m_pHold != null)
            {
                m_pHold.SetParamHold(false);
                m_pHold = null;
            }
        }
        //-----------------------------------------------------
        void CheckObject(CutsceneTrack pTrack)
        {
            if (m_pHold == null)
            {
                m_pHold = pTrack.GetBindLastCutsceneObject();
                if (m_pHold != null)
                {
                    m_bGetPos = m_pHold.GetParamPosition(ref m_Ori);
                }
            }
        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData frameData)
        {
            m_bGetPos = false;
            CheckObject(pTrack);
            if (m_pHold == null) return true;
            m_pHold.SetParamHold(true);
            var clipData = frameData.clip.Cast<PositionClip>();
            if (clipData.lerpTime <= 0)
                m_pHold.SetParamPosition(clipData.position);
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData frameData)
        {
            if (frameData.CanRestore() || frameData.IsLeaveIn())
            {
                if (m_pHold!=null)
                {
                    if(m_bGetPos)m_pHold.SetParamPosition(m_Ori);
                    m_pHold.SetParamHold(false);
                }
            }
            m_bGetPos = false;
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            CheckObject(pTrack);
            if (m_pHold == null)
                return true;
            var clipData = frameData.clip.Cast<PositionClip>();
            if (m_pHold!=null)
            {
                m_pHold.SetParamHold(true);
                if (clipData.lerpTime>0 && m_bGetPos)
                {
                    float lerpTime = Mathf.Min(clipData.lerpTime, clipData.GetDuration());
                    m_pHold.SetParamPosition(Vector3.Lerp(m_Ori, clipData.position,frameData.subTime/ lerpTime));
                }
                else
                    m_pHold.SetParamPosition(clipData.position);
                if (clipData.terrianHeight) m_pHold.SetParamTerrainHeightCheck();
            }
            return true;
        }
    }
}