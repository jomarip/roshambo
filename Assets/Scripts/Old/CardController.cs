using AssemblyCSharp.Assets.Scripts.Models;
using Roshambo.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HandLayout
{
	RightToLeft, LeftToRight
}

public class CardController : MonoBehaviour
{
	public EventHandler<CardInfo> CardSelected { get; set; }

	public float dispX;
	public float dispY;

	private List<GameObject> stars = new List<GameObject>();
	private List<GameObject> backgrounds = new List<GameObject>();
	private GameObject[,] cardArray = new GameObject[3, 3];

	public List<CardInfo> Cards { get; private set; } = new List<CardInfo>();

	public void GenerateCards(HandState hand, GameObject rockPrefab, GameObject paperPrefab, GameObject scissorsPrefab, GameObject cardBack, HandLayout layout) {
		if(this.Cards.Any()) {
			foreach (var c in Cards)
				GameObject.Destroy(c);

			Cards.Clear();
		}

		Vector3 offset = new Vector3();
		if (layout == HandLayout.LeftToRight)
			offset = new Vector3(0f, 0.3f, 0f);

		for (int i = 1; i <= 3; i++) {
			var cardBg = GameObject.Instantiate(cardBack);
			cardBg.transform.parent = transform;
			cardBg.transform.localPosition = (new Vector3((i - 1) * 1.35f, 4f, 0f)) - offset;
			backgrounds.Add(cardBg);
			if (hand.RockCount >= i) {
				var card = GameObject.Instantiate(rockPrefab);
				card.transform.parent = transform;
				card.transform.localPosition = new Vector3((i - 1) * 1.35f, 4f, 0f);
				cardArray[i - 1, 0] = card;
			}

			cardBg = GameObject.Instantiate(cardBack);
			cardBg.transform.parent = transform;
			cardBg.transform.localPosition = (new Vector3((i - 1) * 1.35f, 2f, 0f)) - offset;
			backgrounds.Add(cardBg);
			if (hand.PaperCount >= i) {
				var card = GameObject.Instantiate(paperPrefab);
				card.transform.parent = transform;
				card.transform.localPosition = new Vector3((i - 1) * 1.35f, 2f, 0f);
				cardArray[i - 1, 1] = card;
			}

			cardBg = GameObject.Instantiate(cardBack);
			cardBg.transform.parent = transform;
			cardBg.transform.localPosition = (new Vector3((i - 1) * 1.35f, 0f, 0f)) - offset;
			backgrounds.Add(cardBg);
			if (hand.ScissorsCount >= i) {
				var card = GameObject.Instantiate(scissorsPrefab);
				card.transform.parent = transform;
				card.transform.localPosition = new Vector3((i - 1) * 1.35f, 0f, 0f);
				cardArray[i - 1, 2] = card;
			}
		}

		foreach(var card in cardArray) {
			var info = card.AddComponent<CardInfo>();
			this.Cards.Add(info);
		}

		foreach(var card in Cards) {
			card.CardSelected += (_, e) => this.CardSelected(this, e);
		}

		for(int i = 0; i < 3; i++) {
			cardArray[i, 0].GetComponent<CardInfo>().Type = CardType.Rock;
			cardArray[i, 1].GetComponent<CardInfo>().Type = CardType.Paper;
			cardArray[i, 2].GetComponent<CardInfo>().Type = CardType.Scissors;
		}
	}

	public void SetDragActive(bool active) {

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
