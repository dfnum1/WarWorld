/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneCustomAgentEditor
作    者:	HappLI
描    述:	自定义事件、剪辑参数
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using Framework.Cutscene.Runtime;
using Framework.ED;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Framework.ED.TreeAssetView;
using static UnityEditor.UIElements.ToolbarMenu;
using static UnityEngine.UI.CanvasScaler;
using Color = UnityEngine.Color;

namespace Framework.Cutscene.Editor
{
    public class CutsceneCustomAgentEditor : EditorWindow
    {
        public class AgentItem : TreeAssetView.ItemData
        {
            public ETab Tab;
            public CutsceneCustomAgent.AgentUnit unit;
            public AgentItem(CutsceneCustomAgent.AgentUnit unit, ETab tab)
            {
                this.Tab = tab;
                this.unit = unit;
                name = unit.name + "[" + unit.customType + "]";
            }
            public override Color itemColor()
            {
                if (unit.customType <= 0 || string.IsNullOrEmpty(unit.name))
                    return Color.red;
                return Color.white;
            }
        }
        public enum ETab
        {
            CustomEvent,
            CustomClip,
            None,
        }
        string m_strNewName = "";
        ETab m_eTab = ETab.CustomEvent;
        TreeAssetView m_pEventTreeview;
        TreeAssetView m_pClipTreeview;
        AgentItem m_pSelect = null;

        bool m_bExpandInput = false;
        bool m_bExpandOutput = false;
        Vector2 m_scoll = Vector2.zero;
        HashSet<uint> m_vEventTypes = new HashSet<uint>();
        HashSet<string> m_vEventNames = new HashSet<string>();
        List<CutsceneCustomAgent.AgentUnit> m_vEvents = new List<CutsceneCustomAgent.AgentUnit>();
        HashSet<uint> m_vClipTypes = new HashSet<uint>();
        HashSet<string> m_vClipNames = new HashSet<string>();
        List<CutsceneCustomAgent.AgentUnit> m_vClips = new List<CutsceneCustomAgent.AgentUnit>();
        //--------------------------------------------------------
        static CutsceneCustomAgentEditor ms_pInstnace = null;
        public static void Open()
        {
            if (ms_pInstnace != null)
            {
                ms_pInstnace.Focus();
                return;
            }
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
                return;
            }
            CutsceneCustomAgentEditor window = EditorWindow.GetWindow<CutsceneCustomAgentEditor>();
            window.titleContent = new GUIContent("自定义行为参数编辑器");
        }
        //--------------------------------------------------------
        public static void CloseEditor()
        {
            if (ms_pInstnace != null)
            {
                ms_pInstnace.Close();
                ms_pInstnace = null;
            }
        }
        //--------------------------------------------------------
        void OnEnable()
        {
            ms_pInstnace = this;
            this.minSize = new Vector2(600, 400);
            CustomAgentUtil.Init(true);
            m_pEventTreeview = new TreeAssetView(new string[] { "类型", "名称" });
            m_pClipTreeview = new TreeAssetView(new string[] { "类型", "名称" });
            m_pEventTreeview.OnSelectChange += OnSelectChange;
            m_pClipTreeview.OnSelectChange += OnSelectChange;
            m_pEventTreeview.OnCellDraw += OnDrawItem;
            m_pClipTreeview.OnCellDraw += OnDrawItem;
            m_vEvents = new List<CutsceneCustomAgent.AgentUnit>(CustomAgentUtil.GetEventList());
            m_vClips = new List<CutsceneCustomAgent.AgentUnit>(CustomAgentUtil.GetClipList());
            Refresh();
        }
        //--------------------------------------------------------
        void OnDisable()
        {
            ms_pInstnace = null;
        }
        //--------------------------------------------------------
        void Refresh(ETab tab = ETab.None)
        {
            if(tab == ETab.None || tab == ETab.CustomEvent)
            {
                m_vEventTypes.Clear();
                m_vEventNames.Clear();
                m_pEventTreeview.BeginTreeData();
                for (int i = 0; i < m_vEvents.Count; ++i)
                {
                    var item = m_vEvents[i];
                    m_vEventTypes.Add(item.customType);
                    m_vEventNames.Add(item.name);
                    m_pEventTreeview.AddData(new AgentItem(item, ETab.CustomEvent) { id = i });
                }
                m_pEventTreeview.EndTreeData();
            }
            if (tab == ETab.None || tab == ETab.CustomClip)
            {
                m_vClipTypes.Clear();
                m_vClipNames.Clear();
                m_pClipTreeview.BeginTreeData();
                for (int i = 0; i < m_vClips.Count; ++i)
                {
                    var item = m_vClips[i];
                    m_vClipTypes.Add(item.customType);
                    m_vClipNames.Add(item.name);
                    m_pClipTreeview.AddData(new AgentItem(item, ETab.CustomClip) { id = i });
                }
                m_pClipTreeview.EndTreeData();
            }
        }
        //--------------------------------------------------------
        void OnSelectChange(TreeAssetView.ItemData item)
        {
            if (item == null)
                return;
            m_pSelect = item as AgentItem;
        }
        //--------------------------------------------------------
        bool OnDrawItem(RowArgvData agv)
        {
            var agentItem = agv.itemData.data as AgentItem;
            agentItem.name = agentItem.unit.name + "[" + agentItem.unit.customType + "]";
            if (agv.column == 0)
            {
                EditorGUI.LabelField(agv.rowRect, agentItem.unit.customType.ToString());
            }
            else if (agv.column == 1)
            {
                EditorGUI.LabelField(agv.rowRect, agentItem.unit.name);
            }
            return true;
        }
        //--------------------------------------------------------
        void OnGUI()
        {
            float leftWidth = position.width / 2 - 1;
            GUILayout.BeginArea(new Rect(0, 0, leftWidth, 25));
            m_eTab = (ETab)GUILayout.Toolbar((int)m_eTab, new string[] { "事件", "剪辑" });
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(0, 30, leftWidth, position.height - 30));
            if (m_eTab == ETab.CustomEvent)
            {
                if (m_pEventTreeview != null)
                {
                    m_pEventTreeview.GetColumn(1).width = leftWidth* 2.0f / 3.0f;
                    m_pEventTreeview.GetColumn(0).width = leftWidth  / 3.0f;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("搜索", GUILayout.Width(30));
                    m_pEventTreeview.searchString = EditorGUILayout.TextField(m_pEventTreeview.searchString);
                    EditorGUILayout.EndHorizontal();
                    m_pEventTreeview.OnGUI(new Rect(0, 20, leftWidth, position.height - 80));
                }
            }
            else if (m_eTab == ETab.CustomClip)
            {
                if (m_pClipTreeview != null)
                {
                    m_pClipTreeview.GetColumn(1).width = leftWidth * 2.0f / 3.0f;
                    m_pClipTreeview.GetColumn(0).width = leftWidth / 3.0f;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("搜索", GUILayout.Width(30));
                    m_pClipTreeview.searchString = EditorGUILayout.TextField(m_pClipTreeview.searchString);
                    EditorGUILayout.EndHorizontal(); 
                    m_pClipTreeview.OnGUI(new Rect(0, 20, leftWidth, position.height - 80));
                }
            }
            GUILayout.EndArea();

            UIDrawUtils.DrawColorLine(new Vector3(leftWidth, position.height - 81), new Vector3(position.width, position.height - 81), Color.white);
            GUILayout.BeginArea(new Rect(0, position.height-30, leftWidth, 30));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("保存", new GUILayoutOption[] { GUILayout.Width(leftWidth/2) }))
            {
                CustomAgentUtil.RefreshData(m_vEvents, m_vClips);
            }
            if (GUILayout.Button("提交", new GUILayoutOption[] { GUILayout.Width(leftWidth/2) }))
            {
                CustomAgentUtil.CommitGit();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            UIDrawUtils.DrawColorLine(new Vector3(leftWidth,0), new Vector3(leftWidth, position.height), Color.white);
            GUILayout.BeginArea(new Rect(leftWidth+1, 5, leftWidth, position.height-5));
            if (m_eTab == ETab.CustomEvent)
            {
                DrawCustomEvents();
            }
            else if (m_eTab == ETab.CustomClip)
            {
                DrawCustomClips();
            }
            DrawSelect();
            GUILayout.EndArea();

            UIDrawUtils.DrawColorLine(new Vector3(leftWidth, 26), new Vector3(position.width, 26), Color.white);
        }
        //--------------------------------------------------------
        void DrawSelect()
        {
            if (m_pSelect == null)
                return;
            float posX = position.width / 2;
            if (m_pSelect.Tab == ETab.CustomEvent)
            {
                DrawAgentUnit(m_pSelect.unit, m_vEvents, m_vEventTypes, m_vEventNames);
                m_scoll = EditorGUILayout.BeginScrollView(m_scoll, new GUILayoutOption[] { GUILayout.MaxHeight(position.height - 100) });
                if (m_pSelect!=null)
                {
                    {
                        GUILayout.BeginHorizontal();
                        m_bExpandInput = EditorGUILayout.Foldout(m_bExpandInput, "输入参数");
                        if (GUILayout.Button("添加", GUILayout.Width(50)))
                        {
                            var list = m_pSelect.unit.inputVariables==null? new List<CutsceneCustomAgent.AgentUnit.ParamData>():new System.Collections.Generic.List<CutsceneCustomAgent.AgentUnit.ParamData>(m_pSelect.unit.inputVariables);
                            list.Add(new CutsceneCustomAgent.AgentUnit.ParamData() { name = "NewParam", type = EVariableType.eInt, defaultValue = "0", canEdit = true });
                            m_pSelect.unit.inputVariables = list.ToArray();
                            m_bExpandInput = true;
                        }
                        GUILayout.EndHorizontal();
                        if(m_bExpandInput)
                            m_pSelect.unit.inputVariables = DrawAgentUnitParams(m_pSelect.unit, m_pSelect.unit.inputVariables);
                    }
                    GUILayout.Space(5);
                    {
                        GUILayout.BeginHorizontal();
                        m_bExpandOutput = EditorGUILayout.Foldout(m_bExpandOutput, "输出参数");
                        if (GUILayout.Button("添加", GUILayout.Width(50)))
                        {
                            var list = m_pSelect.unit.outputVariables == null ? new List<CutsceneCustomAgent.AgentUnit.ParamData>() : new System.Collections.Generic.List<CutsceneCustomAgent.AgentUnit.ParamData>(m_pSelect.unit.outputVariables);
                            list.Add(new CutsceneCustomAgent.AgentUnit.ParamData() { name = "NewParam", type = EVariableType.eInt, defaultValue="0", canEdit = false });
                            m_pSelect.unit.outputVariables = list.ToArray();
                            m_bExpandOutput = true;
                        }
                        GUILayout.EndHorizontal();
                        if(m_bExpandOutput) m_pSelect.unit.outputVariables = DrawAgentUnitParams(m_pSelect.unit, m_pSelect.unit.outputVariables);
                    }
                }
                EditorGUILayout.EndScrollView();
                GUILayout.BeginArea(new Rect(2, position.height-30, position.width / 2 - 10, 30));
                if (GUILayout.Button("删除Event", new GUILayoutOption[] { GUILayout.Width(position.width/2-10) }))
                {
                    if (EditorUtility.DisplayDialog("提示", "确定删除改自定义事件?", "删除", "再想想"))
                    {
                        m_vEvents.Remove(m_pSelect.unit);
                        m_pSelect = null;
                        Refresh(ETab.CustomEvent);
                    }
                }
                GUILayout.EndArea();
            }
            else if (m_pSelect.Tab == ETab.CustomClip)
            {
                DrawAgentUnit(m_pSelect.unit, m_vClips, m_vClipTypes, m_vClipNames);
                m_scoll = EditorGUILayout.BeginScrollView(m_scoll, new GUILayoutOption[] { GUILayout.MaxHeight(position.height - 100) });
                if (m_pSelect != null)
                {
                    GUILayout.BeginHorizontal();
                    m_bExpandInput = EditorGUILayout.Foldout(m_bExpandInput, "输入参数");
                    if (GUILayout.Button("添加", GUILayout.Width(50)))
                    {
                        var list = m_pSelect.unit.inputVariables == null ? new List<CutsceneCustomAgent.AgentUnit.ParamData>() : new System.Collections.Generic.List<CutsceneCustomAgent.AgentUnit.ParamData>(m_pSelect.unit.inputVariables);
                        list.Add(new CutsceneCustomAgent.AgentUnit.ParamData() { name = "NewParam", type = EVariableType.eInt });
                        m_pSelect.unit.inputVariables = list.ToArray();
                        m_bExpandInput = true;
                    }
                    GUILayout.EndHorizontal();
                    if(m_bExpandInput) m_pSelect.unit.inputVariables = DrawAgentUnitParams(m_pSelect.unit, m_pSelect.unit.inputVariables);
                }
                EditorGUILayout.EndScrollView();

                GUILayout.BeginArea(new Rect(2, position.height - 30, position.width / 2 - 10, 30));
                if (GUILayout.Button("删除Clip", new GUILayoutOption[] { GUILayout.Width(position.width / 2 - 10) }))
                {
                    if (EditorUtility.DisplayDialog("提示", "确定删除改自定义剪辑?", "删除", "再想想"))
                    {
                        m_vClips.Remove(m_pSelect.unit);
                        m_pSelect = null;
                        Refresh(ETab.CustomClip);
                    }
                }
                GUILayout.EndArea();
            }
        }
        //--------------------------------------------------------
        void DrawCustomClips()
        {
            GUILayout.BeginHorizontal();
            m_strNewName = EditorGUILayout.TextField(m_strNewName);
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_strNewName) || m_vClipNames.Contains(m_strNewName));
            if (GUILayout.Button("新增Clip", new GUILayoutOption[] { GUILayout.Width(100) }))
            {
                CutsceneCustomAgent.AgentUnit unit = new CutsceneCustomAgent.AgentUnit();
                unit.name = m_strNewName;
                unit.customType = 0; // 默认类型为0
                m_vClips.Add(unit);
                Refresh(ETab.CustomClip);
                m_strNewName = "";
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }
        //--------------------------------------------------------
        void DrawCustomEvents()
        {
            GUILayout.BeginHorizontal();
            m_strNewName = EditorGUILayout.TextField(m_strNewName);
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_strNewName) || m_vEventNames.Contains(m_strNewName));
            if (GUILayout.Button("新增Event", new GUILayoutOption[] { GUILayout.Width(100) }))
            {
                CutsceneCustomAgent.AgentUnit unit = new CutsceneCustomAgent.AgentUnit();
                unit.name = m_strNewName;
                unit.customType = 0; // 默认类型为0
                m_vEvents.Add(unit);
                Refresh(ETab.CustomEvent);
                m_strNewName = "";
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }
        //--------------------------------------------------------
        void DrawAgentUnit(CutsceneCustomAgent.AgentUnit unit, List<CutsceneCustomAgent.AgentUnit> vList, HashSet<uint> vTypes, HashSet<string> vNames)
        {
            string curName = unit.name;
            curName = EditorGUILayout.DelayedTextField("事件名称",unit.name);
            if(curName!= unit.name && !string.IsNullOrEmpty(curName))
            {
                if(!vNames.Contains(curName))
                {
                    vNames.Remove(unit.name);
                    vNames.Add(curName);
                    unit.name = curName;
                }
            }
            uint curType = unit.customType;
            curType = (uint)EditorGUILayout.IntField("事件类型",(int)unit.customType);
            if(curType != unit.customType && curType>0)
            {
                if(!vTypes.Contains(curType))
                {
                    vTypes.Remove(unit.customType);
                    vTypes.Add(curType);
                    unit.customType = curType;
                }
            }
        }
        //--------------------------------------------------------
        CutsceneCustomAgent.AgentUnit.ParamData[] DrawAgentUnitParams(CutsceneCustomAgent.AgentUnit unit, CutsceneCustomAgent.AgentUnit.ParamData[] vParams)
        {
            if (vParams == null)
                return vParams;

            float width = position.width / 2-20;
            float controlWidth = 100;
            float editColWidth = 70;
            float headWidth = (width - controlWidth- editColWidth) / 3;
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("参数名", GUILayout.Width(headWidth));
            GUILayout.Label("类型", GUILayout.Width(headWidth));
            GUILayout.Label("初始值", GUILayout.Width(headWidth));
            GUILayout.Label("是否可编辑", GUILayout.Width(editColWidth));
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < vParams.Length; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                // 参数名
                vParams[i].name = EditorGUILayout.TextField(vParams[i].name, GUILayout.Width(headWidth));
                // 类型
                vParams[i].type = (EVariableType)EditorEnumPop.PopEnum(GUIContent.none,vParams[i].type, null, new GUILayoutOption[] { GUILayout.Width(headWidth) });

                if (vParams[i].type == EVariableType.eBool)
                {
                    vParams[i].defaultValue = EditorGUILayout.Toggle(vParams[i].defaultValue.Equals("1", System.StringComparison.OrdinalIgnoreCase) || vParams[i].defaultValue.Equals("true", System.StringComparison.OrdinalIgnoreCase), new GUILayoutOption[] { GUILayout.Width(headWidth) }) ? "1" : "0";
                }
                else if (vParams[i].type == EVariableType.eInt)
                {
                    int valInt = 0;
                    if (!int.TryParse(vParams[i].defaultValue, out valInt))
                    {
                        valInt = 0;
                    }
                    valInt = EditorGUILayout.IntField(valInt, new GUILayoutOption[] { GUILayout.Width(headWidth) });
                    vParams[i].defaultValue = valInt.ToString();
                }
                else if (vParams[i].type == EVariableType.eObjId)
                {
                    int valInt = 0;
                    if (!int.TryParse(vParams[i].defaultValue, out valInt))
                    {
                        valInt = 0;
                    }
                    valInt = EditorGUILayout.IntField(valInt, new GUILayoutOption[] { GUILayout.Width(headWidth) });
                    vParams[i].defaultValue = valInt.ToString();
                }
                else if (vParams[i].type == EVariableType.eFloat)
                {
                    float valInt = 0;
                    if (!float.TryParse(vParams[i].defaultValue, out valInt))
                    {
                        valInt = 0;
                    }
                    valInt = EditorGUILayout.FloatField(valInt, new GUILayoutOption[] { GUILayout.Width(headWidth) });
                    vParams[i].defaultValue = valInt.ToString("F3");
                }
                else if (vParams[i].type == EVariableType.eString)
                {
                    vParams[i].defaultValue = EditorGUILayout.TextField(vParams[i].defaultValue, new GUILayoutOption[] { GUILayout.Width(headWidth) });
                }
                else if (vParams[i].type == EVariableType.eVec2)
                {
                    Vector2 vec = Vector2.zero;
                    var splitV = vParams[i].defaultValue.Split(new char[] { '|', ',' });
                    if (splitV.Length >= 2)
                    {
                        float x = 0, y = 0;
                        if (float.TryParse(splitV[0], out x))
                            vec.x = x;
                        if (float.TryParse(splitV[1], out y))
                            vec.y = y;
                    }
                    vec = EditorGUILayout.Vector2Field("", vec, new GUILayoutOption[] { GUILayout.Width(headWidth) });
                    vParams[i].defaultValue = vec.x.ToString("F3") + "|" + vec.y.ToString("F3");
                }
                else if (vParams[i].type == EVariableType.eVec3)
                {
                    Vector3 vec = Vector3.zero;
                    var splitV = vParams[i].defaultValue.Split(new char[] { '|', ',' });
                    if (splitV.Length >= 3)
                    {
                        float x = 0, y = 0, z = 0;
                        if (float.TryParse(splitV[0], out x))
                            vec.x = x;
                        if (float.TryParse(splitV[1], out y))
                            vec.y = y;
                        if (float.TryParse(splitV[1], out y))
                            vec.z = z;
                    }
                    vec = EditorGUILayout.Vector3Field("", vec, new GUILayoutOption[] { GUILayout.Width(headWidth) });
                    vParams[i].defaultValue = vec.x.ToString("F3") + "|" + vec.y.ToString("F3") + "|" + vec.z.ToString("F3");
                }
                else if (vParams[i].type == EVariableType.eVec4)
                {
                    Vector4 vec = Vector4.zero;
                    var splitV = vParams[i].defaultValue.Split(new char[] { '|', ',' });
                    if (splitV.Length >= 4)
                    {
                        float x = 0, y = 0, z = 0, w = 0;
                        if (float.TryParse(splitV[0], out x))
                            vec.x = x;
                        if (float.TryParse(splitV[1], out y))
                            vec.y = y;
                        if (float.TryParse(splitV[2], out z))
                            vec.z = z;
                        if (float.TryParse(splitV[3], out w))
                            vec.w = w;
                    }
                    vec = EditorGUILayout.Vector4Field("", vec, new GUILayoutOption[] { GUILayout.Width(headWidth) });
                    vParams[i].defaultValue = vec.x.ToString("F3") + "|" + vec.y.ToString("F3") + "|" + vec.z.ToString("F3") + "|" + vec.w.ToString("F3");
                }
                vParams[i].canEdit = EditorGUILayout.Toggle(vParams[i].canEdit, new GUILayoutOption[] { GUILayout.Width(editColWidth) });
                // 上移
                GUI.enabled = i > 0;
                if (GUILayout.Button("↑", GUILayout.Width(30)))
                {
                    var temp = vParams[i - 1];
                    vParams[i - 1] = vParams[i];
                    vParams[i] = temp;
                    unit.inputVariables = vParams;
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                // 下移
                GUI.enabled = i < vParams.Length - 1;
                if (GUILayout.Button("↓", GUILayout.Width(30)))
                {
                    var temp = vParams[i + 1];
                    vParams[i + 1] = vParams[i];
                    vParams[i] = temp;
                    unit.inputVariables = vParams;
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                GUI.enabled = true;

                // 删除
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    if(EditorUtility.DisplayDialog("提示", "是否删除改变量参数?","删除", "再想想"))
                    {
                        var list = new System.Collections.Generic.List<CutsceneCustomAgent.AgentUnit.ParamData>(vParams);
                        list.RemoveAt(i);
                        vParams = list.ToArray();
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            return vParams;
        }
    }
}

#endif