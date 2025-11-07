/********************************************************************
生成日期:	06:30:2025
类    名: 	EActionType
作    者:	HappLI
描    述:	内置的行为类型
*********************************************************************/
namespace Framework.AT.Runtime
{
    //-----------------------------------------------------
    [ATType("任务")]
    internal enum ETaskType
    {
        [ATAction("开始", true, false, true), ATIcon("at_enter_start")]
        eStart = 1,//任务开始

        [ATAction("Tick", true, false, true), ATIcon("at_enter_tick")]
        eTick = 2,

        [ATAction("退出", true, false, true), ATIcon("at_enter_exit")]
        eExit = 3,

        [ATAction("过场动画/开始播放回调", true, false, true)]
        [Return("实例id", typeof(int))]
        [Return("配置id", typeof(int))]
        eCutscenePlayablePlayedCallback = 50,

        [ATAction("过场动画/停止播放回调", true, false, true)]
        [Return("实例id", typeof(int))]
        [Return("配置id", typeof(int))]
        eCutscenePlayableStopedCallback,

        [ATAction("过场动画/暂停播放回调", true, false, true)]
        [Return("实例id", typeof(int))]
        [Return("配置id", typeof(int))]
        eCutscenePlayablePauseCallback,

        [ATAction("过场动画/继续播放回调", true, false, true)]
        [Return("实例id", typeof(int))]
        [Return("配置id", typeof(int))]
        eCutscenePlayableResumeCallback,

        eTaskEndId = 100,//任务开始
    }
    //-----------------------------------------------------
    [ATType("常规")]
    public enum EActionType
    {
        [DrawProps.Disable]eActionBegin = 101,

        [ATAction("变量获取", false, false, false, false)]
        [Return("GUID",typeof(int))]
        eGetVariable = eActionBegin+1,//获取变量

        [ATAction("条件判断")]
        [Argv("参数1", typeof(IVariable), true)]
        [Argv("符号", typeof(ECompareOpType), true)]
        [Argv("参数2", typeof(IVariable), true)]
        [Link("不成立", false)]
        eCondition ,//条件

        [ATAction("运算")]
        [Argv("参数1", typeof(IVariable), true)]
        [Argv("符号", typeof(EOpType), true, null)]
        [Argv("参数2", typeof(IVariable), true)]
        [Return("结果", typeof(IVariable))]
        eOpVariable,

        [ATAction("Dot")]
        [Argv("参数1", typeof(IVariable), true, null, EVariableType.eVec2, EVariableType.eVec3)]
        [Argv("参数2", typeof(IVariable), true, null, EVariableType.eVec2, EVariableType.eVec3)]
        [Return("结果", typeof(float))]
        eDotVariable,

        [ATAction("Cross")]
        [Argv("参数1", typeof(IVariable), true,null,EVariableType.eVec2, EVariableType.eVec3)]
        [Argv("参数2", typeof(IVariable), true, null, EVariableType.eVec2, EVariableType.eVec3)]
        [Return("结果", typeof(IVariable))]
        eCrossVariable,

        [ATAction("坐标距离")]
        [Argv("参数1", typeof(IVariable), true, null,EVariableType.eVec2, EVariableType.eVec3)]
        [Argv("参数2", typeof(IVariable), true, null,EVariableType.eVec2, EVariableType.eVec3)]
        [Return("结果", typeof(float))]
        eDistanceVariable,

        [ATAction("过渡Lerp")]
        [Argv("参数1", typeof(IVariable), true,null,EVariableType.eFloat, EVariableType.eVec2, EVariableType.eVec3)]
        [Argv("参数2", typeof(IVariable), true,null,EVariableType.eFloat, EVariableType.eVec2, EVariableType.eVec3)]
        [Argv("速度", typeof(float), true)]
        [Return("结果", typeof(IVariable))]
        eLerp,

        [ATAction("新建变量", false, false, false)]
        [Argv("变量", typeof(IVariable), true)]
        [Return("输出", typeof(IVariable))]
        eNewVariable,//新建变量

        [ATAction("过场动画/播放")]
        [Argv("过场id", typeof(ushort), true)]
        [Return("实例Id", typeof(int))]
        ePlaySubCutscene = eActionBegin + 100, //播放动画

        [ATAction("过场动画/暂停")]
        [Argv("实例Id","当为0时，表示暂停所有当前cutscene正在播放的过场", typeof(int), true)]
        ePauseSubCutscene,

        [ATAction("过场动画/继续播放")]
        [Argv("实例Id", "当为0时，表示继续所有当前cutscene正在播放的过场", typeof(int), true)]
        eResumeSubCutscene,

        [ATAction("过场动画/停止")]
        [Argv("实例Id", "当为0时，表示停止所有当前cutscene正在播放的过场", typeof(int), true)]
        eStopSubCutscene,

        [ATAction("过场动画/跳到指定位置开始播")]
        [Argv("实例Id", "当为0时，表示操作所有当前cutscene正在播放的过场", typeof(int), true)]
        [Argv("播放位置", "", typeof(float), true)]
        eSeekSubCutscene,

        [ATAction("过场动画/轨道数据绑定")]
        [Argv("实例Id", "", typeof(int), true)]
        [Argv("轨道", "", typeof(int), true)]
        [Argv("数据", "", typeof(IVariable), true)]
        eBindCutsceneTrackData,

        [DrawProps.Disable]
        eCutsceneCustomEvent = 999,//Cutscene自定义事件

        [DrawProps.Disable]
        eCustomBegin = 1000,//自定义事件开始
    }
}