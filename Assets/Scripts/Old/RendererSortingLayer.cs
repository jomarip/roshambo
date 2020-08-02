using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererSortingLayer : MonoBehaviour
{
	public int SortLayer = 0;
	public int SortingLayerID;
	// Use this for initialization
	void Start() {
		SortingLayerID = SortingLayer.GetLayerValueFromName("UI Text");
		Renderer renderer = this.gameObject.GetComponent<Renderer>();
		if (renderer != null) {
			renderer.sortingOrder = SortLayer;
			renderer.sortingLayerID = SortingLayerID;
		}
	}

}
