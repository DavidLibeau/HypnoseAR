using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace UntitledTools {
	namespace LandscapeGeneration {

		/// <summary>
		/// The height map wizard.
		/// </summary>
		public class HeightMapWizard : ScriptableWizard {

			[Space(20f)]
			[Tooltip("The height map importer's settings.")]
			public VariableClasses.ImportVars ImportHeightMap = new VariableClasses.ImportVars ();
			[Space(7.5f)]
			[Tooltip("The height map exporter's settings.")]
			public VariableClasses.ExportVars ExportHeightMap = new VariableClasses.ExportVars ();

			/// <summary>
			/// Creates the wizard.
			/// </summary>
			[MenuItem("Untitled Tools/Landscape Generator Lite/Height Map Wizard")]
			public static void CreateWizard () {
				HeightMapWizard Wizard = ScriptableWizard.DisplayWizard<HeightMapWizard> ("Height Map Wizard", "Export", "Import");
				Wizard.position = new Rect (Display.main.systemWidth / 2, Display.main.systemHeight / 2, 750, 500);
				Wizard.Focus ();
			}

			/// <summary>
			/// Raises the wizard update event.
			/// </summary>
			public void OnWizardUpdate () {
				if (ExportHeightMap.AutoExportPath) {
					ExportHeightMap.ExportPath = Application.dataPath;
				}
			}

			/// <summary>
			/// Raises the wizard create event.
			/// </summary>
			public void OnWizardCreate () {

				if (ExportHeightMap.TerrainToUse != null) {

					TerrainData Tdata = ExportHeightMap.TerrainToUse.terrainData;
					Texture2D CompiledHeightMap = new Texture2D (Tdata.heightmapWidth, Tdata.heightmapHeight);
					Texture2D CompiledDetailMap = new Texture2D (Tdata.detailWidth, Tdata.detailHeight);

					float[,] Heights = Tdata.GetHeights (0, 0, Tdata.heightmapWidth, Tdata.heightmapHeight);
					for (int x = 0; x < Tdata.heightmapWidth; x++) {
						for (int y = 0; y < Tdata.heightmapHeight; y++) {
							float Val = Heights [x, y];
							Color PixelColor = new Color (Val, Val, Val, Val);
							CompiledHeightMap.SetPixel (x, y, PixelColor);
						}
					}

					int[,] DetailMap = Tdata.GetDetailLayer (0, 0, Tdata.detailWidth, Tdata.detailHeight, 0);
					for (int x = 0; x < Tdata.detailWidth; x++) {
						for (int y = 0; y < Tdata.detailHeight; y++) {
							float Val = (float)DetailMap [x, y];
							Color PixelColor = new Color (Val, Val, Val, Val);
							CompiledDetailMap.SetPixel (x, y, PixelColor);
						}
					}

					CompiledHeightMap.alphaIsTransparency = false;
					CompiledHeightMap.Apply ();
					CompiledDetailMap.alphaIsTransparency = false;
					CompiledDetailMap.Apply ();
					GeneratorAPIs.IO.SaveTextureAsPNG (ExportHeightMap.ExportPath, CompiledDetailMap, "Detail_Map_Export");
					GeneratorAPIs.IO.SaveTextureAsPNG (ExportHeightMap.ExportPath, CompiledHeightMap, ExportHeightMap.FileName);

					EditorUtility.DisplayDialog ("Height Map Wizard", "Exported Height Map To: \"" + ExportHeightMap.ExportPath + "\"", "Ok");
					this.Close ();

				} else {
					EditorUtility.DisplayDialog ("Height Map Wizard", "Please Fill All Fields!", "Ok");
					CreateWizard ();
				}

			}

			/// <summary>
			/// Raises the wizard other button event.
			/// </summary>
			public void OnWizardOtherButton () {

				if (ImportHeightMap.HeightMap != null && ImportHeightMap.TerrainToUse != null) {

					TerrainData Tdata = ImportHeightMap.TerrainToUse.terrainData;
					float[,] Heights = new float[Tdata.heightmapWidth, Tdata.heightmapHeight];
					for (int x = 0; x < Tdata.heightmapWidth; x++) {
						for (int y = 0; y < Tdata.heightmapHeight; y++) {
							float GreyscaleVal = ImportHeightMap.HeightMap.GetPixel (x, y).grayscale;
							Heights [x, y] = GreyscaleVal;
						}
					}

					Tdata.size = new Vector3 (Tdata.size.x, ImportHeightMap.Intensity, Tdata.size.z);
					Tdata.SetHeights (0, 0, Heights);

					EditorUtility.DisplayDialog ("Height Map Wizard", "Imported Height Map.", "Ok");
					this.Close ();

				} else {
					EditorUtility.DisplayDialog ("Height Map Wizard", "Please Fill All Fields!", "Ok");
				}

			}

			/// <summary>
			/// Export/Import Variable classes.
			/// </summary>
			public class VariableClasses {

				/// <summary>
				/// Import variables.
				/// </summary>
				[System.Serializable]
				public class ImportVars {
					[Tooltip("The height map texture that the importer will apply to the terrain.")]
					public Texture2D HeightMap;
					[Tooltip("The terrain that the importer will apply the height map on.")]
					public Terrain TerrainToUse;
					[Range(10f, 1000f)]
					[Tooltip("The literal world-space maximum height that the terrain will be set to.")]
					public float Intensity = 100f;
				}

				/// <summary>
				/// Export variables.
				/// </summary>
				[System.Serializable]
				public class ExportVars {
					[Tooltip("The terrain that the exporter will collect data from.")]
					public Terrain TerrainToUse;
					[TextArea]
					[Tooltip("The folder directory that the exporter will compile a png height map file to.")]
					public string ExportPath = "C:\\";
					[Tooltip("Set this to true if you want the export directory to be this project's assets folder.")]
					public bool AutoExportPath = true;
					[Tooltip("What the exporter will name the new png file.")]
					public string FileName = "Saved_Height_Map";
				}

			}

		}

	}
}
