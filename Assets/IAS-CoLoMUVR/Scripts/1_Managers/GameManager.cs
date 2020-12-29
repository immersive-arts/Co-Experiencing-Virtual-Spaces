//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace IAS.CoLocationMUVR
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance;
		//get the local player of the client/host
		public VRPlayerManager localVRPlayer;
		public bool gameIsRunning;
		
		//scene specific offset of the AR camera calibration
		public Vector3 arCameraCalibrationOffset = Vector3.zero;

		//own guardian mesh
		public GameObject roomGuardianObject;
		
		private void Awake()
		{
			Instance = this;
		}
		
		private void Start()
		{
			//turn of the guardian mesh if you are not a VR player
			if (!GlobalVariables.joinAsVRPlayer)
				this.DeactivateGuardianWall();

		}

		public void DeactivateGuardianWall()
		{
			if (this.roomGuardianObject != null)
				this.roomGuardianObject.SetActive(false);
		}
	}
}
