//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.CoLocationMUVR
{
	public class CenterOffsetButton : MonoBehaviour
	{
		public static CenterOffsetButton Instance;
		
		private Transform calculationTarget;
		
		public Transform offsetChildTransform;
		private SphereCollider _targetCollider;
		
		private void Start()
		{
			if (Instance != null)
				Destroy(Instance.gameObject);
			
			Instance = this;

			//if scene have a calibration offset
			if (this.offsetChildTransform != null && GameManager.Instance != null)
			{
				this.offsetChildTransform.localPosition = GameManager.Instance.arCameraCalibrationOffset;
				this._targetCollider = this.gameObject.GetComponent(typeof(SphereCollider)) as SphereCollider;

				if (this._targetCollider != null)
					this._targetCollider.center += GameManager.Instance.arCameraCalibrationOffset;
			}
		}

		//calculate room center point
		public void CalculateCenterOffset()
		{
			if (GameManager.Instance.localVRPlayer != null)
			{
				GameManager.Instance.localVRPlayer.transform.localPosition = Vector3.zero;
				GameManager.Instance.localVRPlayer.transform.localRotation = Quaternion.identity;
				
				if (this.calculationTarget == null)
				{
					this.calculationTarget = new GameObject().transform;
				}
				
				this.calculationTarget.localPosition = this.offsetChildTransform.position;
				this.calculationTarget.localRotation = this.offsetChildTransform.rotation;
				
				//set offset
				GameManager.Instance.localVRPlayer.transform.localPosition = this.calculationTarget.InverseTransformPoint(Vector3.zero);
				GameManager.Instance.localVRPlayer.transform.localRotation = Quaternion.Inverse(this.calculationTarget.rotation);

				if (CameraPlayerUI.Instance != null)
				{
					CameraPlayerUI.Instance.ToggleEnvironmnetVisual(true);
					//CameraPlayerUI.Instance.ToggleCalibrationCurserVisual(false);
				}
			}
		}
		
		private void OnMouseDown()
		{
			if (GameManager.Instance != null)
				this.CalculateCenterOffset();
		}
	}
}
