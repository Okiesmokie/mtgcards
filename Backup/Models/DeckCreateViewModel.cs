using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MTGCards.Core;

namespace MTGCards.Models {
	public class DeckCreateViewModel {
		public List<Set> sets;

		public List<Core.Card> deckCards;
		public string DeckName;
		public int DeckId;
	}
}