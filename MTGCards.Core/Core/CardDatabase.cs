using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Xml.Linq;
using System.Runtime.Caching;

namespace MTGCards.Core {
	public class CardDatabase {
		public List<Card> cards;
		public List<Set> sets;

		private bool isLoaded = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="CardDatabase"/> class.
		/// Gets the card and set databases from the cache if it exists, or
		/// loads them from the XML Database File and stores them in cache if
		/// it doesn't.
		/// </summary>
		/// <param name="filename">The filename of the XML Card Database.</param>
		public CardDatabase(string filename) {
			cards = new List<Card>();
			isLoaded = false;

			ObjectCache cache = MemoryCache.Default;

			if(cache["CardDatabase"] == null) {
				cache["CardDatabase"] = LoadCardsFromDatabase(filename);
			}

			cards = (List<Card>)cache["CardDatabase"];

			if(cache["SetDatabase"] == null) {
				cache["SetDatabase"] = LoadSetsFromDatabase(filename);
			}

			sets = (List<Set>)cache["SetDatabase"];

			isLoaded = true;
		}

		/// <summary>
		/// Loads the sets from the database.
		/// </summary>
		/// <param name="filename">The filename of the XML Card Database.</param>
		/// <returns>A List of all the sets defined in the database.</returns>
		private List<Set> LoadSetsFromDatabase(string filename) {
			var _sets = new List<Set>();

			var xml = XDocument.Load(filename);

			// Load the sets
			var xmlSetList = from s in xml.Descendants("sets").Elements("set")
							 orderby (string)s.Element("longname")
							 select new {
								 Name = (string)s.Element("name"),
								 LongName = (string)s.Element("longname")
							 };

			foreach(var set in xmlSetList) {
				var setEntry = new Set {
					Name = set.Name,
					LongName = set.LongName
				};

				_sets.Add(setEntry);
			}

			return _sets;
		}

		/// <summary>
		/// Loads the cards from the database.
		/// </summary>
		/// <param name="filename">The filename of the XML Card Database.</param>
		/// <returns>A List of every card in the database</returns>
		private List<Card> LoadCardsFromDatabase(string filename) {
			var _cards = new List<Card>();

			var xml = XDocument.Load(filename);

			// Load the cards
			var xmlCardList = from c in xml.Descendants("cards").Elements("card")
							  orderby (string)c.Element("name")
							  select new {
								  Name = (string)c.Element("name"),
								  Set = (string)c.Element("set"),
								  ImageUri = (string)c.Element("set").Attribute("picURL"),
								  Color = (string)c.Element("color"),
								  ManaCost = (string)c.Element("manacost"),
								  Type = (string)c.Element("type"),
								  PT = (string)c.Element("pt"),
								  Text = (string)c.Element("text")
							  };

			foreach(var card in xmlCardList) {
				int power = 0;
				int toughness = 0;

				if(card.PT != null) {
					string[] ptParts = card.PT.Split('/');

					try {
						power = int.Parse(ptParts[0]);
						toughness = int.Parse(ptParts[1]);
					} catch(System.FormatException) {
						power = 0;
						toughness = 0;
					}
				}

				var cardEntry = new Card {
					Name = card.Name,
					Set = card.Set,
					ImageUri = card.ImageUri,
					Color = card.Color,
					ManaCost = card.ManaCost,
					Type = card.Type,
					Power = power,
					Toughness = toughness,
					Text = card.Text
				};

				_cards.Add(cardEntry);
			}

			return _cards;
		}

		/// <summary>
		/// Gets the card with the specified name.
		/// </summary>
		/// <param name="Name">The name.</param>
		/// <returns>The card with the specified name on success, null on failure.</returns>
		public Card GetCardByName(string Name) {
			if(!isLoaded) return null;

			Card[] card = (from c in cards
						   where c.Name.ToLower() == Name.ToLower()
						   select c).ToArray();

			if(card.Length > 0) {
				return card[0];
			} else {
				return null;
			}
		}
	}
}