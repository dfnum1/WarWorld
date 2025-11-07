/********************************************************************
生成日期:	06:30:2025
类    名: 	ControllerRefUtil
作    者:	HappLI
描    述:	控制引用计数工具类
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Editor;
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
namespace Framework.Cutscene.Runtime
{
    //-----------------------------------------------------
    public class ControllerRefUtil
    {
        static Dictionary<int, int> ms_vControllRefs = null;
        //-----------------------------------------------------
        public static void Controll(Component camera)
        {
            if (camera == null)
                return;
            if (ms_vControllRefs == null)
            {
                ms_vControllRefs = new Dictionary<int, int>(2);
            }
            int nID = camera.GetInstanceID();
            int refIndex = 0;
            if (!ms_vControllRefs.TryGetValue(nID, out refIndex))
            {
                refIndex = 0;
            }
            refIndex++;
            ms_vControllRefs[nID] = refIndex;
        }
        //-----------------------------------------------------
        public static void UnControll(Component camera)
        {
            if (camera == null)
                return;
            if (ms_vControllRefs == null)
            {
                return;
            }
            int nID = camera.GetInstanceID();
            int refIndex = 0;
            if (!ms_vControllRefs.TryGetValue(nID, out refIndex))
            {
                return;
            }
            refIndex--;
            if (refIndex <= 0)
            {
                ms_vControllRefs.Remove(nID);
            }
            else
            {
                ms_vControllRefs[nID] = refIndex;
            }
        }
        //-----------------------------------------------------
        public static int ControllRef(Component camera)
        {
            if (camera == null)
                return 0;
            if (ms_vControllRefs == null)
            {
                return 0;
            }
            int nID = camera.GetInstanceID();
            int refIndex = 0;
            if (!ms_vControllRefs.TryGetValue(nID, out refIndex))
            {
                return refIndex;
            }
            return 0;
        }
        //-----------------------------------------------------
        public static bool IsControlling(Component camera)
        {
            if (camera == null)
                return false;
            if (ms_vControllRefs == null)
            {
                return false;
            }
            int nID = camera.GetInstanceID();
            int refIndex = 0;
            if (!ms_vControllRefs.TryGetValue(nID, out refIndex))
            {
                return false;
            }
            return refIndex > 0;
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        public static bool IsEditorObject(UnityEngine.Object pObject)
        {
            if (pObject == null)
                return false; 

            if (CameraPathEditor.IsEditing() && pObject is Camera)
            {
                if (Selection.activeObject == ((Camera)pObject).gameObject)
                    return true;
            }
            if (ms_CameraEditing != null && ms_CameraEditing == pObject)
                return true;
            Transform pTransform = null;
            if (pObject is GameObject)
            {
                pTransform = ((GameObject)pObject).transform;
            }
            else if (pObject is Transform)
            {
                pTransform = ((Transform)pObject);
            }
            if (pTransform == null)
                return false;
            if (CurvePathEditor.IsEditing(pTransform) && pTransform == Selection.activeTransform)
                return true;
            return false;
        }
        //-----------------------------------------------------
        public static bool IsEditorObject(ICutsceneObject pObject)
        {
            if (pObject == null)
                return false;

            if(ms_pEditingObject == pObject)
                return true;

            if (CameraPathEditor.IsEditing() && pObject.GetCamera())
            {
                if(Selection.activeObject == pObject.GetCamera().gameObject)
                    return true;
            }
            if (ms_CameraEditing != null && ms_CameraEditing == pObject.GetCamera())
                return true;
            if (CurvePathEditor.IsEditing(pObject))
                return true;
            var pObj = pObject.GetUniyTransform();
            if(pObj == null)
            {
                if(pObject.GetUniyObject() !=null && pObject.GetUniyObject() is GameObject)
                {
                    pObj =(pObject.GetUniyObject() as GameObject).transform;
                }
            }
            if (pObj == null)
                return false;
            if (CurvePathEditor.IsEditing(pObj) && pObj == Selection.activeTransform)
                return true;
            return false;
        }
        //-----------------------------------------------------
        private static System.Object ms_pEditingObject = null;
        internal static void SetEditingObject(System.Object pObject)
        {
            ms_pEditingObject = pObject;
        }
        //-----------------------------------------------------
        private static Camera ms_CameraEditing = null;
        public static void SetEditingCamea(Camera pObject)
        {
            ms_CameraEditing = pObject;
        }
        //-----------------------------------------------------
        public static bool CanSetCameraEditMode()
        {
            return ms_pEditCameraModeFunc != null;
        }
        //-----------------------------------------------------
        public static bool IsEdingCamera()
        {
            return ms_CameraEditing;
        }
        //-----------------------------------------------------
        public static void SetEditCameraMode(bool bEdit)
        {
            SetEditingCamea(null);
            if (ms_pEditCameraModeFunc == null) return;
            ms_pEditCameraModeFunc(bEdit);
        }
        //-----------------------------------------------------
        static System.Action<bool> ms_pEditCameraModeFunc = null;
        public static void SetEditCameraModeFunc(System.Action<bool> setEditModeFunc)
        {
            ms_pEditCameraModeFunc = setEditModeFunc;
        }
#endif
    }
}