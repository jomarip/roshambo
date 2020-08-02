using AssemblyCSharp.Assets.Scripts.Models;
using Roshambo.Common.Controllers.Matches;
using Roshambo.Common.Controllers.Matches.Net;
using Roshambo.Common.Controllers.Rooms;
using Roshambo.Common.Controllers.Rooms.Net;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AssemblyCSharp.Assets.Scripts.Managers
{
    public class RoomManager
    {
		public EventHandler<string> DisplayMessageEvent { get; set; }
		public IRoomController Controller { get; private set; }
		public MatchManager ActiveMatch { get; private set; }
		public List<MatchManager> MatchList { get; private set; }

		private List<UserState> aiList;
		private UserState currentUser;
		private UserId currentUserId;
		private GamePrefabs prefabs;

		private MatchStyle style;
		private GameType type;

		private string tempJoinName;
		private int aiPlayers;
		private int rounds;

		public RoomManager(IRoomController controller, UserId currentUserId, GamePrefabs prefabs) {
			this.prefabs = prefabs;
			this.currentUserId = currentUserId;
			this.Controller = controller;
			this.MatchList = new List<MatchManager>();

			//this.prefabs.StartMatchButton.onClick.AddListener(OnStartMatchClicked);
			Controller.MatchCreatedEvent += OnMatchCreated;
			Controller.RoomStartedEvent += OnRoomStarted;
			Controller.RoomEndedEvent += OnRoomEnded;
			Controller.UserJoinedEvent += OnUserJoined;
			Controller.UserLeftEvent += OnUserLeft;
			this.type = GameType.Single;

			this.prefabs.SetupMatchButton.onClick.AddListener(OnSetupMatchClicked);
			this.prefabs.ReturnToListButton.onClick.AddListener(OnReturnToListClicked);
			this.prefabs.CreateMatchButton.onClick.AddListener(OnCreateMatchClicked);

			//TODO: Move AI management
			if (controller is LocalRoomController) {
				this.aiPlayers = 5;

				this.aiList = new List<UserState>();
				if (this.Controller is LocalRoomController Local) {
					this.currentUser = Local.AddPlayer(this.currentUserId);
					for (int i = 0; i < aiPlayers; i++) {
						var user = new UserId();
						aiList.Add(Local.AddPlayer(new UserId()));
					}
				}

				for (int i = 0; i < 3; i++) {
					Controller.CreateMatch(new MatchOptions() {
						Name = "Match " + i.ToString(),
						Style = MatchStyle.Rounds,
						Rounds = 3,
						TimerLength = 10f,
					});
				}
			}

			var existing = this.Controller.GetMatches();
			foreach(var match in existing) {
				this.OnMatchCreated(this, (this.Controller, match));
			}
		}

		private void OnCreateMatchClicked() {
			MatchOptions options = new MatchOptions();
			options.Name = this.prefabs.MatchName.text;
			options.Rounds = string.IsNullOrWhiteSpace(this.prefabs.RoundCount.text) == false ? int.Parse(this.prefabs.RoundCount.text) : 5;
			options.Style = (this.prefabs.AllCardsToggle.isOn) ? MatchStyle.AllCards : MatchStyle.Rounds;
			options.TimerLength = string.IsNullOrWhiteSpace(this.prefabs.TimerLength.text) == false ? float.Parse(this.prefabs.TimerLength.text) : 10f;

			if(string.IsNullOrWhiteSpace(options.Name)) {
				options.Name = "Match";
			}

			this.tempJoinName = options.Name;
			Controller.CreateMatch(options);
		}

		private void OnReturnToListClicked() {
			this.prefabs.RoomLobbyFrame.SetActive(true);
			this.prefabs.MatchSetupFrame.SetActive(false);
		}

		private void OnSetupMatchClicked() {
			this.prefabs.RoomLobbyFrame.SetActive(false);
			this.prefabs.MatchSetupFrame.SetActive(true);
		}

		public void Start() {
			this.Controller.StartRoom();
		}

		public void UserCreateMatch(MatchOptions options) {
			this.tempJoinName = options.Name;
			Controller.CreateMatch(options);
		}

		public void Update() {
			if (Controller != null)
				Controller.UpdateMatches();
		}

		#region UI Callbacks
		private void OnStartMatchClicked() {
			//TODO: when match setup screen is done
		}
		#endregion

		#region Controller Event Callbacks
		private void OnRoomStarted(object sender, IRoomController e) {
			this.DisplayMessage(string.Format("Started room: {0}", e.Name));
		}

		private void OnRoomEnded(object sender, IRoomController e) {
			this.DisplayMessage(string.Format("Ended room: '{0}'", e.Name));
		}

		private void OnUserJoined(object sender, (IRoomController Room, UserState User) e) {
			this.DisplayMessage(string.Format("'{0}' joined room '{1}'", e.User.Id.GUID.ToString(), e.Room.Name));
		}

		private void OnUserLeft(object sender, (IRoomController Room, UserState User) e) {
			this.DisplayMessage(string.Format("'{0}' left room '{1}'", e.User.Id.GUID.ToString(), e.Room.Name));
		}
		#endregion

		private void OnMatchCreated(object sender, (IRoomController Room, IMatchController Match) e) {
			this.DisplayMessage(string.Format("Created match '{0}' in room '{1}'", e.Match.Options.Name, e.Room.Name));
			var manager = new MatchManager(e.Match, this.currentUserId, this.prefabs);
			manager.DisplayMessageEvent += (__, msg) => this.DisplayMessage(msg);
			manager.Match.MatchEndedEvent += (__, winner) => {

			};

			MatchList.Add(manager);

			e.Match.UserJoinedEvent += (_, args) => {
				if(args.User.Id.GUID == this.currentUserId.GUID) {
					if(args.Match.State.PlayerList.Count == 1) {
						this.ActiveMatch = manager;

						var c = (LocalMatchController)this.ActiveMatch.Match;
						c.AddPlayer(aiList.First());
						c.StartMatch();
					}
				}
			};

			if (e.Match.Options.Name == tempJoinName) {
				tempJoinName = null;
				var c = (LocalMatchController)manager.Match;
				c.AddPlayer(this.currentUser);
			}

			this.UpdateMatchList();
		}

		private void DisplayMessage(string message) {
			this.DisplayMessageEvent?.Invoke(this, message);
		}

		private void UpdateMatchList() {
			var content = prefabs.MatchListContent;
			var children = Enumerable.Range(0, content.transform.childCount).Select(i => content.transform.GetChild(i));
			foreach (var child in children)
				GameObject.Destroy(child.gameObject);

			int ct = 0;
			foreach(var match in MatchList) {
				var entry = GameObject.Instantiate(prefabs.MatchListEntryItem);
				var info = entry.GetComponent<MatchListEntry>();
				info.SetMatch(match.Match);

				entry.transform.SetParent(content.transform);
				entry.transform.localPosition = new Vector3(0f, -1f * entry.transform.GetComponent<RectTransform>().rect.height * (ct++), 0f);
				entry.transform.localScale = new Vector3(1f, 1f, 1f);

				info.OnJoinClicked += OnUserJoinMatchClicked;
			}
		}

		private void OnUserJoinMatchClicked(object sender, MatchListEntry e) {
			if(e.Match is LocalMatchController local) {
				local.AddPlayer(currentUser);
			}

			if (e.Match is NetClientMatchController remote) {
				var c = this.Controller as NetClientRoomController;
				c.JoinMatch(e.Match.Id);
			}

			this.prefabs.MatchListFrame.SetActive(false);
		}
	}
}
