#version 430 core

in vec2 UV;
#include LogDepth.glsl

#define mPI (3.14159265)
#define mPI2 (2.0*3.14159265)
#define GOLDEN_RATIO (1.6180339)
out vec4 outColor;

layout(binding = 0) uniform sampler2D color;
layout(binding = 1) uniform sampler2D depth;
layout(binding = 2) uniform sampler2D fog;
layout(binding = 3) uniform sampler2D fogDepth;
layout(binding = 4) uniform sampler2D lightpoints;
layout(binding = 6) uniform sampler2D globalIllumination;
layout(binding = 7) uniform sampler2D diffuseColor;
layout(binding = 8) uniform sampler2D normals;
layout(binding = 9) uniform sampler2D worldPos;
layout(binding = 10) uniform sampler2D lastworldPos;
layout(binding = 11) uniform sampler2D meshData;

float centerDepth;


vec3 lookupFog(vec2 fuv){
	vec3 outc = vec3(0);
	int counter = 0;
	for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 0; g2 < 6.0; g2+=1.0)
		{ 
			vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.004);
			vec3 color = texture(fog, fuv + gauss).rgb;
			outc += color;
			counter++;
		}
	}
	return outc / counter;
}
vec3 lookupFogSimple(vec2 fuv){
	return texture(fog, fuv).rgb;
}
/*
vec3 lookupFog(vec2 fuv){
	vec3 outc = vec3(0);
	float near = 99;
	for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 0; g2 < 6.0; g2+=1.0)
		{ 
			vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.005);
			vec3 color = texture(fog, fuv + gauss).rgb;
			float fdepth = texture(fogDepth, fuv + gauss).r;
			if(abs(fdepth - centerDepth) < near){
				near = abs(fdepth - centerDepth);
				outc = color;
			}
		}
	}
	return outc;
}*/




vec3 lookupGIBilinearDepthNearest(vec2 giuv){
    //ivec2 texSize = textureSize(globalIllumination,0);
	//float lookupLengthX = 1.7 / texSize.x;
	//float lookupLengthY = 1.7 / texSize.y;
	//lookupLengthX = clamp(lookupLengthX, 0, 1);
	//lookupLengthY = clamp(lookupLengthY, 0, 1);
	vec3 gi =  (texture(globalIllumination, giuv ).rgb);
	return (texture(diffuseColor, giuv).rgb) * gi	* 1.1 + (texture(color, giuv).rgb) * gi	* 1.1;
}

vec3 lookupGI(vec2 guv){
	return lookupGIBilinearDepthNearest(guv);
}
vec3 lookupGISimple(vec2 giuv){
	return texture(globalIllumination, giuv ).rgb;
}
/*
vec3 subsurfaceScatteringExperiment(){
	float frontDistance = reverseLog(texture(depth, UV).r);
	float backDistance = reverseLog(texture(backDepth, UV).r);
	float deepness =  backDistance - frontDistance;
	return vec3(
		1.0 - deepness * 15
	);
}*/

vec2 proj(vec3 dir){
	vec3 positionCenter = texture(worldPos, UV).rgb; 
	vec3 dirPosition = positionCenter + dir * 0.05;
	
	vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4(normalize(positionCenter), 1.0);
	vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
	clipspace = (ProjectionMatrix * ViewMatrix) * vec4(normalize(dirPosition), 1.0);
	vec2 sspace2 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
	return normalize(sspace2 - sspace1);
}

vec2 refractUV(){
	vec3 rdir = normalize(texture(worldPos, UV).rgb - CameraPosition);
	vec3 crs1 = normalize(cross(CameraPosition, texture(worldPos, UV).rgb));
	vec3 crs2 = normalize(cross(crs1, rdir));
	vec3 rf = refract(rdir, texture(normals, UV).rgb, 0.02);
	return UV + proj(rf) * 0.03;
}


vec3 motionBlurExperiment(vec2 uv){
	vec3 outc = texture(color, uv).rgb;
	vec3 centerPos = texture(worldPos, uv).rgb;
	vec2 nearestUV = uv;
	float worldDistance = 999999;
	
	for(float g = 0; g < mPI2 * 2; g+=0.9)
	{ 
		for(float g2 = 0.0; g2 < 4.0; g2+=0.3)
		{ 
			vec2 dsplc = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.002);
			vec3 pos = texture(lastworldPos, uv + dsplc).rgb;
			float ds = distance(pos, centerPos);
			if(worldDistance > ds){
				worldDistance = ds;
				nearestUV = uv + dsplc;
			}
		}
	}	
	//if(distance(nearestUV, uv) < 0.001) return outc;
	int counter = 0;
	outc = vec3(0);
	vec2 direction = (nearestUV - uv);
	for(float g = 0; g < 1; g+=0.1)
	{ 
		outc += texture(color, mix(uv - direction, uv + direction, g)).rgb;
		counter++;
	}
	return outc / counter;
}

uniform int UseSimpleGI;
uniform int UseFog;
uniform int UseLightPoints;
uniform int UseDepth;
uniform int UseDeferred;
uniform int UseBilinearGI;

layout (std430, binding = 2) buffer SSBOTest
{
  vec3 BufValues[]; 
}; 

#define TechniAmount 0.21        //[0.0 to 1.0]
#define TechniPower  2.85        //[0.0 to 8.0]
#define redNegativeAmount   0.96 //[0.0 to 1.0]
#define greenNegativeAmount 0.96 //[0.0 to 1.0]
#define blueNegativeAmount  0.90 //[0.0 to 1.0]

#define cyanfilter vec3(0.0, 1.30, 1.0)
#define magentafilter vec3(1.0, 0.0, 1.05) 
#define yellowfilter vec3(1.6, 1.6, 0.05)

#define redorangefilter vec2(1.05, 0.620) //RG_
#define greenfilter vec2(0.30, 1.0)       //RG_
#define magentafilter2 magentafilter.rb     //R_B

vec3 TechnicolorPass( vec3 colorInput )
{
    vec3 tcol = colorInput.rgb;

    vec2 rednegative_mul   = tcol.rg * (1.0 / (redNegativeAmount * TechniPower));
    vec2 greennegative_mul = tcol.rg * (1.0 / (greenNegativeAmount * TechniPower));
    vec2 bluenegative_mul  = tcol.rb * (1.0 / (blueNegativeAmount * TechniPower));

    float rednegative   = dot( redorangefilter, rednegative_mul );
    float greennegative = dot( greenfilter, greennegative_mul );
    float bluenegative  = dot( magentafilter2, bluenegative_mul );

    vec3 redoutput   = rednegative.rrr + cyanfilter;
    vec3 greenoutput = greennegative.rrr + magentafilter;
    vec3 blueoutput  = bluenegative.rrr + yellowfilter;

    vec3 result = redoutput * greenoutput * blueoutput;
    colorInput.rgb = mix(tcol, result, TechniAmount);
    return colorInput;
}

#define Gamma      	   1           //[0.000 to 2.000] Adjust midtones. 1.000 is neutral. This setting does exactly the same as the one in Lift Gamma Gain, only with less control.
#define Exposure           1.0           //[-1.000 to 1.000] Adjust exposure
#define Saturation 	   -0.6          //[-1.000 to 1.000] Adjust saturation
#define Bleach              1.000           //[0.000 to 1.000] Brightens the shadows and fades the colors
#define Defog               0.000           //[0.000 to 1.000] How much of the color tint to remove

#define FogColor vec3(-0.51, 0.65, -0.65) 

vec3 TonemapPass( vec3 colorInput )
{
	vec3 color = colorInput.rgb;

	color = clamp(color - Defog * FogColor, 0.0, 1.0); // Defog
	
	color *= pow(2.0f, Exposure); // Exposure
	
	color.x = pow(color.x, Gamma); 
	color.y = pow(color.y, Gamma); 
	color.z = pow(color.z, Gamma); 

	//#define BlueShift 0.00	//Blueshift
	//float4 d = color * float4(1.05f, 0.97f, 1.27f, color.a);
	//color = mix(color, d, BlueShift);
	
	vec3 lumCoeff = vec3(0.2126, 0.7152, 0.0722);
	float lum = dot(lumCoeff, color.rgb);
	
	vec3 blend = lum.rrr; //dont use vec3
	
	float L = clamp( 10.0 * (lum - 0.45), 0.0, 1.0);
  	
	vec3 result1 = 2.0f * color.rgb * blend;
	vec3 result2 = 1.0f - 2.0f * (1.0f - blend) * (1.0f - color.rgb);
	
	vec3 newColor = mix(result1, result2, L);
	vec3 A2 = Bleach * color.rgb;
	vec3 mixRGB = A2 * newColor;
	
	color.rgb += ((1.0f - A2) * mixRGB);
	
	//vec3 middlegray = float(color.r + color.g + color.b) / 3;
	float middlegray = dot(color,vec3(1.0/3.0)); //1fps slower than the original on nvidia, 2 fps faster on AMD
	
	vec3 diffcolor = color - middlegray; //float 3 here
	colorInput.rgb = (color + diffcolor * Saturation)/(1+(diffcolor*Saturation)); //saturation
	
	return colorInput;
}
#define VignetteType       1  //[1|2|3] 1 = Original, 2 = New, 3 = TV style
#define VignetteRatio   1.00  //[0.15 to 6.00]  Sets a width to height ratio. 1.00 (1/1) is perfectly round, while 1.60 (16/10) is 60 % wider than it's high.
#define VignetteRadius  0.70  //[-1.00 to 3.00] lower values = stronger radial effect from center
#define VignetteAmount -1.40  //[-2.00 to 1.00] Strength of black. -2.00 = Max Black, 1.00 = Max White.
#define VignetteSlope      6  //[2 to 16] How far away from the center the change should start to really grow strong (odd numbers cause a larger fps drop than even numbers)
#define VignetteCenter vec2(0.500, 0.500)  //[0.000 to 1.000, 0.000 to 1.000] Center of effect for VignetteType 1. 2 and 3 do not obey this setting.


vec3 VignettePass( vec3 colorInput, vec2 tex )
{
	vec3 vignette = colorInput;
	
	//Set the center
	vec2 tc = tex - VignetteCenter;
	
	//Make the ratio 1:1
	tc.x *= ratio;
	
	//Calculate the distance
    float v = length(tc) / VignetteRadius;
    
    //Apply the vignette
	vignette.rgb = vignette.rgb * (1.0 + pow(v, VignetteSlope) * VignetteAmount); //pow - multiply
	
	return vignette;
}

#define BloomThreshold     22.38       //[0.00 to 50.00] Threshold for what is a bright light (that causes bloom) and what isn't.
#define BloomPower          1.518      //[0.000 to 8.000] Strength of the bloom
#define BloomWidth          0.0105     //[0.0000 to 1.0000] Width of the bloom
vec3 BloomPass( vec3 ColorInput2,vec2 Tex  )
{
	vec3 BlurColor2 = vec3(0);
	vec3 Blurtemp = vec3(0);
	float MaxDistance = sqrt(8*BloomWidth);
	float CurDistance = 0;
	
	//float Samplecount = 0;
	float Samplecount = 25.0;
	
	vec2 blurtempvalue = Tex * resolution * BloomWidth;
	
	//float distancetemp = 1.0 - ((MaxDistance - CurDistance) / MaxDistance);
	
	vec2 BloomSample = vec2(2.5,-2.5);
	vec2 BloomSampleValue;// = BloomSample;
	
	for(BloomSample.x = (2.5); BloomSample.x > -2.0; BloomSample.x = BloomSample.x - 1.0) // runs 5 times
	{
        BloomSampleValue.x = BloomSample.x * blurtempvalue.x;
        vec2 distancetemp = BloomSample * BloomSample * BloomWidth;
        
		for(BloomSample.y = (- 2.5); BloomSample.y < 2.0; BloomSample.y = BloomSample.y + 1.0) // runs 5 ( * 5) times
		{
            distancetemp.y = BloomSample.y * BloomSample.y;
			//CurDistance = sqrt(dot(BloomSample,BloomSample)*BloomWidth); //dot() attempt - same result , same speed. //move x part up ?
			CurDistance = sqrt( (distancetemp.y * BloomWidth) + distancetemp.x); //dot() attempt - same result , same speed. //move x part up ?
			
			//Blurtemp.rgb = myTex2D(s0, vec2(Tex + (BloomSample*blurtempvalue))); //same result - same speed.
			BloomSampleValue.y = BloomSample.y * blurtempvalue.y;
			Blurtemp.rgb = texture(color, vec2(Tex + BloomSampleValue)).rgb; //same result - same speed.
			
			//BlurColor2.rgb += lerp(Blurtemp.rgb,ColorInput2.rgb, 1 - ((MaxDistance - CurDistance)/MaxDistance)); //convert float4 to vec3 and check if it's possible to use a MAD
			BlurColor2.rgb += mix(Blurtemp.rgb,ColorInput2.rgb, 1.0 - ((MaxDistance - CurDistance) / MaxDistance)); //convert float4 to vec3 and check if it's possible to use a MAD
			
			//Samplecount = Samplecount + 1; //take out of loop and replace with constant if it helps (check with compiler)
		}
	}
	BlurColor2.rgb = (BlurColor2.rgb / (Samplecount - (BloomPower - BloomThreshold*5))); //check if using MAD
	float Bloomamount = (dot(ColorInput2.rgb,vec3(0.299f, 0.587f, 0.114f))) ; //try BT 709
	vec3 BlurColor = BlurColor2.rgb * (BloomPower + 4.0); //check if calculated offline and combine with line 24 (the blurcolor2 calculation)

	ColorInput2.rgb = mix(ColorInput2.rgb,BlurColor.rgb, Bloomamount);	

	return ColorInput2;
}


#define Curves_mode                   2     //[0|1|2] Choose what to apply contrast to. 0 = Luma, 1 = Chroma, 2 = both Luma and Chroma. Default is 0 (Luma)
#define Curves_contrast            0.68     //[-1.00 to 1.00] The amount of contrast you want

// -- Advanced curve settings --
#define Curves_formula                9     //[1|2|3|4|5|6|7|8|9|10] The contrast s-curve you want to use.
                                            //1 = Sine, 2 = Abs split, 3 = Smoothstep, 4 = Exp formula, 5 = Simplified Catmull-Rom (0,0,1,1), 6 = Perlins Smootherstep
                                            //7 = Abs add, 8 = Techicolor Cinestyle, 9 = Parabola, 10 = Half-circles.
                                            //Note that Technicolor Cinestyle is practically identical to Sine, but runs slower. In fact I think the difference might only be due to rounding errors.
                                            //I prefer 2 myself, but 3 is a nice alternative with a little more effect (but harsher on the highlight and shadows) and it's the fastest formula.
					
vec3 CurvesPass( vec3 colorInput )
{
  vec3 color = colorInput.rgb; //original input color
  vec3 lumCoeff = vec3(0.2126, 0.7152, 0.0722);  //Values to calculate luma with
  float Curves_contrast_blend = Curves_contrast;
  float PI = acos(-1); //3.14159265

  //calculate luma (grey)
  float luma = dot(lumCoeff, color);
	
  //calculate chroma
	vec3 chroma = color - luma;
	
	//Apply curve to luma
	
	// -- Curve 1 --
  #if Curves_formula == 1
    luma = sin(PI * 0.5 * luma); // Sin - 721 amd fps
    luma *= luma;  
  #endif
  
  // -- Curve 2 --
  #if Curves_formula == 2
    luma = ( (luma - 0.5) / (0.5 + abs(luma-0.5)) ) + 0.5; // 717 amd fps
  #endif

	// -- Curve 3 --
  #if Curves_formula == 3
    //luma = smoothstep(0.0,1.0,luma); //smoothstep
    luma = luma*luma*(3.0-2.0*luma); //faster smoothstep alternative - 776 amd fps
  #endif

	// -- Curve 4 --
  #if Curves_formula == 4
    luma = 1.1048 / (1.0 + exp(-3.0 * (luma * 2.0 - 1.0))) - (0.1048 / 2.0); //exp formula - 706 amd fps
  #endif

	// -- Curve 5 --
  #if Curves_formula == 5
    luma = 0.5 * (luma + 3.0 * luma * luma - 2.0 * luma * luma * luma); //a simple catmull-rom (0,0,1,1) - 726 amd fps
    Curves_contrast_blend = Curves_contrast * 2.0; //I multiply by two to give it a strength closer to the other curves.
  #endif

 	// -- Curve 6 --
  #if Curves_formula == 6
    luma = luma*luma*luma*(luma*(luma*6.0 - 15.0) + 10.0); //Perlins smootherstep - 752 amd fps
	#endif
	
	// -- Curve 7 --
  #if Curves_formula == 7
    luma = ((luma-0.5) / ((0.5/(4.0/3.0)) + abs((luma-0.5)*1.25))) + 0.5; // amd fps
  #endif
	
  
	//Add back the chroma
	color = luma + chroma;
	
	//Blend by Curves_contrast
	colorInput.rgb = mix(colorInput.rgb, color, Curves_contrast_blend);
	
  //Return the result
  return colorInput;
}

const mat3x3 RGB =
mat3x3(
2.67147117265996,-1.26723605786241,-0.410995602172227,
-1.02510702934664,1.98409116241089,0.0439502493584124,
0.0610009456429445,-0.223670750812863,1.15902104167061
);

const mat3x3 XYZ =
mat3x3(
0.500303383543316,0.338097573222739,0.164589779545857,
0.257968894274758,0.676195259144706,0.0658358459823868,
0.0234517888692628,0.1126992737203,0.866839673124201
);

#define Red   10.1  //[1.0 to 15.0]
#define Green 10.1  //[1.0 to 15.0]
#define Blue  10.1  //[1.0 to 15.0]

#define ColorGamma    1.0  //[0.1 to 2.5] Adjusts the colorfulness of the effect in a manner similar to Vibrance. 1.0 is neutral.
#define DPXSaturation 0.9  //[0.0 to 8.0] Adjust saturation of the effect. 1.0 is neutral.

#define RedC   0.48  //[0.60 to 0.20]
#define GreenC 0.51  //[0.60 to 0.20]
#define BlueC  0.50  //[0.60 to 0.20]

#define Blend 0.43    //[0.00 to 1.00] How strong the effect should be
vec3 DPXPass(vec3 InputColor) {

	float DPXContrast = 0.1;

	float DPXGamma = 1.0;

	float RedCurve = Red;
	float GreenCurve = Green;
	float BlueCurve = Blue;

	vec3 B = InputColor.rgb;
	//float3 Bn = B; // I used InputColor.rgb instead.

	B.r = pow(B.r, 1.0/DPXGamma);
	B.g = pow(B.g, 1.0/DPXGamma);
	B.b = pow(B.b, 1.0/DPXGamma);

	B.r = pow(B.r, 1.00);
	B.g = pow(B.g, 1.00);
	B.b = pow(B.b, 1.00);

        B = (B * (1.0 - DPXContrast)) + DPXContrast / 2.0;
 	
 	B.r = (1.0 /(1.0 + exp(- RedCurve * (B.r - RedC))) - (1.0 / (1.0 + exp(RedCurve / 2.0))))/(1.0 - 2.0 * (1.0 / (1.0 + exp(RedCurve / 2.0))));				
	B.g = (1.0 /(1.0 + exp(- GreenCurve * (B.g - GreenC))) - (1.0 / (1.0 + exp(GreenCurve / 2.0))))/(1.0 - 2.0 * (1.0 / (1.0 + exp(GreenCurve / 2.0))));				
	B.b = (1.0 /(1.0 + exp(- BlueCurve * (B.b - BlueC))) - (1.0 / (1.0 + exp(BlueCurve / 2.0))))/(1.0 - 2.0 * (1.0 / (1.0 + exp(BlueCurve / 2.0))));					

        //TODO use faster code for conversion between RGB/HSV  -  see http://www.chilliant.com/rgb2hsv.html
	   float value = max(max(B.r, B.g), B.b);
	   vec3 color = B / value;
	
	   color.x = pow(color.x, 1.0/ColorGamma);
	   color.y = pow(color.y, 1.0/ColorGamma);
	   color.z = pow(color.z, 1.0/ColorGamma);
	
	   vec3 c0 = color * value;

	   c0 = mul(XYZ, c0);

	   float luma = dot(c0, vec3(0.30, 0.59, 0.11)); //Use BT 709 instead?
	   vec3 chroma = c0 - luma;

	   c0 = luma + chroma * DPXSaturation;
	   c0 = mul(RGB, c0);
	
	InputColor.rgb = mix(InputColor.rgb, c0, Blend); //as long as Blend is always 0 we don't really need to lerp. The compiler *should* be smart enough to optimize this though (check to be sure)

	return InputColor;
}

vec3 lookupGIBlurred(vec2 giuv, float radius){
	vec3 outc = vec3(0);
	float last = 0;
	int counter = 0;
    float cdp = texture(depth, giuv).r;
	for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 1; g2 < 6.0; g2+=1.0)
		{ 
			vec2 gauss = giuv + vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * radius);
			float dp = texture(depth, gauss).r;
			if(abs(dp - cdp) > 0.001) continue;
            if(gauss.x < 0 || gauss.x > 1 || gauss.y < 0 || gauss.y > 1) continue;
			vec3 color = texture(globalIllumination, gauss).rgb;
            outc += color;
            counter++;
		}
	}
	vec3 rs = (outc / counter * texture(diffuseColor, giuv).rgb ) * 2.7;
    return rs;
}
void main()
{
	vec2 nUV = UV;
	vec3 color1 = vec3(0);
	if(texture(meshData, UV).b < 0.01){
		nUV = refractUV();
	   //color1 += texture(color, UV).rgb * texture(diffuseColor, UV).a;
	}
	if(UseDeferred == 1) color1 += texture(color, nUV).rgb;
	//if(UseDeferred == 1) color1 += motionBlurExperiment(nUV);
	//if(UseFog == 1) color1 += lookupFog(nUV) * FogContribution;
	if(UseFog == 1) color1 += lookupFogSimple(nUV) * FogContribution;
	if(UseLightPoints == 1) color1 += texture(lightpoints, nUV).rgb;
	if(UseDepth == 1) color1 += texture(depth, nUV).rrr;
	//if(UseBilinearGI == 1) color1 += lookupGIBilinearDepthNearest(nUV);
	if(UseSimpleGI == 1) color1 += lookupGIBlurred(nUV, 0.005) * GIContribution;
	//color1 += texture(globalIllumination, nUV ).rgb;
	centerDepth = texture(depth, UV).r;
	
	gl_FragDepth = centerDepth;
	
	/*if(UV.x > 0 && UV.x < 0.05) color1 = (BufValues[0]);
	if(UV.x > 0.05 && UV.x < 0.1) color1 = (BufValues[1]);
	if(UV.x > 0.1 && UV.x < 0.15) color1 = (BufValues[2]);
	if(UV.x > 0.15 && UV.x < 0.2) color1 = (BufValues[3]);
	*/
    //color1 = TechnicolorPass(color1);
    //color1 = VignettePass(color1, UV);
    //color1 = CurvesPass(color1);
    //color1 = DPXPass(color1);
    //color1 = TonemapPass(color1);
    //float ddot = dot(normalize(texture(diffuseColor, UV).rgb), normalize(color1));
    //if(ddot < 0.4) color1.rgb = vec3(1.0 - abs(ddot))*5 + texture(diffuseColor, UV).rgb;
    outColor = vec4(clamp(color1, 0, 1), 1);
}