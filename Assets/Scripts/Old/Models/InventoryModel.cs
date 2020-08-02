using AssemblyCSharp.Assets.Scripts.Models;
using Roshambo.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class InventoryModel
{
	public int ScissorsCount { get; set; } = 0;
	public int PaperCount { get; set; } = 0;
	public int RockCount { get; set; } = 0;

	public bool HasCards => Total != 0;
	public int Total => ScissorsCount + PaperCount + RockCount;
	public List<CardType> AsCardTypes => (new List<CardType>[] {
		Enumerable.Repeat(CardType.Scissors, ScissorsCount).ToList(),
		Enumerable.Repeat(CardType.Paper, PaperCount).ToList(),
		Enumerable.Repeat(CardType.Rock, RockCount).ToList(),
		}).SelectMany(i => i).ToList();
	public CardType PickRandom(Random random) {
		if (HasCards == false)
			throw new Exception("Cannot pick from empty hand");

		var cards = AsCardTypes;
		var card = cards.Skip(random.Next(cards.Count - 1)).First();
		switch(card) {
			case CardType.Paper: PaperCount--; break;
			case CardType.Rock: RockCount--; break;
			case CardType.Scissors: ScissorsCount--; break;
		}

		return card;
	}
}
