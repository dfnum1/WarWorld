/********************************************************************
生成日期:	06:30:2025
类    名: 	EditorPreferences
作    者:	HappLI
描    述:	编辑器偏好设置
*********************************************************************/
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using Framework.ED;
using Framework.Cutscene.Runtime;

namespace Framework.Cutscene.Editor
{
    public class EditorPreferences
    {
        /// <summary> The last key we checked. This should be the one we modify </summary>
        private static string lastKey => GetProjectKeyPrefix() + "Settings";
        private static string GetProjectKeyPrefix()
        {
            // 用项目路径的 hash 作为前缀，保证唯一性
            string projectPath = Application.dataPath; // 例如 D:/MyProject/Assets
            int hash = projectPath.GetHashCode();
            return $"CutsceneEditor.{hash}.";
        }
        private static Dictionary<Type, Color> typeColors = new Dictionary<Type, Color>();
        private static Dictionary<string, Settings> settings = new Dictionary<string, Settings>();

        [System.Serializable]
        public class Settings : ISerializationCallbackReceiver
        {
            public int FrameRate = 30;
            public float playbackSpeedScale = 1;
            public string generatorCodePath = "Assets/OpenScripts/GameApp/Cutscene";
            public float animationRunStandardDistance = 7;
            public float animationRunStandardTime = 0.8f;
            public float animationRunStandardSpeed = 1;

            [SerializeField]
            public Color colorClipFont = new Color(0.569f, 0.580f, 0.588f, 1.0f);

            [SerializeField]
            public Color clipBckg = new Color(0.5f, 0.5f, 0.5f, 1.0f);

            [SerializeField]
            public Color clipSelectedBckg = new Color(0.7f, 0.7f, 0.7f, 1.0f);

            [SerializeField]
            public Color clipBorderColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);

            [SerializeField]
            public Color clipEaseBckgColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);

            [SerializeField]
            public Color clipBlendIn = new Color(0.286f, 0.306f, 0.329f, 1.0f);

            [SerializeField]
            public Color clipBlendInSelected = new Color(0.408f, 0.427f, 0.478f, 1.0f);

            [SerializeField]
            public Color clipBlendOut = new Color(0.286f, 0.306f, 0.329f, 1.0f);

            [SerializeField]
            public Color clipBlendOutSelected = new Color(0.408f, 0.427f, 0.478f, 1.0f);

            [SerializeField]
            public Color colorRecordingClipOutline = new Color(1, 0, 0, 0.9f);
            [SerializeField]
            public Color colorOverClip = new Color(1, 0, 0, 0.7f);

            [SerializeField] private Color32 _gridLineColor = new Color(0.45f, 0.45f, 0.45f);
            public Color32 gridLineColor { get { return _gridLineColor; } set { _gridLineColor = value; _gridTexture = null; _crossTexture = null; } }

            [SerializeField]
            private Color32 _gridBgColor = new Color(0.55f, 0.55f, 0.55f);
            [SerializeField]
            private Color _linkLineColor = Color.white;

            [SerializeField]
            private float _linkLineWidth = 5;
            public Color32 gridBgColor { get { return _gridBgColor; } set { _gridBgColor = value; _gridTexture = null; } }

            public Color linkLineColor { get { return _linkLineColor; } set { _linkLineColor = value; } }

            public float linkLineWidth { get { return _linkLineWidth; } set { _linkLineWidth = value; } }

            [Obsolete("Use maxZoom instead")]
            public float zoomOutLimit { get { return maxZoom; } set { maxZoom = value; } }

            [UnityEngine.Serialization.FormerlySerializedAs("zoomOutLimit")]
            public float maxZoom = 5f;
            public float minZoom = 1f;
            public Color32 highlightColor = new Color32(255, 255, 255, 255);
            public Color32 excudeColor = new Color32(255, 0, 0, 148);
            public Color32 nodeBgColor = new Color32(255, 255, 255, 148);
            public bool gridSnap = true;
            public bool autoSave = true;
            public bool zoomToMouse = true;
            public bool portTooltips = true;
            private Texture2D _gridTexture;
            public Texture2D gridTexture
            {
                get
                {
                    if (_gridTexture == null) _gridTexture = EditorUtils.GenerateGridTexture(gridLineColor, gridBgColor);
                    return _gridTexture;
                }
            }
            private Texture2D _crossTexture;
            public Texture2D crossTexture
            {
                get
                {
                    if (_crossTexture == null) _crossTexture = EditorUtils.GenerateCrossTexture(gridLineColor);
                    return _crossTexture;
                }
            }

            [SerializeField] private string typeColorsData = "";
            [NonSerialized] public Dictionary<string, Color> typeColors = new Dictionary<string, Color>();

            [SerializeField] private string playAbleDebugerData = "";
            [NonSerialized] public Dictionary<string, bool> playAbleDebugers = new Dictionary<string, bool>();

            public void OnAfterDeserialize()
            {
                // Deserialize typeColorsData
                typeColors = new Dictionary<string, Color>();
                string[] data = typeColorsData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < data.Length; i += 2)
                {
                    Color col;
                    if (ColorUtility.TryParseHtmlString("#" + data[i + 1], out col))
                    {
                        typeColors.Add(data[i], col);
                    }
                }

                playAbleDebugers = new Dictionary<string, bool>();
                string[] debugerData = playAbleDebugerData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < debugerData.Length; i += 2)
                {
                    bool bValue = false;
                    if (bool.TryParse(debugerData[i + 1], out bValue))
                    {
                        playAbleDebugers.Add(debugerData[i], bValue);
                    }
                }
            }

            public void OnBeforeSerialize()
            {
                // Serialize typeColors
                typeColorsData = "";
                foreach (var item in typeColors)
                {
                    typeColorsData += item.Key + "," + ColorUtility.ToHtmlStringRGB(item.Value) + ",";
                }
                playAbleDebugerData = "";
                foreach (var item in playAbleDebugers)
                {
                    playAbleDebugerData += item.Key + "," + item.Value.ToString() + ",";
                }
            }
        }

        /// <summary> Get settings of current active editor </summary>
        public static Settings GetSettings()
        {
            if (!settings.ContainsKey(lastKey)) VerifyLoaded();
            return settings[lastKey];
        }

//#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider CreateActorSystemSettingsProvider() {
            SettingsProvider provider = new SettingsProvider("Preferences/CutsceneEditor", SettingsScope.User) {
                guiHandler = (searchContext) => { PreferencesGUI(); },
            };
            return provider;
        }
//#endif

//#if !UNITY_2019_1_OR_NEWER
//        [PreferenceItem("AT Editor")]
//#endif
        public static void PreferencesGUI()
        {
            VerifyLoaded();
            Settings settings = EditorPreferences.settings[lastKey];
            SystemSettingsGUI(lastKey, settings);
            TypeColorsGUI(lastKey, settings);
            if (GUILayout.Button(new GUIContent("Set Default", "Reset all values to default"), GUILayout.Width(120)))
            {
                ResetPrefs();
            }
        }

        static string[] ms_vFramePop = { "30", "45", "60" };
        private static void SystemSettingsGUI(string key, Settings settings)
        {
            //Label
            EditorGUILayout.LabelField("System", EditorStyles.boldLabel);
            int index = 0;
            for(int i =0; i < ms_vFramePop.Length; ++i)
            {
                if (ms_vFramePop[i] == settings.FrameRate.ToString())
                {
                    index = i;
                    break;
                }
            }
            settings.playbackSpeedScale = EditorGUILayout.Slider("EditScale", settings.playbackSpeedScale, 0.1f, 2.0f);
            index = EditorGUILayout.Popup("FrameRate", index, ms_vFramePop);
            settings.FrameRate = int.Parse(ms_vFramePop[index]);
            string lastPath = settings.generatorCodePath;
            lastPath = EditorGUILayout.TextField("Generator Code Path", settings.generatorCodePath);
            if(lastPath != settings.generatorCodePath)
            {
             //   if(!string.IsNullOrEmpty(settings.generatorCodePath) && System.IO.Directory.Exists(settings.generatorCodePath))
            //    {
            //        System.IO.Directory.Delete(settings.generatorCodePath,true);
           //     }
                settings.generatorCodePath = lastPath;
            }
            bool bChange = false;
            if (GUI.changed) bChange = true;


            var fields = settings.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            for(int i =0; i < fields.Length; ++i)
            {
                if (fields[i].Name == "FrameRate")
                    continue;
                if (fields[i].Name == "playbackSpeedScale")
                    continue;
                if (fields[i].FieldType == typeof(Color))
                {
                    Color col = (Color)fields[i].GetValue(settings);
                    EditorGUI.BeginChangeCheck();
                    col = EditorGUILayout.ColorField(fields[i].Name, col);
                    if (EditorGUI.EndChangeCheck())
                    {
                        fields[i].SetValue(settings, col);
                    }
                }
                else if (fields[i].FieldType == typeof(float))
                {
                    float col = (float)fields[i].GetValue(settings);
                    EditorGUI.BeginChangeCheck();
                    col = EditorGUILayout.FloatField(fields[i].Name, col);
                    if (EditorGUI.EndChangeCheck())
                    {
                        fields[i].SetValue(settings, col);
                    }
                }
            }
            if (bChange) SavePrefs(key, settings);

            EditorGUILayout.Space();
        }

        private static void TypeColorsGUI(string key, Settings settings)
        {
            //Label
            EditorGUILayout.LabelField("Types", EditorStyles.boldLabel);

            //Clone keys so we can enumerate the dictionary and make changes.
            var typeColorKeys = new List<Type>(typeColors.Keys);

            //Display type colors. Save them if they are edited by the user
            foreach (var type in typeColorKeys)
            {
                string typeColorKey = type.Name;
                Color col = typeColors[type];
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                col = EditorGUILayout.ColorField(typeColorKey, col);
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    typeColors[type] = col;
                    if (settings.typeColors.ContainsKey(typeColorKey)) settings.typeColors[typeColorKey] = col;
                    else settings.typeColors.Add(typeColorKey, col);
                    SavePrefs(key, settings);
                    RepaintAll();
                }
            }
        }

        /// <summary> Load prefs if they exist. Create if they don't </summary>
        private static Settings LoadPrefs()
        {
            // Create settings if it doesn't exist
            if (!EditorPrefs.HasKey(lastKey))
            {
                EditorPrefs.SetString(lastKey, JsonUtility.ToJson(new Settings()));
            }
            return JsonUtility.FromJson<Settings>(EditorPrefs.GetString(lastKey));
        }

        /// <summary> Delete all prefs </summary>
        public static void ResetPrefs()
        {
            if (EditorPrefs.HasKey(lastKey)) EditorPrefs.DeleteKey(lastKey);
            if (settings.ContainsKey(lastKey)) settings.Remove(lastKey);
            typeColors = new Dictionary<Type, Color>();
            VerifyLoaded();

            RepaintAll();
        }

        static void RepaintAll()
        {
            var editors = Resources.FindObjectsOfTypeAll<CutsceneEditor>();
            if (editors != null)
            {
                for (int i = 0; i < editors.Length; ++i)
                    editors[i].Repaint();
            }
        }

        /// <summary> Save preferences in EditorPrefs </summary>
        private static void SavePrefs(string key, Settings settings)
        {
            EditorPrefs.SetString(key, JsonUtility.ToJson(settings));
        }

        /// <summary> Check if we have loaded settings for given key. If not, load them </summary>
        private static void VerifyLoaded()
        {
            if (!settings.ContainsKey(lastKey)) settings.Add(lastKey, LoadPrefs());
        }

        /// <summary> Return color based on type </summary>
        public static Color GetTypeColor(System.Type type)
        {
            VerifyLoaded();
            if (type == null) return Color.white;
            Color col = Color.white;
            if (!typeColors.TryGetValue(type, out col))
            {
                string typeName = type.Name;
                if (settings[lastKey].typeColors.ContainsKey(typeName)) typeColors.Add(type, settings[lastKey].typeColors[typeName]);
                else
                {
#if UNITY_5_4_OR_NEWER
                    UnityEngine.Random.InitState(typeName.GetHashCode());
#else
                    UnityEngine.Random.seed = typeName.GetHashCode();
#endif
                    col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                    typeColors.Add(type, col);
                }
            }
            return col;
        }

        public static bool GetPlayableDebuger(EDataType type,int typeId, int customType)
        {
            VerifyLoaded();
            string key = type + "_" + typeId +"_" + customType;
            if (string.IsNullOrEmpty(key)) return true;
            bool debug = false;
            if (settings[lastKey].playAbleDebugers.TryGetValue(key, out debug))
            {
                return debug;
            }
            return true;
        }
        public static void SetPlayableDebuger(EDataType type, int typeId, int customType, bool bDebug)
        {
            VerifyLoaded();
            string key = type + "_" + typeId + "_" + customType;
            if (string.IsNullOrEmpty(key)) return;
            if(!settings[lastKey].playAbleDebugers.TryGetValue(key, out var existValue) || existValue != bDebug)
            {
                settings[lastKey].playAbleDebugers[key] = bDebug;
                SavePrefs(lastKey, settings[lastKey]);
            }
        }
    }
}
#endif