using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UntitledTools {
	namespace LandscapeGeneration {
		
		//All this does is run the landscape generator script that is attached to the gameobject
		[AddComponentMenu("Untitled Tools/Landscape Generator Lite/Generator Run On Start")]
		public class GeneratorRunOnStart : MonoBehaviour {

			[Tooltip("The generator that will be run when the start method is called.")]
			public LandscapeGeneratorLite Generator;
			[Tooltip("Will the generator run when the start method is called?")]
			public bool RunOnStart;

			/// <summary>
			/// Runs the selected generator.
			/// </summary>
			public void Start () {

				//Runs only if run-on-start is checked
				if (RunOnStart) {
					Generator.GeneralSettings.GenerateSeparately = false;
					Generator.AlwaysActive ();
					Generator.Destroy ();
					Generator.GenerateTerrain ();
				}
					
			}

		}

	}
}
