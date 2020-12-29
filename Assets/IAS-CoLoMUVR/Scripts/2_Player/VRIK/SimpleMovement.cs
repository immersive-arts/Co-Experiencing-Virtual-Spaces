//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.VRIK.Examples
{
	public class SimpleMovement : MonoBehaviour
	{
		public float speed = 3f;
		
		// Update is called once per frame
		void Update()
		{
			if (Input.GetAxis("Vertical") != 0)
				transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * Time.deltaTime * this.speed);
			if (Input.GetAxis("Horizontal") != 0)
				transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * this.speed);
		}
	}
}
