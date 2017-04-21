using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceManager {

    protected static ResourceManager m_Inst;

    Dictionary<string, UnityEngine.Object> cachedObjects = new Dictionary<string, UnityEngine.Object>();

    public static ResourceManager Inst
    {
        get
        {
            if (m_Inst == null)
                m_Inst = new ResourceManager();
            return m_Inst as ResourceManager;
        }
    }

    public static GameObject Create(string prefabPath)
    {
        if (!Inst.cachedObjects.ContainsKey(prefabPath))
        {
             Inst.cachedObjects[prefabPath] = Resources.Load(prefabPath);
        }

        if(Inst.cachedObjects[prefabPath] == null)
        {
            Debug.Assert(false, "Could not load prefab with name: " + prefabPath);
            return null;
        }

        return UnityEngine.Object.Instantiate(Inst.cachedObjects[prefabPath]) as GameObject;
    }
}
