/********************************************************************
生成日期:	06:30:2025
类    名: 	CommonOpNode
作    者:	HappLI
描    述:	通用的运算节点
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
namespace Framework.AT.Editor
{
    [NodeBind(EActionType.eOpVariable)]
    public class CommonOpNode : GraphNode
    {
        public CommonOpNode(AgentTreeGraphView pAgent, BaseNode pNode, bool bUpdatePos = true) : base(pAgent, pNode, bUpdatePos)
        {
        }
        //------------------------------------------------------
        protected override void OnInit()
        {
            base.OnInit();
            this.title = "运算-" + " Unknown";
            var attri = GetAttri();
            if (attri!=null && attri.argvs!=null && attri.argvs.Count == 3)
            {
                var ports = bindNode.GetInports(true, attri.argvs.Count);
                if(ports!=null && ports.Length ==3)
                {
                    var pOpVar = m_pGraphView.GetVariable(ports[1].varGuid);
                    if (pOpVar != null && pOpVar is VariableInt)
                    {
                        EOpType opType = (EOpType)((VariableInt)pOpVar).value;
                        this.title = "运算-" + Framework.ED.EditorUtils.GetEnumDisplayName(opType);
                    }
                    else
                        this.title = "运算-" + Framework.ED.EditorUtils.GetEnumDisplayName(EOpType.eAdd);
                }
            }
        }
        //------------------------------------------------------
        protected override void OnArgvPortChanged(ArvgPort port)
        {
            base.OnArgvPortChanged(port);
            if (port.slotIndex == 1)
            {
                var pOpVar = m_pGraphView.GetVariable(port.GetVariableGuid());
                if (pOpVar != null && pOpVar is VariableInt)
                {
                    EOpType opType = (EOpType)((VariableInt)pOpVar).value;
                    this.title = "运算-" + Framework.ED.EditorUtils.GetEnumDisplayName(opType);
                }
            }
        }
        //------------------------------------------------------
        protected override void OnArgvPortVarTypeChanged(ArvgPort port)
        {
            base.OnArgvPortVarTypeChanged(port);
            if(port.slotIndex == 0)
            {
                if (2 < m_vArgvPorts.Count)
                {
                    ChangeVariableType(m_vArgvPorts[2], port.eEnumType);
                    var port2 = m_vArgvPorts[2].bindPort;
                    // 需要重新赋值回去（struct）
                    m_vArgvPorts[2].bindPort.portColor = port.bindPort.portColor;
                    ClearPortVarEle(m_vArgvPorts[2]);
                    InnerDrawPortValue(m_vArgvPorts[2], 2);
                }
                if (0 < m_vReturnPorts.Count)
                {
                    ChangeVariableType(m_vReturnPorts[0], port.eEnumType);
                    var port2 = m_vReturnPorts[0].bindPort;
                    // 需要重新赋值回去（struct）
                    m_vReturnPorts[0].bindPort.portColor = port.bindPort.portColor;
                    ClearPortVarEle(m_vReturnPorts[0]);
                    InnerDrawPortValue(m_vReturnPorts[0], 2);
                }
            }
        }
    }
}
#endif