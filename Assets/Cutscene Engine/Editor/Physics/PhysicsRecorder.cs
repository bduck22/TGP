using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEditor.Timeline;
using System.Collections.Generic;
using System.Linq;
using CutsceneEngine;
using Random = UnityEngine.Random;

namespace CutsceneEngineEditor
{
    public class PhysicsRecorder
    {
        readonly Transform _binding;
        readonly float _startTime;

        readonly Dictionary<string, List<Vector2>>_createdKeyframes = new (); // Keyframe 구조체를 Curve에 직접 추가하면 탄젠트 설정을 다시 해줘야해서 Vector2를 사용함.
        readonly AnimationClip _infiniteClip;
        readonly List<PhysicsSimulatorInspector.RigidbodyState> _states;
        
        float _lastTime;
        int _skippedKeyframes;
        public PhysicsRecorder(Transform binding, AnimationClip infiniteClip, List<PhysicsSimulatorInspector.RigidbodyState> states, float startTime)
        {
            _binding = binding;
            _startTime = startTime;
            _lastTime = startTime;
            _states = states;
            
            if (infiniteClip != null)
            {
                _infiniteClip = infiniteClip;
                var bindings = AnimationUtility.GetCurveBindings(infiniteClip);
                foreach (var b in bindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(infiniteClip, b);
                    foreach (var state in states)
                    {
                        if (state.relativePath == b.path)
                        {
                            state.curves[b.propertyName] = curve ?? new AnimationCurve();
                            break;
                        }
                    }
                }
            }
            Debug.Log($"Start Recording | startTime: {startTime}");
        }
        // 외부에서 호출할 메인 함수
        public void RecordKeyframes(Transform binding, float time)
        {
            if (_states == null) return;

            foreach (var state in _states)
            {
                if (state == null || state.transform == null) continue;
                
                Transform targetTransform = state.transform;
                
                if(!IsRelevantObject(binding, state)) continue;
                
                string path = GetRelativePath(binding, targetTransform);

                // 위치(Position)와 회전(Rotation) 값을 키프레임으로 기록
                SetKeyframe(path, state, "m_LocalPosition", targetTransform.localPosition, time);
                SetKeyframe(path, state, "m_LocalRotation", targetTransform.localRotation, time);
            }

            _lastTime = time;
            // RecordKeyframes에서는 더 이상 타임라인을 갱신하지 않음
        }

        public void CompleteRecording(CurveOptimizationSettings optimizationSettings)
        {
            if (_infiniteClip == null) return;
            // 모든 state의 curve를 AnimationClip에 한 번에 적용

            foreach (var state in _states)
            {
                string path = GetRelativePath(_binding, state.transform);
                
                foreach (var kvp in state.curves)
                {
                    string propertyName = kvp.Key;
                    AnimationCurve curve = kvp.Value;
                    var curveKey = $"{path}.{propertyName}";
                    
                    if(_infiniteClip != null)
                    {
                        for (int i = curve.length - 1; i >= 0; i--)
                        {
                            var key = curve.keys[i];
                            if (key.time < _startTime) continue;
                            if (key.time > _lastTime) continue;
                            curve.RemoveKey(i);
                        }
                    }

                    foreach (var keyframe in _createdKeyframes[curveKey])
                    {
                        curve.AddKey(keyframe.x, keyframe.y);
                    }

                    // for (int i = 0; i < curve.length; i++)
                    // {
                    //     AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                    //     AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                    // }
                    
                    // 곡선 최적화 수행 (constant tangent 키프레임 정보 전달)
                    AnimationCurveOptimizer.Optimize(curve, optimizationSettings, propertyName.Contains("Position") ? CurveType.Position : CurveType.Rotation);

                    EditorCurveBinding binding = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName);
                    AnimationUtility.SetEditorCurve(_infiniteClip, binding, curve);
                }
            }
            
            // 변경사항을 타임라인 윈도우에 반영
            TimelineEditor.Refresh(RefreshReason.SceneNeedsUpdate);
        }

        // 바인딩된 오브젝트가 시뮬레이션 오브젝트들의 부모이거나 자신인지 확인
        private bool IsRelevantObject(Transform binding, PhysicsSimulatorInspector.RigidbodyState state)
        {
            return state.transform == binding || state.transform.IsChildOf(binding);
        }


        // 특정 프로퍼티에 키프레임을 추가/수정하는 헬퍼 함수
        private void SetKeyframe(string path, PhysicsSimulatorInspector.RigidbodyState state, string propertyName, Vector3 value, float time)
        {
            // Vector3 값의 유효성 검사
            if (!IsValidVector3(value))
            {
                Debug.LogWarning($"Invalid Vector3 value for {propertyName} at time {time}: {value}. Skipping keyframe.");
                return;
            }
            
            SetCurveKey(path, state, propertyName + ".x", value.x, time);
            SetCurveKey(path, state, propertyName + ".y", value.y, time);
            SetCurveKey(path, state, propertyName + ".z", value.z, time);
        }

        private void SetKeyframe(string path, PhysicsSimulatorInspector.RigidbodyState state, string propertyName, Quaternion value, float time)
        {
            // Quaternion 값의 유효성 검사 및 정규화
            if (!IsValidQuaternion(value))
            {
                Debug.LogWarning($"Invalid Quaternion value for {propertyName} at time {time}: {value}. Using identity quaternion.");
                value = Quaternion.identity;
            }
            else
            {
                // Quaternion 정규화
                value = NormalizeQuaternion(value);
            }
            
            SetCurveKey(path, state, propertyName + ".x", value.x, time);
            SetCurveKey(path, state, propertyName + ".y", value.y, time);
            SetCurveKey(path, state, propertyName + ".z", value.z, time);
            SetCurveKey(path, state, propertyName + ".w", value.w, time);
        }

        // 값의 유효성을 검사하는 헬퍼 메서드들
        private bool IsValidVector3(Vector3 vector)
        {
            return !float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z) &&
                   !float.IsInfinity(vector.x) && !float.IsInfinity(vector.y) && !float.IsInfinity(vector.z);
        }
        
        private bool IsValidQuaternion(Quaternion quaternion)
        {
            return !float.IsNaN(quaternion.x) && !float.IsNaN(quaternion.y) && !float.IsNaN(quaternion.z) && !float.IsNaN(quaternion.w) &&
                   !float.IsInfinity(quaternion.x) && !float.IsInfinity(quaternion.y) && !float.IsInfinity(quaternion.z) && !float.IsInfinity(quaternion.w) &&
                   (quaternion.x != 0f || quaternion.y != 0f || quaternion.z != 0f || quaternion.w != 0f); // 영벡터가 아닌지 확인
        }
        
        private Quaternion NormalizeQuaternion(Quaternion quaternion)
        {
            float magnitude = Mathf.Sqrt(quaternion.x * quaternion.x + quaternion.y * quaternion.y + quaternion.z * quaternion.z + quaternion.w * quaternion.w);
            
            if (magnitude < 1e-6f) // 너무 작은 경우 identity로 대체
            {
                return Quaternion.identity;
            }
            
            return new Quaternion(
                quaternion.x / magnitude,
                quaternion.y / magnitude,
                quaternion.z / magnitude,
                quaternion.w / magnitude
            );
        }
        
        private bool IsValidFloat(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
        
        private void SetCurveKey(string path, PhysicsSimulatorInspector.RigidbodyState state, string property, float value, float time)
        {
            // 값의 유효성 검사
            if (!IsValidFloat(value))
            {
                Debug.LogWarning($"Invalid float value for {property} at time {time}: {value}. Skipping keyframe.");
                return;
            }
            
            // 시간 유효성 검사
            if (!IsValidFloat(time) || time < 0)
            {
                Debug.LogWarning($"Invalid time value for {property}: {time}. Skipping keyframe.");
                return;
            }
            
            // 값 변경 확인을 위한 키 생성
            string curveKey = $"{path}.{property}";

            state.curves.TryAdd(property, new AnimationCurve());

            List<Vector2> createdKeyframes;
            if (!_createdKeyframes.TryGetValue(curveKey, out createdKeyframes))
            {
                createdKeyframes = new List<Vector2>();
                _createdKeyframes[curveKey] = createdKeyframes;
            }
            
            
            // 현재 값으로 키프레임 추가
            createdKeyframes.Add(new Vector2(time, value));
        }
        
        
        // Root 오브젝트로부터 Target 오브젝트까지의 상대 경로를 구하는 함수
        public static string GetRelativePath(Transform root, Transform target)
        {
            if (root == target) return "";

            List<string> path = new List<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                path.Add(current.name);
                current = current.parent;
            }

            path.Reverse();
            return string.Join("/", path);
        }
    }
}