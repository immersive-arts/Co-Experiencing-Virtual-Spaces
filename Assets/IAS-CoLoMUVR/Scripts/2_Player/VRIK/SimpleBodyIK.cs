//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.CoLocationMUVR
{
	[System.Serializable]
	public class TwoPointIK
	{
		//based of https://wirewhiz.com/how-to-code-two-bone-ik-in-unity/
		public Transform upper;//root of upper arm
		public Transform lower;//root of lower arm
		public Transform end;//root of hand
		public Transform target;//target position of hand
		public Transform pole;//direction to bend towards 
		public float upperElbowRotation;//Rotation offsetts
		public float lowerElbowRotation;

		private float _a;//values for use in cos rule
		private float _b;
		private float _c;
		private Vector3 _en;//Normal of plane we want our arm to be on

		public void Map()
		{
			if (this.upper == null || this.lower == null || this.end == null || this.target == null || this.pole == null)
				return;

			this._a = Vector3.Distance(this.upper.position, this.lower.position);
			this._b = Vector3.Distance(this.lower.position, this.end.position);

			this._c = Vector3.Distance(this.upper.position, this.target.position);
			this._en = Vector3.Cross(this.target.position - this.upper.position, this.pole.position - this.upper.position);

			//Set the rotation of the upper arm
			this.upper.rotation = Quaternion.LookRotation(this.target.position - this.upper.position, Quaternion.AngleAxis(this.upperElbowRotation, lower.position - upper.position) * (_en));
			this.upper.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, lower.localPosition));
			this.upper.rotation = Quaternion.AngleAxis(-CosAngle(_a, _c, _b), -_en) * upper.rotation;

			//set the rotation of the lower arm
			lower.rotation = Quaternion.LookRotation(target.position - lower.position, Quaternion.AngleAxis(lowerElbowRotation, end.position - lower.position) * (_en));
			lower.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, end.localPosition));

			//Rotate teh end object
			this.end.rotation = target.rotation;
		}

		//function that finds angles using the cosine rule 
		float CosAngle(float a, float b, float c)
		{
			if (!float.IsNaN(Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (-2 * a * b)) * Mathf.Rad2Deg))
			{
				return Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (2 * a * b)) * Mathf.Rad2Deg;
			}
			else
			{
				return 1;
			}
		}
	}

	public class SimpleBodyIK : MonoBehaviour
	{
		public float turnSmoothing = 3f;
		public Transform cameraTransform;
		public Transform headConstraint;
		public bool justPosition = false;

		public bool updateMovingDirectionByGlobalMovement = true;

		private Vector3 _lastPosition;

		[Header("Head")]
		public Transform headBone;
		public Transform neckBone;
		[Range(0f, 1f)]
		public float neckWeight = 1f;

		[Header("Hands")]
		public TwoPointIK leftHand;
		public TwoPointIK rightHand;

		[Header("Legs")]
		public Animator animatorController;
		public bool animateLegs = true;
		public float legAnimationBlendMultiplier = 13f;

		public bool flatFeetOnTheGround = true;
		public float feetYPositionOffest;
		public LayerMask feetRayCastLayers;
		public float animationSmooth = 8f;

		public Transform rightHintKnee;
		[Range(0f, 1f)]
		public float rightFootPosWeight;
		private Vector3 _rightFootPos;
		[Range(0f, 1f)]
		public float rightFootRotWeight;
		[Range(0f, 1f)]
		public float rightHintKneeWeight;

		public Transform leftHintKnee;
		[Range(0f, 1f)]
		public float leftFootPosWeight;
		private Vector3 _leftFootPos;
		[Range(0f, 1f)]
		public float leftFootRotWeight;
		[Range(0f, 1f)]
		public float leftHintKneeWeight;

		void LateUpdate()
		{

			//rotate Body
			if (this.headConstraint != null && this.cameraTransform != null)
			{
				this.TrackHead();
				this.transform.position = this.cameraTransform.position - (this.headConstraint.position - this.transform.position);

				if (!this.justPosition)
					this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(this.cameraTransform.forward, this.transform.up), this.transform.up), this.turnSmoothing * Time.deltaTime);

			}

			//move arms
			this.leftHand.Map();
			this.rightHand.Map();

			//move legs
			if (this.animateLegs && this.animatorController != null && this.cameraTransform != null)
			{
				Vector3 dir = this.GetCameraMovingDirection();

				float lastX = this.animatorController.GetFloat("X");
				float lastY = this.animatorController.GetFloat("Y");

				this.animatorController.SetFloat("X", Mathf.Lerp(lastX, dir.x * this.legAnimationBlendMultiplier, this.animationSmooth * Time.deltaTime));
				this.animatorController.SetFloat("Y", Mathf.Lerp(lastY, dir.z * this.legAnimationBlendMultiplier, this.animationSmooth * Time.deltaTime));
			}

		}

		//rotate head/neck bones
		void TrackHead()
		{
			if (this.headBone != null && this.neckBone != null)
			{
				this.neckBone.rotation = Quaternion.Slerp(Quaternion.identity, this.cameraTransform.rotation, this.neckWeight);
				this.headBone.rotation = this.cameraTransform.rotation;
			}
		}

		public Vector3 GetCameraMovingDirection()
		{
			if (this.updateMovingDirectionByGlobalMovement)
			{
				//Doesnt work with rotating environment/Player
				Vector3 direction = this.cameraTransform.position - this._lastPosition;
				this._lastPosition = this.cameraTransform.position;

				direction = Quaternion.Euler(this.cameraTransform.localEulerAngles * -1) * direction;
				return direction;
			}
			else
			{
				Vector3 direction = this.cameraTransform.localPosition - this._lastPosition;
				this._lastPosition = this.cameraTransform.localPosition;

				direction = Quaternion.Euler(this.cameraTransform.localEulerAngles * -1) * direction;
				return direction;
			}
		}

		private void OnAnimatorIK(int layerIndex)
		{
			if (this.flatFeetOnTheGround && this.animatorController != null)
			{
				this._rightFootPos = this.animatorController.GetIKPosition(AvatarIKGoal.RightFoot);

				if (Physics.Raycast(this._rightFootPos + this.transform.up, this.transform.up * -1, out RaycastHit hit, 1f, this.feetRayCastLayers))
				{
					this.animatorController.SetIKPositionWeight(AvatarIKGoal.RightFoot, this.rightFootPosWeight);
					this.animatorController.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + hit.normal * this.feetYPositionOffest);

					if (this.rightHintKnee != null)
					{
						this.animatorController.SetIKHintPosition(AvatarIKHint.RightKnee, this.rightHintKnee.position);
						this.animatorController.SetIKHintPositionWeight(AvatarIKHint.RightKnee, this.rightHintKneeWeight);
					}

					this.animatorController.SetIKRotationWeight(AvatarIKGoal.RightFoot, this.rightFootRotWeight);
					this.animatorController.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal));
				}
				else
				{
					this.animatorController.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
					this.animatorController.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
					this.animatorController.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 0);
				}

				this._leftFootPos = this.animatorController.GetIKPosition(AvatarIKGoal.LeftFoot);


				if (Physics.Raycast(this._leftFootPos + this.transform.up, this.transform.up * -1, out RaycastHit hitLeft, 1f, this.feetRayCastLayers))
				{
					this.animatorController.SetIKPositionWeight(AvatarIKGoal.LeftFoot, this.leftFootPosWeight);
					this.animatorController.SetIKPosition(AvatarIKGoal.LeftFoot, hitLeft.point + hitLeft.normal * this.feetYPositionOffest);

					if (this.leftHintKnee != null)
					{
						this.animatorController.SetIKHintPosition(AvatarIKHint.LeftKnee, this.leftHintKnee.position);
						this.animatorController.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, this.leftHintKneeWeight);
					}

					this.animatorController.SetIKRotationWeight(AvatarIKGoal.LeftFoot, this.leftFootRotWeight);
					this.animatorController.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hitLeft.normal), hitLeft.normal));
				}
				else
				{
					this.animatorController.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
					this.animatorController.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
					this.animatorController.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 0);
				}
			}
		}
	}
}

