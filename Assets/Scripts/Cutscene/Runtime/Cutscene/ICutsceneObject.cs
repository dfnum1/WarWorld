/********************************************************************
生成日期:	06:30:2025
类    名: 	ICutsceneObject
作    者:	HappLI
描    述:	过场基础对象接口
*********************************************************************/
using System.Runtime.InteropServices;
using UnityEngine;
namespace Framework.Cutscene.Runtime
{
    //------------------------------------------------------
    //! ParamData
    //------------------------------------------------------
    public enum EParamType
	{
		ePosition,
		eEulerAngle,
        eQuraternion,
        eScale,
		eFov,
		eHold,
		ePlayAction,
        eStopAction,
        eActionSpeed,
        eBindSlotMatrix,
        eTerrainHeightCheck,
    }
    //------------------------------------------------------
    //! ParamData
    //------------------------------------------------------
    [StructLayout(LayoutKind.Explicit, Size = 16)]
	struct ParamData
	{
		[FieldOffset(0)]
		public int intVal0;
		[FieldOffset(0)]
		public float floatVal0;

		[FieldOffset(4)]
		public int intVal1;
		[FieldOffset(4)]
		public float floatVal1;

		[FieldOffset(8)]
		public int intVal2;
		[FieldOffset(8)]
		public float floatVal2;

		[FieldOffset(12)]
		public int intVal3;
		[FieldOffset(12)]
		public float floatVal3;

		[FieldOffset(0)]
		public long longValue0;

        [FieldOffset(8)]
		public long longValue1;
        //------------------------------------------------------
        public void SetBool(bool bValue)
		{
			intVal0 = bValue ? 1 : 0;
        }
        //------------------------------------------------------
        public bool GetBool()
		{
			return intVal0 != 0;
		}
        //------------------------------------------------------
        public int ToInt(int offset = 0)
        {
            switch(offset)
            {
                case 1: return intVal1;
                case 2: return intVal2;
                case 3: return intVal3;
                default: return intVal0;
            }
        }
        //------------------------------------------------------
        public long ToLong(int offset = 0)
        {
            switch (offset)
            {
                case 1: return longValue1;
                default: return intVal0;
            }
        }
        //------------------------------------------------------
        public float ToFloat(int offset = 0)
        {
            switch (offset)
            {
                case 1: return floatVal1;
                case 2: return floatVal2;
                case 3: return floatVal3;
                default: return floatVal0;
            }
        }
        //------------------------------------------------------
        public Vector2 ToVector2()
        {
            return new Vector2(floatVal0, floatVal1);
        }
        //------------------------------------------------------
        public Vector3 ToVector3()
		{
			return new Vector3(floatVal0, floatVal1, floatVal2);
		}
        //------------------------------------------------------
        public Vector4 ToVector4()
		{
			return new Vector4(floatVal0, floatVal1, floatVal2, floatVal3);
		}
        //------------------------------------------------------
        public Quaternion ToQuaternion()
		{
			return new Quaternion(floatVal0, floatVal1, floatVal2, floatVal3);
		}
        //------------------------------------------------------
        public Color ToColor()
		{
			return new Color(floatVal0, floatVal1, floatVal2, floatVal3);
		}
	}
    //------------------------------------------------------
    public struct CutsceneParam
    {
        ParamData           paramData;
        public string       strData;
        public Matrix4x4    matrixData;
        public static CutsceneParam DEF = new CutsceneParam() { };
        //------------------------------------------------------
        public void Clear()
        {
            paramData.longValue0 = 0;
            paramData.longValue1 = 0;
            strData = null;
            matrixData = Matrix4x4.identity;
        }
        //------------------------------------------------------
        public void SetAction(string action, int layer, float time)
        {
            paramData.intVal0 = layer; //动作层
            paramData.floatVal1 = time; //动作时间
            strData = action;          //动作名称
        }
        //------------------------------------------------------
        public void SetActionSpeed(string action, float speed)
        {
            paramData.floatVal0 = speed; //动作时间
            strData = action;          //动作名称
        }
        //------------------------------------------------------
        public void SetBool(bool bValue)
        {
            paramData.SetBool(bValue);
        }
        //------------------------------------------------------
        public bool GetBool()
        {
            return paramData.GetBool();
        }
        //------------------------------------------------------
        public int ToInt(int offset)
        {
            return paramData.ToInt(offset);
        }
        //------------------------------------------------------
        public void SetInt(int value)
        {
            paramData.intVal0 = value;
        }
        //------------------------------------------------------
        public long ToLong(int offset)
        {
            return paramData.ToLong(offset);
        }
        //------------------------------------------------------
        public void SetLong(long value)
        {
            paramData.longValue0 = value;
        }
        //------------------------------------------------------
        public float ToFloat(int offset =0)
        {
            return paramData.ToFloat(offset);
        }
        //------------------------------------------------------
        public void SetFloat(float value)
        {
            paramData.floatVal0 = value;
        }
        //------------------------------------------------------
        public Vector2 ToVector2()
        {
            return paramData.ToVector2();
        }
        //------------------------------------------------------
        public void SetVector2(Vector2 pos)
        {
            paramData.floatVal0 = pos.x;
            paramData.floatVal1 = pos.y;
        }
        //------------------------------------------------------
        public Vector3 ToVector3()
        {
            return paramData.ToVector3();
        }
        //------------------------------------------------------
        public void SetVector3(Vector3 pos)
        {
            paramData.floatVal0 = pos.x;
            paramData.floatVal1 = pos.y;
            paramData.floatVal2 = pos.z;
        }
        //------------------------------------------------------
        public Vector4 ToVector4()
        {
            return paramData.ToVector4();
        }
        //------------------------------------------------------
        public void SetVector4(Vector4 pos)
        {
            paramData.floatVal0 = pos.x;
            paramData.floatVal1 = pos.y;
            paramData.floatVal2 = pos.z;
            paramData.floatVal3 = pos.w;
        }
        //------------------------------------------------------
        public Quaternion ToQuaternion()
        {
            return paramData.ToQuaternion();
        }
        //------------------------------------------------------
        public void SetQuaternion(Quaternion pos)
        {
            paramData.floatVal0 = pos.x;
            paramData.floatVal1 = pos.y;
            paramData.floatVal2 = pos.z;
            paramData.floatVal3 = pos.w;
        }
        //------------------------------------------------------
        public Color ToColor()
        {
            return paramData.ToColor();
        }
        //------------------------------------------------------
        public void SetColor(Color color)
        {
            paramData.floatVal0 = color.r;
            paramData.floatVal1 = color.g;
            paramData.floatVal2 = color.b;
            paramData.floatVal3 = color.a;
        }
        //------------------------------------------------------
        public void SetString(string strData)
        {
            this.strData = strData;
        }
        //------------------------------------------------------
        public Matrix4x4 ToMatrix()
        {
            return this.matrixData;
        }
        //------------------------------------------------------
        public void SetMatrix(Matrix4x4 matrix)
        {
            this.matrixData = matrix;
        }
        //------------------------------------------------------
        public override string ToString()
        {
            return strData;
        }
    }
    //------------------------------------------------------
    //! ICutsceneObject
    //------------------------------------------------------
    public interface ICutsceneObject
    {
        UnityEngine.Object GetUniyObject();
        UnityEngine.Transform GetUniyTransform();
        UnityEngine.Animator GetAnimator();
        UnityEngine.Camera GetCamera();

        bool SetParameter(EParamType type, CutsceneParam paramData);
        bool GetParameter(EParamType type, ref CutsceneParam paramData);

        void Destroy(); //销毁对象
    }
    //------------------------------------------------------
    public static class CutsceneObjectUtil
    {
        static CutsceneParam ms_pParams = new CutsceneParam();
        public static bool PlayAction(this ICutsceneObject pObj, string action, int layer, float time)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            ms_pParams.SetAction(action, layer, time);
            return pObj.SetParameter(EParamType.ePlayAction, ms_pParams);
        }
        //------------------------------------------------------
        public static bool StopAction(this ICutsceneObject pObj, string action)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            ms_pParams.strData = action;
            return pObj.SetParameter(EParamType.eStopAction, ms_pParams);
        }
        //------------------------------------------------------
        public static bool SetActionSpeed(this ICutsceneObject pObj, string action, float speed)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            ms_pParams.SetActionSpeed(action, speed);
            return pObj.SetParameter(EParamType.eActionSpeed, ms_pParams);
        }
        //------------------------------------------------------
        public static bool SetParamTerrainHeightCheck(this ICutsceneObject pObj)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            return pObj.SetParameter(EParamType.eTerrainHeightCheck, ms_pParams);
        }
        //------------------------------------------------------
        public static bool SetParamPosition(this ICutsceneObject pObj, Vector3 position)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            ms_pParams.SetVector3(position);
            if (pObj.SetParameter(EParamType.ePosition, ms_pParams)) return true;
            if (pObj.GetUniyTransform())
            {
                pObj.GetUniyTransform().position = position;
                return true;
            }
            return false;
        }
        //------------------------------------------------------
        public static bool SetParamEulerAngle(this ICutsceneObject pObj, Vector3 eulerAnle)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            ms_pParams.SetVector3(eulerAnle);
            if (pObj.SetParameter(EParamType.eEulerAngle, ms_pParams)) return true;
            if (pObj.GetUniyTransform())
            {
                pObj.GetUniyTransform().eulerAngles = eulerAnle;
                return true;
            }
            return false;
        }
        //------------------------------------------------------
        public static bool SetParamQuaternion(this ICutsceneObject pObj, Quaternion eulerAnle)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            ms_pParams.SetQuaternion(eulerAnle);
            if (pObj.SetParameter(EParamType.eQuraternion, ms_pParams)) return true;
            if (pObj.GetUniyTransform())
            {
                pObj.GetUniyTransform().rotation = eulerAnle;
                return true;
            }
            return false;
        }
        //------------------------------------------------------
        public static bool SetParamScale(this ICutsceneObject pObj, Vector3 scale)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            ms_pParams.SetVector3(scale);
            if (pObj.SetParameter(EParamType.eScale, ms_pParams)) return true;
            if(pObj.GetUniyTransform())
            {
                pObj.GetUniyTransform().localScale = scale;
                return true;
            }
            return false;
        }
        //------------------------------------------------------
        public static bool SetParamFov(this ICutsceneObject pObj, float fov)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            ms_pParams.SetFloat(fov);
            if (pObj.SetParameter(EParamType.eFov, ms_pParams)) return true;
            if(pObj.GetCamera())
            {
                pObj.GetCamera().fieldOfView = fov;
                return true;
            }
            return false;
        }
        //------------------------------------------------------
        public static bool SetParamHold(this ICutsceneObject pObj, bool bHold)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            ms_pParams.SetBool(bHold);
            return pObj.SetParameter(EParamType.eHold, ms_pParams);
        }
        //------------------------------------------------------
        public static bool GetParamBindSlotMatrix(this ICutsceneObject pObj, string bindSlot, out Matrix4x4 matrix)
        {
            matrix = Matrix4x4.identity;
            if (pObj == null) return false;
            ms_pParams.Clear();
            ms_pParams.SetString(bindSlot);
            if (pObj.GetParameter(EParamType.eBindSlotMatrix, ref ms_pParams))
            {
                matrix = ms_pParams.ToMatrix();
                return true;
            }
            return false;
        }
        //------------------------------------------------------
        public static bool GetParamPosition(this ICutsceneObject pObj, ref Vector3 position)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            if(pObj.GetParameter(EParamType.ePosition, ref ms_pParams))
            {
                position = ms_pParams.ToVector3();
                return true;
            }
            else if (pObj.GetUniyTransform())
            {
                position = pObj.GetUniyTransform().position;
                return true;
            }
            return false;
        }
        //------------------------------------------------------
        public static bool GetParamEulerAngle(this ICutsceneObject pObj, ref Vector3 eulerAnle)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            if (pObj.GetParameter(EParamType.eEulerAngle, ref ms_pParams))
            {
                eulerAnle = ms_pParams.ToVector3();
                return true;
            }
            else if (pObj.GetUniyTransform())
            {
                eulerAnle = pObj.GetUniyTransform().eulerAngles;
                return true;
            }
            return false;
        }
        //------------------------------------------------------
        public static bool GetParamQuaternion(this ICutsceneObject pObj, ref Quaternion eulerAnle)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            if (pObj.GetParameter(EParamType.eQuraternion, ref ms_pParams))
            {
                eulerAnle = ms_pParams.ToQuaternion();
                return true;
            }
            else if(pObj.GetUniyTransform())
            {
                eulerAnle = pObj.GetUniyTransform().rotation;
                return true;
            }
            return false;
        }
        //------------------------------------------------------
        public static bool GetParamScale(this ICutsceneObject pObj, ref Vector3 scale)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            if (pObj.GetParameter(EParamType.eScale, ref ms_pParams))
            {
                scale = ms_pParams.ToVector3();
                return true;
            }
            else if (pObj.GetUniyTransform())
            {
                scale = pObj.GetUniyTransform().localScale;
                return true;
            }
            return false;
        }
        //------------------------------------------------------
        public static bool GetParamFov(this ICutsceneObject pObj, ref float fov)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            if (pObj.GetParameter(EParamType.eFov, ref ms_pParams))
            {
                fov = ms_pParams.ToFloat();
                return true;
            }
            else if (pObj.GetCamera())
            {
                fov = pObj.GetCamera().fieldOfView;
                return true;
            }
            return false;
        }
        //------------------------------------------------------
        public static bool GetParamIsHold(this ICutsceneObject pObj, ref bool bHoled)
        {
            if (pObj == null) return false;
            ms_pParams.Clear();
            if (pObj.GetParameter(EParamType.eHold, ref ms_pParams))
            {
                bHoled = ms_pParams.GetBool();
                return true;
            }
            return false;
        }
    }
}