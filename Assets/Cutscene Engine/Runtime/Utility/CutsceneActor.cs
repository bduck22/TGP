using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CutsceneEngine
{
    /// <summary>
    /// Marks a hierarchy as the visual source that can be cloned for a matching <see cref="CutsceneActorPreview"/>.
    /// </summary>
    [AddComponentMenu("Cutscene Engine/Cutscene Actor (Cutscene Engine)")]
    public class CutsceneActor : MonoBehaviour
    {
        static readonly List<CutsceneActor> Instances = new List<CutsceneActor>();

        [SerializeField] string key = "actor1";

        /// <summary> Unique key shared with <see cref="CutsceneActorPreview"/> instances. </summary>
        public string Key => key;

        public delegate Action TransformInitializationCallback(Vector3 position, Quaternion rotation);

        public event TransformInitializationCallback onTransformInitialized;
        public event Action onResetBinding;
        
        void OnEnable()
        {
            Register();
        }

        void OnDisable()
        {
            Unregister();
        }

        void Register()
        {
            if (!Instances.Contains(this))
            {
                Instances.Add(this);
            }
        }

        void Unregister()
        {
            Instances.Remove(this);
        }

        internal void InitializeTransform(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            onTransformInitialized?.Invoke(position, rotation);
        }

        internal void OnResetBinding()
        {
            onResetBinding?.Invoke();
        }

        /// <summary> Finds an active origin that matches the provided key. </summary>
        public static CutsceneActor Find(string lookupKey)
        {
            if (string.IsNullOrWhiteSpace(lookupKey)) return null;

            for (var i = Instances.Count - 1; i >= 0; i--)
            {
                var origin = Instances[i];
                if (!origin)
                {
                    Instances.RemoveAt(i);
                    continue;
                }

                if (!origin.isActiveAndEnabled) continue;
                if (origin.Key == lookupKey) return origin;
            }

            return null;
        }
    }
}
