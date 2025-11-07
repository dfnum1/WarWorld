/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneGroupBindObjectFollowTargetClip
作    者:	HappLI
描    述:	跟随剪辑
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
using System.Reflection;
using UnityEditor;
#endif
using Framework.DrawProps;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("目标组跟随Clip")]
    public class CutsceneGroupBindObjectFollowTargetClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp       baseProp;
        [Display("跟随目标"),RowFieldInspector("OnCutsceneGroupSelect")] 
        public ushort                  followGroup; //跟组
        [Display("跟随目标绑点"), RowFieldInspector("OnSelectBindSlot")]
        public string followBindSlot;
        [Display("跟随偏移")] public Vector3            followOffset; //跟随偏移量
        [Display("跟随角度偏移")] public Vector3 followRotOffset; //跟随角度偏移量
        [Display("跟随速度", "如果<=0,表示紧紧跟随，没有过渡过程")]
        public float followSpeed = 0.0f; //跟随速度   

        [Display("保持相对角度")]
        public bool keepReleationRot = false;

        [Display("设置为绑点子节点"), StateByField("followBindSlot","",false)]
        public bool setAsBindSlotChild = false; //设置为绑点子节点
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new CutsceneGroupBindObjectFollowTargetDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eFollowTargetGroup;
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
        static List<string> m_vPopCutsceneGpNames = new List<string>();
        static List<ushort> m_vPopCutsceneGpIds = new List<ushort>();
        public void OnCutsceneGroupSelect(System.Object pOwner, System.Reflection.FieldInfo fieldInfo)
        {
            if (fieldInfo.Name != "followGroup")
                return;
            if (baseProp.ownerTrackObject == null || baseProp.ownerTrackObject.GetPlayable() == null)
                return;

            m_vPopCutsceneGpNames.Clear();
            m_vPopCutsceneGpIds.Clear();
            baseProp.ownerTrackObject.GetPlayable().GetGroupNames(m_vPopCutsceneGpNames, m_vPopCutsceneGpIds, baseProp.ownerTrackObject.GetGroupId());

            int index= UnityEditor.EditorGUILayout.Popup(m_vPopCutsceneGpIds.IndexOf(followGroup), m_vPopCutsceneGpNames.ToArray());
            if (index >= 0 && index < m_vPopCutsceneGpIds.Count)
            {
                if(followGroup != baseProp.ownerTrackObject.GetGroupId())
                {
                    followGroup = m_vPopCutsceneGpIds[index];
                }
            }
            else followGroup = ushort.MaxValue;
        }
        //-----------------------------------------------------
        [NonSerialized] List<string> m_vEndBindPops = new List<string>();
        [NonSerialized] private Transform m_pEndLastTransfrom = null;
        public void OnSelectBindSlot(System.Object pOwner, System.Reflection.FieldInfo fieldInfo)
        {
            if (fieldInfo.Name != "followBindSlot")
                return;
            if (baseProp.ownerTrackObject == null)
                return;
            var bindObj = baseProp.ownerTrackObject.GetPlayable().GetBindLastCutsceneObject(this.followGroup);
            if (bindObj == null)
                return;

            var objTrans = bindObj?.GetUniyTransform();

            List<string> vBindPops = null;
            Transform lastTransform = null;
            lastTransform = m_pEndLastTransfrom;
            vBindPops = m_vEndBindPops;
            if (lastTransform != objTrans)
            {
                lastTransform = objTrans;
                vBindPops.Clear();
                vBindPops.Add("");
                if (objTrans)
                {
                    EditorUtil.AddAllChildPaths(objTrans, "", vBindPops);
                }
            }
            m_pEndLastTransfrom = lastTransform;
            if (objTrans == null)
                return;

            string bindNode = (string)fieldInfo.GetValue(this);

            int nSelect = m_vEndBindPops.IndexOf(bindNode);
            if (nSelect < 0 && string.IsNullOrEmpty(bindNode)) nSelect = 0;
            nSelect = EditorGUILayout.Popup("", nSelect, vBindPops.ToArray(), new GUILayoutOption[] { GUILayout.Width(80) });
            if (nSelect >= 0 && nSelect < vBindPops.Count)
            {
                bindNode = vBindPops[nSelect];
            }
            fieldInfo.SetValue(this, bindNode);
        }
        //-----------------------------------------------------
        [AddInspector]
        public void OnCameraEditor()
        {
            if (baseProp.ownerTrackObject == null)
            {
                return;
            }
            CutsceneGroupBindObjectFollowTargetDriver driver = null;
            ICutsceneObject pFollower = null;
            ICutsceneObject pFollowTarget = null;
            var drivers = baseProp.ownerTrackObject.GetDrivers(this);
            if (drivers != null)
            {
                foreach (var db in drivers)
                {
                    if (db is CutsceneGroupBindObjectFollowTargetDriver)
                    {
                        driver = db as CutsceneGroupBindObjectFollowTargetDriver;
                        pFollower = driver.GetFollower();
                        pFollowTarget = driver.GetFollowTarget();
                        break;
                    }
                }
            }

            UnityEditor.EditorGUILayout.LabelField("---------------当前跟随信息-----------------------------");
            pFollower.ObjectField("跟随者");
            pFollowTarget.ObjectField("跟随目标");
            if (pFollower!=null && pFollowTarget != null && driver.IsEnter())
            {
                if (GUILayout.Button(driver.IsEditor()? "退出编辑" : "进入编辑"))
                {
                    driver.EditorFollow(!driver.IsEditor());
                }
                if (driver.IsEditor() && GUILayout.Button("设置当前跟随参数"))
                {
                    Vector3 positionTarget = Vector3.zero;
                    Quaternion rotTarget = Quaternion.identity;
                    pFollowTarget.GetParamPosition(ref positionTarget);
                    pFollowTarget.GetParamQuaternion(ref rotTarget);

                    if(!string.IsNullOrEmpty(this.followBindSlot) )
                    {
                        var targetTrans = pFollowTarget.GetUniyTransform();
                        if (targetTrans != null)
                        {
                            var bindTrans = CutsceneUtil.Find(targetTrans,this.followBindSlot);
                            if (bindTrans != null)
                            {
                                positionTarget = bindTrans.position;
                                rotTarget = bindTrans.rotation;
                            }
                        }
                        else
                        {
                            if (pFollowTarget.GetParamBindSlotMatrix(this.followBindSlot, out var bindSlotMatrix))
                            {
                                positionTarget = CutsceneUtil.GetPosition(bindSlotMatrix);
                                rotTarget = bindSlotMatrix.rotation;
                            }
                        }
                    }

                    Vector3 position = Vector3.zero;
                    Quaternion rot = Quaternion.identity;
                    pFollower.GetParamPosition(ref position);
                    pFollower.GetParamQuaternion(ref rot);

                    followOffset = position - positionTarget;
                    if (keepReleationRot)
                        followRotOffset = (Quaternion.Inverse(rotTarget) * rot).eulerAngles;
                    else
                        followRotOffset = rot.eulerAngles;
                }
            }
        }
#endif
    }
    //-----------------------------------------------------
    //CutsceneGroupBindObjectFollowTargetDriver
    //-----------------------------------------------------
    public class CutsceneGroupBindObjectFollowTargetDriver : ACutsceneDriver
    {
        private ICutsceneObject m_pFollowerObj = null;//跟随者
        private Vector3 m_FollowerPos = Vector3.zero;
        private Vector3 m_FollowerEulerAngle = Vector3.zero;
        private ICutsceneObject m_pFollowTargetObj = null;//跟随目标
        private string m_LastBindSlot = null;
        private Transform m_BindSlotTransform = null;
        private Transform m_pBackupTransformParent = null;
#if UNITY_EDITOR
        private bool m_bEditor = false;
        private bool m_bEnter = false;
        public void EditorFollow(bool bEdit)
        {
            bool bLastEditor = m_bEditor;
            m_bEditor = bEdit;
            ControllerRefUtil.SetEditingObject(bEdit?m_pFollowerObj:null);
            if (m_bEditor && m_pFollowerObj!=null)
            {
                Vector3 paramPos = Vector3.zero;
                m_pFollowerObj.GetParamPosition(ref paramPos);
                SceneView.lastActiveSceneView?.LookAt(paramPos);
            }
            if(m_bEditor)SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("正在编辑中...."), float.MaxValue-1);
            else if(bLastEditor) SceneView.lastActiveSceneView?.ShowNotification(new GUIContent("退出编辑."), 0.8f);
        }
        //-----------------------------------------------------
        public bool IsEditor()
        {
            return m_bEditor;
        }
        //-----------------------------------------------------
        public bool IsEnter()
        {
            return m_bEnter;
        }
        //-----------------------------------------------------
        void OnSceneEditor(UnityEditor.SceneView sceneView)
        {
            if (m_pFollowerObj == null || !m_bEditor) return;
            Vector3 paramPos = Vector3.zero;
            Quaternion paramEuler = Quaternion.identity;
            Vector3 paramScale = Vector3.one;
            m_pFollowerObj.GetParamPosition(ref paramPos);
            m_pFollowerObj.GetParamQuaternion(ref paramEuler);
            m_pFollowerObj.GetParamScale(ref paramScale);
            if (Tools.current == Tool.Scale)
            {
                m_pFollowerObj.SetParamScale(Handles.DoScaleHandle(paramScale, paramPos, paramEuler, HandleUtility.GetHandleSize(paramPos)));
            }
            else if (Tools.current == Tool.Rotate)
            {
                m_pFollowerObj.SetParamQuaternion(Handles.DoRotationHandle(paramEuler, paramPos));
            }
            else
            {
                m_pFollowerObj.SetParamPosition(Handles.DoPositionHandle(paramPos, paramEuler));
            }
            Selection.activeTransform = null;
            if(Event.current.type == EventType.KeyUp)
            {
                if(Event.current.keyCode == KeyCode.Escape)
                {
                    EditorFollow(false);
                }
            }
        }
#endif
        //-----------------------------------------------------
        public override void OnDestroy()
        {
            if(m_pBackupTransformParent!=null && m_pFollowerObj!=null)
            {
                m_pFollowerObj.GetUniyTransform()?.SetParent(m_pBackupTransformParent, false);    
            }
			m_pFollowerObj = null;
            m_pBackupTransformParent = null;
            m_BindSlotTransform = null;
            m_LastBindSlot = null;
        }
        //-----------------------------------------------------
        public ICutsceneObject GetFollower()
        {
            return m_pFollowerObj;
        }
        //-----------------------------------------------------
        public ICutsceneObject GetFollowTarget()
        {
            return m_pFollowTargetObj;
        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData clip)
        {
            CutsceneGroupBindObjectFollowTargetClip followTarget = clip.clip.Cast<CutsceneGroupBindObjectFollowTargetClip>();

            m_pBackupTransformParent = null;
            m_pFollowTargetObj = pTrack.GetCutscene().GetGroupBindLastCutsceneObject(followTarget.followGroup);
            m_pFollowerObj = pTrack.GetBindLastCutsceneObject();

            if (m_pFollowerObj != null)
            {
                if(followTarget.setAsBindSlotChild)
                    m_pBackupTransformParent = m_pFollowerObj.GetUniyTransform()?.parent;
                m_pFollowerObj.GetParamPosition(ref m_FollowerPos);
                m_pFollowerObj.GetParamEulerAngle(ref m_FollowerEulerAngle);
            }
#if UNITY_EDITOR
            m_bEnter = true;
            if(IsEditorMode()) SceneView.duringSceneGui += OnSceneEditor;
#endif
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
            if(clip.CanRestore())
            {
                if (m_pBackupTransformParent != null && m_pFollowerObj != null)
                {
                    m_pFollowerObj.GetUniyTransform()?.SetParent(m_pBackupTransformParent);
                }
                m_pBackupTransformParent = null;
                if (m_pFollowerObj!=null)
                {
                    m_pFollowerObj.SetParamPosition(m_FollowerPos);
                    m_pFollowerObj.SetParamEulerAngle(m_FollowerEulerAngle);
                }
                m_pFollowerObj = null;
            }
#if UNITY_EDITOR
            m_bEnter = false;
            EditorFollow(false);
            SceneView.duringSceneGui -= OnSceneEditor;
#endif
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            if (m_pFollowerObj == null || m_pFollowTargetObj == null)
            {
                return true;
            }
#if UNITY_EDITOR
            if (m_bEditor || (IsEditorMode() && ControllerRefUtil.IsEditorObject(m_pFollowerObj)))
                return true;
#endif

            var clipData = frameData.clip.Cast<CutsceneGroupBindObjectFollowTargetClip>();

            Vector3 finalPos = clipData.followOffset;
            Quaternion finalRot = Quaternion.identity;

            bool keepRot = clipData.keepReleationRot;

            m_pFollowTargetObj.GetParamPosition(ref finalPos);
            m_pFollowTargetObj.GetParamQuaternion(ref finalRot);

            if( !string.IsNullOrEmpty(clipData.followBindSlot) )
            {
                if (m_LastBindSlot != clipData.followBindSlot)
                {
                    m_LastBindSlot = clipData.followBindSlot;
                    m_BindSlotTransform = null;
                    var targetTrans = m_pFollowTargetObj.GetUniyTransform();
                    if (targetTrans != null)
                    {
                        var bindTrans = CutsceneUtil.Find(targetTrans,clipData.followBindSlot);
                        if (bindTrans != null)
                        {
                            m_BindSlotTransform = bindTrans;
                            if (m_BindSlotTransform != null)
                            {
                                if(clipData.setAsBindSlotChild && m_pBackupTransformParent)
                                {
                                    var transform = m_pFollowerObj.GetUniyTransform();
                                    if (transform)
                                    {
                                        transform.SetParent(m_BindSlotTransform.parent);
                                    }
                                }
                            }
                        }
                    }
                }
                if (m_BindSlotTransform != null)
                {
                    finalPos = m_BindSlotTransform.position;
                    finalRot = m_BindSlotTransform.rotation;
                }
                else
                {
                    if(m_pFollowTargetObj.GetParamBindSlotMatrix(clipData.followBindSlot, out var  bindSlotMatrix))
                    {
                        finalPos = CutsceneUtil.GetPosition(bindSlotMatrix);
                        finalRot = bindSlotMatrix.rotation;
                    }
                }
            }

            finalPos += clipData.followOffset;
            if (keepRot)
            {
                finalRot *= Quaternion.Euler(clipData.followRotOffset);
            }
            if (clipData.followSpeed <= 0.0f)
            {
                m_pFollowerObj.SetParamPosition(finalPos);
                if (keepRot) m_pFollowerObj.SetParamQuaternion(finalRot);
            }
            else
            {
                Vector3 followerPos = Vector3.zero;
                Quaternion followerRot = Quaternion.identity;
                m_pFollowerObj.GetParamPosition(ref followerPos);
                m_pFollowerObj.GetParamQuaternion(ref followerRot);

                var pos = Vector3.Lerp(followerPos, finalPos, frameData.deltaTime * clipData.followSpeed);
                m_pFollowerObj.SetParamPosition(pos);
                if (keepRot)
                {
                    Quaternion rot = Quaternion.Slerp(followerRot, finalRot, frameData.deltaTime * clipData.followSpeed);
                    m_pFollowerObj.SetParamQuaternion(rot);
                }
            }
            return true;
        }
    }
}