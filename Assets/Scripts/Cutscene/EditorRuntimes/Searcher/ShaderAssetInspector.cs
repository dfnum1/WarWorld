#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    [Framework.DrawProps.StringViewPlugin("OnDrawSelectShaderInspector")]
    public class ShaderAssetInspector
    {
        public static System.Object OnDrawSelectShaderInspector(System.Object pData, System.Reflection.FieldInfo pField, GUIContent displayName)
        {
            if (pField.FieldType != typeof(string))
                return pData;

            var pathObj = pField.GetValue(pData);
            string curPath = "";
            if (pathObj != null) curPath = pathObj.ToString();

            EditorGUILayout.BeginHorizontal();
            string lastPath = curPath;
            var shader = EditorGUILayout.ObjectField(displayName,Shader.Find(curPath), typeof(Shader), false);
            if (shader != null) curPath = shader.name;
            else curPath = "";
            if(lastPath!= curPath)
            {
                pField.SetValue(pData, curPath);
            }
            if (GUILayout.Button("Select", GUILayout.Width(45)))
            {
                var searchProvider = ScriptableObject.CreateInstance<ShaderSearcher>();
                searchProvider.onSelectShader = shader =>
                {
                    // 选中后回写shader路径
                    pField.SetValue(pData, shader.name);
                    GUI.FocusControl(null); // 失去焦点刷新
                };
                SearchWindow.Open(
                    new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                    searchProvider
                );
            }
            EditorGUILayout.EndHorizontal();
            return pData;
        }
    }
    public class ShaderSearcher : ScriptableObject, ISearchWindowProvider
    {
        public System.Action<Shader> onSelectShader;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Shaders"), 0)
            };

            // 获取所有Shader
            var shaderGuids = AssetDatabase.FindAssets("t:Shader");
            var shaderDict = new Dictionary<string, List<Shader>>();

            foreach (var guid in shaderGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (shader == null) continue;

                // 按"/"分组
                string[] split = shader.name.Split('/');
                string group = split.Length > 1 ? split[0] : "Other";
                if (!shaderDict.ContainsKey(group))
                    shaderDict[group] = new List<Shader>();
                shaderDict[group].Add(shader);
            }

            foreach (var group in shaderDict)
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent(group.Key), 1));
                foreach (var shader in group.Value)
                {
                    tree.Add(new SearchTreeEntry(new GUIContent(shader.name))
                    {
                        level = 2,
                        userData = shader
                    });
                }
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (entry.userData is Shader shader)
            {
                onSelectShader?.Invoke(shader);
                return true;
            }
            return false;
        }
    }
}
#endif