#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace Framework.Cutscene.Editor
{
    public class ConstUtil
    {
        public static readonly float kDurationGuiThickness = 5.0f;
        internal const string k_Elipsis = "…";
        internal static readonly double kMinDuration = 1 / 60.0;
        internal static readonly double kMaxTimeValue = 1000000;
        public static int kVisibilityBufferInPixels = 10;
        public static readonly double kTimeEpsilon = 1e-14;
        public static readonly double kFrameRateEpsilon = 1e-6;
        public static readonly double k_MaxTimelineDurationInSeconds = 9e6; //104 days of running time
        public static readonly double kFrameRateRounding = 1e-2;

        public static readonly float k_MinMixWidth = 2;
        public static readonly float k_MaxHandleWidth = 10f;
        public static readonly float k_MinHandleWidth = 1f;
    }
    public static class TextUtil
    {
        public static readonly string k_ImagePath = "icons/{0}.png";
        public static readonly string HoldText = L10n.Tr("HOLD");

        public static GUIContent NewGroupName = new GUIContent("新建组");
        public static string AddTrackName = "添加轨道";

        public static readonly IconData[] k_ClipErrorIcons = { new IconData { icon = iconWarn, tint = kClipErrorColor } };
        public static Texture2D m_iconWarn = null;
        public static Texture2D iconWarn
        {
            get
            {
                if (m_iconWarn == null)
                {
                    m_iconWarn = EditorGUIUtility.LoadRequired("console.warnicon.inactive.sml") as Texture2D;
                }
                return m_iconWarn;
            }
        }
        public static readonly Color kClipErrorColor = new Color(0.957f, 0.737f, 0.008f, 1f);
        public static Color ClipBgColor = new Color(70.0f / 255.0f, 70.0f / 255.0f, 70.0f / 255.0f, 1.0f);
        private static GUIStyle ms_displayBackground = null;
        public static GUIStyle displayBackground
        {
            get
            {
                if (ms_displayBackground == null)
                {
                    ms_displayBackground = GetGUIStyle("sequenceClip");
                }
                return ms_displayBackground;
            }
        }
        private static GUIStyle ms_timeCursor = null;
        public static GUIStyle timeCursor
        {
            get
            {
                if (ms_timeCursor == null)
                {
                    ms_timeCursor = GetGUIStyle("Icon-TimeCursor");
                }
                return ms_timeCursor;
            }
        }
        private static GUIStyle ms_fontClip = null;
        public static GUIStyle fontClip
        {
            get
            {
                if (ms_fontClip == null)
                {
                    ms_fontClip = new GUIStyle(GetGUIStyle("Font-Clip"));
                }
                return ms_fontClip;
            }
        }
        private static GUIStyle ms_fontClipLoop = null;
        public static GUIStyle fontClipLoop
        {
            get
            {
                if (ms_fontClipLoop == null)
                {
                    ms_fontClipLoop = new GUIStyle(GetGUIStyle("Font-Clip")) { fontStyle = FontStyle.Bold };
                }
                return ms_fontClipLoop;
            }
        }
        private static GUIStyle ms_selectedStyle = null;
        public static GUIStyle selectedStyle
        {
            get
            {
                if (ms_selectedStyle == null)
                {
                    ms_selectedStyle = GetGUIStyle("Color-Selected");
                }
                return ms_selectedStyle;
            }
        }
        private static GUIStyle ms_TileStyle = null;
        public static GUIStyle titleStyle
        {
            get
            {
                if (ms_TileStyle == null)
                {
                    ms_TileStyle = new GUIStyle();
                    ms_TileStyle.fontSize = 10;
                    ms_TileStyle.normal.textColor = Color.white;
                    ms_TileStyle.alignment = TextAnchor.MiddleCenter;

                }
                return ms_TileStyle;
            }
        }
        private static GUIStyle ms_PanelTileStyle = null;
        public static GUIStyle panelTitleStyle
        {
            get
            {
                if (ms_PanelTileStyle == null)
                {
                    ms_PanelTileStyle = new GUIStyle();
                    ms_PanelTileStyle.fontSize = 15;
                    ms_PanelTileStyle.normal.textColor = Color.white;
                    ms_PanelTileStyle.alignment = TextAnchor.MiddleCenter;

                }
                return ms_PanelTileStyle;
            }
        }
        public static GUIStyle GetGUIStyle(string s)
        {
            return EditorStyles.FromUSS(s);
        }
        static string ResolveIcon(string icon)
        {
            return string.Format(k_ImagePath, icon);
        }
        static Dictionary<string, Texture2D> ms_LoadIcons = new Dictionary<string, Texture2D>();
        public static Texture2D LoadIcon(string iconName)
        {
            try
            {
                if (ms_LoadIcons.TryGetValue(iconName, out var icon))
                    return icon;
                var obj = UnityEditor.EditorGUIUtility.TrIconContent(iconName);
                if (obj != null)
                {
                    ms_LoadIcons[iconName] = obj.image as Texture2D; 
                    return obj.image as Texture2D;
                }
                var objT = EditorGUIUtility.LoadRequired(iconName == null ? null : ResolveIcon(iconName));
                if (objT == null)
                {
                    ms_LoadIcons[iconName] = null;
                    return null;
                }
                ms_LoadIcons[iconName] = objT as Texture2D;
                return objT as Texture2D;
            }
            catch
            {
                return null;
            }
        }
        public static string Elipsify(string label, float destinationWidth, float neededWidth)
        {
            var ret = label;

            if (label.Length == 0)
                return ret;

            if (destinationWidth < neededWidth)
            {
                float averageWidthOfOneChar = neededWidth / label.Length;
                int floor = Mathf.Max((int)Mathf.Floor(destinationWidth / averageWidthOfOneChar), 0);

                if (floor < ConstUtil.k_Elipsis.Length)
                    ret = string.Empty;
                else if (floor == ConstUtil.k_Elipsis.Length)
                    ret = ConstUtil.k_Elipsis;
                else if (floor < label.Length)
                    ret = label.Substring(0, floor - ConstUtil.k_Elipsis.Length) + ConstUtil.k_Elipsis;
            }

            return ret;
        }
        public static Styles styles { get { return _styles != null ? _styles : _styles = new Styles(); } }
        public static Styles _styles = null;
        public static GUIStyle OutputPort { get { return new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperRight }; } }
        public class Styles
        {
            public GUIStyle inputPort, nodeHeader, nodeHeaderDesc, nodeBody, tooltip, nodeHighlight;

            public Styles()
            {
                GUIStyle baseStyle = new GUIStyle("Label");
                baseStyle.fixedHeight = 18;

                inputPort = new GUIStyle(baseStyle);
                inputPort.alignment = TextAnchor.UpperLeft;
                inputPort.padding.left = 10;

                nodeHeader = new GUIStyle();
                nodeHeader.alignment = TextAnchor.MiddleCenter;
                nodeHeader.fontSize = 16;
                nodeHeader.fontStyle = FontStyle.Bold;
                nodeHeader.normal.textColor = Color.white;

                nodeBody = new GUIStyle();
                nodeBody.normal.background = IconUtils.nodeBody;
                nodeBody.border = new RectOffset(32, 32, 32, 32);
                nodeBody.padding = new RectOffset(16, 16, 4, 16);

                nodeHighlight = new GUIStyle();
                nodeHighlight.normal.background = IconUtils.nodeHighlight;
                nodeHighlight.border = new RectOffset(32, 32, 32, 32);

                tooltip = new GUIStyle("helpBox");
                tooltip.alignment = TextAnchor.MiddleCenter;
            }
        }
    }
    internal static class TimeUtil
    {
        public static float GetAnimationClipLength(AnimationClip clip)
        {
            if (clip == null || clip.empty)
                return 0;

            float length = clip.length;
            if (clip.frameRate > 0)
            {
                float frames = Mathf.Round(clip.length * clip.frameRate);
                length = frames / clip.frameRate;
            }
            return length;
        }
        public static bool HasUsableAssetDuration(ClipDraw clip)
        {
            double length = clip.GetDuration();
            return (length < ConstUtil.kMaxTimeValue) && !double.IsInfinity(length) && !double.IsNaN(length);
        }
        public static double FromFrames(double frames, double frameRate)
        {
            if (frameRate <= 0) frameRate = 30;
            return frames / frameRate;
        }
        public static double ToExactFrames(double time, double frameRate)
        {
            if (frameRate <= 0) frameRate = 30;
            return time * frameRate;
        }
        public static bool OnFrameBoundary(double time, double frameRate)
        {
            return OnFrameBoundary(time, frameRate, GetEpsilon(time, frameRate));
        }

        public static bool OnFrameBoundary(double time, double frameRate, double epsilon)
        {
            if (frameRate <= 0) frameRate = 30;

            double exact = ToExactFrames(time, frameRate);
            double rounded = Math.Round(exact);

            return Math.Abs(exact - rounded) < epsilon;
        }
        public static double GetEpsilon(double time, double frameRate)
        {
            return Math.Max(Math.Abs(time), 1) * frameRate * ConstUtil.kTimeEpsilon;
        }
        public static int ToFrames(double time, double frameRate)
        {
            if (frameRate <= 0) frameRate = 30;
            time = Math.Min(Math.Max(time, -ConstUtil.k_MaxTimelineDurationInSeconds), ConstUtil.k_MaxTimelineDurationInSeconds);
            // this matches OnFrameBoundary
            double tolerance = GetEpsilon(time, frameRate);
            if (time < 0)
            {
                return (int)Math.Ceiling(time * frameRate - tolerance);
            }
            return (int)Math.Floor(time * frameRate + tolerance);
        }
        public static string ToTimeString(this Framework.ED.TimeArea.TimeFormat timeFormat, double time, double frameRate, string format = "f2")
        {
            switch (timeFormat)
            {
                case Framework.ED.TimeArea.TimeFormat.Frames: return TimeAsFrames(time, frameRate, format);
                case Framework.ED.TimeArea.TimeFormat.Seconds: return time.ToString(format, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat);
            }

            return time.ToString(format);
        }
        public static string TimeAsFrames(double timeValue, double frameRate, string format = "F2")
        {
            if (OnFrameBoundary(timeValue, frameRate)) // make integral values when on time borders
                return ToFrames(timeValue, frameRate).ToString();
            return ToExactFrames(timeValue, frameRate).ToString(format);
        }
        static string RemoveChar(string str, Func<char, bool> charToRemoveFunc)
        {
            var len = str.Length;
            var src = str.ToCharArray();
            var dstIdx = 0;
            for (var i = 0; i < len; i++)
            {
                if (!charToRemoveFunc(src[i]))
                    src[dstIdx++] = src[i];
            }
            return new string(src, 0, dstIdx);
        }
        public static double ParseTimeSeconds(string timeCode, double frameRate, double defaultValue)
        {
            timeCode = RemoveChar(timeCode, c => char.IsWhiteSpace(c));
            string[] sections = timeCode.Split(':');
            if (sections.Length == 0 || sections.Length > 4)
                return defaultValue;

            int hours = 0;
            int minutes = 0;
            double seconds = 0;

            try
            {
                // depending on the format of the last numbers
                // seconds format
                string lastSection = sections[sections.Length - 1];
                {
                    if (!double.TryParse(lastSection, NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds))
                        if (Regex.Match(lastSection, @"^\d+\.\d+$").Success)
                            seconds = double.Parse(lastSection);
                        else
                            return defaultValue;

                    if (!double.TryParse(lastSection, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds))
                        return defaultValue;

                    if (sections.Length > 3) return defaultValue;
                    if (sections.Length > 1) minutes = int.Parse(sections[sections.Length - 2]);
                    if (sections.Length > 2) hours = int.Parse(sections[sections.Length - 3]);
                }
            }
            catch (FormatException)
            {
                return defaultValue;
            }

            return seconds + minutes * 60 + hours * 3600;
        }
        public static double FromTimeString(this TimelineDrawLogic state, string timeString)
        {
            double newTime = state.GetTimeFormat().FromTimeString(timeString, state.GetFrameRate(), -1);
            if (newTime >= 0.0)
            {
                return newTime;
                //    return state.timeReferenceMode == TimeReferenceMode.Global ?
                //        state.editSequence.ToLocalTime(newTime) : newTime;
            }

            return state.GetCurrentTime();
        }
        public static double FromTimeString(this Framework.ED.TimeArea.TimeFormat timeFormat, string timeString, double frameRate, double defaultValue)
        {
            double time = defaultValue;
            switch (timeFormat)
            {
                case Framework.ED.TimeArea.TimeFormat.Frames:
                    if (!double.TryParse(timeString, NumberStyles.Any, CultureInfo.InvariantCulture, out time))
                        return defaultValue;
                    time = FromFrames(time, frameRate);
                    break;
                case Framework.ED.TimeArea.TimeFormat.Seconds:
                    time = ParseTimeSeconds(timeString, frameRate, defaultValue);
                    break;
                default:
                    time = defaultValue;
                    break;
            }

            return time;
        }
    }
}
#endif