//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SpatialTracking;

namespace IAS.CoLocationMUVR
{
	public class CameraPlayerManager : VRPlayerManager
	{
		public GameObject arCameraManagerPrefab;
		[HideInInspector]
		public ARCameraPlayerManager spawnedArCameraManager;
		public GameObject cameraModel;
		

		public override void OnStartLocalPlayer()
		{
			//base.OnStartLocalPlayer();
			if (GameManager.Instance != null)
			{
				GameManager.Instance.localVRPlayer = this;
			}

			//turn of local camera mesh
			if (this.cameraModel != null)
				this.cameraModel.SetActive(false);
		}

		
		public override void Update()
		{
			//if is local player, create the AR camera
			if (this.isLocalPlayer && this.spawnedArCameraManager == null)
			{
				if (this.arCameraManagerPrefab != null)
					this.spawnedArCameraManager = Instantiate(this.arCameraManagerPrefab, this.transform).GetComponent(typeof(ARCameraPlayerManager)) as ARCameraPlayerManager;

			}
			//if local: update network target object orientation to AR camera
			if (this.isLocalPlayer && this.head != null && this.spawnedArCameraManager != null)
			{
				this.head.transform.localPosition = this.spawnedArCameraManager.targetCamera.transform.localPosition;
				this.head.transform.localRotation = this.spawnedArCameraManager.targetCamera.transform.localRotation;
			}
			
			//toggle camera mesh
			if (!this.isLocalPlayer && this.cameraModel != null)
			{
				if (GlobalVariables.showCameras && !this.cameraModel.activeInHierarchy)
					this.cameraModel.SetActive(true);
				
				if (!GlobalVariables.showCameras && this.head.activeInHierarchy)
					this.cameraModel.SetActive(false);
			}
		}
		
		
	}

}
