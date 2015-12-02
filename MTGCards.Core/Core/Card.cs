using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MTGCards.Core {
	public class Card {
		public string Name { get; set; }
		public string ImageUri { get; set; }
		public string Set { get; set; }
		public string Color { get; set; }
		public string ManaCost { get; set; }
		public string Type { get; set; }
		public int Power { get; set; }
		public int Toughness { get; set; }
		public string Text { get; set; }

		public int Count { get; set; }
		public string PT { get; set; }
	}
}