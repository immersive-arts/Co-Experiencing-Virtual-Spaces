//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.CoLocationMUVR
{
	//transfer damage/heal to the VRPlayerManager component
	public class PlayerHitCollider : MonoBehaviour, IHitableObject
	{
		public VRPlayerManager targetPlayer;
		
		public void BeforHit()
		{
			
		}
		
		public void IsHited(float hitPoints, GameObject other)
		{
			this.targetPlayer.GetDamage(hitPoints, null);
		}
		
		public void HealHit(float healPoints)
		{
			this.targetPlayer.Heal(healPoints);
		}
		
		public Rigidbody GetRigidbody()
		{
			return null;
		}
		
		public void FistHit(GameObject handObject)
		{
			
		}
	}
}

