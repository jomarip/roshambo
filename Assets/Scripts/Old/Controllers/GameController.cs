using AssemblyCSharp.Assets.Scripts.Models;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyCSharp.Assets.Scripts.Controllers
{
	public class GameController {
		public enum GameState {
			MainMenu,
			Match,
			PostMatch
		}

		public GlobalInventoryModel GlobalInventory { get; private set; }
		public OldMatchController CurrentMatch { get; private set; }
		public OldHandState PlayerHand { get; private set; }
		public List<OldHandState> OpponentHands { get; private set; }

		private Random random;
		private GamePrefabs prefabs;

		public GameController(GamePrefabs prefabs) {
			this.SetupNewGame();
			this.random = new Random();
			this.prefabs = prefabs;
			this.prefabs.ReturnHomeButton.onClick.AddListener(OnReturnHomeClicked);
			this.prefabs.RematchButton.onClick.AddListener(OnRematchClicked);
			this.prefabs.SinglePlayerButton.onClick.AddListener(OnStartSinglePlayerClicked);
			this.prefabs.MatchResultFrame.SetActive(false);
			this.prefabs.MatchSetupFrame.SetActive(true);
		}

		private void OnStartSinglePlayerClicked() {
			this.SetupNewGame();
			this.StartMatch();
		}

		private void OnRematchClicked() {
			this.SimulateOpponentMatches(OldMatchController.MatchStyleOld.Rounds, new OldMatchController.MatchStyleOptions() {
				RoundCount = 3,
			});

			this.StartMatch();
		}

		private void OnReturnHomeClicked() {
			this.prefabs.MatchResultFrame.SetActive(false);
			this.prefabs.MatchSetupFrame.SetActive(true);
			if (CurrentMatch != null)
				CurrentMatch.Dispose();

			CurrentMatch = null;
		}

		private void SetupNewGame() {
			this.GlobalInventory = new GlobalInventoryModel();
			this.PlayerHand = new OldHandState();
			this.PlayerHand.Cards = this.GlobalInventory.CreateNewPlayerHand();
			this.PlayerHand.Stars = 3;
			this.OpponentHands = Enumerable.Range(1, 10).Select(_ => new OldHandState {
				Cards = this.GlobalInventory.CreateNewPlayerHand(),
				Stars = 3,
			}).ToList();
		}

		public void StartMatch() {
			this.prefabs.MatchSetupFrame.SetActive(false);
			this.prefabs.MatchResultFrame.SetActive(false);
			this.prefabs.VictoryFrame.SetActive(false);
			this.prefabs.DefeatFrame.SetActive(false);

			if (this.CurrentMatch != null)
				this.CurrentMatch.EndMatch();

			var validOpponents = OpponentHands.Where(i => i.Stars > 0 && i.Cards.Total > 0);
			if(validOpponents.Any() == false) {
				return;
			}

			var opponent = validOpponents.Skip(random.Next(validOpponents.Count() - 1)).First();

			this.CurrentMatch = new OldMatchController(
				prefabs,
				GlobalInventory,
				PlayerHand,
				opponent,
				OldMatchController.MatchStyleOld.Rounds,
				new OldMatchController.MatchStyleOptions() {
					RoundCount = 5
				});

			this.CurrentMatch.MatchCompletedEvent = OnMatchCompleted;
			this.CurrentMatch.StartMatch();
		}

		public void SimulateOpponentMatches(OldMatchController.MatchStyleOld style, OldMatchController.MatchStyleOptions options) {
			var opponents = OpponentHands.Where(i => i.Stars > 0 && i.Cards.HasCards).ToList();
			for (int i = 0; i < opponents.Count; i++) {
				var ind = random.Next(opponents.Count - 1);
				var tmp = opponents[i];
				opponents[i] = opponents[ind];
				opponents[ind] = tmp;
			}

			List<(OldHandState first, OldHandState second)> matchups = new List<(OldHandState first, OldHandState second)>();

			while (opponents.Count > 1) {
				var first = opponents.First();
				var second = opponents.Skip(1).First();
				opponents = opponents.Skip(2).ToList();
				matchups.Add((first, second));
			}

			foreach(var match in matchups) {
				SimulateOpponentMatch(style, options, match.first, match.second);
			}
		}

		public void SimulateOpponentMatch(OldMatchController.MatchStyleOld style, OldMatchController.MatchStyleOptions options, OldHandState first, OldHandState second) {
			switch(style) {
				case OldMatchController.MatchStyleOld.AllCards:

					break;

				case OldMatchController.MatchStyleOld.Rounds:
					for(int i = 0; i < options.RoundCount; i++) {
						if (first.Cards.HasCards == false || second.Cards.HasCards == false)
							break;

						var firstCard = first.Cards.PickRandom(random);
						var secondCard = second.Cards.PickRandom(random);

						if (firstCard == secondCard)
							continue;

						if (firstCard == CardType.Rock) {
							if (secondCard == CardType.Paper) {
								first.Stars--;
								second.Stars++;
							} else {
								first.Stars++;
								second.Stars--;
							}
						}

						if (firstCard == CardType.Paper) {
							if (secondCard == CardType.Scissors) {
								first.Stars--;
								second.Stars++;
							} else {
								first.Stars++;
								second.Stars--;
							}
						}

						if (firstCard == CardType.Scissors) {
							if (secondCard == CardType.Rock) {
								first.Stars--;
								second.Stars++;
							} else {
								first.Stars++;
								second.Stars--;
							}
						}

						if (first.Stars == 0 || second.Stars == 0)
							break;
					}
					break;
			}
		}

		private void OnMatchCompleted(object sender, OldMatchController e) {
			this.prefabs.MatchResultFrame.SetActive(true);
			switch(e.CurrentState) {
				case OldMatchController.MatchState.MatchEndedVictory:
					this.prefabs.VictoryFrame.SetActive(true);
					this.prefabs.DefeatFrame.SetActive(false);
					break;

				case OldMatchController.MatchState.MatchEndedDefeat:
					this.prefabs.VictoryFrame.SetActive(false);
					this.prefabs.DefeatFrame.SetActive(true);
					this.SetupNewGame();
					break;

				case OldMatchController.MatchState.MatchEndedDraw:
					this.prefabs.VictoryFrame.SetActive(true);
					this.prefabs.DefeatFrame.SetActive(false);
					break;
			}

			if (CurrentMatch != null) {
				CurrentMatch.Dispose();
				CurrentMatch = null;
			}
		}

		public void Update() {
			if (this.CurrentMatch != null)
				this.CurrentMatch.UpdateMatch();
		}
    }
}
