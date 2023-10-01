using UnityEngine;
using UnityEngine.SceneManagement;
using Fhiz;

namespace Fhiz {

	/*
	* UnityScene
	*/
	[System.Serializable]
	public class UnityScene
	{
		
		[SerializeField]
		private Object sceneAsset;
		
		// SceneName
		public string SceneName
		{
			get { return sceneAsset.name; }
		}

	}

}