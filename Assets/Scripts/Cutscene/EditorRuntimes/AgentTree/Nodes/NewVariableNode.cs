#if UNITY_EDITOR
/********************************************************************
生成日期:	06:30:2025
类    名: 	NewVariableNode
作    者:	HappLI
描    述:	新建变量节点
*********************************************************************/
using Framework.AT.Runtime;
using UnityEditor.Experimental.GraphView;

namespace Framework.AT.Editor
{
    [NodeBind(EActionType.eNewVariable)]
    public class NewVariableNode : GraphNode
    {
        public NewVariableNode(AgentTreeGraphView pAgent, BaseNode pNode, bool bUpdatePos = true) : base(pAgent, pNode, bUpdatePos)
        {
        }
        //------------------------------------------------------
        protected override void BuildReturnPort()
        {
            this.outputContainer.Clear();
            if (m_vArgvPorts.Count != m_vReturnPorts.Count)
                return;
            for (int i = 0; i < m_vReturnPorts.Count; ++i)
            {
                var port = m_vReturnPorts[i];
                var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ArvgPort));
                inputPort.portName = "";
                inputPort.portColor = m_vArgvPorts[i].bindPort.portColor;
                inputPort.source = port;
                inputPort.style.marginRight = 4;
                m_vArgvPorts[i].bindPort.Add(inputPort);
                port.bindPort = inputPort;
            }
        }
    }
}
#endif