/********************************************************************
生成日期:	06:30:2025
类    名: 	FollowToClip
作    者:	HappLI
描    述:	跟随剪辑
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
#endif
using Framework.DrawProps;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("跟随Clip")]
    public class FollowToClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp       baseProp;
        [Display("相机跟随")] public bool               camera = true;
        [Display("跟随者"),DrawProps.StateByField("camera", "false"),DisplayDrawType(typeof(BinderUnityObject))] 
        public int                  follower; //跟随者
        [Display("跟随偏移")] public Vector3            followOffset; //跟随偏移量
        [Display("跟随角度偏移")] public Vector3 followRotOffset; //跟随角度偏移量
        [Display("跟随速度", "如果<=0,表示紧紧跟随，没有过渡过程")]
        public float followSpeed = 0.0f; //跟随速度   

        [Display("保持相对角度"), StateByField("camera", "false")]
        public bool keepReleationRot = false;                       
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new FollowToDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eFollowTo;
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
        [System.NonSerialized] Transform m_pFollowTarget = null;
        [System.NonSerialized] Transform m_pBinderFollowTarget = null;
        [AddInspector]
        public void OnCameraEditor()
        {
            UnityEditor.EditorGUILayout.LabelField("---------------------------------------------------------------------");
            m_pBinderFollowTarget = null;
            if (baseProp.ownerTrackObject!=null)
            {
                var binder = baseProp.ownerTrackObject.GetBindLastCutsceneObject();
                if(binder!=null)
                {
                    m_pBinderFollowTarget = binder.GetUniyTransform();
                }
            }
            if(m_pBinderFollowTarget!=null)
            {
                UnityEditor.EditorGUILayout.ObjectField("跟随目标", m_pBinderFollowTarget, typeof(Transform), true);
            }
            else
                m_pFollowTarget = UnityEditor.EditorGUILayout.ObjectField("跟随目标", m_pFollowTarget, typeof(Transform), true) as Transform;
            Transform pTransform = m_pBinderFollowTarget;
            if (pTransform == null) pTransform = m_pFollowTarget;
            if(pTransform == null)
            {
                UnityEditor.EditorGUILayout.HelpBox("请设置跟随的编辑目标", UnityEditor.MessageType.Warning);
            }
            if (GUILayout.Button("设置当前跟随参数"))
            {
                if(pTransform!=null)
                {
                    if(this.camera)
                    {
                        var camera = Camera.main;
                        if (camera)
                        {
                            followOffset = camera.transform.position - pTransform.position;
                            followRotOffset = (Quaternion.Inverse(pTransform.rotation) * camera.transform.rotation).eulerAngles;
                        }
                    }
                    else
                    {
                         var binder = ObjectBinderUtils.GetBinder(this.follower);
                        if (binder.IsValid() && binder.GetUniyTransform())
                        {
                            followOffset = binder.GetUniyTransform().position - pTransform.position;
                            followRotOffset = (Quaternion.Inverse(pTransform.rotation) * binder.GetUniyTransform().rotation).eulerAngles;
                        }
                    }
                }
            }
        }
#endif
    }
    //-----------------------------------------------------
    //FollowToDriver
    //-----------------------------------------------------
    public class FollowToDriver : ACutsceneDriver
    {
        private ICutsceneObject m_pFollowObj = null;//跟随者
        private Vector3 m_FollowerPos = Vector3.zero;
        private Vector3 m_FollowerEulerAngle = Vector3.zero;

        private Vector3 m_TargetPos = Vector3.zero;
        private Vector3 m_TargetEulerAngle = Vector3.zero;
        private ICutsceneObject m_pFollowTargetObj = null;//跟随目标
        private Transform m_pFollowTarget;
        private Transform m_pFollower;
        private Camera m_pCamera;
        private bool m_bControlled = false;
        //-----------------------------------------------------
        public override void OnDestroy()
        {
            m_pFollowTarget = null;
            if (m_bControlled)
            {
                m_bControlled = false;
                ControllerRefUtil.UnControll(m_pCamera);
            }
            m_pCamera = null;
        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData clip)
        {
            FollowToClip followTo = clip.clip.Cast<FollowToClip>();

            m_pCamera = null;
            m_pFollower = null;
            if (followTo.camera)
            {
                var camera = Camera.main;
                m_pCamera = camera;
                if(!m_bControlled)
                {
                    ControllerRefUtil.Controll(camera);
                    m_bControlled = true;
                }
                if (camera) m_pFollower = camera.transform;
            }
            else
            {
                m_pFollowObj = ObjectBinderUtils.GetBinder(followTo.follower);
                if(m_pFollowObj!=null)
                {
                    m_pFollower = m_pFollowObj.GetUniyTransform();
                }
            }

            if (m_pFollowObj!=null)
            {
                m_pFollowObj.GetParamPosition(ref m_FollowerPos);
                m_pFollowObj.GetParamEulerAngle(ref m_FollowerEulerAngle);
            }
            if (m_pFollower)
            {
                m_FollowerPos = m_pFollower.position;
                m_FollowerEulerAngle = m_pFollower.eulerAngles;
            }
            m_pFollowTargetObj = pTrack.GetBindLastCutsceneObject();
            if (m_pFollowTargetObj != null)
            {
                var obj = m_pFollowTargetObj.GetUniyObject() as GameObject;
                if (obj!=null)
                {
                    m_pFollowTarget = obj.transform;
                }
            }
            if (m_pFollowTargetObj!=null)
            {
                m_pFollowTargetObj.GetParamPosition(ref m_TargetPos);
                m_pFollowTargetObj.GetParamEulerAngle(ref m_TargetEulerAngle);
            }
            if (m_pFollowTarget)
            {
                m_TargetPos = m_pFollowTarget.position;
                m_TargetEulerAngle = m_pFollowTarget.eulerAngles;
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
            if(clip.CanRestore())
            {
                if (m_pFollowTarget)
                {
                    m_pFollowTarget.position = m_TargetPos;
                    m_pFollowTarget.eulerAngles = m_TargetEulerAngle;
                }
                if (m_pFollowTargetObj!=null)
                {
                    m_pFollowTargetObj.SetParamPosition(m_TargetPos);
                    m_pFollowTargetObj.SetParamEulerAngle(m_TargetEulerAngle);
                }
                if (m_pFollowObj!=null)
                {
                    m_pFollowObj.SetParamPosition(m_FollowerPos);
                    m_pFollowObj.SetParamEulerAngle(m_FollowerEulerAngle);
                }
                if (m_pFollower)
                {
                    m_pFollower.position = m_FollowerPos;
                    m_pFollower.eulerAngles = m_FollowerEulerAngle;
                }
            }
            if(m_bControlled)
            {
                m_bControlled = false;
                ControllerRefUtil.UnControll(m_pCamera);
            }
            m_pCamera = null;
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            if (m_pFollowTarget == null && m_pFollowTargetObj == null)
                return true;
#if UNITY_EDITOR
            if (IsEditorMode() && (ControllerRefUtil.IsEditorObject(m_pFollower) ||
                ControllerRefUtil.IsEditorObject(m_pFollower)))
                return true;
#endif

            var clipData = frameData.clip.Cast<FollowToClip>();

            Vector3 finalPos = clipData.followOffset;
            Quaternion finalRot = Quaternion.identity;

            bool keepRot = clipData.keepReleationRot && !clipData.camera;
            if (m_pFollowTarget)
            {
                finalPos = m_pFollowTarget.position + clipData.followOffset;
                if (keepRot)
                {
                    finalRot = m_pFollowTarget.rotation * Quaternion.Euler(clipData.followRotOffset);
                }
            }
            else if (m_pFollowTargetObj != null)
            {
                m_pFollowTargetObj.GetParamPosition(ref finalPos);
                m_pFollowTargetObj.GetParamQuaternion(ref finalRot);
                finalPos += clipData.followOffset;
                if (keepRot)
                {
                    finalRot *= Quaternion.Euler(clipData.followRotOffset);
                }
            }

            if (clipData.followSpeed <= 0.0f)
            {
                if (m_pFollowObj != null)
                {
                    m_pFollowObj.SetParamPosition(finalPos);
                    if (keepRot) m_pFollowObj.SetParamQuaternion(finalRot);
                }
                else if (m_pFollower)
                {
                    m_pFollower.position = finalPos;
                    if (keepRot) m_pFollower.rotation = finalRot;
                }
            }
            else
            {
                if (m_pFollowObj != null)
                {
                    var pos = Vector3.Lerp(m_pFollower.position, finalPos, frameData.deltaTime * clipData.followSpeed);
                    if (!m_pFollowObj.SetParamPosition(pos))
                    {
                        if (m_pFollower) m_pFollower.position = pos;
                    }
                    if (keepRot)
                    {
                        Quaternion rot = Quaternion.Slerp(m_pFollower.rotation, finalRot, frameData.deltaTime * clipData.followSpeed);
                        if (!m_pFollowObj.SetParamQuaternion(rot))
                        {
                            if (m_pFollower) m_pFollower.rotation = rot;
                        }
                    }
                }
                else
                {
                    if (m_pFollower)
                    {
                        m_pFollower.position = Vector3.Lerp(m_pFollower.position, finalPos, frameData.deltaTime * clipData.followSpeed);
                        if (keepRot) m_pFollower.rotation = Quaternion.Slerp(m_pFollower.rotation, finalRot, frameData.deltaTime * clipData.followSpeed);
                    }
                }
            }
            return true;
        }
    }
}