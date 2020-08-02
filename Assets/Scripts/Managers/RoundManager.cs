using Roshambo.Common.Controllers.Rounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyCSharp.Assets.Scripts.Managers
{
    public class RoundManager
	{
		public IRoundController Round { get; private set; }

        public RoundManager(IRoundController round) {
			this.Round = round;
		}
    }
}
