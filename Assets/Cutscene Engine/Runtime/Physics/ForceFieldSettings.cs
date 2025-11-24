using UnityEngine;

namespace CutsceneEngine
{
    /// <summary>
    /// Defines the shape of the force field.
    /// </summary>
    public enum ForceFieldShape
    {
        Sphere,
        Hemisphere,
        Box,
        Cylinder
    }

    public enum ForceDimension
    {
        Mode3D,
        Mode2D
    }

    /// <summary>
    /// Applies a force to all rigidbodies within a defined area of effect.
    /// </summary>
    [AddComponentMenu("Cutscene Engine/Force Field Settings (Cutscene Engine)")]
    public class ForceFieldSettings : MonoBehaviour
    {
        // Shape Settings
        [Tooltip("The shape of the force field.")]
        public ForceFieldShape shape = ForceFieldShape.Sphere;

        [Tooltip("The radius of the sphere or cylinder shape.")]
        [Min(0)]
        public float radius = 1f;

        [Tooltip("The length of the cylinder. The cylinder is oriented along the local Y-axis.")]
        [Min(0)]
        public float length = 5f;

        [Tooltip("The dimensions of the box shape.")]
        public Vector3 boxSize = Vector3.one;

        // Force Settings
        [Tooltip("The magnitude of the force to be applied. Negative values will apply force in the opposite direction.")]
        public float forceMagnitude = 10f;

        [Tooltip("A curve that defines how the force diminishes over distance from the center/surface. The X-axis represents the normalized distance (0 to 1), and the Y-axis represents the force multiplier (0 to 1).")]
        public AnimationCurve forceFalloff = AnimationCurve.Linear(0, 1, 1, 0);

        [Tooltip("The type of force to apply to 3D rigidbodies.")]
        public ForceMode forceMode3D = ForceMode.Force;

        [Tooltip("The type of force to apply to 2D rigidbodies.")]
        public ForceMode2D forceMode2D = ForceMode2D.Force;

        // Dimension Settings
        [Tooltip("Determines whether to apply force to 2D or 3D rigidbodies.")]
        public ForceDimension dimension = ForceDimension.Mode3D;

        [Tooltip("The duration to apply the force when Time is set to Continuous. 0 or less means infinite.")]
        public float duration = 1f;

        [Tooltip("Delay before force is applied.")]
        [Min(0)]
        public float startDelay = 0f;

        // Target Settings
        [Tooltip("The root transform to search for rigidbodies. If null, it will affect all rigidbodies in the scene within range.")]
        public Transform targetRoot;

        private Rigidbody[] _rigidbodies3D;
        private Rigidbody2D[] _rigidbodies2D;
        private bool _isInitialized;

        void Initialize()
        {
            if (_isInitialized) return;
            FindRigidbodies();
            _isInitialized = true;
        }

        public void ApplyForce(float elapsedTime)
        {
            Initialize();

            if (elapsedTime < startDelay) return;

            if (duration > 0 && elapsedTime > startDelay + duration)
            {
                return;
            }
            
            ApplyForceToAll();
        }

        /// <summary>
        /// Finds all rigidbodies that can be affected by this force field.
        /// </summary>
        void FindRigidbodies()
        {
            if (targetRoot != null)
            {
                _rigidbodies3D = targetRoot.GetComponentsInChildren<Rigidbody>();
                _rigidbodies2D = targetRoot.GetComponentsInChildren<Rigidbody2D>();
            }
            else
            {
                var simulator = GetComponentInParent<PhysicsSimulator>();
                if (simulator)
                {
                    _rigidbodies3D = simulator.GetComponentsInChildren<Rigidbody>();
                    _rigidbodies2D = simulator.GetComponentsInChildren<Rigidbody2D>();
                }
                else
                {
                    var cutscene = GetComponentInParent<Cutscene>();
                    if (cutscene)
                    {
                        _rigidbodies3D = cutscene.GetComponentsInChildren<Rigidbody>();
                        _rigidbodies2D = cutscene.GetComponentsInChildren<Rigidbody2D>();
                    }
                }
            }
        }

        /// <summary>
        /// Applies the configured force to all relevant rigidbodies within the field.
        /// </summary>
        void ApplyForceToAll()
        {
            if (dimension == ForceDimension.Mode3D)
            {
                // 3D Rigidbodies
                foreach (var rb in _rigidbodies3D)
                {
                    if (rb && IsInField(rb.position))
                    {
                        Vector3 direction = GetForceDirection(rb.position);
                        float falloff = GetFalloffMultiplier(rb.position);
                        rb.AddForce(direction * forceMagnitude * falloff, forceMode3D);
                    }
                }
            }
            else // dimension == ForceDimension.Mode2D
            {
                // 2D Rigidbodies
                foreach (var rb in _rigidbodies2D)
                {
                    if (rb && IsInField(rb.position))
                    {
                        Vector3 direction3D = GetForceDirection(rb.position);
                        Vector2 direction2D = new Vector2(direction3D.x, direction3D.y);
                        float falloff = GetFalloffMultiplier(rb.position);
                        rb.AddForce(direction2D * forceMagnitude * falloff, forceMode2D);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the force multiplier based on the object's position within the field.
        /// </summary>
        /// <param name="position">The world position to check.</param>
        /// <returns>A multiplier (0-1) based on the forceFalloff curve.</returns>
        float GetFalloffMultiplier(Vector3 position)
        {
            Vector3 localPosition = transform.InverseTransformPoint(position);
            float normalizedDistance = 0f;

            switch (shape)
            {
                case ForceFieldShape.Sphere:
                case ForceFieldShape.Hemisphere:
                    if (radius > 0f)
                        normalizedDistance = Mathf.Clamp01(localPosition.magnitude / radius);
                    else
                        normalizedDistance = 0f;
                    break;

                case ForceFieldShape.Box:
                    if (boxSize.y > 0f)
                        normalizedDistance = Mathf.Clamp01(localPosition.y / boxSize.y);
                    else
                        normalizedDistance = 0f;
                    break;

                case ForceFieldShape.Cylinder:
                    if (length > 0f)
                        normalizedDistance = Mathf.Clamp01(localPosition.y / length);
                    else
                        normalizedDistance = 0f;
                    break;
            }

            return forceFalloff.Evaluate(normalizedDistance);
        }

        /// <summary>
        /// Checks if a given world position is inside the force field.
        /// </summary>
        /// <param name="position">The world position to check.</param>
        /// <returns>True if the position is inside the field, false otherwise.</returns>
        bool IsInField(Vector3 position)
        {
            Vector3 localPosition = transform.InverseTransformPoint(position);

            switch (shape)
            {
                case ForceFieldShape.Sphere:
                    return localPosition.magnitude <= radius;

                case ForceFieldShape.Hemisphere:
                    return localPosition.magnitude <= radius && Vector3.Dot(localPosition.normalized, Vector3.up) >= 0;

                case ForceFieldShape.Box:
                    return Mathf.Abs(localPosition.x) <= boxSize.x / 2f &&
                           localPosition.y >= 0f &&
                           localPosition.y <= boxSize.y &&
                           Mathf.Abs(localPosition.z) <= boxSize.z / 2f;

                case ForceFieldShape.Cylinder:
                    Vector2 planarPos = new Vector2(localPosition.x, localPosition.z);
                    return planarPos.magnitude <= radius &&
                           localPosition.y >= 0f &&
                           localPosition.y <= length;
            }
            return false;
        }

        /// <summary>
        /// Calculates the direction of the force for a given world position.
        /// </summary>
        /// <param name="position">The world position of the object being affected.</param>
        /// <returns>The calculated force direction in world space.</returns>
        Vector3 GetForceDirection(Vector3 position)
        {
            Vector3 localPosition = transform.InverseTransformPoint(position);
            Vector3 direction = Vector3.zero;

            switch (shape)
            {
                case ForceFieldShape.Sphere:
                case ForceFieldShape.Hemisphere:
                    direction = localPosition.normalized;
                    break;
                case ForceFieldShape.Box:
                    direction = Vector3.up; // Local Y direction
                    break;
                case ForceFieldShape.Cylinder:
                    direction = Vector3.up; // Local Y direction
                    break;
            }

            // Transform direction from local to world space
            return transform.TransformDirection(direction);
        }


    }
}
