using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyCSharp.Assets.Scripts.Models
{
    public class GlobalInventoryModel : InventoryModel
    {
		public List<InventoryModel> Inventories { get; private set; }
		 = new List<InventoryModel>();

		public int TotalScissorsCount {
			get {
				return Inventories.Sum(i => i.ScissorsCount) + ScissorsCount;
			}
		}

		public int TotalPaperCount {
			get {
				return Inventories.Sum(i => i.PaperCount) + PaperCount;
			}
		}

		public int TotalRockCount {
			get {
				return Inventories.Sum(i => i.RockCount) + RockCount;
			}
		}

		public InventoryModel CreateNewPlayerHand() {
			var inventory = new InventoryModel();
			inventory.PaperCount = 3;
			inventory.ScissorsCount = 3;
			inventory.RockCount = 3;
			Inventories.Add(inventory);
			return inventory;
		}
	}
}
