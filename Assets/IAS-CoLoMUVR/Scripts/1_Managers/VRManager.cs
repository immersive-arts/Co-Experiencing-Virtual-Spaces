//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRManager : MonoBehaviour
{
    public OVRManager.FixedFoveatedRenderingLevel tiledMultiresLevel;

    public static VRManager Instance;

    private void Start()
    {
        OVRManager.fixedFoveatedRenderingLevel = this.tiledMultiresLevel;

        Instance = this;
    }

	//set the FixedFoveatedRenderingLevel of the Oculus Quest headset
	public void SetFoveatedRenderingLevel(OVRManager.FixedFoveatedRenderingLevel renderingLevel)
    {
        OVRManager.fixedFoveatedRenderingLevel = renderingLevel;
    }
}
