using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    internal class InstancedLitShader : BaseShaderGUI
    {
        static readonly string[] workflowModeNames = Enum.GetNames(typeof(LitGUI.WorkflowMode));

        private LitGUI.LitProperties litProperties;
        private LitDetailGUI.LitProperties litDetailProperties;
        MaterialProperty instancedProp;
        MaterialProperty bottomColorProp;
        MaterialProperty bottomColorHeightProp;
        MaterialProperty terrainBlendProp;
        MaterialProperty swaySpeedProp;
        MaterialProperty grassStiffnessProp;
        MaterialProperty windIntensityProp;
        MaterialProperty windTextureProp;
        MaterialProperty windTextureScaleProp;
        MaterialProperty windTextureSpeedProp;
        MaterialProperty windContrastProp;
        MaterialProperty windDirectionProp;
        MaterialProperty windHighLightProp;
        public override void FillAdditionalFoldouts(MaterialHeaderScopeList materialScopesList)
        {
            materialScopesList.RegisterHeaderScope(LitDetailGUI.Styles.detailInputs, Expandable.Details, _ => LitDetailGUI.DoDetailArea(litDetailProperties, materialEditor));
        }

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            litProperties = new LitGUI.LitProperties(properties);
            litDetailProperties = new LitDetailGUI.LitProperties(properties);

            instancedProp = BaseShaderGUI.FindProperty("_Instanced", properties);
            bottomColorProp = BaseShaderGUI.FindProperty("_BottomColor", properties);
            bottomColorHeightProp = BaseShaderGUI.FindProperty("_BottomColorHeight", properties);
            terrainBlendProp = BaseShaderGUI.FindProperty("_TerrainBlend", properties);
            swaySpeedProp = BaseShaderGUI.FindProperty("_SwaySpeed", properties);
            grassStiffnessProp = BaseShaderGUI.FindProperty("_GrassStiffness", properties);
            windIntensityProp = BaseShaderGUI.FindProperty("_WindIntensity", properties);
            windTextureProp = BaseShaderGUI.FindProperty("_WindTexture", properties);
            windTextureScaleProp = BaseShaderGUI.FindProperty("_WindTextureScale", properties);
            windTextureSpeedProp = BaseShaderGUI.FindProperty("_WindTextureSpeed", properties);
            windContrastProp = BaseShaderGUI.FindProperty("_WindContrast", properties);
            windDirectionProp = BaseShaderGUI.FindProperty("_WindDirection", properties);
            windHighLightProp = BaseShaderGUI.FindProperty("_WindHighLight", properties);
        }

        // material changed check
        public override void ValidateMaterial(Material material)
        {
            SetMaterialKeywords(material, LitGUI.SetMaterialKeywords, LitDetailGUI.SetMaterialKeywords);
            if(material.HasProperty("_Instanced"))
            {
                float x = material.GetFloat("_Instanced");
                CoreUtils.SetKeyword(material, "INSTANCED_ENABLE", x >=1.0f);
            }
        }

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            if (litProperties.workflowMode != null)
                DoPopup(LitGUI.Styles.workflowModeText, litProperties.workflowMode, workflowModeNames);

            base.DrawSurfaceOptions(material);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            materialEditor.ColorProperty(bottomColorProp, "Bottom Color");
            materialEditor.RangeProperty(bottomColorHeightProp, "Bottom Color Height");
            materialEditor.ColorProperty(windHighLightProp, "Wind HighLight Color");
            materialEditor.RangeProperty(terrainBlendProp, "Terrain Blend");
            materialEditor.VectorProperty(swaySpeedProp, "Sway Speed");
            materialEditor.FloatProperty(grassStiffnessProp, "Grass Stiffness");
            materialEditor.RangeProperty(windIntensityProp, "Wind Intensity");
            materialEditor.TextureProperty(windTextureProp, "Wind Texture");
            materialEditor.VectorProperty(windTextureScaleProp, "Wind Scale");
            materialEditor.VectorProperty(windTextureSpeedProp, "Wind Speed");
            materialEditor.VectorProperty(windContrastProp, "Wind Contrast");
            materialEditor.FloatProperty(windDirectionProp, "Wind Direction");
            LitGUI.Inputs(litProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        public static GUIContent instancedText =
               EditorGUIUtility.TrTextContent("Instanced Indirect",
                   "X");

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            if (litProperties.reflections != null && litProperties.highlights != null)
            {
                materialEditor.ShaderProperty(litProperties.highlights, LitGUI.Styles.highlightsText);
                materialEditor.ShaderProperty(litProperties.reflections, LitGUI.Styles.reflectionsText);
                materialEditor.ShaderProperty(instancedProp, instancedText);
            }

            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Blend", (float)blendMode);

            material.SetFloat("_Surface", (float)surfaceType);
            if (surfaceType == SurfaceType.Opaque)
            {
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            if (oldShader.name.Equals("Standard (Specular setup)"))
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Specular);
                Texture texture = material.GetTexture("_SpecGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
            else
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Metallic);
                Texture texture = material.GetTexture("_MetallicGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
        }
    }
}
