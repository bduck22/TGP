using System.Linq;
using CutsceneEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
#if URP
using UnityEngine.Rendering.Universal;
#endif
#if HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.Timeline;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.VFX;

namespace CutsceneEngineEditor
{
    [CustomTimelineEditor(typeof(ColorTrack))]
    public class ColorTrackEditor : TrackEditor
    {
        static Texture2D icon;
        public override TrackDrawOptions GetTrackOptions(TrackAsset track, Object binding)
        {
            var option = base.GetTrackOptions(track, binding);

            if (!icon) icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Cutscene Engine/Editor/Icons/Color Icon.png");
            option.icon = icon;

            var t = (ColorTrack)track;
            var b = binding as GameObject;
            if(b)
            {
                if (b.TryGetComponent(out Graphic g))
                {
                    if (t.applyToMaterialProperty && !g.material.HasColor(t.propertyName))
                    {
                        option.errorText = $"Cannot find color property named '{t.propertyName}'";
                    }
                }
                else if (b.TryGetComponent(out UIDocument doc))
                {
                    var elementPath = t.elementName;
                    var paths = elementPath.Split('/');
                    VisualElement ve = null;
                    if(paths.Length > 0)
                    {
                        for (int i = 0; i < paths.Length; i++)
                        {
                            ve = doc.rootVisualElement.Q(paths[i]);
                        }
                        
                        if (ve == null)
                        {
                            option.errorText = $"Cannot find element at path: {elementPath}";
                        }
                    }
                    else
                    {
                        ve = doc.rootVisualElement.Q(elementPath);
                        
                        if (ve == null)
                        {
                            option.errorText = $"Cannot find element: {elementPath}";
                        }
                    }
                    
                    
                }
#if URP || HDRP
                else if (b.TryGetComponent(out DecalProjector decal))
#else
                else if (b.TryGetComponent(out Projector decal))
#endif
                {
                    var mat = decal.material;
                    if(!string.IsNullOrWhiteSpace(t.propertyName))
                    {
                        if (!mat.HasColor(t.propertyName)) option.errorText = $"Cannot find color property named '{t.propertyName}'";
                    }
                }
#if VFX
                else if (b.TryGetComponent(out VisualEffect vfx))
                {
                    if (string.IsNullOrWhiteSpace(t.propertyName))
                    {
                        option.errorText = "You should set the property name of the color property";
                    }
                    else
                    {
                        if (!vfx.HasVector4(t.propertyName))
                        {
                            option.errorText = $"Cannot find Color(Vector4) property named '{t.propertyName}'";
                        }
                    }
                }
#endif
                
                else if (b.TryGetComponent(out Light l))
                {
                    option.errorText = "Please use LightTrack to change the color of the light.";
                }
                else if (b.TryGetComponent(out SpriteRenderer sr))
                {
                    if (t.applyToMaterialProperty && !g.material.HasColor(t.propertyName))
                    {
                        option.errorText = $"Cannot find color property named '{t.propertyName}'";
                    }
                }
                else if (b.TryGetComponent(out Renderer r))
                {
                    if (t.materialIndex < 0)
                    {
                        if (r.sharedMaterials.Any(x => !x.HasColor(t.propertyName)))
                        {
                            option.errorText = $"Cannot find any materials that uses a color property named '{t.propertyName}'";
                        }
                    }
                    else
                    {
                        if (t.materialIndex > r.sharedMaterials.Length - 1)
                        {
                            option.errorText = $"materialIndex is out of range. It must be less than {r.sharedMaterials.Length - 1}";
                        }
                        else
                        {
                            var mat = r.sharedMaterials[t.materialIndex];
                            if(!string.IsNullOrWhiteSpace(t.propertyName))
                            {
                                if (!mat.HasColor(t.propertyName)) option.errorText = $"Cannot find color property named '{t.propertyName}'";
                            }
                        }
                    }
                }
                else
                {
                    // invalid
                    option.errorText = "Cannot find component that uses color properties";
                }
            }
            return option;
        }
    }
}