/********************************************************************
生成日期:	06:30:2025
类    名: 	LockUtil
作    者:	HappLI
描    述:	数据锁定工具类
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    public static class LockUtil
    {
        class Lock
        {
            public int refCnt;
            public Dictionary<string, object> vDatas = new Dictionary<string, object>();
        }
        static Dictionary<object, Lock> ms_vLocks = new Dictionary<object, Lock>();
        public static void BackupCamera(this Camera camera, bool bReBackup = false)
        {
            if (camera == null)
                return;
            if (!ms_vLocks.TryGetValue(camera, out var locks))
            {
                locks = new Lock();
                ms_vLocks[camera] = locks;
            }
            else
            {
                if(!bReBackup)
                {
                    locks.refCnt++;
                    return; // 已经备份过了
                }
            }
            locks.refCnt++;
            locks.vDatas["pos"] = camera.transform.position;
          //  locks.vDatas["euler"] = camera.transform.eulerAngles;
            locks.vDatas["rotation"] = camera.transform.rotation;
            locks.vDatas["fov"] = camera.fieldOfView;
        }
        //-----------------------------------------------------
        public static void RestoreCamera(this Camera camera)
        {
            if (camera == null)
                return;
            if (ms_vLocks.TryGetValue(camera, out var locks))
            {
                locks.refCnt--;
                if (locks.vDatas.TryGetValue("pos", out var pos))
                {
                    camera.transform.position = (Vector3)pos;
                }
                if (locks.vDatas.TryGetValue("euler", out var euler))
                {
                    camera.transform.eulerAngles = (Vector3)euler;
                }
                if (locks.vDatas.TryGetValue("fov", out var fov))
                {
                    camera.fieldOfView = (float)fov;
                }
                if (locks.vDatas.TryGetValue("rotation", out var rotation))
                {
                    camera.transform.rotation = (Quaternion)rotation;
                }
                if (locks.refCnt<=0)
                {
                    ms_vLocks.Remove(camera);
                }
            }
        }
        //-----------------------------------------------------
        public static void RestoreGameObject(this GameObject pGo)
        {
            if (pGo == null)
                return;
            if (ms_vLocks.TryGetValue(pGo, out var locks))
            {
                if (locks.vDatas.TryGetValue("pos", out var pos))
                {
                    pGo.transform.position = (Vector3)pos;
                }
                if (locks.vDatas.TryGetValue("euler", out var euler))
                {
                    pGo.transform.eulerAngles = (Vector3)euler;
                }
                if (locks.vDatas.TryGetValue("rotation", out var rotation))
                {
                    pGo.transform.rotation = (Quaternion)rotation;
                }
                if (locks.vDatas.TryGetValue("scale", out var scale))
                {
                    pGo.transform.localScale = (Vector3)scale;
                }
                if (locks.vDatas.TryGetValue("active", out var active))
                {
                    pGo.gameObject.SetActive((bool)active);
                }
                locks.refCnt--;
                if (locks.refCnt <= 0)
                {
                    ms_vLocks.Remove(pGo);
                }
            }
        }
        //-----------------------------------------------------
        public static void BackupGameObject(this GameObject pGo, bool bReBackup = false)
        {
            if (pGo == null)
                return;
            if (!ms_vLocks.TryGetValue(pGo, out var locks))
            {
                locks = new Lock();
                ms_vLocks[pGo] = locks;
            }
            else
            {
                if(!bReBackup)
                {
                    locks.refCnt++;
                    return;
                }
            }
            locks.refCnt++;
            locks.vDatas["pos"] = pGo.transform.position;
            locks.vDatas["rotation"] = pGo.transform.rotation;
            locks.vDatas["scale"] = pGo.transform.localScale;
            locks.vDatas["active"] = pGo.activeSelf;
        }
        //-----------------------------------------------------
        public static void Restore(this Material pGo, bool bReBackup = false)
        {
            if (pGo == null)
                return;
            if (ms_vLocks.TryGetValue(pGo, out var locks))
            {
                var shader = pGo.shader;
                int propCount = shader != null ? shader.GetPropertyCount() : 0;
                for (int i = 0; i < propCount; i++)
                {
                    string propName = shader.GetPropertyName(i);
                    var propType = shader.GetPropertyType(i);
                    if (locks.vDatas.TryGetValue(propName, out var value))
                    {
                        switch (propType)
                        {
                            case UnityEngine.Rendering.ShaderPropertyType.Color:
                                pGo.SetColor(propName, (Color)value);
                                break;
                            case UnityEngine.Rendering.ShaderPropertyType.Vector:
                                pGo.SetVector(propName, (Vector4)value);
                                break;
                            case UnityEngine.Rendering.ShaderPropertyType.Float:
                            case UnityEngine.Rendering.ShaderPropertyType.Range:
                                pGo.SetFloat(propName, (float)value);
                                break;
                            case UnityEngine.Rendering.ShaderPropertyType.Texture:
                                pGo.SetTexture(propName, value as Texture);
                                break;
                        }
                    }
                }
                locks.refCnt--;
                if (locks.refCnt <= 0)
                {
                    ms_vLocks.Remove(pGo);
                }
            }
        }
        //-----------------------------------------------------
        public static void Backup(this Material pGo, bool bReBackup = false)
        {
            if (pGo == null)
                return;
            if (!ms_vLocks.TryGetValue(pGo, out var locks))
            {
                locks = new Lock();
                ms_vLocks[pGo] = locks;
            }
            else
            {
                if (!bReBackup)
                {
                    locks.refCnt++;
                    return;
                }
            }
            locks.refCnt++;

            // 备份所有属性
            var shader = pGo.shader;
            int propCount = shader != null ? shader.GetPropertyCount() : 0;
            for (int i = 0; i < propCount; i++)
            {
                string propName = shader.GetPropertyName(i);
                var propType = shader.GetPropertyType(i);
                switch (propType)
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
                        locks.vDatas[propName] = pGo.GetColor(propName);
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
                        locks.vDatas[propName] = pGo.GetVector(propName);
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
                        locks.vDatas[propName] = pGo.GetFloat(propName);
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
                        locks.vDatas[propName] = pGo.GetTexture(propName);
                        break;
                }
            }
        }
        //-----------------------------------------------------
        public static void BackupTransform(this Transform transform, bool bReBackup = false)
        {
            if (transform == null)
                return;
            if (!ms_vLocks.TryGetValue(transform, out var locks))
            {
                locks = new Lock();
                ms_vLocks[transform] = locks;
            }
            else
            {
                if (!bReBackup)
                {
                    locks.refCnt++;
                    return;
                }
            }
            locks.refCnt++;
            locks.vDatas["pos"] = transform.position;
        //    locks.vDatas["euler"] = transform.eulerAngles;
            locks.vDatas["rotation"] = transform.rotation;
            locks.vDatas["scale"] = transform.localScale;
        }
        //-----------------------------------------------------
        public static void RestoreTransform(this Transform transform)
        {
            if (transform == null)
                return;
            if (ms_vLocks.TryGetValue(transform, out var locks))
            {
                if (locks.vDatas.TryGetValue("pos", out var pos))
                {
                    transform.position = (Vector3)pos;
                }
                if (locks.vDatas.TryGetValue("euler", out var euler))
                {
                    transform.eulerAngles = (Vector3)euler;
                }
                if (locks.vDatas.TryGetValue("rotation", out var rotation))
                {
                    transform.rotation = (Quaternion)rotation;
                }
                if (locks.vDatas.TryGetValue("scale", out var scale))
                {
                    transform.localScale = (Vector3)scale;
                }
                locks.refCnt--;
                if (locks.refCnt <= 0)
                {
                    ms_vLocks.Remove(transform);
                }
            }
        }
        //-----------------------------------------------------
        public static void Backup(this ICutsceneObject transform, bool bReBackup = false)
        {
            if (transform == null)
                return;
            if (!ms_vLocks.TryGetValue(transform, out var locks))
            {
                locks = new Lock();
                ms_vLocks[transform] = locks;
            }
            else
            {
                if (!bReBackup)
                {
                    locks.refCnt++;
                    return;
                }
            }
            locks.refCnt++;
            Vector3 temp = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            if(transform.GetParamPosition(ref temp)) locks.vDatas["pos"] = temp;
            if (transform.GetParamEulerAngle(ref temp)) locks.vDatas["euler"] = temp;
            if (transform.GetParamQuaternion(ref rot)) locks.vDatas["rot"] = rot;
            if (transform.GetParamScale(ref temp)) locks.vDatas["scale"] = temp;
        }
        //-----------------------------------------------------
        public static void Restore(this ICutsceneObject transform)
        {
            if (transform == null)
                return;
            if (ms_vLocks.TryGetValue(transform, out var locks))
            {
                if (locks.vDatas.TryGetValue("pos", out var pos))
                {
                    transform.SetParamPosition((Vector3)pos);
                }
                if (locks.vDatas.TryGetValue("euler", out var euler))
                {
                    transform.SetParamEulerAngle((Vector3)euler);
                }
                if (locks.vDatas.TryGetValue("scale", out var scale))
                {
                    transform.SetParamScale((Vector3)scale);
                }
                if (locks.vDatas.TryGetValue("rot", out var rot))
                {
                    transform.SetParamQuaternion((Quaternion)rot);
                }
                locks.refCnt--;
                if (locks.refCnt <= 0)
                {
                    ms_vLocks.Remove(transform);
                }
            }
        }
        //-----------------------------------------------------
        public static void BackupObject(System.Object pObj, bool bReBackup = false)
        {
            if (pObj == null)
                return;
            if (!ms_vLocks.TryGetValue(pObj, out var locks))
            {
                locks = new Lock();
                ms_vLocks[pObj] = locks;
            }
            else
            {
                if (!bReBackup)
                {
                    locks.refCnt++;
                    return;
                }
            }
            locks.refCnt++;
            var fields = pObj.GetType().GetFields( System.Reflection.BindingFlags.Public| System.Reflection.BindingFlags.Instance);
            foreach (var db in fields)
            {
                locks.vDatas[db.Name] = db.GetValue(pObj);
            }
            fields = pObj.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var db in fields)
            {
                locks.vDatas[db.Name] = db.GetValue(pObj);
            }
        }
        //-----------------------------------------------------
        public static void RestoreObject(this System.Object pObj)
        {
            if (pObj == null)
                return;
            if (ms_vLocks.TryGetValue(pObj, out var locks))
            {
                var fields = pObj.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var db in locks.vDatas)
                {
                    var field = pObj.GetType().GetField(db.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if(field == null)
                        field = pObj.GetType().GetField(db.Key, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null && db.Value!=null)
                        field.SetValue(pObj, db.Value);
                }
                locks.refCnt--;
                if (locks.refCnt <= 0)
                {
                    ms_vLocks.Remove(pObj);
                }
            }
        }
        //-----------------------------------------------------
        public static List<object> RestoreParams(string key)
        {
            if (key == null)
                return null;
            if (ms_vLocks.TryGetValue(key, out var locks))
            {
                locks.refCnt--;
                if (locks.refCnt <= 0)
                {
                    ms_vLocks.Remove(key);
                }
                return locks.vDatas.Values.ToList();
            }
            return new List<object>();
        }
        //-----------------------------------------------------
        public static void BackupParams(string key, params System.Object[] vParams)
        {
            if (key == null)
                return;
            if (!ms_vLocks.TryGetValue(key, out var locks))
            {
                locks = new Lock();
                ms_vLocks[key] = locks;
            }
            else
            {
                locks.refCnt++;
                return;
            }
            locks.refCnt++;
            for(int i =0; i < vParams.Length; ++i)
            {
                locks.vDatas[i.ToString()] = vParams[i];
            }
        }
    }
}

#endif