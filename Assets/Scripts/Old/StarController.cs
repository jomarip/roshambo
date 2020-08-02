using Roshambo.Common.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StarController : MonoBehaviour
{
	private List<GameObject> stars = new List<GameObject>();

	public virtual void GenerateStars(PlayerState player, GameObject starPrefab) {
		if (stars.Any()) {
			foreach (var star in stars)
				if (star != null)
					GameObject.Destroy(star);
		}

		stars.Clear();
		var top = 1.7f;
		var bottom = -1.7f;
		var div = (top - bottom) / (float)player.Stars;
		for (int i = 0; i < player.Stars; i++) {
			var star = GameObject.Instantiate(starPrefab);
			star.transform.parent = transform;
			star.transform.localPosition = new Vector3(0f, (top + (div / 2f)) - ((i + 1) * div), 0f);
			stars.Add(star);
		}
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
