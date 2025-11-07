/********************************************************************
生成日期:	06:30:2025
类    名: 	TimeScaleClip
作    者:	HappLI
描    述:	时间缩放
*********************************************************************/
using Framework.DrawProps;
using UnityEngine;
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
#endif

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("时间缩放Clip")]
    public class TimeScaleClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp baseProp;
        [Display("缩放曲线")] public AnimationCurve curve;
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new TimeScaleDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eTimeScale;
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
        void OnCameraEditor()
        {
        }
#endif
    }
    //-----------------------------------------------------
    //时间缩放驱动逻辑
    //-----------------------------------------------------
    public class TimeScaleDriver : ACutsceneDriver
    {
        private float m_Scale;
        //-----------------------------------------------------
        public override void OnDestroy()
        {
#if UNITY_EDITOR
            if (IsEditorMode())
            {
                var vParams = LockUtil.RestoreParams("Time.timeScale");
                if (vParams.Count > 0)
                {
                    Time.timeScale = (float)vParams[0];
                }
                vParams = LockUtil.RestoreParams("editorTimeScale");
                if (vParams.Count > 0 && GetOwnerEditor()!=null)
                {
                    var editor = GetOwnerEditor() as Framework.ED.EditorWindowBase;
                    if(editor) editor.SetTimeScale((float)vParams[0]);
                }
            }
#endif
        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData clip)
        {
#if UNITY_EDITOR
            if(IsEditorMode())
            {
                var vParams = LockUtil.RestoreParams("Time.timeScale");
                if( vParams.Count>0)
                {
                    Time.timeScale = (float)vParams[0];
                }

                LockUtil.BackupParams("Time.timeScale", Time.timeScale);

                if (GetOwnerEditor() != null)
                {
                    var editor = GetOwnerEditor() as Framework.ED.EditorWindowBase;
                    if (editor) editor.SetTimeScale(1.0f);
                }
            }
#endif
            m_Scale = Time.timeScale;
            Time.timeScale = m_Scale;
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
            if(clip.CanRestore())
            {
                Time.timeScale = m_Scale;
            }
#if UNITY_EDITOR
            if (IsEditorMode())
            {
                var vParams = LockUtil.RestoreParams("Time.timeScale");
                if (vParams.Count > 0)
                {
                    Time.timeScale = (float)vParams[0];
                }
                if (GetOwnerEditor() != null)
                {
                    var editor = GetOwnerEditor() as Framework.ED.EditorWindowBase;
                    if (editor) editor.SetTimeScale(1.0f);
                }
            }
#endif
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            TimeScaleClip timeScaleClip = frameData.clip.Cast<TimeScaleClip>();
            if (timeScaleClip.curve == null || timeScaleClip.curve.length <=0)
                return true;
            float maxTime = timeScaleClip.curve[timeScaleClip.curve.length - 1].time;


            Time.timeScale = timeScaleClip.curve.Evaluate(frameData.subTime / frameData.clip.GetDuration()* maxTime);
#if UNITY_EDITOR
            if (GetOwnerEditor() != null)
            {
                var editor = GetOwnerEditor() as Framework.ED.EditorWindowBase;
                if (editor) editor.SetTimeScale(Time.timeScale);
            }
#endif
            return true;
        }
    }
}