using Roshambo.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfo : MonoBehaviour
{
	public EventHandler<CardInfo> CardSelected { get; set; }

	public UserId Owner { get; set; }
	public CardType Type { get; set; }
	public Vector3 Origin { get; set; }
	public GameObject OpponentPlatform { get; private set; }

	// Start is called before the first frame update
	void Start()
    {
	}

	// Update is called once per frame
	void Update()
    {
        
    }
}
