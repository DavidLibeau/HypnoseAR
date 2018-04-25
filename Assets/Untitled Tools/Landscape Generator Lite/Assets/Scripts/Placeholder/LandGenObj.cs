using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UntitledTools {
	namespace LandscapeGeneration {

		//A placeholder component that is used to locate the in-use landscape generator objects.
		[DisallowMultipleComponent]
		[AddComponentMenu("Untitled Tools/Landscape Generator Lite/Placeholder")]
		public class LandGenObj : MonoBehaviour {
			//Stores the object's type
			public ObjectType type = ObjectType.Other;
			public enum ObjectType {
				Landscape, Wind, Other
			};
		}

	}
}
