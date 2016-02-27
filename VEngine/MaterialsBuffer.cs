using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    class MaterialsBuffer
    {
        ShaderStorageBuffer Buffer;
        int Count = 0;

        public MaterialsBuffer()
        {
            Buffer = new ShaderStorageBuffer();
        }

        public void UseBuffer(uint point)
        {
            Buffer.Use(point);
        }

        public void Update(List<GenericMaterial> materials)
        {
            byte[] bytes = new byte[96 * materials.Count];
            int cursor = 0, cursor2 = 0;
            foreach(var mat in materials) {
                bset(SerializeMaterial(mat), ref bytes, cursor, 96);
                cursor += 96;
                mat.BufferOffset = cursor2;
                cursor2++;
            }
            Buffer.MapData(bytes);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            Count = materials.Count;
        }

        private static void bset(byte[] src, ref byte[] dest, int start, int count)
        {
            //Array.Copy(src, 0, dest, start, count);
            for(int i = 0; i < count; i++)
            {
                dest[i + start] = src[i]; 
            }
        }
        private byte[] SerializeMaterial(GenericMaterial mat)
        {
            /*
            vec4 diff 16
            vec4 spec 16
            float roughness 4
            float parallax height 4
            float roughness 4
            float roughness 4

            uvec2 dif 8
            uvec2 spec 8

            uvec2 aplh 8
            uvec2 rough 8

            uvec2 bump 8
            uvec2 norm 8

            totals 96 bytes
            */
            byte[] bytes = new byte[96];
            Array.Clear(bytes, 0, 96);
            int cursor = 0;
            bset(BitConverter.GetBytes(mat.DiffuseColor.X), ref bytes, cursor, 4);
            cursor += 4;
            bset(BitConverter.GetBytes(mat.DiffuseColor.Y), ref bytes, cursor, 4);
            cursor += 4;
            bset(BitConverter.GetBytes(mat.DiffuseColor.Z), ref bytes, cursor, 4);
            cursor += 4;
            bset(BitConverter.GetBytes(mat.DiffuseColor.X), ref bytes, cursor, 4);
            cursor += 4;

            bset(BitConverter.GetBytes(mat.SpecularColor.X), ref bytes, cursor, 4);
            cursor += 4;
            bset(BitConverter.GetBytes(mat.SpecularColor.Y), ref bytes, cursor, 4);
            cursor += 4;
            bset(BitConverter.GetBytes(mat.SpecularColor.Z), ref bytes, cursor, 4);
            cursor += 4;
            bset(BitConverter.GetBytes(mat.SpecularColor.X), ref bytes, cursor, 4);
            cursor += 4;

            bset(BitConverter.GetBytes(mat.Roughness), ref bytes, cursor, 4);
            cursor += 4;
            bset(BitConverter.GetBytes(mat.ParallaxHeightMultiplier), ref bytes, cursor, 4);
            cursor += 4;
            bset(BitConverter.GetBytes(mat.Roughness), ref bytes, cursor, 4);
            cursor += 4;
            bset(BitConverter.GetBytes(mat.Roughness), ref bytes, cursor, 4);
            cursor += 4;

            if(mat.DiffuseTexture != null)
                bset(mat.DiffuseTexture.SerializeBindlessHandle(), ref bytes, cursor, 8);
            cursor += 8;
            if(mat.SpecularTexture != null)
                bset(mat.SpecularTexture.SerializeBindlessHandle(), ref bytes, cursor, 8);
            cursor += 8;
            if(mat.AlphaTexture != null)
                bset(mat.AlphaTexture.SerializeBindlessHandle(), ref bytes, cursor, 8);
            cursor += 8;
            if(mat.RoughnessTexture != null)
                bset(mat.RoughnessTexture.SerializeBindlessHandle(), ref bytes, cursor, 8);
            cursor += 8;
            if(mat.BumpTexture != null)
                bset(mat.BumpTexture.SerializeBindlessHandle(), ref bytes, cursor, 8);
            cursor += 8;
            if(mat.NormalsTexture != null)
                bset(mat.NormalsTexture.SerializeBindlessHandle(), ref bytes, cursor, 8);
            cursor += 8;

            return bytes;
        }


    }
}
