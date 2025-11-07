/********************************************************************
生成日期:	06:30:2025
类    名: 	VariablesEditor
作    者:	HappLI
描    述:	变量编辑绘制
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using Framework.Cutscene.Editor;
using Framework.Cutscene.Runtime;
using Framework.DrawProps;
using Framework.ED;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework.AT.Editor
{
    [Inspector(typeof(Variables))]
    public class VariablesEditor
    {
        public static bool ms_bCanCustomChangeRule = false;
        public static bool CanCusomChangeRule
        {
            get { return ms_bCanCustomChangeRule; }
            set { ms_bCanCustomChangeRule = value;}
        }
        static EVariableType ms_nAddType = EVariableType.eInt;
        public static Variables DrawInspector(System.Object data, System.Object parentData, System.Reflection.FieldInfo parentField)
        {
            Variables variables;
            if (data != null) variables = (Variables)data;
            else variables = new Variables();

            if (variables.variables == null)
                variables.variables = new VariableList();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("变量类型", GUILayout.Width(100));
            GUILayout.Label("变量名", GUILayout.Width(100));
            GUILayout.Label("初始值", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            int removeIndex = -1;
            int moveUpIndex = -1;
            int moveDownIndex = -1;

            bool isClipCustomData = false;
            uint customAgentType = 0;
            if(parentData!=null)
            {
                if(parentData is CutsceneCustomEvent)
                {
                    customAgentType = ((CutsceneCustomEvent)parentData).customType;
                }
                if (parentData is CutsceneCustomClip)
                {
                    isClipCustomData = true;
                    customAgentType = ((CutsceneCustomClip)parentData).customType;
                }
            }

            for (int i = 0; i < variables.variables.GetVarCount(); ++i)
            {
                EVariableType type = variables.variables.GetVarType(i);

                EditorGUILayout.BeginHorizontal();

                // 变量类型
                EditorGUI.BeginDisabledGroup(!CanCusomChangeRule);
                var newType = (EVariableType)EditorEnumPop.PopEnum("",type,null, new GUILayoutOption[] { GUILayout.Width(80) });
                EditorGUI.EndDisabledGroup();
                string defValue = null;
                string varName = "";
                if (customAgentType!=0)
                {
                    CutsceneCustomAgent.AgentUnit agentUnit = null;
                    ParamPortMapFieldAttribute portMapFieldAttr = null;
                    if (isClipCustomData)
                    {
                        agentUnit = CustomAgentUtil.GetClip(customAgentType);
                        if (parentField != null && parentField.IsDefined(typeof(ParamPortMapFieldAttribute)))
                        {
                            portMapFieldAttr = parentField.GetCustomAttribute<ParamPortMapFieldAttribute>();
                        }
                    }
                    else
                    {
                        agentUnit = CustomAgentUtil.GetEvent(customAgentType);
                        if (parentField != null && parentField.IsDefined(typeof(ParamPortMapFieldAttribute)))
                        {
                            portMapFieldAttr = parentField.GetCustomAttribute<ParamPortMapFieldAttribute>();
                        }
                    }
                    if (agentUnit !=null && portMapFieldAttr !=null)
                    {
                        var portAttrDef = agentUnit.GetType().GetField(portMapFieldAttr.fieldName);
                        if(portAttrDef!=null)
                        {
                            var paramDataObj = portAttrDef.GetValue(agentUnit);
                            if(paramDataObj!=null && paramDataObj is CutsceneCustomAgent.AgentUnit.ParamData[])
                            {
                                CutsceneCustomAgent.AgentUnit.ParamData[] paramDatas = paramDataObj as CutsceneCustomAgent.AgentUnit.ParamData[];
                                if(paramDatas!=null && i < paramDatas.Length)
                                {
                                    varName = paramDatas[i].name;
                                    defValue = paramDatas[i].defaultValue;
                                }
                            }
                        }
                    }
                }
                if (newType != type)
                {
                    ED.InspectorDrawUtil.RegisterUndo(variables); // 类型变化前
                    variables.variables.ChangeType(i, newType, defValue);
                    type = newType;
                }

                // 变量名和初始值
                if (type == EVariableType.eInt)
                {
                    int oldValue = variables.variables.GetInt(i);
                    EditorGUILayout.LabelField(varName, GUILayout.Width(120));

                    GUILayout.BeginHorizontal(GUILayout.Width(120));
                    int newValue = EditorGUILayout.IntField(oldValue);
                    if (defValue != null && GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool"), GUILayout.Width(24), GUILayout.Height(18)))
                    {
                        if (int.TryParse(defValue, out var tempV)) newValue = tempV;
                        else newValue = 0;
                        GUI.FocusControl(null);
                    }
                    GUILayout.EndHorizontal();
                    // 检查数值是否变化
                    if (newValue != oldValue)
                    {
                        ED.InspectorDrawUtil.RegisterUndo(variables);
                    }
                    variables.variables.SetInt(i, newValue);
                }
                else if (type == EVariableType.eFloat)
                {
                    float oldValue = variables.variables.GetFloat(i);
                    EditorGUILayout.LabelField(varName, GUILayout.Width(120));

                    GUILayout.BeginHorizontal(GUILayout.Width(120));
                    float newValue = EditorGUILayout.FloatField(oldValue);
                    if (defValue != null && GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool"), GUILayout.Width(24), GUILayout.Height(24)))
                    {
                        if (float.TryParse(defValue, out var tempV)) newValue = tempV;
                        else newValue = 0;
                    }
                    GUILayout.EndHorizontal();
                    if (!Mathf.Approximately(newValue, oldValue))
                    {
                        ED.InspectorDrawUtil.RegisterUndo(variables);
                    }
                    variables.variables.SetFloat(i, newValue);
                }
                else if (type == EVariableType.eString)
                {
                    string oldValue = variables.variables.GetString(i);
                    EditorGUILayout.LabelField(varName, GUILayout.Width(120));
                    GUILayout.BeginHorizontal(GUILayout.Width(120));
                    string newValue = EditorGUILayout.TextField(oldValue);
                    if (defValue != null && GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool"), GUILayout.Width(24), GUILayout.Height(24)))
                    {
                        newValue = defValue;
                        GUI.FocusControl(null);
                    }
                    GUILayout.EndHorizontal();
                    if (newValue != oldValue)
                    {
                        ED.InspectorDrawUtil.RegisterUndo(variables);
                    }
                    variables.variables.SetString(i, newValue);
                }
                else if (type == EVariableType.eBool)
                {
                    bool oldValue = variables.variables.GetBool(i);
                    EditorGUILayout.LabelField(varName, GUILayout.Width(120));
                    GUILayout.BeginHorizontal(GUILayout.Width(120));
                    bool newValue = EditorGUILayout.Toggle(oldValue);
                    if (defValue != null && GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool"), GUILayout.Width(24), GUILayout.Height(24)))
                    {
                        if (defValue.Equals("true", System.StringComparison.OrdinalIgnoreCase) || defValue != "0") newValue = true;
                        else newValue = false;
                        GUI.FocusControl(null);
                    }
                    GUILayout.EndHorizontal();
                    if (newValue != oldValue)
                    {
                        ED.InspectorDrawUtil.RegisterUndo(variables);
                    }
                    variables.variables.SetBool(i, newValue);
                }
                else if (type == EVariableType.eVec2)
                {
                    Vector2 oldValue = variables.variables.GetVec2(i);
                    EditorGUILayout.LabelField(varName, GUILayout.Width(120));
                    GUILayout.BeginHorizontal(GUILayout.Width(120));
                    var newValue = EditorGUILayout.Vector2Field("", oldValue);
                    if (defValue != null && GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool"), GUILayout.Width(24), GUILayout.Height(24)))
                    {
                        var splitVal = defValue.Split(new char[] { '|', ',' });
                        if (splitVal.Length >= 2)
                        {
                            if (float.TryParse(splitVal[0], out var x) && float.TryParse(splitVal[1], out var y))
                            {
                                newValue = new Vector2(x, y);
                            }
                            else
                            {
                                newValue = Vector2.zero;
                            }
                        }
                        else
                        {
                            newValue = Vector2.zero;
                        }
                        GUI.FocusControl(null);
                    }
                    GUILayout.EndHorizontal();
                    if (newValue != oldValue)
                    {
                        ED.InspectorDrawUtil.RegisterUndo(variables);
                    }
                    variables.variables.SetVec2(i, newValue);
                }
                else if (type == EVariableType.eVec3)
                {
                    Vector3 oldValue = variables.variables.GetVec3(i);
                    EditorGUILayout.LabelField(varName, GUILayout.Width(120));
                    GUILayout.BeginHorizontal(GUILayout.Width(120));
                    var newValue = EditorGUILayout.Vector3Field("", oldValue);
                    if (defValue != null && GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool"), GUILayout.Width(24), GUILayout.Height(24)))
                    {
                        var splitVal = defValue.Split(new char[] { '|', ',' });
                        if (splitVal.Length >= 3)
                        {
                            if (float.TryParse(splitVal[0], out var x) && float.TryParse(splitVal[1], out var y) && float.TryParse(splitVal[2], out var z))
                            {
                                newValue = new Vector3(x, y, z);
                            }
                            else
                            {
                                newValue = Vector3.zero;
                            }
                        }
                        else
                        {
                            newValue = Vector3.zero;
                        }
                        GUI.FocusControl(null);
                    }
                    GUILayout.EndHorizontal();
                    if (newValue != oldValue)
                    {
                        ED.InspectorDrawUtil.RegisterUndo(variables);
                    }
                    variables.variables.SetVec3(i, newValue);
                }
                else if (type == EVariableType.eVec4)
                {
                    Vector4 oldValue = variables.variables.GetVec4(i);
                    EditorGUILayout.LabelField(varName, GUILayout.Width(120));
                    GUILayout.BeginHorizontal(GUILayout.Width(120));
                    var newValue = EditorGUILayout.Vector4Field("", oldValue);
                    if (defValue != null && GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool"), GUILayout.Width(24), GUILayout.Height(24)))
                    {
                        var splitVal = defValue.Split(new char[] { '|', ',' });
                        if (splitVal.Length >= 4)
                        {
                            if (float.TryParse(splitVal[0], out var x) && float.TryParse(splitVal[1], out var y) && float.TryParse(splitVal[2], out var z) && float.TryParse(splitVal[3], out var w))
                            {
                                newValue = new Vector4(x, y, z, w);
                            }
                            else
                            {
                                newValue = Vector4.zero;
                            }
                        }
                        else
                        {
                            newValue = Vector4.zero;
                        }
                        GUI.FocusControl(null);
                    }
                    GUILayout.EndHorizontal();
                    if (newValue != oldValue)
                    {
                        ED.InspectorDrawUtil.RegisterUndo(variables);
                    }
                    variables.variables.SetVec4(i, newValue);
                }

                if (CanCusomChangeRule)
                {
                    // 上移按钮
                    GUI.enabled = i > 0;
                    if (GUILayout.Button("↑", GUILayout.Width(24)))
                        moveUpIndex = i;
                    GUI.enabled = i < variables.variables.GetVarCount() - 1;
                    if (GUILayout.Button("↓", GUILayout.Width(24)))
                        moveDownIndex = i;
                    GUI.enabled = true;

                    // 删除按钮
                    if (GUILayout.Button("X", GUILayout.Width(24)))
                        removeIndex = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (CanCusomChangeRule)
            {
                // 处理上移
                if (moveUpIndex > 0)
                {
                    ED.InspectorDrawUtil.RegisterUndo(variables);
                    variables.variables.SwapIndex(moveUpIndex, moveUpIndex-1);
                }
                // 处理下移
                if (moveDownIndex >= 0 && moveDownIndex < variables.variables.GetVarCount() - 1)
                {
                    ED.InspectorDrawUtil.RegisterUndo(variables);
                    variables.variables.SwapIndex(moveUpIndex, moveUpIndex + 1);
                }
                // 删除变量
                if (removeIndex >= 0)
                {
                    ED.InspectorDrawUtil.RegisterUndo(variables);
                    variables.variables.RemoveIndex(moveUpIndex);
                }

                // 添加新变量
                GUILayout.BeginHorizontal();
                ms_nAddType = (EVariableType)EditorEnumPop.PopEnum("", ms_nAddType, null, new GUILayoutOption[] { GUILayout.Width(80) });
                if (GUILayout.Button("添加变量", GUILayout.Width(80)))
                {
                    ED.InspectorDrawUtil.RegisterUndo(variables);
                    variables.variables.AddVariable(VariableUtil.CreateVariable(ms_nAddType));
                }
                GUILayout.EndHorizontal();
            }

            return variables;
        }
    }
}
#endif