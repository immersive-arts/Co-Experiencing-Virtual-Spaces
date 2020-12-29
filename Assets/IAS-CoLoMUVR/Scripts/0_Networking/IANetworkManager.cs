//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace IAS.CoLocationMUVR {
	//custom message used for spawning different player prefabs
	public struct PlayerSpawnMessage : NetworkMessage
	{
		public bool isVrPlayer; //if false: spawn a camera player //The camera player prefab is in the base mirror playerPrefab slot
		public int index;

		public void Deserialize(NetworkReader reader) { }

		public void Serialize(NetworkWriter writer) { }
	}

	[AddComponentMenu("")]
	public class IANetworkManager : NetworkManager
	{
		//let you use different VRPlayers
		public List<GameObject> vrPlayersPrefabs = new List<GameObject>();

		//register the SpawnMessage
		public override void OnStartServer()
		{
			base.OnStartServer();
			NetworkServer.RegisterHandler<PlayerSpawnMessage>(OnCreateCharacter);
		}

		//this function get invoked when a PlayerSpawnMessage is received
		void OnCreateCharacter(NetworkConnection conn, PlayerSpawnMessage message)
		{
			GameObject prefab = this.GetCorrectPlayerPrefab(message.isVrPlayer, message.index);
			if (prefab != null)
			{
				GameObject gameObject = Instantiate(prefab);
				//call this to use this gameobject as the primary player
				NetworkServer.AddPlayerForConnection(conn, gameObject);

				//add serverUI info text if it exists
				if (ServerUI.Instance != null)
				{
					if (message.isVrPlayer)
						ServerUI.Instance.AddServerLogText("New Player has joined the Server " + conn.address +  " : Type index " + message.index);
					else
						ServerUI.Instance.AddServerLogText("New Player has joined the Server " + conn.address +  " : Camera");

					ServerUI.Instance.AddServerLogText("-Current connections: " + NetworkServer.connections.Count);
				}
			}
		}


		public override void OnServerDisconnect(NetworkConnection conn)
		{
			//add serverUI info text if it exists
			if (ServerUI.Instance != null)
			{
				ServerUI.Instance.AddServerLogText("Player disconnect");
				ServerUI.Instance.AddServerLogText("-Current connections: " + NetworkServer.connections.Count);
			}
			base.OnServerDisconnect(conn);
		}

		public override void OnClientConnect(NetworkConnection conn)
		{
			base.OnClientConnect(conn);

			//send the PlayerSapwnMessage when the client connect
			PlayerSpawnMessage characterMessage = new PlayerSpawnMessage
			{
				//take data from static variables
				isVrPlayer = GlobalVariables.joinAsVRPlayer,
				index = GlobalVariables.playerTypeIndex
			};

			conn.Send(characterMessage);

		}

		//get the right player prefab to spawn
		GameObject GetCorrectPlayerPrefab(bool joinsAsVRPlayer, int index)
		{
			if (joinsAsVRPlayer)
			{
				if (index >= 0 && this.vrPlayersPrefabs.Count > index)
					return this.vrPlayersPrefabs[index];
				else
					return this.playerPrefab;
			}
			else
				return this.playerPrefab;
		}

		//register the vrPlayer prefabs
		public override void RegisterClientMessages()
		{
			base.RegisterClientMessages();

			for (int i = 0; i < vrPlayersPrefabs.Count; i++)
			{
				GameObject prefab = vrPlayersPrefabs[i];
				if (prefab != null)
				{
					ClientScene.RegisterPrefab(prefab);
				}
			}
		}
	} 
}
