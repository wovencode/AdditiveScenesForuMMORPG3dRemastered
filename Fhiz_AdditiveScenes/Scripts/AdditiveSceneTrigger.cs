using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using Fhiz;

namespace Fhiz {

	/*
	* AdditiveSceneTrigger
	*/
	[RequireComponent(typeof(BoxCollider))]
	public partial class AdditiveSceneTrigger : MonoBehaviour
	{

		[Header("Components")]
		public AdditiveSceneManager additiveSceneManager;

		[Tooltip("Assign the additive scene to load for this zone")]
		public UnityScene additiveScene;

		[Header("Debug")]
		public bool isActive = true;

		[Header("Editor")]
		public Color gizmoColor = new Color(0, 1, 1, 0.25f);
		public Color gizmoWireColor = new Color(1, 1, 1, 0.8f);

		/*
		* Start
		* @Client
		* Destroys the game object when on a client as its not required
		*/
		[ClientCallback]
		void Start()
		{
			if (!NetworkServer.active)
				Destroy(this.gameObject);
		}

		/*
		* OnTriggerEnter
		* @Server
		* Adds the player to the new scene by increasing player count and loads the scene
		* on the server as well as on all nearby clients (works when logging in as well).
		*
		*/
		[ServerCallback]
		void OnTriggerEnter(Collider other)
		{
		
			if (!isActive)
				return;		

			Player player = other.GetComponentInParent<Player>();

			if (player && player.isServer)
				if (NetworkServer.active)
					additiveSceneManager.LoadAdditiveSceneAsync(additiveScene.SceneName, player);

		}
	
		/*
		* OnTriggerExit
		* @Server
		* Removes the player from the scene again, reduces player count and unloads the scene
		* everywhere if no players are left inside (works when logging out as well).
		*
		*/
		[ServerCallback]
		void OnTriggerExit(Collider other)
		{

			if (!isActive)
				return;		

		   Player player = other.GetComponentInParent<Player>();

			if (player && player.isServer)
				if (NetworkServer.active)
					additiveSceneManager.UnloadAdditiveSceneAsync(additiveScene.SceneName, player);
				
		}

		/*
		* OnDrawGizmos
		* @Editor
		* Just to show the scene trigger area in editor
		*/
		void OnDrawGizmos()
		{
			BoxCollider collider = GetComponent<BoxCollider>();

			// we need to set the gizmo matrix for proper scale & rotation
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
			Gizmos.color = gizmoColor;
			Gizmos.DrawCube(collider.center, collider.size);
			Gizmos.color = gizmoWireColor;
			Gizmos.DrawWireCube(collider.center, collider.size);
			Gizmos.matrix = Matrix4x4.identity;
		}

	}

}