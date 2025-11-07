/********************************************************************
生成日期:	06:30:2025
类    名: 	IDraw
作    者:	HappLI
描    述:	绘制类接口
*********************************************************************/
#if UNITY_EDITOR
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    public interface IDraw
    {
        bool IsDragging();
        float GetBegin();
        float GetEnd();
        void Draw(Rect rect, TimelineDrawLogic statLogic);
        void DragEnd();
        bool DragOffset(float offsetTime, bool bUsed = true, bool bUndo = true, bool bDuration = false);
        void OnEvent(Event evt, TimelineDrawLogic stateLogic, bool bFroceDrag = false);
        bool CanSelectDurationSnap(Event evt, TimelineDrawLogic stateLogic);
        TimelineDrawLogic.TimelineTrack GetOwnerTrack();
        bool CanSelect(Event evt, TimelineDrawLogic stateLogic);

        public Runtime.IDataer GetData();
        public void SetData(Runtime.IDataer dater);

        void OnDelete();
        ACutsceneCustomEditor GetCustomEditor();
    }
}
#endif