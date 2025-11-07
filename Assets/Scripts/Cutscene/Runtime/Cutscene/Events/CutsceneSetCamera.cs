/********************************************************************
生成日期:	06:30:2025
类    名: 	CutsceneSetCamera
作    者:	HappLI
描    述:	设置相机视角
*********************************************************************/
using Framework.AT.Runtime;
using Framework.DrawProps;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Framework.Cutscene.Runtime
{
    [System.Serializable, CutsceneEvent("设置相机视角")]
    public class CutsceneSetCamera : IBaseEvent
    {
        [Display("基本属性")]public BaseEventProp baseProp;

        [Display("位置")] public Vector3 position;
        [Display("角度")] public Vector3 eulerAngle;
        [Display("广角")] public float fFOV = 60;
        //-----------------------------------------------------
        public ACutsceneDriver CreateDriver()
        {
            return new CutsceneSetCameraDriver();
        }
        //-----------------------------------------------------
        public ushort GetIdType()
        {
            return (ushort)EEventType.eSetGameCamera;
        }
        //-----------------------------------------------------
        public string GetName()
        {
            return baseProp.name;
        }
        //-----------------------------------------------------
        public float GetTime()
        {
            return baseProp.time;
        }
#if UNITY_EDITOR
        //-----------------------------------------------------
        [AddInspector]
        public void OnCameraEditor()
        {
            this.fFOV = UnityEditor.EditorGUILayout.Slider("广角", fFOV, 10.0f, 179.0f);
            if (GUILayout.Button("设置当前相机参数"))
            {
                var mainCamera = Camera.main;
                if (mainCamera)
                {
                    position = mainCamera.transform.position;
                    eulerAngle = mainCamera.transform.eulerAngles;
                    fFOV = mainCamera.fieldOfView;
                }
            }
        }
#endif
    }
    //-----------------------------------------------------
    public class CutsceneSetCameraDriver : ACutsceneDriver
    {
        public override bool OnEventTrigger(CutsceneTrack pTrack, IBaseEvent pEvt)
        {
            if (pEvt is CutsceneSetCamera setCameraEvent)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    mainCamera.transform.position = setCameraEvent.position;
                    mainCamera.transform.eulerAngles = setCameraEvent.eulerAngle;
                    mainCamera.fieldOfView = setCameraEvent.fFOV;

                    var drivers = pTrack.GetCacheTrackDriversByType(typeof(CameraLerpGameDriver));
                    if(drivers!=null)
                    {
                        foreach(var db in drivers)
                        {
                            if (db.pDriver is CameraLerpGameDriver cameraDriver)
                            {
                                cameraDriver.SyncGameCameraData(db.pTrack);
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}