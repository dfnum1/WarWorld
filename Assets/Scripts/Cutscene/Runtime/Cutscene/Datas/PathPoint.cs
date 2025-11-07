/********************************************************************
生成日期:	23:07:2025
类    名: 	PathPoint
作    者:	HappLI
描    述:	路径点数据结构
*********************************************************************/
using Framework.DrawProps;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable]
    public struct PathPoint
    {
        [Display("位置")] public Vector3 position;
        [Display("角度")] public Vector3 eulerAngle;
        [Display("缩放")] public Vector3 scale;
        [Display("入切角")] public Vector3 inTan;
        [Display("出切角")] public Vector3 outTan;
    }
    //-----------------------------------------------------
    //! PathCurve
    //-----------------------------------------------------
    public struct PathCurve : System.IDisposable
    {
        public struct Point
        {
            public Vector3 position;
            public Quaternion rot;
            public Vector3 scale;
            public bool useRot;
            public static Point DEF = new Point() { useRot = true, position = Vector3.zero, rot = Quaternion.identity, scale = Vector3.one };
        }
        PathPoint[] m_vPoints;
        private List<float> m_SegmentLengths;
        private float m_TotalLength;
        private const int SampleCount = 30;
        public void Set(PathPoint[] vPoints)
        {
            m_vPoints = vPoints;
            m_SegmentLengths = null;
            m_TotalLength = 0;

            if(vPoints!=null && vPoints.Length>2)
                CalculateCurveLengths(vPoints);
        }
        //-----------------------------------------------------
        public bool Evaluate(float time, float duration, out Point point, bool bPathFoward = false)
        {
            return Evaluate(time/ Mathf.Max(duration, 0.01f), out point, bPathFoward);
        }
        //-----------------------------------------------------
        public bool Evaluate(float normalTime, out Point point, bool bPathFoward = false)
        {
            point = Point.DEF;
            normalTime = Mathf.Clamp01(normalTime);
            if (m_vPoints == null) return false;

            if (m_vPoints.Length == 1)
            {
                var p0 = m_vPoints[0];
                point.position = p0.position;
                point.scale = p0.scale;
                point.rot = Quaternion.Euler(p0.eulerAngle);
            }
            else if(m_vPoints.Length ==2)
            {
                var p0 = m_vPoints[0];
                var p1 = m_vPoints[1];

                Vector3 start = p0.position;
                Vector3 end = p1.position;
                Vector3 tan0 = p0.position + p0.outTan;
                Vector3 tan1 = p1.position + p1.inTan;

                point.useRot = true;
                // 贝塞尔插值
                point.position = Bezier(start, tan0, tan1, end, normalTime);
                point.scale = Vector3.Lerp(p0.scale, p1.scale, normalTime);
                point.rot = Quaternion.identity;
                if (bPathFoward)
                {
                    Vector3 nextPos = Bezier(start, tan0, tan1, end, Mathf.Min(normalTime + 0.01f, 1f));
                    if ((nextPos - point.position).sqrMagnitude > 0.0f)
                        point.rot = Quaternion.LookRotation(nextPos - point.position, Vector3.up);
                    else point.useRot = false;
                }
                else
                    point.rot = Quaternion.Lerp(Quaternion.Euler(p0.eulerAngle), Quaternion.Euler(p1.eulerAngle), normalTime);
            }
            else
            {
                float targetLen = normalTime * m_TotalLength;

                int segIdx = 0;
                float localT = 0;
                if (normalTime >= 0.98f && m_vPoints.Length > 0)
                {
                    segIdx = m_vPoints.Length - 1;
                    var cur = m_vPoints[segIdx];
                    point.useRot = true;
                    point.position = cur.position;
                    point.scale = cur.scale;
                    point.rot = Quaternion.identity;
                    point.rot = Quaternion.Euler(cur.eulerAngle);
                    return true;
                }
                else
                {
                    GetUniformT(m_vPoints, targetLen, out segIdx, out localT);
                    if (segIdx < 0)
                        return true;
                }


                var p0 = m_vPoints[segIdx];
                var p1 = m_vPoints[segIdx + 1];

                Vector3 start = p0.position;
                Vector3 end = p1.position;
                Vector3 tan0 = p0.position + p0.outTan;
                Vector3 tan1 = p1.position + p1.inTan;

                point.useRot = true;
                // 贝塞尔插值
                point.position = Bezier(start, tan0, tan1, end, localT);
                point.scale = Vector3.Lerp(p0.scale, p1.scale, localT);
                point.rot = Quaternion.identity;
                if (bPathFoward)
                {
                    Vector3 nextPos = Bezier(start, tan0, tan1, end, Mathf.Min(localT + 0.01f, 1f));
                    if ((nextPos - point.position).sqrMagnitude > 0.0f)
                        point.rot = Quaternion.LookRotation(nextPos - point.position, Vector3.up);
                    else point.useRot = false;
                }
                else
                    point.rot = Quaternion.Lerp(Quaternion.Euler(p0.eulerAngle), Quaternion.Euler(p1.eulerAngle), localT);
            }

            return true;
        }
        //-----------------------------------------------------
        private void CalculateCurveLengths(PathPoint[] points)
        {
            int segCount = points.Length - 1;
            if (m_SegmentLengths == null)
            {
                m_SegmentLengths = UnityEngine.Pool.ListPool<float>.Get();
            }
            m_SegmentLengths.Clear();
            m_TotalLength = 0f;

            const float unitLen = 0.05f; // 每隔0.1米采样一次，可根据实际需求调整
            const int minSample = 20;   // 每段最少采样数
            const int maxSample = 200;  // 每段最多采样数

            for (int i = 0; i < segCount; ++i)
            {
                // 先粗略采样估算长度
                float roughLen = 0f;
                Vector3 prev = points[i].position;
                for (int j = 1; j <= 10; ++j)
                {
                    float t = j / 10f;
                    Vector3 p = Bezier(
                        points[i].position,
                        points[i].position + points[i].outTan,
                        points[i + 1].position + points[i + 1].inTan,
                        points[i + 1].position,
                        t
                    );
                    roughLen += Vector3.Distance(prev, p);
                    prev = p;
                }
                // 根据长度自适应采样数
                int sampleCount = Mathf.Clamp(Mathf.CeilToInt(roughLen / unitLen), minSample, maxSample);

                float segLen = 0f;
                prev = points[i].position;
                for (int j = 1; j <= sampleCount; ++j)
                {
                    float t = j / (float)sampleCount;
                    Vector3 p = Bezier(
                        points[i].position,
                        points[i].position + points[i].outTan,
                        points[i + 1].position + points[i + 1].inTan,
                        points[i + 1].position,
                        t
                    );
                    segLen += Vector3.Distance(prev, p);
                    prev = p;
                }
                m_SegmentLengths.Add(segLen);
                m_TotalLength += segLen;
            }
        }
        //-----------------------------------------------------
        private void GetUniformT(PathPoint[] points, float distance, out int segIdx, out float localT)
        {
            segIdx = -1;
            localT = 0;
            if (m_SegmentLengths == null)
                return;
            float accum = 0f;
            segIdx = 0;
            for (; segIdx < m_SegmentLengths.Count; ++segIdx)
            {
                if (accum + m_SegmentLengths[segIdx] >= distance)
                    break;
                accum += m_SegmentLengths[segIdx];
            }
            segIdx = Mathf.Clamp(segIdx, 0, m_SegmentLengths.Count - 1);
            float segLen = m_SegmentLengths[segIdx];
            float segDist = distance - accum;
            // 反查t，采用等分采样+线性插值
            float prevLen = 0f;
            Vector3 prev = points[segIdx].position;
            localT = 0f;
            for (int j = 1; j <= SampleCount; ++j)
            {
                float t = j / (float)SampleCount;
                Vector3 p = Bezier(
                    points[segIdx].position,
                    points[segIdx].position + points[segIdx].outTan,
                    points[segIdx + 1].position + points[segIdx + 1].inTan,
                    points[segIdx + 1].position,
                    t
                );
                float d = Vector3.Distance(prev, p);
                if (prevLen + d >= segDist)
                {
                    float remain = segDist - prevLen;
                    float lerp = d > 0f ? remain / d : 0f;
                    localT = ((j - 1) + lerp) / SampleCount;
                    return;
                }
                prevLen += d;
                prev = p;
            }
            localT = 1f;
        }
        //-----------------------------------------------------
        private Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            return u * u * u * p0 +
                   3 * u * u * t * p1 +
                   3 * u * t * t * p2 +
                   t * t * t * p3;
        }
        //-----------------------------------------------------
        public void Dispose()
        {
            if (m_SegmentLengths != null)
            {
                UnityEngine.Pool.ListPool<float>.Release(m_SegmentLengths);
                m_SegmentLengths = null;
            }
        }
    }
}