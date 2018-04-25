using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UntitledTools {
	namespace LandscapeGeneration {

		public static class GeneratorAPIs {

			/// <summary>
			/// Gets the height map.
			/// </summary>
			/// <returns>The height map.</returns>
			/// <param name="Width">Width.</param>
			/// <param name="Height">Height.</param>
			/// <param name="Lacunarity">Lacunarity.</param>
			/// <param name="Persistance">Persistance.</param>
			/// <param name="Scale">Scale.</param>
			/// <param name="Offset">Offset.</param>
			/// <param name="Seed">Seed.</param>
			/// <param name="Octaves">Octaves.</param>
			/// <param name="HeightCurve">Height curve.</param>
			public static float[,] GetHeightMap (int Width, int Height, float Lacunarity, float Persistance, float Scale, int Seed, int Octaves, AnimationCurve HeightCurve, bool Islandify, int BlurIterations = 0) {

				float[,] NoiseMapFloat = new float[Width, Height];

				System.Random PRNG = new System.Random (Seed);
				Vector2[] OctaveOffsets = new Vector2[Octaves];
				for (int o = 0; o < OctaveOffsets.Length; o++) {
					float OffsetX = (float)PRNG.Next (-100000, 100000);
					float OffsetY = (float)PRNG.Next (-100000, 100000);
					OctaveOffsets [o] = new Vector2 (OffsetX, OffsetY);
				}

				Vector2 MiddleOfMap = new Vector2 ((float)Width / 2, (float)Height / 2);
				float IslandifyEffect = (float)Width / 2;

				float MaxHeight = float.MinValue;
				float MinHeight = float.MaxValue;

				if (Scale <= 0f) {
					Scale = 0.0001f;
				}

				int EditorIndex = 0;

				//Creates the perlin map
				for (int x = 0; x < Width; x++) {
					for (int y = 0; y < Height; y++) {

						float NoiseMapVal = 0f;
						float Amplitude = 1f;
						float Frequency = 1f;

						for (int o = 0; o < Octaves; o++) {

							float SampledX = x / Scale * Frequency + OctaveOffsets[o].x;
							float SampledY = y / Scale * Frequency + OctaveOffsets[o].y;

							float PerlinMap = Mathf.PerlinNoise (SampledX, SampledY) * 2 - 1;
							NoiseMapVal += PerlinMap * Amplitude;
							NoiseMapFloat [x, y] = NoiseMapVal;

							Amplitude *= Persistance;
							Frequency *= Lacunarity;

						}

						if (NoiseMapVal > MaxHeight) {
							MaxHeight = NoiseMapVal;
						} else if (NoiseMapVal < MinHeight) {
							MinHeight = NoiseMapVal;
						}

					}
				}

				for (int x = 0; x < Width; x++) {
					for (int y = 0; y < Height; y++) {
						
						NoiseMapFloat [x, y] = Mathf.InverseLerp (MinHeight, MaxHeight, NoiseMapFloat [x, y]);
						NoiseMapFloat [x, y] = HeightCurve.Evaluate (NoiseMapFloat [x, y]);

						if (Islandify) {
							Vector2 CurrentPos = new Vector2 ((float)x, (float)y);
							float Distance = Vector2.Distance (CurrentPos, MiddleOfMap);
							float IslandifyFactor = 1 - (Distance / IslandifyEffect);
							NoiseMapFloat [x, y] *= IslandifyFactor;
						}

					}
				}

				float[,] NewMapFloats = new float[NoiseMapFloat.GetLength (0), NoiseMapFloat.GetLength (1)];
				for (int i = 0; i < BlurIterations; i++) {
					
					for (int x = 1; x < Width - 1; x++) {
						for (int y = 1; y < Height - 1; y++) {
							
							float ValuesAdded = 
								NoiseMapFloat [x + 1, y] + 
								NoiseMapFloat [x - 1, y] + 
								NoiseMapFloat [x, y + 1] + 
								NoiseMapFloat [x, y - 1] + 
								NoiseMapFloat [x + 1, y + 1] + 
								NoiseMapFloat [x - 1, y + 1] + 
								NoiseMapFloat [x + 1, y - 1] + 
								NoiseMapFloat [x - 1, y - 1];
							float Average = ValuesAdded / 8f;
							NewMapFloats [x, y] = Average;

						}
					}

					NoiseMapFloat = NewMapFloats;

				}

				if (BlurIterations > 0) {
					return NewMapFloats;
				} else {
					return NoiseMapFloat;
				}

			}

			/// <summary>
			/// Gets the texture from a 2D float array.
			/// </summary>
			/// <returns>The texture from a 2D float array.</returns>
			/// <param name="Values">Values.</param>
			public static Texture2D GetTextureFrom2DFloat (float[,] Values) {

				int Width = Values.GetLength (0);
				int Height = Values.GetLength (1);
				Texture2D ReturnTex = new Texture2D (Width, Height);

				for (int x = 0; x < Width; x++) {
					for (int y = 0; y < Height; y++) {
						float Value = Values [x, y];
						Color PixelColor = new Color (Value, Value, Value, Value);
						ReturnTex.SetPixel (x, y, PixelColor);
					}
				}
					
				ReturnTex.Apply ();
				return ReturnTex;

			}

			/// <summary>
			/// Gets the pow of two.
			/// </summary>
			/// <returns>The pow of two.</returns>
			/// <param name="_enum">Enum.</param>
			public static int GetPowOfTwo (PowTwoNum _enum) {
				string EnumString = _enum.ToString ();
				EnumString = EnumString.Replace ("_", "");
				int ReturnNum = int.Parse (EnumString);
				return ReturnNum;
			} 

			public static class IO {

				/// <summary>
				/// Saves the texture as PNG.
				/// </summary>
				/// <returns>The texture as PNG.</returns>
				/// <param name="DestPath">Destination path.</param>
				/// <param name="TextureObject">Texture object.</param>
				/// <param name="FileName">File name.</param>
				public static string SaveTextureAsPNG (string DestPath, Texture2D TextureObject, string FileName = "Saved_PNG") {

					if (FileName == string.Empty || FileName == null) {
						FileName = "Saved_PNG";
					}

					string FilePath = DestPath + "/" + FileName + ".png";
					byte[] TextureBytes = TextureObject.EncodeToPNG ();
					File.WriteAllBytes (FilePath, TextureBytes);
					return FilePath;

				}

			}
				
			public static class TerrainAPIs {

				/// <summary>
				/// Gets the terrain texture prototype.
				/// </summary>
				/// <returns>The terrain texture prototype.</returns>
				/// <param name="TerrainSettings">Terrain settings.</param>
				/// <param name="TileSize">Tile size.</param>
				/// <param name="TypeID">Type I.</param>
				public static SplatPrototype GetTerrainTexturePrototype (GeneratorFields.TerrainSettings TerrainSettings, Vector2 TileSize, int TypeID = 0) {

					SplatPrototype TerrainTex = new SplatPrototype ();

					if (TypeID == 0) {

						TerrainTex.texture = TerrainSettings.TerrainTextures.AlbedoTextures.Grass;
						TerrainTex.normalMap = TerrainSettings.TerrainTextures.NormalMaps.Grass;
						TerrainTex.metallic = TerrainSettings.TerrainTextures.Metallic.Grass;
						TerrainTex.specular = TerrainSettings.TerrainTextures.SpecularColors.Grass;
						TerrainTex.tileSize = new Vector2 (9f, 9f);

					} else if (TypeID == 1) {

						TerrainTex.texture = TerrainSettings.TerrainTextures.AlbedoTextures.Dirt;
						TerrainTex.normalMap = TerrainSettings.TerrainTextures.NormalMaps.Dirt;
						TerrainTex.metallic = TerrainSettings.TerrainTextures.Metallic.Dirt;
						TerrainTex.specular = TerrainSettings.TerrainTextures.SpecularColors.Dirt;
						TerrainTex.tileSize = new Vector2 (9f, 9f);

					} else if (TypeID == 2) {

						TerrainTex.texture = TerrainSettings.TerrainTextures.AlbedoTextures.Rock;
						TerrainTex.normalMap = TerrainSettings.TerrainTextures.NormalMaps.Rock;
						TerrainTex.metallic = TerrainSettings.TerrainTextures.Metallic.Rock;
						TerrainTex.specular = TerrainSettings.TerrainTextures.SpecularColors.Rock;
						TerrainTex.tileSize = new Vector2 (9f, 9f);

					} else if (TypeID == 3) {

						TerrainTex.texture = TerrainSettings.TerrainTextures.AlbedoTextures.Sand;
						TerrainTex.normalMap = TerrainSettings.TerrainTextures.NormalMaps.Sand;
						TerrainTex.metallic = TerrainSettings.TerrainTextures.Metallic.Sand;
						TerrainTex.specular = TerrainSettings.TerrainTextures.SpecularColors.Sand;
						TerrainTex.tileSize = new Vector2 (9f, 9f);

					} else {
						return null;
					}

					return TerrainTex;

				}

			}

		}

	}
}


