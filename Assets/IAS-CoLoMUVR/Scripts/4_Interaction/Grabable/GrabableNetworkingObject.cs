//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Mirror;

namespace IAS.CoLocationMUVR
{
	public class GrabableNetworkingObject : NetworkBehaviour, IGrabableObject
	{
		[Header("Grab")]
		public bool canDrag = true;
		
		private Rigidbody _rigidbody;
		
		public bool isInteracting = false;
		private VrController _attachedController;
		public float velocityFactor = 2000f;
		public float rotationFactor = 60f;
		
		private bool _startetInteracting = false;
		public bool canGrabifPlayerIsDead = true;

		public float hoverOutlineWidth = 0.2f;

		protected Material _mat;
		
		public virtual void Awake()
		{
			this._rigidbody = this.gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
			
			if (this._rigidbody == null)
				print("no rigidbody");
			
			else
			{
				this._rigidbody.useGravity = true;
			}

			this._mat = this.gameObject.GetComponent<MeshRenderer>().material;
		}
		
		
		void Update()
		{
			if (isServer)
			{
				if (this.connectionToClient != null)
				{
					//if authority has changed while object is grabed on the server
					if (this.isInteracting)
						this.AttachedController().ForceEndGrab();
					
					//if object is grabed by a other client and gravity is on -> deactivate gravity
					if (this._rigidbody.useGravity)
						this._rigidbody.useGravity = false;
					
				}
				//have authority back but gravity still tunred off
				else if (!this.isInteracting && this.connectionToClient == null && !this._rigidbody.useGravity)
				{
					this._rigidbody.useGravity = true;
				}
			}
			else
			{	
				//Has authority on client and is grabing but gravity is on
				if (this.hasAuthority && this.isInteracting && this._rigidbody.useGravity)
				{
					this._rigidbody.useGravity = false;
				}
				//Has authority bug grabing is over -> check velocity to end authority
				else if(this.hasAuthority && !this.isInteracting && this._rigidbody.velocity.sqrMagnitude < 0.01f)
				{
					if (GameManager.Instance != null && GameManager.Instance.localVRPlayer != null)
						GameManager.Instance.localVRPlayer.CmdRemoveObjectAuthority(netIdentity);
				}
			}
		}
		
		public override void OnStopAuthority()
		{
			base.OnStopAuthority();
			//if authority has changed while interacting, an other player started grabing this object
			if (this.isInteracting)
			{
				this.AttachedController().ForceEndGrab();
				
				this._rigidbody.useGravity = false;
			}
		}

		
		#region IGrabable
		public bool CanDrag()
		{
			if (!this.canDrag)
				return this.canDrag;
			else
			{
				//retunr fals if player is dead and canGrabifPlayerIsDead is false
				if (!this.canGrabifPlayerIsDead && GameManager.Instance != null && GameManager.Instance.localVRPlayer != null && GameManager.Instance.localVRPlayer.currentHealth <= 0)
					return false;
				else
					return this.canDrag;
			}
		}
		
		public bool StartetInteracting()
		{
			return this._startetInteracting;
		}
		
		public bool IsInteracting()
		{
			return this.isInteracting;
		}
		public void SetIsInteracting(bool b)
		{
			this.isInteracting = b;
		}
		
		public VrController AttachedController()
		{
			return this._attachedController;
		}
		
		public void SetAttachedController(VrController controller)
		{
			this._attachedController = controller;
		}
		
		public GameObject GameObject()
		{
			return this.gameObject;
		}
		
		public float VelocitiyFactor()
		{
			return this.velocityFactor;
		}
		public float RotationFactor()
		{
			return this.rotationFactor;
		}
		public void OnStartClosestObject()
		{
			if (this._mat != null && this._mat.HasProperty("_OutlineWidth"))
				this._mat.SetFloat("_OutlineWidth", this.hoverOutlineWidth);
		}
		public void OnEndClosestObject()
		{
			if (this._mat != null && this._mat.HasProperty("_OutlineWidth"))
				this._mat.SetFloat("_OutlineWidth", 0);
		}

		//invoked by vrcontroller
		public virtual void StartGrabing()
		{
			this._startetInteracting = true;

			//change network authority
			if (!this.isServer && GameManager.Instance != null && GameManager.Instance.localVRPlayer != null)
				GameManager.Instance.localVRPlayer.CmdGetObjectAuthority(netIdentity);
			else if (this.isServer)
				this.netIdentity.RemoveClientAuthority();
			this.StartCoroutine(StarGrabWaitingTime());
		}
		
		public IEnumerator StarGrabWaitingTime()
		{
			yield return new WaitForEndOfFrame();
			if (this._rigidbody != null)
				this._rigidbody.useGravity = false;
		}

		//invoked by vrcontroller
		public void EndGrabing()
		{
			if (this._rigidbody != null)
				this._rigidbody.useGravity = true;
			
		}
		
		
		public void ForceEndDrag()
		{
			if (this.AttachedController() != null)
			{
				this.AttachedController().ForceEndGrab();
				
			}
		}
		
		public Rigidbody GetRigidbody()
		{
			return this._rigidbody;
		}
		#endregion
	}
}
