//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.CoLocationMUVR
{
	public class ARCameraPlayerManager : MonoBehaviour
	{
		private float _startMouseDownTime;
		private float inputWaitingTime = 1f;

		public Camera targetCamera;
		
		public CameraPlayerUI targetCameraUI;

		//toggle camera UI
		public void Update()
		{
			if (this.targetCameraUI != null)
			{
				if (!this.targetCameraUI.gameObject.activeInHierarchy)
				{
					this.targetCameraUI.gameObject.SetActive(true);
					this._startMouseDownTime = Time.time;
				}
				
				if (Input.GetMouseButtonDown(0))
					this._startMouseDownTime = Time.time;
				
				if (Input.GetMouseButton(0) && this._startMouseDownTime + inputWaitingTime < Time.time)
				{
					this.targetCameraUI.ToggleCameraUI(true);
					this._startMouseDownTime = Time.time;
				}
			}
		}
	}
}
