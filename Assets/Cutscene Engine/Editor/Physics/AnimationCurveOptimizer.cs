using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CutsceneEngine;

namespace CutsceneEngineEditor
{
    internal enum CurveType
    {
        Position,
        Rotation
    }
    internal static class AnimationCurveOptimizer
    {
        public static void Optimize(AnimationCurve curve, CurveOptimizationSettings settings, CurveType curveType)
        {
            if (curve == null || curve.length < 3) return;

            settings ??= new CurveOptimizationSettings();
            var step = curve.keys[^1].time / curve.length;
            var valueThreshold = curveType == CurveType.Position ? settings.positionThreshold : settings.angleThreshold;
            for (int iteration = 0; iteration < settings.iteration; iteration++)
            {
                var keepKeys = new bool[curve.length];
                keepKeys[0] = true;
                keepKeys[^1] = true;

                for (int i = 1; i < curve.length - 1; i++)
                {
                    var key = curve[i];
                    var prevKey = curve[i - 1];
                    var nextKey = curve[i + 1];
            
                    var dirIn = new Vector2(key.time, key.value) - new Vector2(prevKey.time, prevKey.value);
                    var dirOut = new Vector2(nextKey.time, nextKey.value) - new Vector2(key.time, key.value);

                    dirIn.Normalize();
                    dirOut.Normalize();

                    var angle = Vector2.Angle(dirIn, dirOut);

                    if(Mathf.Abs(prevKey.value - key.value) >= valueThreshold || 
                       Mathf.Abs(key.value - nextKey.value) >= valueThreshold)
                        keepKeys[i] = true;
                
                    if (angle >= settings.curvatureThreshold)
                    {
                        keepKeys[i] = true;
                    }
                }
            
                for (int i = curve.length - 1; i > 0; i--)
                {
                    if (keepKeys[i] == false)
                    {
                        curve.RemoveKey(i);
                    }
                }

                
                for (int i = 1; i < curve.length; i++)
                {
                    var key = curve[i];
                    var prevKey = curve[i - 1];
                    
                    if (key.time - prevKey.time > step * 2.5f && Mathf.Abs(prevKey.value - key.value) < valueThreshold)
                    {
                        AnimationUtility.SetKeyBroken(curve, i-1, true);
                        AnimationUtility.SetKeyBroken(curve, i, true);
                        AnimationUtility.SetKeyRightTangentMode(curve, i-1, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                        
                        curve.keys[i - 1] = prevKey;
                        curve.keys[i] = key;
                    }
                }
            }
        }

    }
}