using Roshambo.Common.Controllers.Matches;
using Roshambo.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchListEntry : MonoBehaviour
{
	public EventHandler<MatchListEntry> OnJoinClicked { get; set; }
	public IMatchController Match { get; set; }

	public Text MatchName;
	public Button JoinButton;

    // Start is called before the first frame update
    void Start()
    {
		JoinButton.onClick.AddListener(OnJoinClickEvent);
    }

	private void OnJoinClickEvent() {
		this.OnJoinClicked?.Invoke(this, this);
	}

	public void SetMatch(IMatchController match) {
		this.Match = match;
		MatchName.text = match.Options.Name;
	}
}
