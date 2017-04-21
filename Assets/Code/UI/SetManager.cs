using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SetManager
{    
    protected static SetManager m_Inst;

    List<Set> Sets = new List<Set>();
    Set currentSet;

    public static SetManager Inst
    {
        get
        {
            if (m_Inst == null)
                m_Inst = new SetManager();
            return m_Inst as SetManager;
        }
    }

    public static T OpenSet<T>(Action<T> onInit = null) where T : Set
    {
        string setName = typeof(T).Name;

        // Make current set inactive
        if (Inst.currentSet != null) { Inst.currentSet.gameObject.SetActive(true); }

        // Create the new set and mark active
        GameObject setGO = ResourceManager.Create("UI/Sets/" + setName);
        if (setGO != null)
        {
            T castedSet = setGO.GetComponent<T>();
            if (castedSet != null)
            {
                Inst.Sets.Add(castedSet);
                Inst.currentSet = castedSet;
                Inst.currentSet.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning(setName + " didn't have a Set component attached, adding it for you..");
                castedSet = setGO.AddComponent<T>();
                Inst.Sets.Add(castedSet);
                Inst.currentSet = castedSet;
                Inst.currentSet.gameObject.SetActive(true);
            }

            // Perform the Init after set has been opened
            if(onInit != null)
                onInit.Invoke(castedSet);

            return castedSet;
        }

        return null;
    }

    public static void CloseSet(Set set)
    {
        var inst = Inst;
        if (!inst.Sets.Contains(set))
        {
            Debug.Log("Tried to close a set that wasn't on the stack");
            return;
        }

        RemoveAndDestroySet(set);
    }

    static void RemoveAndDestroySet(Set set)
    {
        Inst.Sets.RemoveAll(s => s == set);

        if (set)
        {
            UnityEngine.Object.Destroy(set.gameObject);
        }
    }
}