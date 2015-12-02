using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MTGCards.Models {
	public class CardTypeHelper {
		public string Type;
		public List<Core.Card> CardList;
		public int Count;
	}

	public class DeckStatisticsViewModel {
		public CardTypeHelper[] CardTypes;

		public System.Data.Entity.Design.PluralizationServices.PluralizationService Pluralize;

		public void PopulateDeck(string[] cardNames, string dbFilename) {
			Core.CardDatabase db = new Core.CardDatabase(dbFilename);

			CardTypes = new CardTypeHelper[] {
				new CardTypeHelper { Type = "Creature", CardList = new List<Core.Card>(), Count = 0 },
				new CardTypeHelper { Type = "Land", CardList = new List<Core.Card>(), Count = 0 },
				new CardTypeHelper { Type = "Planeswalker", CardList = new List<Core.Card>(), Count = 0 },
				new CardTypeHelper { Type = "Enchantment", CardList = new List<Core.Card>(), Count = 0 },
				new CardTypeHelper { Type = "Artifact", CardList = new List<Core.Card>(), Count = 0 },
				new CardTypeHelper { Type = "Sorcery", CardList = new List<Core.Card>(), Count = 0 },
				new CardTypeHelper { Type = "Instant", CardList = new List<Core.Card>(), Count = 0 }
			};

			var uniqueCards = from c in cardNames
							  group c by c into cardGroup
							  select new {
								  Name = cardGroup.Key,
								  Count = cardGroup.Count()
							  };

			foreach(var card in uniqueCards) {
				Core.Card cardToAdd = db.GetCardByName(card.Name);

				if(cardToAdd != null) {
					cardToAdd.Count = card.Count;

					foreach(var cardType in CardTypes) {
						if(cardToAdd.Type.Contains(cardType.Type)) {
							cardType.Count += cardToAdd.Count;
							cardType.CardList.Add(cardToAdd);
						}
					}
				}
			}

			return;
		}

		public void GenerateStatistics() {
			Pluralize = System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(System.Globalization.CultureInfo.CurrentCulture);
		}
	}
}