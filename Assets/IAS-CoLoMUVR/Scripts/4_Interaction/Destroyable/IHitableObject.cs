//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.CoLocationMUVR
{
	public interface IHitableObject
	{
		Rigidbody GetRigidbody();
		void IsHited(float hitPoints, GameObject other);
		void BeforHit();
		
		void FistHit(GameObject handObject);
		
		void HealHit(float healPoints);
	}
}
