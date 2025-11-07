/********************************************************************
生成日期:		06:30:2025
类    名: 	CutsceneGraph
作    者:	HappLI
描    述:	过场数据流程图
*********************************************************************/
using Framework.AT.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
	//-----------------------------------------------------
	[System.Serializable]
	public class CutsceneNode
	{
        public CutsceneData cutSceneData;
	}
    //-----------------------------------------------------
    [System.Serializable]
    public class CutsceneGraph
    {
        public List<CutsceneNode> vCutscenes;
        public AgentTreeData agentTree;

        [System.NonSerialized]
        private UnityEngine.Object m_pOwnerObject = null;
        //-----------------------------------------------------
        internal void SetOwnerObject(UnityEngine.Object obj)
        {
            m_pOwnerObject = obj;
        }
        //-----------------------------------------------------
        public UnityEngine.Object GetOwnerObject()
        {
            return m_pOwnerObject;
        }
        //-----------------------------------------------------
        public CutsceneData GetEnterCutscene()
        {
            if (vCutscenes == null || vCutscenes.Count <= 0)
                return null;
            return vCutscenes[0].cutSceneData;
        }
        //-----------------------------------------------------
        public CutsceneData GetSubCutscene(string name)
        {
            if (vCutscenes == null || vCutscenes.Count <= 0)
                return null;
            for(int i =0; i < vCutscenes.Count; ++i)
            {
                if (vCutscenes[i].cutSceneData.name.CompareTo(name) == 0)
                    return vCutscenes[i].cutSceneData;
            }
            return null;
        }
        //-----------------------------------------------------
        public CutsceneData GetSubCutscene(ushort id)
        {
            if (vCutscenes == null || vCutscenes.Count <= 0)
                return null;
            for (int i = 0; i < vCutscenes.Count; ++i)
            {
                if (vCutscenes[i].cutSceneData.id == id)
                    return vCutscenes[i].cutSceneData;
            }
            return null;
        }
        //-----------------------------------------------------
        public bool OnDeserialize(string content)
        {
            try
            {
                if(vCutscenes!=null) vCutscenes.Clear();
                JsonUtility.FromJsonOverwrite(content, this);
                if(vCutscenes!=null)
                {
                    for(int j = 0; j < vCutscenes.Count; ++j)
                    {
                        vCutscenes[j].cutSceneData.OnDeserialize();
                    }
                }
                if(agentTree != null) agentTree.Deserialize();
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        public string OnSerialize()
        {
            // 最终序列化整个 CutsceneData
            if(agentTree != null) agentTree.Serialize(false);
            if(vCutscenes!=null)
            {
                for (int i = 0; i < vCutscenes.Count; ++i)
                {
                    vCutscenes[i].cutSceneData.OnSerialize();
                }
            }

            return JsonUtility.ToJson(this, true);
        }
        //-----------------------------------------------------
        public bool AddCutscene(CutsceneData cutscene)
        {
            if (string.IsNullOrEmpty(cutscene.name))
            {
                Debug.LogError("cutscene 名称不能为空");
                return false;
            }
            if (vCutscenes == null)
                vCutscenes = new List<CutsceneNode>();
            for (int i =0; i < vCutscenes.Count; ++i)
            {
                if(vCutscenes[i].cutSceneData.name.CompareTo(cutscene.name) == 0)
                {
                    Debug.LogWarning("cutscene 重名，无法添加");
                    return false;
                }
            }
            CutsceneNode node = new CutsceneNode();
            node.cutSceneData = cutscene;
            vCutscenes.Add(node);
            return true;
        }
        //-----------------------------------------------------
        public CutsceneData FindCutscene(string name)
        {
            if (vCutscenes == null) return null;
            for (int i = 0; i < vCutscenes.Count; ++i)
            {
                if (vCutscenes[i].cutSceneData.name.CompareTo(name) == 0)
                {
                    return vCutscenes[i].cutSceneData;
                }
            }
            return null;
        }
        //-----------------------------------------------------
        public int IndexOfCutscene(CutsceneData cutscene)
        {
            if (vCutscenes == null) return -1;
            for (int i = 0; i < vCutscenes.Count; ++i)
            {
                if (vCutscenes[i].cutSceneData== cutscene)
                {
                    return i;
                }
            }
            return -1;
        }
#endif
    }

}