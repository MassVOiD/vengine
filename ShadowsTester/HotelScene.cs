using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using VEngine;
using VEngine.FileFormats;
using VEngine.Generators;

namespace ShadowsTester
{
    public class HotelScene
    {
        class TexInfo
        {
            public string Type = "";
            public string TexFile = "";
            public bool InvertUVY = false, InvertUVX = false;
            public Vector2 UVScale = Vector2.One;
            public Vector3 Value = Vector3.One;
            public float Gain = 1.0f;

        }

        private static float StringToFloat(string str)
        {
            str = str.Trim();
            float n;
            if(!float.TryParse(str, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out n))
                throw new ArgumentException("Invalid line in scene string: " + str);
            return n;
        }

        public HotelScene()
        {
            var scene = Game.World.Scene;

            var unknownMaterial = new GenericMaterial(new Vector3(1));

            var lines = System.IO.File.ReadAllLines(Media.Get("scene.scn"));
            int linec = lines.Length;

            var texmap = new Dictionary<string, TexInfo>();
            var matermap = new Dictionary<string, GenericMaterial>();

            var mat = new GenericMaterial();
            var trans = Matrix4.Identity;
            var glossyNtex = new Texture(Media.Get("200_norm.jpg"));
            var fussyNtex = new Texture(Media.Get("174_norm.jpg"));

            for(int i = 0; i < linec; i++)
            {
                var line = lines[i];
                if(line.Contains("scene.textures.texture"))
                {
                    var sdot = line.Split('.');
                    string keyname = sdot[2];
                    if(!texmap.ContainsKey(keyname))
                        texmap.Add(keyname, new TexInfo());
                    if(line.Contains(keyname + ".gain"))
                    {
                        var vals = line.Split('=')[1];
                        vals.Trim();
                        texmap[keyname].Gain = StringToFloat(vals);
                    }
                    if(line.Contains(keyname + ".value"))
                    {
                        var vals = line.Split('=')[1].Trim().Split(' ');
                        if(vals.Length >= 3)
                            texmap[keyname].Value = new Vector3(StringToFloat(vals[0]), StringToFloat(vals[1]), StringToFloat(vals[2]));
                        else if(vals.Length == 1)
                            texmap[keyname].Value = new Vector3(StringToFloat(vals[0]));
                    }
                    if(line.Contains(keyname + ".uvscale"))
                    {
                        var vals = line.Split('=')[1].Trim().Split(' ');
                        if(vals.Length >= 2)
                            texmap[keyname].UVScale = new Vector2(StringToFloat(vals[0]), StringToFloat(vals[1]));
                        else if(vals.Length == 1)
                            texmap[keyname].UVScale = new Vector2(StringToFloat(vals[0]));
                    }
                    if(line.Contains(keyname + ".file"))
                    {
                        var vals = line.Split('=')[1].Trim(new char[] { ' ', '"' });
                        texmap[keyname].TexFile = vals.Replace(".exr", ".png");
                    }
                    if(line.Contains(keyname + ".type"))
                    {
                        var vals = line.Split('=')[1].Trim(new char[] { ' ', '"' });
                        texmap[keyname].Type = vals;
                    }
                }
                else if(line.Contains("scene.materials.material"))
                {
                    var sdot = line.Split('.');
                    string keyname = sdot[2];
                    if(!matermap.ContainsKey(keyname))
                        matermap.Add(keyname, new GenericMaterial());
                    if(line.Contains(keyname + ".d ") || line.Contains(keyname + ".kd "))
                    {
                        var vals = line.Split('=')[1].Trim(new char[] { ' ', '"' });
                        if(texmap.ContainsKey(vals))
                        {

                            //matermap[keyname].
                            //matermap[keyname].if(texmap.ContainsKey(vals))
                            {
                                if(texmap[vals].TexFile.Length > 1)
                                {
                                    matermap[keyname].DiffuseTexture = new Texture(Media.Get(System.IO.Path.GetFileName(texmap[vals].TexFile)));
                                }
                                else
                                {
                                    matermap[keyname].DiffuseColor = texmap[vals].Value;
                                }
                                matermap[keyname].InvertUVYAxis = texmap[vals].InvertUVX || texmap[vals].InvertUVY;
                            }
                        }
                    }
                    if(line.Contains(keyname + ".uroughness"))
                    {
                        var vals = line.Split('=')[1].Trim(new char[] { ' ', '"' });
                        if(texmap.ContainsKey(vals))
                        {
                            if(texmap[vals].TexFile.Length > 1)
                            {
                                matermap[keyname].RoughnessTexture = new Texture(Media.Get(System.IO.Path.GetFileName(texmap[vals].TexFile)));
                            }
                            else
                            {
                                matermap[keyname].Roughness = Math.Max(1f, texmap[vals].Value.X);
                            }
                        }
                    }
                    if(line.Contains(keyname + ".vroughness"))
                    {
                        var vals = line.Split('=')[1].Trim(new char[] { ' ', '"' });
                        if(texmap.ContainsKey(vals))
                        {
                            if(texmap[vals].TexFile.Length > 1)
                            {
                                matermap[keyname].RoughnessTexture = new Texture(Media.Get(System.IO.Path.GetFileName(texmap[vals].TexFile)));
                            }
                            else
                            {
                                matermap[keyname].DiffuseColor = texmap[vals].Value;
                            }
                        }
                    }
                    if(line.Contains(keyname + ".n "))
                    {
                        var vals = line.Split('=')[1].Trim(new char[] { ' ', '"' });
                        if(texmap.ContainsKey(vals))
                        {
                            if(texmap[vals].TexFile.Length > 1)
                            {
                                matermap[keyname].NormalsTexture = new Texture(Media.Get(System.IO.Path.GetFileName(texmap[vals].TexFile)));
                            }
                        }
                    }
                    if(line.Contains(keyname + ".k ") || line.Contains(keyname + ".ks "))
                    {
                        var vals = line.Split('=')[1].Trim(new char[] { ' ', '"' });
                        if(texmap.ContainsKey(vals))
                        {
                            if(texmap[vals].TexFile.Length > 1)
                            {
                                matermap[keyname].SpecularTexture = new Texture(Media.Get(System.IO.Path.GetFileName(texmap[vals].TexFile)));
                            }
                            else
                            {
                                matermap[keyname].SpecularColor = texmap[vals].Value;
                            }
                        }
                    }
                }
                else if(line.Contains(".ply"))
                {
                    var s = line.Split('=')[1];
                    s = s.Trim(' ', '"');
                    var ply = Object3dManager.LoadFromRaw(Media.Get(System.IO.Path.GetFileName(s) + ".raw"));
                    if(mat.NormalsTexture == null && mat.RoughnessTexture == null)
                    {
                   //     mat.NormalsTexture = mat.Roughness < 0.3 ? glossyNtex : fussyNtex;
                   //     mat.NormalMapScale = 100.0f;
                    }
                    var mesh = Mesh3d.Create(new Object3dInfo(ply.Vertices), mat);
                    trans.Transpose();
                    trans = trans * Matrix4.CreateScale(0.6f) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-90.0f));
                    mesh.GetInstance(0).Transformation.SetPosition(trans.ExtractTranslation());
                    mesh.GetInstance(0).Transformation.SetOrientation(trans.ExtractRotation());
                    mesh.GetInstance(0).Transformation.SetScale(trans.ExtractScale());
                    scene.Add(mesh);
                    mat = new GenericMaterial();
                    trans = Matrix4.Identity;
                }
                else if(line.Contains(".material"))
                {
                    var vals = line.Split('=')[1].Trim(new char[] { ' ', '"' });
                    if(matermap.ContainsKey(vals))
                    {
                        mat = matermap[vals];
                    }
                }
                if(line.Contains(".transformation"))
                {
                    var s = line.Split('=')[1];
                    s = s.Trim(' ');
                    var numbers = s.Split(' ');
                    for(int f = 0; f < numbers.Length; f++)
                    {
                        trans[f % 4, (int)Math.Floor((float)f / 4)] = StringToFloat(numbers[f]);
                    }
                }
            }

            DynamicCubeMapController.Create();

        }
    }
}