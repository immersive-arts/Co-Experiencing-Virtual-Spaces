//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Discovery;

namespace IAS.CoLocationMUVR
{
	public class MenuUIManager : MonoBehaviour
	{
		public bool isVRPlayerLobby = true;
		
		public static MenuUIManager Instance;
		public GameObject mainParent;
		public GameObject joinServerParent;
		public GameObject createHostParent;
		
		public Image[] trackingIndexImages;
		public int currentTrackingIndex = 0;
		
		public GameObject serverTextParent;
		
		public GameObject sceneTextParent;
		public GameObject sceneTextPrefab;
		
		[Scene]
		public string[] scenes;
		
		public Text debugVisibility;
		public Text showCamerasText;
		
		public GameObject showServersButton;
		public GameObject startHostButton;
		
		public float discoverServerWaitTime = 2f;
		private float _lastDiscoverserverTime;

		public virtual void Start()
		{
			if (NetworkDiscoveryUI.Instance != null)
				NetworkDiscoveryUI.Instance.targetMenuUI = this;
			
			GlobalVariables.LoadPlayerPrefs();
			this.currentTrackingIndex = GlobalVariables.trackingIndex;
			GlobalVariables.joinAsVRPlayer = this.isVRPlayerLobby;
			
			Instance = this;
			this.SetActiveTrackingIndexImage();
			
			this.CreateSceneTexts();
			this.UpdateDebugObjectVisibility();
			this.UpdateShowCamerasUI();
		}
		
		private void Update()
		{
			if (this._lastDiscoverserverTime < Time.time && this.joinServerParent != null && this.joinServerParent.activeInHierarchy)
				this.DiscoverServers();
		}
		
		public void SetTrackingIndex(int index)
		{
			this.currentTrackingIndex = index;
			this.SetActiveTrackingIndexImage();
		}
		
		void SetActiveTrackingIndexImage()
		{
			for (int i = 0; i < this.trackingIndexImages.Length; i++)
			{
				if (i == this.currentTrackingIndex)
				{
					this.trackingIndexImages[i].color = Color.white;
					this.trackingIndexImages[i].transform.localScale = Vector3.one * 1.2f;
				}
				else
				{
					this.trackingIndexImages[i].color = Color.grey;
					this.trackingIndexImages[i].transform.localScale = Vector3.one;
				}
			}
		}

		//create possible scenes/games buttons
		void CreateSceneTexts()
		{
			if (this.scenes.Length > 0 && this.sceneTextParent != null)
			{
				for (int i = 0; i < scenes.Length; i++)
				{
					GameObject obj = GameObject.Instantiate(this.sceneTextPrefab, this.sceneTextParent.transform);
					Text objText = obj.GetComponentInChildren<Text>();
					string sceneName = this.scenes[i];
					int stringIndex = sceneName.LastIndexOf('/');
					if (stringIndex > 0)
						sceneName = sceneName.Substring(stringIndex + 1, sceneName.Length - stringIndex - 7);
					
					objText.text = sceneName;
					
					int index = i;
					
					obj.GetComponent<InteractiveEvent>().interactEvents.AddListener(delegate { this.SelectScene(index); });
				}
			}
		}

		//delete al server buttons
		public void DeletAllServerTexts()
		{
			for (int i = this.serverTextParent.transform.childCount; i > 0; i--)
			{
				Destroy(this.serverTextParent.transform.GetChild(i - 1).gameObject);
			}
		}

		//create running server button
		public void CreateServerTexts(ServerResponse info)
		{
			GameObject obj = GameObject.Instantiate(this.sceneTextPrefab, this.serverTextParent.transform);
			Text objText = obj.GetComponentInChildren<Text>();
			objText.text = info.EndPoint.Address.ToString();
			
			obj.GetComponent<InteractiveEvent>().interactEvents.AddListener(delegate { this.Connect(info); });
		}

		//invoke to join a running server
		void Connect(ServerResponse info)
		{
			NetworkManager.singleton.StartClient(info.uri);
		}

		//invoke to start a host/game
		public void SelectScene(int index)
		{
			if (NetworkManager.singleton != null)
			{
				NetworkManager.singleton.onlineScene = scenes[index];
				
				if (NetworkDiscoveryUI.Instance != null)
					NetworkDiscoveryUI.Instance.StartHost();
			}
		}
		
		public void ToggleDebugObjectVisibility()
		{
			GlobalVariables.showDebugObjects = !GlobalVariables.showDebugObjects;
			this.UpdateDebugObjectVisibility();
		}
		void UpdateDebugObjectVisibility()
		{
			if (GlobalVariables.showDebugObjects && this.debugVisibility != null)
				this.debugVisibility.text = "X";
			else if (!GlobalVariables.showDebugObjects && this.debugVisibility != null)
				this.debugVisibility.text = "";
		}
		
		
		public void ToggleShowCameras()
		{
			GlobalVariables.ToggleShowCamera();
			this.UpdateShowCamerasUI();
		}
		
		void UpdateShowCamerasUI()
		{
			if (GlobalVariables.showCameras && this.showCamerasText != null)
				this.showCamerasText.text = "X";
			else if (!GlobalVariables.showCameras && this.showCamerasText != null)
				this.showCamerasText.text = "";
		}
		
		public void ToggleStartHost()
		{
			//activate/deactivate host button parent
			if (this.createHostParent != null)
			{
				if (this.createHostParent.activeInHierarchy)
				{
					this.createHostParent.SetActive(false);
					if (this.startHostButton != null)
						this.startHostButton.transform.localScale = Vector3.one;
				}
				else
				{
					this.createHostParent.SetActive(true);
					if (this.startHostButton != null)
						this.startHostButton.transform.localScale = Vector3.one * 1.1f;
				}
			}

			//deactivate server button parent
			if (this.joinServerParent != null)
			{
				this.joinServerParent.SetActive(false);
				if (this.showServersButton != null)
					this.showServersButton.transform.localScale = Vector3.one;
			}

			//activate/deactivate main UI parent
			if (this.mainParent != null)
			{
				if (this.createHostParent != null && this.createHostParent.activeInHierarchy)
					this.mainParent.SetActive(false);
				else if (this.joinServerParent != null && this.joinServerParent.activeInHierarchy)
					this.mainParent.SetActive(false);
				else
					this.mainParent.SetActive(true);
			}
		}

		public void ToggleShowServer()
		{
			//activate/deactivate running servers button parent
			if (this.joinServerParent != null)
			{
				if (this.joinServerParent.activeInHierarchy)
				{
					this.joinServerParent.SetActive(false);
					if (this.showServersButton != null)
						this.showServersButton.transform.localScale = Vector3.one;
				}
				else
				{
					this.joinServerParent.SetActive(true);
					if (this.showServersButton != null)
						this.showServersButton.transform.localScale = Vector3.one * 1.1f;
				}
			}

			//deactivate host button parent
			if (this.createHostParent != null)
			{
				this.createHostParent.SetActive(false);
				if (this.startHostButton != null)
					this.startHostButton.transform.localScale = Vector3.one;
			}

			//activate/deactivate main UI parent
			if (this.mainParent != null)
			{
				if (this.createHostParent != null && this.createHostParent.activeInHierarchy)
					this.mainParent.SetActive(false);
				else if (this.joinServerParent != null && this.joinServerParent.activeInHierarchy)
					this.mainParent.SetActive(false);
				else
					this.mainParent.SetActive(true);
			}
			
			this.DiscoverServers();
		}
		
		
		public void DiscoverServers()
		{
			this._lastDiscoverserverTime = Time.time + this.discoverServerWaitTime;
			if (NetworkDiscoveryUI.Instance != null)
				NetworkDiscoveryUI.Instance.FindServer();
		}
	}
}
