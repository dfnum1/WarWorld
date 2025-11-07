/********************************************************************
生成日期:	06:30:2025
类    名: 	GetVariableNode
作    者:	HappLI
描    述:	变量获取节点
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using Framework.Cutscene.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Framework.AT.Editor
{
    [NodeBind(EActionType.eGetVariable)]
    public class GetVariableNode : GraphNode
    {
        public GetVariableNode(AgentTreeGraphView pAgent, BaseNode pNode, bool bUpdatePos = true) : base(pAgent, pNode, bUpdatePos)
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
        protected override void CreateOutports()
        {
            var attr = GetAttri();
            if (attr == null)
                return;

            if (attr.returns.Count > 0)
            {
                var outportVar = bindNode.GetOutports(true, attr.returns.Count);
                for (int i = 0; i < attr.returns.Count; ++i)
                {
                    var nodePort = outportVar[i];
                    var graphNode = m_pGraphView.GetVariableOwnerNode(nodePort.varGuid);
                    if (graphNode != null)
                    {
                        var returnPort = graphNode.GetReturn(nodePort.varGuid);
                        if (returnPort != null)
                        {
                            nodePort.pVariable = returnPort.GetVariable();
                        }
                        else
                            nodePort.pVariable = null;
                    }
                    else
                    {
                        nodePort.pVariable = null;
                    }
                    outportVar[i] = nodePort;
                    ArvgPort port = new ArvgPort();
                    port.grapNode = this;
                    port.nodePort = nodePort;
                    port.attri = attr.returns[i];
                    port.isInput = false;
                    port.slotIndex = i;
                    m_vReturnPorts.Add(port);
                    m_pGraphView.AddPort(port);
                }
            }
            else
                bindNode.GetOutports(true, 0);
        }
        //------------------------------------------------------
        protected override void BuildReturnPort()
        {
            this.outputContainer.Clear();
            for (int i = 0; i < m_vReturnPorts.Count; ++i)
            {
                var port = m_vReturnPorts[i];
                var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ArvgPort));
                inputPort.portName = port.attri.name;
                inputPort.source = port;
                inputPort.style.marginRight = 4;
                inputPort.portColor = EditorPreferences.GetTypeColor(port.GetVariable().GetType());

                var fieldRoot = new VisualElement();
                fieldRoot.style.flexDirection = FlexDirection.Row;
                inputPort.Add(fieldRoot);


                this.inputContainer.Add(inputPort);
                m_vReturnPorts[i].fieldRoot = fieldRoot;
                m_vReturnPorts[i].bindPort = inputPort;
                DrawPortValue(port);
            }
            for (int i = 0; i < m_vReturnPorts.Count; ++i)
            {
                var port = m_vReturnPorts[i];
                if (port == null)
                    continue;
                port.bindPort.source = null;
                var graphNode = m_pGraphView.GetVariableOwnerNode(port.GetVariableGuid());
                if (graphNode != null)
                {
                    var returnPort = graphNode.GetReturn(port.GetVariableGuid());
                    if (returnPort != null)
                    {
                        port.bindPort.portName = returnPort.GetName();
                        port.bindPort.portColor = EditorPreferences.GetTypeColor(returnPort.GetVariable().GetType());
                        port.bindPort.source = port;
                        returnPort.refReturnPort = port;
                    }
                    else
                    {
                        port.bindPort.portName = "!ERROR!";
                    }
                }
                else
                {
                    port.bindPort.portName = "!ERROR!";
                }
            }
        }
    }
}
#endif