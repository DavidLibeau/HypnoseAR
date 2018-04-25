using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UntitledTools {
	namespace LandscapeGeneration {

		/// <summary>
		/// Powers of two.
		/// </summary>
		public enum PowTwoNum {
			_2 = 2, _4 = 4, _8 = 8, _16 = 16, _32 = 32, _64 = 64, _128 = 128, _256 = 256, _512 = 512, _1024 = 1024, _2048 = 2048, _4096 = 4096, _8192 = 8192
		};

		//The classes for all of the variables in the landscape generator
		public class GeneratorFields {

			#region GeneralVars
			//The variables that didn't fit in any other catagory
			[System.Serializable]
			public class GeneralSettings {

				[Header("General Settings")]
				[Space(10)]
				[Tooltip("Combines every button into one single button.")]
				public bool AutoGenerate;
				[Tooltip("Shows buttons for generating every aspect of the landscape separately.")]
				public bool GenerateSeparately;
				[Tooltip("Will the landscape generator place the player on the terrain?")]
				public bool UsePlayer;
				[Tooltip("If \"Use Player\" is checked, then this is the player object that will be placed on the terrain.")]
				public Transform Player;

			}
			#endregion

			#region TerrainVars
			//The variables for terrain generation
			[System.Serializable]
			public class TerrainSettings {

				[Header("Terrain Settings")]
				[Space(10)]
				[Tooltip("If you are not procedurally generating the terrain, this is the terrain that textures, details, trees, and detail objects will be generated on.")]
				public GameObject Terrain;
				[Tooltip("Are you procedurally generating the terrain?  If so, the \"Terrain\" setting can be set to none.")]
				public bool UseProceduralTerrain = true;
				[Tooltip("Makes procedural terrains island-like.")]
				public bool Islandify = false;
				[Space(5f)]
				[Tooltip("All of the settings for generating procedural terrains.")]
				public Internal.ProceduralTerrain ProceduralSettings;
				[Space(5f)]
				[Tooltip("All of the terrain texturing settings.")]
				public Internal.TerrainTexVars TerrainTextures;
				[Space(5f)]
				[Tooltip("The world height of the sand on the terrain.")]
				public float SandHeight;
				[Range(0f, 1f)]
				[Tooltip("The blend factor between the rock layer and the grass layer.")]
				public float RockyBlend = 1f;
				[Range(0f, 1f)]
				[Tooltip("The blend factor between the rock layer and the dirt layer.")]
				public float DirtRockBlend = 0.5f;
				[Range(1f, 4f)]
				[Tooltip("The impact of the dirt layer on the terrain.")]
				public float DirtWeight = 1.5f;

			}
			#endregion

			#region InternalClasses
			/// <summary>
			/// Internal classes and variables.
			/// </summary>
			public class Internal {

				[System.Serializable]
				/// <summary>
				/// Terrain texture variables.
				/// </summary>
				public class TerrainTexVars {

					[Tooltip("The main textures of the texture layers.")]
					public Textures AlbedoTextures;
					[Tooltip("The normal maps or bump maps of the texture layers")]
					public Textures NormalMaps;
					[Tooltip("The smoothness values of the texture layers.")]
					public Floats Smoothness;
					[Tooltip("The metallic values of each texture layer.")]
					public Floats Metallic;
					[Tooltip("The specular colors or shine colors of the texture layers.")]
					public Colors SpecularColors;

					[System.Serializable]
					public class Textures {
						[Tooltip("Grass texture field.")]
						public Texture2D Grass;
						[Tooltip("Dirt texture field.")]
						public Texture2D Dirt;
						[Tooltip("Rock texture field.")]
						public Texture2D Rock;
						[Tooltip("Sand texture field.")]
						public Texture2D Sand;
					}

					[System.Serializable]
					public class Floats {
						[Range(0f, 1f)]
						[Tooltip("Grass value.")]
						public float Grass;
						[Range(0f, 1f)]
						[Tooltip("Dirt value.")]
						public float Dirt;
						[Range(0f, 1f)]
						[Tooltip("Rock value.")]
						public float Rock;
						[Range(0f, 1f)]
						[Tooltip("Sand value.")]
						public float Sand;
					}

					[System.Serializable]
					public class Colors {
						[Tooltip("Grass specular color.")]
						public Color Grass;
						[Tooltip("Dirt specular color.")]
						public Color Dirt;
						[Tooltip("Rock specular color.")]
						public Color Rock;
						[Tooltip("Sand specular color.")]
						public Color Sand;
					}

				}
					
				[System.Serializable]
				/// <summary>
				/// Procedural terrain settings class.
				/// </summary>
				public class ProceduralTerrain {
					
					[Tooltip("The generated sizes of the different terrain maps (ex. detail map; height map; texture (or alpha) map.)")]
					public PowTwoNum MapSizes = PowTwoNum._512;
					[Tooltip("The detail map resolution. (1024 is recommended)")]
					public PowTwoNum DetailMapRes = PowTwoNum._1024;
					[Tooltip("The detail map resolution per pixel (or per chunk). (8, 16 and 32 are recommended).")]
					public PowTwoNum DetailMapResPerPixel = PowTwoNum._8;
					[Tooltip("The actual, physical, world-space size of the terrain.")]
					public Vector3 TerrainSize = new Vector3 (1000f, 150f, 1000f);
					[Tooltip("How much the terrain will be smoothed.")]
					public int SmoothIterations = 3;
					[Tooltip("The seed of the terrain.")]
					public int Seed = 1234;
					[Tooltip("How fast each detail layer gets more detailed.")]
					public float Lacunarity = 2f;
					[Tooltip("How much each new detail layer affects the terrain's height map.")]
					public float Persistance = 0.5f;
					[Range(0.001f, 10000f)]
					[Tooltip("The height map scale.")]
					public float TerrainScale = 200f;
					[Tooltip("The curve that the terrain heights will be multiplied by, this is incredibly powerful due to the fact that you essentially create the shape of the terrain here.")]
					public AnimationCurve HeightScaleCurve = new AnimationCurve (DefaultFrames);
					[Tooltip("The detail levels of the terrain.")]
					public int Octaves = 5;

					//The variable for the default value of the HeightScaleCurve field
					private static Keyframe[] DefaultFrames = { new Keyframe (0f, 0f), new Keyframe (1f, 1f) };

				}

			}
			#endregion InternalClasses

		}

	}
}
