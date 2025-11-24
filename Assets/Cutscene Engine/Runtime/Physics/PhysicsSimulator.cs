using System;
using UnityEngine;

namespace CutsceneEngine
{
    [Serializable]
    public class CurveOptimizationSettings
    {
        [Range(0, 0.25f)]public float positionThreshold = 0.01f;
        [Range(0, 25f)]public float angleThreshold = 1f;
        [Range(0, 50f)]public float curvatureThreshold = 25f;
        [Range(1, 10)]public int iteration = 1;
    }
    
    /// <summary>
    /// Component that simulates physics for recording and playback in cutscenes.
    /// Allows recording physics movements and forces to recreate them during timeline playback.
    /// </summary>
    [AddComponentMenu("Cutscene Engine/Physics Simulator (Cutscene Engine)")]
    public class PhysicsSimulator : MonoBehaviour
    {
        [Tooltip("This adds a slight delay when starting the simulation. The simulation will continue during this delay, but recording will not occur.\n\n" +
                 "Some objects may have a collider partially overlapping the ground or slightly off the ground. " +
                 "In these cases, starting the simulation will take some time for the object to find a stable position. \n" +
                 "This is a useful option to allow time for physics objects to stabilize.")]
        public float preDelay;
        [Tooltip("This is the delta time used in the simulation. When recording key frames, recording is also based on this time. \n\n" +
                 "If you are concerned about performance, it is recommended to increase this value. " +
                 "You should adjust it by testing the quality and performance of the simulation yourself.")]
        public float simulationStep = 0.02f;
        
        [Tooltip("The time to apply the simulation. This is the value used when running a simulation with the Run Timed Simulation button in the Inspector.\n\n" +
                 "When this time is up, the simulation will automatically end, and if there is a track being recorded, that will also end.")]
        public float simulationDuration = 2.0f;
        
        
        [Tooltip("Settings for optimizing animation curves when recording physics simulations. These settings control how keyframes are reduced while preserving curve quality.")]
        public CurveOptimizationSettings curveOptimizationSettings = new CurveOptimizationSettings();
    }
}