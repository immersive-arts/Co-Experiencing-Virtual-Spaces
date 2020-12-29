//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Discovery;

namespace IAS.CoLocationMUVR {
	//screenspace UI for non VR server
    public class ServerUI : MonoBehaviour
    {
        public static ServerUI Instance;
        public GameObject serverUIParent;
        public bool showServerUIOnStart = true;

        public GameObject logParent;

        public Text serverLogText;
        private string[] _serverLog;
        public int logLength = 20;
        private bool _canUpdateLog = false;


		public GameObject serverTextParent;
		public GameObject sceneTextPrefab;
		[Scene]
		public string[] scenes;

		public Text fpsText;
		public GameObject serverRunningUIParent;

        private void Awake()
        {
			if (Instance != null)
				Destroy(this.gameObject);

			DontDestroyOnLoad(this.gameObject);
            Instance = this;
            this._serverLog = new string[this.logLength];
            this.AddServerLogText("Application is Running");

			this.CreateSceneTexts();
			this.ShowServerRunningUI(false);

            if (this.serverUIParent != null)
                this.serverUIParent.SetActive(this.showServerUIOnStart);
        }


        private void Update()
        {
            if (this._canUpdateLog && this.serverLogText != null)
                this.UpdateLogText();
            if (this.fpsText != null)
                this.UpdateFPSText();

            if (Input.GetKeyDown(KeyCode.Space))
                this.ToggleServerUI();
        }

		//add text to the log
        public void AddServerLogText(string text)
        {
            for (int i = this._serverLog.Length -1; i >= 0; i--)
            {
                if (i == 0)
                    this._serverLog[i] = text;
                else
                    this._serverLog[i] = this._serverLog[i - 1];
            }
            this._canUpdateLog = true;
        }

		//update the log Text object in the next frame
        void UpdateLogText()
        {
            if (this.serverLogText != null)
            {
                this.serverLogText.text = "";
                for (int i = this._serverLog.Length - 1; i >= 0; i--)
                {
                    if (this._serverLog[i] != null)
                        this.serverLogText.text = this.serverLogText.text + "\n" + this._serverLog[i];
                }
                this._canUpdateLog = false;
            }
        }

		//turn on/off the logParent
		public void ToggleServerLog()
        {
            if (this.logParent != null)
            {
                this.logParent.gameObject.SetActive(!this.logParent.gameObject.activeInHierarchy);
            }
        }

		//turn on/off the serverUIParent
        public void ToggleServerUI()
        {
            if (this.serverUIParent != null)
                this.serverUIParent.SetActive(!this.serverUIParent.activeInHierarchy);
        }

		//shows the current FPS of the server
        void UpdateFPSText()
        {
            this.fpsText.text = Mathf.RoundToInt(1f / Time.deltaTime).ToString() + " FPS";
        }

		//create UI buttons for all possible scenes
		void CreateSceneTexts()
		{
			if (this.scenes.Length > 0 && this.serverTextParent != null)
			{
				for (int i = 0; i < scenes.Length; i++)
				{
					GameObject obj = GameObject.Instantiate(this.sceneTextPrefab, this.serverTextParent.transform);
					Text objText = obj.GetComponentInChildren<Text>();
					string sceneName = this.scenes[i];
					int stringIndex = sceneName.LastIndexOf('/');
					if (stringIndex > 0)
						sceneName = sceneName.Substring(stringIndex + 1, sceneName.Length - stringIndex - 7);

					objText.text = sceneName;

					int index = i;

					obj.GetComponent<Button>().onClick.AddListener(delegate { this.SelectScene(index); });
				}
			}
		}

		//activate/deactivate the scene selectio buttons
		public void ShowGameSelectionWindow(bool b)
		{
			if (this.serverTextParent != null)
				this.serverTextParent.SetActive(b);
		}

		//activate/deactivate the server running UI
		public void ShowServerRunningUI(bool b)
		{
			if (this.serverRunningUIParent != null)
				this.serverRunningUIParent.SetActive(b);
		}

		//function to start a scene as server
		public void SelectScene(int index)
		{
			if (NetworkManager.singleton != null)
			{
				NetworkManager.singleton.onlineScene = scenes[index];

				if (NetworkDiscoveryUI.Instance != null)
				{
					NetworkDiscoveryUI.Instance.StartServerServer();

					//update the serverUI
					string sceneName = this.scenes[index];
					int stringIndex = sceneName.LastIndexOf('/');
					this.AddServerLogText("---Start Server--- Running Game " + sceneName.Substring(stringIndex + 1, sceneName.Length - stringIndex - 7));
					this.ShowGameSelectionWindow(false);
					this.ShowServerRunningUI(true);
				}
			}
		}

		public void StopServer()
		{
			if (NetworkManager.singleton != null)
			{
				if (NetworkManager.singleton.mode == NetworkManagerMode.ServerOnly)
				{
					//update the serverUI
					NetworkManager.singleton.StopServer();
					this.AddServerLogText("---Stop Server---");
					this.ShowGameSelectionWindow(true);
					this.ShowServerRunningUI(false);
				}
			}
		}
	}
}
