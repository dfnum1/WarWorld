/********************************************************************
生成日期:	06:30:2025
类    名: 	ToolBarDrawLogic
作    者:	HappLI
描    述:	工具栏绘制逻辑
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Runtime;
using Framework.ED;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    [EditorBinder(typeof(CutsceneEditor), "ToolBarRect")]
    public class ToolBarDrawLogic : AEditorLogic
    {
        //--------------------------------------------------------
        protected override void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("创建", new GUILayoutOption[] { GUILayout.Width(80) }))
            {
                CutsceneObject cutscene = ScriptableObject.CreateInstance<CutsceneObject>();
                string saveFile = EditorUtility.SaveFilePanel("保存cutscene", Application.dataPath, "New", "asset");
                saveFile = saveFile.Replace("\\", "/");
                if (!string.IsNullOrEmpty(saveFile) && saveFile.StartsWith(Application.dataPath.Replace("\\", "/")))
                {
                    saveFile = saveFile.Replace("\\", "/").Replace(Application.dataPath.Replace("\\", "/"), "Assets");
                    if (saveFile.StartsWith("/")) saveFile = saveFile.Substring(1);
                    AssetDatabase.CreateAsset(cutscene, saveFile);
                    AssetDatabase.SaveAssets();

                    GetOwner<CutsceneEditor>().OnChangeSelect(cutscene);
                }
            }
            if(GUILayout.Button("保存", new GUILayoutOption[] { GUILayout.Width(80) }))
            {
                GetOwner().SaveChanges();
            }
            if(GUILayout.Button("自定义行为参数编辑", new GUILayoutOption[] { GUILayout.Width(120) }))
            {
                CutsceneCustomAgentEditor.Open();
            }
            if (GUILayout.Button("行为树", new GUILayoutOption[] { GUILayout.Width(120) }))
            {
                GetOwner<CutsceneEditor>().OpenAgentTreeEdit();
            }
            if (Application.isPlaying && GUILayout.Button("打开游戏运行时正在播放", new GUILayoutOption[] { GUILayout.Width(150) }))
            {
                var runtimeMgr = DataUtils.GetRuntimeCutsceneManger();
                if(runtimeMgr == null)
                {
                    EditorUtility.DisplayDialog("提示", "当前没有正在播放的Cutscene", "确定");
                    return;
                }
                var cutscenes = runtimeMgr.GetAllCutscenes();
                if(cutscenes == null || cutscenes.Count<=0)
                {
                    EditorUtility.DisplayDialog("提示", "当前没有正在播放的Cutscene", "确定");
                    return;
                }
                GenericMenu menu = new GenericMenu();
                foreach(var cs in cutscenes)
                {
                    var pData = cs.Value.GetCutsceneData();
                    if (pData == null)
                        continue;
                    if (pData.GetOwnerObject() == null)
                        continue;

                    if(!(pData.GetOwnerObject() is CutsceneObject))
                    {
                        continue;
                    }
                    if (cs.Value.GetPlayable() == null) continue;
                    string subName = cs.Value.GetPlayable().GetName();
                    if (subName == null) subName = "";
                    CutsceneObject cutsceneObj = pData.GetOwnerObject() as CutsceneObject;
                    menu.AddItem(new GUIContent(cutsceneObj.name + "-" + subName), false, (obj) => {
                        var cso = obj as CutsceneInstance;
                        if(cso!=null)
                            GetOwner<CutsceneEditor>().OpenRuntimePlayingCutscene(cso);
                    }, cs.Value);
                }
                menu.ShowAsContext();
            }
            if (GUILayout.Button("文档说明", new GUILayoutOption[] { GUILayout.Width(80) }))
            {
                Application.OpenURL("https://docs.qq.com/doc/DTHNGdXpNaVdmT1dx");
            }
            GUILayout.EndHorizontal();
        }
    }
}

#endif