using System;
using System.Linq;
#if TMP
using TMPro;
#endif
using UnityEngine;
using UnityEngine.Playables;
#if URP
using UnityEngine.Rendering.Universal;
#endif
#if HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.Timeline;
using UnityEngine.UI;
using UnityEngine.UIElements;
#if VFX
using UnityEngine.VFX;
#endif


namespace CutsceneEngine
{
    [TrackColor(1f, 0f, 1f)]
    [TrackClipType(typeof(ColorClip))]
    [TrackBindingType(typeof(GameObject))]
    public class ColorTrack : TrackAsset
    {
        [Tooltip("If this value is true, " +
                 "the existing color will be multiplied by the color of the clip, if false, it will be replaced.")]
        public bool isTint;
        [Tooltip("The index of the material in the material array of a given renderer to which to apply the color change. " +
                 "If this value is less than 0, the change will be applied to all materials.")]
        public int materialIndex = -1;
        [Tooltip("The name of the color property of the material. " +
                 "If this field is empty, it will perform changes to material.color.")]
        public string propertyName = "_BaseColor";

        [Tooltip("The name or path of the UI Element to which you want to apply the color change. " +
                 "If there are multiple elements with the same name, enter the path using '/' as a separator.")]
        public string elementName = "UI_ELEMENT_NAME";
        [Tooltip("The property to apply color changes to in a UI Element.")]
        public UIElementColorTarget uiElementColorTarget;
        
        [Tooltip("For UI graphic elements or SpriteRenderer, if you are using a custom material, decide whether to apply color to the UI element or the material. " +
                 "If this value is true, apply color to a property of the material.")]
        public bool applyToMaterialProperty;
#if URP || HDRP
        [Tooltip("If this value is true, the alpha value of the color is also applied to the Opacity of the DecalProjector.")]
        public bool applyAlphaToDecalOpacity = true;
#endif


        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.displayName = " ";
            clip.duration = 1;
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var playable = ScriptPlayable<ColorMixerBehaviour>.Create(graph, inputCount);
            var behaviour = playable.GetBehaviour();
            behaviour.isTint = isTint;
#if URP || HDRP
            behaviour.applyAlphaToDecalOpacity = applyAlphaToDecalOpacity;
#endif
            behaviour.applyToMaterialProperty = applyToMaterialProperty;
            if (go)
            {
                if (go.TryGetComponent(out PlayableDirector director))
                {
                    var binding = director.GetGenericBinding(this) as GameObject;
                    if(binding)
                    {
                        if (binding.TryGetComponent(out Graphic g))
                        {
                            behaviour.graphic = g;
                            behaviour.targetType = ColorTargetType.UIGraphc;
                            
                            
                            if(g.material == g.defaultMaterial || !applyToMaterialProperty)
                            {
                                behaviour.initialColor = g.color;
                            }
                            else
                            {
                                behaviour.originalMaterial = g.material;
                                behaviour.propertyID = Shader.PropertyToID(propertyName);
                                behaviour.initialColor = g.material.GetColor(behaviour.propertyID);
                            }
                        }
#if VFX
                        else if (binding.TryGetComponent(out VisualEffect vfx))
                        {
                            behaviour.targetType = ColorTargetType.VFX;
                            behaviour.vfx = vfx;
                            behaviour.propertyID = Shader.PropertyToID(propertyName);
                            behaviour.initialColor = vfx.GetVector4(behaviour.propertyID);
                        }
#endif
                        else if (binding.TryGetComponent(out UIDocument doc))
                        {
                            behaviour.targetType = ColorTargetType.UIElement;
                            behaviour.uiElementColorTarget = uiElementColorTarget;
                            behaviour.uiDocument = doc;
                            behaviour.elementName = elementName;
                        }
#if URP || HDRP
                        else if (binding.TryGetComponent(out DecalProjector decal))
#else
                        else if (binding.TryGetComponent(out Projector decal))
#endif
                        
                        {
                            behaviour.targetType = ColorTargetType.Decal;
                            behaviour.decal = decal;
                            behaviour.originalMaterial = decal.material;

                            if (string.IsNullOrWhiteSpace(propertyName))
                            {
                                behaviour.initialColor = decal.material.color;
                            }
                            else
                            {
                                behaviour.propertyID = Shader.PropertyToID(propertyName);
                                if(decal.material.HasColor(behaviour.propertyID)) behaviour.initialColor = decal.material.GetColor(behaviour.propertyID);
                            }
                        }
                        else if (binding.TryGetComponent(out SpriteRenderer sr))
                        {
                            behaviour.targetType = ColorTargetType.SpriteRenderer;
                            behaviour.spriteRenderer = sr;
                            behaviour.initialColor = sr.color;
                            
                            if(applyToMaterialProperty)
                            {
                                behaviour.originalMaterial = sr.sharedMaterial;
                                behaviour.propertyID = Shader.PropertyToID(propertyName);
                                behaviour.initialColor = sr.sharedMaterial.GetColor(behaviour.propertyID);
                            }
                            else
                            {
                                behaviour.initialColor = sr.color;
                            }
                        }
                        else if (binding.TryGetComponent(out Renderer r))
                        {
                            behaviour.targetType = ColorTargetType.Renderer;
                            behaviour.originalMaterials = r.sharedMaterials;
                            behaviour.materialIndex = materialIndex;
                            behaviour.renderer = r;
                            if (materialIndex > -1)
                            {
                                if (string.IsNullOrWhiteSpace(propertyName) || !r.sharedMaterials[materialIndex].HasColor(propertyName))
                                {
                                    behaviour.initialColor = r.sharedMaterials[materialIndex].color;    
                                }
                                else
                                {
                                    behaviour.initialColor = r.sharedMaterials[materialIndex].GetColor(propertyName);
                                }
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(propertyName))
                                {
                                    behaviour.initialColors = r.sharedMaterials.Select(x => x.color).ToArray();
                                }
                                else
                                {
                                    behaviour.propertyID = Shader.PropertyToID(propertyName);
                                    behaviour.initialColors = r.sharedMaterials.Select(x =>
                                    {
                                        if (x.HasColor(behaviour.propertyID)) return x.GetColor(behaviour.propertyID);
                                        return x.color;
                                    }).ToArray();
                                }
                            }
                        }
                        else
                        {
                            behaviour.targetType = ColorTargetType.InValid;
                        }
                    }
                }
            }
            
            
            return playable;
        }

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            var c = clip.asset as ColorClip;
            c.start = clip.start;
            c.end = clip.end;
            return base.CreatePlayable(graph, gameObject, clip);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var binding = director.GetGenericBinding(this) as GameObject;
            if(binding)
            {
                driver.AddFromName<SpriteRenderer>(binding.gameObject, "m_Color");
                driver.AddFromName<Graphic>(binding.gameObject, "m_Color");
                driver.AddFromName<Graphic>(binding.gameObject, "m_Material");
#if URP || HDRP
                if(applyAlphaToDecalOpacity) driver.AddFromName<DecalProjector>(binding.gameObject, "m_FadeFactor");
#endif
                
#if TMP
                driver.AddFromName<TMP_Text>(binding.gameObject, "m_fontColor");
#endif
            }
        }
    }
}