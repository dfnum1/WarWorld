/********************************************************************
生成日期:	06:30:2025
类    名: 	xxxPort
作    者:	HappLI
描    述:	编辑端口数据结构
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using UnityEditor.Experimental.GraphView;

namespace Framework.AT.Editor
{
    public class LinkPort
    {
        public Port bindPort;
        public GraphNode graphNode;
        public ArvgPort argvPort;
    }
    public class ArvgPort
    {
        public Port bindPort;
        public ArvgPort refReturnPort;
        public UnityEngine.UIElements.VisualElement fieldRoot;
        public GraphNode grapNode;
        public ArgvAttribute attri;

        public UnityEngine.UIElements.VisualElement fieldElement;
        public UnityEngine.UIElements.VisualElement enumPopFieldElement;

        public EVariableType eEnumType = EVariableType.eInt;

        public bool isInput;
        public int slotIndex;// Slot index in the node, used for serialization and deserialization
        public NodePort nodePort;
        public short GetVariableGuid()
        {
            return nodePort.varGuid;
        }
        public IVariable GetVariable()
        {
            return nodePort.pVariable;
        }
        public int GetDummyCnt()
        {
            if(nodePort.dummyPorts == null) return 0;
            return nodePort.dummyPorts.Length;
        }
        public long GetLinkPortKey(int index)
        {
            if (nodePort.dummyPorts == null || index<0 || index >= nodePort.dummyPorts.Length) return 0;
            var port = nodePort.dummyPorts[index];
            return port.guid * 10000000 + port.slotIndex*10000 + port.type;
        }
        public long GetKey()
        {
            return grapNode.bindNode.guid * 10000000 + slotIndex * 10000 + (long)(isInput ? 0 : 1);
        }
        public string GetName()
        {
            return attri.name;
        }
    }
    /*
    public enum EPortIO
    {
        In = 0,
        Out = 1,
        LinkIn = 2,
        LinkOut=3,
    }

    public interface IPortNode
    {
        int GetGUID();
        GraphNode GetNode();

        IPort GetPort();

        Color GetColor();
        bool IsOutput();
        bool IsInput();
        EPortIO getIO();

        Rect GetRect();
        void SetRect(Rect rc);
        Rect GetViewRect();
        void SetViewRect(Rect rc);
        bool CanConnectTo(IPortNode port);

        void ClearConnections();
        string GetTips();
        string SetTips(string name);

        Attribute GetAttribute();
        void SetAttribute(Attribute attri);
    }

    public class PortUtil
    {
        static Dictionary<AgentTreeLogic, Dictionary<int, IPortNode>> ms_allPortPositions = new Dictionary<AgentTreeLogic, Dictionary<int, IPortNode>>();

        //------------------------------------------------------
        public static void Clear(AgentTreeLogic pLogic)
        {
            if (ms_allPortPositions.TryGetValue(pLogic, out var ports))
            {
                ports.Clear();
                ms_allPortPositions.Remove(pLogic);
            }
            ms_allPortPositions.Clear();
        }
        //------------------------------------------------------
        public static void SetPort(AgentTreeLogic logic, int guid, IPortNode node)
        {
            if (!ms_allPortPositions.TryGetValue(logic, out var ports))
            {
                ports = new Dictionary<int, IPortNode>();
                ms_allPortPositions[logic] = ports;
            }
            ports[guid] = node;
        }
        //------------------------------------------------------
        public static IPortNode GetPort(AgentTreeLogic logic, int guid)
        {
            if (ms_allPortPositions.TryGetValue(logic, out var ports))
            {
                IPortNode outPort;
                if (ports.TryGetValue(guid, out outPort))
                    return outPort;
            }

            return null;
        }
    }
    public class LinkPort : IPortNode
    {
        public EPortIO direction;
        public GraphNode baseNode;

        public Rect rect;
        public Rect view;

        public EPortIO getIO() { return direction; }

        public IPort GetPort() { return null; }

        public virtual void Clear()
        {
            baseNode = null;
        }
        public virtual void ClearConnections()
        {
            baseNode.Links.Clear();
        }

        public virtual int GetGUID()
        {
            if (baseNode == null)
                return 0;
            if (baseNode.bindNode == null) return 0;
            return baseNode.GetGUID() * 10000000 + (((int)direction << 8));
        }

        public int GetLinkGUID()
        {
            return GetGUID();
        }

        public Rect GetRect() { return rect; }
        public void SetRect(Rect rc) { rect = rc; }

        public Rect GetViewRect() { return view; }
        public void SetViewRect(Rect rc) { view = rc; }


        public bool IsInput() { return direction == EPortIO.LinkIn; }
        public bool IsOutput() { return direction == EPortIO.LinkOut; }


        public GraphNode GetNode()
        {
            return baseNode;
        }

        public virtual Color GetColor()
        {
            return Color.white;
        }


        public virtual bool CanConnectTo(IPortNode port)
        {
            // Figure out which is input and which is output
            //NodePort input = null, output = null;
            //if (IsInput) input = this;
            //else output = this;
            //if (port.IsInput) input = port;
            //else output = port;
            //// If there isn't one of each, they can't connect
            //if (input == null || output == null) return false;
            //// Check type constraints
            //if (input.typeConstraint == XNode.Node.TypeConstraint.Inherited && !input.ValueType.IsAssignableFrom(output.ValueType)) return false;
            //if (input.typeConstraint == XNode.Node.TypeConstraint.Strict && input.ValueType != output.ValueType) return false;
            //// Success
            if (port.GetType() != GetType()) return false;
            return port.getIO() != getIO();
        }
        public void SetConstFlag() { }

        string m_strTips = "";
        public string GetTips()
        {
            return m_strTips;
        }
        public string SetTips(string name) { return m_strTips = name; }

        Attribute m_Attribute = null;
        public Attribute GetAttribute() { return m_Attribute; }
        public void SetAttribute(Attribute attri)
        {
            m_Attribute = attri;
        }
    }
    */
}
#endif
