/********************************************************************
生成日期:	06:30:2025
类    名: 	FrameData
作    者:	HappLI
描    述:	帧数据
*********************************************************************/
namespace Framework.Cutscene.Runtime
{
    public struct FrameData
    {
		public CutsceneInstance cutscene;
		public CutsceneTrack ownerTrack;
		public IBaseClip clip;
		public int frameRate;
		public float curTime;
		public float totalDuration; //总持续时间
        public float deltaTime;
		public EPlayableStatus eStatus;
		public EDriverStatus clipStatus;

        public bool isBlending;
        public float blendFactor;//0-1
		public float blendTime; //混合时间
        public float subTime;
		public int ToFrame()
		{
			return UnityEngine.Mathf.FloorToInt(curTime * frameRate);
        }
		public bool IsValid()
		{
			return ownerTrack != null && clip!=null;
		}
		public bool IsLeaveIn()
		{
			if (clip == null || curTime < clip.GetTime())
				return true;
			return false;
		}
		public bool CanRestore()
		{
			if (clip == null)
				return true;
			return clip.GetEndEdgeType() != EClipEdgeType.KeepClamp && clip.GetEndEdgeType() != EClipEdgeType.KeepState;
        }
		public bool IsOvered()
		{
			return curTime >= totalDuration;
        }
        public static FrameData DEF = new FrameData() { ownerTrack = null, clip = null };
    }
}