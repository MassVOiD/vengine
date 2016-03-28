#version 430 core

layout(location = 0) out vec4 outColor;

layout (binding = 6, r32ui)  uniform uimage3D VoxelsTextureRed;
layout (binding = 1, r32ui)  uniform uimage3D VoxelsTextureGreen;
layout (binding = 2, r32ui)  uniform uimage3D VoxelsTextureBlue;
layout (binding = 3, r32ui)  uniform uimage3D VoxelsTextureCount;
uniform float BoxSize;
uniform int GridSize;

// xyz in range 0 -> 1
void WriteColor3d(vec3 xyz, vec3 color){
    uint r = uint(color.r * 128);
    uint g = uint(color.g * 128);
    uint b = uint(color.b * 128);
    imageAtomicAdd(VoxelsTextureRed, ivec3(xyz * float(GridSize)), r);
    imageAtomicAdd(VoxelsTextureGreen, ivec3(xyz * float(GridSize)), g);
    imageAtomicAdd(VoxelsTextureBlue, ivec3(xyz * float(GridSize)), b);
    imageAtomicAdd(VoxelsTextureCount, ivec3(xyz * float(GridSize)), 1);
    //memoryBarrier();
}

uniform int DisablePostEffects;
uniform float VDAOGlobalMultiplier;

#include LogDepth.glsl

vec2 UV = gl_FragCoord.xy / resolution.xy;


FragmentData currentFragment;

#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
#include Direct.glsl
#include AmbientOcclusion.glsl
#include RSM.glsl
#include EnvironmentLight.glsl

#include ParallaxOcclusion.glsl


void main(){
    vec3 dcolor = DiffuseColor;

	vec2 UVx = Input.TexCoord;
    
	if(UseDiffuseTex) dcolor = texture(diffuseTex, UVx).rgb; 
    
    vec3 hafbox = ToCameraSpace(Input.WorldPos) / BoxSize;
    hafbox = clamp(hafbox, -1.0, 1.0);
    WriteColor3d(hafbox * 0.5 + 0.5, dcolor);
	
	outColor = vec4(dcolor, 0.2);
}