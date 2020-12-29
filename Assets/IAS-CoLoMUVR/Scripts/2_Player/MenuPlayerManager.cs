//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace IAS.CoLocationMUVR
{
	public class MenuPlayerManager : MonoBehaviour
	{
		public GameObject head;
		public VrController leftHand;
		public VrController rightHand;
		
		private BodyScaleManager _targetBodyScaleManager;
		
		void Update()
		{
			//update hands oritentation
			this.leftHand.UpdateTransform();
			this.rightHand.UpdateTransform();

			//update head oritentation
			Vector3 centerEyePosition = Vector3.zero;
			Quaternion centerEyeRotation = Quaternion.identity;
			
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.CenterEye, NodeStatePropertyType.Position, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out centerEyePosition))
				this.head.transform.localPosition = centerEyePosition;
			if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.CenterEye, NodeStatePropertyType.Orientation, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out centerEyeRotation))
				this.head.transform.localRotation = centerEyeRotation;
			
			//input to calculate the body scale
			if (Input.GetKeyDown(KeyCode.O) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch))
			{
				this.CalculateBodyScaleManager();
			}
		}

		//calcualte the body scale 
		void CalculateBodyScaleManager()
		{
			if (this._targetBodyScaleManager == null)
				this._targetBodyScaleManager = this.gameObject.GetComponent(typeof(BodyScaleManager)) as BodyScaleManager;
			
			if (this._targetBodyScaleManager != null)
			{
				this._targetBodyScaleManager.Calibrate();
				GlobalVariables.bodyScale = this._targetBodyScaleManager.currentSizeValue;
				GlobalVariables.armScale = this._targetBodyScaleManager.currentArmSizeValue;
			}
		}
	}
}
