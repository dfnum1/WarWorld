//auto generator
namespace Framework.Cutscene.Runtime
{
	public class CutsceneUtil
	{
		public static Framework.Cutscene.Runtime.IDataer CreateDataer(Framework.Cutscene.Runtime.EDataType type, ushort typeId)
		{
		    switch(type)
		    {
		        case Framework.Cutscene.Runtime.EDataType.eClip:
		            switch(typeId)
		            {
		                case 7: return new Framework.Cutscene.Runtime.ActiveClip();
		                case 3: return new Framework.Cutscene.Runtime.AnimatorActionClip();
		                case 11: return new Framework.Cutscene.Runtime.CameraLerpGameClip();
		                case 2: return new Framework.Cutscene.Runtime.CameraMoveClip();
		                case 5: return new Framework.Cutscene.Runtime.CameraPathClip();
		                case 10: return new Framework.Cutscene.Runtime.CameraShakeClip();
		                case 18: return new Framework.Cutscene.Runtime.CutsceneAnimationClip();
		                case 1: return new Framework.Cutscene.Runtime.CutsceneCustomClip();
		                case 14: return new Framework.Cutscene.Runtime.CutsceneGroupBindObjectFollowTargetClip();
		                case 13: return new Framework.Cutscene.Runtime.CutsceneInstancePrefabClip();
		                case 12: return new Framework.Cutscene.Runtime.FollowToClip();
		                case 19: return new Framework.Cutscene.Runtime.MaterialClip();
		                case 6: return new Framework.Cutscene.Runtime.ParticleClip();
		                case 15: return new Framework.Cutscene.Runtime.PositionClip();
		                case 20: return new Framework.Cutscene.Runtime.ProjecitleClip();
		                case 16: return new Framework.Cutscene.Runtime.RotationClip();
		                case 17: return new Framework.Cutscene.Runtime.ScaleClip();
		                case 9: return new Framework.Cutscene.Runtime.TimeScaleClip();
		                case 8: return new Framework.Cutscene.Runtime.TransformCurvePathClip();
		                case 4: return new Framework.Cutscene.Runtime.TransformMovePathClip();
		                default: return null;
		            }
		        case Framework.Cutscene.Runtime.EDataType.eEvent:
		            switch(typeId)
		            {
		                case 1: return new Framework.Cutscene.Runtime.CutsceneCustomEvent();
		                case 3: return new Framework.Cutscene.Runtime.CutsceneExecuteAT();
		                case 7: return new Framework.Cutscene.Runtime.CutscenePauseCutsceneEvent();
		                case 5: return new Framework.Cutscene.Runtime.CutscenePlayCutsceneEvent();
		                case 8: return new Framework.Cutscene.Runtime.CutsceneResumeCutsceneEvent();
		                case 2: return new Framework.Cutscene.Runtime.CutsceneSetATPort();
		                case 4: return new Framework.Cutscene.Runtime.CutsceneSetCamera();
		                case 6: return new Framework.Cutscene.Runtime.CutsceneStopCutsceneEvent();
		                default: return null;
		            }
		        default: return null;
		    }
		}
		//-----------------------------------------------------
		public static Framework.Cutscene.Runtime.ACutsceneDriver CreateDriver(long key)
		{
		    switch(key)
		    {
		        default: return null;
		    }
		}
	}
}
