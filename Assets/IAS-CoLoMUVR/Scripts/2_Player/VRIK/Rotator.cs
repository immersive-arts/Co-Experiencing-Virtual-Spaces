//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.VRIK.Examples
{
	public class Rotator : MonoBehaviour
	{
		public Vector3 rotationVector = Vector3.up;
		public float rotationSpeed = 3f;
		
		// Update is called once per frame
		void Update()
		{
			transform.Rotate(this.rotationVector * Time.deltaTime * this.rotationSpeed);
		}
	}
}
