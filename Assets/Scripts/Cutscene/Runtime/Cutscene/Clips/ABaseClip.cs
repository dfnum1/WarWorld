/********************************************************************
生成日期:	06:30:2025
类    名: 	ABaseClip
作    者:	HappLI
描    述:	基础剪辑clip,与ABaseEvent 的区别在于，Clip拥有持续时长
*********************************************************************/
using Framework.DrawProps;
namespace Framework.Cutscene.Runtime
{
	public enum EClipType : ushort
	{
		eCustom = 1,
		eCameraMove = 2, //相机移动剪辑
		eAnimatorAction = 3, //Animator动作剪辑
        eTransMovePath = 4, //对象路径移动剪辑
        eCameraPath = 5, //相机路径剪辑
        eParticle = 6, //特效剪辑
        eActive = 7, //激活剪辑
        eTansCurvePath = 8, //曲线运动动画剪辑
		eTimeScale = 9, //时间缩放剪辑
        eCameraShake = 10, //镜头抖动
        eCameraLerpToGame = 11, //相机插值到游戏视角
        eFollowTo = 12, //跟随目标
        eInstancePrefab = 13, //实例化预制体剪辑
        eFollowTargetGroup = 14, //跟随目标

        eSetPosition = 15, //设置位置
        eSetRotation = 16, //设置转向
        eSetScale = 17, //设置缩放
        eAnimation = 18, //Animation剪辑

        eMaterial = 19, //材质属性剪辑
        eProjecitle = 20, //弹道剪辑

        eCutsomBegin = 1000,
	}
    //-----------------------------------------------------
    public enum EClipEdgeType
	{
		[Display("无")]None = 0,
		[Display("保持")]KeepClamp,
        [Display("保持状态")] KeepState,
        [Display("重复")]Repeat,
	}
    //-----------------------------------------------------
    public interface IBaseClip : IDataer
    {
		float GetDuration();
		EClipEdgeType GetEndEdgeType();
		ushort GetRepeatCount();
		float GetBlend(bool bIn);
    }
	//-----------------------------------------------------
	[System.Serializable]
	public struct BaseClipProp
	{
        [DefaultValue("")] public string name; //剪辑名称
        [DefaultValue(0), UnEdit, Display("开始时间")] public float time; //开始时间
        [DefaultValue(5), UnEdit, Display("持续时间")] public float duration; //持续时间
        [DefaultValue(EClipEdgeType.None), Display("结束类型")] public EClipEdgeType endEdgeType; //结束边缘类型
        [DefaultValue(1), StateByField("endEdgeType", "Repeat"), Display("重复次数")] public ushort repeatCnt; //重复次数   
        [DefaultValue(0), UnEdit] public float blendIn;
        [DefaultValue(0), UnEdit] public float blendOut;

        public float GetBlend(bool bIn)
        {
            return bIn?this.blendIn:this.blendOut;
        }

#if UNITY_EDITOR
        [System.NonSerialized] public System.Object ownerObject;
        [System.NonSerialized] public CutsceneTrack ownerTrackObject;
#endif
    }
  //  public abstract class ABaseClip
  //  {
		//public float time;
		//public float duration;
		//public EClipEdgeType endEdgeType;
		//public ushort repeatCnt = 1;
  //      public string name;
		//public abstract int GetClipType();

		//public T Cast<T>() where T : ABaseClip
		//{
		//	return this as T;
  //      }
  //      public bool IsCast<T>() where T : ABaseClip
  //      {
  //          return this is T;
  //      }
  //  }
}