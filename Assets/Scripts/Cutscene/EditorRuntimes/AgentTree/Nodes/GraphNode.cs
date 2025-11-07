/********************************************************************
生成日期:	06:30:2025
类    名: 	GraphNode
作    者:	HappLI
描    述:	基础的节点绘制
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using Framework.Cutscene.Editor;
using Framework.ED;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Framework.AT.Editor
{
    public class GraphNode : Node
    {
        public AT.Runtime.BaseNode bindNode;

        protected LinkPort m_pLinkIn;
        protected LinkPort m_pLinkOut;

        protected Dictionary<ArvgPort, LinkPort> m_vOtherLinks = new Dictionary<ArvgPort, LinkPort>();

        protected List<ArvgPort> m_vArgvPorts = new List<ArvgPort>();
        protected List<ArvgPort> m_vReturnPorts = new List<ArvgPort>();

        private bool m_isFlashing = false;
        private Color m_lastBgColor = Color.red;
        private double m_flashEndTime = 0;

        protected AgentTreeGraphView m_pGraphView;
        public GraphNode(AgentTreeGraphView pAgent, AT.Runtime.BaseNode pNode, bool bUpdatePos = true)
        {
            m_pGraphView = pAgent;
            bindNode = pNode;
            var attri = GetAttri();
            if (GetAttri() != null)
            {
                this.title = attri.displayName;
            }
            else this.title = "Unknown Node";

            OnInit();

            AddIcon();
            if (attri != null)
            {
                AddTitlePorts(attri.actionAttr.hasInput, attri.actionAttr.hasOutput);
            }

            CheckPorts();
            BuildPorts();

            if(bUpdatePos) UpdatePosition();

            //! refresh
            bool expandLock = this.expanded;
            if(expandLock)
            {
                this.expanded = false;
                this.expanded = true;
            }

            this.title += "[" + bindNode.guid + "]";
        }
        //------------------------------------------------------
        protected void AddIcon()
        {
            var attri = GetAttri();
            if (attri == null)
                return;
            if (attri.iconAttr != null)
            {
                var icon = AgentTreeUtil.LoadIcon(attri.iconAttr.name);
                if (icon != null)
                {
                    var img = new Image
                    {
                        image = icon,
                        style =
                        {
                            width = 32,
                            height = 32,
                            marginLeft = 10,
                            marginTop = 4,
                        }
                    };
                    // 添加到标题栏或主容器
                    this.titleContainer.Insert(0, img);
                }
            }

        }
        //------------------------------------------------------
        protected virtual void OnInit()
        {

        }
        //------------------------------------------------------
        public void UpdatePosition()
        {
            this.SetPosition(new Rect(bindNode.posX * 0.01f, bindNode.posY * 0.01f, GetWidth(), GetHeight()));
        }
        //------------------------------------------------------
        public void UpdateSize()
        {
            this.style.width = GetWidth();
         //   this.style.height = GetHeight();

            // 让子容器自适应
            this.inputContainer.style.flexGrow = 1;
            this.outputContainer.style.flexGrow = 1;
            this.titleContainer.style.height = 30;

            // 触发布局和重绘
            this.MarkDirtyRepaint();
            this.inputContainer.MarkDirtyRepaint();
            this.outputContainer.MarkDirtyRepaint();
            this.titleContainer.MarkDirtyRepaint();
        }
        //------------------------------------------------------
        public void Release()
        {
            if(m_pLinkIn!=null)
            {
                var connections = m_pLinkIn.bindPort.connections;
                if(connections!=null)
                {
                    foreach (var db in connections)
                    {
                        m_pGraphView.RemoveElement(db);
                    }
                }
                m_pLinkIn.bindPort.Clear();
            }
            if (m_pLinkOut != null)
            {
                var connections = m_pLinkOut.bindPort.connections;
                if (connections != null)
                {
                    foreach (var db in connections)
                    {
                        m_pGraphView.RemoveElement(db);
                    }
                }
                m_pLinkOut.bindPort.Clear();
            }
            foreach(var db in m_vOtherLinks)
            {
                var connections = db.Value.bindPort.connections;
                if (connections != null)
                {
                    foreach (var con in connections)
                    {
                        m_pGraphView.RemoveElement(con);
                    }
                }
                db.Value.bindPort.Clear();
            }
            foreach (var db in m_vArgvPorts)
            {
                m_pGraphView.DelPort(db);
            }


            foreach (var db in m_vReturnPorts)
            {
                if (db.refReturnPort != null)
                {
                    db.refReturnPort.bindPort.portName = "!ERROR!";
                    var connects = db.refReturnPort.bindPort.connections;
                    if(connects!=null)
                    {
                        foreach (var edge in connects)
                        {
                            m_pGraphView.RemoveElement(edge);
                        }
                    }
                }
                if (db.bindPort != null)
                {
                    var connects = db.bindPort.connections;
                    if (connects != null)
                    {
                        foreach (var edge in connects)
                        {
                            m_pGraphView.RemoveElement(edge);
                        }
                    }
                }
                m_pGraphView.DelPort(db);
            }

            m_vArgvPorts.Clear();
            m_vReturnPorts.Clear();
        }
        //------------------------------------------------------
        public LinkPort GetLink(bool isInput)
        {
            if (isInput) return m_pLinkIn;
            return m_pLinkOut;
        }
        //------------------------------------------------------
        public ArvgPort GetArvg(int index)
        {
            if (m_vArgvPorts == null || index < 0 || index >= m_vArgvPorts.Count)
                return m_vArgvPorts[index];
            return null;
        }
        //------------------------------------------------------
        public List<ArvgPort> GetArvgs()
        {
            return m_vArgvPorts;
        }
        //------------------------------------------------------
        public ArvgPort GetArvg(short guid)
        {
            if (m_vArgvPorts == null)
                return null;
            for (int i = 0; i < m_vArgvPorts.Count; ++i)
            {
                if (m_vArgvPorts[i].GetVariableGuid() == guid)
                    return m_vArgvPorts[i];
            }
            return null;
        }
        //------------------------------------------------------
        public ArvgPort GetReturn(int index)
        {
            if (m_vReturnPorts == null || index < 0 || index >= m_vReturnPorts.Count)
                return m_vReturnPorts[index];
            return null;
        }
        //-----------------------------------------------------
        public List<ArvgPort> GetReturns()
        {
            return m_vReturnPorts;
        }
        //------------------------------------------------------
        public Dictionary<ArvgPort, LinkPort> GetOtherLinks()
        {
            return m_vOtherLinks;
        }
        //------------------------------------------------------
        public ArvgPort GetReturn(short guid)
        {
            for(int i =0; i < m_vReturnPorts.Count; ++i)
            {
                if (m_vReturnPorts[i].GetVariableGuid() == guid)
                    return m_vReturnPorts[i];
            }
            return null;
        }
        //------------------------------------------------------
        public virtual float GetWidth()
        {
            // 标题宽度
            float titleWidth = 0f;
            var style = EditorStyles.boldLabel;
            if (bindNode != null && !string.IsNullOrEmpty(this.title))
                titleWidth = style.CalcSize(new GUIContent(this.title)).x;

            // 属性（端口）名称最大宽度
            float portWidth = 0f;
            var portStyle = EditorStyles.label;
            foreach (var port in m_vArgvPorts)
            {
                if (port != null && port.attri != null && !string.IsNullOrEmpty(port.attri.name))
                {
                    float w = portStyle.CalcSize(new GUIContent(port.attri.name)).x + 65;
                    if(port.GetVariable()!=null)
                    {
                        if (port.GetVariable().GetType() == typeof(AT.Runtime.VariableString))
                        {
                            w += 70;
                        }
                        else if (port.GetVariable().GetType() == typeof(AT.Runtime.VariableVec2))
                        {
                            w += 70;
                        }
                        else if (port.GetVariable().GetType() == typeof(AT.Runtime.VariableVec3))
                        {
                            w += 150;
                        }
                        else if (port.GetVariable().GetType() == typeof(AT.Runtime.VariableVec4))
                        {
                            w += 210;
                        }
                    }
                    if (w > portWidth) portWidth = w;
                }
            }
            foreach (var port in m_vReturnPorts)
            {
                if (port != null && port.attri != null && !string.IsNullOrEmpty(port.attri.name))
                {
                    float w = portStyle.CalcSize(new GUIContent(port.attri.name)).x + 65;
                    if(port.GetVariable()!=null)
                    {
                        if (port.GetVariable().GetType() == typeof(AT.Runtime.VariableString))
                        {
                            w += 70;
                        }
                        else if (port.GetVariable().GetType() == typeof(AT.Runtime.VariableVec2))
                        {
                            w += 70;
                        }
                        else if (port.GetVariable().GetType() == typeof(AT.Runtime.VariableVec3))
                        {
                            w += 150;
                        }
                        else if (port.GetVariable().GetType() == typeof(AT.Runtime.VariableVec4))
                        {
                            w += 210;
                        }
                    }

                    if (w > portWidth) portWidth = w;
                }
            }

            // 预留端口和padding空间
            float minWidth = 120f;
            float totalWidth = Mathf.Max(titleWidth + 40, portWidth + 80, minWidth);
            return totalWidth;
        }
        //------------------------------------------------------
        public virtual float GetHeight()
        {
            // 标题高度
            float titleHeight = 30f;
            // 每个端口高度
            float portHeight = 22f;
            int portCount = Mathf.Max(m_vArgvPorts.Count, m_vReturnPorts.Count);
            // 至少有标题高度
            float totalHeight = titleHeight + portCount * portHeight + 10f;
            // 最小高度
            return Mathf.Max(totalHeight, 50f);
        }
        //------------------------------------------------------
        public void OnNotifyExcuted()
        {
            if(m_vArgvPorts!=null)
            {
                foreach (var db in m_vArgvPorts)
                {
                    UpdatePortRuntimeValue(db);
                }
            }
            if (m_vReturnPorts != null)
            {
                foreach (var db in m_vReturnPorts)
                {
                    if (m_vOtherLinks.ContainsKey(db))
                        continue;
                    UpdatePortRuntimeValue(db);
                    if (db.bindPort == null)
                        return;
                    if (db.bindPort.connections != null)
                    {
                        foreach (var connect in db.bindPort.connections)
                        {
                            if (connect.input.source is ArvgPort)
                            {
                                ArvgPort inPort = connect.input.source as ArvgPort;
                                if (inPort.grapNode != null && inPort.grapNode is NewVariableNode)
                                {
                                    inPort.grapNode.FlashRed();
                                    inPort.grapNode.OnNotifyExcuted();
                                }
                            }
                        }
                    }
                }
            }
        }
        //------------------------------------------------------
        public void FlashRed(float duration = 1f)
        {
            if (m_isFlashing) return;
            m_isFlashing = true;
            m_lastBgColor = this.style.backgroundColor.value;
            this.style.backgroundColor = new Color(1f, 0.2f, 0.2f, 1f); // 红色

            m_flashEndTime = duration;
        }
        //------------------------------------------------------
        public void Update(float deltaTime)
        {
            if (m_isFlashing && m_flashEndTime > 0)
            {
                m_flashEndTime -= deltaTime;
                if (m_flashEndTime <= 0)
                {
                    this.style.backgroundColor = m_lastBgColor;
                    m_isFlashing = false;
                }
            }
        }
        //------------------------------------------------------
        public void Save()
        {
            bindNode.posX = (int)(this.GetPosition().x*100);
            bindNode.posY = (int)(this.GetPosition().y*100);
            List<short> nextActions = new List<short>();
            if(m_pLinkOut!=null)
            {
                var connections = m_pLinkOut.bindPort.connections;
                if(connections!=null)
                {
                    foreach(var db in connections)
                    {
                        if(db.input.source is LinkPort)
                        {
                            var input = (LinkPort)db.input.source;
                            if(!nextActions.Contains(input.graphNode.bindNode.guid)) nextActions.Add(input.graphNode.bindNode.guid);
                        }
                    }
                }
            }
            bindNode.nextActions = nextActions.ToArray();

            foreach(var db in m_vOtherLinks)
            {
                if (db.Value.argvPort == null)
                    continue;
                var pVar = m_pGraphView.GetVariable(db.Value.argvPort.GetVariableGuid());
                if (pVar == null)
                    continue;
                if(pVar is VariableInt)
                {
                    VariableInt pStr = (VariableInt)pVar;
                    List<int> vDo = new List<int>();
                    var connections = db.Value.bindPort.connections;
                    if (connections != null)
                    {
                        foreach (var con in connections)
                        {
                            if (con.input.source is LinkPort)
                            {
                                var input = (LinkPort)con.input.source;
                                if (!vDo.Contains(input.graphNode.bindNode.guid)) vDo.Add(input.graphNode.bindNode.guid);
                            }
                        }
                    }
                    if (vDo.Count > 0)
                        pStr.value = vDo[0];
                    else pStr.value = -1;
                        m_pGraphView.UpdateVariable(pStr);
                }
            }

            if(m_vArgvPorts!=null)
            {
                foreach(var db in m_vArgvPorts)
                {
                    var connections = db.bindPort.connections;
                    db.nodePort.dummyPorts = null;
                    if (connections != null)
                    {
                        List<AT.Runtime.DummyPort> vLinkPorts = new List<AT.Runtime.DummyPort>();
                        foreach (var conent in connections)
                        {
                            if (conent.output.source is ArvgPort)
                            {
                                var output = (ArvgPort)conent.output.source;
                                var nodePorts = db.grapNode.bindNode.GetInports(false);
                                if(nodePorts!=null && db.slotIndex>=0 && db.slotIndex < nodePorts.Length)
                                {
                                    AT.Runtime.DummyPort dummy = new AT.Runtime.DummyPort();
                                    dummy.guid = output.grapNode.bindNode.guid;
                                    dummy.type = (byte)(output.isInput?0:1);
                                    dummy.slotIndex = (byte)output.slotIndex;
                                    vLinkPorts.Add(dummy);
                                }
                            }
                        }
                        if (vLinkPorts.Count > 0) db.nodePort.dummyPorts = vLinkPorts.ToArray();
                    }
                }

                var inputVars = bindNode.GetInports(false);
                if(inputVars!=null)
                {
                    for (int i = 0; i < m_vArgvPorts.Count && i < inputVars.Length; ++i)
                    {
                        inputVars[i] = m_vArgvPorts[i].nodePort;
                        if (m_vOtherLinks.ContainsKey(m_vArgvPorts[i]))
                            continue;
                        m_pGraphView.UpdatePortVariableDefault(m_vArgvPorts[i]);
                    }
                }
            }
            if (m_vReturnPorts != null)
            {
                var portVars = bindNode.GetOutports(false);
                if (portVars != null)
                {
                    for (int i = 0; i < m_vReturnPorts.Count && i < portVars.Length; ++i)
                    {
                        portVars[i] = m_vReturnPorts[i].nodePort;
                        if (m_vOtherLinks.ContainsKey(m_vReturnPorts[i]))
                            continue;
                        m_pGraphView.UpdatePortVariableDefault(m_vReturnPorts[i]);
                    }
                }
            }
        }
        //------------------------------------------------------
        public virtual void AddTitlePorts(bool hasIn, bool hasOut)
        {
            // 创建左侧 input port
            if(hasIn)
            {
                m_pLinkIn = new LinkPort();
                m_pLinkIn.graphNode = this;

                var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, UnityEditor.Experimental.GraphView.Port.Capacity.Multi, typeof(LinkPort));
                inputPort.portName = ""; // 不显示名字
                inputPort.style.marginRight = 4;
                inputPort.style.backgroundImage = IconUtils.linkOuter;
                inputPort.style.backgroundColor = Color.clear;
                inputPort.style.width = 16;
                inputPort.style.height = 16;
                inputPort.portColor = EditorPreferences.GetSettings().linkLineColor;
                inputPort.source = m_pLinkIn;
                titleContainer.Insert(0, inputPort);
                m_pLinkIn.bindPort = inputPort;
            }


            // 创建右侧 output port
            if(hasOut)
            {
                m_pLinkOut = new LinkPort();
                m_pLinkOut.graphNode = this;
                var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Multi, typeof(LinkPort));
                outputPort.portName = ""; // 不显示名字
                outputPort.style.marginLeft = 4;
                outputPort.style.backgroundImage = IconUtils.linkOuter;
                outputPort.style.backgroundColor = Color.clear;
                outputPort.style.width = 16;
                outputPort.style.height = 16;
                outputPort.portColor = EditorPreferences.GetSettings().linkLineColor;
                outputPort.source = m_pLinkOut;
                titleContainer.Add(outputPort);
                m_pLinkOut.bindPort = outputPort;
            }
        }
        //------------------------------------------------------
        public AgentTreeAttri GetAttri()
        {
            if (bindNode == null) return null;
            int customType = 0;
            if(bindNode is AT.Runtime.CutsceneEvent)
            {
                customType = ((AT.Runtime.CutsceneEvent)bindNode).eventType;
            }
            return AgentTreeUtil.GetAttri(bindNode.type, customType);
        }
        //------------------------------------------------------
        public bool CanChangeValue(ArvgPort port)
        {
            return port.attri.canEdit;// && !m_pGraphView.isDebugPort;
        }
        //------------------------------------------------------
        protected virtual void OnArgvPortChanged(ArvgPort port)
        {
            if (port == null || port.bindPort == null)
                return;
        }
        //------------------------------------------------------
        protected void ClearPortVarEle(ArvgPort port)
        {
        }
        //------------------------------------------------------
        protected virtual void OnArgvPortVarTypeChanged(ArvgPort port)
        {
            if (port == null || port.bindPort == null)
                return;

            var val = m_pGraphView.ChangeVariable(port.GetVariableGuid(), port.eEnumType);
            // 需要重新赋值回去（struct）
            port.bindPort.portColor = EditorPreferences.GetTypeColor(val.GetType());
            port.nodePort.pVariable = val;

            ClearPortVarEle(port);


            InnerDrawPortValue(port, 2);
        }
        //------------------------------------------------------
        public void ChangeVariableType(ArvgPort port, Runtime.EVariableType newType)
        {
            var val2 = m_pGraphView.ChangeVariable(port.GetVariableGuid(), newType);
            port.nodePort.pVariable = val2;
        }
        //------------------------------------------------------
        public void CheckPorts()
        {
            foreach (var db in m_vArgvPorts)
                m_pGraphView.DelPort(db);

            foreach (var db in m_vReturnPorts)
                m_pGraphView.DelPort(db);

            m_vArgvPorts.Clear();
            m_vReturnPorts.Clear();
            this.inputContainer.Clear();
            this.outputContainer.Clear();

            CreateInports();
            CreateOutports();
        }
        //------------------------------------------------------
        protected virtual void CreateInports()
        {
            var attr = GetAttri();
            if (attr == null)
                return;
            if (attr.argvs.Count > 0)
            {
                var inputVars = bindNode.GetInports(true, attr.argvs.Count);
                if(inputVars!=null)
                {
                    for (int i = 0; i < attr.argvs.Count; ++i)
                    {
                        var nodePort = inputVars[i];

                        nodePort.pVariable = m_pGraphView.CreateVariable(attr.argvs[i], nodePort.varGuid);
                        nodePort.varGuid = nodePort.pVariable.GetGuid();
                        inputVars[i] = nodePort;

                        if (nodePort.pVariable == null)
                        {
                            m_vArgvPorts.Add(null);
                            continue;
                        }
                        ArvgPort port = new ArvgPort();
                        port.grapNode = this;
                        port.nodePort = nodePort;
                        port.attri = attr.argvs[i];
                        port.isInput = true;
                        port.slotIndex = i;
                        m_vArgvPorts.Add(port);
                        m_pGraphView.AddPort(port);
                    }
                }
            }
            else
                bindNode.GetInports(true, 0);

        }
        //------------------------------------------------------
        protected virtual void CreateOutports()
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
                    nodePort.pVariable = m_pGraphView.CreateVariable(attr.returns[i], nodePort.varGuid);
                    nodePort.varGuid = nodePort.pVariable.GetGuid();
                    outportVar[i] = nodePort;
                    if (nodePort.pVariable == null)
                    {
                        m_vReturnPorts.Add(null);
                        continue;
                    }
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
        void BuildPorts()
        {
            BuildArvPort();
            BuildReturnPort();
        }
        //------------------------------------------------------
        protected virtual void BuildArvPort()
        {
            this.inputContainer.Clear();
            for(int i =0; i < m_vArgvPorts.Count; ++i)
            {
                var port = m_vArgvPorts[i];
                var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ArvgPort));
                inputPort.portName = port.attri.name;
                inputPort.portColor = EditorPreferences.GetTypeColor(port.GetVariable().GetType());
                inputPort.source = port;
                inputPort.style.marginRight = 4;

                var fieldRoot = new VisualElement();
                fieldRoot.style.flexDirection = FlexDirection.Row;
                inputPort.Add(fieldRoot);

                this.inputContainer.Add(inputPort);

                m_vArgvPorts[i].fieldRoot = fieldRoot;
                m_vArgvPorts[i].bindPort = inputPort;
                DrawPortValue(port);
            }
        }
        //------------------------------------------------------
        protected virtual void BuildReturnPort()
        {
            var attris = GetAttri();
            this.outputContainer.Clear();
            for (int i = 0; i < m_vReturnPorts.Count; ++i)
            {
                var port = m_vReturnPorts[i];
                if (attris.linkAttributes.TryGetValue(attris.returns[i], out var linkAgvAttr))
                {
                    LinkPort linkPort = new LinkPort();
                    linkPort.graphNode = this;
                    linkPort.argvPort = port;
                    var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, UnityEditor.Experimental.GraphView.Port.Capacity.Single, typeof(LinkPort));
                    outputPort.portName = port.attri.name;
                    outputPort.style.marginLeft = 4;
                    outputPort.style.marginTop = (m_vReturnPorts.Count+m_vArgvPorts.Count-1)*16;
                    outputPort.style.backgroundImage = IconUtils.linkOuter;
                    outputPort.style.backgroundColor = Color.clear;
                    outputPort.style.width = 16;
                    outputPort.style.height = 16;
                    outputPort.portColor = EditorPreferences.GetSettings().linkLineColor;
                    outputPort.source = linkPort;
                    linkPort.bindPort  = outputPort;
                    m_vOtherLinks[port] = linkPort;

                    port.bindPort = outputPort;
                    this.outputContainer.Add(outputPort);
                }
                else
                {
                    var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ArvgPort));
                    inputPort.portName = port.attri.name;
                    inputPort.portColor = EditorPreferences.GetTypeColor(port.GetVariable().GetType());
                    inputPort.source = port;
                    inputPort.style.marginRight = 4;

                    var fieldRoot = new VisualElement();
                    fieldRoot.style.flexDirection = FlexDirection.Row;
                    inputPort.Add(fieldRoot);
                    this.inputContainer.Add(inputPort);

                    m_vReturnPorts[i].fieldRoot = fieldRoot;
                    m_vReturnPorts[i].bindPort = inputPort;
                    DrawPortValue(port);
                }
            }
        }
        //------------------------------------------------------
        protected virtual void DrawPortValue(ArvgPort port)
        {
            if(port.fieldRoot!=null)
            {
                if ((port.nodePort.dummyPorts != null && port.nodePort.dummyPorts.Length > 0) || !port.attri.canEdit ||
                    (port.bindPort.connections != null && port.bindPort.connections.Count() > 0))
                    port.fieldRoot.SetEnabled(false);
                else
                    port.fieldRoot.SetEnabled(true);
            }

            if (port.attri.argvType == typeof(AT.Runtime.IVariable))
            {
                InnerDrawPortValue(port);
                bool bDrawEnumPopVarType = true;
                if(port.slotIndex>0)
                {
                    // 如果是输入端口的第一个参数，且类型是变量类型，则不显示枚举类型选择
                    if (port.isInput && m_vArgvPorts[0].attri.argvType == typeof(AT.Runtime.IVariable))
                    {
                        bDrawEnumPopVarType = false;
                    }
                }
                if(bDrawEnumPopVarType && port.isInput)
                {
                    if (port.attri.limitVarTypes != null && port.attri.limitVarTypes.Length>0)
                    {
                        var popList = new List<AT.Runtime.EVariableType>(port.attri.limitVarTypes);
                        if (!popList.Contains(port.eEnumType))
                            port.eEnumType = popList[0];
                        var popup = new PopupField<AT.Runtime.EVariableType>(
                            popList,
                            port.eEnumType,
                            t => EditorUtils.GetEnumDisplayName(t),
                            t => EditorUtils.GetEnumDisplayName(t)
                        );
                        popup.style.width = 50;
                        popup.style.marginLeft = 4;
                        popup.style.unityTextAlign = TextAnchor.MiddleRight;

                        popup.SetEnabled((port.bindPort.connections == null || port.bindPort.connections.Count() <= 0) && port.fieldRoot.enabledSelf);

                        port.enumPopFieldElement = popup;

                        popup.RegisterValueChangedCallback(evt =>
                        {
                            port.eEnumType = evt.newValue;
                            for (int i = 0; i < m_vArgvPorts.Count; ++i)
                            {
                                m_vArgvPorts[i].eEnumType = port.eEnumType;
                            }
                            OnArgvPortVarTypeChanged(port);
                            UpdateSize();
                        });

                        port.bindPort.Add(popup);
                    }
                    else
                    {
                        var varTypeField = new EnumField(port.eEnumType)
                        {
                            style =
                        {
                            width = 50,
                            marginLeft = 4,
                            unityTextAlign = TextAnchor.MiddleRight
                        }
                        };
                        varTypeField.SetEnabled( (port.bindPort.connections == null || port.bindPort.connections.Count()<=0) && port.fieldRoot.enabledSelf );
                        varTypeField.RegisterValueChangedCallback(evt =>
                        {
                            // 更新变量值
                            if ((AT.Runtime.EVariableType)evt.newValue != Runtime.EVariableType.eNone)
                            {
                                port.eEnumType = (AT.Runtime.EVariableType)evt.newValue;
                                OnArgvPortVarTypeChanged(port);
                                UpdateSize();
                            }
                        });
                        port.enumPopFieldElement = varTypeField;
                        port.bindPort.Add(varTypeField);
                    }
                }
                return;
            }
            InnerDrawPortValue(port);
        }
        //------------------------------------------------------
        protected virtual void InnerDrawPortValue(ArvgPort port, int insertIndex = -1)
        {
            if(port.fieldElement!=null)
            {
                port.fieldRoot.Remove(port.fieldElement);
            }

            short varGuid = port.GetVariableGuid();
            var portVariable = m_pGraphView.GetVariable(varGuid);
            if (portVariable == null)
                return;

            if (portVariable is AT.Runtime.VariableInt)
            {
                if (port.attri.argvType.IsEnum)
                {
                    var intVar = (AT.Runtime.VariableInt)portVariable;
                    Enum enumVal = (Enum)Enum.ToObject(port.attri.argvType, intVar.value);
                    var varTypeField = new EnumField(enumVal)
                    {
                        style =
                        {
                            width = 50,
                            marginLeft = 4,
                            unityTextAlign = TextAnchor.MiddleRight
                        }
                    };
                    varTypeField.RegisterValueChangedCallback(evt =>
                    {
                        // 更新变量值
                        if(CanChangeValue(port))
                        {
                            intVar.value = Convert.ToInt32(evt.newValue);
                            OnArgvPortChanged(port);
                            // 需要重新赋值回去（struct）
                            m_pGraphView.UpdateVariable(intVar);
                        }
                    });
                    varTypeField.userData = port;
                    varTypeField.SetEnabled(port.attri.canEdit);
                    {
                        var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                        if (runtimeVar != null && runtimeVar is AT.Runtime.VariableInt)
                            varTypeField.value = (Enum)Enum.ToObject(port.attri.argvType, ((AT.Runtime.VariableInt)runtimeVar).value);
                    }
                    port.fieldRoot.Add(varTypeField);
                    port.fieldElement = varTypeField;
                }
                else
                {
                    var intVar = (AT.Runtime.VariableInt)portVariable;
                    var intField = new IntegerField
                    {
                        value = intVar.value,
                        style =
                        {
                            width = 50,
                            marginLeft = 4,
                            unityTextAlign = TextAnchor.MiddleRight
                        }
                    };
                    intField.RegisterValueChangedCallback(evt =>
                    {
                        if (CanChangeValue(port))
                        {
                            // 更新变量值
                            intVar.value = evt.newValue;
                            OnArgvPortChanged(port);
                            // 需要重新赋值回去（struct）
                            m_pGraphView.UpdateVariable(intVar);
                        }
                    });
                    intField.userData = port;
                    intField.SetEnabled(port.attri.canEdit);
                    if (!port.attri.canEdit)
                    {
                        var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                        if (runtimeVar != null && runtimeVar is AT.Runtime.VariableInt)
                            intField.value =  ((AT.Runtime.VariableInt)runtimeVar).value;
                    }
                    port.fieldRoot.Add(intField);
                    port.fieldElement = intField;
                }
            }
            if (portVariable is AT.Runtime.VariableObjId)
            {
                var intVar = (AT.Runtime.VariableObjId)portVariable;
                var intField = new IntegerField
                {
                    value = intVar.value.id,
                    style =
                        {
                            width = 50,
                            marginLeft = 4,
                            unityTextAlign = TextAnchor.MiddleRight
                        }
                };
                intField.RegisterValueChangedCallback(evt =>
                {
                    if (CanChangeValue(port))
                    {
                        // 更新变量值
                        intVar.value.id = evt.newValue;
                        OnArgvPortChanged(port);
                        // 需要重新赋值回去（struct）
                        m_pGraphView.UpdateVariable(intVar);
                    }
                });
                intField.userData = port;
                intField.SetEnabled(port.attri.canEdit);
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableObjId)
                        intField.value = ((AT.Runtime.VariableObjId)runtimeVar).value.id;
                }
                port.fieldRoot.Add(intField);
                port.fieldElement = intField;
            }
            else if (portVariable is AT.Runtime.VariableBool)
            {
                var intVar = (AT.Runtime.VariableBool)portVariable;
                var intField = new UnityEngine.UIElements.Toggle
                {
                    value = intVar.value,
                    style =
                        {
                            width = 50,
                            marginLeft = 4,
                            unityTextAlign = TextAnchor.MiddleRight
                        }
                };
                intField.userData = port;
                intField.SetEnabled(port.attri.canEdit);
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableBool)
                        intField.value = ((AT.Runtime.VariableBool)runtimeVar).value;
                }
                intField.RegisterValueChangedCallback(evt =>
                {
                    if (CanChangeValue(port))
                    {
                        // 更新变量值
                        intVar.value = evt.newValue;
                        OnArgvPortChanged(port);
                        // 需要重新赋值回去（struct）
                        m_pGraphView.UpdateVariable(intVar);
                    }
                });
                   
                port.fieldRoot.Add(intField);
                port.fieldElement = intField;
            }
            else if (portVariable is AT.Runtime.VariableFloat)
            {
                var intVar = (AT.Runtime.VariableFloat)portVariable;
                var intField = new FloatField
                {
                    value = intVar.value,
                    style =
                        {
                            width = 50,
                            marginLeft = 4,
                            unityTextAlign = TextAnchor.MiddleRight
                        }
                };
                intField.RegisterValueChangedCallback(evt =>
                {
                    if (CanChangeValue(port))
                    {
                        // 更新变量值
                        intVar.value = evt.newValue;
                        OnArgvPortChanged(port);
                        // 需要重新赋值回去（struct）
                        m_pGraphView.UpdateVariable(intVar);
                    }
                });
                intField.userData = port;
                intField.SetEnabled(port.attri.canEdit);
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableFloat)
                    {
                        intField.value = ((AT.Runtime.VariableFloat)runtimeVar).value;
                    }
                }
                port.fieldRoot.Add(intField);
                port.fieldElement = intField;
            }
            else if (portVariable is AT.Runtime.VariableString)
            {
                var intVar = (AT.Runtime.VariableString)portVariable;
                var intField = new UnityEngine.UIElements.TextField
                {
                    value = intVar.value,
                    style =
                        {
                            width = 120,
                            marginLeft = 4,
                            unityTextAlign = TextAnchor.MiddleRight
                        }
                };
                intField.userData = port;
                intField.SetEnabled(port.attri.canEdit);
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableString)
                        intField.value = ((AT.Runtime.VariableString)runtimeVar).value;
                }
                intField.RegisterValueChangedCallback(evt =>
                {
                    if(CanChangeValue(port))
                    {
                        // 更新变量值
                        intVar.value = evt.newValue;
                        OnArgvPortChanged(port);
                        // 需要重新赋值回去（struct）
                        m_pGraphView.UpdateVariable(intVar);
                    }
                });
                    
                port.fieldRoot.Add(intField);
                port.fieldElement = intField;
            }
            else if(portVariable is AT.Runtime.VariableVec2)
            {
                var temVar = (AT.Runtime.VariableVec2)portVariable;
                var intField = new UnityEditor.UIElements.Vector2Field
                {
                    value = temVar.value,
                    style =
                        {
                            width = 120,
                            marginLeft = 4,
                            unityTextAlign = TextAnchor.MiddleRight
                        }
                };
                intField.userData = port;
                intField.SetEnabled(port.attri.canEdit);
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableVec2)
                        intField.value = ((AT.Runtime.VariableVec2)runtimeVar).value;
                }
                intField.RegisterValueChangedCallback(evt =>
                {
                    if (CanChangeValue(port))
                    {
                        // 更新变量值
                        temVar.value = evt.newValue;
                        OnArgvPortChanged(port);
                        // 需要重新赋值回去（struct）
                        m_pGraphView.UpdateVariable(temVar);
                    }
                });
                   
                port.fieldRoot.Add(intField);
                port.fieldElement = intField;
            }
            else if (portVariable is AT.Runtime.VariableVec3)
            {
                var temVar = (AT.Runtime.VariableVec3)portVariable;
                var intField = new UnityEditor.UIElements.Vector3Field
                {
                    value = temVar.value,
                    style =
                        {
                            width = 200,
                            marginLeft = 4,
                            unityTextAlign = TextAnchor.MiddleRight
                        }
                };
                intField.userData = port;
                intField.SetEnabled(port.attri.canEdit);
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableVec3)
                        intField.value = ((AT.Runtime.VariableVec3)runtimeVar).value;
                }
                intField.RegisterValueChangedCallback(evt =>
                {
                    if (CanChangeValue(port))
                    {
                        // 更新变量值
                        temVar.value = evt.newValue;
                        // 需要重新赋值回去（struct）
                        OnArgvPortChanged(port);
                        m_pGraphView.UpdateVariable(temVar);
                    }
                });
                
                port.fieldRoot.Add(intField);
                port.fieldElement = intField;
            }
            else if (portVariable is AT.Runtime.VariableVec4)
            {
                var temVar = (AT.Runtime.VariableVec4)portVariable;
                var intField = new UnityEditor.UIElements.Vector4Field
                {
                    value = temVar.value,
                    style =
                        {
                            width = 260,
                            marginLeft = 4,
                            unityTextAlign = TextAnchor.MiddleRight
                        }
                };
                intField.userData = port;
                intField.SetEnabled(port.attri.canEdit);
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableVec4)
                        intField.value = ((AT.Runtime.VariableVec4)runtimeVar).value;
                }
                intField.RegisterValueChangedCallback(evt =>
                {
                    if (CanChangeValue(port))
                    {
                        // 更新变量值
                        temVar.value = evt.newValue;
                        OnArgvPortChanged(port);
                        // 需要重新赋值回去（struct）
                        m_pGraphView.UpdateVariable(temVar);
                    }
                });
                 
                port.fieldRoot.Add(intField);
                port.fieldElement = intField;
            }
        }
        //-----------------------------------------------------
        protected virtual void UpdatePortRuntimeValue(ArvgPort port)
        {
            if (port.fieldElement == null)
                return;
            short varGuid = port.GetVariableGuid();
            var portVariable = m_pGraphView.GetVariable(varGuid);
            if (portVariable == null)
                return;

            if (m_vOtherLinks.ContainsKey(port))
                return;

            //  if (port.attri.canEdit)
            //       return;

            if (portVariable is AT.Runtime.VariableInt)
            {
                if (port.attri.argvType.IsEnum)
                {
                    if (port.fieldElement is EnumField)
                    {
                        var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                        if (runtimeVar != null && runtimeVar is AT.Runtime.VariableInt)
                        {
                            EnumField enumField = (EnumField)port.fieldElement;
                            enumField.value = (Enum)Enum.ToObject(port.attri.argvType, ((AT.Runtime.VariableInt)runtimeVar).value);
                        }
                    }
                }
                else
                {
                    if (port.fieldElement is IntegerField)
                    {
                        var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                        if (runtimeVar != null && runtimeVar is AT.Runtime.VariableInt)
                        {
                            IntegerField eleField = (IntegerField)port.fieldElement;
                            eleField.value = ((AT.Runtime.VariableInt)runtimeVar).value;
                        }
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableObjId)
            {
                if (port.fieldElement is IntegerField)
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableObjId)
                    {
                        IntegerField eleField = (IntegerField)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableObjId)runtimeVar).value.id;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableBool)
            {
                if (port.fieldElement is Toggle)
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableBool)
                    {
                        Toggle eleField = (Toggle)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableBool)runtimeVar).value;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableFloat)
            {
                if (port.fieldElement is FloatField)
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableFloat)
                    {
                        FloatField eleField = (FloatField)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableFloat)runtimeVar).value;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableString)
            {
                if (port.fieldElement is TextField)
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableString)
                    {
                        TextField eleField = (TextField)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableString)runtimeVar).value;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableVec2)
            {
                if (port.fieldElement is Vector2Field)
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableVec2)
                    {
                        Vector2Field eleField = (Vector2Field)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableVec2)runtimeVar).value;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableVec3)
            {
                if (port.fieldElement is Vector3Field)
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableVec3)
                    {
                        Vector3Field eleField = (Vector3Field)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableVec3)runtimeVar).value;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableVec4)
            {
                if (port.fieldElement is Vector4Field)
                {
                    var runtimeVar = m_pGraphView.GetRuntimeVariable(port);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableVec4)
                    {
                        Vector4Field eleField = (Vector4Field)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableVec4)runtimeVar).value;
                    }
                }
            }
        }
        //-----------------------------------------------------
        public virtual void SetPortDefalueValue(ArvgPort port)
        {
            if (port.fieldElement == null)
                return;
            short varGuid = port.GetVariableGuid();
            var portVariable = m_pGraphView.GetVariable(varGuid);
            if (portVariable == null)
                return;

      //      if (port.attri.canEdit)
      //          return;

            if (portVariable is AT.Runtime.VariableInt)
            {
                if (port.attri.argvType.IsEnum)
                {
                    if (port.fieldElement is EnumField)
                    {
                        m_pGraphView.UpdatePortVariableDefault(port);
                        var runtimeVar = m_pGraphView.GetVariable(varGuid);
                        if (runtimeVar != null && runtimeVar is AT.Runtime.VariableInt)
                        {
                            EnumField enumField = (EnumField)port.fieldElement;
                            enumField.value = (Enum)Enum.ToObject(port.attri.argvType, ((AT.Runtime.VariableInt)runtimeVar).value);
                        }
                    }
                }
                else
                {
                    if (port.fieldElement is IntegerField)
                    {
                        m_pGraphView.UpdatePortVariableDefault(port);
                        var runtimeVar = m_pGraphView.GetVariable(varGuid);
                        if (runtimeVar != null && runtimeVar is AT.Runtime.VariableInt)
                        {
                            IntegerField eleField = (IntegerField)port.fieldElement;
                            eleField.value = ((AT.Runtime.VariableInt)runtimeVar).value;
                        }
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableObjId)
            {
                if (port.fieldElement is IntegerField)
                {
                    m_pGraphView.UpdatePortVariableDefault(port);
                    var runtimeVar = m_pGraphView.GetVariable(varGuid);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableObjId)
                    {
                        IntegerField eleField = (IntegerField)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableObjId)runtimeVar).value.id;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableBool)
            {
                if (port.fieldElement is Toggle)
                {
                    m_pGraphView.UpdatePortVariableDefault(port);
                    var runtimeVar = m_pGraphView.GetVariable(varGuid);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableBool)
                    {
                        Toggle eleField = (Toggle)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableBool)runtimeVar).value;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableFloat)
            {
                if (port.fieldElement is FloatField)
                {
                    m_pGraphView.UpdatePortVariableDefault(port);
                    var runtimeVar = m_pGraphView.GetVariable(varGuid);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableFloat)
                    {
                        FloatField eleField = (FloatField)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableFloat)runtimeVar).value;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableString)
            {
                if (port.fieldElement is TextField)
                {
                    m_pGraphView.UpdatePortVariableDefault(port);
                    var runtimeVar = m_pGraphView.GetVariable(varGuid);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableString)
                    {
                        TextField eleField = (TextField)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableString)runtimeVar).value;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableVec2)
            {
                if (port.fieldElement is Vector2Field)
                {
                    m_pGraphView.UpdatePortVariableDefault(port);
                    var runtimeVar = m_pGraphView.GetVariable(varGuid);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableVec2)
                    {
                        Vector2Field eleField = (Vector2Field)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableVec2)runtimeVar).value;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableVec3)
            {
                if (port.fieldElement is Vector3Field)
                {
                    m_pGraphView.UpdatePortVariableDefault(port);
                    var runtimeVar = m_pGraphView.GetVariable(varGuid);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableVec3)
                    {
                        Vector3Field eleField = (Vector3Field)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableVec3)runtimeVar).value;
                    }
                }
            }
            else if (portVariable is AT.Runtime.VariableVec4)
            {
                if (port.fieldElement is Vector4Field)
                {
                    m_pGraphView.UpdatePortVariableDefault(port);
                    var runtimeVar = m_pGraphView.GetVariable(varGuid);
                    if (runtimeVar != null && runtimeVar is AT.Runtime.VariableVec4)
                    {
                        Vector4Field eleField = (Vector4Field)port.fieldElement;
                        eleField.value = ((AT.Runtime.VariableVec4)runtimeVar).value;
                    }
                }
            }
        }
    }
}
#endif