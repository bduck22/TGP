using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace CutsceneEngine
{
    public static class CutsceneEngineUtility
    {
        internal static void SmartDestroy(Object target)
        {
#if UNITY_EDITOR
            if(!Application.isPlaying) Object.DestroyImmediate(target);
            else
#endif
            Object.Destroy(target);
        }

        
        public static IEnumerable<T> GetTracks<T>(this PlayableDirector director) where T : TrackAsset
        {
            foreach (var playableBinding in director.playableAsset.outputs)
            {
                if (playableBinding.sourceObject is T track) yield return track;
            }
        }

        public static T GetTrack<T>(this PlayableDirector director) where T : TrackAsset
        {
            foreach (var playableBinding in director.playableAsset.outputs)
            {
                if(playableBinding.sourceObject is T track) return track;
            }

            return null;
        }
        public static T GetTrack<T>(this PlayableDirector director, Predicate<T> predicate) where T : TrackAsset
        {
            foreach (var playableBinding in director.playableAsset.outputs)
            {
                if(playableBinding.sourceObject is T track && predicate.Invoke(track)) return track;
            }

            return null;
        }

        public static T GetTrackOf<T>(this PlayableDirector director, PlayableAsset clip) where T : TrackAsset
        {
            foreach (var playableBinding in director.playableAsset.outputs)
            {
                if(playableBinding.sourceObject is T track)
                {
                    foreach (var timelineClip in track.GetClips())
                    {
                        if (timelineClip.asset == clip) return track;
                    }
                }
            }

            return null;
        }
        public static T GetTrackOf<T>(this TimelineAsset asset, PlayableAsset clip) where T : TrackAsset
        {
            foreach (var binding in asset.outputs)
            {
                if(binding.sourceObject is T track)
                {
                    foreach (var timelineClip in track.GetClips())
                    {
                        if (timelineClip.asset == clip) return track;
                    }
                }
            }

            return null;
        }
        

        public static double GetNormalizedTime(this TimelineClip clip, double time)
        {
            return GetNormalizedTime(time, clip.start, clip.end);
        }
        public static double GetNormalizedTime(double time, double start, double end)
        {
            if (time <= start) return 0f;
            if (time >= end) return 1f;
            return (time - start) / (end - start);
        }


        public static double GetTimelineTime(this TimelineClip clip, float normalizedTime)
        {
            return GetTimelineTime(normalizedTime, clip.start, clip.end);
        }
        public static double GetTimelineTime(float normalizedTime, double start, double end)
        {
            return start + normalizedTime * (end - start);
        }
    }
}