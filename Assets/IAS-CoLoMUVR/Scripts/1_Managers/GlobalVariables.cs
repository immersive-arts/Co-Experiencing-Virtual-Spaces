//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.CoLocationMUVR
{
	public static class GlobalVariables
	{
		//can be used if the players have different calibration points
		public static int trackingIndex = 0;
		//change this to spawn a diffrent VRplayer prefab
		public static int playerTypeIndex = 0;
		
		public static bool joinAsVRPlayer = true;
		
		public static float bodyScale = 1f;
		public static float armScale = 1f;

		public static bool showCameras = true;

		//if false: objects with the DebugVisualObject.cs will SetActive(false) on Start
		public static bool showDebugObjects = false;

		//oculus touch controller offset
		public static Vector3 centerPositionControllerOffset = new Vector3(0.013f, 0.029f, 0.008f);
		
		public static void ToggleShowCamera()
		{
			showCameras = !showCameras;
		}

		
		public static void LoadPlayerPrefs()
		{
			if (PlayerPrefs.HasKey("TrackingIndex"))
				trackingIndex = PlayerPrefs.GetInt("TrackingIndex");
		}
		
		public static void SavePlayerPrefs()
		{
			PlayerPrefs.SetInt("TrackingIndex", trackingIndex);
			
			PlayerPrefs.Save();
		}
	}
}
