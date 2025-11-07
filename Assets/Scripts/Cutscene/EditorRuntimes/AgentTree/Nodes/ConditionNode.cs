/********************************************************************
生成日期:	06:30:2025
类    名: 	ConditionNode
作    者:	HappLI
描    述:	条件节点
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using Framework.Cutscene.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Framework.AT.Editor
{
    [NodeBind(EActionType.eCondition)]
    public class ConditionNode : GraphNode
    {
        LinkPort m_ElsePort = null;
        public ConditionNode(AgentTreeGraphView pAgent, BaseNode pNode, bool bUpdatePos = true) : base(pAgent, pNode, bUpdatePos)
        {
        }
        //------------------------------------------------------
        protected override void OnInit()
        {
            var attri = GetAttri();
            if (attri == null)
                return;

            var outport = bindNode.GetOutports();
            if (outport == null || outport.Length <= 0)
                return;
            var owerNode = m_pGraphView.GetVariableOwnerNode(outport[0].varGuid);
            if (owerNode != null)
            {
                this.title += "[" + owerNode.title + "]";
            }
        }
        //------------------------------------------------------
        public override void AddTitlePorts(bool hasIn, bool hasOut)
        {
            base.AddTitlePorts(hasIn, hasOut);

        }
        //------------------------------------------------------
        protected override void OnArgvPortVarTypeChanged(ArvgPort port)
        {
            base.OnArgvPortVarTypeChanged(port);
            if(port.slotIndex == 0)
            {
                if (2 < m_vArgvPorts.Count)
                {
                    var port2 = m_vArgvPorts[2].bindPort;
                    var val2 = m_pGraphView.ChangeVariable(m_vArgvPorts[2].GetVariableGuid(), port.eEnumType);
                    // 需要重新赋值回去（struct）
                    m_vArgvPorts[2].bindPort.portColor = EditorPreferences.GetTypeColor(val2.GetType());
                    m_vArgvPorts[2].nodePort.pVariable = val2;

                    ClearPortVarEle(m_vArgvPorts[2]);
                    InnerDrawPortValue(m_vArgvPorts[2], 2);
                }
            }

        }
    }
}
#endif