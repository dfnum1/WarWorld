/********************************************************************
生成日期:	06:30:2025
类    名: 	TimeUtils
作    者:	HappLI
描    述:	时间与绘制位置转换工具类
*********************************************************************/
#if UNITY_EDITOR
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    public class TimeUtils
    {
        //-----------------------------------------------------
        /// <summary>
        /// 时间转为绘制位置（像素）
        /// </summary>
        /// <param name="time">起始时间</param>
        /// <param name="duration">持续时间</param>
        /// <param name="timelineWidth">时间轴宽度（像素）</param>
        /// <param name="maxTime">时间轴最大时间</param>
        /// <returns>Vector2(x: 起始像素, y: 宽度像素)</returns>
        public static Vector2 TimeToPosition(float time, float duration, float timelineWidth, float maxTime)
        {
            float pixelsPerSecond = timelineWidth / maxTime;
            return new Vector2(time * pixelsPerSecond, duration * pixelsPerSecond);
        }
        //-----------------------------------------------------
        /// <summary>
        /// 绘制位置（像素）转为时间
        /// </summary>
        /// <param name="position">Vector2(x: 起始像素, y: 宽度像素)</param>
        /// <param name="timelineWidth">时间轴宽度（像素）</param>
        /// <param name="maxTime">时间轴最大时间</param>
        /// <returns>Vector2(x: 起始时间, y: 持续时间)</returns>
        public static float PositionToTime(Vector2 position, float timelineWidth, float maxTime)
        {
            float secondsPerPixel = maxTime / timelineWidth;
            return position.x * secondsPerPixel;
        }
    }
}

#endif