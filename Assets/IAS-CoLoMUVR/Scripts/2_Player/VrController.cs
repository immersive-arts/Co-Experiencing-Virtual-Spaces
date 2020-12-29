//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.XR;
using Mirror;

namespace IAS.CoLocationMUVR
{
	public class VrController : MonoBehaviour {
		
		private Transform _transform;
		
		public VRPlayerManager targetVRPlayer;
		public OVRInput.Controller m_controller;
		public OVRInput.Controller m_hand;
		public HandState currentHandState;
		private OVRHand _ovrHand;
		
		public Animator animatorComp;
		private string _grabAnimationParameterName;
		
		public Transform noTrackingIdlePosition; //override IK position if the cameras don't detect the hands

		//grabing
		List<IGrabableObject> objectsHoveringOver = new List<IGrabableObject>();
		public IGrabableObject closestObj;
		private IGrabableObject _lastClosestObj;
		public IGrabableObject interactingObj;
		
		private bool _isGrapDown = false;
		
		//drag
		public Transform interactionPoint;
		private Vector3 posDelta;
		
		private Quaternion rotationDelta;
		private float angel;
		private Vector3 axis;

		public bool doClosestHoverObjectActions = false;


		[Header("Finger Tracking Aproximation")]
		
		public OVRSkeleton handSkeleton;
		
		public Finger[] fingers;
		//if finger tracking as active
		private bool _doFingerTracking;
		public bool DoFingerTracking() { return this._doFingerTracking; }
		private float _lastFingerValueUpdateTime;
		[Range(0f,1f)]
		public float fingerTrackingSyncInterval = 0.1f;

		//Ik offset for controller tracking and for hand tracking
		public Transform ikOffsetObject;
		public Vector3 offsetControllerPosition;
		public Vector3 offsetControllerRotation;
		public Vector3 offsetHandPosition;
		public Vector3 offsetHandRotation;
		
		[System.Serializable]
		public class Finger
		{
			public Transform bone0;
			public Transform bone1;
			public Transform bone2;
			//for local bendValue calculation
			public float aAngleValue = 720f;
			public float bAngleValue = 520f;

			//for networking bendValue recalculation
			public float aReMapAngleValue = 720f;
			public float bReMapAngleValue = 520f;
			
			private float _angleValue;
			
			private Quaternion _networkRotation0;
			private float _networkAngleZ1;
			private float _networkAngleZ2;
			
			public float lerpValue = 0.4f;

			//calculate local data
			public float CalculateAngleValue()
			{
				if (bone1 != null && bone2 != null)
				{
					float currentAngleValue = bone1.localEulerAngles.z;
					
					if (currentAngleValue < 180f)
						currentAngleValue += 360f;
					
					currentAngleValue += bone2.localEulerAngles.z;
					if (bone2.localEulerAngles.z < 180f)
						currentAngleValue += 360f;
					
					this._angleValue = Mathf.InverseLerp(aAngleValue, bAngleValue, currentAngleValue);
				}
				
				return _angleValue;
			}
			
			public Quaternion GetNetworkRotation0() { return this._networkRotation0; }
			public float GetAngleValue() { return this._angleValue; }
			
			//update bones by networking data
			public void SmoothFingerFollow()
			{
				this.bone0.localRotation = this._networkRotation0;
				this.bone1.localEulerAngles =  new Vector3(this.bone1.localEulerAngles.x, this.bone1.localEulerAngles.y,
				                                           Mathf.LerpAngle(this.bone1.localEulerAngles.z, this._networkAngleZ1, this.lerpValue));
				
				this.bone2.localEulerAngles = new Vector3(this.bone2.localEulerAngles.x, this.bone2.localEulerAngles.y,
				                                          Mathf.LerpAngle(this.bone2.localEulerAngles.z, this._networkAngleZ2, this.lerpValue));
			}

			//calculate data by netwroking values
			public void SetData(Quaternion newRotation, float angelValue, OVRInput.Controller targetController)
			{
				this._networkRotation0 = newRotation;
				this._angleValue = angelValue;

				float angle1 = Mathf.Lerp(aReMapAngleValue, bReMapAngleValue, this._angleValue) / 2f;
				
				float angle2 = angle1;
				
				if (targetController == OVRInput.Controller.LHand)
				{
					angle1 -= 5f;
					angle2 += 5f;
				}
				else
				{
					angle1 += 5f;
					angle2 -= 5f;
				}
				
				if (angle1 >= 360f)
					angle1 -= 360f;
				if (angle2 >= 360f)
					angle2 -= 360f;
				
				this._networkAngleZ1 = angle1;
				this._networkAngleZ2 = angle2;
			}
		}
		
		
		private void Start()
		{
			this._transform = this.transform;
			this.SetIKOffsetObject();
			
			this._ovrHand = this.gameObject.GetComponent(typeof(OVRHand)) as OVRHand;
		}
		
		void Update () {
			
			if (this.targetVRPlayer != null && this.targetVRPlayer.isLocalPlayer || this.targetVRPlayer == null && !NetworkManager.singleton.isNetworkActive)
			{
				//if controller tracking update animation state
				if (OVRInput.IsControllerConnected(m_controller))
					this.UpdateHandAnimationState();
				//if hand tracking udpate networking values
				else if (!OVRInput.IsControllerConnected(this.m_controller) && this.targetVRPlayer != null && this.targetVRPlayer.isLocalPlayer && this._doFingerTracking)
					this.UpdateFingerValuesLocal();

				this.Grab();

				//activate/deactivate hand tracking
				if (this.animatorComp != null)
				{
					int i = 1;
					if (this.m_controller == OVRInput.Controller.RTouch)
						i = 2;
					
					if (OVRInput.GetActiveController() == OVRInput.Controller.Touch && this.animatorComp.GetLayerWeight(i) < 1f)
					{
						this.animatorComp.SetLayerWeight(i, 1f);
						this._doFingerTracking = false;
						this.SetIKOffsetObject();
					}
					else if (OVRInput.GetActiveController() == OVRInput.Controller.Hands && this.animatorComp.GetLayerWeight(i) > 0f)
					{
						this.animatorComp.SetLayerWeight(i, 0f);
						this._doFingerTracking = true;
						this.SetIKOffsetObject();
					}
				}
			}
		}
		
		private void LateUpdate()
		{
			//activate/deactivate hand tracking
			if (this.animatorComp != null && this.targetVRPlayer != null )
			{
				int i = 1;
				if (this.m_controller == OVRInput.Controller.RTouch)
					i = 2;
				
				if (!this._doFingerTracking)
				{
					if (this.animatorComp.GetLayerWeight(i) < 1f)
						this.animatorComp.SetLayerWeight(i, 1f);
				}
				else if (this.animatorComp.GetLayerWeight(i) > 0f)
					this.animatorComp.SetLayerWeight(i, 0f);
			}
			//douring hand tracking override base bone rotation
			if (this._doFingerTracking && this.handSkeleton != null && this.handSkeleton.Bones.Count > 0)
				this.handSkeleton.Bones[0].Transform.rotation = this.ikOffsetObject.rotation;
			
			//if not local player: update hand tracking by networking data
			if (this.targetVRPlayer != null && !this.targetVRPlayer.isLocalPlayer && this._doFingerTracking)
			{
				foreach (Finger finger in this.fingers)
					finger.SmoothFingerFollow();
			}
		}

		//update position
		public void UpdateTransform()
		{
			if (OVRInput.GetActiveController() == OVRInput.Controller.Touch || OVRInput.GetActiveController() == this.m_controller)
			{
				if (OVRInput.GetControllerPositionValid(this.m_controller))
				{
					this._transform.localPosition = OVRInput.GetLocalControllerPosition(this.m_controller);
					this._transform.localRotation = OVRInput.GetLocalControllerRotation(this.m_controller);
				}
			}
			else
			{
				if (PauseUI.Instance != null && PauseUI.Instance.calibrationGameObject && PauseUI.Instance.calibrationGameObject.activeInHierarchy)
				{
					//during calibration no update of hand position if not controller tracking
				}
				else
				{
					if (OVRInput.GetControllerPositionTracked(this.m_controller) && OVRInput.GetActiveController() == OVRInput.Controller.Hands)
					{
						this._transform.localPosition = OVRInput.GetLocalControllerPosition(m_controller);
						this._transform.localRotation = OVRInput.GetLocalControllerRotation(m_controller);
					}
					else if (this.noTrackingIdlePosition != null)
					{
						this._transform.position = Vector3.Lerp(this._transform.position, this.noTrackingIdlePosition.position, 0.2f);
						this._transform.rotation = Quaternion.Slerp(this._transform.rotation, this.noTrackingIdlePosition.rotation, 0.1f);
					}
				}
			}
			
			
		}

		//change ik offset object orientation based on tracking type
		void SetIKOffsetObject()
		{
			if (this.ikOffsetObject != null)
			{
				if (this._doFingerTracking)
				{
					this.ikOffsetObject.localPosition = this.offsetHandPosition;
					this.ikOffsetObject.localRotation = Quaternion.Euler(this.offsetHandRotation);
				}
				else
				{
					this.ikOffsetObject.localPosition = this.offsetControllerPosition;
					this.ikOffsetObject.localRotation = Quaternion.Euler(this.offsetControllerRotation);
				}
			}
		}


		//update hand animation by controller states
		void UpdateHandAnimationState()
		{
			if (!OVRInput.Get(OVRInput.Touch.One, this.m_controller) && !OVRInput.Get(OVRInput.Touch.Two, this.m_controller) && 
			    !OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, this.m_controller) && OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger, this.m_controller))
			{
				if (this.currentHandState != HandState.Thumbup)
				{
					this.currentHandState = HandState.Thumbup;
					if (this.animatorComp != null)
						this.animatorComp.SetInteger(this.m_controller.ToString(), (int)this.currentHandState);
					
					if (this.targetVRPlayer != null && this.targetVRPlayer.isLocalPlayer)
						this.targetVRPlayer.CmdChangeHandState(this.m_controller == OVRInput.Controller.RTouch ? true : false, this.currentHandState);
				}
			}
			else if (OVRInput.Get(OVRInput.Touch.One, this.m_controller) && !OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, this.m_controller) ||
			         OVRInput.Get(OVRInput.Touch.Two, this.m_controller) && !OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, this.m_controller)||
			         OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, this.m_controller) && !OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, this.m_controller))
			{
				if (this.currentHandState != HandState.Pointing)
				{
					this.currentHandState = HandState.Pointing;
					if (this.animatorComp != null)
						this.animatorComp.SetInteger(this.m_controller.ToString(), (int)this.currentHandState);
					
					if (this.targetVRPlayer != null && this.targetVRPlayer.isLocalPlayer)
						this.targetVRPlayer.CmdChangeHandState(this.m_controller == OVRInput.Controller.RTouch ? true : false, this.currentHandState);
				}
			}
			else if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, this.m_controller) > 0.5f
			         || OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, this.m_controller) > 0.5f)
			{
				if (this.currentHandState != HandState.Fist)
				{
					this.currentHandState = HandState.Fist;
					if (this.animatorComp != null)
						this.animatorComp.SetInteger(this.m_controller.ToString(), (int)this.currentHandState);
					
					if (this.targetVRPlayer != null && this.targetVRPlayer.isLocalPlayer)
						this.targetVRPlayer.CmdChangeHandState(this.m_controller == OVRInput.Controller.RTouch ? true : false, this.currentHandState);
				}
			}
			else
			{
				if (this.currentHandState != HandState.Idle)
				{
					this.currentHandState = HandState.Idle;
					if (this.animatorComp != null)
						this.animatorComp.SetInteger(this.m_controller.ToString(), (int)this.currentHandState);
					
					if (this.targetVRPlayer != null && this.targetVRPlayer.isLocalPlayer)
						this.targetVRPlayer.CmdChangeHandState(this.m_controller == OVRInput.Controller.RTouch ? true : false, this.currentHandState);
				}
			}
			
			
		}

		//update hand animation by networking
		public void UpdateHandState(HandState state)
		{
			if (this._doFingerTracking)
			{
				this._doFingerTracking = false;
				this.SetIKOffsetObject();
			}
			
			if (this.currentHandState != state)
			{
				this.currentHandState = state;
				if (this.animatorComp != null)
					this.animatorComp.SetInteger(this.m_controller.ToString(), (int)this.currentHandState);
			}
		}

		#region Fingertracking

		void UpdateFingerValuesLocal()
		{
			foreach (Finger f in this.fingers)
				f.CalculateAngleValue();

			if (this._lastFingerValueUpdateTime < Time.time)
			{
				for (int i = 0; i < this.fingers.Length; i++)
				{
					this.targetVRPlayer.CmdUpdateFingerDataOnServer(this.m_controller == OVRInput.Controller.RTouch ? true : false, i, this.fingers[i].bone0.localRotation, this.fingers[i].GetAngleValue());
				}
				this._lastFingerValueUpdateTime = Time.time + this.fingerTrackingSyncInterval;
			}
		}

		public void UpdateFingerValuesNetworking(int index, Quaternion rotation, float value)
		{
			if (!this._doFingerTracking)
			{
				this._doFingerTracking = true;
				this.SetIKOffsetObject();
			}

			if (index >= 0 && index <= this.fingers.Length)
			{
				this.fingers[index].SetData(rotation, value, this.m_controller);
			}
		}


		#endregion

		#region grab
		//grab objects
		public void Grab()
		{
			//get closest object
			if (!this._isGrapDown)
			{
				if (this._lastClosestObj != this.closestObj)
					this._lastClosestObj = this.closestObj;

				if (this.objectsHoveringOver.Count > 0)
				{
					float minDistance = float.MaxValue;
					float distance;

					for (int i = this.objectsHoveringOver.Count - 1; i >= 0; i--)
					{
						if (this.objectsHoveringOver[i].Equals(null) || !this.objectsHoveringOver[i].GameObject().activeInHierarchy)
						{
							this.objectsHoveringOver.RemoveAt(i);
						}
						else if (this.objectsHoveringOver[i].GameObject() != null && this.objectsHoveringOver[i].CanDrag())
						{
							distance = Vector3.Distance(this._transform.position, this.objectsHoveringOver[i].GameObject().transform.position);
							if (distance < minDistance)
							{
								minDistance = distance;
								this.closestObj = this.objectsHoveringOver[i];
							}
						}
					}
				}
				else
				{
					this.closestObj = null;
				}
				//hover effects
				if (this.doClosestHoverObjectActions)
				{
					if (this._lastClosestObj != null && this._lastClosestObj != this.closestObj)
						this._lastClosestObj.OnEndClosestObject();
					if (this.closestObj != null && this.closestObj != this._lastClosestObj)
						this.closestObj.OnStartClosestObject();
				}
			}



			if (this.GrabStrength() > 0.6f)
			{
				//start Grab
				if (this._isGrapDown == false)
				{
					this._isGrapDown = true;

					this.interactingObj = this.closestObj;
					if (this.interactingObj != null)
					{
						//end interaction by old object
						if (this.interactingObj.IsInteracting() && this.interactingObj.AttachedController() != null)
							this.interactingObj.AttachedController().EndInteraction(this.interactingObj);
						this.StartInteraction(this.interactingObj);
					}
				}

				//during Grab
				else if (this._isGrapDown)
				{
					if (this.interactingObj != null)
						this.Drag(interactingObj);
				} 
				
			}
			//end Grab
			else if (this.GrabStrength() <= 0.6f)
			{
				if (this._isGrapDown)
					this._isGrapDown = false;

				if (this.interactingObj != null && this.interactingObj.AttachedController() != null)
				{
					this.interactingObj.AttachedController().EndInteraction(this.interactingObj);
					this.closestObj = null;
					this.interactingObj = null;
				}
			}
		}

		//use this to get the strength of the grab and trigger button strength of the controller and Index pinch strength with handtracking
		public float GrabStrength()
		{
			if (this._doFingerTracking && this._ovrHand != null)
			{
				return this._ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
			}
			else
			{
				float gripValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, this.m_controller);
				float indexValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, this.m_controller);
				
				return gripValue > indexValue ? gripValue : indexValue;
			}
		}

		public void ChangeGrabTarget(IGrabableObject currentTarget, IGrabableObject newTarget)
		{
			if (this.interactingObj == currentTarget)
			{
				this.interactingObj = newTarget;
				this.StartInteraction(newTarget);
			}
		}

		//force end grab if object get destroyed
		public void ForceEndGrab()
		{
			if (this.interactingObj != null && this.interactingObj.AttachedController() != null)
			{
				this.interactingObj.AttachedController().EndInteraction(this.interactingObj);
			}
			this.closestObj = null;
			this.interactingObj = null;
			
			objectsHoveringOver = new List<IGrabableObject>();
		}
		
		public void OnTriggerEnter(Collider other)
		{
			IGrabableObject collidedObj = other.gameObject.GetComponent(typeof(IGrabableObject)) as IGrabableObject;
			if (collidedObj != null)
			{
				this.AddToHoverList(collidedObj);
			}
		}
		
		public void OnTriggerExit(Collider other)
		{
			IGrabableObject collidedObj = other.gameObject.GetComponent(typeof(IGrabableObject)) as IGrabableObject;
			
			if (collidedObj != null)
			{
				this.RemoveFromHoverList(collidedObj);
			}
		}
		
		public void AddToHoverList(IGrabableObject target)
		{
			if (target != null && !this.objectsHoveringOver.Contains(target) && target.CanDrag())
				this.objectsHoveringOver.Add(target);
		}
		
		public void RemoveFromHoverList(IGrabableObject target)
		{
			if (target != null && this.objectsHoveringOver.Contains(target))
			{
				this.objectsHoveringOver.Remove(target);
			}
		}
		
		
		public virtual void Drag(IGrabableObject target)
		{
			if (!target.CanDrag())
			{
				this.ForceEndGrab();
				return;
			}
			if (target.AttachedController() && target.IsInteracting() && target.GetRigidbody())
			{
				//Debug.Log("Drag " + target.GameObject().name);
				this.posDelta = target.AttachedController().transform.position - this.interactionPoint.position;
				target.GetRigidbody().velocity = this.posDelta * target.VelocitiyFactor() * Time.fixedDeltaTime;
				
				this.rotationDelta = target.AttachedController().transform.rotation * Quaternion.Inverse(this.interactionPoint.rotation);
				this.rotationDelta.ToAngleAxis(out this.angel, out this.axis);
				if (this.angel > 180)
					this.angel -= 360;
				target.GetRigidbody().angularVelocity = (Time.fixedDeltaTime * this.angel * this.axis) * target.RotationFactor();
			}
		}
		
		public void StartInteraction(IGrabableObject target)
		{
			if (!target.CanDrag())
				return;
			if (this.interactionPoint == null)
			{
				this.interactionPoint = new GameObject().transform;
				this.interactionPoint.name = this.gameObject.name + "InteractionPoint";
			}

			//Hover end effects
			if (this.doClosestHoverObjectActions)
				target.OnEndClosestObject();

			target.SetAttachedController(this);
			this.interactionPoint.position = this._transform.position;
			this.interactionPoint.rotation = this._transform.rotation;
			this.interactionPoint.SetParent(target.GameObject().transform, true);
			target.StartGrabing();
			target.SetIsInteracting(true);
			this.PlayGrabAudio();
		}
		
		public void EndInteraction(IGrabableObject target)
		{
			if (target != null && this == target.AttachedController())
			{
				this.interactingObj = null;
				if (target.GameObject() != null)
				{
					target.EndGrabing();
					target.SetAttachedController(null);
					target.SetIsInteracting(false);
				}
				if (this.interactionPoint != null)
					this.interactionPoint.parent = null;
			}
			
		}
		
		void PlayGrabAudio()
		{

		}
		
		#endregion
		
		public void SetVribration(float frequency, float amplitude, float duration)
		{
			OVRInput.SetControllerVibration(frequency, amplitude, this.m_controller);
			
			this.StopCoroutine(this.VribrationStop(0f));
			this.StartCoroutine(this.VribrationStop(duration));
		}
		
		IEnumerator VribrationStop(float duration)
		{
			yield return new WaitForSeconds(duration);
			OVRInput.SetControllerVibration(0f, 0f, this.m_controller);
		}
	}
	
	public enum HandState {Idle = 0, Pointing = 1, Fist = 2, Thumbup = 3 }
}
