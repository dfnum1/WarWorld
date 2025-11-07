/********************************************************************
生成日期:	08:29:2025
类    名: 	MaterialClip
作    者:	HappLI
描    述:	材质属性剪辑
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
using UnityEditor;
using Framework.ED;
#endif
using Framework.DrawProps;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneClip("材质属性Clip")]
    public class MaterialClip : IBaseClip
    {
        [Display("基本属性")] public BaseClipProp       baseProp;
        [Display("变量名"), Disable] public string              propName = "_Color";
        [Display("变量名"), Disable] public ShaderPropertyType propType = ShaderPropertyType.Vector;
        [Display("原属性"), Disable] public Vector4             propValue = new Vector4(1, 1, 1, 1);
        [Display("目标属性"), Disable] public Vector4           toPropValue = new Vector4(1, 1, 1, 1);
        [Display("贴图"), StringViewPlugin("OnDrawSelectTextureInspector"), Disable]
        public string                 textureName = null;
        [Display("控制曲线"), Disable] public AnimationCurve     propCurve;
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new MaterialDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EClipType.eMaterial;
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
        [System.NonSerialized] List<string> m_vPopProps = new List<string>();
        [System.NonSerialized] List<string> m_vPopPropType = new List<string>();
        [System.NonSerialized] Material m_pMaterial = null;
        [AddInspector]
        public void OnInspector()
        {
            if (baseProp.ownerTrackObject == null || baseProp.ownerTrackObject.GetBindLastCutsceneObject() == null)
            {
                DrawMaterialProp();
                return;
            }
            var bindObj = baseProp.ownerTrackObject.GetBindLastCutsceneObject();
            if(bindObj.GetUniyObject() == null)
            {
                DrawMaterialProp();
                return;
            }
            Material material = null;
            if(bindObj.GetUniyObject() is Material)
            {
                material = bindObj.GetUniyObject() as Material;
            }
            else if (bindObj.GetUniyObject() is Renderer)
            {
                Renderer renderer = bindObj.GetUniyObject() as Renderer;
                material = renderer.sharedMaterial;
            }
            else if (bindObj.GetUniyObject() is GameObject)
            {
                GameObject goRender = bindObj.GetUniyObject() as GameObject;
                var renderer = goRender.GetComponent<Renderer>();
                if(renderer == null) renderer = goRender.GetComponentInChildren<Renderer>();
                material = renderer.sharedMaterial;
            }
            if(material == null)
            {
                DrawMaterialProp();
                return;
            }
            if(m_pMaterial != material)
            {
                m_pMaterial = material;
                m_vPopProps.Clear();
                m_vPopPropType.Clear();
                int propCount = material.shader.GetPropertyCount();
                for (int i = 0; i < propCount; i++)
                {
                    string propName = material.shader.GetPropertyName(i);
                    var propType = material.shader.GetPropertyType(i);
                    m_vPopProps.Add(propName);
                    m_vPopPropType.Add(propType.ToString());
                }
            }
            // 绘制属性选择
            int selectedIndex = Mathf.Max(0, m_vPopProps.IndexOf(propName));
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup("属性名", selectedIndex, m_vPopProps.ToArray());
            propName = m_vPopProps.Count > selectedIndex ? m_vPopProps[selectedIndex] : propName;
            propType = material.shader.GetPropertyType(selectedIndex);
            if (EditorGUI.EndChangeCheck())
            {
                switch (material.shader.GetPropertyType(selectedIndex))
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
                        propValue = (Vector4)material.GetColor(propName);
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
                        propValue =  material.GetVector(propName);
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
                        propValue.x =  material.GetFloat(propName);
                        break;
                }
            }

            // 显示属性类型

            DrawMaterialProp();
        }
        //-----------------------------------------------------
        void DrawMaterialProp()
        {
            string typeStr = propType.ToString();
            EditorGUILayout.LabelField("属性类型", typeStr);
            // 根据类型绘制属性值输入
            switch (propType)
            {
                case UnityEngine.Rendering.ShaderPropertyType.Color:
                    propValue = EditorGUILayout.ColorField("原值", propValue);
                    toPropValue = EditorGUILayout.ColorField("目标值", toPropValue);
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Vector:
                    propValue = EditorGUILayout.Vector4Field("原值", propValue);
                    toPropValue = EditorGUILayout.Vector4Field("目标值", toPropValue);
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Float:
                case UnityEngine.Rendering.ShaderPropertyType.Range:
                    propValue.x = EditorGUILayout.FloatField("原值", propValue.x);
                    toPropValue.x = EditorGUILayout.FloatField("目标值", toPropValue.x);
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Texture:
                    InspectorDrawUtil.DrawPropertyByFieldName(this, "textureName");
                    break;
            }

            // 控制曲线
            GUILayout.BeginHorizontal();
            propCurve = EditorGUILayout.CurveField("控制曲线", propCurve);
            if (GUILayout.Button("Clear", new GUILayoutOption[] { GUILayout.Width(50) }))
            {
                propCurve.keys = null;
            }
            GUILayout.EndHorizontal();
        }
#endif
    }
    //-----------------------------------------------------
    //材质属性驱动
    //-----------------------------------------------------
    public class MaterialDriver : ACutsceneDriver
    {
        private Material m_pMaterial;
        Texture m_pTexture = null;
        string m_curTextureName = string.Empty;

        //-----------------------------------------------------
        public override void OnDestroy()
        {
#if UNITY_EDITOR
            if (IsEditorMode() && m_pMaterial) m_pMaterial.Restore();
#endif
            m_pMaterial = null;
        }
        //-----------------------------------------------------
        void CheckMaterial(CutsceneTrack pTrack)
        {
            if (m_pMaterial) return;

            var bindObj = pTrack.GetBindLastCutsceneObject();
            if (bindObj == null)
                return;
            if (bindObj.GetUniyObject() is Material)
            {
                m_pMaterial = bindObj.GetUniyObject() as Material;
            }
            else if (bindObj.GetUniyObject() is Renderer)
            {
                Renderer renderer = bindObj.GetUniyObject() as Renderer;
                m_pMaterial = renderer.sharedMaterial;
            }
            else if (bindObj.GetUniyObject() is GameObject)
            {
                GameObject goRender = bindObj.GetUniyObject() as GameObject;
                var renderer = goRender.GetComponent<Renderer>();
                if (renderer == null) renderer = goRender.GetComponentInChildren<Renderer>();
                m_pMaterial = renderer.sharedMaterial;
            }
#if UNITY_EDITOR
            if (m_pMaterial)
            {
                if (IsEditorMode())
                {
                    m_pMaterial.Restore();
                    m_pMaterial.Backup();
                }
            }
#endif
        }
        //-----------------------------------------------------
        public override bool OnClipEnter(CutsceneTrack pTrack, FrameData clip)
        {
            CheckMaterial(pTrack);
            return true;
        }
        //-----------------------------------------------------
        public override bool OnClipLeave(CutsceneTrack pTrack, FrameData clip)
        {
            if (clip.CanRestore() || clip.IsOvered())
            {
#if UNITY_EDITOR
                if (IsEditorMode() && m_pMaterial) m_pMaterial.Restore();
#endif
                m_pMaterial = null;
            }
            return true;
        }
        //-----------------------------------------------------
        public override bool OnFrameClip(CutsceneTrack pTrack, FrameData frameData)
        {
            if (!IsEditorMode() && frameData.eStatus == EPlayableStatus.Pause)
                return true;

            // 检查材质
            CheckMaterial(pTrack);
            if (m_pMaterial == null)
                return true;

            // 获取剪辑数据
            var clipData = frameData.clip as MaterialClip;
            if (clipData == null || string.IsNullOrEmpty(clipData.propName))
                return true;

            // 计算当前时间进度
            float duration = Mathf.Max(clipData.GetDuration(), 0.01f);
            float t = Mathf.Clamp01(frameData.subTime / duration);
            float curveValue = (clipData.propCurve != null && clipData.propCurve.length>0) ? clipData.propCurve.Evaluate(t) : t;

            // 根据属性类型设置材质属性
            var shader = m_pMaterial.shader;
            int propIndex = shader.FindPropertyIndex(clipData.propName);
            if (propIndex < 0)
                return true;

            var propType = shader.GetPropertyType(propIndex);
            switch (propType)
            {
                case UnityEngine.Rendering.ShaderPropertyType.Color:
                    {
                        // 线性插值颜色
                        Color baseColor = clipData.propValue;
                        Color targetColor = clipData.toPropValue;
                        Color lerpColor = Color.Lerp(baseColor, targetColor, curveValue);
                        m_pMaterial.SetColor(clipData.propName, lerpColor);
                    }
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Vector:
                    {
                        Vector4 baseVec = clipData.propValue;
                        Vector4 targetVec = clipData.toPropValue;
                        Vector4 lerpVec = Vector4.Lerp(baseVec, targetVec, curveValue);
                        m_pMaterial.SetVector(clipData.propName, lerpVec);
                    }
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Float:
                case UnityEngine.Rendering.ShaderPropertyType.Range:
                    {
                        float baseVal = clipData.propValue.x;
                        float targetVal = clipData.toPropValue.x;
                        float lerpVal = Mathf.Lerp(baseVal, targetVal, curveValue);
                        m_pMaterial.SetFloat(clipData.propName, lerpVal);
                    }
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Texture:
                    {
                        // 贴图类型只在进入时设置，不做插值
                        if (!string.IsNullOrEmpty(clipData.textureName))
                        {
                            if (m_pTexture == null || clipData.textureName.CompareTo(m_curTextureName)!=0)
                            {
                                LoadAsset(clipData.textureName, (obj) =>
                                {
                                    if (obj is Texture)
                                    {
                                        m_pTexture = obj as Texture;
                                        m_pMaterial.SetTexture(clipData.propName, obj as Texture);
                                    }
                                }, false);
                                m_curTextureName = clipData.textureName;
                            }
                        }
                    }
                    break;
            }
            return true;
        }
    }
}