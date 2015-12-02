using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using MTGCards.Core;
using MTGCards.Core.Menus;

namespace MTGCards.Controllers {
	public class DeckController : MenuControllerBase {
		private string cardDbPath = "~/Content/xml/cards.xml";	// The path to the XML Card Database on the server

		protected override MenuItem[] CreateMenu() {
			return new MenuItem[] {
				new MenuItem { Uri = Url.Action("Index", "Home"), Text = "Home" },
				new MenuItem { Uri = Url.Action("Index", "Account"), Text = "Account" },
				new MenuItem { Uri = Url.Action("Index", "Deck"), Text = "Deck" },
				new MenuItem { Uri = Url.Action("Create", "Deck", new { id = -1 }), Text = "Create", SubItem = true },
				new MenuItem { Uri = Url.Action("Load", "Deck"), Text = "Load", SubItem = true },
				new MenuItem { Uri = Url.Action("List", "Deck"), Text = "List", SubItem = true }
				//new MenuItem { Uri = Url.Action("CheckLegality", "Deck"), Text = "Check Legality", SubItem = true }
			};
		}

		/// <summary>
		/// Get /Deck/
		/// </summary>
		/// <returns>The view.</returns>
		public ActionResult Index() {
			return View();
		}

		/// <summary>
		/// GET /Deck/Create/[id]
		/// The deck editor
		/// </summary>
		/// <param name="id">The id of the deck to edit, or -1 to create a new deck.</param>
		/// <returns>The view.</returns>
		public ActionResult Create(int id = -1) {
			if(!Models.AccountUtils.isLoggedIn(this)) return RedirectToAction("LogIn", "Account");

			CardDatabase db = new CardDatabase(Server.MapPath(cardDbPath));

			var model = new Models.DeckCreateViewModel();

			List<string> deckCardList = new List<string>();

			if(id >= 0) {
				var accountDB = new Models.AccountsDataContext();
				
				var loginHash = Request.Cookies["loginHash"].Value;
				var accountId = (from a in accountDB.Accounts
								 where a.LoginHash == loginHash
								 select a.Id).ToArray();

				if(accountId.Length <= 0) return RedirectToAction("LogOut", "Account");

				// Check if this account is the owner of the deck
				var deckDB = new Models.DecksDataContext();
				var deck = (from d in deckDB.Decks
							where (d.Id == id) &&
								  (d.OwnerId == accountId[0])
							select d).ToArray();

				if(deck.Length <= 0) {
					// This account does not own the deck
					return RedirectToAction("Create", "Deck", new { id = -1 });
				}

				model.DeckName = deck[0].Name;

				var cardDB = new Models.CardsDataContext();
				var cards = from c in cardDB.Cards
							where c.DeckId == id
							select c.CardName;

				foreach(var card in cards) {
					deckCardList.Add(card);
				}
			}

			model.deckCards = new List<Core.Card>();
			model.deckCards = (from c in deckCardList
							   orderby c
							   select db.GetCardByName(c)
							  ).ToList();

			model.sets = db.sets;
			model.DeckId = id;

			return View(model);
		}

		/// <summary>
		/// Checks if the card meets certain filter conditions.
		/// </summary>
		/// <param name="card">The card to check the conditions of.</param>
		/// <param name="setFilter">A comma-separated list of sets to include.</param>
		/// <param name="colorFilter">A comma-separated list of colors to include</param>
		/// <returns>true if the conditions are met, false otherwise.</returns>
		private bool CheckFilterConditions(Card card, string setFilter, string colorFilter) {
			bool passesSetFilter = false;
			bool passesColorFilter = false;

			if(setFilter != string.Empty) {
				string[] sets = setFilter.Split(',');

				foreach(var set in sets) {
					if(card.Set.ToLower() == set.ToLower()) {
						passesSetFilter = true;
						break;
					}
				}
			} else {
				passesSetFilter = true;
			}

			if(colorFilter != string.Empty) {
				string[] colors = colorFilter.Split(',');

				foreach(var color in colors) {
					if((card.Color != null ? card.Color : "X").ToLower() == color.ToLower()) {
						passesColorFilter = true;
						break;
					}
				}
			} else {
				passesColorFilter = true;
			}

			return (passesSetFilter && passesColorFilter);
		}

		/// <summary>
		/// GET /Deck/GetCardInfo/[id]
		/// Gets a JSON object containing the info for the specified card.
		/// </summary>
		/// <param name="id">The name of the card.</param>
		/// <returns>A JSON object containing the info for the specified card.</returns>
		public string GetCardInfo(string id) {
			CardDatabase db = new CardDatabase(Server.MapPath(cardDbPath));

			Card card = db.GetCardByName(id);

			if(card != null) {
				JavaScriptSerializer js = new JavaScriptSerializer();
				return js.Serialize(card);
			}

			return string.Empty;
		}

		/// <summary>
		/// GET /Deck/GetFilteredCardList?nameFilter&setFilter&colorFilter
		/// Gets the filtered card list.
		/// </summary>
		/// <param name="nameFilter">The partial card name.</param>
		/// <param name="setFilter">A comma-separated list of sets to include.</param>
		/// <param name="colorFilter">A comma-separated list of colors to include.</param>
		/// <returns>A JSON array of cards in the format Color|Set|Name.</returns>
		public string GetFilteredCardList(string nameFilter = "", string setFilter = "", string colorFilter = "") {
			CardDatabase db = new CardDatabase(Server.MapPath(cardDbPath));

			string[] cardList = (from c in db.cards
								 where (c.Name.ToLower().Contains(nameFilter.ToLower())) &&
								       (CheckFilterConditions(c, setFilter, colorFilter))
								 orderby c.Name
								 select string.Format("{0}|{1}|{2}", c.Color, c.Set, c.Name)).ToArray();

			if(cardList.Length > 0) {
				JavaScriptSerializer js = new JavaScriptSerializer();
				return js.Serialize(cardList);
			}

			return string.Empty;
		}

		/// <summary>
		/// POST /Deck/SaveDeck?cardList[]&deckName&deckId
		/// Saves the deck with the specified name and id.
		/// </summary>
		/// <param name="cardList">The card list.</param>
		/// <param name="deckName">The name of the deck.</param>
		/// <param name="deckId">The deck id, or -1 to create a new deck.</param>
		/// <returns>A redirect to /Deck/List on success, /Deck/Create/[id] on failure.</returns>
		[HttpPost]
		public ActionResult SaveDeck(string[] cardList, string deckName = "", int deckId = -1) {
			if(deckName == string.Empty) {
				return RedirectToAction("Create", new { deckId = deckId });
			}

			if(!Models.AccountUtils.isLoggedIn(this)) {
				return RedirectToAction("LogIn", "Account");
			}

			var loginHash = Request.Cookies["loginHash"].Value;

			var accountId = Models.AccountUtils.GetAccountID(loginHash);

			if(accountId < 0) return RedirectToAction("LogOut", "Account");

			if(deckId < 0) {
				// New deck
				var deckDB = new Models.DecksDataContext();
				var newDeck = new Models.Deck {
					OwnerId = accountId,
					Name = deckName
				};

				deckDB.Decks.InsertOnSubmit(newDeck);
				deckDB.SubmitChanges();

				var cards = new List<Models.Card>();

				Array.ForEach(cardList, c => {
					cards.Add(new Models.Card {
						DeckId = newDeck.Id,
						CardName = c
					});
				});

				var cardsDB = new Models.CardsDataContext();
				cardsDB.Cards.InsertAllOnSubmit(cards);
				cardsDB.SubmitChanges();
			} else {
				// Existing deck
				var deckDB = new Models.DecksDataContext();
				var deck = from d in deckDB.Decks
						   where (d.Id == deckId) &&
						         (d.OwnerId == accountId)
						   select d;

				if(deck.ToArray().Length <= 0) return RedirectToAction("Create");

				deck.First().Name = deckName;
				deckDB.SubmitChanges();

				var cardsDB = new Models.CardsDataContext();

				// Clear the deck before inserting the cards
				var currentCards = from c in cardsDB.Cards
								   where c.DeckId == deckId
								   select c;

				cardsDB.Cards.DeleteAllOnSubmit(currentCards);

				// Add the new cards to the deck
				var cards = new List<Models.Card>();

				Array.ForEach(cardList, c => {
					cards.Add(new Models.Card {
						DeckId = deckId,
						CardName = c
					});
				});

				cardsDB.Cards.InsertAllOnSubmit(cards);
				cardsDB.SubmitChanges();
			}

			return RedirectToAction("List");
		}

		/// <summary>
		/// GET /Deck/Delete/[id]
		/// Deletes the specified deck from the database.
		/// </summary>
		/// <param name="id">The deck id.</param>
		/// <returns>A redirect to /Deck/List.</returns>
		public ActionResult Delete(int id) {
			if(!Models.AccountUtils.isLoggedIn(this)) return RedirectToAction("LogIn", "Account");

			var loginHash = Request.Cookies["loginHash"].Value;
			var accountId = Models.AccountUtils.GetAccountID(loginHash);

			if(accountId < 0) return RedirectToAction("LogOut", "Account");

			var deckDB = new Models.DecksDataContext();

			var decks = from d in deckDB.Decks
						where (d.OwnerId == accountId) &&
							  (d.Id == id)
						select d;

			deckDB.Decks.DeleteAllOnSubmit(decks);
			deckDB.SubmitChanges();

			return RedirectToAction("List");
		}

		/// <summary>
		/// GET /Deck/Load
		/// Loads a deck from a saved file.
		/// </summary>
		/// <returns>The view</returns>
		public ActionResult Load() {
			return View();
		}

		/// <summary>
		/// Loads a deck from the uploaded file.
		/// </summary>
		/// <param name="deckFile">The uploaded deck file.</param>
		/// <returns>A redirect to /Deck/List on success, /Deck/Load on failure.</returns>
		[HttpPost]
		public ActionResult Load(HttpPostedFileBase deckFile) {
			if(deckFile != null && deckFile.ContentLength > 0) {
				var xml = XDocument.Load(new StreamReader(deckFile.InputStream));
				if((string)xml.Element("cockatrice_deck").Attribute("version") != string.Empty) {
					var deckName = (string)xml.Element("cockatrice_deck").Element("deckname");

					var zone = from z in xml.Descendants("cockatrice_deck").Elements("zone")
								where (string)z.Attribute("name") == "main"
								select z;

					var cards = from c in zone.Elements("card")
								select c;

					var cardList = new List<string>();
					foreach(var card in cards) {
						var count = int.Parse((string)card.Attribute("number"));
						var name = (string)card.Attribute("name");

						for(int i = 0; i < count; ++i) {
							cardList.Add(name);
						}
					}

					SaveDeck(cardList.ToArray(), deckName);
				}
			} else {
				return RedirectToAction("Load");
			}

			return RedirectToAction("List");
		}

		/// <summary>
		/// Lists the decks that belong to the user.
		/// </summary>
		/// <returns>The view.</returns>
		public ActionResult List() {
			if(!Models.AccountUtils.isLoggedIn(this)) return RedirectToAction("LogIn", "Account");

			var loginHash = Request.Cookies["loginHash"].Value;
			var accountId = Models.AccountUtils.GetAccountID(loginHash);

			if(accountId < 0) return RedirectToAction("LogOut", "Account");

			var deckDB = new Models.DecksDataContext();

			var decks = from d in deckDB.Decks
						where d.OwnerId == accountId
						select d;

			var model = new Models.DeckListViewModel();
			model.deckList = new List<Models.Deck>();

			foreach(var deck in decks) {
				model.deckList.Add(deck);
			}

			return View(model);
		}

		/// <summary>
		/// GET /Deck/Statistics/[id]
		/// Gives a statistical analysis of a deck.
		/// </summary>
		/// <param name="id">The id of the deck to analyze.</param>
		/// <returns>The view on success, a redirect to /Deck/List on failure.</returns>
		public ActionResult Statistics(int id = -1) {
			if(id < 0) return RedirectToAction("List");
			if(!Models.AccountUtils.isLoggedIn(this)) return RedirectToAction("LogIn", "Account");

			var loginHash = Request.Cookies["loginHash"].Value;
			var accountId = Models.AccountUtils.GetAccountID(loginHash);

			if(accountId < 0) return RedirectToAction("LogOut", "Account");

			// Check if this account is the owner of the deck
			var deckDB = new Models.DecksDataContext();
			var isOwner = (from d in deckDB.Decks
						   where (d.Id == id) &&
						  		 (d.OwnerId == accountId)
						   select true).ToArray().Length > 0 ? true : false;

			if(!isOwner) return RedirectToAction("List");

			var model = new Models.DeckStatisticsViewModel();
			List<string> cardList = new List<string>();

			var cardsDB = new Models.CardsDataContext();
			var cardsInDeck = from c in cardsDB.Cards
							  where c.DeckId == id
							  select c;

			foreach(var card in cardsInDeck) {
				cardList.Add(card.CardName);
			}

			model.PopulateDeck(cardList.ToArray(), Server.MapPath(cardDbPath));
			model.GenerateStatistics();

			return View(model);
		}
	}
}
