using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using System.Linq;
using Fhiz;

namespace Fhiz {

	/*
	* PlayerAdditiveSceneController
	*/
	[RequireComponent(typeof(Player))]
	[DisallowMultipleComponent]
	public partial class PlayerAdditiveSceneController : NetworkBehaviour
	{

		protected AdditiveSceneManager additiveScenesManager;

		/*
		* Start
		* @Client @Server
		*/
		public void Start()
		{
			additiveScenesManager = ((NetworkManagerMMO)NetworkManager.singleton).GetComponent<AdditiveSceneManager>();
		}

		/*
		* RpcClientLoadAdditiveScene
		* @Client
		* Loads the scene on all nearby clients except the client who triggered this
		* (as it is loaded on the local client already)
		*
		*/
		[ClientRpc(includeOwner = false)]
		public void RpcClientLoadAdditiveScene(string additiveSceneName)
		{
			additiveScenesManager.LoadAdditiveSceneAsync(additiveSceneName);
		}

		/*
		* RpcClientUnloadAdditiveScene
		* @Client
		* Unloads the the scene on all nearby clients except the client who triggered this
		* (as it is unloaded on the local client already
		*
		*/
		[ClientRpc(includeOwner = false)]
		public void RpcClientUnloadAdditiveScene(string additiveSceneName)
		{
			additiveScenesManager.UnloadAdditiveSceneAsync(additiveSceneName);
		}

	}

}