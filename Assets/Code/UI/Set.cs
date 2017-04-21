using UnityEngine;
using System.Collections;

public class Set : MonoBehaviour
{
    protected void CloseSet()
    {
        SetManager.CloseSet(this);
    }
}