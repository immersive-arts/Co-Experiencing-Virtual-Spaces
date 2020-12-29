//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IAS.CoLocationMUVR
{
	//events invoked by the interaction ray
	public class InteractiveEvent : MonoBehaviour, IInteractable
	{
		public UnityEvent interactEvents;
		public UnityEvent onHoverEnterEvents;
		public UnityEvent onHoverExitEvents;

		void Awake()
		{
			
		}
		
		public void Interact()
		{
			this.interactEvents.Invoke();
		}
		
		
		public void OnHoverEnter()
		{
			this.onHoverEnterEvents.Invoke();
		}
		
		public void OnHoverStay()
		{
			
		}
		
		public void OnHoverExit()
		{
			this.onHoverExitEvents.Invoke();
		}
		
		public void OnMouseDown()
		{
			this.Interact();
		}
	}
}
