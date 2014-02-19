using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using Planetion.Objects;
using Planetion.Internals;

#if ANDROID
using Android.Graphics;
#endif

namespace Planetion.Loaders {
	public class ObjLoader {
        
		private class MtlData {
			public float[] AmbientLight;
			public float[] DiffuseLight;
			public float[] SpecularLight;
			public float SpecularShininess = 0;
			public Bitmap AmbientMap;
			public Bitmap DiffuseMap;
			public Bitmap SpecularMap;
		}

		public static Mesh Load(string fileName) {
            CultureInfo cult = CultureInfo.CreateSpecificCulture("en-us");
			StreamReader rd = new StreamReader(fileName);
			List<float> sharedVertices = new List<float>();
			List<float> sharedTexCoords = new List<float>();
			List<float> sharedNormals = new List<float>();

			List<float> vertices = new List<float>();
			List<float> texCoords = new List<float>();
			List<float> normals = new List<float>();

			Mesh mesh = new Mesh();
			Dictionary<string, MtlData> mtllibs = new Dictionary<string, MtlData>();
			string fullPath = Directory.GetParent(fileName).FullName;

			string line;
			while ((line = rd.ReadLine()) != null) {
				string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 0) continue;

				switch (parts[0]) {
					case "v":
						sharedVertices.Add(float.Parse(parts[1], cult));
						sharedVertices.Add(float.Parse(parts[2], cult));
						sharedVertices.Add(float.Parse(parts[3], cult));
						break;
					case "vt":
						sharedTexCoords.Add(float.Parse(parts[1], cult));
						sharedTexCoords.Add(float.Parse(parts[2], cult));
						break;
					case "vn":
						sharedNormals.Add(float.Parse(parts[1], cult));
						sharedNormals.Add(float.Parse(parts[2], cult));
						sharedNormals.Add(float.Parse(parts[3], cult));
						break;
					case "f":
						if (parts.Length > 4) throw new FormatException("Faces must be triangulated!");
						
						for (int i = 1; i <= 3; ++i) {
							string[] indicies = parts[i].Split(new char[] { '/' });
							int vertInd = (int.Parse(indicies[0]) - 1) * 3;
							vertices.Add(sharedVertices[vertInd]);
							vertices.Add(sharedVertices[vertInd + 1]);
							vertices.Add(sharedVertices[vertInd + 2]);

							if (indicies.Length > 1 && indicies[1] != "") {
								int textureInd = (int.Parse(indicies[1]) - 1) * 2;
								texCoords.Add(sharedTexCoords[textureInd]);
								texCoords.Add(1 - sharedTexCoords[textureInd + 1]);
							}
							
							if (indicies.Length > 2) {
								int normalInd = (int.Parse(indicies[2]) - 1) * 3;
								normals.Add(sharedNormals[normalInd]);
								normals.Add(sharedNormals[normalInd + 1]);
								normals.Add(sharedNormals[normalInd + 2]);
							}
						}
						break;
					/*case "s":
						if (parts[1] == "1" || parts[1] == "on") {
							mesh.ShadingModel = OpenTK.Graphics.OpenGL.ShadingModel.Smooth;
						} else {
							mesh.ShadingModel = OpenTK.Graphics.OpenGL.ShadingModel.Flat;
						}
						break;*/
					case "mtllib":
						parseMtllib(fullPath + "/" + parts[1], mtllibs);
						break;
					case "usemtl":
						setMtlProperties(mesh, mtllibs[parts.Length > 1 ? parts[1] : ""]);
						break;
				}
			}
			rd.Close();

			mesh.Vertices = vertices.ToArray();
            if (texCoords.Count != 0) {
                mesh.TexCoords = texCoords.ToArray();
            }
            if (normals.Count != 0) {
                mesh.Normals = normals.ToArray();
            }
			mesh.Create();
			return mesh;
		}

		private static void setMtlProperties(Mesh mesh, MtlData p) {
			/*mesh.AmbientLight = p.AmbientLight;
			mesh.DiffuseLight = p.DiffuseLight;
			mesh.SpecularLight = p.SpecularLight;
			mesh.SpecularShininess = p.SpecularShininess;
            mesh.AmbientMap = p.AmbientMap;
            mesh.SpecularMap = p.SpecularMap;*/
			mesh.DiffuseMap = p.DiffuseMap;
		}

		private static void parseMtllib(string file, Dictionary<string, MtlData> mtllib) {
			CultureInfo cult = CultureInfo.CreateSpecificCulture("en-us");
			StreamReader rd = new StreamReader(file);
			string line;
			MtlData nowMtl = null;

			while ((line = rd.ReadLine()) != null) {
				string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length == 0) continue;

				switch (parts[0]) {
					case "newmtl":
						nowMtl = new MtlData();
						mtllib.Add(parts.Length > 1 ? parts[1] : "", nowMtl);
						break;
					case "Ka":
						nowMtl.AmbientLight = new float[] { float.Parse(parts[1], cult), float.Parse(parts[2], cult), float.Parse(parts[3], cult) };
						break;
					case "Kd":
						nowMtl.DiffuseLight = new float[] { float.Parse(parts[1], cult), float.Parse(parts[2], cult), float.Parse(parts[3], cult) };
						break;
					case "Ks":
						nowMtl.SpecularLight = new float[] { float.Parse(parts[1], cult), float.Parse(parts[2], cult), float.Parse(parts[3], cult) };
						break;
					case "Ns":
						nowMtl.SpecularShininess = float.Parse(parts[1], cult) * 128 / 1000;
						break;
					case "map_Ka":
#if ANDROID
                        nowMtl.AmbientMap = (Bitmap)BitmapFactory.DecodeFile(Directory.GetParent(file).FullName + "/" + parts[1]);
#else
						nowMtl.AmbientMap = (Bitmap)Bitmap.FromFile(Directory.GetParent(file).FullName + "/" + parts[1]);
#endif
						break;
					case "map_Kd":
#if ANDROID
                        nowMtl.DiffuseMap = (Bitmap)BitmapFactory.DecodeFile(Directory.GetParent(file).FullName + "/" + parts[1]);
#else
						nowMtl.DiffuseMap = (Bitmap)Bitmap.FromFile(Directory.GetParent(file).FullName + "/" + parts[1]);
#endif
						break;
					case "map_Ks":
#if ANDROID
                        nowMtl.SpecularMap = (Bitmap)BitmapFactory.DecodeFile(Directory.GetParent(file).FullName + "/" + parts[1]);
#else
						nowMtl.SpecularMap = (Bitmap)Bitmap.FromFile(Directory.GetParent(file).FullName + "/" + parts[1]);
#endif
						break;
				}
			}
			rd.Close();
		}
	}
}