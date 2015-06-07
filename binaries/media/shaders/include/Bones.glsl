const int MAX_BONES = 64;
uniform int UseBoneSystem;
uniform int BonesCount;
uniform vec3 BonesHeads[MAX_BONES];
uniform vec3 BonesTails[MAX_BONES];
uniform int BonesParents[MAX_BONES];
uniform mat4 BonesRotationMatrices[MAX_BONES];

int determineBone(vec3 modelpos){
	int index = 0;
	float maxdist = 99999;
	for(int i=0;i<BonesCount;i++){
		float dist = distance(modelpos, BonesTails[i]) + distance(modelpos, BonesHeads[i]);
		if(dist < maxdist){
			index = i;
			maxdist = dist;
		}
	}
	return index;
}

vec3 applyBoneRotationChain(vec3 pos, int boneIndex){
	int pointer = boneIndex;
	for(int i = 0; i < BonesCount; i++){
    
        pos -= BonesHeads[pointer];
        pos = (BonesRotationMatrices[pointer] * vec4(pos, 1)).xyz;
        pos += BonesHeads[pointer];
        
		if(BonesParents[pointer] > 0){
			pointer = BonesParents[pointer];
		} else {
			break;
		}
	}
	return pos;
}
vec3 applyBoneRotationChainNormal(vec3 nrm, int boneIndex){
	int pointer = boneIndex;
    mat4 mat = mat4(1);
	for(int i = 0; i < BonesCount; i++){
    
        mat = mat * BonesRotationMatrices[pointer];
		if(BonesParents[pointer] > 0){
			pointer = BonesParents[pointer];
		} else {
			break;
		}
	}
	return (mat * vec4(nrm, 0)).xyz;
}