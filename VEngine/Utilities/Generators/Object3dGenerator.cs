using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using OpenTK;

namespace VEngine.Generators
{
    public class Object3dGenerator
    {
        public static bool UseCache = true;

        public static Object3dManager CreateCube(Vector3 dimensions, Vector2 uvScale)
        {
            float[] VBO = new float[288]{
             -1,  1,  -1,  0.333333f,  0,  -1,  0,  0,
             -1,  -1,  -1,  0.666667f,  0,  -1,  0,  0,
             -1,  -1,  1,  0.666667f,  0.333333f,  -1,  0,  0,
             1,  1,  -1,  0,  0.333333f,  0,  0,  -1,
             1,  -1,  -1,  0.333333f,  0.333333f,  0,  0,  -1,
             -1,  -1,  -1,  0.333333f,  0.666667f,  0,  0,  -1,
             1,  1,  1,  0,  0,  1,  0,  0,
             1,  -1,  1,  0.333333f,  0,  1,  0,  0,
             1,  -1,  -1,  0.333333f,  0.333333f,  1,  0,  0,
             -1,  1,  1,  0.666667f,  0.333333f,  0,  0,  1,
             -1,  -1,  1,  0.666667f,  0.666667f,  0,  0,  1,
             1,  -1,  1,  0.333333f,  0.666667f,  0,  0,  1,
             -1,  -1,  -1,  1,  0.333333f,  0,  -1,  0,
             1,  -1,  -1,  0.666667f,  0.333333f,  0,  -1,  0,
             1,  -1,  1,  0.666667f,  0,  0,  -1,  0,
             1,  1,  -1,  0.333333f,  0.666667f,  0,  1,  0,
             -1,  1,  -1,  0.333333f,  1,  0,  1,  0,
             -1,  1,  1,  0,  1,  0,  1,  0,
             -1,  1,  1,  0.333333f,  0.333333f,  -1,  0,  0,
             -1,  1,  -1,  0.333333f,  0,  -1,  0,  0,
             -1,  -1,  1,  0.666667f,  0.333333f,  -1,  0,  0,
             -1,  1,  -1,  0,  0.666667f,  0,  0,  -1,
             1,  1,  -1,  0,  0.333333f,  0,  0,  -1,
             -1,  -1,  -1,  0.333333f,  0.666667f,  0,  0,  -1,
             1,  1,  -1,  0,  0.333333f,  1,  0,  0,
             1,  1,  1,  0,  0,  1,  0,  0,
             1,  -1,  -1,  0.333333f,  0.333333f,  1,  0,  0,
             1,  1,  1,  0.333333f,  0.333333f,  0,  0,  1,
             -1,  1,  1,  0.666667f,  0.333333f,  0,  0,  1,
             1,  -1,  1,  0.333333f,  0.666667f,  0,  0,  1,
             -1,  -1,  1,  1,  0,  0,  -1,  0,
             -1,  -1,  -1,  1,  0.333333f,  0,  -1,  0,
             1,  -1,  1,  0.666667f,  0,  0,  -1,  0,
             1,  1,  1,  0,  0.666667f,  0,  1,  0,
             1,  1,  -1,  0.333333f,  0.666667f,  0,  1,  0,
             -1, 1, 1, 0, 1, 0, 1, 0
            };
            for(int i = 0; i < VBO.Length; i += 8)
            {
                VBO[i] *= dimensions.X;
                VBO[i + 1] *= dimensions.Y;
                VBO[i + 2] *= dimensions.Z;
                VBO[i + 3] *= uvScale.X;
                VBO[i + 4] *= uvScale.Y;
            }
            return new Object3dManager(VertexInfo.FromFloatArray(VBO));
        }

        public static Object3dManager CreateGround(Vector2 start, Vector2 end, Vector2 uvScale, Vector3 normal)
        {
            float[] VBO = {
                start.X, 0, end.Y, 0, 0, normal.X, normal.Y, normal.Z,
                end.X, 0, end.Y, 0, uvScale.Y, normal.X, normal.Y, normal.Z,
                start.X, 0, start.Y, uvScale.X, 0, normal.X, normal.Y, normal.Z,
                end.X, 0, start.Y, uvScale.X, uvScale.Y, normal.X, normal.Y, normal.Z,
                start.X, 0, start.Y, uvScale.X, 0, normal.X, normal.Y, normal.Z,
                end.X, 0, end.Y, 0, uvScale.Y, normal.X, normal.Y, normal.Z
            };
            return new Object3dManager(VertexInfo.FromFloatArray(VBO));
        }

        public static Object3dManager CreatePlane(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector2 uvScale, Vector3 normal)
        {
            float[] VBO = {
                v1.X, v1.Y, v1.Z, 0, 0, normal.X, normal.Y, normal.Z,
                v2.X, v2.Y, v1.Z, 0, uvScale.Y, normal.X, normal.Y, normal.Z,
                v3.X, v3.Y, v1.Z, uvScale.X, 0, normal.X, normal.Y, normal.Z,
                v4.X, v4.Y, v1.Z, uvScale.X, uvScale.Y, normal.X, normal.Y, normal.Z,
                v3.X, v3.Y, v1.Z, uvScale.X, 0, normal.X, normal.Y, normal.Z,
                v2.X, v2.Y, v1.Z, 0, uvScale.Y, normal.X, normal.Y, normal.Z,
            };
            return new Object3dManager(VertexInfo.FromFloatArray(VBO));
        }

        public static Object3dManager CreateTerrain(Vector2 start, Vector2 end, Vector2 uvScale, Vector3 normal, int subdivisions, Func<float, float, float> heightGenerator)
        {
            var VBO = new List<float>();
            var VBOParts = new List<float[]>();
            var indices = new List<uint>();
            uint count = 0, x, y;
            float partx1, partx2, party1, party2;
            float[] vertex1, vertex2, vertex3, vertex4;
            uint xp1, yp1;
            Vector3 normal1;
            for(x = 0; x < subdivisions; x++)
            {
                for(y = 0; y < subdivisions; y++)
                {
                    xp1 = x + 1;
                    yp1 = y + 1;
                    partx1 = (float)x / subdivisions;
                    party1 = (float)y / subdivisions;

                    partx2 = (float)(xp1) / subdivisions;
                    party2 = (float)(yp1) / subdivisions;

                    vertex1 = GetTerrainVertex(start, end, uvScale, normal, partx1, party1, heightGenerator);
                    vertex2 = GetTerrainVertex(start, end, uvScale, normal, partx2, party1, heightGenerator);
                    vertex3 = GetTerrainVertex(start, end, uvScale, normal, partx1, party2, heightGenerator);
                    vertex4 = GetTerrainVertex(start, end, uvScale, normal, partx2, party2, heightGenerator);

                    normal1 = -Vector3.Cross(GetVector(vertex2) - GetVector(vertex1),
                        GetVector(vertex3) - GetVector(vertex1)).Normalized();

                    uint[] indicesPart = {
                        count, count+1, count+2, count+3, count+2, count+1
                    };

                    VBOParts.Add(new float[] { vertex3[0], vertex3[1], vertex3[2], vertex3[3], vertex3[4], normal1.X, normal1.Y, normal1.Z });
                    VBOParts.Add(new float[] { vertex4[0], vertex4[1], vertex4[2], vertex4[3], vertex4[4], normal1.X, normal1.Y, normal1.Z });
                    VBOParts.Add(new float[] { vertex1[0], vertex1[1], vertex1[2], vertex1[3], vertex1[4], normal1.X, normal1.Y, normal1.Z });
                    VBOParts.Add(new float[] { vertex2[0], vertex2[1], vertex2[2], vertex2[3], vertex2[4], normal1.X, normal1.Y, normal1.Z });
                    indices.AddRange(indicesPart);

                    count += 4;
                }

                //GC.Collect();
            }
            //GC.Collect();

            for(int i = 0; i < VBOParts.Count; i++)
            {
                for(int g = i < 8 ? 0 : i - 8; g < (i + 8 > VBOParts.Count ? VBOParts.Count : i + 8); g++)
                {
                    if(i != g)
                    {
                        if(VBOParts[i][0] == VBOParts[g][0] && VBOParts[i][1] == VBOParts[g][1] && VBOParts[i][2] == VBOParts[g][2])
                        //if(GetVector(VBOParts[i]) == GetVector(VBOParts[g]))
                        {
                            Vector3 n = Vector3.Lerp(GetNormal(VBOParts[i]), GetNormal(VBOParts[g]), 0.5f);

                            VBOParts[i][5] = n.X;
                            VBOParts[i][6] = n.Y;
                            VBOParts[i][7] = n.Z;

                            VBOParts[g][5] = n.X;
                            VBOParts[g][6] = n.Y;
                            VBOParts[g][7] = n.Z;
                        }
                    }
                }

                if(i > subdivisions * 4 + 8)
                    for(int g = i - subdivisions * 4 - 8; g < (i - subdivisions * 4 + 8 > VBOParts.Count ? VBOParts.Count : i - subdivisions * 4 + 8); g++)
                    {
                        if(i != g)
                        {
                            if(VBOParts[i][0] == VBOParts[g][0] && VBOParts[i][1] == VBOParts[g][1] && VBOParts[i][2] == VBOParts[g][2])
                            //if(GetVector(VBOParts[i]) == GetVector(VBOParts[g]))
                            {
                                Vector3 n = Vector3.Lerp(GetNormal(VBOParts[i]), GetNormal(VBOParts[g]), 0.5f);

                                VBOParts[i][5] = n.X;
                                VBOParts[i][6] = n.Y;
                                VBOParts[i][7] = n.Z;

                                VBOParts[g][5] = n.X;
                                VBOParts[g][6] = n.Y;
                                VBOParts[g][7] = n.Z;
                            }
                        }
                    }

                if(i < VBOParts.Count - subdivisions * 4 - 8)
                    for(int g = i + subdivisions * 4 - 8; g < (i + subdivisions * 4 + 8 > VBOParts.Count ? VBOParts.Count : i + subdivisions * 4 + 8); g++)
                    {
                        if(i != g)
                        {
                            if(VBOParts[i][0] == VBOParts[g][0] && VBOParts[i][1] == VBOParts[g][1] && VBOParts[i][2] == VBOParts[g][2])
                            //if(GetVector(VBOParts[i]) == GetVector(VBOParts[g]))
                            {
                                Vector3 n = Vector3.Lerp(GetNormal(VBOParts[i]), GetNormal(VBOParts[g]), 0.5f);

                                VBOParts[i][5] = n.X;
                                VBOParts[i][6] = n.Y;
                                VBOParts[i][7] = n.Z;

                                VBOParts[g][5] = n.X;
                                VBOParts[g][6] = n.Y;
                                VBOParts[g][7] = n.Z;
                            }
                        }
                    }
            }

            for(int i = 0; i < VBOParts.Count; i+=4)
            {
                VBO.AddRange(VBOParts[i]);
                VBO.AddRange(VBOParts[i + 1]);
                VBO.AddRange(VBOParts[i + 2]);
                VBO.AddRange(VBOParts[i + 3]);
                VBO.AddRange(VBOParts[i + 2]);
                VBO.AddRange(VBOParts[i + 1]);
            }

            var finalObject = new Object3dManager(VertexInfo.FromFloatArray(VBO.ToArray()));

            //SaveCache(start, end, uvScale, normal, subdivisions, finalObject);

            return finalObject;
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 GetNormal(float[] VBOPart)
        {
            return new Vector3(VBOPart[5], VBOPart[6], VBOPart[7]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float[] GetTerrainVertex(Vector2 start, Vector2 end, Vector2 uvScale, Vector3 normal, float partx, float party, Func<float, float, float> heightGenerator)
        {
            var h = heightGenerator.Invoke(
                start.X + ((end.X - start.X) * partx), 
                start.Y + ((end.Y - start.Y) * party));

            return new float[]{
                start.X + ((end.X - start.X) * partx),
                h,
                start.Y + ((end.Y - start.Y) * party),
                partx * uvScale.X,
                party * uvScale.Y
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 GetVector(float[] VBOPart)
        {
            return new Vector3(VBOPart[0], VBOPart[1], VBOPart[2]);
        }
        
    }
}