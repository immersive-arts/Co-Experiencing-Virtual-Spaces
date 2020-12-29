//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAS.VRIK;

namespace IAS.CoLocationMUVR
{
	public class BodyScaleManager : MonoBehaviour {
		public GameObject targetBody;
		private VRPlayerManager _targetvrPlayerManager;
		
		[Range(0f,2f)]
		public float currentSizeValue = 1f;
		public float baseEyeHeight; //this is scale [1,1,1]
		public GameObject headTarget;
		
		[Header("Arm scaling")]
		public bool neutralizeHandScale = true;
		public float baseArmLength; //the length of one arm if the scale is [1,1,1] 
		[Range(0.1f, 2f)]
		public float currentArmSizeValue = 1f;
		public GameObject leftHandTarget;
		public GameObject rightHandTarget;
		
		public GameObject leftUpperarmBone;
		public GameObject leftForearmBone;
		public GameObject leftHandBone;
		
		public GameObject rightUpperarmBone;
		public GameObject rightForearmBone;
		public GameObject rightHandBone;
		
		private bool _updateInNextFrame = false;
		
		public bool multiplyWithHandScale = true;
		public VrController targetLeftHandController;
		public VrController targetRightHandController;
		private Vector3 _handScale = Vector3.one;
		private bool _lastFingerTrakingState = false;

		
		private void Start()
		{
			this._targetvrPlayerManager = this.gameObject.GetComponent(typeof(VRPlayerManager)) as VRPlayerManager;
		}

		//set values by player start or from server data
		public void SetValues(float bodySize, float armSize)
		{
			this.currentSizeValue = bodySize;
			this.currentArmSizeValue = armSize;
			
			this._updateInNextFrame = true;
		}
		
		private void Update()
		{
			#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.C))
			this.Calibrate();
			#endif
			if (this._updateInNextFrame)
			{
				this.UpdateScaleValue();
				this.UpdateArmeScale();
				this._updateInNextFrame = false;
			}

			//update hand scale to server
			if (this.multiplyWithHandScale && this.rightHandBone != null && this.leftHandBone != null
			    && this.targetRightHandController != null && this.targetLeftHandController != null)
			{
				if (this._lastFingerTrakingState != this.targetRightHandController.DoFingerTracking() && this._lastFingerTrakingState != this.targetLeftHandController.DoFingerTracking() && this._targetvrPlayerManager == null ||
				    this._lastFingerTrakingState != this.targetRightHandController.DoFingerTracking() && this._lastFingerTrakingState != this.targetLeftHandController.DoFingerTracking() && this._targetvrPlayerManager != null && this._targetvrPlayerManager.isLocalPlayer)
				{
					//if finger tracking is active
					if (this._lastFingerTrakingState == false)
					{
						float leftRootScale = this.targetRightHandController.handSkeleton.GetRootScale();
						float rightRootScale = this.targetLeftHandController.handSkeleton.GetRootScale();
						
						this.rightHandBone.transform.localScale = this._handScale * leftRootScale;
						this.leftHandBone.transform.localScale = this._handScale * rightRootScale;
						if (this._targetvrPlayerManager != null && this._targetvrPlayerManager.isLocalPlayer)
							this._targetvrPlayerManager.CmdChangeHandScaleOnServer((leftRootScale + rightRootScale) / 2f);

						this._lastFingerTrakingState = true;
					}
					else
					{
						this.rightHandBone.transform.localScale = this._handScale;
						this.leftHandBone.transform.localScale = this._handScale;
						if (this._targetvrPlayerManager != null && this._targetvrPlayerManager.isLocalPlayer)
							this._targetvrPlayerManager.CmdChangeHandScaleOnServer(1f);

						this._lastFingerTrakingState = false;
					}
				}
			}
		}

		//update hand root scale from server
		public void UpdateTrackedHandRootScale(float size)
		{
			this.leftHandBone.transform.localScale = this._handScale * size;
			this.rightHandBone.transform.localScale = this._handScale * size;
		}

		//calibrate the body size and amr length
		public void Calibrate()
		{
			this.CalculateSizeValue();
			this.CalculateArmeSizeValue();
		}
		
		public void CalculateSizeValue()
		{
			if (this.headTarget != null)
			{
				Vector3 headPos = this.transform.InverseTransformPoint(this.headTarget.transform.position);
				this.currentSizeValue = 1f / this.baseEyeHeight * headPos.y;

				//save to static variable to access it in game scenes
				GlobalVariables.bodyScale = this.currentSizeValue;
				this.UpdateScaleValue();
			}
		}

		//update the body object if the currentSizeValue has changed
		public void UpdateScaleValue()
		{
			if (this.targetBody != null)
			{
				this.targetBody.transform.localScale = new Vector3(this.currentSizeValue, this.currentSizeValue, this.currentSizeValue);
			}
		}

		public void CalculateArmeSizeValue()
		{
			float leftArmLength = 0f;
			float rightArmLength = 0f;

			float scaledBoneHandDistance = this.baseArmLength * this.currentSizeValue;
			//measure both arm lengths
			if (this.rightHandTarget != null && this.rightUpperarmBone != null)
				rightArmLength = Vector3.Distance(this.rightHandTarget.transform.position, this.rightUpperarmBone.transform.position);
			
			if (this.leftHandTarget != null && this.leftUpperarmBone != null)
				leftArmLength = Vector3.Distance(this.leftHandTarget.transform.position, this.leftUpperarmBone.transform.position);

			//calculate currentArmSizeValue 
			float trackedArmLength = (leftArmLength + rightArmLength) / 2f;

			this.currentArmSizeValue = 1f / scaledBoneHandDistance * trackedArmLength;
			this.currentArmSizeValue = Mathf.Clamp(this.currentArmSizeValue, 0.1f, 2f);

			//save to static variable to access it in game scenes
			GlobalVariables.armScale = this.currentArmSizeValue;
			this.UpdateArmeScale();
		}
		
		public void UpdateArmeScale()
		{
			if (this.leftUpperarmBone != null && this.leftForearmBone != null && this.leftHandBone != null &&
			    this.rightUpperarmBone != null && this.rightForearmBone != null && this.rightHandBone != null)
			{
				Vector3 scale = Vector3.one;
				scale.x = this.currentArmSizeValue;
				scale.y = this.currentArmSizeValue;
				scale.z = this.currentArmSizeValue;
				
				this.leftUpperarmBone.transform.localScale = scale;
				//this.leftForearmBone.transform.localScale = scale;
				
				this.rightUpperarmBone.transform.localScale = scale;
				//this.rightForearmBone.transform.localScale = scale;
				
				float offset = 1f / this.currentArmSizeValue * 1f;

				//inverted scaling of the hands
				if (this.neutralizeHandScale)
				{
					float bodyOffset = 1f / this.currentSizeValue * 1f;
					scale.x = offset * bodyOffset;
					scale.y = offset * bodyOffset;
					scale.z = offset * bodyOffset;
					
					this.leftHandBone.transform.localScale = scale;
					this.rightHandBone.transform.localScale = scale;
					this._handScale = scale;
				}
				else
				{
					scale.x = offset;
					scale.y = offset;
					scale.z = offset;
					
					this.leftHandBone.transform.localScale = scale;
					this.rightHandBone.transform.localScale = scale;
					this._handScale = scale;
				}
			}
		}
	}
}
