/********************************************************************
生成日期:	06:30:2025
类    名: 	ProjecitleClip
作    者:	HappLI
描    述:	弹道剪辑
*********************************************************************/
using Framework.DrawProps;
using System.Collections.Generic;
using UnityEngine;
using System;


#if UNITY_EDITOR
using Framework.ED;
using System.Reflection;
using UnityEditor;
using Framework.Cutscene.Editor;
#endif
namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("弹道Clip")]
    public class ProjecitleClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp baseProp;

        [Display("开始预制体"), StringViewPlugin("OnDrawSelectPrefabInspector")]
        public string startPrefab;
        [Display("发射挂点"),RowFieldInspector("OnSelectBindSlot")] public string startBindSlot;
        [Display("发射位置偏移"), DisplayNameByField("startBindSlot", "null", "发射位置")] public Vector3 startPosition;
        [Display("发射旋转偏移"), DisplayNameByField("startBindSlot", "null", "发射旋转")] public Vector3 startRotate;
        [Display("发射缩放")] public Vector3 startScale = Vector3.one;
        [Display("发射点曲率")] public Vector3 startTan = Vector3.zero;


        [Display("发射中预制"), StringViewPlugin("OnDrawSelectPrefabInspector")]
        public string luanchingPrefab;
        [Display("发射中-自旋转")] public AnimationCurve launchRotate;
        [Display("发射中-自缩放")] public AnimationCurve launchScale;


        [Display("结束预制体"), StringViewPlugin("OnDrawSelectPrefabInspector")]
        public string endPrefab;
        [Display("结束挂点"), RowFieldInspector("OnSelectEndBindSlot")] public string endBindSlot;
        [Display("结束位置偏移"), DisplayNameByField("endBindSlot", "null", "结束位置")] public Vector3 endPosition;
        [Display("结束旋转偏移"), DisplayNameByField("endBindSlot", "null", "结束旋转")] public Vector3 endRotate;
        [Display("结束缩放")] public Vector3 endScale = Vector3.one;
        [Display("结束点曲率")] public Vector3 endTan = Vector3.zero;

        [Display("弹道目标"), RowFieldInspector("OnCutsceneGroupSelect")]
        public ushort targetGroup; //跟组

        [Display("异步加载")] public bool asyncLoad = true;
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new ProjecitleDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eProjecitle;
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
        public void OnSceneView(SceneView sceneView)
        {
            if (baseProp.ownerTrackObject == null)
                return;
            var sourceObj = baseProp.ownerTrackObject.GetBindLastCutsceneObject();
            var targetObj = baseProp.ownerTrackObject.GetCutscene().GetGroupBindLastCutsceneObject(targetGroup);
            if (sourceObj == null || targetObj == null)
                return;
            var sourceTrans = sourceObj.GetUniyTransform();
            var targetTrans = targetObj.GetUniyTransform();
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.zero;
            Transform startTrans = null;
            Transform endTrans = null;
            Vector3 sourcePos = Vector3.zero;
            Vector3 targetPos = Vector3.zero;
            sourceObj.GetParamPosition(ref sourcePos);
            targetObj.GetParamPosition(ref targetPos);
            if (!string.IsNullOrEmpty(this.startBindSlot) && sourceTrans!=null)
            {
                startTrans = sourceTrans.Find(this.startBindSlot);
                if (startTrans != null)
                    startPos = startTrans.position + this.startPosition;
                else
                    startPos = sourcePos + this.startPosition;
            }
            else
            {
                startPos = sourcePos + this.startPosition;
            }
            if (!string.IsNullOrEmpty(this.endBindSlot) && targetTrans)
            {
                endTrans = targetTrans.Find(this.endBindSlot);
                if (endTrans != null)
                    endPos = endTrans.position + this.endPosition;
                else
                    endPos = targetPos + this.endPosition;
            }
            else
            {
                endPos = targetPos + this.endPosition;
            }
            if (!Event.current.control)
            {
                Handles.color = Color.yellow;
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.DoPositionHandle(startPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                    this.startPosition = newPos - sourcePos;

                EditorGUI.BeginChangeCheck();
                newPos = Handles.DoPositionHandle(endPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    this.endPosition = newPos - targetPos;
                }
            }
 
            Handles.SphereHandleCap(0, startPos, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.SphereHandleCap(0, endPos, Quaternion.identity, 0.1f, EventType.Repaint);

            Vector3 startTanPos = startPos + this.startTan;
            Vector3 endTanPos = endPos + this.endTan;

            if (Event.current.control)
            {
                EditorGUI.BeginChangeCheck();
                startTanPos = Handles.PositionHandle(startTanPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    this.startTan = startTanPos - startPos;
                }

                EditorGUI.BeginChangeCheck();
                endTanPos = Handles.PositionHandle(endTanPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    this.endTan = endTanPos - endPos;
                }
            }

            Handles.DrawBezier(startPos, endPos, startTanPos, endTanPos, Color.green, null, 5f);
        }
        //-----------------------------------------------------
        [NonSerialized] List<string> m_vStartBindPops = new List<string>();
        [NonSerialized]private Transform m_pStartLastTransfrom = null;
        public void OnSelectBindSlot(System.Object pOwner, System.Reflection.FieldInfo fieldInfo)
        {
            if (fieldInfo.Name != "startBindSlot")
                return;
            if (baseProp.ownerTrackObject == null)
                return;
            var iObj = baseProp.ownerTrackObject.GetBindLastCutsceneObject();
            if (iObj == null)
                return;

            var objTrans = iObj?.GetUniyTransform();

            List<string> vBindPops = null;
            Transform lastTransform = null;

                lastTransform = m_pStartLastTransfrom;
                vBindPops = m_vStartBindPops;
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
            m_pStartLastTransfrom = lastTransform;
            if (objTrans == null)
                return;

            string bindNode = (string)fieldInfo.GetValue(this);

            int nSelect = m_vStartBindPops.IndexOf(bindNode);
            if (nSelect < 0 || string.IsNullOrEmpty(bindNode)) nSelect = 0;
            nSelect = EditorGUILayout.Popup("", nSelect, vBindPops.ToArray(),new GUILayoutOption[] { GUILayout.Width(80) });
            if(nSelect >=0 && nSelect < vBindPops.Count)
            {
                bindNode = vBindPops[nSelect];
            }
            fieldInfo.SetValue(this, bindNode);
        }
        [NonSerialized] List<string> m_vEndBindPops = new List<string>();
        [NonSerialized] private Transform m_pEndLastTransfrom = null;
        public void OnSelectEndBindSlot(System.Object pOwner, System.Reflection.FieldInfo fieldInfo)
        {
            if (fieldInfo.Name != "endBindSlot")
                return;
            if (baseProp.ownerTrackObject == null)
                return;
            var bindObj = baseProp.ownerTrackObject.GetPlayable().GetBindLastCutsceneObject(this.targetGroup);
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
            if (nSelect < 0 || string.IsNullOrEmpty(bindNode)) nSelect = 0;
            nSelect = EditorGUILayout.Popup("", nSelect, vBindPops.ToArray(), new GUILayoutOption[] { GUILayout.Width(80) });
            if (nSelect >= 0 && nSelect < vBindPops.Count)
            {
                bindNode = vBindPops[nSelect];
            }
            fieldInfo.SetValue(this, bindNode);
        }
        static List<string> m_vPopCutsceneGpNames = new List<string>();
        static List<ushort> m_vPopCutsceneGpIds = new List<ushort>();
        public void OnCutsceneGroupSelect(System.Object pOwner, System.Reflection.FieldInfo fieldInfo)
        {
            if (fieldInfo.Name != "targetGroup")
                return;
            if (baseProp.ownerTrackObject == null || baseProp.ownerTrackObject.GetPlayable() == null)
                return;

            m_vPopCutsceneGpNames.Clear();
            m_vPopCutsceneGpIds.Clear();
            baseProp.ownerTrackObject.GetPlayable().GetGroupNames(m_vPopCutsceneGpNames, m_vPopCutsceneGpIds, baseProp.ownerTrackObject.GetGroupId());

            int index = UnityEditor.EditorGUILayout.Popup(m_vPopCutsceneGpIds.IndexOf(targetGroup), m_vPopCutsceneGpNames.ToArray());
            if (index >= 0 && index < m_vPopCutsceneGpIds.Count)
            {
                if (targetGroup != baseProp.ownerTrackObject.GetGroupId())
                {
                    targetGroup = m_vPopCutsceneGpIds[index];
                }
            }
            else targetGroup = ushort.MaxValue;
        }
#endif		
    }
    //-----------------------------------------------------
    //相机移动驱动逻辑
    //-----------------------------------------------------
    public class ProjecitleDriver : ACutsceneDriver
    {
        private GameObject m_StartInstance;
        private GameObject m_LaunchInstance;
        private GameObject m_EndInstance;
        private Transform m_StartBindTrans;
        private Transform m_EndBindTrans;
        private string m_lastStartBindNode = null;
        private string m_lastEndBindNode = null;

#if UNITY_EDITOR
        private ParticleSystem[] m_StartParticles;
        private ParticleSystem[] m_LaunchParticles;
        private ParticleSystem[] m_EndParticles;
#endif
        //-----------------------------------------------------
        public override void OnDestroy()
        {
            DestroyParticle(-1);
            m_StartBindTrans = null;
            m_EndBindTrans = null;
        }
        //-----------------------------------------------------
        public GameObject GetGameObject(int state)
        {
            switch(state)
            {
                case 0: return m_StartInstance;
                case 1: return m_LaunchInstance;
                case 2: return m_EndInstance;
            }
            return null;
        }
        //-----------------------------------------------------
        public Transform GetBindTransform(int state)
        {
            switch(state)
            {
                case 0: return m_StartBindTrans;
                case 2: return m_EndBindTrans;
            }
            return null;
        }
        //-----------------------------------------------------
        void SetGameObject(int state, GameObject obj, ParticleSystem[] particles)
        {
            switch(state)
            {
                case 0:
                    m_StartInstance = obj;
#if UNITY_EIITOR
                    m_StartParticles = particles;
#endif
                    break;
                case 1:
                    m_LaunchInstance = obj;
#if UNITY_EIITOR
                    m_LaunchParticles = particles;
#endif
                    break;
                case 2:
                    m_EndInstance = obj;
#if UNITY_EIITOR
                    m_EndParticles = particles;
#endif
                    break;
            }
        }
        //-----------------------------------------------------
        public GameObject GetStartInstance()
        {
            return GetGameObject(0);
        }
        //-----------------------------------------------------
        public GameObject GetEndInstance()
        {
            return GetGameObject(2);
        }
        //-----------------------------------------------------
        public GameObject GetLaunchInstance()
        {
            return GetGameObject(1);
        }
        //-----------------------------------------------------
        void OnInstanceStart(GameObject pObj)
        {
            OnInstance(pObj,0);
        }
        //-----------------------------------------------------
        void OnInstanceLaunch(GameObject pObj)
        {
            OnInstance(pObj, 1);
        }
        //-----------------------------------------------------
        void OnInstanceEnd(GameObject pObj)
        {
            OnInstance(pObj, 2);
            if (pObj)
                pObj.transform.localScale = Vector3.zero;
        }
        //-----------------------------------------------------
        void OnInstance(GameObject pObj, int state)
        {
            if (pObj == null)
                return;

            if (IsDestroyed())
            {
                DestroyParticle(state);
                return;
            }
            if (pObj == GetGameObject(state))
                return;

            ParticleSystem[] parSystems = null;
#if UNITY_EDITOR
            if (IsEditorMode())
            {
                parSystems = pObj.GetComponentsInChildren<ParticleSystem>(true);
                if (parSystems == null || parSystems.Length <= 0)
                    parSystems = pObj.GetComponents<ParticleSystem>();
                if (!Application.isPlaying)
                {
                    foreach (var ps in parSystems)
                    {
                        ps.Play(true);
                    }
                }
            }
#endif
            SetGameObject(state, pObj, parSystems);
            UpdateTransform(state);
        }
        //-----------------------------------------------------
        void UpdateTransform(int state, ProjecitleClip clip = null, CutsceneTrack pTrack = null)
        {
            var go = GetGameObject(state);
            if (go == null)
                return;
            var bindTrans = GetBindTransform(state);
            var transform = go.transform;


            if(clip!=null)
            {
                if (state == 0)
                {
                    transform.position = clip.startPosition;
                    transform.eulerAngles = clip.endRotate;
                    transform.localScale = clip.startScale;
                }
                else if (state == 2)
                {
                    transform.position = clip.endPosition;
                    transform.eulerAngles = clip.endRotate;
                    transform.localScale = clip.endScale;
                }
            }


            if (bindTrans != null)
            {
                transform.SetParent(bindTrans, false);
                transform.position = Vector3.zero;
                transform.eulerAngles = Vector3.zero;
                if (clip != null)
                {
                    if (state == 0)
                    {
                        transform.localPosition = clip.startPosition;
                        transform.localEulerAngles = clip.startRotate;
                    }
                    else if (state == 2)
                    {
                        transform.localPosition = clip.endPosition;
                        transform.localEulerAngles = clip.endRotate;
                    }
                }
                else
                {
                    transform.localPosition = Vector3.zero;
                    transform.localScale = Vector3.one;
                    transform.localEulerAngles = Vector3.zero;
                }
            }
            else if (clip != null)
            {
                if (state == 0)
                {
                    if (pTrack != null)
                    {
                        var obj = pTrack.GetBindLastCutsceneObject();
                        Vector3 tempPos = Vector3.zero;
                        if (obj != null && obj.GetParamPosition(ref tempPos))
                        {
                            transform.position = tempPos + clip.startPosition;
                        }
                    }
                }
                else if (state == 2)
                {
                    if (pTrack != null)
                    {
                        var obj = pTrack.GetCutscene().GetGroupBindLastCutsceneObject(clip.targetGroup);
                        Vector3 tempPos = Vector3.zero;
                        if (obj != null && obj.GetParamPosition(ref tempPos))
                        {
                            transform.position = tempPos + clip.endPosition;
                        }
                    }
                }
            }
        }
        //-----------------------------------------------------
        void CheckStartBindTransform(CutsceneTrack pTrack, ProjecitleClip parClip)
        {
            var instance = GetGameObject(0);
            if (instance == null)
                return;
            string bindSlot = parClip.startBindSlot;
            if (string.IsNullOrEmpty(bindSlot))
            {
                if (instance) instance.transform.SetParent(null);
                m_StartBindTrans = null;
                m_lastStartBindNode = null;
                return;
            }
            string lastBindNode = m_lastStartBindNode;
            if (bindSlot.CompareTo(lastBindNode) == 0)
            {
                return;
            }

            m_StartBindTrans = null;
            List<ICutsceneObject> vObjs = null;
            vObjs = pTrack.GetBindAllCutsceneObject(vObjs);
            if (vObjs == null || vObjs.Count == 0)
            {
                Debug.LogWarning("ParticleClip: No bind objects found for particle effect.");
                return;
            }
            Transform pBind = GetSlotTransform(vObjs, bindSlot);
            UpdateTransform(0, parClip);
            if (pBind != null)
            {
                m_StartBindTrans = pBind;
                m_lastStartBindNode = bindSlot;
            }
        }
        //-----------------------------------------------------
        void CheckEndBindTransform(CutsceneTrack pTrack, ProjecitleClip parClip)
        {
            var bindObj = pTrack.GetCutscene().GetGroupBindLastCutsceneObject(parClip.targetGroup);
            if (bindObj == null)
                return;

            string bindSlot = parClip.endBindSlot;
            if (string.IsNullOrEmpty(bindSlot))
            {
                m_lastEndBindNode = null;
                m_EndBindTrans = null;
                return;
            }
            string lastBindNode = m_lastEndBindNode;
            if (bindSlot.CompareTo(lastBindNode) == 0)
            {
                return;
            }
            m_EndBindTrans = null;
            var go = bindObj.GetUniyObject() as GameObject;
            if (go != null && !string.IsNullOrEmpty(bindSlot))
            {
                var node = go.transform.Find(bindSlot);
                if (node != null)
                    m_EndBindTrans = node;
                else
                    m_EndBindTrans = go.transform;
            }
            else if (go != null && go.name.CompareTo(bindSlot) == 0)
            {
                m_EndBindTrans = go.transform;
            }
            m_lastEndBindNode = bindSlot;
        }
        //-----------------------------------------------------
        Transform GetSlotTransform(List<ICutsceneObject> vObjs, string bindSlot)
        {
            if (vObjs == null || vObjs.Count == 0)
                return null;
            Transform pBind = null;
            foreach (var db in vObjs)
            {
                var go = db.GetUniyObject() as GameObject;
                if (go != null && !string.IsNullOrEmpty(bindSlot))
                {
                    var node = go.transform.Find(bindSlot);
                    if (node != null)
                        pBind = node;
                    else
                        pBind = go.transform;
                }
                else if (go != null && go.name.CompareTo(bindSlot) == 0)
                {
                    pBind = go.transform;
                }
            }
            return pBind;
        }
        //-----------------------------------------------------
        public override bool OnCreateClip(CutsceneTrack pTrack, IBaseClip clip)
        {
         //   ProjecitleClip parClip = clip.Cast<ProjecitleClip>();
        //    SpawnInstance(parClip.startPrefab, OnInstanceStart, parClip.asyncLoad);
        //    SpawnInstance(parClip.startPrefab, OnInstanceLaunch, parClip.asyncLoad);
        //    SpawnInstance(parClip.startPrefab, OnInstanceEnd, parClip.asyncLoad);
            return base.OnCreateClip(pTrack, clip);
        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData clip)
        {
            ProjecitleClip parClip = clip.clip.Cast<ProjecitleClip>();
            CheckStartBindTransform(pTrack, parClip);
            CheckEndBindTransform(pTrack, parClip);
            SpawnInstance(parClip.startPrefab, OnInstanceStart, parClip.asyncLoad);
            SpawnInstance(parClip.luanchingPrefab, OnInstanceLaunch, parClip.asyncLoad);
            SpawnInstance(parClip.endPrefab, OnInstanceEnd, parClip.asyncLoad);
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
            if(!clip.IsLeaveIn())
            {
                if (m_EndInstance)
                {
                    UpdateTransform(2, clip.clip.Cast<ProjecitleClip>());
                }
            }
            if (clip.CanRestore() || clip.IsLeaveIn())
            {
                DestroyParticle(0);
                DestroyParticle(1);
            }
            return true;
        }
        //-----------------------------------------------------
        void TestParticle(ParticleSystem[] particles, float time)
        {
            if (particles == null) return;
            foreach (var ps in particles)
            {
                if (ps == null) continue;
                if (ps.isPlaying)
                    ps.Simulate(time, true, true, true); // 重置并模拟到当前时间
                else
                    ps.Play();
            }
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            ProjecitleClip parClip = frameData.clip.Cast<ProjecitleClip>();
            CheckStartBindTransform(pTrack, parClip);
            CheckEndBindTransform(pTrack, parClip);

            UpdateTransform(0, parClip);
#if UNITY_EDITOR
            if (IsEditorMode() || frameData.eStatus == EPlayableStatus.Start)
            {
                TestParticle(m_StartParticles, frameData.subTime);
                TestParticle(m_LaunchParticles, frameData.subTime);
            }
#endif
            if (m_LaunchInstance)
            {
                var transform = m_LaunchInstance.transform;
                float t = frameData.curTime / parClip.GetDuration();
                Vector3 startPos = parClip.startPosition;
                if(m_StartBindTrans) startPos = m_StartBindTrans.position + parClip.startPosition;
                else
                {
                    var obj = pTrack.GetBindLastCutsceneObject();
                    Vector3 tempPos = Vector3.zero;
                    if (obj !=null && obj.GetParamPosition(ref tempPos))
                    {
                        startPos = tempPos + parClip.startPosition;
                    }
                }
                Vector3 endPos = parClip.endPosition;
                if (m_EndBindTrans) endPos = m_EndBindTrans.position + parClip.endPosition;
                else
                {
                    var obj = pTrack.GetCutscene().GetGroupBindLastCutsceneObject(parClip.targetGroup);
                    Vector3 tempPos = Vector3.zero;
                    if (obj != null && obj.GetParamPosition(ref tempPos))
                    {
                        endPos = tempPos + parClip.endPosition;
                    }
                }

                    Vector3 pos = Bezier(startPos, startPos + parClip.startTan, endPos + parClip.endTan, endPos, t);
                transform.localPosition = pos;
                if (parClip.launchRotate != null && parClip.launchRotate.length>0)
                {
                    float rot = parClip.launchRotate.Evaluate(t);
                    transform.Rotate(Vector3.up, rot, Space.Self);
                }
                if (parClip.launchScale != null && parClip.launchScale.length>0)
                {
                    float scale = parClip.launchScale.Evaluate(t);
                    transform.localScale = Vector3.one * scale;
                }
            }
            return true;
        }
        //-----------------------------------------------------
        private Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            return u * u * u * p0 +
                   3 * u * u * t * p1 +
                   3 * u * t * t * p2 +
                   t * t * t * p3;
        }
        //-----------------------------------------------------
        private void DestroyParticle(int state)
        {
            switch(state)
            {
                case 0:
                    {
                        if (m_StartInstance != null)
                        {
                            DespawnInstance(m_StartInstance);
                            m_StartInstance = null;
                        }
#if UNITY_EDITOR
                        m_StartParticles = null;
#endif
                    }
                    break;
                case 1:
                    {
                        if (m_LaunchInstance != null)
                        {
                            DespawnInstance(m_LaunchInstance);
                            m_LaunchInstance = null;
                        }
#if UNITY_EDITOR
                        m_LaunchInstance = null;
#endif
                    }
                    break;
                case 2:
                    {
                        if (m_EndInstance != null)
                        {
                            DespawnInstance(m_EndInstance);
                            m_EndInstance = null;
                        }
#if UNITY_EDITOR
                        m_EndParticles = null;
#endif
                    }
                    break;
                case -1:
                    {
                        DestroyParticle(0);
                        DestroyParticle(1);
                        DestroyParticle(2);
                    }
                    break;
            }
        }
    }
}