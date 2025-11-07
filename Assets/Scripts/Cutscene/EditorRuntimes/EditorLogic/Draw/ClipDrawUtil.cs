/********************************************************************
生成日期:	06:30:2025
类    名: 	ClipDrawUtil
作    者:	HappLI
描    述:	剪辑绘制工具类
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Framework.Cutscene.Editor
{

    [Flags]
    public enum ClipCaps
    {
        /// <summary>
        /// No features are supported.
        /// </summary>
        None = 0,

        /// <summary>
        /// The clip supports loops.
        /// </summary>
        Looping = 1 << 0,

        /// <summary>
        /// The clip supports clip extrapolation.
        /// </summary>
        Extrapolation = 1 << 1,

        /// <summary>
        /// The clip supports initial local times greater than zero.
        /// </summary>
        ClipIn = 1 << 2,

        /// <summary>
        /// The clip supports time scaling.
        /// </summary>
        SpeedMultiplier = 1 << 3,

        /// <summary>
        /// The clip supports blending between clips.
        /// </summary>
        Blending = 1 << 4,

        /// <summary>
        /// The clip supports time scaling, and sets the default trim mode in the editor to scale the clip
        /// (speed multiplier) when the start/end of the clip is trimmed.
        /// </summary>
        AutoScale = 1 << 5 | SpeedMultiplier,

        /// <summary>
        /// All features are supported.
        /// </summary>
        All = ~None
    }
    public enum BlendKind
    {
        None,
        Ease,
        Mix
    }

    enum BlendAngle
    {
        Descending,
        Ascending
    }
    struct ClipBorder
    {
        public readonly Color color;
        public readonly float thickness;

        ClipBorder(Color color, float thickness)
        {
            this.color = color;
            this.thickness = thickness;
        }

        const float k_ClipSelectionBorder = 1.0f;
        const float k_ClipRecordingBorder = 2.0f;

        public static ClipBorder Recording()
        {
            return new ClipBorder(EditorPreferences.GetSettings().colorRecordingClipOutline, k_ClipRecordingBorder);
        }

        public static ClipBorder Selection()
        {
            return new ClipBorder(Color.white, k_ClipSelectionBorder);
        }

        public static ClipBorder Default()
        {
            return new ClipBorder(EditorPreferences.GetSettings().clipBorderColor, k_ClipSelectionBorder);
        }
    }
    public struct ClipBlends
    {
        public readonly BlendKind inKind;
        public readonly Rect inRect;

        public readonly BlendKind outKind;
        public readonly Rect outRect;

        public ClipBlends(BlendKind inKind, Rect inRect, BlendKind outKind, Rect outRect)
        {
            this.inKind = inKind;
            this.inRect = inRect;
            this.outKind = outKind;
            this.outRect = outRect;
        }

        public static readonly ClipBlends kNone = new ClipBlends(BlendKind.None, Rect.zero, BlendKind.None, Rect.zero);
    }
    struct ClipDrawData
    {
        public IBaseClip clip;             // clip being drawn
        public Rect targetRect;               // rectangle to draw to
        public Rect unclippedRect;            // the clip's unclipped rect
        public Rect clippedRect;              // the clip's clipped rect to the visible time area
        public Rect clipCenterSection;        // clip center section
        public string title;                  // clip title
        public string tips;
        public bool selected;                 // is the clip selected
        public bool inlineCurvesSelected;     // is the inline curve of the clip selected
        public double localVisibleStartTime;
        public double localVisibleEndTime;
        public IconData[] leftIcons;
        public IconData[] rightIcons;
        public IBaseClip previousClip;
        public bool previousClipSelected;
        public bool supportsLooping;
        public int minLoopIndex;
        public List<Rect> loopRects;
        public ClipBlends clipBlends;
        public Color swatchColor;
        public bool overlapWithClip; // does this clip overlap with the previous clip
    }
    internal static class ClipDrawUtil
    {
        const float k_ClipSwatchLineThickness = 4.0f;
        const float k_MinClipWidth = 7.0f;
        const float k_ClipInOutMinWidth = 15.0f;
        const float k_ClipLoopsMinWidth = 20.0f;
        const float k_ClipLabelPadding = 6.0f;
        const float k_ClipLabelMinWidth = 10.0f;
        const float k_IconsPadding = 1.0f;
        const float k_ClipInlineWidth = 1.0f;
        public static readonly double kTimeEpsilon = 1e-14;
        static readonly double kMinOverlapTime = kTimeEpsilon * 1000;
        public static readonly IconData k_DiggableClipIcon = new IconData(TextUtil.LoadIcon("VideoClip Icon"));
        //--------------------------------------------------------
        public static void DrawClip(ClipDrawData drawData)
        {
            DrawDefaultClip(drawData);

            //if (drawData.clip.asset is AnimationPlayableAsset)
            //{
            //    var state = TimelineWindow.instance.state;
            //    if (state.recording && state.IsArmedForRecord(drawData.clip.GetParentTrack()))
            //    {
            //        ClipDrawer.DrawAnimationRecordBorder(drawData);
            //        ClipDrawer.DrawRecordProhibited(drawData);
            //    }
            //}
        }
        //--------------------------------------------------------
        public static void DrawDefaultClip(ClipDrawData drawData)
        {
            var setting = EditorPreferences.GetSettings();
            var blendInColor = drawData.selected ? setting.clipBlendInSelected : setting.clipBlendIn;
            var blendOutColor = drawData.selected ? setting.clipBlendOutSelected : setting.clipBlendOut;
            var easeBackgroundColor = setting.clipEaseBckgColor;

            DrawClipBlends(drawData.clipBlends, blendInColor, blendOutColor, easeBackgroundColor);
            DrawClipBackground(drawData.clipCenterSection, drawData.selected, drawData.overlapWithClip);

            if (drawData.targetRect.width > k_MinClipWidth)
            {
                DrawClipEditorBackground(drawData);
            }
            else
            {
                drawData.targetRect.width = k_MinClipWidth;
                drawData.clipCenterSection.width = k_MinClipWidth;
            }

            //  if (!drawData.ClipDrawOptions.hideScaleIndicator)
            //      DrawClipTimescale(drawData.targetRect, drawData.clippedRect, drawData.clip.timeScale);
            if (drawData.targetRect.width >= k_ClipInOutMinWidth)
                DrawClipInOut(drawData.targetRect, drawData.clip);

            var labelRect = drawData.clipCenterSection;
            if (drawData.targetRect.width >= k_ClipLoopsMinWidth)
            {
               // bool selected = drawData.selected || drawData.inlineCurvesSelected;
               // if (selected)
                {
                    if (drawData.loopRects != null && drawData.loopRects.Any())
                    {
                        DrawLoops(drawData);
                    }
                }
            }

            labelRect.xMin += k_ClipLabelPadding;
            labelRect.xMax -= k_ClipLabelPadding;

            if (labelRect.width > k_ClipLabelMinWidth)
            {
                DrawClipLabel(drawData, labelRect, Color.white);
            }

            DrawClipSwatch(drawData.targetRect, drawData.swatchColor);
            DrawClipBorder(drawData);
        }
        //--------------------------------------------------------
        static void DrawClipSwatch(Rect targetRect, Color swatchColor)
        {
            // Draw Colored Line at the bottom.
            var colorRect = targetRect;
            colorRect.yMin = colorRect.yMax - k_ClipSwatchLineThickness;
            EditorGUI.DrawRect(colorRect, swatchColor);
        }
        //--------------------------------------------------------
        static void DrawClipBorder(ClipDrawData drawData)
        {
            DrawClipDefaultBorder(drawData.clipCenterSection, ClipBorder.Default(), drawData.clipBlends);

            var selectionBorder = ClipBorder.Selection();

            if (drawData.selected)
                DrawClipSelectionBorder(drawData.clipCenterSection, selectionBorder, drawData.clipBlends);

            if (drawData.previousClip != null && drawData.previousClipSelected)
            {
                bool shouldDrawLeftLine = Math.Abs(drawData.previousClip.GetTime() - drawData.clip.GetTime()) < double.Epsilon;
                DrawClipBlendSelectionBorder(drawData.clipCenterSection, selectionBorder, drawData.clipBlends, shouldDrawLeftLine);
            }
        }
        //--------------------------------------------------------
        static void DrawClipDefaultBorder(Rect clipRect, ClipBorder border, ClipBlends blends)
        {
            var color = border.color;
            var thickness = border.thickness;

            // Draw vertical lines at the edges of the clip
            EditorGUI.DrawRect(new Rect(clipRect.xMin, clipRect.y, thickness, clipRect.height), color); //left
            //only draw the right one when no out mix blend
            if (blends.outKind != BlendKind.Mix)
                EditorGUI.DrawRect(new Rect(clipRect.xMax - thickness, clipRect.y, thickness, clipRect.height), color); //right
            //draw a vertical line for the previous clip
            if (blends.inKind == BlendKind.Mix)
                EditorGUI.DrawRect(new Rect(blends.inRect.xMin, blends.inRect.y, thickness, blends.inRect.height), color); //left

            //Draw blend line
            if (blends.inKind == BlendKind.Mix)
                DrawBlendLine(blends.inRect, BlendAngle.Descending, thickness, color);
        }
        //--------------------------------------------------------
        static void DrawClipBlendSelectionBorder(Rect clipRect, ClipBorder border, ClipBlends blends, bool shouldLeftLine = false)
        {
            var color = border.color;
            var thickness = border.thickness;
            if (blends.inKind == BlendKind.Mix)
            {
                DrawBlendLine(blends.inRect, BlendAngle.Descending, thickness, color);
                var xBottom1 = blends.inRect.xMin;
                var xBottom2 = blends.inRect.xMax;
                EditorGUI.DrawRect(new Rect(xBottom1, clipRect.max.y - thickness, xBottom2 - xBottom1, thickness), color);
                if (shouldLeftLine)
                    EditorGUI.DrawRect(new Rect(xBottom1, clipRect.min.y, thickness, clipRect.max.y - clipRect.min.y), color);
            }
        }
        //--------------------------------------------------------
        public static void DrawClipSelectionBorder(Rect clipRect, ClipBorder border, ClipBlends blends)
        {
            var thickness = border.thickness;
            var color = border.color;
            var min = clipRect.min;
            var max = clipRect.max;

            //Left line
            if (blends.inKind == BlendKind.None)
                EditorGUI.DrawRect(new Rect(min.x, min.y, thickness, max.y - min.y), color);
            else
                DrawBlendLine(blends.inRect, blends.inKind == BlendKind.Mix ? BlendAngle.Descending : BlendAngle.Ascending, thickness, color);

            //Right line
            if (blends.outKind == BlendKind.None)
                EditorGUI.DrawRect(new Rect(max.x - thickness, min.y, thickness, max.y - min.y), color);
            else
                DrawBlendLine(blends.outRect, BlendAngle.Descending, thickness, color);

            //Top line
            var xTop1 = blends.inKind == BlendKind.Mix ? blends.inRect.xMin : min.x;
            var xTop2 = max.x;
            EditorGUI.DrawRect(new Rect(xTop1, min.y, xTop2 - xTop1, thickness), color);

            //Bottom line
            var xBottom1 = blends.inKind == BlendKind.Ease ? blends.inRect.xMin : min.x;
            var xBottom2 = blends.outKind == BlendKind.None ? max.x : blends.outRect.xMax;
            EditorGUI.DrawRect(new Rect(xBottom1, max.y - thickness, xBottom2 - xBottom1, thickness), color);
        }
        //--------------------------------------------------------
        static Vector3[] s_BlendLines = new Vector3[4];
        static void DrawBlendLine(Rect rect, BlendAngle blendAngle, float width, Color color)
        {
            var halfWidth = width / 2.0f;
            Vector2 p0, p1;
            var inverse = 1.0f;
            if (blendAngle == BlendAngle.Descending)
            {
                p0 = rect.min;
                p1 = rect.max;
            }
            else
            {
                p0 = new Vector2(rect.xMax, rect.yMin);
                p1 = new Vector2(rect.xMin, rect.yMax);
                inverse = -1.0f;
            }
            s_BlendLines[0] = new Vector3(p0.x - halfWidth, p0.y + halfWidth * inverse);
            s_BlendLines[1] = new Vector3(p1.x - halfWidth, p1.y + halfWidth * inverse);
            s_BlendLines[2] = new Vector3(p1.x + halfWidth, p1.y - halfWidth * inverse);
            s_BlendLines[3] = new Vector3(p0.x + halfWidth, p0.y - halfWidth * inverse);
            EditorUtil.DrawPolygonAA(color, s_BlendLines);
        }
        //--------------------------------------------------------
        static void DrawLoops(ClipDrawData drawData)
        {
            if (drawData.loopRects == null || drawData.loopRects.Count == 0)
                return;

            var oldColor = GUI.color;
            var setting = EditorPreferences.GetSettings();

            int loopIndex = drawData.minLoopIndex;
            for (int l = 0; l < drawData.loopRects.Count; l++)
            {
                Rect theRect = drawData.loopRects[l];
                GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.2f);
                EditorGUI.DrawRect(theRect, new Color(0.0f, 0.0f, 0.0f, 0.2f));
                var style = TextUtil.fontClipLoop;
                GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.3f);
                var loopContent = new GUIContent(drawData.supportsLooping ? GetLoopString(loopIndex) : TextUtil.HoldText);
                EditorGUI.LabelField(theRect, loopContent, style);

                loopIndex++;
            }

            GUI.color = oldColor;
        }
        //--------------------------------------------------------
        static void DrawClipLabel(ClipDrawData data, Rect availableRect, Color color)
        {
            var setting = EditorPreferences.GetSettings();
            string errorText=null;// = "Error no asset";
            var hasError = !string.IsNullOrEmpty(errorText);
            var textColor = hasError ? TextUtil.kClipErrorColor : color;
            var tooltip = hasError ? errorText : data.tips;
            var displayTitle = true;

            if (hasError)
                DrawClipLabel(data.title, availableRect, textColor, TextUtil.k_ClipErrorIcons, null, tooltip, displayTitle);
            else
                DrawClipLabel(data.title, availableRect, textColor, data.leftIcons, data.rightIcons, tooltip, displayTitle);
        }
        //--------------------------------------------------------
        public static void DrawClipLabel(string title, Rect availableRect, Color color, string errorText = "", bool displayTitle = true, GUIStyle style =null)
        {
            var hasError = !string.IsNullOrEmpty(errorText);
            var textColor = hasError ? TextUtil.kClipErrorColor : color;

            if (hasError)
                DrawClipLabel(title, availableRect, textColor, TextUtil.k_ClipErrorIcons, null, errorText, displayTitle, style);
            else
                DrawClipLabel(title, availableRect, textColor, null, null, errorText, displayTitle, style);
        }
        //--------------------------------------------------------
        static readonly GUIContent s_TitleContent = new GUIContent();
        static void DrawClipLabel(string title, Rect availableRect, Color textColor, IconData[] leftIcons, IconData[] rightIcons, string tooltipMessage = "", bool displayTitle = true, GUIStyle style = null)
        {
            var neededIconWidthLeft = 0.0f;
            var neededIconWidthRight = 0.0f;

            if (style == null)
                style = TextUtil.fontClip;

            if (leftIcons != null)
                for (int i = 0, n = leftIcons.Length; i < n; ++i)
                    neededIconWidthLeft += leftIcons[i].width + k_IconsPadding;

            if (rightIcons != null)
                for (int i = 0, n = rightIcons.Length; i < n; ++i)
                    neededIconWidthRight += rightIcons[i].width + k_IconsPadding;

            var neededIconWidth = Mathf.Max(neededIconWidthLeft, neededIconWidthRight);
            float iconPosX = availableRect.center.x;
            float halfTextWidth = 0;
            if (displayTitle)
            {
                s_TitleContent.text = title;
                var neededTextWidth = style.CalcSize(s_TitleContent).x;
                if (neededTextWidth > availableRect.width)
                    s_TitleContent.text = TextUtil.Elipsify(title, availableRect.width, neededTextWidth);
                halfTextWidth = neededTextWidth / 2.0f;
            }
            else
            {
                // text is set explicitly to an empty string to avoid drawing the default text on mac.
                s_TitleContent.text = String.Empty;
                iconPosX -= neededIconWidth / 2.0f;
            }

            // Atomic operation: We either show all icons or no icons at all
            if (halfTextWidth + neededIconWidth < availableRect.width / 2.0f)
            {
                if (leftIcons != null)
                    DrawClipIcons(leftIcons, IconData.Side.Left, iconPosX - halfTextWidth, availableRect.center.y);

                if (rightIcons != null)
                    DrawClipIcons(rightIcons, IconData.Side.Right, iconPosX + halfTextWidth, availableRect.center.y);
            }

            //draw label even if empty to display tooltip
            s_TitleContent.tooltip = tooltipMessage;
            DrawClipName(availableRect, s_TitleContent, textColor, style);
        }
        //--------------------------------------------------------
        public static string DelayedTextField(Rect rect, string text, GUIStyle style =null)
        {
            if(style == null) style = TextUtil.fontClip;
            rect.width = style.CalcSize(new GUIContent(text)).x;
            text = EditorGUI.DelayedTextField(rect, text, style);
            return text;
        }
        //--------------------------------------------------------
        static void DrawClipIcons(IconData[] icons, IconData.Side side, float positionX, float positionY)
        {
            float offset = k_IconsPadding;
            foreach (var iconData in icons)
            {
                offset += (iconData.width / 2.0f + k_IconsPadding);

                var iconRect =
                    new Rect(0.0f, 0.0f, iconData.width, iconData.height)
                    {
                        center = new Vector2(positionX + offset * (int)side, positionY)
                    };

                DrawIcon(iconRect, iconData.tint, iconData.icon);

                offset += iconData.width / 2.0f;
            }
        }
        //--------------------------------------------------------
        static void DrawClipName(Rect rect, GUIContent content, Color textColor, GUIStyle style = null)
        {
            if (style == null) style = TextUtil.fontClip;
            EditorUtil.ShadowLabel(rect, content, style, textColor, Color.black);
        }
        //--------------------------------------------------------
        static void DrawIcon(Rect imageRect, Color color, Texture2D icon)
        {
            GUI.DrawTexture(imageRect, icon, ScaleMode.ScaleAndCrop, true, 0, color, 0, 0);
        }
        //--------------------------------------------------------
        static readonly Dictionary<int, string> s_LoopStringCache = new Dictionary<int, string>(100);
        static string GetLoopString(int loopIndex)
        {
            string loopString = null;
            if (!s_LoopStringCache.TryGetValue(loopIndex, out loopString))
            {
                loopString = "L" + loopIndex;
                s_LoopStringCache[loopIndex] = loopString;
            }
            return loopString;
        }
        //--------------------------------------------------------
        static void DrawClipInOut(Rect targetRect, IBaseClip clip)
        {
            //var assetDuration = clip.GetClipAssetEndTime();

            //bool drawClipOut = assetDuration < double.MaxValue &&
            //                                 assetDuration - clip.end > ConstUtil.kTimeEpsilon;

            //bool drawClipIn = clip.clipIn > 0.0;

            //if (!drawClipIn && !drawClipOut)
            //    return;

            //var rect = targetRect;

            //if (drawClipOut)
            //{
            //    var icon = DirectorStyles.Instance.clipOut;
            //    var iconRect = new Rect(rect.xMax - icon.fixedWidth - 2.0f,
            //        rect.yMin + (rect.height - icon.fixedHeight) * 0.5f,
            //        icon.fixedWidth, icon.fixedHeight);

            //    GUI.Label(iconRect, GUIContent.none, icon);
            //}

            //if (drawClipIn)
            //{
            //    var icon = DirectorStyles.Instance.clipIn;
            //    var iconRect = new Rect(2.0f + rect.xMin,
            //        rect.yMin + (rect.height - icon.fixedHeight) * 0.5f,
            //        icon.fixedWidth, icon.fixedHeight);

            //    GUI.Label(iconRect, GUIContent.none, icon);
            //}
        }
        //--------------------------------------------------------
        static void DrawClipTimescale(Rect targetRect, Rect clippedRect, double timeScale)
        {
            if (timeScale != 1.0)
            {
                const float xOffset = 4.0f;
                const float yOffset = 6.0f;
                var segmentLength = timeScale > 1.0f ? 5.0f : 15.0f;

                // clamp to the visible region to reduce the line count (case 1213189), but adject the start segment to match the visuals of drawing targetRect
                var startX = clippedRect.min.x - ((clippedRect.min.x - targetRect.min.x) % (segmentLength * 2));
                var endX = clippedRect.max.x;

                var start = new Vector3(startX + xOffset, targetRect.min.y + yOffset, 0.0f);
                var end = new Vector3(endX - xOffset, targetRect.min.y + yOffset, 0.0f);

                var setting = EditorPreferences.GetSettings();
                EditorUtil.DrawDottedLine(start, end, segmentLength, setting.colorClipFont);
                EditorUtil.DrawDottedLine(start + new Vector3(0.0f, 1.0f, 0.0f), end + new Vector3(0.0f, 1.0f, 0.0f), segmentLength, setting.colorClipFont);
            }
        }
        //--------------------------------------------------------
        static void DrawClipEditorBackground(ClipDrawData drawData)
        {
            //var isRepaint = (Event.current.type == EventType.Repaint);
            //if (isRepaint && drawData.clipEditor != null)
            //{
            //    var customBodyRect = drawData.clippedRect;
            //    customBodyRect.yMin += k_ClipInlineWidth;
            //    customBodyRect.yMax -= k_ClipSwatchLineThickness;
            //    var region = new ClipBackgroundRegion(customBodyRect, drawData.localVisibleStartTime, drawData.localVisibleEndTime);
            //    try
            //    {
            //        drawData.clipEditor.DrawBackground(drawData.clip, region);
            //    }
            //    catch (Exception e)
            //    {
            //        Debug.LogException(e);
            //    }
            //}
        }
        //--------------------------------------------------------
        static void DrawClipBackground(Rect clipCenterSection, bool selected, bool bOverWithClip)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            var setting = EditorPreferences.GetSettings();

            var color = selected ? setting.clipSelectedBckg : setting.clipBckg;
            if (bOverWithClip) color = setting.colorOverClip;
            EditorGUI.DrawRect(clipCenterSection, color);
        }
        //--------------------------------------------------------
        static Vector3[] s_BlendVertices = new Vector3[3];
        static void DrawClipBlends(ClipBlends blends, Color inColor, Color outColor, Color backgroundColor)
        {
            switch (blends.inKind)
            {
                case BlendKind.Ease:
                    //     2
                    //   / |
                    //  /  |
                    // 0---1
                    EditorGUI.DrawRect(blends.inRect, backgroundColor);
                    s_BlendVertices[0] = new Vector3(blends.inRect.xMin, blends.inRect.yMax);
                    s_BlendVertices[1] = new Vector3(blends.inRect.xMax, blends.inRect.yMax);
                    s_BlendVertices[2] = new Vector3(blends.inRect.xMax, blends.inRect.yMin);
                    EditorUtil.DrawPolygonAA(inColor, s_BlendVertices);
                    break;
                case BlendKind.Mix:
                    // 0---2
                    //  \  |
                    //   \ |
                    //     1
                    s_BlendVertices[0] = new Vector3(blends.inRect.xMin, blends.inRect.yMin);
                    s_BlendVertices[1] = new Vector3(blends.inRect.xMax, blends.inRect.yMax);
                    s_BlendVertices[2] = new Vector3(blends.inRect.xMax, blends.inRect.yMin);
                    EditorUtil.DrawPolygonAA(inColor, s_BlendVertices);
                    break;
            }

            if (blends.outKind != BlendKind.None)
            {
                if (blends.outKind == BlendKind.Ease)
                    EditorGUI.DrawRect(blends.outRect, backgroundColor);
                // 0
                // | \
                // |  \
                // 1---2
                s_BlendVertices[0] = new Vector3(blends.outRect.xMin, blends.outRect.yMin);
                s_BlendVertices[1] = new Vector3(blends.outRect.xMin, blends.outRect.yMax);
                s_BlendVertices[2] = new Vector3(blends.outRect.xMax, blends.outRect.yMax);
                EditorUtil.DrawPolygonAA(outColor, s_BlendVertices);
            }
        }
        //--------------------------------------------------------
        public static bool Overlaps(ClipDraw blendOut, ClipDraw blendIn)
        {
            if (blendIn == blendOut)
                return false;

            if (Math.Abs(blendIn.GetBegin() - blendOut.GetBegin()) < kTimeEpsilon)
            {
                return blendIn.GetDuration() >= blendOut.GetDuration();
            }

            return blendIn.GetBegin() >= blendOut.GetBegin() && blendIn.GetBegin() < blendOut.GetEnd();
        }
        //--------------------------------------------------------
        public static void UpdateClipIntersection(ClipDraw blendOutClip, ClipDraw blendInClip)
        {
        //    if (!blendOutClip.SupportsBlending() || !blendInClip.SupportsBlending())
        //        return;

            if (blendInClip.GetBegin() - blendOutClip.GetBegin() < blendOutClip.GetDuration() - blendInClip.GetDuration())
                return;

            float duration = Math.Max(0, blendOutClip.GetBegin() + blendOutClip.GetDuration() - blendInClip.GetBegin());
            duration = duration <= kMinOverlapTime ? 0 : duration;

            bool bDirty = blendOutClip.clip.GetBlendDuration(ECutsceneClipBlendType.Out) != duration || blendInClip.clip.GetBlendDuration(ECutsceneClipBlendType.In) !=duration;
            if(bDirty)
            {
                blendOutClip.RegisterUndo();
                blendOutClip.clip.SetBlendDuration(ECutsceneClipBlendType.Out, duration);
                blendInClip.clip.SetBlendDuration(ECutsceneClipBlendType.In, duration);
            }

            //var blendInMode = blendInClip.blendInCurveMode;
            //var blendOutMode = blendOutClip.blendOutCurveMode;

            //if (blendInMode == TimelineClip.BlendCurveMode.Manual && blendOutMode == TimelineClip.BlendCurveMode.Auto)
            //{
            //    blendOutClip.mixOutCurve = CurveEditUtility.CreateMatchingCurve(blendInClip.mixInCurve);
            //}
            //else if (blendInMode == TimelineClip.BlendCurveMode.Auto && blendOutMode == TimelineClip.BlendCurveMode.Manual)
            //{
            //    blendInClip.mixInCurve = CurveEditUtility.CreateMatchingCurve(blendOutClip.mixOutCurve);
            //}
            //else if (blendInMode == TimelineClip.BlendCurveMode.Auto && blendOutMode == TimelineClip.BlendCurveMode.Auto)
            //{
            //    blendInClip.mixInCurve = null; // resets to default curves
            //    blendOutClip.mixOutCurve = null;
            //}
        }
        //--------------------------------------------------------
        public static void Draw(TimelineDrawLogic state, double start, double end)
        {
            if(Mathf.Abs((float)end - (float)start) < 0.1f)
            {
                var bounds = state.timelineRect;
                bounds.xMin = Mathf.Max(bounds.xMin, state.TimeToTimeAreaPixel(start));
                bounds.xMax = Mathf.Min(bounds.xMax, state.TimeToTimeAreaPixel(start+0.1f));

                bounds.position += state.timeZoomRect.position;

                EditorUtil.DrawDottedLine(new Vector2(bounds.xMin, bounds.yMin), new Vector2(bounds.xMin, bounds.yMax), 4.0f, Color.black);
            }
            else
            {
                var bounds = state.timelineRect;
                bounds.xMin = Mathf.Max(bounds.xMin, state.TimeToTimeAreaPixel(start));
                bounds.xMax = Mathf.Min(bounds.xMax, state.TimeToTimeAreaPixel(end));

                bounds.position += state.timeZoomRect.position;
                var color = TextUtil.selectedStyle.focused.textColor;
                color.a = 0.12f;
                EditorGUI.DrawRect(bounds, color);

                EditorGUI.DrawRect(new Rect(bounds.position + Vector2.up * 5, new Vector2(bounds.width, 5)), TextUtil.selectedStyle.normal.textColor);

                EditorUtil.DrawDottedLine(new Vector2(bounds.xMin, bounds.yMin), new Vector2(bounds.xMin, bounds.yMax), 4.0f, Color.black);
                EditorUtil.DrawDottedLine(new Vector2(bounds.xMax, bounds.yMin), new Vector2(bounds.xMax, bounds.yMax), 4.0f, Color.black);
            }

        }
    }
}
#endif