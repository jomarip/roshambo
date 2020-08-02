using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AssemblyCSharp.Assets.Scripts.Models
{
    public class GamePrefabs
	{
		public GameObject MessagePrefab { get; set; }
		public GameObject StarPrefab { get; set; }

		public GameObject PlayerRockCardPrefab { get; set; }
		public GameObject PlayerScissorsCardPrefab { get; set; }
		public GameObject PlayerPaperCardPrefab { get; set; }

		public GameObject OpponentRockCardPrefab { get; set; }
		public GameObject OpponentScissorsCardPrefab { get; set; }
		public GameObject OpponentPaperCardPrefab { get; set; }

		public GameObject PlayerCardRootObject { get; set; }
		public GameObject OpponentCardRootObject { get; set; }

		public GameObject PlayerStarRootObject { get; set; }
		public GameObject OpponentStarRootObject { get; set; }

		public GameObject CardBackground { get; set; }

		public GameObject RoomLobbyFrame { get; set; }
		public GameObject MatchListEntryItem { get; set; }
		public GameObject MatchListFrame { get; set; }
		public GameObject MatchListContent { get; set; }
		public Button CreateMatchButton { get; set; }
		public Button SetupMatchButton { get; set; }
		public Button ReturnToListButton { get; set; }

		public Text InfoText { get; set; }

		public GameObject CounterFrame { get; set; }
		public Text RockCounter { get; set; }
		public Text PaperCounter { get; set; }
		public Text ScissorsCounter { get; set; }

		public Button ReturnHomeButton { get; set; }
		public Button RematchButton { get; set; }

		public GameObject VictoryFrame { get; set; }
		public GameObject DefeatFrame { get; set; }
		public GameObject MatchResultFrame { get; set; }

		public Button MultiPlayerButton { get; set; }
		public Button SinglePlayerButton { get; set; }
		public Button StartMatchButton { get; set; }

		public GameObject MatchSetupFrame { get; set; }
		public GameObject SelectGameTypeFrame { get; set; }

		public Text UsernameText { get; set; }
		public Text PasswordText { get; set; }
		public Button LoginButton { get; set; }
		public GameObject LoginFrame { get; set; }

		public CardController PlayerCardController { get; set; }
		public CardController OpponentCardController { get; set; }

		public StarController PlayerStarController { get; set; }
		public StarController OpponentStarController { get; set; }

		public Toggle AllCardsToggle { get; set; }
		public Toggle RoundsToggle { get; set; }
		public Text RoundCount { get; set; }
		public Text TimerLength { get; set; }
		public Text MatchName { get; set; }
	}
}
