//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.CoLocationMUVR
{
	public interface IGrabableObject
	{
		bool CanDrag();
		
		bool StartetInteracting();
		
		Rigidbody GetRigidbody();
		
		bool IsInteracting();
		void SetIsInteracting(bool b);
		
		VrController AttachedController();
		
		void SetAttachedController(VrController controller);
		
		GameObject GameObject();

		void OnStartClosestObject();
		void OnEndClosestObject();

		float VelocitiyFactor();
		float RotationFactor();
		
		void StartGrabing();
		void EndGrabing();
		
		void ForceEndDrag();
	}
}
