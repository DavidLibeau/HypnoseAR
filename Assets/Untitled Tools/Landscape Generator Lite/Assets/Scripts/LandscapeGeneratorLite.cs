using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UntitledTools {
	namespace LandscapeGeneration {

		//The main generation script!
		[DisallowMultipleComponent]
		[AddComponentMenu("Untitled Tools/Landscape Generator Lite/Landscape Generator (Lite)")]
		public class LandscapeGeneratorLite : MonoBehaviour {

			#if UNITY_EDITOR
			//Initializes a new generator object when the menu item is called
			[MenuItem("Untitled Tools/Landscape Generator Lite/New Landscape Generator (Lite)")]
			private static void InitializeGenerator () {
				GameObject NewGenerator = new GameObject ("Landscape Generator Lite");
				NewGenerator.AddComponent<LandscapeGeneratorLite> ();
				Selection.activeGameObject = NewGenerator;
			}
			#endif

			#region PublicVars
			[Space(20f)]
			[Tooltip("All of the settings that did not fit in any other category.")]
			public GeneratorFields.GeneralSettings GeneralSettings = new GeneratorFields.GeneralSettings ();
			[Space(10f)]
			[Tooltip("All of the settings for generating the terrain and its textures.")]
			public GeneratorFields.TerrainSettings TerrainSettings = new GeneratorFields.TerrainSettings ();
			#endregion

			#region PrivateVars
			//The other private variables
			private int RanObj;
			private LandGenObj[] SceneObjs;
			private GameObject SelectedTerrain;
			[HideInInspector]
			public Vector3 TerrainSize;
			private Vector3 Pos;
			private float Rot;
			private Terrain TComponent;
			[HideInInspector]
			public TerrainData TData;
			private float RockyBlendOpposite;
			#endregion

			[HideInInspector]
			public Texture2D HeightMap;

			#region EditorPrivateVars
			private float ProgressBarProgress = 0f;
			#endregion

			#region AlwaysActive
			//This will always be running
			//The try-catches for all of these functions are so you can still play the game even if you got an error
			public void AlwaysActive () {

				//Both of these foreach loops add the specified tags to there object counterparts
				if (TerrainSettings.Terrain != null) {
					if (!TerrainSettings.Terrain.GetComponent<LandGenObj> ()) {
						TerrainSettings.Terrain.AddComponent<LandGenObj> ();
						TerrainSettings.Terrain.GetComponent<LandGenObj> ().type = LandGenObj.ObjectType.Landscape;
					} else if (TerrainSettings.Terrain.GetComponent<LandGenObj> ().type == LandGenObj.ObjectType.Other) {
						TerrainSettings.Terrain.GetComponent<LandGenObj> ().type = LandGenObj.ObjectType.Landscape;
					}
				}
					
				RockyBlendOpposite = (1f - TerrainSettings.RockyBlend);

				//Makes sure these variables don't go over a specific, or under a specific amount
				if (TerrainSettings.RockyBlend > 1f) {
					TerrainSettings.RockyBlend = 1f;
				}
				if (TerrainSettings.RockyBlend < 0f) {
					TerrainSettings.RockyBlend = 0f;
				}

			}
			#endregion

			#region TerrainGeneration
			//This runs when the terrain needs to be generated (it generates the terrain)
			public void GenerateTerrain () {

				#if UNITY_EDITOR
				if (GeneralSettings.AutoGenerate) {
					ProgressBarProgress = 0.25f;
					EditorUtility.DisplayProgressBar ("Landscape Generator", "Generating Terrain", ProgressBarProgress);
				} else if (!GeneralSettings.AutoGenerate && !GeneralSettings.GenerateSeparately) {
					ProgressBarProgress = 0.25f;
					EditorUtility.DisplayProgressBar ("Landscape Generator", "Generating Terrain", ProgressBarProgress);
				}
				#endif

				if (TerrainSettings.UseProceduralTerrain == false) {

					SelectedTerrain = TerrainSettings.Terrain;
					TComponent = SelectedTerrain.GetComponent<Terrain> ();
					TData = TComponent.terrainData;
					int DetailMapRes = GeneratorAPIs.GetPowOfTwo (TerrainSettings.ProceduralSettings.DetailMapRes);
					int DetailPerPix = GeneratorAPIs.GetPowOfTwo (TerrainSettings.ProceduralSettings.DetailMapResPerPixel);
					TData.SetDetailResolution (DetailMapRes, DetailPerPix);
					TComponent.gameObject.isStatic = false;

				} else {

					GameObject TerrainSceneObj = new GameObject ("Procedural Terrain");
					TerrainSceneObj.isStatic = false;
					TerrainSceneObj.AddComponent<LandGenObj> ().type = LandGenObj.ObjectType.Landscape;

					int MapSizes = GeneratorAPIs.GetPowOfTwo (TerrainSettings.ProceduralSettings.MapSizes);

					TerrainData NewTerrainData = new TerrainData ();
					NewTerrainData.alphamapResolution = MapSizes + 1;
					NewTerrainData.baseMapResolution = MapSizes;
					NewTerrainData.heightmapResolution = MapSizes;
					NewTerrainData.size = TerrainSettings.ProceduralSettings.TerrainSize;
					NewTerrainData.name = "Procedural Terrain Data";

					TerrainSceneObj.AddComponent<Terrain> ();
					TerrainSceneObj.AddComponent<TerrainCollider> ();
					TerrainSceneObj.GetComponent<Terrain> ().terrainData = NewTerrainData;
					TerrainSceneObj.GetComponent<TerrainCollider> ().terrainData = NewTerrainData;
					TerrainSettings.Terrain = TerrainSceneObj;

					SelectedTerrain = TerrainSettings.Terrain;
					TComponent = SelectedTerrain.GetComponent<Terrain> ();
					TData = TComponent.terrainData;
					int DetailMapRes = GeneratorAPIs.GetPowOfTwo (TerrainSettings.ProceduralSettings.DetailMapRes);
					int DetailPerPix = GeneratorAPIs.GetPowOfTwo (TerrainSettings.ProceduralSettings.DetailMapResPerPixel);
					TData.SetDetailResolution (DetailMapRes, DetailPerPix);

				}

				TerrainSize = TComponent.terrainData.size;
				Pos = new Vector3 (Random.Range (0f, TerrainSize.x), 0f, Random.Range (0f, TerrainSize.z));
				Pos.y = TComponent.SampleHeight (Pos);
				Quaternion PlayerRot = Quaternion.identity;
				PlayerRot.eulerAngles = new Vector3 (0f, Random.Range (-360f, 360f), 0f);

				if (GeneralSettings.UsePlayer) {
					GeneralSettings.Player.transform.position = Pos;
					GeneralSettings.Player.transform.rotation = PlayerRot;
				}

				//If you are using procedural terrain, this is the code for generating it.
				if (TerrainSettings.UseProceduralTerrain) {

					//Generates the procedural terrain's height map
					float[,] HeightMapVals = TData.GetHeights (0, 0, TData.heightmapWidth, TData.heightmapHeight);
					int Width = TData.heightmapWidth;
					int Height = TData.heightmapHeight;
					HeightMapVals = GeneratorAPIs.GetHeightMap 
						(Width, Height, TerrainSettings.ProceduralSettings.Lacunarity, TerrainSettings.ProceduralSettings.Persistance, TerrainSettings.ProceduralSettings.TerrainScale, TerrainSettings.ProceduralSettings.Seed, TerrainSettings.ProceduralSettings.Octaves, TerrainSettings.ProceduralSettings.HeightScaleCurve, TerrainSettings.Islandify, TerrainSettings.ProceduralSettings.SmoothIterations);
					HeightMap = GeneratorAPIs.GetTextureFrom2DFloat (HeightMapVals);

					//Applies the terrain's height map
					TData.SetHeights (0, 0, HeightMapVals);

				}

				SplatPrototype[] TerrainTex = new SplatPrototype[4];
				Vector2 TileSize = Vector2.one * 9f;
				//Grass Texture Settings
				TerrainTex[0] = GeneratorAPIs.TerrainAPIs.GetTerrainTexturePrototype (TerrainSettings, TileSize, 0);
				//Dirt Texture Settings
				TerrainTex[1] = GeneratorAPIs.TerrainAPIs.GetTerrainTexturePrototype (TerrainSettings, TileSize, 1);
				//Rock Texture Settings
				TileSize = new Vector2 (9f, 9f);
				TerrainTex[2] = GeneratorAPIs.TerrainAPIs.GetTerrainTexturePrototype (TerrainSettings, TileSize, 2);
				//Sand Texture Settings
				TileSize = Vector2.one * 9f;
				TerrainTex[3] = GeneratorAPIs.TerrainAPIs.GetTerrainTexturePrototype (TerrainSettings, TileSize, 3);

				//Applies both the texture and grass detail settings
				TData.splatPrototypes = TerrainTex;

				//Generates where the textures will be (this runs even when you aren't using procedural terrain!)
				float[,,] map = new float[TData.alphamapWidth, TData.alphamapHeight, TData.alphamapLayers];

				for (int y = 0; y < TData.alphamapHeight ; y++) {
					for (int x = 0; x < TData.alphamapWidth; x++) {

						float NormX = x * 1.0f / (TData.alphamapWidth - 1);
						float NormY = y * 1.0f / (TData.alphamapHeight - 1);

						float angle = TData.GetSteepness (NormX, NormY);
						float height = TData.GetHeight (x, y);
						float frac = angle / 90f;

						if (height < TerrainSettings.SandHeight) {

							map [y, x, 3] = 1f;
							map [y, x, 2] = 0f;
							map [y, x, 1] = 0f;
							map [y, x, 0] = 0f;

						} else {

							map [y, x, 3] = 0f;

							if (frac + RockyBlendOpposite > TerrainSettings.DirtRockBlend) {
								map [y, x, 2] = Mathf.Clamp01 ((frac * TerrainSettings.DirtWeight) + RockyBlendOpposite);
								map [y, x, 0] = Mathf.Clamp01 (TerrainSettings.RockyBlend - (frac * TerrainSettings.DirtWeight));
							} else {
								map [y, x, 1] = Mathf.Clamp01 ((frac * TerrainSettings.DirtWeight) + RockyBlendOpposite);
								map [y, x, 0] = Mathf.Clamp01 (TerrainSettings.RockyBlend - (frac * TerrainSettings.DirtWeight));
							}

						}

					}
				}
					
				//Applies the texture (alpha) maps
				TData.SetAlphamaps (0, 0, map);
				TComponent.Flush ();

				#if UNITY_EDITOR
				EditorUtility.ClearProgressBar ();
				#endif

			}
			#endregion TerrainGeneration

			#region ExtraFuctions
			//Destroys all terrain objects and any other related ones in the scene
			public void Destroy () {

				#if UNITY_EDITOR
				if (GeneralSettings.AutoGenerate) {
					ProgressBarProgress = 0.50f;
					EditorUtility.DisplayProgressBar ("Landscape Genertor", "Destroying terrain", ProgressBarProgress);
				} else if (!GeneralSettings.AutoGenerate && !GeneralSettings.GenerateSeparately) {
					ProgressBarProgress = 0.50f;
					EditorUtility.DisplayProgressBar ("Landscape Genertor", "Destroying terrain", ProgressBarProgress);
				}
				#endif
					
				SceneObjs = FindObjectsOfType<LandGenObj> ();

				if (TerrainSettings.UseProceduralTerrain) {
					foreach (LandGenObj obj in SceneObjs) {
						DestroyImmediate (obj.gameObject);
					}
				} else {
					
					foreach (LandGenObj obj in SceneObjs) {
						if (obj.type != LandGenObj.ObjectType.Landscape) {
							DestroyImmediate (obj.gameObject);
						}
					}

					if (TComponent != null) {
						for (int i = 0; i < TComponent.terrainData.detailPrototypes.Length; i++) {
							TComponent.terrainData.SetDetailLayer (0, 0, i, new int[TComponent.terrainData.detailWidth, TComponent.terrainData.detailHeight]);
						}
						if (TComponent.terrainData.treeInstances != null) {
							TComponent.terrainData.treeInstances = new TreeInstance[0];
						}
						TComponent.terrainData.RefreshPrototypes ();
						TComponent.Flush ();
					}

				}

				#if UNITY_EDITOR
				EditorUtility.ClearProgressBar ();
				#endif

			}

			//Organizes the objects in the scene (NOTE: after this, you can't edit the terrain or any related objects with the landscape generator)
			public void Finish () {

				#if UNITY_EDITOR
				if (GeneralSettings.AutoGenerate) {
					ProgressBarProgress = 0.50f;
					EditorUtility.DisplayProgressBar ("Landscape Genertor", "Compiling terrain", ProgressBarProgress);
				} else if (!GeneralSettings.AutoGenerate && !GeneralSettings.GenerateSeparately) {
					ProgressBarProgress = 0.50f;
					EditorUtility.DisplayProgressBar ("Landscape Genertor", "Compiling terrain", ProgressBarProgress);
				}
				#endif

				SceneObjs = FindObjectsOfType<LandGenObj> ();
				GameObject Landscape = new GameObject ("Landscape");

				//Parents terrain to terrain folder
				foreach (LandGenObj obj in SceneObjs) {
					obj.transform.parent = Landscape.transform;
					if (obj.type == LandGenObj.ObjectType.Wind) {
						obj.gameObject.name = "WindZone";
					}
					DestroyImmediate (obj.gameObject.GetComponent<LandGenObj> ());
				}

				TerrainSettings.Terrain = null;
				#if UNITY_EDITOR
				EditorUtility.ClearProgressBar ();
				#endif

			}
			#endregion ExtraFuctions

		}
	}
}
