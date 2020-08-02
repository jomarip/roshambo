using AssemblyCSharp.Assets.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Roshambo.Common.Models;

namespace AssemblyCSharp.Assets.Scripts.Controllers
{
    public class OldMatchController
    {
		public EventHandler<OldMatchController> MatchCompletedEvent { get; set; }

		public enum MatchStyleOld
		{
			Rounds,
			AllCards,
		}

		public struct MatchStyleOptions
		{
			public int RoundCount { get; set; }
		}

        public enum MatchState
		{
			MatchNew,
			PlayerWaiting,
			PlayerChosen,
			OpponentWaiting,
			OpponentChosen,
			DisplayWin,
			DisplayLoss,
			DisplayDraw,
			MatchEndedVictory,
			MatchEndedDefeat,
			MatchEndedDraw,
		}

		public MatchStyleOld CurrentStyle { get; private set; }
		public MatchStyleOptions MatchOptions { get; }

		private Stopwatch stateTimer = new Stopwatch();
		private MatchState currentState;
		public MatchState CurrentState {
			get {
				return currentState;
			}

			private set {
				this.stateTimer.Reset();
				this.stateTimer.Start();
				var old = this.currentState;
				this.currentState = value;
				this.OnStateSet(old, value);
			}
		}

		public GameObject PlayerCardObject { get; private set; }
		public GameObject OpponentCardObject { get; private set; }

		public PlayerHandController Player { get; private set; }
		public OpponentHandController Opponent { get; private set; }

		private GlobalInventoryModel globalInventory;
		private GamePrefabs prefabs;
		private CardType playerCard;
		private CardType opponentCard;
		private int round;

		public OldMatchController(GamePrefabs prefabs, GlobalInventoryModel globalInventory, OldHandState playerHand, OldHandState opponentHand, MatchStyleOld style, MatchStyleOptions options) {
			this.CurrentStyle = style;
			this.MatchOptions = options;

			this.globalInventory = globalInventory;
			this.prefabs = prefabs;
			this.Player = new PlayerHandController(
				playerHand,
				prefabs.PlayerRockCardPrefab, 
				prefabs.PlayerPaperCardPrefab, 
				prefabs.PlayerScissorsCardPrefab,
				prefabs.CardBackground,
				prefabs.StarPrefab);

			this.Opponent = new OpponentHandController(
				opponentHand,
				prefabs.OpponentRockCardPrefab, 
				prefabs.OpponentPaperCardPrefab, 
				prefabs.OpponentScissorsCardPrefab,
				prefabs.CardBackground,
				prefabs.StarPrefab);

			this.Player.GenerateCards(prefabs.PlayerCardRootObject, new UnityEngine.Vector3(-7.25f, -2.5f, 0f), HandControllerBase.HandLayout.RightToLeft);
			this.Opponent.GenerateCards(prefabs.OpponentCardRootObject, new UnityEngine.Vector3(4.6f, -2.2f, 0f), HandControllerBase.HandLayout.LeftToRight);

			this.Player.GenerateStars(prefabs.PlayerStarRootObject, new Vector3(-3.15f, -5.45f, 0f));
			this.Opponent.GenerateStars(prefabs.OpponentStarRootObject, new Vector3(3.25f, -5.45f, 0f));

			this.Player.CardChosenEvent += OnPlayerCardChosen;
			this.Opponent.CardChosenEvent += OnOpponentCardChosen;
		}

		private void OnOpponentCardChosen(object sender, CardType e) {
			if (CurrentState != MatchState.OpponentWaiting)
				return;

			this.opponentCard = e;
			this.CurrentState = MatchState.OpponentChosen;
			this.PlayerCardObject = (sender as MonoBehaviour).gameObject;
		}

		private void OnPlayerCardChosen(object sender, CardType e) {
			if (CurrentState != MatchState.PlayerWaiting)
				return;

			this.playerCard = e;
			this.CurrentState = MatchState.PlayerChosen;
			this.OpponentCardObject = (sender as MonoBehaviour).gameObject;
		}

		public void StartMatch() {
			if (this.CurrentState == MatchState.MatchNew)
				this.CurrentState = MatchState.PlayerWaiting;
		}

		public void UpdateMatch() {
			this.Player.Update();
			this.Opponent.Update();

			switch(this.currentState) {
				case MatchState.DisplayWin:
				case MatchState.DisplayLoss:
				case MatchState.DisplayDraw:
					if (this.stateTimer.Elapsed.TotalSeconds > 2) {
						this.CurrentState = MatchState.PlayerWaiting;
						prefabs.InfoText.gameObject.SetActive(false);

						if(this.PlayerCardObject != null) {
							GameObject.Destroy(this.PlayerCardObject);
							this.PlayerCardObject = null;
						}

						if(this.OpponentCardObject != null) {
							GameObject.Destroy(this.OpponentCardObject);
							this.OpponentCardObject = null;
						}

						//end game cases
						this.round += 1;
						switch(CurrentStyle) {
							case MatchStyleOld.AllCards:
								if (Player.Hand.Cards.HasCards == false)
									this.CurrentState = MatchState.MatchEndedDefeat;

								if (Opponent.Hand.Cards.HasCards == false)
									this.CurrentState = MatchState.MatchEndedVictory;

								if (Player.Hand.Stars <= 0)
									this.CurrentState = MatchState.MatchEndedDefeat;

								if (Opponent.Hand.Stars <= 0)
									this.CurrentState = MatchState.MatchEndedVictory;
								break;

							case MatchStyleOld.Rounds:
								if (Player.Hand.Stars <= 0)
									this.CurrentState = MatchState.MatchEndedDefeat;

								if (Opponent.Hand.Stars <= 0)
									this.CurrentState = MatchState.MatchEndedVictory;

								if (this.round == MatchOptions.RoundCount) {
									if(Player.Hand.Stars == Opponent.Hand.Stars)
										this.CurrentState = MatchState.MatchEndedDraw;

									if (Opponent.Hand.Stars <= 0)
										this.CurrentState = MatchState.MatchEndedVictory;

									if (Player.Hand.Stars == 0)
										this.CurrentState = MatchState.MatchEndedDefeat;

									this.CurrentState = MatchState.MatchEndedDraw;
								}

								break;
						}

						if(Player.Hand.Stars <= 0) {
							this.CurrentState = MatchState.MatchEndedDefeat;
						}
					}
					break;
			}
		}

		public void UpdateCounter() {
			this.prefabs.RockCounter.text = globalInventory.TotalRockCount.ToString();
			this.prefabs.PaperCounter.text = globalInventory.TotalPaperCount.ToString();
			this.prefabs.ScissorsCounter.text = globalInventory.TotalScissorsCount.ToString();
		}

		private void OnStateSet(MatchState old, MatchState value) {
			coundownTimerController.Instance.StopTimer();
			dragCardController.Instance.IsActive = false;
			UpdateCounter();

			switch (value) {
				case MatchState.PlayerWaiting:
					dragCardController.Instance.IsActive = true;
					if (old != value) {
						//coundownTimerController.Instance.RunTimer();
						coundownTimerController.Instance.TimeElapsedEvent = (_, __) => {
							if (this.CurrentState == MatchState.PlayerWaiting) {
								Log("Player loss by timer elapsed");
								this.CurrentState = MatchState.DisplayLoss;
							}

							return;
						};
					}
					break;

				case MatchState.PlayerChosen:
					this.CurrentState = MatchState.OpponentWaiting;
					Log("Player chose: " + playerCard.ToString());
					switch (playerCard) {
						case CardType.Paper:
							Player.Hand.Cards.PaperCount -= 1;
							break;

						case CardType.Scissors:
							Player.Hand.Cards.ScissorsCount -= 1;
							break;

						case CardType.Rock:
							Player.Hand.Cards.RockCount -= 1;
							break;
					}
					break;

				case MatchState.OpponentWaiting:
					Log("Waiting on opponent");
					AiManagerController.Instance.SelectRandom();
					break;

				case MatchState.OpponentChosen:

					Log("Opponent chose: " + opponentCard.ToString());
					switch (opponentCard) {
						case CardType.Paper:
							Opponent.Hand.Cards.PaperCount -= 1;
							break;

						case CardType.Scissors:
							Opponent.Hand.Cards.ScissorsCount -= 1;
							break;

						case CardType.Rock:
							Opponent.Hand.Cards.RockCount -= 1;
							break;
					}

					Log("Determining round result");
					this.DetermineRound();
					break;

				case MatchState.DisplayWin:
					Log("Player victory");
					prefabs.InfoText.gameObject.SetActive(true);
					prefabs.InfoText.text = "You Win!";
					Player.Hand.Stars += 1;
					Opponent.Hand.Stars -= 1;
					break;

				case MatchState.DisplayLoss:
					Log("Player loss");
					prefabs.InfoText.gameObject.SetActive(true);
					prefabs.InfoText.text = "You Lose!";
					Player.Hand.Stars -= 1;
					Opponent.Hand.Stars += 1;
					break;

				case MatchState.DisplayDraw:
					Log("Match draw");
					prefabs.InfoText.gameObject.SetActive(true);
					prefabs.InfoText.text = "It's a Draw!";
					break;

				case MatchState.MatchEndedVictory:
					Log("Match ended - Player Victory");
					MatchCompletedEvent?.Invoke(this, this);
					break;

				case MatchState.MatchEndedDefeat:
					Log("Match ended - Player Defeat");
					MatchCompletedEvent?.Invoke(this, this);
					break;

				case MatchState.MatchEndedDraw:
					Log("Match ended - Draw");
					MatchCompletedEvent?.Invoke(this, this);
					break;
			}

			switch(CurrentState) {
				case MatchState.DisplayWin:
				case MatchState.DisplayLoss:
				case MatchState.PlayerWaiting:
				case MatchState.MatchNew:
					this.Player.GenerateStars(prefabs.PlayerStarRootObject, new Vector3(-3.15f, -0.5f, 0f));
					this.Opponent.GenerateStars(prefabs.OpponentStarRootObject, new Vector3(3.25f, -0.5f, 0f));
					break;
			}
		}

		private void DetermineRound() {
			if (playerCard == opponentCard) {
				this.CurrentState = MatchState.DisplayDraw;
				return;
			}

			switch (playerCard) {
				case CardType.Rock:
					if (opponentCard == CardType.Scissors) this.CurrentState = MatchState.DisplayWin;
					if (opponentCard == CardType.Paper) this.CurrentState = MatchState.DisplayLoss;
					break;

				case CardType.Paper:
					if (opponentCard == CardType.Rock) this.CurrentState = MatchState.DisplayWin;
					if (opponentCard == CardType.Scissors) this.CurrentState = MatchState.DisplayLoss;
					break;

				case CardType.Scissors:
					if (opponentCard == CardType.Paper) this.CurrentState = MatchState.DisplayWin;
					if (opponentCard == CardType.Rock) this.CurrentState = MatchState.DisplayLoss;
					break;
			}
		}

		public void EndMatch() {
			if(this.CurrentState != MatchState.MatchEndedDefeat || 
				this.CurrentState != MatchState.MatchEndedVictory || 
				this.CurrentState != MatchState.MatchEndedDraw)
				this.CurrentState = MatchState.MatchEndedDraw;
		}

		private void Log(string message) {
			UnityEngine.Debug.Log(message);
		}

		public void Dispose() {
			Player.Dispose();
			Opponent.Dispose();
		}
    }
}
