using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE:
// StartCoroutine used in this context does not relate to StartCoroutine as part of Monobehavior
namespace MfUnity.App
{
    public static class CoroutineRunner
    {
        public static MonoBehaviour Instance
        {
            get
            {
                if (_Instance == null)
                {
                    Initialize();
                    return _Instance;
                }
                return _Instance;
            }
        }
        private static MonoBehaviour _Instance = null;

        public static void Initialize()
        {
            if (_Instance != null)
                return;
            GameObject go = new GameObject("CoroutineRunner");
            GameObject.DontDestroyOnLoad(go);
            _Instance = go.AddComponent<MonoBehaviour>();
        }
        
        public static Coroutine StartCoroutine(string name, IEnumerator coroutine)
        { return null; }
        
        public static Coroutine WaitForRealSeconds(float time, Action onComplete = null)
        { return CoroutineRunner.StartCoroutine("WaitForRealSeconds", _WaitForRealSeconds(time, onComplete)); }

        public static Coroutine WaitForEndOfFrame(Action onComplete = null)
        { return CoroutineRunner.StartCoroutine("WaitForEndOfFrameRoutine", WaitForEndOfFrameRoutine(onComplete)); }

        static IEnumerator WaitForEndOfFrameRoutine(Action onComplete)
        {
            yield return new WaitForEndOfFrame();

        }

        static IEnumerator _WaitForRealSeconds(float time, Action onComplete = null)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < (start + time))
                yield return null;


        }

        // Wait for all of the async ops simultaneously.  Don't "yield return" on them individually or they will each take at least one frame.
        // AsyncOperations are not general YieldInstructions.  They are only used in Unity's asset system.
        public static IEnumerator WaitForMultipleAsyncOps<T>(List<T> asyncOps) where T:AsyncOperation
        {
            for (;;)
            {
                int lastNotDone = -1;
                for (int i = 0; i < asyncOps.Count; i++)
                    if (!asyncOps[i].isDone)
                        lastNotDone = i;
                if (lastNotDone == -1)
                    break;
                yield return asyncOps[lastNotDone];
            }
        }

        public static void StopCoroutine(IEnumerator coroutine)
        { if (Instance) Instance.StopCoroutine(coroutine); }

        public static void StopCoroutine(Coroutine coroutine)
        { if (Instance) Instance.StopCoroutine(coroutine); }

        public static Coroutine AfterNFrames(uint frames, Action action)
        { return StartCoroutine("AfterNFrames", _AfterNFrames(frames, action)); }
        private static IEnumerator _AfterNFrames(uint frames, Action action)
        {
            for (uint i = 0; i < frames; ++i)
                yield return null;
            action();
        }

        public static Coroutine NextFrame(Action action)
        { return StartCoroutine("AfterNFrames", _AfterNFrames(1, action)); }
        public static Coroutine NextFrame<T1>(Action<T1> action, T1 arg1)
        { return NextFrame(() => action(arg1)); }
        public static Coroutine NextFrame<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
        { return NextFrame(() => action(arg1, arg2)); }
        public static Coroutine NextFrame<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        { return NextFrame(() => action(arg1, arg2, arg3)); }

        public static Coroutine AfterYieldInstruction(YieldInstruction yieldInstruction, Action<YieldInstruction> action)
        { return StartCoroutine("AfterYieldInstruction", _AfterYieldInstruction(yieldInstruction, action)); }
        private static IEnumerator _AfterYieldInstruction(YieldInstruction yieldInstruction, Action<YieldInstruction> action)
        {
            yield return yieldInstruction;
            action(yieldInstruction);
        }

        public static Coroutine AfterPredicate(Func<bool> predicate, Action action)
        { return StartCoroutine("AfterPredicate", _AfterPredicate(predicate, action)); }
        private static IEnumerator _AfterPredicate(Func<bool> predicate, Action action)
        {
            while (predicate() == false)
                yield return null;
            action();
        }

        // This is used to visibly see which AfterPredicates are still running in console
        public static Coroutine DebugAfterPredicate(Func<bool> predicate, Action action, string message)
        { return StartCoroutine("AfterPredicate", _AfterPredicate(predicate, action, message)); }
        private static IEnumerator _AfterPredicate(Func<bool> predicate, Action action, string message)
        {
            while (predicate() == false)
            {
                yield return null;
            }
            action();
        }

        public static Coroutine WwwGet(string uri, Action<WWW> onCompleted)
        { return StartCoroutine("WwwGet:" + uri, _WwwGet(uri, onCompleted)); }
        private static IEnumerator _WwwGet(string uri, Action<WWW> onCompleted)
        {
            using (WWW www = new WWW(uri))
            {
                yield return www;
                onCompleted(www);
            }
        }

        // yield return CoroutineRunner.DoAsync(doneAsync => 
        //    YourFunction(yourResult => doneAsync()));
        public static Coroutine DoAsync(Action<Action> toDo)
        {
            bool doneAsync = false;
            toDo(() => doneAsync = true);
            return StartCoroutine("DoAsync", _DoAsync(() => { return doneAsync; }));
        }
        private static IEnumerator _DoAsync(Func<bool> doneAsync)
        {
            while (!doneAsync())
                yield return null;
        }
    }
}