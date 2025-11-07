/********************************************************************
生成日期:	06:30:2025
类    名: 	VariablePool
作    者:	HappLI
描    述:   变量缓冲池
*********************************************************************/
using System.Collections.Generic;

namespace Framework.AT.Runtime
{
    public static class VariablePool
    {
        static int                  MAX_POOL = 16;
        static Stack<VariableList>  ms_vLists = null;
        static Stack<VariableKV>    ms_vKvs = null;
        static Stack<VariableStack> ms_vStacks = null;
        //-----------------------------------------------------
        internal static VariableList GetVariableList()
        {
            if (ms_vLists!=null && ms_vLists.Count > 0)
                return ms_vLists.Pop();
            return new VariableList();
        }
        //-----------------------------------------------------
        internal static void ReleaseVariableList(VariableList list)
        {
            if (list == null) return;
            list.Clear();
            if (ms_vLists == null) ms_vLists = new Stack<VariableList>(MAX_POOL);
            else
            {
                if (ms_vLists.Count >= MAX_POOL) return;
            }
            if (ms_vLists.Contains(list))
                return;
            ms_vLists.Push(list);
        }
        //-----------------------------------------------------
        internal static VariableKV GetVariableKV()
        {
            if (ms_vKvs!=null && ms_vKvs.Count > 0)
                return ms_vKvs.Pop();
            return new VariableKV();
        }
        //-----------------------------------------------------
        internal static void ReleaseVariableKV(VariableKV kv)
        {
            if (kv == null) return;
            kv.Clear();
            if (ms_vKvs == null) ms_vKvs = new Stack<VariableKV>(MAX_POOL);
            else
            {
                if (ms_vKvs.Count >= MAX_POOL) return;
            }
            if (ms_vKvs.Contains(kv))
                return;
            ms_vKvs.Push(kv);
        }
        //-----------------------------------------------------
        internal static VariableStack GetVariableStack()
        {
            if (ms_vStacks!=null && ms_vStacks.Count > 0)
                return ms_vStacks.Pop();
            return new VariableStack();
        }
        //-----------------------------------------------------
        internal static void ReleaseVariableStack(VariableStack stack)
        {
            if (stack == null) return;
            stack.Clear();
            if (ms_vStacks == null) ms_vStacks = new Stack<VariableStack>(MAX_POOL);
            else
            {
                if (ms_vStacks.Count >= MAX_POOL) return;
            }
            if (ms_vStacks.Contains(stack))
                return;
            ms_vStacks.Push(stack);
        }
        //-----------------------------------------------------
        internal static void Release(this VariableKV kv)
        {
            ReleaseVariableKV(kv);
        }
        //-----------------------------------------------------
        internal static VariableKV Malloc()
        {
            return GetVariableKV();
        }
        //-----------------------------------------------------
        internal static void ClearAll()
        {
            if (ms_vLists != null) ms_vLists.Clear();
            if (ms_vKvs != null) ms_vKvs.Clear();
            if (ms_vStacks != null) ms_vStacks.Clear();
        }      
        //-----------------------------------------------------
        public static void Release(this VariableList kv)
        {
            ReleaseVariableList(kv);
        }
        //-----------------------------------------------------
        public static void Release(this VariableStack kv)
        {
            ReleaseVariableStack(kv);
        }
    }
}