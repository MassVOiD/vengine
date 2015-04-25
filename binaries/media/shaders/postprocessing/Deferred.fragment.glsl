#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 29) uniform sampler2D backWorldPosTex;
layout(binding = 30) uniform sampler2D worldPosTex;
layout(binding = 31) uniform sampler2D normalsTex;

const int MAX_SIMPLE_LIGHTS = 2000;
uniform int SimpleLightsCount;
uniform vec3 SimpleLightsPos[MAX_SIMPLE_LIGHTS];
uniform vec4 SimpleLightsColors[MAX_SIMPLE_LIGHTS];

out vec4 outColor;

float testVisibility(vec2 uv1, vec2 uv2, vec3 lpos) {
	vec3 d3d1Front = texture(worldPosTex, uv1).rgb;
	vec3 d3d2 = lpos;
	float ovis = 1;
	for(float i=0;i<1.0;i+= 0.01) { 
		vec2 ruv = mix(uv1, uv2, i);
		if(ruv.x < 0 || ruv.x > 1 || ruv.y < 0 || ruv.y > 1) continue;
		float rd3dFront = distance(CameraPosition, texture(worldPosTex, ruv).rgb);
		if(rd3dFront < distance(CameraPosition, mix(d3d1Front, d3d2, i))) {
			ovis -= 0.09;
			if(ovis <= 0) return 0;
		}
	}
	return ovis;
}

vec2 refractUV(){
	vec3 rdir = normalize(CameraPosition - texture(worldPosTex, UV).rgb);
	vec3 crs1 = normalize(cross(CameraPosition, texture(worldPosTex, UV).rgb));
	vec3 crs2 = normalize(cross(crs1, rdir));
	vec3 rf = refract(rdir, texture(normalsTex, UV).rgb, 0.6);
	return UV - vec2(dot(rf, crs1), dot(rf, crs2)) * 0.3;
}


#define mPI (3.14159265)
#define mPIo2 (3.14159265*0.5)
#define mPIo4 (3.14159265*0.25)
#define mPI2 (2*3.14159265)
// fast hbao by Adrian Chlubek (afl_ext) @ 25.04.2015
vec3 GoodHBAO() 
{
	// get original diffuse color
	vec3 originalColor = texture(texColor, UV).rgb * 0.1;
	// get center pixel world position
	vec3 centerPosition = texture(worldPosTex, UV).rgb;  
	// get a position 'above' the surface, by adding multiplied normal to world position
	// this could also be a directional light inverted direction + world pos
	vec3 lookupPosition = centerPosition + texture(normalsTex, UV).rgb * 15.0;
	// get distance from center position to sampling (above) position
	float A = distance(lookupPosition, centerPosition);
	// calculate how big area of texture should be sampled
	float AInv = 1.0 / (distance(CameraPosition, centerPosition) + 1.0);
	// create some boring variables
	vec3 outc = vec3(0);
	int counter = 0;
	float minval = 2;
	// two loops, one for doing circles, and second for circle radius
	for(float g = 0.05; g < mPI2; g += 0.4) 
	{
		minval = 2;
		for(float g2 = 0.05; g2 < 1; g2 += 0.08) 
		{ 			
			// calculate lookup UV
			vec2 coord = UV + (vec2(sin(g), cos(g)) * ((g2) * 2.0 * AInv));
			// get position of pixel under that UV
			vec3 coordPosition = texture(worldPosTex, coord).rgb;  
			// calculate distance from that position and sampling (above center) position
			float B = distance(lookupPosition, coordPosition);
			// calculate distance from that position to center pixel world position
			float C = distance(coordPosition, centerPosition);
			// skip too far away pixels
			if(C > 0.9) continue;
			// because 3 triangle sides are known, calculate free horizon angle 
			float angle = acos( (C*C + A*A - B*B ) / (2 * C * A) );
			// fix too bright outer corners
			if(angle > mPIo2)angle = mPIo2;
			// add color multiplied by angle, adjusted by powering 
			if(minval > angle){
				counter++;
				minval = angle;
				if(angle != 0 && angle > 0) {
					outc += originalColor * (pow(angle / mPIo2, 9));
				}
			}
		}	
	}
	// return final color
	return outc /counter;
}

void main()
{	
	float alpha = texture(texColor, UV).a;
	vec2 nUV = UV;
	if(alpha < 0.99){
		//nUV = refractUV();
	}
	vec3 colorOriginal = texture(texColor, nUV).rgb;
	vec3 color1 = GoodHBAO() ;	
	if(texture(texColor, UV).a < 0.99){
	    color1 += texture(texColor, UV).rgb * texture(texColor, UV).a;
	}
	gl_FragDepth = texture(texDepth, nUV).r;
	vec4 fragmentPosWorld3d = texture(worldPosTex, nUV);
	vec4 normal = texture(normalsTex, nUV);
	if(normal.a == 0.0){
		color1 = colorOriginal;
	} else {
			
		vec3 cameraRelativeToVPos = CameraPosition - fragmentPosWorld3d.xyz;
		for(int i=0;i<LightsCount;i++){

		
			float distanceToLight = distance(fragmentPosWorld3d.xyz, LightsPos[i]);
            //if(worldDistance < 0.0002) continue;
			float att = 1.0 / pow(((distanceToLight/1.0) + 1.0), 2.0) * 390.0;
			mat4 lightPV = (LightsPs[i] * LightsVs[i]);
			
			
			vec3 lightRelativeToVPos = LightsPos[i] - fragmentPosWorld3d.xyz;
			vec3 R = reflect(lightRelativeToVPos, normal.xyz);
			float cosAlpha = -dot(normalize(cameraRelativeToVPos), normalize(R));
			float specularComponent = clamp(pow(cosAlpha, 80.0 / normal.a), 0.0, 1.0) * fragmentPosWorld3d.a;


			lightRelativeToVPos = LightsPos[i] - fragmentPosWorld3d.xyz;
			float dotdiffuse = dot(normalize(lightRelativeToVPos), normalize (normal.xyz));
			float diffuseComponent = clamp(dotdiffuse, 0.0, 1.0);
			
			//int counter = 0;

			// do shadows
			vec4 lightClipSpace = lightPV * vec4(fragmentPosWorld3d.xyz, 1.0);
			if(lightClipSpace.z >= 0.0){ 
				vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;	
				if(lightScreenSpace.x > 0.0 && lightScreenSpace.x < 1.0 && lightScreenSpace.y > 0.0 && lightScreenSpace.y < 1.0){ 
					float percent = clamp(getShadowPercent(lightScreenSpace, fragmentPosWorld3d.xyz, i), 0.0, 1.0);
					

					float culler = clamp((1.0 - distance(lightScreenSpace, vec2(0.5)) * 2.0), 0.0, 1.0);

					color1 += ((colorOriginal * (diffuseComponent * LightsColors[i].rgb)) 
					+ (LightsColors[i].rgb * specularComponent)) * LightsColors[i].a 
					* culler * att * percent;
					
				}
			}
			
		}
		

		for(int i=0;i<SimpleLightsCount;i++){
		
			vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4(SimpleLightsPos[i], 1.0);
			vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
			//if(clipspace.z < 0.0) continue;
			float vis = testVisibility(UV, sspace1, SimpleLightsPos[i]);
					
			float dist = distance(CameraPosition, SimpleLightsPos[i]);
			float att = 1.0 / pow(((dist/1.0) + 1.0), 2.0) * 390.0;
			float revlog = reverseLog(texture(texDepth, nUV).r);
			
			vec3 lightRelativeToVPos = SimpleLightsPos[i] - fragmentPosWorld3d.xyz;
			vec3 R = reflect(lightRelativeToVPos, normal.xyz);
			float cosAlpha = -dot(normalize(cameraRelativeToVPos), normalize(R));
			float specularComponent = clamp(pow(cosAlpha, 80.0 / normal.a), 0.0, 1.0) * fragmentPosWorld3d.a;
			
			lightRelativeToVPos = SimpleLightsPos[i] - fragmentPosWorld3d.xyz;
			float dotdiffuse = dot(normalize(lightRelativeToVPos), normalize (normal.xyz));
			float diffuseComponent = clamp(dotdiffuse, 0.0, 1.0);
			color1 += ((colorOriginal * (diffuseComponent * SimpleLightsColors[i].rgb)) 
					+ (SimpleLightsColors[i].rgb * specularComponent))
					*att * vis;
			}
		
	}
    outColor = vec4(color1, 1);
}