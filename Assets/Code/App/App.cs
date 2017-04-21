using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class App : MonoBehaviour
{
    public static App Inst { get { return m_Inst; } }
    static App m_Inst;

    [NonSerialized] public bool IsRunning = true;

    public App()
    {
        if (m_Inst == null)
            m_Inst = this;
    }

    // Game entry point (this is the first thing done when the game boots)
    public void Start()
    {
        // TODO: Load all of your data

        // TODO: Initialize the application    
    }

    private void Init()
    {
        // TODO: Add your app after loading data
    }

    // Update is called once per frame
    void Update()
    {

#if UNITY_EDITOR
        // ------------------------
        // Dev Hacks
        // ------------------------
        
        // NOTE: Put dev hacks here

        // ------------------------
#endif
    }

    public void Reset()
    {
        // TODO: Reset the entire application
    }

    public void Pause()
    {
        App.Inst.IsRunning = false;
    }

    public void Unpause()
    {
        App.Inst.IsRunning = false;

    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }

    public bool GetIsRunning()
    {
        return IsRunning;
    }
}