using AssemblyCSharp.Assets.Scripts.Models;
using Roshambo.Common.Controllers.Matches;
using Roshambo.Common.Controllers.Rounds;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AssemblyCSharp.Assets.Scripts.Managers
{
    public class MatchManager
    {
		public EventHandler<string> DisplayMessageEvent { get; set; }

		public IMatchController Match { get; private set; }
		private GamePrefabs prefabs;
		private UserId localUser;
		private PlayerState player;
		private PlayerState opponent;

		public RoundManager ActiveRound { get; private set; }
		public CardInfo PlayerSelectedCard { get; private set; }
		public CardInfo OpponentSelectedCard { get; private set; }

		public MatchManager(IMatchController match, UserId localUser, GamePrefabs prefabs) {
			this.localUser = localUser;
			this.prefabs = prefabs;
			this.Match = match;

			this.Match.MatchStartedEvent += OnMatchStarted;
			this.Match.MatchEndedEvent += OnMatchEnded;
			this.Match.UserJoinedEvent += OnUserJoined;
			this.Match.UserLeftEvent += OnUserLeft;
			this.Match.RoundCreatedEvent += OnRoundCreated;
		}

		private void OnMatchStarted(object sender, IMatchController e) {
			this.Log(e.Options.Name + ": Match Started.");
			if (this.localUser == null) {
				//TODO: Set up auto AI
				return;
			}

			coundownTimerController.Instance.TimeElapsedEvent = OnTimerElapsed;
			this.player = e.State.PlayerList.First(i => i.Id.GUID == this.localUser.GUID);
			this.opponent = e.State.PlayerList.First(i => i.Id.GUID != this.localUser.GUID);

			this.prefabs.PlayerCardController.GenerateCards(player.PlayerHand, prefabs.PlayerRockCardPrefab, prefabs.PlayerPaperCardPrefab, prefabs.PlayerScissorsCardPrefab, prefabs.CardBackground, HandLayout.RightToLeft);
			this.prefabs.OpponentCardController.GenerateCards(player.PlayerHand, prefabs.OpponentRockCardPrefab, prefabs.OpponentPaperCardPrefab, prefabs.OpponentScissorsCardPrefab, prefabs.CardBackground, HandLayout.LeftToRight);
			AiManagerController.Instance.aiCards = this.prefabs.OpponentCardController.Cards.Select(i => i.gameObject).ToList();

			this.prefabs.PlayerStarController.GenerateStars(player, prefabs.StarPrefab);
			this.prefabs.OpponentStarController.GenerateStars(opponent, prefabs.StarPrefab);

			this.prefabs.PlayerCardController.CardSelected += OnPlayerCardSelected;
			this.prefabs.OpponentCardController.CardSelected += OnOpponentCardSelected;
			this.prefabs.CounterFrame.SetActive(true);
		}

		private void OnTimerElapsed(object sender, coundownTimerController e) {
			dragCardController.Instance.IsActive = false;
			this.ActiveRound.Round.PlayCard(this.player.Id, CardType.None);

			if(this.Match is LocalMatchController)
				AiManagerController.Instance.SelectRandom();

			this.PlayerSelectedCard = null;
		}

		private void OnOpponentCardSelected(object sender, CardInfo e) {
			this.ActiveRound.Round.PlayCard(this.opponent.Id, e.Type);
			this.OpponentSelectedCard = e;
		}

		private void OnPlayerCardSelected(object sender, CardInfo e) {
			dragCardController.Instance.IsActive = false;
			this.ActiveRound.Round.PlayCard(this.player.Id, e.Type);
			AiManagerController.Instance.SelectRandom();
			this.PlayerSelectedCard = e;
		}

		private void OnMatchEnded(object sender, (IMatchController Match, PlayerState Winner) e) {
			this.Log(e.Match.Options.Name + ": Match Ended.");
			this.prefabs.CounterFrame.SetActive(false);
			this.prefabs.InfoText.gameObject.SetActive(false);

			this.prefabs.MatchResultFrame.SetActive(true);
			if(e.Winner == null || e.Winner.Id.GUID != localUser.GUID) {
				this.prefabs.VictoryFrame.SetActive(false);
				this.prefabs.DefeatFrame.SetActive(true);
			} else {
				this.prefabs.VictoryFrame.SetActive(true);
				this.prefabs.DefeatFrame.SetActive(false);
			}
		}

		private void OnUserJoined(object sender, (IMatchController Match, PlayerState User) e) {
			this.Log(e.Match.Options.Name + ": User " + e.User.Id.Name + " joined.");
		}

		private void OnUserLeft(object sender, (IMatchController Match, PlayerState User) e) {
			this.Log(e.Match.Options.Name + ": User " + e.User.Id.Name + " left.");
		}

		private void OnRoundCreated(object sender, (IMatchController Match, IRoundController Round) e) {
			this.prefabs.InfoText.gameObject.SetActive(false);
			this.prefabs.PlayerStarController.GenerateStars(player, prefabs.StarPrefab);
			this.prefabs.OpponentStarController.GenerateStars(opponent, prefabs.StarPrefab);

			if (this.PlayerSelectedCard != null) {
				GameObject.Destroy(this.PlayerSelectedCard.gameObject);
				this.PlayerSelectedCard = null;
			}

			if (this.OpponentSelectedCard != null) {
				GameObject.Destroy(this.OpponentSelectedCard.gameObject);
				this.OpponentSelectedCard = null;
			}

			this.ActiveRound = new RoundManager(e.Round);
			this.Log(e.Match.Options.Name + ": New Round");


			if (localUser == null)
				return;

			dragCardController.Instance.IsActive = true;

			if (e.Match.Options.TimerLength > 0.1f)
				coundownTimerController.Instance.RunTimer(e.Match.Options.TimerLength);

			this.ActiveRound.Round.PlayerWinRoundEvent += (_, ee) => {
				this.Match.State.PlayerList.First(i => i.Id.GUID == ee.User.GUID).Stars += 1;
				if (ee.User.GUID == this.localUser.GUID) {
					this.prefabs.InfoText.text = "You win";
					this.prefabs.InfoText.gameObject.SetActive(true);
				}
			};

			this.ActiveRound.Round.PlayerLoseRoundEvent += (_, ee) => {
				this.Match.State.PlayerList.First(i => i.Id.GUID == ee.User.GUID).Stars -= 1;
				if (ee.User.GUID == this.localUser.GUID) {
					this.prefabs.InfoText.text = "You lose";
					this.prefabs.InfoText.gameObject.SetActive(true);
				}
			};

			this.ActiveRound.Round.RoundDrawEvent += (_, ee) => {
				this.prefabs.InfoText.text = "Draw";
				this.prefabs.InfoText.gameObject.SetActive(true);
			};
		}

		private void Log(string message) {
			this.DisplayMessageEvent?.Invoke(this, message);
		}
	}
}
