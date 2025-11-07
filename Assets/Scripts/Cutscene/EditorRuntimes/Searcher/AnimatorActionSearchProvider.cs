/********************************************************************
生成日期:	7:9:2025  10:00
类    名: 	AnimatorActionSearchProvider
作    者:	HappLI
描    述:	动作选择搜索
*********************************************************************/
#if UNITY_EDITOR
using Framework.Cutscene.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Framework.Cutscene.Editor
{
    /// <summary>
    /// 支持两级弹窗：第一级为AnimatorController资源列表，第二级为选中AnimatorController的动作列表
    /// </summary>
    public class AnimatorActionSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public System.Action<string, float,bool> onSelectAction;

        // 用于二级弹窗的数据结构
        [System.Serializable]
        public class ActionData
        {
            public string controllerPath;
            public string actionName;
            public float length;
            public bool loop;
        }

        private List<ActionData> m_actionList = null;
        private string m_controllerName = null;

        private string m_AnimatorPath = null;
        private ICutsceneObject m_CutsceneObject = null;
        public void SetAnimator(ICutsceneObject pObject)
        {
            m_AnimatorPath = null;
            m_CutsceneObject = pObject;
            if (pObject == null)
                return;
            var animator = pObject.GetAnimator();
            if (animator && animator.runtimeAnimatorController)
                m_AnimatorPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
        }

        /// <summary>
        /// 设置当前二级弹窗的动作列表
        /// </summary>
        public void SetActionList(string controllerPath, AnimatorController controller)
        {
            m_actionList = new List<ActionData>();
            m_controllerName = System.IO.Path.GetFileNameWithoutExtension(controllerPath);
            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    m_actionList.Add(new ActionData
                    {
                        controllerPath = controllerPath,
                        actionName = state.state.name,
                        loop = state.state.motion.isLooping,
                        length = state.state.motion.averageDuration
                    });
                }
            }
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();

            if (m_actionList != null)
            {
                // 二级弹窗：显示动作列表
                tree.Add(new SearchTreeGroupEntry(new GUIContent($"选择动作 - {m_controllerName}")));
                if (m_actionList.Count == 0)
                {
                    tree.Add(new SearchTreeEntry(new GUIContent("(无动作)"))
                    {
                        level = 1,
                        userData = null
                    });
                }
                else
                {
                    foreach (var action in m_actionList)
                    {
                        tree.Add(new SearchTreeEntry(new GUIContent(action.actionName))
                        {
                            level = 1,
                            userData = action
                        });
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(m_AnimatorPath))
                {
                    AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(m_AnimatorPath);
                    if(controller== null)
                    {
                        tree.Add(new SearchTreeGroupEntry(new GUIContent($"选择动作 - {System.IO.Path.GetFileNameWithoutExtension(m_AnimatorPath)}")));
                        tree.Add(new SearchTreeEntry(new GUIContent("(无动作)"))
                        {
                            level = 1,
                            userData = null
                        });
                    }
                    else
                    {
                        tree.Add(new SearchTreeGroupEntry(new GUIContent($"选择动作 - {controller.name}")));
                        SetActionList(m_AnimatorPath, controller);
                        if (m_actionList.Count == 0)
                        {
                            tree.Add(new SearchTreeEntry(new GUIContent("(无动作)"))
                            {
                                level = 1,
                                userData = null
                            });
                        }
                        else
                        {
                            foreach (var action in m_actionList)
                            {
                                tree.Add(new SearchTreeEntry(new GUIContent(action.actionName))
                                {
                                    level = 1,
                                    userData = action
                                });
                            }
                        }
                    }
                      
                }
                else if(m_CutsceneObject !=null)
                {
                    tree.Add(new SearchTreeGroupEntry(new GUIContent($"选择动作")));
                    m_actionList = new List<ActionData>();
                    List<CutObjActionInfo> actions = m_CutsceneObject.GetAnimatonsProvider();
                    foreach(var db in actions)
                    {
                        ActionData action = new ActionData();
                        action.actionName = db.name;
                        action.length = db.length;
                        action.loop = db.bLoop;
                        m_actionList.Add(action);
                    }
                    if (m_actionList.Count == 0)
                    {
                        tree.Add(new SearchTreeEntry(new GUIContent("(无动作)"))
                        {
                            level = 1,
                            userData = null
                        });
                    }
                    else
                    {
                        foreach (var action in m_actionList)
                        {
                            tree.Add(new SearchTreeEntry(new GUIContent(action.actionName))
                            {
                                level = 1,
                                userData = action
                            });
                        }
                    }
                }
                else
                {
                    // 一级弹窗：显示AnimatorController资源列表
                    tree.Add(new SearchTreeGroupEntry(new GUIContent("选择AnimatorController")));

                    string[] guids = AssetDatabase.FindAssets("t:AnimatorController");
                    foreach (var guid in guids)
                    {
                        string controllerPath = AssetDatabase.GUIDToAssetPath(guid);
                        string controllerName = System.IO.Path.GetFileNameWithoutExtension(controllerPath);
                        tree.Add(new SearchTreeEntry(new GUIContent(controllerName))
                        {
                            level = 1,
                            userData = controllerPath // 只保存路径
                        });
                    }
                    if (guids.Length == 0)
                    {
                        tree.Add(new SearchTreeEntry(new GUIContent("未找到AnimatorController"))
                        {
                            level = 1,
                            userData = null
                        });
                    }
                }

            }
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            // 一级弹窗：选择AnimatorController，弹出二级动作弹窗
            if (entry.userData is string controllerPath)
            {
                AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
                if (controller == null)
                {
                    EditorUtility.DisplayDialog("提示", "AnimatorController类型不匹配", "确定");
                    return false;
                }

                // 记录当前鼠标位置
                Vector2 mouseScreenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

                // 延迟到下一帧再弹出二级动作选择窗口，避免同一帧多次弹窗无效
                EditorApplication.delayCall += () =>
                {
                    var actionProvider = ScriptableObject.CreateInstance<AnimatorActionSearchProvider>();
                    actionProvider.onSelectAction = onSelectAction; // 复用回调
                    actionProvider.SetActionList(controllerPath, controller); // 传递动作列表
                    SearchWindow.Open(new SearchWindowContext(mouseScreenPos), actionProvider);
                };
                return true;
            }
            // 二级弹窗：选择动作
            else if (entry.userData is ActionData actionData)
            {
                onSelectAction?.Invoke(actionData.actionName, actionData.length, actionData.loop);
                return true;
            }
            return false;
        }
    }
}
#endif