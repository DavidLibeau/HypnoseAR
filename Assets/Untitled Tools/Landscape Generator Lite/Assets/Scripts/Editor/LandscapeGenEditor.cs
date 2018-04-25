using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UntitledTools {
	namespace LandscapeGeneration {
		//The script for the editor of the main generation script
		[CustomEditor(typeof(LandscapeGeneratorLite))]
		public class LandscapeGenEditor : Editor {

			private int AlwaysActiveTimer = 0;

			public override void OnInspectorGUI () {

				DrawDefaultInspector ();
				LandscapeGeneratorLite LandGen = (LandscapeGeneratorLite)target;

				AlwaysActiveTimer--;
				if (AlwaysActiveTimer < 1) {
					LandGen.AlwaysActive ();
					AlwaysActiveTimer = 25;
				}

				GUILayout.Space (20f);

				//If the auto-generate is on then generate everything on one button press
				if (LandGen.GeneralSettings.AutoGenerate) {

					//Destroys terrain, then generates everything and organizes it
					if (GUILayout.Button ("Create Landscape", GUILayout.Height (35f))) {
						LandGen.AlwaysActive ();
						LandGen.Destroy ();
						LandGen.GenerateTerrain ();
						LandGen.AlwaysActive ();
						LandGen.Finish ();
					}

				//If generate separately is on, then show more generation options
				} else if (LandGen.GeneralSettings.GenerateSeparately) {
					
					//Generates only the terrain
					if (GUILayout.Button ("Generate Terrain", GUILayout.Height (25f))) {
						LandGen.GenerateTerrain ();
					}

					GUILayout.BeginHorizontal ();

					//Destroys the current terrain
					if (GUILayout.Button ("Destroy", GUILayout.Height (35f))) {
						LandGen.AlwaysActive ();
						LandGen.Destroy ();
					}

					//Organizes and removes tags of all objects
					if (GUILayout.Button ("Finish", GUILayout.Height (35f))) {
						LandGen.AlwaysActive ();
						LandGen.Finish ();
					}

					GUILayout.EndHorizontal ();

				//Otherwise, if no special generation options are selected, show the default buttons
				} else {

					GUILayout.BeginHorizontal ();

					//Generates terrain, trees, water, and detail objects
					if (GUILayout.Button ("Generate", GUILayout.Height (35f))) {
						LandGen.AlwaysActive ();
						LandGen.Destroy ();
						LandGen.GenerateTerrain ();
						LandGen.AlwaysActive ();
					}

					//Destroys the current terrain
					if (GUILayout.Button ("Destroy", GUILayout.Height (35f))) {
						LandGen.AlwaysActive ();
						LandGen.Destroy ();
					}

					//Organizes and removes tags of all objects
					if (GUILayout.Button ("Finish", GUILayout.Height (35f))) {
						LandGen.AlwaysActive ();
						LandGen.Finish ();
					}

					GUILayout.EndHorizontal ();

				}

			}

		}
	}
}
