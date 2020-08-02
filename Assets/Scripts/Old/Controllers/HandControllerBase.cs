using AssemblyCSharp.Assets.Scripts.Models;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AssemblyCSharp.Assets.Scripts.Controllers
{
    public abstract class HandControllerBase
    {
		public enum HandLayout
		{
			RightToLeft,
			LeftToRight,
		}

		public EventHandler<CardType> CardChosenEvent { get; set; }

		public OldHandState Hand { get; protected set; }
		public GameObject StarPrefab { get; protected set; }
		public GameObject BackgroundPrefab { get; protected set; }
		public GameObject RockPrefab { get; protected set; }
		public GameObject PaperPrefab { get; protected set; }
		public GameObject ScissorsPrefab { get; protected set; }

		private List<GameObject> stars = new List<GameObject>();
		private List<GameObject> backgrounds = new List<GameObject>();
		private GameObject[,] cardArray = new GameObject[3,3];

		public abstract void Update();
		protected abstract void AddEventHooks(GameObject card, CardType type);

		public virtual void GenerateStars(GameObject parent, Vector3 pin) {
			if(stars.Any()) {
				foreach (var star in stars)
					if(star != null)
						GameObject.Destroy(star);
			}

			stars.Clear();
			var top = 1.7f;
			var bottom = -1.7f;
			var div = (top - bottom) / (float)Hand.Stars;
			for(int i = 0; i < Hand.Stars; i++) {
				var star = GameObject.Instantiate(StarPrefab);
				star.transform.position = pin + new Vector3(0f, (top + (div / 2f)) - ((i + 1) * div), 0f);
				stars.Add(star);
			}
		}

		public virtual void GenerateCards(GameObject parent, Vector3 pin, HandLayout layout) {
			//hack fix because the opponent card prefabs had different center positions
			Vector3 offset = new Vector3();
			if (layout == HandLayout.LeftToRight)
				offset = new Vector3(0f, 0.3f, 0f);

			for (int i = 1; i <= 3; i++) {
				var cardBg = GameObject.Instantiate(this.BackgroundPrefab);
				cardBg.transform.parent = parent.transform;
				cardBg.transform.position = (new Vector3(pin.x + ((i - 1) * 1.35f), pin.y + 4f, 0f)) - offset;
				backgrounds.Add(cardBg);
				if (this.Hand.Cards.RockCount >= i) {
					var card = GameObject.Instantiate(this.RockPrefab);
					card.transform.parent = parent.transform;
					card.transform.position = new Vector3(pin.x + ((i - 1) * 1.35f), pin.y + 4f, 0f);
					this.AddEventHooks(card, CardType.Rock);
					cardArray[i - 1, 0] = card;
				}

				cardBg = GameObject.Instantiate(this.BackgroundPrefab);
				cardBg.transform.parent = parent.transform;
				cardBg.transform.position = (new Vector3(pin.x + ((i - 1) * 1.35f), pin.y + 2f, 0f)) - offset;
				backgrounds.Add(cardBg);
				if (this.Hand.Cards.PaperCount >= i) {
					var card = GameObject.Instantiate(this.PaperPrefab);
					card.transform.parent = parent.transform;
					card.transform.position = new Vector3(pin.x + ((i - 1) * 1.35f), pin.y + 2f, 0f);
					this.AddEventHooks(card, CardType.Paper);
					cardArray[i - 1, 1] = card;
				}

				cardBg = GameObject.Instantiate(this.BackgroundPrefab);
				cardBg.transform.position = (new Vector3(pin.x + ((i - 1) * 1.35f), pin.y, 0f)) - offset;
				cardBg.transform.parent = parent.transform;
				backgrounds.Add(cardBg);
				if (this.Hand.Cards.ScissorsCount >= i) {
					var card = GameObject.Instantiate(this.ScissorsPrefab);
					card.transform.parent = parent.transform;
					card.transform.position = new Vector3(pin.x + ((i - 1) * 1.35f), pin.y, 0f);
					this.AddEventHooks(card, CardType.Scissors);
					cardArray[i - 1, 2] = card;
				}
			}

			if(layout == HandLayout.LeftToRight) {
				List<GameObject> cards = new List<GameObject>();
				foreach (var c in cardArray)
					if (c != null)
						cards.Add(c);

				AiManagerController.Instance.aiCards = cards;
			}
		}

		public void Dispose() {
			foreach (var obj in cardArray)
				if (obj != null)
					GameObject.Destroy(obj);

			foreach (var bg in backgrounds)
				GameObject.Destroy(bg);

			foreach (var star in stars)
				GameObject.Destroy(star);

			backgrounds = null;
			cardArray = null;
			stars = null;
		}
	}
}
