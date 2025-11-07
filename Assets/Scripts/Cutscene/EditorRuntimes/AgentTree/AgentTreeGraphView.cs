/********************************************************************
生成日期:	06:30:2025
类    名: 	AgentTreeGraphView
作    者:	HappLI
描    述:	行为树视图绘制视图
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using Framework.Cutscene.Editor;
using Framework.Cutscene.Runtime;
using Framework.ED;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Framework.AT.Editor
{
    public class AgentTreeGraphView : GraphView , IAgentTreeCallback
    {
        bool m_bLastPlaying = false;
        CutsceneObject m_pObject;
        AgentTreeData m_pAgentTreeData = null;
        AEditorLogic m_pOwnerEditorLogic;
        Dictionary<BaseNode, GraphNode> m_vNodes = new Dictionary<BaseNode, GraphNode>();
        Dictionary<short, GraphNode> m_vGuidNodes = new Dictionary<short, GraphNode>();
        Dictionary<short, IVariable> m_vVariables = new Dictionary<short, IVariable>();
        Dictionary<long, ArvgPort> m_vPorts = new Dictionary<long, ArvgPort>();
        public AgentTreeGraphView(AEditorLogic pOwner)
        {
            m_pOwnerEditorLogic = pOwner;
            // 允许对Graph进行Zoom in/out
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            // 允许拖拽Content
            this.AddManipulator(new ContentDragger());
            // 允许Selection里的内容
            this.AddManipulator(new SelectionDragger());
            // GraphView允许进行框选
            this.AddManipulator(new RectangleSelector());

            // 添加右键菜单
            this.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                evt.menu.InsertAction(0,"执行", (a) =>
                {
                    var agentTree = GetCurrentRuntimeAgentTree();
                    if(agentTree == null)
                    {
                        CutsceneGraph graph = new CutsceneGraph();
                        AgentTreeData pData = new AgentTreeData();
                        Save(pData);
                        pData.Init(true);
                        graph.agentTree = pData;
                        if(GetCurrentCutscene().CreateAgentTree(graph))
                        {
                            GetCurrentCutscene().GetAgentTree().RegisterCallback(this);
                            GetCurrentCutscene().Enable(true);
                        }
                    }
                    else
                    {
                        if (agentTree != null)
                        {
                            AgentTreeData pData = new AgentTreeData();
                            Save(pData);
                            pData.Init(true);
                            agentTree.Create(pData);
                            agentTree.RegisterCallback(this);
                            agentTree.Enable(true);
                            GetCurrentCutscene().Enable(true);
                        }
                    }
       
                });
                evt.menu.InsertAction(1,"停止", (a) =>
                {
                    var agentTree = GetCurrentRuntimeAgentTree();
                    if (agentTree != null)
                        agentTree.Enable(false);
                    UpdatePortsVariableDefault();
                });
            }));

            var menuWindowProvider = (AgentTreeSearcher)ScriptableObject.CreateInstance<AgentTreeSearcher>();
            menuWindowProvider.ownerGraphView = this;
            menuWindowProvider.OnSelectEntryHandler = OnMenuSelectEntry;
            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), menuWindowProvider);
            };
            this.graphViewChanged += OnGraphViewChanged;
        }
        //-----------------------------------------------------
        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    OnEdgeConnected(edge);
                }
            }
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is GraphNode node)
                    {
                        RemoveNode(node.bindNode);
                    }
                    else if (element is Edge)
                    {
                        OnEdgeDisconnected((Edge)element);
                    }
                }
            }
            return change;
        }
        //-----------------------------------------------------
        public bool isPlaying
        {
            get
            {
                var agentTree = GetCurrentRuntimeAgentTree();
                if (agentTree != null)
                    return agentTree.IsEnable();
                return false;
            }
        }
        //-----------------------------------------------------
        void OnEdgeConnected(Edge edge)
        {
            var outputPort = edge.output;
            var inputPort = edge.input;
            if (inputPort != null && inputPort.source != null && inputPort.source is ArvgPort)
            {
                ArvgPort arvPort = inputPort.source as ArvgPort;
                arvPort.fieldRoot.SetEnabled(false);
                if(arvPort.enumPopFieldElement!=null)
                    arvPort.enumPopFieldElement.SetEnabled(arvPort.fieldRoot.enabledSelf);
            }
        }
        //-----------------------------------------------------
        void OnEdgeDisconnected(Edge edge)
        {
            var outputPort = edge.output;
            var inputPort = edge.input;
            if (inputPort != null && inputPort.connections.Count() <= 1 && inputPort.source != null && inputPort.source is ArvgPort)
            {
                ArvgPort arvPort = inputPort.source as ArvgPort;
                arvPort.fieldRoot.SetEnabled(arvPort.attri.canEdit); 
                if (arvPort.enumPopFieldElement != null)
                    arvPort.enumPopFieldElement.SetEnabled(arvPort.fieldRoot.enabledSelf);
            }
        }
        //-----------------------------------------------------
        private bool OnMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            BaseNode newNode = null;
            if (searchTreeEntry.userData is AgentTreeAttri)
            {
                AgentTreeAttri attri = searchTreeEntry.userData as AgentTreeAttri;

                if(attri.isCutsceneCustomEvent)
                {
                    CutsceneEvent pAction = new CutsceneEvent();
                    pAction.type = attri.actionType;
                    pAction.eventType = attri.cutsceneCusomtType;
                    pAction.guid = GeneratorGUID();
                    pAction.nextActions = null;
                    newNode = pAction;
                }
                else if (attri.actionAttr.isTask)
                {
                    if(attri.actionType <= (int)ETaskType.eTaskEndId && HasTaskNode((ETaskType)attri.actionType))
                    {
                        m_pOwnerEditorLogic.GetOwner().ShowNotification(new GUIContent("Task node already exists in the tree."), 2.0f);
                        return false;
                    }
                    EnterTask pTask = new EnterTask();
                    pTask.type = attri.actionType;
                    pTask.guid = GeneratorGUID();
                    pTask.nextActions = null;
                    newNode = pTask;
                }
                else
                {
                    AT.Runtime.ActionNode pAction = new AT.Runtime.ActionNode();
                    pAction.type = attri.actionType;
                    pAction.guid = GeneratorGUID();
                    pAction.nextActions = null;
                    newNode = pAction;
                }
            }
            else if (searchTreeEntry.userData is EventAttriData)
            {
                EventAttriData attri = searchTreeEntry.userData as EventAttriData;
            }
            else if (searchTreeEntry.userData is ArvgPort)
            {
                ArvgPort arcgPort = searchTreeEntry.userData as ArvgPort;
                if(!arcgPort.isInput)
                {
                    AT.Runtime.ActionNode pAction = new AT.Runtime.ActionNode();
                    pAction.type = (int)EActionType.eGetVariable;
                    pAction.guid = GeneratorGUID();
                    var nodePorts = pAction.GetOutports(true, 1);
                    nodePorts[0].varGuid = arcgPort.GetVariableGuid();
                    pAction.nextActions = null;
                    newNode = pAction;
                }
            }
            if (newNode == null)
                return false;


            Vector2 windowPos = context.screenMousePosition;
            if (m_pOwnerEditorLogic.GetOwner() != null)
                windowPos -= m_pOwnerEditorLogic.GetOwner().position.position;
            Vector2 graphPos = this.contentViewContainer.WorldToLocal(windowPos);
            newNode.posX = (int)((graphPos.x)*100);
            newNode.posY = (int)((graphPos.y)*100);
            var viewNode = AddNode(newNode);

            return true;
        }
        //--------------------------------------------------------
        public void OnEnable(AEditorLogic logic)
        {
            m_pOwnerEditorLogic = logic;
        }
        //-----------------------------------------------------
        public AgentTree GetCurrentRuntimeAgentTree()
        {
            var cutsceneInstance = m_pOwnerEditorLogic.GetOwner<AgentTreeWindow>().GetCutsceneInstance();
            if (cutsceneInstance == null)
                return null;
            return cutsceneInstance.GetAgentTree();
        }
        //-----------------------------------------------------
        public CutsceneInstance GetCurrentCutscene()
        {
            return m_pOwnerEditorLogic.GetOwner<AgentTreeWindow>().GetCutsceneInstance();
        }
        //-----------------------------------------------------
        public IVariable GetRuntimeVariable(ArvgPort port)
        {
            var agentTree = GetCurrentRuntimeAgentTree();
            if (agentTree == null) return null;
            var returnVal = agentTree.GetVariable(port.GetVariableGuid(), true);
            if(returnVal ==  null && port.nodePort.dummyPorts!=null && port.nodePort.dummyPorts.Length>0)
            {
               var dummyPort =  agentTree.GetDummyPort(port.grapNode.bindNode, port.slotIndex, port.isInput);
                if(dummyPort.IsValid())
                {
                    NodePort[] ports = null;
                    if (dummyPort.type == 0) ports = dummyPort.pNode.GetInports();
                    else ports = dummyPort.pNode.GetOutports();
                    if(ports!=null)
                        returnVal = agentTree.GetVariable(ports[dummyPort.slotIndex].varGuid, true);
                }
            }
            return returnVal;
        }
        //-----------------------------------------------------
        public CutsceneGraph GetCutsceneGraph()
        {
            if (m_pObject != null) return m_pObject.GetCutsceneGraph();
            return null;
        }
        //-----------------------------------------------------
        public void SetAgentTree(CutsceneObject pObj, AgentTreeData pAgentTree)
        {
            var cutscene = GetCurrentCutscene();
            cutscene.CreateAgentTree(pObj.GetCutsceneGraph());
            var agentTree = cutscene.GetAgentTree();
            if (agentTree != null)
                agentTree.RegisterCallback(this);
            m_pObject = pObj;
            m_pAgentTreeData = pAgentTree;
            m_vVariables = pAgentTree.GetVariableGUIDs();
            if (pAgentTree.tasks!=null)
            {
                for(int i =0; i < pAgentTree.tasks.Length; ++i)
                {
                    AddNode(pAgentTree.tasks[i]);
                }
            }
            if (pAgentTree.actions != null)
            {
                for (int i = 0; i < pAgentTree.actions.Length; ++i)
                {
                    AddNode(pAgentTree.actions[i]);
                }
            }
            if (pAgentTree.events != null)
            {
                for (int i = 0; i < pAgentTree.events.Length; ++i)
                {
                    AddNode(pAgentTree.events[i]);
                }
            }
            CreateLinkLine();
        }
        //--------------------------------------------------------
        public GraphNode AddNode(BaseNode pNode)
        {
            if (m_vNodes.TryGetValue(pNode, out var viewNode))
                return viewNode;

            int customType = 0;
            if(pNode is CutsceneEvent)
            {
                customType = ((CutsceneEvent)pNode).eventType;
            }
            GraphNode graphNode = null;
            var attri = AgentTreeUtil.GetAttri(pNode.type, customType);
            if(attri!=null && attri.graphNodeType!=null)
            {
                graphNode = (GraphNode)Activator.CreateInstance(attri.graphNodeType, this, pNode, true);
            }
            else
                graphNode = new GraphNode(this, pNode);
            AddElement(graphNode);
            graphNode.UpdatePosition();
            m_vNodes.Add(pNode, graphNode);
            m_vGuidNodes[pNode.guid] = graphNode;
            return graphNode;
        }
        //--------------------------------------------------------
        public GraphNode GetNode(BaseNode pNode)
        {
            if (m_vNodes.TryGetValue(pNode, out var graphNode))
            {
                return graphNode;
            }
            return null;
        }
        //--------------------------------------------------------
        internal bool HasTaskNode(ETaskType type)
        {
            foreach (var db in m_vNodes)
            {
                if(db.Value.bindNode is EnterTask)
                {
                    if (((EnterTask)db.Value.bindNode).type == (int)type)
                        return true;
                }
            }
            return false;
        }
        //--------------------------------------------------------
        public GraphNode GetNode(short guid)
        {
            if (m_vGuidNodes.TryGetValue(guid, out var graphNode))
            {
                return graphNode;
            }
            return null;
        }
        //--------------------------------------------------------
        public void RemoveNode(AT.Runtime.BaseNode pNode)
        {
            if(m_vNodes.TryGetValue(pNode, out var graphNode))
            {
                graphNode.Release();
                this.RemoveElement(graphNode);
                m_vNodes.Remove(pNode);
                m_vGuidNodes.Remove(pNode.guid);
            }
        }
        //--------------------------------------------------------
        public void OnDisable()
        {

        }
        //--------------------------------------------------------
        public void OnGUI(Rect rect)
        {
        }
        //--------------------------------------------------------
        public void OnSaveChanges()
        {
            if (m_pAgentTreeData == null)
                return;
            if(isPlaying)
            {
                UpdatePortsVariableDefault();
            }

            Save(m_pAgentTreeData);
            //! 裁剪没用到的变量

            if (isPlaying)
            {
                var agentTree = GetCurrentRuntimeAgentTree();
                if (agentTree != null)
                {
                    foreach (var db in m_vNodes)
                    {
                        if (agentTree.IsExecuted(db.Value.bindNode.guid))
                        {
                            db.Value.OnNotifyExcuted();
                        }
                    }
                }
            }
            if (m_pObject)
            {
                m_pObject.GetCutsceneGraph().agentTree = m_pAgentTreeData;
            }
        }
        //-----------------------------------------------------
        public void OnEnableCutscene(CutsceneInstance pCutscene, bool bEnable)
        {
            if (pCutscene.GetAgentTree() == null)
                return;
            if (bEnable)
            {
                if (pCutscene.GetAgentTree().GetData() == m_pAgentTreeData)
                    pCutscene.GetAgentTree().RegisterCallback(this);
            }
            else
                pCutscene.GetAgentTree().UnregisterCallback(this);
        }
        //-----------------------------------------------------
        public void Save(AgentTreeData pData)
        {
            List<EnterTask> vTasks = new List<EnterTask>();
            List<AT.Runtime.ActionNode> vActions = new List<AT.Runtime.ActionNode>();
            List<CutsceneEvent> vCutsceneEvent = new List<CutsceneEvent>();
            foreach (var db in m_vNodes)
            {
                var graphNode = db.Value;

                graphNode.Save();

                if (graphNode.bindNode is EnterTask)
                {
                    vTasks.Add(graphNode.bindNode as EnterTask);
                }
                else if (graphNode.bindNode is CutsceneEvent)
                {
                    vCutsceneEvent.Add(graphNode.bindNode as CutsceneEvent);
                }
                else if (graphNode.bindNode is AT.Runtime.ActionNode)
                {
                    vActions.Add(graphNode.bindNode as AT.Runtime.ActionNode);
                }
            }
            if (vTasks.Count > 0) pData.tasks = vTasks.ToArray();
            else pData.tasks = null;
            if (vActions.Count > 0) pData.actions = vActions.ToArray();
            else pData.actions = null;
            if (vCutsceneEvent.Count > 0) pData.events = vCutsceneEvent.ToArray();
            else pData.events = null;
            CollectAllNodeVariables();
            var guidVars = pData.GetVariableGUIDs();
            if (guidVars != m_vVariables)
            {
                pData.SetVariables(m_vVariables);
            }
            pData.Serialize(false);
        }
        //-----------------------------------------------------
        public void CollectAllNodeVariables()
        {
            // 1. 收集所有节点端口上的变量
            HashSet<short> usedGuids = new HashSet<short>();
            foreach (var nodePair in m_vNodes)
            {
                var graphNode = nodePair.Value;
                // 输入端口
                var inPorts = graphNode.GetArvgs();
                if (inPorts != null)
                {
                    foreach (var port in inPorts)
                    {
                        var variable = port?.GetVariable();
                        if (variable != null)
                            usedGuids.Add(variable.GetGuid());
                    }
                }
                // 输出端口
                var outPorts = graphNode.GetReturns();
                if (outPorts != null)
                {
                    foreach (var port in outPorts)
                    {
                        var variable = port?.GetVariable();
                        if (variable != null)
                            usedGuids.Add(variable.GetGuid());
                    }
                }
            }

            // 2. 保留 m_vVariables 中被节点引用的变量，移除未被引用的
            var allGuids = m_vVariables.Keys.ToList();
            foreach (var guid in allGuids)
            {
                if (!usedGuids.Contains(guid))
                    m_vVariables.Remove(guid);
            }

            // 3. 补充节点引用但 m_vVariables 中没有的变量
            foreach (var guid in usedGuids)
            {
                if (!m_vVariables.ContainsKey(guid))
                {
                    // 尝试从节点端口获取变量并加入
                    foreach (var nodePair in m_vNodes)
                    {
                        var graphNode = nodePair.Value;
                        var inPorts = graphNode.GetArvgs();
                        if (inPorts != null)
                        {
                            foreach (var port in inPorts)
                            {
                                var variable = port?.GetVariable();
                                if (variable != null && variable.GetGuid() == guid)
                                {
                                    m_vVariables[guid] = variable;
                                    break;
                                }
                            }
                        }
                        var outPorts = graphNode.GetReturns();
                        if (outPorts != null)
                        {
                            foreach (var port in outPorts)
                            {
                                var variable = port?.GetVariable();
                                if (variable != null && variable.GetGuid() == guid)
                                {
                                    m_vVariables[guid] = variable;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        //------------------------------------------------------
        public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            if (startAnchor.source != null)
            {
                foreach (var port in ports.ToList())
                {
                    if (startAnchor.node == port.node ||
                        startAnchor.direction == port.direction ||
                        startAnchor.portType != port.portType)
                    {
                        continue;
                    }

                    if (port.source == null)
                        continue;
                    if (port.source.GetType() != startAnchor.source.GetType())
                        continue;

                    if(port.source is ArvgPort)
                    {
                        var srcPort = (ArvgPort)port.source;
                        var starPort = (ArvgPort)startAnchor.source;

                        if (srcPort.GetVariable() == null || starPort.GetVariable() == null)
                            continue;

                        if (srcPort.GetVariableGuid() == starPort.GetVariableGuid())
                            continue;
                        if (GetVariableOwnerNode(srcPort.GetVariableGuid()) == GetVariableOwnerNode(starPort.GetVariableGuid()))
                            continue;

                        if (srcPort.GetVariable().GetType() != starPort.GetVariable().GetType())
                            continue;
                    }
                    else if (port.source is LinkPort)
                    {
                    }
                    else
                        continue;
                    compatiblePorts.Add(port);
                }
            }

            return compatiblePorts;
        }
        //-----------------------------------------------------
        void CreateLinkLine()
        {
            foreach(var db in m_vNodes)
            {
                var graphNode = db.Value;
                var linkOutPort = graphNode.GetLink(false);
                if (linkOutPort!=null && graphNode.bindNode.nextActions != null)
                {
                    for(int i =0; i < graphNode.bindNode.nextActions.Length; ++i)
                    {
                        GraphNode linkNode = GetNode(graphNode.bindNode.nextActions[i]);
                        if (linkNode == null)
                            continue;
                        if (linkNode == graphNode)
                            continue;

                        var linkInPort = linkNode.GetLink(true);
                        if(linkInPort!=null)
                        {
                            var edge = new Edge
                            {
                                output = linkOutPort.bindPort,
                                input = linkInPort.bindPort
                            };
                            edge.edgeControl.inputColor = edge.edgeControl.outputColor = EditorPreferences.GetSettings().linkLineColor;
                            edge.edgeControl.capRadius = EditorPreferences.GetSettings().linkLineWidth;
                            linkOutPort.bindPort.Connect(edge);
                            linkInPort.bindPort.Connect(edge);
                            AddElement(edge);
                        }
                    }
                }

                var otherLinks = graphNode.GetOtherLinks();
                foreach(var othLink in otherLinks)
                {
                    var pVar = othLink.Key.GetVariable();
                    if(pVar is VariableInt)
                    {
                        VariableInt pStr = (VariableInt)pVar;
                        //if(!string.IsNullOrEmpty(pStr.value))
                        {
                         //   string[] splits = pStr.value.Split(new char[] { ';', '|' });
                          //  for(int i =0; i < splits.Length; ++i)
                            {
                                short guidTemp = (short)pStr.value;
                         //       if (short.TryParse(splits[i], out var guidTemp))
                                {
                                    GraphNode linkNode = GetNode(guidTemp);
                                    if (linkNode == null)
                                        continue;
                                    if (linkNode == graphNode)
                                        continue;

                                    var linkInPort = linkNode.GetLink(true);
                                    if (linkInPort != null)
                                    {
                                        var edge = new Edge
                                        {
                                            output = othLink.Value.bindPort,
                                            input = linkInPort.bindPort
                                        };
                                        edge.edgeControl.inputColor = edge.edgeControl.outputColor = EditorPreferences.GetSettings().linkLineColor;
                                        edge.edgeControl.capRadius = EditorPreferences.GetSettings().linkLineWidth;
                                        othLink.Value.bindPort.Connect(edge);
                                        linkInPort.bindPort.Connect(edge);
                                        AddElement(edge);
                                    }
                                }
                            }
                        }
                    }
                }

                var inports = graphNode.GetArvgs();
                if(inports!=null)
                {
                    for(int i =0;i < inports.Count; ++i)
                    {
                        int dummyCnt = inports[i].GetDummyCnt();
                        for(int j =0; j < dummyCnt; ++j)
                        {         
                            long linkKey = inports[i].GetLinkPortKey(j);
                            var linkInPort = GetPort(linkKey);
                            if (linkInPort != null && linkInPort.grapNode != graphNode)
                            {
                                var edge = new Edge
                                {
                                    output = linkInPort.bindPort,
                                    input = inports[i].bindPort
                                };
                                edge.edgeControl.capRadius = EditorPreferences.GetSettings().linkLineWidth;
                                linkInPort.bindPort.Connect(edge);
                                inports[i].bindPort.Connect(edge);
                                AddElement(edge);
                            }
                        }
                    }
                }
            }
        }
        //------------------------------------------------------
        public void OnEvent(Rect rect, Event e)
        {
        }
        //-----------------------------------------------------
        public void OnUpdate(float fTime)
        {
            foreach (var db in m_vNodes)
            {
                db.Value.Update(fTime);
            }
            if(m_bLastPlaying != isPlaying)
            {
                UpdatePortsVariableDefault();
                if(isPlaying)
                {
                    var agentTree = GetCurrentRuntimeAgentTree();
                    if(agentTree!=null)
                    {
                        foreach(var db in m_vNodes)
                        {
                            if(agentTree.IsExecuted(db.Value.bindNode.guid))
                            {
                                db.Value.OnNotifyExcuted();
                            }
                        }
                    }
                }
                m_bLastPlaying = isPlaying;
            }
        }
        //-----------------------------------------------------
        public void CreateActionNode(object val)
        {

        }
        //-----------------------------------------------------
        void Repaint()
        {
            m_pOwnerEditorLogic.Repaint();
        }
        //------------------------------------------------------
        short GeneratorGUID()
        {
            short guid = 1;
            int maxStack = 1000000;
            while(m_vGuidNodes.ContainsKey(guid))
            {
                guid++;
                maxStack--;
                if (maxStack <= 0) break;
            }
            return guid;
        }
        //-----------------------------------------------------
        public List<ArvgPort> GetArvgPorts()
        {
            List<ArvgPort> vPorts = new List<ArvgPort>();
            foreach(var db in m_vNodes)
            {
                var ports = db.Value.GetArvgs();
                if (ports != null)
                    vPorts.AddRange(ports);

                ports = db.Value.GetReturns();
                if (ports != null)
                    vPorts.AddRange(ports);
            }
            return vPorts;
        }
        //-----------------------------------------------------
        public IVariable CreateVariable(ArgvAttribute pAttr,short guid = 0)
        {
            if(guid!=0)
            {
                if (m_vVariables.TryGetValue(guid, out var pExistVar))
                    return pExistVar;
            }
            guid = GeneratorVarGUID();
            IVariable pVar = VariableUtil.CreateVariable(pAttr, guid);
            m_vVariables[guid] = pVar;
            return pVar;
        }
        //------------------------------------------------------
        public void UpdateVariable(IVariable variable)
        {
            // 如果行为树已经启用，则不允许添加新的变量
            if (isPlaying || m_bLastPlaying)
                return;
            m_vVariables[variable.GetGuid()] = variable;
        }
        //-----------------------------------------------------
        public IVariable ChangeVariable(short guid, EVariableType newType)
        {
            m_vVariables.Remove(guid);
            IVariable val = VariableUtil.CreateVariable(newType, guid);
            m_vVariables[guid] = val;
            return val;
        }
        //-----------------------------------------------------
        public IVariable GetVariable(short guid)
        {
            if (m_vVariables.TryGetValue(guid, out var variable))
            {
                return variable;
            }
            return null;
        }
        //-----------------------------------------------------
        public void UpdatePortVariableDefault(ArvgPort port)
        {
            if(!port.attri.canEdit)
            {
                var portVar = GetVariable(port.GetVariableGuid());
                if(portVar!=null)
                {
                    portVar = VariableUtil.CreateVariable(port.attri, port.GetVariableGuid());
                    m_vVariables[port.GetVariableGuid()] = portVar;
                }
            }
        }
        //-----------------------------------------------------
        public void UpdatePortsVariableDefault()
        {
            foreach (var db in m_vNodes)
            {
                var argvPorts = db.Value.GetArvgs();
                if (argvPorts != null)
                {
                    foreach (var sub in argvPorts)
                    {
                        db.Value.SetPortDefalueValue(sub);
                    }
                }
                var returnPort = db.Value.GetReturns();
                if (returnPort != null)
                {
                    foreach (var sub in returnPort)
                    {
                        db.Value.SetPortDefalueValue(sub);
                    }
                }
            }
        }
        //------------------------------------------------------
        public GraphNode GetVariableOwnerNode(short guid)
        {
            foreach (var db in m_vNodes)
            {
                var argvPorts = db.Value.GetArvgs();
                if (argvPorts!=null)
                {
                    foreach(var sub in argvPorts)
                    {
                        if (sub.GetVariableGuid() == guid)
                            return db.Value;
                    }
                }
                var returnPort = db.Value.GetReturns();
                if (returnPort != null)
                {
                    foreach (var sub in returnPort)
                    {
                        if (sub.GetVariableGuid() == guid)
                            return db.Value;
                    }
                }
            }
            return null;
        }
        //------------------------------------------------------
        public void AddPort(ArvgPort port)
        {
            m_vPorts[port.GetKey()] = port;
        }
        //-----------------------------------------------------
        public void DelPort(ArvgPort port)
        {
            m_vPorts.Remove(port.GetKey());
            if (port.GetVariable() != null)
                RemoveVariable(port.GetVariable().GetGuid());
        }
        //-----------------------------------------------------
        public ArvgPort GetPort(long key)
        {
            if (m_vPorts.TryGetValue(key, out var port))
            {
                return port;
            }
            return null;
        }
        //-----------------------------------------------------
        public void RemoveVariable(short guid)
        {
            if (m_vVariables.ContainsKey(guid))
            {
                m_vVariables.Remove(guid);
            }
        }
        //------------------------------------------------------
        public short GeneratorVarGUID()
        {
            short guid = 1;
            int maxStack = 1000000;
            while (m_vVariables.ContainsKey(guid))
            {
                guid++;
                maxStack--;
                if (maxStack <= 0) break;
            }
            return guid;
        }
        //-----------------------------------------------------
        public void OnNotifyExecutedNode(AgentTree pAgentTree, BaseNode pNode)
        {
            if (m_vNodes.TryGetValue(pNode, out var graphNode) && graphNode != null)
            {
                graphNode.OnNotifyExcuted();
                graphNode.FlashRed(1f);
            }
        }
        //-----------------------------------------------------
        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
        }
        //-----------------------------------------------------
        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
        }
    }
}

#endif