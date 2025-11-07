/********************************************************************
生成日期:	06:30:2025
类    名: 	TrackBindCutsceneGraphNode
作    者:	HappLI
描    述:	轨道绑定过场动画节点
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using Framework.Cutscene.Runtime;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
namespace Framework.AT.Editor
{
    [NodeBind(EActionType.eBindCutsceneTrackData)]
    public class TrackBindCutsceneGraphNode : GraphNode
    {
        List<string> m_vCutscenePops = new List<string>();
        List<int> m_vCutsceneIds = new List<int>();
        public TrackBindCutsceneGraphNode(AgentTreeGraphView pAgent, BaseNode pNode, bool bUpdatePos = true) : base(pAgent, pNode, bUpdatePos)
        {
        }
        //------------------------------------------------------
        public override float GetWidth()
        {
            return base.GetWidth()+100;
        }
        //-----------------------------------------------------
        void RefreshCutscenePopupList()
        {
            m_vCutscenePops.Clear();
            m_vCutsceneIds.Clear();
            var nodes = m_pGraphView.GetCutsceneGraph();
            if(nodes!=null)
            {
                foreach (var node in nodes.vCutscenes)
                {
                    if (node is CutsceneNode cutsceneNode)
                    {
                        if (cutsceneNode.cutSceneData == null || cutsceneNode.cutSceneData.groups == null) continue;
                        foreach (var db in cutsceneNode.cutSceneData.groups)
                        {
                            if (db == null) continue;
                            m_vCutscenePops.Add(cutsceneNode.cutSceneData.name + " - " + db.name);
                            m_vCutsceneIds.Add((cutsceneNode.cutSceneData.id << 16) | db.id); // 高16位为过场动画ID，低16位为组ID
                        }
                    }
                }
            }
        }
        //-----------------------------------------------------
        protected override void BuildArvPort()
        {
            base.BuildArvPort();
            if(m_vArgvPorts.Count>2)
            {
                var nodes = m_pGraphView.GetCutsceneGraph();

                ArvgPort port = m_vArgvPorts[1];
                short varGuid = port.GetVariableGuid();
                var portVariable = m_pGraphView.GetVariable(varGuid);
                if(portVariable!=null && portVariable.GetVariableType() == EVariableType.eInt)
                {
                    RefreshCutscenePopupList();
                    var intVar = (AT.Runtime.VariableInt)portVariable;
                    int selectedIndex = m_vCutsceneIds.IndexOf(intVar.value);
                    if(selectedIndex <0 && m_vCutsceneIds.Count>0)
                    {
                        intVar.value = m_vCutsceneIds[0];
                        if (port.fieldRoot.childCount > 0)
                        {
                            var field = port.fieldRoot.ElementAt(0);
                            if (field is IntegerField)
                            {
                                ((IntegerField)field).value = intVar.value;
                            }
                        }
                    }
                    // 创建 PopupField
                    var popup = new PopupField<string>(
                        m_vCutscenePops,
                        selectedIndex
                    );
                    popup.style.width = 80;
                    // 下拉前刷新
                    popup.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        // 这里刷新你的 m_vCutscenePops 和 m_vCutsceneIds
                        RefreshCutscenePopupList();

                        // 刷新 choices
                        popup.choices = new List<string>(m_vCutscenePops);

                        // 保持选中项
                        int newIndex = m_vCutsceneIds.IndexOf(intVar.value);
                        if (newIndex < 0) newIndex = 0;
                        popup.SetValueWithoutNotify(m_vCutscenePops.Count > 0 ? m_vCutscenePops[newIndex] : "");
                    });

                    // 选项变更回调
                    popup.RegisterValueChangedCallback(evt =>
                    {
                        selectedIndex = m_vCutscenePops.IndexOf(evt.newValue);
                        // 你可以在这里保存选中索引到节点数据
                        if(selectedIndex>=0 && selectedIndex< m_vCutsceneIds.Count)
                        {
                            if (CanChangeValue(port))
                            {
                                // 更新变量值
                                intVar.value = m_vCutsceneIds[selectedIndex];
                                OnArgvPortChanged(port);
                                // 需要重新赋值回去（struct）
                                m_pGraphView.UpdateVariable(intVar);

                                if(port.fieldRoot.childCount>1)
                                {
                                    var field = port.fieldRoot.ElementAt(0);
                                    if(field is IntegerField)
                                    {
                                        ((IntegerField)field).value = intVar.value;
                                    }
                                }
                            }
                        }
                    });

                    // 添加到端口的 VisualElement
                    if (port.fieldRoot.childCount > 0)
                    {
                        var field = port.fieldRoot.ElementAt(0);
                        if (field is IntegerField)
                        {
                            field.SetEnabled(false);
                        }
                    }
                    port.fieldRoot.Add(popup);
                }
            }
        }
    }
}
#endif