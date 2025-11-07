/********************************************************************
生成日期:	06:30:2025
类    名: 	InspectorDrawLogic
作    者:	HappLI
描    述:	数据面板逻辑
*********************************************************************/
#if UNITY_EDITOR
using Framework.ED;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Cutscene.Runtime;
using Framework.AT.Editor;

namespace Framework.Cutscene.Editor
{
    [EditorBinder(typeof(CutsceneEditor), "InspectorRect")]
    public class InspectorDrawLogic : ACutsceneLogic, UndoHandler
    {
        Vector2 m_Scoller;
        List<ICutsceneObject> m_vBindObjects = new List<ICutsceneObject>();
        System.Collections.Generic.List<IDraw> m_vSelectClips = null;
        CutsceneData.Group m_pSelectGroup;
        //--------------------------------------------------------
        protected override void OnEnable()
        {
        }
        //--------------------------------------------------------
        protected override void OnDisable()
        {
            if (m_vSelectClips == null) return;
            foreach (var db in m_vSelectClips)
            {
                db.GetCustomEditor()?.OnDisable();
            }
        }
        //--------------------------------------------------------
        public override void OnSelectClips(List<IDraw> vSelectClips)
        {
            if (m_vSelectClips == null) m_vSelectClips = new List<IDraw>();
            // 这里可以处理选中剪辑的逻辑
            // 例如：更新Inspector显示的属性等
            foreach (var db in m_vSelectClips)
            {
                db.GetCustomEditor()?.OnDisable();
            }
            m_vSelectClips.Clear();
            m_vSelectClips.AddRange(vSelectClips);
            foreach (var db in m_vSelectClips)
            {
                db.GetCustomEditor()?.OnEnable();
            }
        }
        //--------------------------------------------------------
        public override void OnSelectGroup(CutsceneData.Group group)
        {
            m_pSelectGroup = group;
        }
        //--------------------------------------------------------
        public void RegisterUndoData(object data)
        {
            RegisterUndoData();
        }
        //--------------------------------------------------------
        protected override void OnUpdate(float delta)
        {
            base.OnUpdate(delta);
            if(m_vSelectClips!=null)
            {
                foreach (var db in m_vSelectClips)
                {
                    db.GetCustomEditor()?.OnUpdate(delta);
                }
            }
        }
        //--------------------------------------------------------
        public override void OnSceneView(SceneView sceneView)
        {
            if (m_vSelectClips == null || m_vSelectClips.Count <= 0)
                return;
            for(int i =0; i< m_vSelectClips.Count; i++)
            {
                var draw = m_vSelectClips[i];
                if (draw == null) continue;
                draw.GetData().OnSceneView(sceneView,this);
            }
        }
        //--------------------------------------------------------
        protected override void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(IsRuntimePlayingCutscene());
            var window = GetOwner<CutsceneEditor>();
            Rect rect = GetRect();
            GUILayout.BeginArea(new Rect(rect.x, rect.y + 20, rect.width, rect.height - 20));

            if (m_pSelectGroup != null)
            {
                int cutBinder = m_pSelectGroup.binderId;
                var obj = ObjectBinderUtils.GetBinder(m_pSelectGroup.binderId).GetBinder();
                EditorGUI.BeginChangeCheck();
                obj = (CutsceneObjectBinder)EditorGUILayout.ObjectField("组绑定对象", obj, typeof(CutsceneObjectBinder), true);
                bool changed = EditorGUI.EndChangeCheck();
                if (obj != null)
                {
                    cutBinder = obj.GetBindID();
                    ObjectBinderUtils.BindObject(obj);
                }
                if (cutBinder != m_pSelectGroup.binderId)
                {
                    if (changed) RegisterUndoData();
                    m_pSelectGroup.binderId = cutBinder;
                }
                EditorGUILayout.LabelField("组绑定对象Id:" + m_pSelectGroup.binderId);
            }
            m_Scoller = GUILayout.BeginScrollView(m_Scoller);

            if(m_vSelectClips!=null && m_vSelectClips.Count>0)
            {
                InspectorDrawUtil.BeginChangeCheck(this);
                for (int i = 0; i < m_vSelectClips.Count; i++)
                {
                    var draw = m_vSelectClips[i];
                    if (draw == null) continue;

                    bool bLastStatus = VariablesEditor.CanCusomChangeRule;
                    VariablesEditor.CanCusomChangeRule = false;
                    ClipDraw clip = draw as ClipDraw;
                    if(clip!=null)
                    {
                        DrawClipInspector(clip);
                    }
                    EventDraw pEvt = draw as EventDraw;
                    if (pEvt != null)
                    {
                        DrawEventInspector(pEvt);
                    }
                    VariablesEditor.CanCusomChangeRule = bLastStatus;
                }
                InspectorDrawUtil.EndChangeCheck();
            }

            GUILayout.EndScrollView();

            //    HandleUtilityWrapper.DrawProperty(m_Test);
            GUILayout.EndArea();
            UIDrawUtils.DrawColorLine(new Vector2(rect.xMin, rect.y + 20), new Vector2(rect.xMax, rect.y + 20), new Color(1,1,1,0.5f));
            GUILayout.BeginArea(new Rect(rect.x, rect.y, rect.width, 20));
            GUILayout.Label("属性面板", TextUtil.panelTitleStyle);
            GUILayout.EndArea();

            EditorGUI.EndDisabledGroup();
        }
        //--------------------------------------------------------
        void DrawEventInspector(EventDraw pEvt)
        {
            EditorGUI.BeginDisabledGroup(!pEvt.CanEdit());
            string clipName = pEvt.clip.GetName();
            clipName += "[" + pEvt.GetBegin().ToString("F2") + "]";
            pEvt.expandProp = EditorGUILayout.Foldout(pEvt.expandProp, clipName);
            var cutsceneInstance = GetOwner<CutsceneEditor>().GetCutsceneInstance();
            CutsceneTrack pSubObj = null;
            if(cutsceneInstance!=null && cutsceneInstance.GetPlayable()!=null)
            {
                pSubObj = cutsceneInstance.GetPlayable().GetTrack(pEvt.ownerTrack.track);
            }
            pEvt.clip.SetOwnerObject(GetOwner().GetCurrentObj(), pSubObj);
            if (pEvt.expandProp)
            {
                EditorGUI.indentLevel++;
                if (pEvt.GetCustomEditor() != null)
                {
                    DataUtils.SetCurrentLogic(this);
                    pEvt.GetCustomEditor().SetDefaultDraw(DoDrawInspector);
                }
                if (pEvt.GetCustomEditor() == null || !pEvt.GetCustomEditor().DrawInspector())
                {
                    DoDrawInspector(pEvt);
                }
 
                EditorGUI.indentLevel--;
            }
            if (cutsceneInstance != null && pSubObj != null)
            {
                EditorGUILayout.LabelField("-------------------------------------------------------------------");
                EditorGUILayout.LabelField("绑定的对象数据:");
                m_vBindObjects.Clear();
                pSubObj.GetBindAllCutsceneObject(m_vBindObjects);
                CutsceneObjUtil.DrawCutsceneObjectsGUI(m_vBindObjects);
            }
            EditorGUI.EndDisabledGroup();
        }
        //--------------------------------------------------------
        void DoDrawInspector(IDraw draw)
        {
            if (draw is ClipDraw)
            {
                ClipDraw clip = draw as ClipDraw;
                EditorGUI.BeginDisabledGroup(!clip.CanEdit());
                clip.clip = (IBaseClip)InspectorDrawUtil.DrawProperty(clip.clip, null, null, (data, field, prevData) =>
                {
                    var dataValue = field.GetValue(data);
                    if (field.FieldType == typeof(AnimationClip))
                    {
                        if (dataValue != null && dataValue is AnimationClip)
                        {
                            clip.DragEnd();
                            if (clip.clip.IsDefaultName())
                                clip.clip.SetName(((AnimationClip)dataValue).name);
                            clip.SetEnd(clip.GetBegin() + TimeUtil.GetAnimationClipLength((AnimationClip)dataValue));
                        }
                        else if (dataValue != null && dataValue is AudioClip)
                        {
                            clip.DragEnd();
                            if (clip.clip.IsDefaultName())
                                clip.clip.SetName(((AudioClip)dataValue).name);
                            clip.SetEnd(clip.GetBegin() + ((AudioClip)dataValue).length);
                        }
                    }
                });
                EditorGUI.EndDisabledGroup();
            }
            else if(draw is EventDraw)
            {
                EventDraw pEvt = draw as EventDraw;
                EditorGUI.BeginDisabledGroup(!pEvt.CanEdit());
                pEvt.clip = (IBaseEvent)InspectorDrawUtil.DrawProperty(pEvt.clip, null, null, (data, field, prevData) =>
                {
                    var dataValue = field.GetValue(data);
                });
                EditorGUI.EndDisabledGroup();
            }
        }
        //--------------------------------------------------------
        void DrawClipInspector(ClipDraw clip)
        {
            EditorGUI.BeginDisabledGroup(!clip.CanEdit());
            // 绘制每个剪辑的属性
            string clipName = clip.clip.GetName();
            clipName += "[" + clip.GetBegin().ToString("F2") + "-" + clip.GetEnd().ToString("F2") + "]["+clip.clip.GetType()+"]";
            clip.expandProp = EditorGUILayout.Foldout(clip.expandProp, clipName);
            CutsceneTrack pSubObj = null;
            var cutsceneInstance = GetOwner<CutsceneEditor>().GetCutsceneInstance();
            if (cutsceneInstance != null && cutsceneInstance.GetPlayable() != null)
            {
                pSubObj = cutsceneInstance.GetPlayable().GetTrack(clip.ownerTrack.track);
            }
            clip.clip.SetOwnerObject(GetOwner().GetCurrentObj(), pSubObj);
            if (clip.expandProp)
            {
                EditorGUI.indentLevel++;
                DataUtils.SetCurrentLogic(this);
                if (clip.GetCustomEditor() != null)
                    clip.GetCustomEditor().SetDefaultDraw(DoDrawInspector);
                if (clip.GetCustomEditor() == null || !clip.GetCustomEditor().DrawInspector())
                {
                    DoDrawInspector(clip);
                }
                EditorGUI.indentLevel--;
            }

            if(cutsceneInstance!=null && pSubObj!=null)
            {
                EditorGUILayout.LabelField("-------------------------------------------------------------------");
                EditorGUILayout.LabelField("绑定的对象数据:");
                m_vBindObjects.Clear();
                pSubObj.GetBindAllCutsceneObject(m_vBindObjects);
                CutsceneObjUtil.DrawCutsceneObjectsGUI(m_vBindObjects);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}

#endif