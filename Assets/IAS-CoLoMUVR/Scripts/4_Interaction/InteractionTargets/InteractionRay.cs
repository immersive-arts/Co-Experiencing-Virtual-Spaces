//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.CoLocationMUVR
{
	public class InteractionRay : MonoBehaviour
	{
		public VrController targetController;
		
		public float rayLength;
		public LayerMask hitLayer;
		
		public bool interactPerTime = false;
		public float interactionWaitingTime = 2f;
		
		private IInteractable _targetInteractable;
		private bool isInteracting = false;
		
		public GameObject lineRenderObject;
		
		public GameObject hitEndPointObject;
		
		private void Start()
		{
			if (this.targetController == null)
				this.interactPerTime = true;
		}
		
		void Update()
		{
			if (this.lineRenderObject != null && !this.lineRenderObject.activeInHierarchy)
				this.lineRenderObject.SetActive(true);
			
			RaycastHit hit;
			if (Physics.Raycast(this.gameObject.transform.position, this.gameObject.transform.forward, out hit, this.rayLength, this.hitLayer))
			{
				IInteractable newTarget = hit.collider.GetComponent(typeof(IInteractable)) as IInteractable;
				
				if (this.hitEndPointObject != null)
				{
					this.hitEndPointObject.SetActive(true);
					this.hitEndPointObject.transform.position = hit.point;
					this.hitEndPointObject.transform.rotation = Quaternion.LookRotation(hit.normal);
				}
				//target interactable object
				if (newTarget != null)
				{
					//is new interactable
					if (newTarget != this._targetInteractable)
					{
						if (this._targetInteractable != null)
							this._targetInteractable.OnHoverExit();
						
						this._targetInteractable = newTarget;
						this._targetInteractable.OnHoverEnter();

						//interact per timer
						if (this.interactPerTime)
							StartCoroutine(this.WaitInteractionTime(newTarget));
						
					}
					//if same target
					else if (newTarget == this._targetInteractable)
						this._targetInteractable.OnHoverStay();
				}
				//target is not interactable
				else if (this._targetInteractable != null)
				{
					this._targetInteractable.OnHoverExit();
					this._targetInteractable = null;
				}
				
			}
			//no target
			else
			{
				if (this._targetInteractable != null)
				{
					this._targetInteractable.OnHoverExit();
					this._targetInteractable = null;
				}
				if (this.hitEndPointObject != null)
					this.hitEndPointObject.SetActive(false);
			}
			
			
			if (this.targetController != null)
			{
				//start interact
				if (this.targetController.GrabStrength() > 0.5f && !this.isInteracting)
				{
					this.isInteracting = true;
					if (this._targetInteractable != null)
					{
						this._targetInteractable.Interact();
					}
				}
				//end interact
				else if (this.targetController.GrabStrength() < 0.5f && this.isInteracting)
					this.isInteracting = false;
			}
		}
		
		IEnumerator WaitInteractionTime(IInteractable target)
		{
			float t = 0;
			while (t < this.interactionWaitingTime)
			{
				t += Time.deltaTime;
				if (CenterUI.Instance != null)
					CenterUI.Instance.UpdateWaitingCourser(t / this.interactionWaitingTime);
				yield return new WaitForEndOfFrame();
				if (target != this._targetInteractable)
					break;
			}
			
			if (CenterUI.Instance != null)
				CenterUI.Instance.UpdateWaitingCourser(0);
			if (target == this._targetInteractable)
				target.Interact();
		}
	}
}
