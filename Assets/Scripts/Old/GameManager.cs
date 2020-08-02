using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AssemblyCSharp.Assets.Scripts.Controllers;
using AssemblyCSharp.Assets.Scripts.Models;
using Roshambo.Common.Controllers.Rooms;
using System;
using Roshambo.Common.Models;
using static AssemblyCSharp.Assets.Scripts.Controllers.OldMatchController;
using Roshambo.Common.Controllers.Matches;
using System.Linq;
using AssemblyCSharp.Assets.Scripts.Managers;
using Roshambo.Common.Controllers.Rooms.Net;
using Roshambo.Common.Controllers.RoomList.Net;
using Roshambo.Common.Controllers.RoomList;

public enum GameChoices
{
    NONE,
    ROCK,
    PAPER,
    SCISSORS
}

public enum GameType
{
	Single,
	Multi
}

public class GameManager : MonoBehaviour
{
	public GameObject MessagePrefab;
    public Text infoText;

	public GameObject CounterFrame;
	public Text RockCounter;
	public Text PaperCounter;
	public Text ScissorsCounter;

	public GameObject PlayerRock;
	public GameObject PlayerPaper;
	public GameObject PlayerScissors;
	public GameObject PlayerCardRoot;
	public GameObject PlayerStarRoot;

	public GameObject OpponentRock;
	public GameObject OpponentPaper;
	public GameObject OpponentScissors;
	public GameObject OpponentCardRoot;
	public GameObject OpponentStarRoot;

	public GameObject RoomLobbyFrame;
	public GameObject MatchListEntryItem;
	public GameObject MatchListFrame;
	public GameObject MatchListContent;
	public Button SetupMatchButton;
	public Button CreateMatchButton;
	public Button ReturnToListButton;
	public GameObject Star;
	public GameObject CardBackground;
	public Button ReturnHomeButton;
	public Button RematchButton;
	public GameObject VictoryFrame;
	public GameObject DefeatFrame;
	public GameObject MatchResultFrame;
	public Button MultiPlayerButton;
	public Button SinglePlayerButton;
	public Button StartMatchButton;
	public GameObject SelectGameTypeFrame;
	public GameObject MatchSetupFrame;
	public Text UsernameText;
	public Text PasswordText;
	public Button LoginButton;
	public GameObject LoginFrame;
	public CardController PlayerCardController;
	public CardController OpponentCardController;
	public StarController PlayerStarController;
	public StarController OpponentStarController;
	public Toggle AllCardsToggle;
	public Toggle RoundsToggle;
	public Text RoundCount;
	public Text TimerLength;
	public Text MatchName;
	private GamePrefabs prefabs;
	static GameManager instance;
	private UserId currentUserId;
	private string username;

	public static GameManager Instance {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }

	private NetClientRoomListController roomlist;

	public RoomManager Manager { get; private set; }


	// Start is called before the first frame update
	private void Start()
    {
		this.prefabs = new GamePrefabs() {
			InfoText = infoText,
			OpponentCardRootObject = OpponentCardRoot,
			OpponentPaperCardPrefab = OpponentPaper,
			OpponentRockCardPrefab = OpponentRock,
			OpponentScissorsCardPrefab = OpponentScissors,
			PlayerCardRootObject = PlayerCardRoot,
			PlayerPaperCardPrefab = PlayerPaper,
			PlayerRockCardPrefab = PlayerRock,
			PlayerScissorsCardPrefab = PlayerScissors,
			CardBackground = CardBackground,
			RockCounter = RockCounter,
			PaperCounter = PaperCounter,
			ScissorsCounter = ScissorsCounter,
			StarPrefab = Star,
			OpponentStarRootObject = OpponentStarRoot,
			PlayerStarRootObject = PlayerStarRoot,
			DefeatFrame = DefeatFrame,
			RematchButton = RematchButton,
			ReturnHomeButton = ReturnHomeButton,
			VictoryFrame = VictoryFrame,
			MatchResultFrame = MatchResultFrame,
			MatchSetupFrame = MatchSetupFrame,
			MultiPlayerButton = MultiPlayerButton,
			SinglePlayerButton = SinglePlayerButton,
			SelectGameTypeFrame = SelectGameTypeFrame,
			StartMatchButton = StartMatchButton,
			CounterFrame = CounterFrame,
			LoginButton = LoginButton,
			LoginFrame = LoginFrame,
			PasswordText = PasswordText,
			UsernameText = UsernameText,
			OpponentCardController = OpponentCardController,
			OpponentStarController = OpponentStarController,
			PlayerCardController = PlayerCardController,
			PlayerStarController = PlayerStarController,
			MatchListFrame = MatchListFrame,
			MatchListEntryItem = MatchListEntryItem,
			RoomLobbyFrame = RoomLobbyFrame,
			MatchListContent = MatchListContent,
			SetupMatchButton = SetupMatchButton,
			ReturnToListButton = ReturnToListButton,
			CreateMatchButton = CreateMatchButton,
			AllCardsToggle = AllCardsToggle,
			MessagePrefab = MessagePrefab,
			RoundCount = RoundCount,
			RoundsToggle = RoundsToggle,
			TimerLength	 = TimerLength,
			MatchName = MatchName,
		};

		this.prefabs.ReturnHomeButton.onClick.AddListener(OnReturnHomeClicked);
		this.prefabs.RematchButton.onClick.AddListener(OnRematchClicked);
		this.prefabs.SinglePlayerButton.onClick.AddListener(OnSinglePlayerClicked);
		this.prefabs.MultiPlayerButton.onClick.AddListener(OnMultiPlayerClicked);
		this.prefabs.LoginButton.onClick.AddListener(OnLoginClicked);

		this.prefabs.MatchListFrame.SetActive(false);
		this.prefabs.MatchResultFrame.SetActive(false);
		this.prefabs.MatchSetupFrame.SetActive(false);
		this.prefabs.SelectGameTypeFrame.SetActive(false);
		this.prefabs.CounterFrame.SetActive(false);
		this.prefabs.LoginFrame.SetActive(true);
	}

	private void OnLoginClicked() {
        Debug.Log("Username" + this.prefabs.UsernameText.text);
		this.username = this.prefabs.UsernameText.text;
		this.currentUserId = new UserId();
		this.currentUserId.GUID = Guid.NewGuid();
		this.currentUserId.Name = username;

		this.prefabs.LoginFrame.SetActive(false);
		this.prefabs.SelectGameTypeFrame.SetActive(true);
		this.DisplayMessage("Username set to: " + this.username);
	}

	private void OnMultiPlayerClicked() {
		this.joined = false;
		this.prefabs.SelectGameTypeFrame.SetActive(false);

		this.roomlist = new NetClientRoomListController(currentUserId, "localhost", 12345);
		this.roomlist.RoomListUpdatedEvent += OnRoomListUpdated;
		this.roomlist.LocalUserJoinRoomEvent += OnJoinedRoom;
		this.roomlist.Client.Router.PacketReceivedEvent += (s, p) => Debug.Log("recv: " + p.GetType().Name + " " + p.ToString());
		this.roomlist.Client.Router.PacketSentEvent += (s, p) => Debug.Log("send: " + p.GetType().Name + " " + p.ToString());
		this.roomlist.Connect();
	}

	private void OnJoinedRoom(object sender, IRoomController e) {
		this.prefabs.RoomLobbyFrame.SetActive(true);
		this.Manager = new RoomManager(e, this.currentUserId, this.prefabs);
		this.Manager.DisplayMessageEvent += (_, msg) => this.DisplayMessage(msg);
		this.Manager.Start();
		joined = true;
	}

    private bool joined = false;
    private void OnRoomListUpdated(object sender, IRoomListController e)
    {
        if (e.RoomList.Count > 0)
        {
            var cl = e as NetClientRoomListController;
            var room = e.RoomList.First();
            cl.Join(room.State.Id);
        }
    }

    private void OnSinglePlayerClicked()
    {
        this.prefabs.SelectGameTypeFrame.SetActive(false);
        this.prefabs.RoomLobbyFrame.SetActive(true);

        this.Manager = new RoomManager(new LocalRoomController(new RoomId()
        {
            GUID = Guid.NewGuid(),
            Name = "Single Player Room",
        }), this.currentUserId, this.prefabs);
        this.Manager.DisplayMessageEvent += (_, msg) => this.DisplayMessage(msg);
        this.Manager.Start();
    }

    private void OnRematchClicked()
    {
        this.prefabs.MatchResultFrame.SetActive(false);
        this.prefabs.RoomLobbyFrame.SetActive(true);
    }

    private void OnReturnHomeClicked()
    {
        this.prefabs.MatchResultFrame.SetActive(false);
        this.prefabs.SelectGameTypeFrame.SetActive(true);
    }

    private void Update()
    {
        if (this.Manager != null)
            this.Manager.Update();

        if (this.roomlist != null)
            this.roomlist.Update();
    }

    private void DisplayMessage(string message)
    {
        Debug.Log(message);
    }

    #region yeeet
    //   public void CheckStatus()
    //   {
    //       returnOriginalPosController[] Object1 = FindObjectsOfType<returnOriginalPosController>();
    //       for (int i = 0; i < Object1.Length; i++)
    //       {
    //           if (Object1[i].isOnPlatform)
    //           {
    //               obj1 = Object1[i];

    //               if (Object1[i].word == "r")
    //               {
    //                   player_Choice = GameChoices.ROCK;
    //               }
    //               if (Object1[i].word == "p")
    //               {
    //                   player_Choice = GameChoices.PAPER;
    //               }
    //               if (Object1[i].word == "s")
    //               {
    //                   player_Choice = GameChoices.SCISSORS;
    //               }

    //               break;
    //           }
    //       }

    //       aiReturnOriginalPosController[] Object2 = FindObjectsOfType<aiReturnOriginalPosController>();
    //       for (int i = 0; i < Object2.Length; i++)
    //       {
    //           if (Object2[i].isOnPlatform)
    //           {
    //               obj2 = Object2[i];

    //               if (Object2[i].word == "r")
    //               {
    //                   opponent_Choice = GameChoices.ROCK;
    //               }
    //               if (Object2[i].word == "p")
    //               {
    //                   opponent_Choice = GameChoices.PAPER;
    //               }
    //               if (Object2[i].word == "s")
    //               {
    //                   opponent_Choice = GameChoices.SCISSORS;
    //               }

    //               break;
    //           }
    //       }

    //       DetermineWinner();
    //   }

    //   void DetermineWinner()
    //   {

    //       if (player_Choice == opponent_Choice)
    //       {
    //           // draw

    //           infoText.text = "It's a Draw!";
    //           StartCoroutine(DisplayWinnerAndRestart());

    //           return;

    //       }

    //       if (player_Choice == GameChoices.PAPER && opponent_Choice == GameChoices.ROCK)
    //       {
    //           // player won

    //           infoText.text = "You Win!";
    //           StartCoroutine(DisplayWinnerAndRestart());
    //           lose = false;
    //           win = true;
    //           if (counter < 3)
    //               counter++;
    //           starActivator();

    //           return;
    //       }

    //       if (opponent_Choice == GameChoices.PAPER && player_Choice == GameChoices.ROCK)
    //       {
    //           // opponent won

    //           infoText.text = "You Lose!";
    //           StartCoroutine(DisplayWinnerAndRestart());
    //           lose = true;
    //           win = false;
    //           if (counter > 0)
    //               counter--;
    //           starActivator();

    //           return;
    //       }

    //       if (player_Choice == GameChoices.ROCK && opponent_Choice == GameChoices.SCISSORS)
    //       {
    //           // player won
    //           lose = false;
    //           win = true;
    //           infoText.text = "You Win!";
    //           StartCoroutine(DisplayWinnerAndRestart());
    //           if (counter < 3)
    //               counter++;
    //           starActivator();

    //           return;
    //       }

    //       if (opponent_Choice == GameChoices.ROCK && player_Choice == GameChoices.SCISSORS)
    //       {
    //           // opponent won
    //           lose = true;
    //           win = false;
    //           infoText.text = "You Lose! ";
    //           StartCoroutine(DisplayWinnerAndRestart());
    //           if (counter > 0)
    //               counter--;
    //           starActivator();

    //           return;
    //       }

    //       if (player_Choice == GameChoices.SCISSORS && opponent_Choice == GameChoices.PAPER)
    //       {
    //           // player won
    //           lose = false;
    //           win = true;
    //           infoText.text = "You Win!";
    //           StartCoroutine(DisplayWinnerAndRestart());
    //           if (counter < 3)
    //               counter++;
    //           starActivator();

    //           return;
    //       }

    //       if (opponent_Choice == GameChoices.SCISSORS && player_Choice == GameChoices.PAPER)
    //       {
    //           // opponent won
    //           lose = true;
    //           win = false;
    //           infoText.text = "You Lose!";

    //           StartCoroutine(DisplayWinnerAndRestart());

    //           if (counter > 0)
    //               counter--;
    //           starActivator();

    //           return;
    //       }

    //   }

    //   public void TimerLoss()
    //   {

    //       lose = true;
    //       win = false;
    //       infoText.text = "You Lose!";
    //       if (counter > 0)
    //           counter--;
    //       starActivator();

    //       StartCoroutine(DisplayWinnerAndRestart());
    //   }

    //   IEnumerator DisplayWinnerAndRestart()
    //   {

    //       infoText.gameObject.SetActive(true);

    //       yield return new WaitForSeconds(2f);

    //       infoText.gameObject.SetActive(false);

    //       if (obj1 != null)
    //       {
    //           obj1.destroyIT();
    //       }
    //       if (obj2 != null)
    //       {
    //           obj2.destroyIT();
    //       }

    //       coundownTimerController.Instance.RunTimer();

    //   }

    //   void starActivator()
    //   {
    //       if (counter == 2 && win == true)
    //       {
    //           Debug.Log("1");
    //           playerStars[1].SetActive(true);
    //           oponentStars[1].SetActive(false);
    //           win = false;
    //       }
    //       if (counter == 3 && win == true)
    //       {
    //           Debug.Log("2");
    //           playerStars[2].SetActive(true);
    //           oponentStars[0].SetActive(false);
    //           win = false;
    //       }

    //       if (counter == 2 && lose == true)
    //       {
    //           Debug.Log("22");
    //           playerStars[2].SetActive(false);
    //           oponentStars[0].SetActive(true);
    //           lose = false;
    //       }
    //       if (counter == 1 && lose == true)
    //       {
    //           Debug.Log("33");
    //           playerStars[1].SetActive(false);
    //           oponentStars[1].SetActive(true);
    //           lose = false;
    //       }
    //       if (counter == 0 && lose == true)
    //       {
    //           Debug.Log("44");
    //           playerStars[0].SetActive(false);
    //           oponentStars[2].SetActive(true);
    //           infoText.text = "Game over!";
    //           lose = false;
    //           coundownTimerController.Instance.flag = 1;
    //           SceneManager.LoadScene("GameOver");
    //       }

    //   }
    #endregion yeet
}
