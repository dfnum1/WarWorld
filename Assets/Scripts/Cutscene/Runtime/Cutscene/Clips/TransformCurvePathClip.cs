/********************************************************************
生成日期:	06:30:2025
类    名: 	TransformCurvePathClip
作    者:	HappLI
描    述:	曲线运动动画剪辑
*********************************************************************/
using Framework.DrawProps;
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
using Framework.Cutscene.Editor;
using UnityEditor;
#endif

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("曲线运动Clip")]
    public class TransformCurvePathClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp baseProp;
        [Display("路径朝向")] public bool pathForward = true; //路径朝向
        [Display("缩放")] public bool scaleToggle = true; //缩放
        [Display("朝向")] public bool rotToggle = true;
        [Display("位置")] public bool posToggle = true;
        [Display("地表高度")] public bool terrianHeight = false;
        [Display("路径点")] public PathPoint[] pathPoints; //路径点
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new TransformCurvePathDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eTansCurvePath;
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
        [System.NonSerialized] bool m_bExpandPoints = false;
        [AddInspector]
        public void OnCameraEditor()
        {
            if (this.pathPoints != null)
            {
                m_bExpandPoints = m_bExpandPoints = EditorGUILayout.Foldout(m_bExpandPoints, "路径点列表");
                if (m_bExpandPoints)
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < this.pathPoints.Length; i++)
                    {
                        this.pathPoints[i].position = EditorGUILayout.Vector3Field(GetName() + "路径点" + i, this.pathPoints[i].position);
                    }
                    EditorGUI.indentLevel--;
                }
            }

            UnityEditor.EditorGUILayout.LabelField("---------------------------------------------------------------------");
            CurvePathEditor.editTest = EditorGUILayout.ObjectField("编辑对象", CurvePathEditor.editTest, typeof(Transform), true) as Transform;
            if(CurvePathEditor.editTest == null && CurvePathEditor.isEditing)
            {
                var binder = baseProp.ownerTrackObject.GetBindLastCutsceneObject();
                CurvePathEditor.editCutsceneObject = binder;
                if (binder!=null) CurvePathEditor.editTest = binder.GetUniyTransform();
            }
            if (GUILayout.Button("编辑运行曲线"))
            {
                CurvePathEditor.StartEdit(pathPoints, (paths) => {
                    pathPoints = paths.ToArray();
                }, GetDuration());
            }
            CurvePathEditor.SetPreviewDuration(GetDuration(), pathForward);
        }
#endif
    }
    //-----------------------------------------------------
    //相机移动驱动逻辑
    //-----------------------------------------------------
    public class TransformCurvePathDriver : ACutsceneDriver
    {
        private ICutsceneObject m_pHoldObject = null;
        private Transform m_pTransform;
        private Vector3 m_OrigPos;
        private Vector3 m_OrigScale;
        private Quaternion m_OrigRot;

        private PathCurve m_pCurve;
        bool m_bController = false;
        //-----------------------------------------------------
        public override void OnDestroy()
        {
            m_pCurve.Dispose();

            if (m_bController)
            {
    			ControllerRefUtil.UnControll(m_pTransform);
            	m_bController = false;
			}
            m_pTransform = null;
            m_pHoldObject = null;
        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData clip)
        {
            m_pHoldObject = null;
            m_pTransform = null;
            CheckBindObj(pTrack, clip);
            var clipData = clip.clip.Cast<TransformCurvePathClip>();
            var points = clipData.pathPoints;
            m_pCurve.Set(points);
            return true;
        }
        //-----------------------------------------------------
        void CheckBindObj(CutsceneTrack pTrack, FrameData clip)
        {
            if(m_pHoldObject != null || m_pTransform != null)
            {
                return;
            }
            var bindObj = pTrack.GetBindLastCutsceneObject();
            if (bindObj == null) return;
            m_pHoldObject = bindObj;
            var unityObj = bindObj.GetUniyObject();
            if (unityObj)
            {
                bindObj.SetParamHold(true);
                if (unityObj is Transform)
                    m_pTransform = unityObj as Transform;
                else if (unityObj is GameObject)
                    m_pTransform = ((GameObject)unityObj).transform;

            }

            if (m_pHoldObject != null)
            {
                m_pHoldObject.GetParamPosition(ref m_OrigPos);
                m_pHoldObject.GetParamQuaternion(ref m_OrigRot);
                m_pHoldObject.GetParamScale(ref m_OrigScale);
            }

            if (m_pTransform != null)
            {
                m_OrigPos = m_pTransform.position;
                m_OrigScale = m_pTransform.localScale;
                m_OrigRot = m_pTransform.rotation;
            }
            ControllerRefUtil.ControllRef(m_pTransform);
            m_bController = true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
            var bindObj = pTrack.GetBindLastCutsceneObject();
            if (bindObj != null)
                bindObj.SetParamHold(false);
            if (m_pHoldObject != null && m_pHoldObject != bindObj)
                m_pHoldObject.SetParamHold(false);
            if (clip.CanRestore())
            {
                if (m_pTransform != null)
                {
                    m_pTransform.position = m_OrigPos;
                    m_pTransform.rotation = m_OrigRot;
                    m_pTransform.localScale = m_OrigScale;
                }
                if(m_pHoldObject!=null)
                {
                    m_pHoldObject.SetParamPosition(m_OrigPos);
                    m_pHoldObject.SetParamQuaternion(m_OrigRot);
                    m_pHoldObject.SetParamScale(m_OrigScale);
                }
            }
            m_pCurve.Dispose();
            if(m_bController)
            {   
	 			ControllerRefUtil.UnControll(m_pTransform);
            	m_bController = false;
			}
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
#if UNITY_EDITOR
            if (IsEditorMode() && (ControllerRefUtil.IsEditorObject(m_pTransform) || ControllerRefUtil.IsEditorObject(m_pHoldObject)))
                return true;
#endif
            var clipData = frameData.clip.Cast<TransformCurvePathClip>();
            var points = clipData.pathPoints;
            if (points == null) return true;
            CheckBindObj(pTrack, frameData);
            float duration = clipData.GetDuration();
            float t = Mathf.Clamp01(frameData.subTime / Mathf.Max(duration, 0.01f));

            if (m_pCurve.Evaluate(t, out var point, clipData.pathForward))
            {
                if (m_pTransform)
                {
                    if (clipData.posToggle) m_pTransform.position = point.position;
                    if (clipData.scaleToggle) m_pTransform.localScale = point.scale;
                    if(point.useRot && clipData.rotToggle) m_pTransform.rotation = point.rot;
                }
                if (m_pHoldObject != null)
                {
                    m_pHoldObject.SetParamHold(true);
                    if (clipData.posToggle) m_pHoldObject.SetParamPosition(point.position);
                    if (point.useRot && clipData.rotToggle) m_pHoldObject.SetParamQuaternion(point.rot);
                    if(clipData.scaleToggle) m_pHoldObject.SetParamScale(point.scale);
                    if (clipData.terrianHeight) m_pHoldObject.SetParamTerrainHeightCheck();
                }
            }
            return true;
        }
    }
}