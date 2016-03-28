using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VEngine
{
    /*
    AbsLight

    P Shadowmapping on off 
    P Transformation
    P Color
    P f32 Cutoff 

    T PointLight
    T SpotLight
     P Angle
    
    */
    public class Light : ITransformable
    {

        private ShadowMapper Mapper = new ShadowMapper();

        public enum ShadowMapTypeEnum
        {
            Single,
            Cubemap
        }

        public TransformationManager Transformation;
        public bool ShadowMappingEnabled = false;
        public bool StaticShadowMap = false;
        public bool ShadowMapRefreshNeeded = true;
        public float CutOffDistance = 100.0f;
        public float Angle = OpenTK.MathHelper.DegreesToRadians(90.0f);
        public Vector3 Color = Vector3.One;

        private Matrix4 LastVPMatrix = Matrix4.Identity;

        public ShadowMapTypeEnum ShadowMapType = ShadowMapTypeEnum.Single;
        public ShadowMapQuality ShadowsQuality = ShadowMapQuality.Medium;

        public Light(TransformationManager trans)
        {
            Transformation = trans;
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }

        public void UpdateShadowMap()
        {
            if(!StaticShadowMap || (StaticShadowMap && ShadowMapRefreshNeeded))
            {
                if(ShadowMapType == ShadowMapTypeEnum.Single)
                {
                    LastVPMatrix = Mapper.MapSingle(ShadowsQuality, Transformation, Angle, CutOffDistance);
                }
                else if(ShadowMapType == ShadowMapTypeEnum.Cubemap)
                {
                    Mapper.MapCube(ShadowsQuality, Transformation, CutOffDistance);
                }
                ShadowMapRefreshNeeded = false;
            }
        }

        public void SetUniforms()
        {
            var s = ShaderProgram.Current;
            s.SetUniform("LightColor", Color);
            s.SetUniform("LightPosition", Transformation.Position);
            s.SetUniform("LightOrientation", Transformation.Orientation);
            s.SetUniform("LightAngle", Angle);
            s.SetUniform("LightCutOffDistance", CutOffDistance);
            s.SetUniform("LightUseShadowMap", ShadowMappingEnabled);
            s.SetUniform("LightShadowMapType", ShadowMapType == ShadowMapTypeEnum.Single ? 0 : 1);
            s.SetUniform("LightVPMatrix", LastVPMatrix);
        }

        public void BindShadowMap(int single, int cube)
        {
            if(ShadowMapType == ShadowMapTypeEnum.Single)
            {
                Mapper.UseTextureSingle(ShadowsQuality, single);
            }
            else if(ShadowMapType == ShadowMapTypeEnum.Cubemap)
            {
                Mapper.UseTextureCube(ShadowsQuality, cube);
            }
        }
    }
}
