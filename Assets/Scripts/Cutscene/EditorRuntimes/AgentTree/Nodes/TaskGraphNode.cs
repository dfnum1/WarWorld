/********************************************************************
生成日期:	06:30:2025
类    名: 	TaskGraphNode
作    者:	HappLI
描    述:	任务节点
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using UnityEngine.UIElements;

namespace Framework.AT.Editor
{
    public class TaskGraphNode : GraphNode
    {
        public TaskGraphNode(AgentTreeGraphView pAgent, BaseNode pNode, bool bUpdatePos = true) : base(pAgent, pNode, bUpdatePos)
        {
        }
        //------------------------------------------------------
        protected override void OnInit()
        {
            var attri = GetAttri();
            if (attri == null)
                return;
            if (bindNode.type <= (int)ETaskType.eExit)
            {
                this.title = "";
                this.titleContainer.Clear();
            }
        }
        //------------------------------------------------------
        protected override void CreateInports()
        {
        }
        //------------------------------------------------------
        public override void AddTitlePorts(bool hasIn, bool hasOut)
        {
            base.AddTitlePorts(hasIn, hasOut);
            if(m_pLinkOut!=null && bindNode.GetOutportCount()<=0)
            {
                m_pLinkOut.bindPort.style.marginTop = 10;
            }
        }
    }
}
#endif