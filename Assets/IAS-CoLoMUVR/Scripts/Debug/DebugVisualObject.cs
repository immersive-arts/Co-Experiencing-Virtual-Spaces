//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.CoLocationMUVR
{
	public class DebugVisualObject : MonoBehaviour
	{
		//Mostly used for centerpoints to check the trackingdata if needed
		void Start()
		{
			if (!GlobalVariables.showDebugObjects)
				this.gameObject.SetActive(false);

		}

	}
}