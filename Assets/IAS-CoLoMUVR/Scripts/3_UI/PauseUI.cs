//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

namespace IAS.CoLocationMUVR
{
	public class PauseUI : MonoBehaviour
	{
		public static PauseUI Instance;
		public Transform playerHead;
		public Transform playerLeftHand;
		public Transform playerRightHand;
		private VRPlayerManager _targetPlayer;
		
		public float minAngleOffset = 15f;
		
		private bool _isPauseActive = false;
		
		public GameObject uiParentObject;
		public GameObject uiCourser;
		public GameObject pauseGameObject;
		public GameObject calibrationGameObject;
		
		public GameObject interactionRayObject;
		
		public GameObject calibrateCourser;
		
		private float _startMousDownTime;
		
		public Text showCamerasText;
		
		public Text calibrationIndexText;
		
		public Vector3[] trackingOffsets;
		
		
		
		private void Awake()
		{
			if (Instance != null && Instance != this)
				Destroy(Instance.gameObject);
			
			Instance = this;

			//if has multiple calibration tracking offsets update text
			if (this.calibrationIndexText != null)
			{
				if (this.trackingOffsets != null && this.trackingOffsets.Length > GlobalVariables.trackingIndex && this.trackingOffsets.Length > 1)
					this.calibrationIndexText.text = "Callibration Player " + (GlobalVariables.trackingIndex + 1);
			}
			this.UpdateShowCamerasText();
		}


		public void SetTargetPlayer(VRPlayerManager target)
		{
			this._targetPlayer = target;
			this.playerHead = this._targetPlayer.head.transform;
			this.playerLeftHand = this._targetPlayer.leftHand.transform;
			this.playerRightHand = this._targetPlayer.rightHand.transform;
		}
		
		private void LateUpdate()
		{
			if (!GlobalVariables.joinAsVRPlayer)
				return;
			
			if (this.playerHead != null)
				this.FollowPlayer();

			//toggle on/off UI
			if (Input.GetKeyDown(KeyCode.P) || OVRInput.GetDown(OVRInput.Button.Start))
				this.TogglePauseUi(!this._isPauseActive);

			//if calibration mode is active, update courser oritentation
			if (this.calibrateCourser != null && this.calibrateCourser.activeInHierarchy && this.playerLeftHand != null && this.playerRightHand != null)
				this.SetCalibrationCourser();
		}

		//follow the player head
		void FollowPlayer()
		{
			if (this.transform.parent == null && this.playerHead.parent != null)
				this.transform.parent = this.playerHead.parent;
			
			
			this.transform.position = this.playerHead.position;
			Quaternion newRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(this.playerHead.forward, this.transform.up));
			if (Quaternion.Angle(this.transform.rotation, newRotation) > this.minAngleOffset)
				this.transform.rotation = Quaternion.Slerp(this.transform.rotation, newRotation, 1.5f * Time.deltaTime);
			
			if (this.interactionRayObject != null)
			{
				this.interactionRayObject.transform.position = this.playerHead.position;
				this.interactionRayObject.transform.rotation = this.playerHead.rotation;
			}
		}
		
		public void TogglePauseUi(bool b)
		{
			
			if (this.uiParentObject != null)
				this.uiParentObject.SetActive(b);
			if (this.uiCourser != null)
				this.uiCourser.SetActive(b);
			if (this.pauseGameObject != null)
				this.pauseGameObject.SetActive(b);
			if (this.calibrationGameObject != null)
				this.calibrationGameObject.SetActive(!b);
			if (this.calibrateCourser != null)
				this.calibrateCourser.SetActive(false);
			if (this.interactionRayObject != null)
				this.interactionRayObject.SetActive(b);
			
			this._isPauseActive = b;
		}

		public void TogglePauseUi()
		{
			bool b = !this._isPauseActive;
			if (this.uiParentObject != null)
				this.uiParentObject.SetActive(b);
			if (this.uiCourser != null)
				this.uiCourser.SetActive(b);
			if (this.pauseGameObject != null)
				this.pauseGameObject.SetActive(b);
			if (this.calibrationGameObject != null)
				this.calibrationGameObject.SetActive(!b);
			if (this.calibrateCourser != null)
				this.calibrateCourser.SetActive(false);
			if (this.interactionRayObject != null)
				this.interactionRayObject.SetActive(b);

			this._isPauseActive = b;
		}

		//disconnect from server/stop server and go back to menu
		public void BackToMenu()
		{
			if (NetworkManager.singleton != null)
			{
				if (NetworkManager.singleton.mode == NetworkManagerMode.Host)
					NetworkManager.singleton.StopHost();
				else if (NetworkManager.singleton.mode == NetworkManagerMode.ClientOnly)
					NetworkManager.singleton.StopClient();
			}
			else
				SceneManager.LoadScene(0);
		}

		//activat/deactivate calibration UI and courser
		public void ToggleCalibrationUI(bool b)
		{
			if (!GlobalVariables.joinAsVRPlayer)
				return;
			
			if (this.pauseGameObject != null)
				this.pauseGameObject.SetActive(!b);
			if (this.uiCourser != null)
				this.uiCourser.SetActive(b);
			if (this.uiParentObject != null)
				this.uiParentObject.SetActive(b);
			if (this.calibrationGameObject != null)
				this.calibrationGameObject.SetActive(b);
			if (this.calibrateCourser != null)
				this.calibrateCourser.SetActive(b);
			if (this.interactionRayObject != null)
				this.interactionRayObject.SetActive(b);
			
			this._isPauseActive = b;
		}

		//update courser oroentation to player hands
		void SetCalibrationCourser()
		{
			if (!GlobalVariables.joinAsVRPlayer)
				return;

			this.calibrateCourser.transform.position = this.playerRightHand.position;
			Vector3 yNeutralPos = this.playerLeftHand.position;
			yNeutralPos.y = this.playerRightHand.transform.position.y;
			
			if (this.playerHead.parent != null)
				this.calibrateCourser.transform.transform.LookAt(yNeutralPos, this.playerHead.parent.transform.up);
			
			//after calculate rotation set to right hand position
			this.calibrateCourser.transform.position = this.playerRightHand.position + GlobalVariables.centerPositionControllerOffset;
		}

		//invoke to calibrate the conter of the room on the local player
		public void CalibrateCenter()
		{
			if (!GlobalVariables.joinAsVRPlayer)
				return;
			
			if (GameManager.Instance != null && GameManager.Instance.localVRPlayer != null)
			{
				Vector3 offset = Vector3.zero;
				if (this.trackingOffsets != null && this.trackingOffsets.Length > 0 && GlobalVariables.trackingIndex < this.trackingOffsets.Length)
					offset = this.trackingOffsets[GlobalVariables.trackingIndex];
				GameManager.Instance.localVRPlayer.CalculateCenterOffset(offset);
			}
		}

		
		public void ToggleShowCameras()
		{
			GlobalVariables.showCameras = !GlobalVariables.showCameras;
			this.UpdateShowCamerasText();
		}
		
		public void UpdateShowCamerasText()
		{
			if (this.showCamerasText != null)
			{
				this.showCamerasText.text = GlobalVariables.showCameras ? "X" : "";
			}
		}
	}
}
