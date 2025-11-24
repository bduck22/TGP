using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CutsceneEngine;
using UnityEditor.Timeline;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CutsceneEngineEditor
{
    [CustomEditor(typeof(PhysicsSimulator))]
    public class PhysicsSimulatorInspector : Editor
    {
        public class RigidbodyState
        {
            public readonly Rigidbody rigidbody;
            public readonly Rigidbody2D rigidbody2D;
            public readonly Transform transform;
            public Vector3 position;
            public Quaternion rotation;
            public readonly string relativePath;
            public readonly Dictionary<string, AnimationCurve> curves = new();

            public RigidbodyState(Transform root, Rigidbody rb)
            {
                rigidbody = rb;
                transform = rb.transform;
                position = rb.transform.position;
                rotation = rb.transform.rotation;

                rb.position = position;
                rb.rotation = rotation;

                relativePath = PhysicsRecorder.GetRelativePath(root, rb.transform);
            }

            public RigidbodyState(Transform root, Rigidbody2D rb)
            {
                rigidbody2D = rb;
                transform = rb.transform;
                position = rb.transform.position;
                rotation = rb.transform.rotation;

                rb.position = position;
                rb.rotation = rotation.eulerAngles.z;

                relativePath = PhysicsRecorder.GetRelativePath(root, rb.transform);
            }
        }

        // Static fields for simulation state
        static bool isSimulating;
        static bool isPaused;
        static bool isTimedSimulation;
        static bool wasPreDelayComplete;
        static float currentSimulationTime;
        static double preDelayTimer;
        static double timeAccumulator;
        static double lastUpdateTime;
        
        // Recording-related fields
        static AnimationTrack recordingTrack;
        static Transform recordingTrackBinding;
        static double recordingStartDirectorTime;
        static List<RigidbodyState> allRigidbodyStates = new List<RigidbodyState>();
        static List<TimelineClip> existClips = new();
        static List<ForceSettings> forceSettings = new();
        static List<ForceFieldSettings> forceFieldSettings = new();
        static bool shouldRecording;
        static PhysicsRecorder recorder;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            CheckRecordingTrack();
            
            DrawSimulationControls();
            DrawTimedSimulationControls();
            serializedObject.ApplyModifiedProperties();
        }


        void DrawSimulationControls()
        {
            EditorGUILayout.LabelField("Simulation Controls", EditorStyles.boldLabel);

            if (shouldRecording)
            {
                EditorGUILayout.HelpBox("Cannot edit properties during recording or previewing.", MessageType.Info, true);
                GUI.enabled = false;
            }
            var simulatioStepProp = serializedObject.FindProperty(nameof(PhysicsSimulator.simulationStep));
            EditorGUILayout.PropertyField(simulatioStepProp, new GUIContent("Simulation Step (seconds)"));
            
            var preDelayProp = serializedObject.FindProperty(nameof(PhysicsSimulator.preDelay));
            EditorGUILayout.PropertyField(preDelayProp, new GUIContent("Pre Delay (seconds)"));
            
            var curveOptimizationSettings = serializedObject.FindProperty(nameof(PhysicsSimulator.curveOptimizationSettings));
            EditorGUILayout.PropertyField(curveOptimizationSettings, new GUIContent("Curve Optimization Settings"), true);

            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !isSimulating || isPaused;
            if (GUILayout.Button(isPaused ? "Resume" : "Play"))
            {
                if (!isSimulating) StartSimulation(false);
                else ResumeSimulation();
            }

            GUI.enabled = isSimulating && !isPaused;
            if (GUILayout.Button("Pause"))
            {
                PauseSimulation();
            }

            GUI.enabled = isSimulating;
            if (GUILayout.Button("Stop"))
            {
                StopSimulation();
            }

            GUI.enabled = isPaused;
            if (GUILayout.Button("Apply Paused State"))
            {
                ApplyPausedState();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        void DrawTimedSimulationControls()
        {
            EditorGUILayout.LabelField("Timed Simulation", EditorStyles.boldLabel);
            
            if (shouldRecording)
            {
                GUI.enabled = false;
            }
            var durationProp = serializedObject.FindProperty(nameof(PhysicsSimulator.simulationDuration));
            EditorGUILayout.PropertyField(durationProp, new GUIContent("Duration (seconds)"));

            GUI.enabled = true;
            
            GUI.enabled = !isSimulating;
            if (GUILayout.Button("Run Timed Simulation"))
            {
                StartSimulation(true);
            }
            GUI.enabled = true;
        }
        
        
        void StartSimulation(bool startWithDuration)
        {
            isTimedSimulation = startWithDuration;
            InitializeSimulation();
            EditorApplication.update += SimulationUpdateLoop;
            EditorApplication.delayCall += () =>
            {
                foreach (var v in SceneView.sceneViews)
                {
                    var sceneView = v as SceneView;
                    EditorUtility.SetDirty(sceneView);   
                }
                EditorApplication.Step();
            };
        }

        void PauseSimulation()
        {
            if (!isSimulating) return;
            isPaused = true;
            timeAccumulator = 0;
        }

        void ResumeSimulation()
        {
            if (!isSimulating || !isPaused) return;
            isPaused = false;
            lastUpdateTime = EditorApplication.timeSinceStartup;
        }

        void InitializeSimulation()
        {
            isSimulating = true;
            isPaused = false;
            Physics.simulationMode = SimulationMode.Script;
            Physics2D.simulationMode = SimulationMode2D.Script;
            
            InitializeRigidbodies();
            InitializeTimeVariables();
            InitializeRecording();
            FindForceSettings();
            FindForceFieldSettings();
        }

        void InitializeRigidbodies()
        {
            allRigidbodyStates.Clear();
            
            foreach (var rb in FindObjectsByType<Rigidbody>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                allRigidbodyStates.Add(new RigidbodyState(recordingTrackBinding, rb));
            }

            foreach (var rb in FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                allRigidbodyStates.Add(new RigidbodyState(recordingTrackBinding, rb));
            }
        }

        void InitializeTimeVariables()
        {
            timeAccumulator = 0;
            lastUpdateTime = EditorApplication.timeSinceStartup;
            currentSimulationTime = 0;
            preDelayTimer = 0;
            wasPreDelayComplete = false;
        }

        void InitializeRecording()
        {
            shouldRecording = false;
            recordingTrack = null;
            recordingTrackBinding = null;
            existClips.Clear();

            var window = TimelineEditor.GetWindow();
            if (!window) return;

            var director = TimelineEditor.inspectedDirector;
            if (!director) return;

            SetupRecordingTrack();
            recordingStartDirectorTime = director.time;
        }

        void CheckRecordingTrack()
        {
            recordingTrack = null;
            recordingTrackBinding = null;
            shouldRecording = false;
            var director = TimelineEditor.inspectedDirector;
            if (!director) return;
            
            foreach (var track in director.GetTracks<AnimationTrack>())
            {
                if (!track.IsRecording()) continue;

                recordingTrack = track;
                var binding = director.GetGenericBinding(track) as Animator;
                if (binding)
                {
                    recordingTrackBinding = binding.transform;
                }
                SetupRecordingClips(track);
                break;
            }

            shouldRecording = recordingTrack && recordingTrackBinding;
        }

        void SetupRecordingTrack()
        {
            CheckRecordingTrack();
            if (shouldRecording)
            {
                recorder = new PhysicsRecorder(recordingTrackBinding, recordingTrack.infiniteClip, allRigidbodyStates, 
                    (float)recordingStartDirectorTime);
                
                if (recordingTrack.infiniteClip != null)
                {
                    Undo.RecordObject(recordingTrack.infiniteClip, "Recording Track");
                    recordingTrack.infiniteClip.ClearCurves();
                }
            }

            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved | RefreshReason.SceneNeedsUpdate);
        }

        void SetupRecordingClips(AnimationTrack track)
        {
            if (track.hasClips && track.infiniteClip == null)
            {
                foreach (var clip in track.GetClips())
                {
                    existClips.Add(clip);
                }

                foreach (var clip in existClips)
                {
                    track.DeleteClip(clip);
                }
            }
            else
            {
                track.CreateInfiniteClip("Physics Record");
            }
        }

        void FindForceSettings()
        {
            forceSettings.Clear();
            ((PhysicsSimulator)target).GetComponentsInChildren(false, forceSettings);
        }

        void FindForceFieldSettings()
        {
            forceFieldSettings.Clear();
            forceFieldSettings.AddRange(FindObjectsByType<ForceFieldSettings>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        }

        void SimulationUpdateLoop()
        {
            if (!isSimulating || isPaused) return;
            
            var simulator = (PhysicsSimulator)target;
            if (preDelayTimer < simulator.preDelay && !wasPreDelayComplete )
            {
                preDelayTimer = EditorApplication.timeSinceStartup - lastUpdateTime;
                Physics.Simulate(simulator.simulationStep);
                Physics2D.Simulate(simulator.simulationStep);

                if (preDelayTimer > simulator.preDelay)
                {
                    lastUpdateTime = EditorApplication.timeSinceStartup;
                    wasPreDelayComplete = true;
                }
                return;
            }
            
            
            if (shouldRecording)RecordCurrentFrame();
            
            ProcessPhysicsSteps();
            CheckTimedSimulationEnd();
            UpdateSimulationTime();
            
            SceneView.RepaintAll();
            EditorApplication.Step();
        }

        void UpdateSimulationTime()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            double deltaTime = currentTime - lastUpdateTime;
            lastUpdateTime = currentTime;
            timeAccumulator += deltaTime;
        }

        void ProcessPhysicsSteps()
        {
            var simulator = (PhysicsSimulator)target;
            
            while (timeAccumulator >= simulator.simulationStep)
            {
                foreach (var forceSetting in forceSettings)
                {
                    if (forceSetting != null)
                    {
                        forceSetting.ApplyForces(currentSimulationTime);
                    }
                }

                foreach (var forceField in forceFieldSettings)
                {
                    if (forceField != null)
                    {
                        forceField.ApplyForce(currentSimulationTime);
                    }
                }

                Physics.Simulate(simulator.simulationStep);
                Physics2D.Simulate(simulator.simulationStep);
            
                currentSimulationTime += simulator.simulationStep;
                timeAccumulator -= simulator.simulationStep;
                if(shouldRecording)
                {
                    timeAccumulator = 0;
                    break;
                }
            }
        }

        void RecordCurrentFrame()
        {
            if (recordingTrack.infiniteClip != null)
            {
                if (TimelineEditor.inspectedDirector)
                {
                    TimelineEditor.inspectedDirector.time = recordingStartDirectorTime + currentSimulationTime;
                }
                recorder.RecordKeyframes(recordingTrackBinding, (float)recordingStartDirectorTime + currentSimulationTime);
            }
            else
            {
                recorder.RecordKeyframes(recordingTrackBinding, currentSimulationTime);
            }
        }

        void CheckTimedSimulationEnd()
        {
            var simulator = (PhysicsSimulator)target;
            if (isTimedSimulation && currentSimulationTime >= simulator.simulationDuration)
            {
                StopSimulation();
            }
        }

        void StopSimulation()
        {
            if (!isSimulating) return;
            
            Debug.Log("Stop Simulation");
            EditorApplication.update -= SimulationUpdateLoop;

            ResetSimulationState();
            CompleteRecording();
            ResetRigidbodyStates();
            
            SceneView.RepaintAll();
        }

        void ResetSimulationState()
        {
            isSimulating = false;
            isPaused = false;
            isTimedSimulation = false;
            Physics.simulationMode = SimulationMode.FixedUpdate;
            Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        }

        void ResetRigidbodyStates()
        {
            foreach (var state in allRigidbodyStates)
            {
                if (state.rigidbody2D)
                {
#if UNITY_6000_0_OR_NEWER
                    state.rigidbody2D.linearVelocity = Vector3.zero;
#else
                    state.rigidbody2D.velocity = Vector2.zero;
#endif
                    
                    state.rigidbody2D.angularVelocity = 0;
                }

                if (state.rigidbody)
                {
#if UNITY_6000_0_OR_NEWER
                    state.rigidbody.linearVelocity = Vector3.zero;
#else
                    state.rigidbody.velocity = Vector2.zero;
#endif
                    state.rigidbody.angularVelocity = Vector3.zero;
                }

                state.transform.position = state.position;
                state.transform.rotation = state.rotation;
            }
            
            Physics.SyncTransforms();
            Physics2D.SyncTransforms();
            allRigidbodyStates.Clear();
        }

        void CompleteRecording()
        {
            Debug.Log($"shuld recording? {shouldRecording}");
            if (!shouldRecording) return;
            
            var simulator = (PhysicsSimulator)target;
            recorder.CompleteRecording(simulator.curveOptimizationSettings);
            
            if (recordingTrack.infiniteClip == null)
            {
                CreateRecordingClip();
            }

            recordingTrack.StopRecording();
            var window = TimelineEditor.GetWindow();
            if (window) 
            {
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }
        }

        void CreateRecordingClip()
        {
            Undo.PerformUndo();

            var timelineClip = recordingTrack.CreateRecordableClip($"Physics Recording {DateTime.Now:h-m-s}");
            var playableAsset = timelineClip.asset as AnimationPlayableAsset;
            
            foreach (var state in allRigidbodyStates)
            {
                foreach (var kv in state.curves)
                {
                    playableAsset.clip.SetCurve(state.relativePath, typeof(Transform), kv.Key, kv.Value);
                }
            }

            timelineClip.duration = currentSimulationTime;
            timelineClip.start = recordingStartDirectorTime;

            var director = TimelineEditor.inspectedDirector;
            if (director)
            {
                director.time = timelineClip.end;
            }
        }

        void ApplyPausedState()
        {
            if (!isPaused) return;

            foreach (var state in allRigidbodyStates)
            {
                Undo.RecordObject(state.transform, "Apply Paused Physics State");
                state.position = state.transform.position;
                state.rotation = state.transform.rotation;
            }
            
            StopSimulation();
        }

        void OnDisable()
        {
            if (isSimulating)
            {
                StopSimulation();
            }
        }
    }
}