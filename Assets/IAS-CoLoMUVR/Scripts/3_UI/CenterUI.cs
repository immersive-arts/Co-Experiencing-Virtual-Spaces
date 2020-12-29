//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IAS.CoLocationMUVR
{
	//interaction courser
	public class CenterUI : MonoBehaviour
	{
		public static CenterUI Instance;
		
		public Image waitingCourser;
		
		private void Awake()
		{
			Instance = this;
			
			this.gameObject.SetActive(false);
		}
		
		//udpate timer visual
		public void UpdateWaitingCourser(float t)
		{
			if (this.waitingCourser != null)
			{
				if (t > 0)
				{
					if (!this.waitingCourser.gameObject.activeInHierarchy)
						this.waitingCourser.gameObject.SetActive(true);
					this.waitingCourser.fillAmount = t;
				}
				else
				{
					if (this.waitingCourser.gameObject.activeInHierarchy)
						this.waitingCourser.gameObject.SetActive(false);
					this.waitingCourser.fillAmount = 0;
				}
			}
		}
	}
}
