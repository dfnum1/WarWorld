/********************************************************************
生成日期:	06:30:2025
类    名: 	ACutsceneLogic
作    者:	HappLI
描    述:	过场动作基础逻辑类
*********************************************************************/
#if UNITY_EDITOR
using Framework.AT.Runtime;
using Framework.Cutscene.Runtime;
using Framework.ED;
using System.Collections.Generic;

namespace Framework.Cutscene.Editor
{
    public class ACutsceneLogic : AEditorLogic
    {
        UndoLogic m_pUndoLogic = null;
        CutsceneData m_pCurData = null;
        //--------------------------------------------------------
        public bool IsRuntimePlayingCutscene()
        {
            CutsceneEditor pEdit = GetOwner<CutsceneEditor>();
            if (pEdit == null) return false;
            return pEdit.IsRuntimeOpenPlayingCutscene();
        }
        //--------------------------------------------------------
        public void RegisterUndoData(bool isDirtyData=true)
        {
            if (m_pCurData == null) return;
            RegisterUndoData(m_pCurData, isDirtyData);
        }
        //--------------------------------------------------------
        public void RegisterUndoData(CutsceneData data, bool isDirtyData = true)
        {
            if (data == null)
                return;
            if (m_pUndoLogic == null) m_pUndoLogic = GetLogic<UndoLogic>();
            if (m_pUndoLogic == null)
                return;
            m_pUndoLogic.LockUndoData(data, isDirtyData);
        }
        //--------------------------------------------------------
        public void SetCutsceneData(CutsceneData data, bool bDirtyData)
        {
            m_pCurData = data;
            var logics = GetOwner().GetLogics();
            foreach (var db in logics)
            {
                if (db.GetType() == this.GetType())
                    continue;
                if (db is ACutsceneLogic)
                {
                    ((ACutsceneLogic)db).m_pCurData = data;
                    if (bDirtyData)
                        ((ACutsceneLogic)db).OnRefreshData(data);
                    else
                        ((ACutsceneLogic)db).OnChangeSelect(data);
                }
            }
        }
        //--------------------------------------------------------
        public void UndoAgentTreeData(AgentTreeData pData)
        {
            var logics = GetOwner().GetLogics();
            foreach (var db in logics)
            {
                if (db.GetType() == this.GetType())
                    continue;
                if (db is ACutsceneLogic)
                {
                    ((ACutsceneLogic)db).OnRefreshData(pData);
                }
            }
        }
        //--------------------------------------------------------
        public override void OnChangeSelect(object pData)
        {
            if (pData == null)
                return;

            if (pData is CutsceneData)
                m_pCurData = pData as CutsceneData;
        }
        //--------------------------------------------------------
        public CutsceneData GetAsset(bool bAutoNew = false)
        {
            if (m_pCurData == null && bAutoNew)
            {
               m_pCurData = new CutsceneData();
                SetCutsceneData(m_pCurData,false);
            }
            return m_pCurData;
        }
        //--------------------------------------------------------
        public CutsceneGraph GetCutsceneGraph()
        {
            var pObj = GetOwner().GetCurrentObj();
            if (pObj == null) return null;
            if(pObj is CutsceneObject)
            {
                return ((CutsceneObject)pObj).GetCutsceneGraph();
            }
            return null;
        }
        //--------------------------------------------------------
        internal void SelectClips(List<IDraw> vSelectClips)
        {
            var logics = GetOwner().GetLogics();
            foreach (var db in logics)
            {
                if (db.GetType() == this.GetType())
                    continue;
                if (db is ACutsceneLogic)
                {
                    ((ACutsceneLogic)db).OnSelectClips(vSelectClips);
                }
            }
        }
        //--------------------------------------------------------
        public virtual void OnSelectClips(List<IDraw> OnSelectClips)
        {
        }
        //--------------------------------------------------------
        public virtual void OnSelectGroup(CutsceneData.Group group)
        {
        }
        //-----------------------------------------------------
        public virtual void OnEnableCutscene(CutsceneInstance pCutscene, bool bEnable)
        {

        }
    }
}

#endif