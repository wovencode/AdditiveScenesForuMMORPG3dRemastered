using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using System.Linq;
using Tymski;
using Fhiz;

namespace Fhiz {

	/*
	* AdditiveScene
	*/
	[Serializable]
	public struct additiveScene
	{
		
		[Tooltip("Drag and drop a scene object from your project here")]
		public SceneReference scene;

		[Tooltip("Drag and drop a collider object from main scene hierarchy here")]
		public Collider collider;   // required for bounds check

		[HideInInspector]public bool loaded; // is the scene currently loaded?
		[HideInInspector]public List<Player> players; // number of players inside

		/*
		* name
		* Returns the scene objects name
		*/
		public string name
		{
			get
			{
				return scene.ScenePath;
			}
		}

		/*
		* level
		* Returns the total level of all players currently inside this scene
		*/
		public int level
		{
			get
			{
				int totalLevel = 0;

				foreach (Player player in players)
					if (player != null)
						totalLevel += player.level.current;

				return totalLevel;
			}

		}

	}

}