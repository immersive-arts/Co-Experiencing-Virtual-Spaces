//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.XR;


namespace IAS.CoLocationMUVR
{
	[System.Serializable]
	public struct FingerData
	{
		public float bendValue;
		public Quaternion baseRotation;
	}
	
	public class VRPlayerManager : NetworkBehaviour
	{
		private Transform _transform;
		
		[SyncVar(hook = nameof(ChangeHealthOnClient))]
		public float currentHealth;
		public float maxHealth = 3f;
		
		public int trackingIndex;
		public int teamIndex;
		
		public GameObject center;
		public GameObject head;
		public VrController leftHand;
		public VrController rightHand;
		[SyncVar(hook = nameof(OnUpdateLeftHandState))]
		private HandState _leftHandState = HandState.Idle;
		
		[SyncVar(hook = nameof(OnUpdateRightHandState))]
		private HandState _rightHandState = HandState.Idle;

		//syncList for networking fingertracking
		public class FingerDatas : SyncList<FingerData> {}
		public FingerDatas fingerDatas = new FingerDatas();
		private bool _updateFingerDatas = false;
		
		public bool trackHands = true;
		
		public BodyScaleManager targetBodyScaleManager;
		
		public Transform calculationTarget;
		
		public VRPlayerHealthUI targetHealthUI;
		public Vector3 localHealthBarPositionOffset;
		
		[SyncVar(hook = nameof(UpdateBodyScale))]
		public float bodyScale = 1f;
		
		[SyncVar(hook = nameof(UpdateArmScale))]
		public float armScale = 1f;
		
		[SyncVar(hook = nameof(UpdateHandScale))]
		private float _handScale = 1f;
		
		public override void OnStartLocalPlayer()
		{
			base.OnStartLocalPlayer();
			if (GameManager.Instance != null)
			{
				GameManager.Instance.localVRPlayer = this;
			}
			
			this.bodyScale = GlobalVariables.bodyScale;
			this.armScale = GlobalVariables.armScale;
			
			this.UpdateBodySize(this.bodyScale, this.armScale);
			this.CmdUpdateBodySizeOnServer(this.bodyScale, this.armScale);
			
			if (PauseUI.Instance != null)
			{
				PauseUI.Instance.SetTargetPlayer(this);
				PauseUI.Instance.ToggleCalibrationUI(true);
			}
			
			this.currentHealth = this.maxHealth;
			this.CmdChangeHealthOnServer(this.currentHealth);
			this.InitializeUIHealth();
			
			
			this.CmdAddFingerDatasToList(this.leftHand.fingers.Length);
			this.CmdAddFingerDatasToList(this.rightHand.fingers.Length);
			
		}
		
		public virtual void Start()
		{
			this._transform = this.transform;
			this.fingerDatas.Callback += ChangeFingerData;
			
			//remove all VRComponents if not local player
			if (!isLocalPlayer)
			{
				Destroy(this.gameObject.GetComponent<OVRCameraRig>());
				Destroy(this.gameObject.GetComponent<OVRHeadsetEmulator>());
				
				if (this.head != null)
				{
					Destroy(this.head.GetComponent<Camera>());
					Destroy(this.head.GetComponent<AudioListener>());
				}
				if (this.leftHand != null)
				{
					Destroy(this.leftHand.GetComponent<OVRHand>());
					Destroy(this.leftHand.GetComponent<OVRCustomSkeleton>());
					Destroy(this.leftHand.GetComponent<SphereCollider>());
					Destroy(this.leftHand.GetComponent<Rigidbody>());
				}
				if (this.rightHand != null)
				{
					Destroy(this.rightHand.GetComponent<OVRHand>());
					Destroy(this.rightHand.GetComponent<OVRCustomSkeleton>());
					Destroy(this.rightHand.GetComponent<SphereCollider>());
					Destroy(this.rightHand.GetComponent<Rigidbody>());
				}
			}
		}
		
		public virtual void Update()
		{
			if (isLocalPlayer)
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
				
			}
		}

		//update local finger bones in lateUpdate
		void LateUpdate()
		{
			if (this._updateFingerDatas && !this.isLocalPlayer)
			{
				for (int i = 0; i < this.fingerDatas.Count; i++)
				{
					if (i < 5)
						this.leftHand.UpdateFingerValuesNetworking(i, this.fingerDatas[i].baseRotation, this.fingerDatas[i].bendValue);
					else
						this.rightHand.UpdateFingerValuesNetworking(i - 5, this.fingerDatas[i].baseRotation, this.fingerDatas[i].bendValue);
				}
				
				this._updateFingerDatas = false;
			}
		}
		
		#region Hand and FingerTracking Networking
		[Command]
		public void CmdChangeHandState(bool isRightHand, HandState newHandState)
		{
			if (isRightHand)
				this._rightHandState = newHandState;
			else
				this._leftHandState = newHandState;
		}
		
		
		void OnUpdateLeftHandState(HandState _, HandState newValue)
		{
			if (this.isLocalPlayer)
				return;
			this._leftHandState = newValue;
			this.leftHand.UpdateHandState(this._leftHandState);
		}
		
		void OnUpdateRightHandState(HandState _, HandState newValue)
		{
			if (this.isLocalPlayer)
				return;
			this._rightHandState = newValue;
			this.rightHand.UpdateHandState(this._rightHandState);
		}
		
		
		[Command]
		//initialize sync list on start
		void CmdAddFingerDatasToList(int fingers)
		{
			for (int i = 0; i < fingers; i++)
			{
				FingerData finger = new FingerData
				{
					bendValue = 0f,
					baseRotation = Quaternion.identity
				};
				
				this.fingerDatas.Add(finger);
			}
		}
		
		[Command]
		public void CmdUpdateFingerDataOnServer(bool isRightHand, int fingerIndex, Quaternion baseRotation, float bendValue)
		{
			int index = isRightHand ? fingerIndex + 5 : fingerIndex;
			
			FingerData finger = new FingerData
			{
				bendValue = bendValue,
				baseRotation = baseRotation
			};
			
			this.fingerDatas[index] = finger;
		}
		
		void ChangeFingerData(SyncList<FingerData>.Operation op, int itemIndex, FingerData oldData, FingerData newData)
		{
			if (!this.isLocalPlayer)
			{
				this.fingerDatas[itemIndex] = newData;
				
				this._updateFingerDatas = true;
			}
		}
		
		[Command]
		public void CmdChangeHandScaleOnServer(float value)
		{
			this._handScale = value;
		}

		//invoke if _handScale get update from server
		void UpdateHandScale(float _, float newValue)
		{
			this._handScale = newValue;
			if (this.targetBodyScaleManager != null)
				this.targetBodyScaleManager.UpdateTrackedHandRootScale(newValue);
		}
		
		#endregion
		
		
		public void InitializeUIHealth()
		{
			if (this.targetHealthUI != null)
			{
				this.targetHealthUI.Initialize(this.maxHealth);
				this.targetHealthUI.UpdateHealthBar(this.currentHealth);
				if (this.isLocalPlayer)
					this.targetHealthUI.transform.localPosition = this.targetHealthUI.transform.localPosition + this.localHealthBarPositionOffset;
			}
		}
		
		
		public void GetDamage(float hitPoints, GameObject other)
		{
			if (!this.isLocalPlayer)
				return;
			this.currentHealth = Mathf.Clamp(this.currentHealth - hitPoints, 0f, this.maxHealth);
			
			if (this.targetHealthUI != null)
			{
				if (!this.targetHealthUI.IsInitialized())
					this.InitializeUIHealth();

				this.targetHealthUI.UpdateHealthBar(this.currentHealth);
				if (this.currentHealth <= 0)
					this.targetHealthUI.SetDamageVignietteAlpha(0.8f);
				else
					this.targetHealthUI.PlayDamageVigniette();
				
			}
			this.CmdChangeHealthOnServer(this.currentHealth);
			
			this.CameraShake(0.1f, 0.02f);
		}
		
		public void Heal(float healPoints)
		{
			if (!this.isLocalPlayer)
				return;
			float lastHealth = this.currentHealth;
			
			this.currentHealth = Mathf.Clamp(this.currentHealth + healPoints, 0f, this.maxHealth);
			Debug.Log("heal");
			
			if (this.targetHealthUI != null)
			{
				if (!this.targetHealthUI.IsInitialized())
					this.InitializeUIHealth();
				this.targetHealthUI.UpdateHealthBar(this.currentHealth);
				if (lastHealth <= 0)
					this.targetHealthUI.SetDamageVignietteAlpha(0f);
				
			}
			
			this.CmdChangeHealthOnServer(this.currentHealth);
		}
		
		[Command]
		void CmdChangeHealthOnServer(float value)
		{
			if (!this.isLocalPlayer)
			{
				this.currentHealth = value;
				if (this.targetHealthUI != null)
				{
					if (!this.targetHealthUI.IsInitialized())
						this.InitializeUIHealth();
					this.targetHealthUI.UpdateHealthBar(this.currentHealth);
				}
			}
		}

		//invoke if currentHealth get update from server
		void ChangeHealthOnClient(float _, float newValue)
		{
			if (newValue != this.currentHealth)
			{
				this.currentHealth = newValue;
				if (this.targetHealthUI != null)
				{
					if (!this.targetHealthUI.IsInitialized())
						this.InitializeUIHealth();
					this.targetHealthUI.UpdateHealthBar(this.currentHealth);
				}
			}
		}

		//invoke if bodyScale get update from server
		void UpdateBodyScale(float _, float newValue)
		{
			this.bodyScale = newValue;
			this.UpdateBodySize(this.bodyScale, this.armScale);
		}

		//invoke if armScale get update from server
		void UpdateArmScale(float _, float newValue)
		{
			this.armScale = newValue;
			this.UpdateBodySize(this.bodyScale, this.armScale);
		}

		//update BodyScaleManager values and visual
		public void UpdateBodySize(float size, float armSize)
		{
			if (this.targetBodyScaleManager != null)
				this.targetBodyScaleManager.SetValues(size, armSize);
		}
		
		[Command]
		public void CmdUpdateBodySizeOnServer(float size, float armSize)
		{
			this.bodyScale = size;
			this.armScale = armSize;
			
			this.UpdateBodySize(this.bodyScale, this.armScale);
		}

		//invoke to calibrate the center of the space
		public void CalculateCenterOffset(Vector3 offset)
		{
			this.center.transform.localPosition = Vector3.zero;
			this.center.transform.localRotation = Quaternion.identity;
			
			if (this.calculationTarget == null)
			{
				this.calculationTarget = new GameObject().transform;
				this.calculationTarget.parent = this.gameObject.transform;
			}
			
			//set rotation
			this.calculationTarget.position = this.rightHand.transform.position;
			Vector3 yNeutralPos = this.leftHand.transform.position;
			yNeutralPos.y = this.rightHand.transform.position.y;
			this.calculationTarget.transform.LookAt(yNeutralPos, center.transform.up);
			
			//after calculate rotation set to right hand position
			this.calculationTarget.position = this.rightHand.transform.position;
			
			//set offset
			this.center.transform.localPosition = this.calculationTarget.InverseTransformPoint(Vector3.zero) + GlobalVariables.centerPositionControllerOffset + offset;
			this.center.transform.localRotation = Quaternion.Inverse(this.calculationTarget.rotation);
		}
		
		public void CameraShake(float duration, float magnitude)
		{
			StartCoroutine(this.Shake(duration, magnitude));
		}
		
		IEnumerator Shake(float duration, float magnitude)
		{
			Vector3 originalPosition = this._transform.localPosition;
			
			float elapsedTime = 0f;
			
			while (elapsedTime < duration)
			{
				float x = Random.Range(1f, -1f) * magnitude;
				float y = Random.Range(1f, -1f) * magnitude;
				
				this._transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
				
				elapsedTime += Time.deltaTime;
				yield return null;
			}
			this._transform.localPosition = originalPosition;
		}


		//general networking actions
		[Command]
		public void CmdGetObjectAuthority(NetworkIdentity item)
		{
			if (item.connectionToClient != null)
				item.RemoveClientAuthority();
			item.AssignClientAuthority(connectionToClient);
		}
		
		[Command]
		public void CmdRemoveObjectAuthority(NetworkIdentity item)
		{
			item.RemoveClientAuthority();
		}

		[Command]
		public void CmdDestroyObjectOnServer(GameObject target)
		{
			NetworkServer.Destroy(target);
		}
	}
}
