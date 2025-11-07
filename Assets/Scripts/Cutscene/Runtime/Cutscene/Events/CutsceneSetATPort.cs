/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneSetATPort
作    者:	HappLI
描    述:	设置行为树端口数据事件
*********************************************************************/
using Framework.AT.Runtime;
using Framework.DrawProps;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneEvent("设置行为树变量")]
    public class CutsceneSetATPort : IBaseEvent
    {
        [System.Serializable]
        public struct Port
        {
            public int index;
            public string value;
        }
        [Display("基本属性")]public BaseEventProp baseProp;

        [Display("节点Id"), RowFieldInspector] public short nodeGuid;
        [Display("端口"),FieldInspector] public Port[] paramPorts;
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new CutsceneSetATPortDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EEventType.eSetATPortData;
        }
        //-----------------------------------------------------
        public string GetName()
        {
            return baseProp.name;
        }
        //-----------------------------------------------------
        public float GetTime()
        {
            return baseProp.time;
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        void OnDrawFieldLineRow(System.Object pOwner, System.Reflection.FieldInfo fieldInfo)
        {
            if (fieldInfo.Name == "nodeGuid")
            {
                if (GUILayout.Button("选择行为节点"))
                {
                    var provider = ScriptableObject.CreateInstance<Editor.AgentTreeInputPortSearchProvider>();
                    if (baseProp.ownerObject != null && baseProp.ownerObject is CutsceneObject)
                    {
                        CutsceneObject cutsceneObject = baseProp.ownerObject as CutsceneObject;
                        provider.SetAgentTreeData(cutsceneObject.GetCutsceneGraph().agentTree);
                    }
                    provider.onSelect = (node, attri, index) =>
                    {
                        this.nodeGuid = node.guid;
                    };
                    // 弹出搜索窗口，位置可根据需要调整
                    UnityEditor.Experimental.GraphView.SearchWindow.Open(new UnityEditor.Experimental.GraphView.SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
                }
            }
        }
        //-----------------------------------------------------
        [System.NonSerialized] int ms_vPortArgvsIndex = 0;
        void OnFieldInspector(System.Object pOwner, System.Reflection.FieldInfo fieldInfo)
        {
            if (fieldInfo.Name == "paramPorts")
            {
                if (baseProp.ownerObject != null && baseProp.ownerObject is CutsceneObject)
                {
                    CutsceneObject cutsceneObject = baseProp.ownerObject as CutsceneObject;
                    var node = cutsceneObject.GetCutsceneGraph().agentTree.GetNode(nodeGuid);
                    if (node != null)
                    {
                        int customType = 0;
                        if (node is CutsceneEvent)
                        {
                            customType = ((CutsceneEvent)node).eventType;
                        }
                        var attri = AT.Editor.AgentTreeUtil.GetAttri(node.type, customType);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("参数名", GUILayout.Width(100));
                        EditorGUILayout.LabelField("值", GUILayout.Width(100));
                        EditorGUILayout.LabelField("", GUILayout.Width(15));
                        EditorGUILayout.EndHorizontal();
                        bool bExistPort = false;
                        if(paramPorts!=null)
                        {
                            for (int j = 0; j < paramPorts.Length; ++j)
                            {
                                var port = paramPorts[j];
                                if (port.index == ms_vPortArgvsIndex)
                                {
                                    bExistPort = true;
                                }
                                EditorGUILayout.BeginHorizontal();
                                if (attri != null && port.index >= 0 && port.index < attri.argvs.Count)
                                {
                                    var argvAttr = attri.argvs[port.index];
                                    UnityEditor.EditorGUILayout.LabelField(argvAttr.name, GUILayout.Width(100));
                                    if (argvAttr.argvType == typeof(byte))
                                    {
                                        byte.TryParse(port.value, out var val);
                                        port.value = Mathf.Clamp(UnityEditor.EditorGUILayout.IntField("", val, GUILayout.Width(100)), byte.MinValue, byte.MaxValue).ToString();
                                    }
                                    else if (argvAttr.argvType == typeof(short))
                                    {
                                        short.TryParse(port.value, out var val);
                                        port.value = Mathf.Clamp(UnityEditor.EditorGUILayout.IntField("", val, GUILayout.Width(100)), short.MinValue, short.MaxValue).ToString();
                                    }
                                    else if (argvAttr.argvType == typeof(ushort))
                                    {
                                        ushort.TryParse(port.value, out var val);
                                        port.value = Mathf.Clamp(UnityEditor.EditorGUILayout.IntField("", val, GUILayout.Width(100)), ushort.MinValue, ushort.MaxValue).ToString();
                                    }
                                    else if (argvAttr.argvType == typeof(int))
                                    {
                                        int.TryParse(port.value, out var val);
                                        port.value = UnityEditor.EditorGUILayout.IntField("", val, GUILayout.Width(100)).ToString();
                                    }
                                    else if (argvAttr.argvType == typeof(float))
                                    {
                                        float.TryParse(port.value, out var val);
                                        port.value = UnityEditor.EditorGUILayout.FloatField("", val, GUILayout.Width(100)).ToString();
                                    }
                                    else if (argvAttr.argvType == typeof(string))
                                    {
                                        port.value = UnityEditor.EditorGUILayout.TextField("", port.value, GUILayout.Width(100));
                                    }
                                    else if (argvAttr.argvType == typeof(ObjId))
                                    {
                                        int.TryParse(port.value, out var binderId);
                                        var binders = ObjectBinderUtils.GetBinder(binderId);
                                        var obj = binders.GetBinder();
                                        obj = (CutsceneObjectBinder)UnityEditor.EditorGUILayout.ObjectField("", obj, typeof(CutsceneObjectBinder), true, GUILayout.Width(100));
                                        if (obj != null)
                                        {
                                            binderId = obj.GetBindID();
                                            ObjectBinderUtils.BindObject(obj);
                                        }
                                        else
                                        {
                                            binderId = 0;
                                        }
                                        port.value = binderId.ToString();
                                    }
                                    paramPorts[j] = port;
                                }
                                else
                                {
                                    UnityEditor.EditorGUILayout.LabelField("!ERROR!", GUILayout.Width(100));
                                }
                                if (GUILayout.Button("-", GUILayout.Width(15)))
                                {
                                    //!删除当前端口
                                    List<Port> ports = new List<Port>(paramPorts);
                                    ports.RemoveAt(j);
                                    paramPorts = ports.ToArray();
                                    EditorGUILayout.EndHorizontal();
                                    break;
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        
                        EditorGUILayout.BeginHorizontal();
                        ms_vPortArgvsIndex = EditorGUILayout.Popup(ms_vPortArgvsIndex, attri.popArgvs.ToArray());
                        EditorGUI.BeginDisabledGroup(bExistPort || ms_vPortArgvsIndex < 0 && ms_vPortArgvsIndex >= attri.argvs.Count);
                        if (GUILayout.Button("新增节点参数"))
                        {
                            List<Port> ports = null;
                            if (paramPorts == null) ports = new List<Port>();
                            else ports = new List<Port>(paramPorts);
                            ports.Add(new Port() { index = ms_vPortArgvsIndex, value = "" });
                            paramPorts = ports.ToArray();
                            ms_vPortArgvsIndex = 0;
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }
#endif
    }
    //-----------------------------------------------------
    internal class CutsceneSetATPortDriver : ACutsceneDriver
    {
        public override bool OnEventTrigger(CutsceneTrack pTrack, IBaseEvent pEvt)
        {
            var portData = pEvt.Cast<CutsceneSetATPort>();
            if (portData.paramPorts == null || portData.paramPorts.Length<=0)
                return true;
            var pAgentTree = pTrack.GetCutscene().GetAgentTree();
            if (pAgentTree == null)
                return true;
            var node = pAgentTree.GetNode(portData.nodeGuid);
            if (node == null)
                return true;
            var ports = node.GetInports();
            if(ports == null )
                return true;
            for(int i =0; i < portData.paramPorts.Length && i < ports.Length; ++i)
            {
                var port = portData.paramPorts[i];
                if (ports[port.index].pVariable == null)
                    return true;
                switch (ports[port.index].pVariable.GetVariableType())
                {
                    case EVariableType.eInt:
                        if (int.TryParse(port.value, out int intValue))
                        {
                            pAgentTree.SetInportInt(node, port.index, intValue);
                        }
                        break;
                    case EVariableType.eFloat:
                        if (float.TryParse(port.value, out float floatValue))
                        {
                            pAgentTree.SetInportFloat(node, port.index, floatValue);
                        }
                        break;
                    case EVariableType.eString:
                        pAgentTree.SetInportString(node, port.index, port.value);
                        break;
                    case EVariableType.eBool:
                        if (bool.TryParse(port.value, out bool boolValue))
                        {
                            pAgentTree.SetInportBool(node, port.index, boolValue);
                        }
                        break;
                    case EVariableType.eVec2:
                        string[] split = port.value.Split('|');
                        if (split.Length == 2)
                        {
                            if (float.TryParse(split[0], out float x) && float.TryParse(split[1], out float y))
                            {
                                pAgentTree.SetInportVec2(node, port.index, new Vector2(x, y));
                            }
                        }
                        break;
                    case EVariableType.eVec3:
                        {
                            split = port.value.Split('|');
                            if (split.Length == 3)
                            {
                                if (float.TryParse(split[0], out float x) && float.TryParse(split[1], out float y) && float.TryParse(split[2], out float z))
                                {
                                    pAgentTree.SetInportVec3(node, port.index, new Vector3(x, y, z));
                                }
                            }
                        }
                        break;
                    case EVariableType.eVec4:
                        {
                            split = port.value.Split('|');
                            if (split.Length == 4)
                            {
                                if (float.TryParse(split[0], out float x) && float.TryParse(split[1], out float y) && float.TryParse(split[2], out float z) && float.TryParse(split[3], out float w))
                                {
                                    pAgentTree.SetInportVec4(node, port.index, new Vector4(x, y, z, w));
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            
            return true;
        }
    }
}