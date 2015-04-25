using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEngine
{
    public class GraphicsSettings
    {
        public float HBAOContribution
        {
            get;
            set;
        }
        public float GIContribution
        {
            get;
            set;
        }
        public float MainLightAttentuation
        {
            get;
            set;
        }
        public float SimpleLightAttentuation
        {
            get;
            set;
        }
        public float FogContribution
        {
            get;
            set;
        }
        public float FogSamples
        {
            get;
            set;
        }
        public float GISamples
        {
            get;
            set;
        }
        public float HBAOSamples
        {
            get;
            set;
        }
        public float ShadowsBlur
        {
            get;
            set;
        }
        public float ShadowsSamples
        {
            get;
            set;
        }
        public float LightPointSize
        {
            get;
            set;
        }
        public float SimpleLightPointSize
        {
            get;
            set;
        }
        public float BloomSamples
        {
            get;
            set;
        }
        public float BloomSize
        {
            get;
            set;
        }
        public float BloomContribution
        {
            get;
            set;
        }
        public float GIDiffuseComponent
        {
            get;
            set;
        }
        public float HBAOStrength
        {
            get;
            set;
        }

        public GraphicsSettings()
        {
            HBAOContribution = 1.0f;
            GIContribution = 1.0f;
            MainLightAttentuation = 390.0f;
            SimpleLightAttentuation = 200.0f;
            FogContribution = 1.0f;
            FogSamples = 50;
            GISamples = 100;
            HBAOSamples = 12.5f;
            ShadowsBlur = 1.0f;
            ShadowsSamples = 6;
            LightPointSize = 1.0f;
            SimpleLightPointSize = 1.0f;
            BloomSamples = 16;
            BloomSize = 1.0f;
            BloomContribution = 1.0f;
            GIDiffuseComponent = 0.1f;
            HBAOStrength = 9.0f;
        }

        public void SetUniforms(ShaderProgram program)
        {
            program.SetUniform("HBAOContribution", HBAOContribution);
            program.SetUniform("GIContribution", GIContribution);
            program.SetUniform("MainLightAttentuation", MainLightAttentuation);
            program.SetUniform("SimpleLightAttentuation", SimpleLightAttentuation);
            program.SetUniform("FogContribution", FogContribution);
            program.SetUniform("FogSamples", 1.0f / FogSamples);
            program.SetUniform("GISamples", GISamples);
            program.SetUniform("HBAOSamples", HBAOSamples);
            program.SetUniform("ShadowsBlur", ShadowsBlur);
            program.SetUniform("ShadowsSamples", 1.0f / ShadowsSamples);
            program.SetUniform("LightPointSize", LightPointSize);
            program.SetUniform("SimpleLightPointSize", SimpleLightPointSize);
            program.SetUniform("BloomSamples", 1.0f / BloomSamples);
            program.SetUniform("BloomSize", BloomSize);
            program.SetUniform("BloomContribution", BloomContribution);
            program.SetUniform("GIDiffuseComponent", GIDiffuseComponent);
            program.SetUniform("HBAOStrength", HBAOStrength);
        }

    }
}
