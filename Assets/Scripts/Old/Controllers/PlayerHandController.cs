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
    public class PlayerHandController : HandControllerBase
	{
		public PlayerHandController(OldHandState hand, GameObject rockPrefab, GameObject paperPrefab, GameObject scissorsPrefab, GameObject cardBack, GameObject star) {
			Hand = hand;
			StarPrefab = star;
			BackgroundPrefab = cardBack;
			RockPrefab = rockPrefab;
			PaperPrefab = paperPrefab;
			ScissorsPrefab = scissorsPrefab;
		}

		public override void Update() {
		}

		protected override void AddEventHooks(GameObject card, CardType type) {
		}
	}
}
