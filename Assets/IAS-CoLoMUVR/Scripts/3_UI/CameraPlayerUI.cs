//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

namespace IAS.CoLocationMUVR
{
	public class CameraPlayerUI : MonoBehaviour
	{
		public static CameraPlayerUI Instance;

		public GameObject cameraUI;
		
		public Camera targetCamera;
		private RenderTexture _backgroundRenderTexture;
		public ARCameraBackground arCameraBackground;
		
		
		public Camera target3DCamera;
		private RenderTexture _3dRenderTexture;
		
		public RawImage uiBackgroundRawImage;
		public RawImage ui3dRawImage;

		public Text showCalibrationCourser;
		private bool _showCalibrationCourser = true;
		public Text showEnvironment;
		private bool _showEnvironment = false;

		private bool _wasReactivateTextVisible = false;

		public LayerMask justCalibrationCourserVisual;
		public LayerMask justEnvironmentVisual;
		public LayerMask allVisual;


		public void ToggleCameraUI(bool b)
		{
			if (this.cameraUI != null)
				this.cameraUI.SetActive(b);
		}

		//show how to reactivate ui text -> just first time
		public void ShowReactivatUiText(GameObject target)
		{
			if (!this._wasReactivateTextVisible)
			{
				target.SetActive(true);
				this.StartCoroutine(WaitTime(target));
			}
		}

		IEnumerator WaitTime(GameObject target)
		{
			yield return new WaitForSeconds(4f);
			
			target.SetActive(false);
			this._wasReactivateTextVisible = true;
		}

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			this._backgroundRenderTexture = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 0, RenderTextureFormat.RGB111110Float);
			this._3dRenderTexture = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 1, RenderTextureFormat.RGB111110Float);
			
			if (this.target3DCamera != null)
			{
				this.target3DCamera.targetTexture = this._3dRenderTexture;
			}
			if (this.ui3dRawImage != null)
				this.ui3dRawImage.texture = this._3dRenderTexture;
			
			if (this.uiBackgroundRawImage != null)
				this.uiBackgroundRawImage.texture = this._backgroundRenderTexture;

			this.UpdateEnvironmentVisualText();
			this.UpdateCalibrationCourserText();
			this.UpdateCameraVisualLayers();
		}
		
		private void LateUpdate()
		{
			//if split screen on, blit camera image to render texture
			if (this.uiBackgroundRawImage.gameObject.activeInHierarchy && this._backgroundRenderTexture != null && arCameraBackground != null && this.arCameraBackground.material != null)
				BlitToRenderTexture(_backgroundRenderTexture, arCameraBackground);
		}
		
		void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
		{
			//if split screen on, blit camera image to render texture
			if (_backgroundRenderTexture != null && arCameraBackground != null)
				BlitToRenderTexture(_backgroundRenderTexture, arCameraBackground);
		}
		
		public static void BlitToRenderTexture(RenderTexture renderTexture, ARCameraBackground cameraBackground)
		{
			//copy the camera background to a RenderTexture
			Graphics.Blit(null, renderTexture, cameraBackground.material);
		}

		//camera image + virtual image spitscreen
		public void ToggleSplitScreenMode()
		{
			if (this.uiBackgroundRawImage != null && this.targetCamera != null)
			{
				//on
				if (!this.uiBackgroundRawImage.gameObject.activeInHierarchy)
				{
					this.uiBackgroundRawImage.gameObject.SetActive(true);
					this.ui3dRawImage.gameObject.SetActive(true);
					this.target3DCamera.gameObject.SetActive(true);
					this.target3DCamera.projectionMatrix = this.targetCamera.projectionMatrix;
					this.targetCamera.enabled = false;
				}
				//off
				else
				{
					this.uiBackgroundRawImage.gameObject.SetActive(false);
					this.ui3dRawImage.gameObject.SetActive(false);
					this.target3DCamera.gameObject.SetActive(false);
					this.targetCamera.enabled = true;
				}
			}
		}

		//show calibration UI culling mask layer
		public void ToggleCalibrationCurserVisual(bool b)
		{
			this._showCalibrationCourser = b;
			this.UpdateCalibrationCourserText();
			this.UpdateCameraVisualLayers();
		}

		public void ToggleCalibrationCurserVisual()
		{
			this._showCalibrationCourser = !this._showCalibrationCourser;
			this.UpdateCalibrationCourserText();
			this.UpdateCameraVisualLayers();
		}
		private void UpdateCalibrationCourserText()
		{
			if (this.showCalibrationCourser != null)
				this.showCalibrationCourser.text = this._showCalibrationCourser ? "X" : "";
		}


		//show environment culling mask layer
		public void ToggleEnvironmnetVisual(bool b)
		{
			this._showEnvironment = b;
			this.UpdateEnvironmentVisualText();
			this.UpdateCameraVisualLayers();
		}
		public void ToggleEnvironmnetVisual()
		{
			this._showEnvironment = !this._showEnvironment;
			this.UpdateEnvironmentVisualText();
			this.UpdateCameraVisualLayers();
		}
		private void UpdateEnvironmentVisualText()
		{
			if (this.showEnvironment != null)
				this.showEnvironment.text = this._showEnvironment ? "X" : "";
		}

		//update culling mask
		void UpdateCameraVisualLayers()
		{
			if (this.targetCamera != null)
			{
				if (this._showEnvironment && this._showCalibrationCourser)
					this.targetCamera.cullingMask = this.allVisual;
				else if (this._showEnvironment)
					this.targetCamera.cullingMask = this.justEnvironmentVisual;
				else 
					this.targetCamera.cullingMask = this.justCalibrationCourserVisual;

			}
		}
	}
}
