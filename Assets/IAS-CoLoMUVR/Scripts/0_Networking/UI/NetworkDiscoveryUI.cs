//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;

namespace IAS.CoLocationMUVR
{
	// adapted copy of the NetworkDiscovery class from Mirror
	public class NetworkDiscoveryUI : MonoBehaviour
	{
		public static NetworkDiscoveryUI Instance;
		public MenuUIManager targetMenuUI;
		
		readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
		Vector2 scrollViewPos = Vector2.zero;
		
		public NetworkDiscovery networkDiscovery;
		
		private void Awake()
		{
			if (Instance == null)
				Instance = this;
		}
		
		#if UNITY_EDITOR
		void OnValidate()
		{
			if (networkDiscovery == null)
			{
				networkDiscovery = GetComponent<NetworkDiscovery>();
				UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
				UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
			}
		}
		#endif
		
		public void DiscoverServer()
		{
			if (this.targetMenuUI != null)
			{
				this.targetMenuUI.DeletAllServerTexts();

				//create a UI button if a server get discoverd
				foreach (ServerResponse info in discoveredServers.Values)
					this.targetMenuUI.CreateServerTexts(info);
			}
		}
		
		
		public void OnDiscoveredServer(ServerResponse info)
		{
			// Note that you can check the versioning to decide if you can connect to the server or not using this method
			discoveredServers[info.serverId] = info;
			
			this.DiscoverServer();
		}
		
		public void FindServer()
		{
			discoveredServers.Clear();
			networkDiscovery.StartDiscovery();
		}
		
		// LAN Host
		public void StartHost()
		{
			discoveredServers.Clear();
			NetworkManager.singleton.StartHost();
			networkDiscovery.AdvertiseServer();
		}
		
		// Dedicated server
		public void StartServerServer()
		{
			discoveredServers.Clear();
			NetworkManager.singleton.StartServer();
			
			networkDiscovery.AdvertiseServer();
		}
	}
}
