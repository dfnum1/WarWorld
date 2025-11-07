#if UNITY_EDITOR
/********************************************************************
生成日期:	06:30:2025
类    名: 	UndoLogic
作    者:	HappLI
描    述:	数据逻辑处理
*********************************************************************/
using Framework.AT.Runtime;
using Framework.Cutscene.Runtime;
using Framework.ED;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    [EditorBinder(typeof(CutsceneEditor))]
    public class UndoLogic : ACutsceneLogic
    {
        struct StackData
        {
            public System.Object data;
            public string json;
            public bool isDirtyData;
            public StackData(System.Object data, bool isDirtyData)
            {
                this.isDirtyData = isDirtyData;
                this.data = data;
                if (data is CutsceneData)
                    this.json = ((CutsceneData)data).OnSerialize();
                else if (data is CutsceneGraph)
                    this.json = ((CutsceneGraph)data).OnSerialize();
                else if (data is AgentTreeData)
                    this.json = ((AgentTreeData)data).Serialize(true);
                else this.json = null;
            }
        }
        Dictionary<object, Stack<StackData>> m_vAssetUndoStacks = new Dictionary<object, Stack<StackData>>();
        Stack<StackData> m_vUndoStacks = new Stack<StackData>();
        bool m_bStackPop = false;
        //--------------------------------------------------------
        protected override void OnEnable()
        {
            base.OnEnable();
        }
        //--------------------------------------------------------
        public override void OnSaveChanges()
        {
        }
        //--------------------------------------------------------
        public override void OnChangeSelect(object pAsset)
        {
            if(m_vAssetUndoStacks.TryGetValue(pAsset, out var stacks))
            {
                m_vUndoStacks = stacks;
            }
            else
            {
                m_vUndoStacks = new Stack<StackData>();
                m_vAssetUndoStacks[pAsset] = m_vUndoStacks;
            }
        }
        //--------------------------------------------------------
        public void LockUndoData(System.Object pData, bool isDirtyData)
        {
            if (pData == null)
                return;
            if (m_vAssetUndoStacks.TryGetValue(pData, out var stacks))
            {
                m_vUndoStacks = stacks;
            }
            else
            {
                m_vUndoStacks = new Stack<StackData>();
                m_vAssetUndoStacks[pData] = m_vUndoStacks;
            }
            m_vUndoStacks.Push( new StackData(pData, isDirtyData));
        }
        //--------------------------------------------------------
        public void RedoData()
        {
            if (m_vUndoStacks.Count<=0)
                return;
        }
        //--------------------------------------------------------
        public void UndoData()
        {
            if (m_vUndoStacks.Count <= 0)
                return;

            var obj = m_vUndoStacks.Pop();
            bool bStack = false;
            if(!string.IsNullOrEmpty(obj.json))
            {
                if (obj.data is CutsceneGraph)
                {
                    CutsceneGraph graphData = (CutsceneGraph)obj.data;
                    string lastName = null;
                    var currAsset = GetAsset();
                    if(currAsset!=null)
                    {
                        lastName = currAsset.name;
                    }
                    graphData.OnDeserialize(obj.json);
                    if(!string.IsNullOrEmpty(lastName))
                    {
                        currAsset = graphData.FindCutscene(lastName);
                        SetCutsceneData(currAsset, false);
                    }
                }
                else if (obj.data is CutsceneData)
                {
                    CutsceneData graphData = (CutsceneData)obj.data;
                    graphData.OnDeserialize(obj.json);
                    bStack = true;
                    m_bStackPop = true;
                    SetCutsceneData(graphData, obj.isDirtyData);
                    m_bStackPop = false;
                }
                else if (obj.data is AgentTreeData)
                {
                    AgentTreeData graphData = (AgentTreeData)obj.data;
                    graphData.Deserialize(obj.json);
                    bStack = true;
                    m_bStackPop = true;
                    UndoAgentTreeData(graphData);
                    m_bStackPop = false;
                }
            }
        }
    }
}

#endif