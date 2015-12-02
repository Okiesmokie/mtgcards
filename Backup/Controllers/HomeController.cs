using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MTGCards.Core;
using MTGCards.Core.Menus;

namespace MTGCards.Controllers {
	public class HomeController : MenuControllerBase {
		protected override MenuItem[] CreateMenu() {
			return new MenuItem[] {
				new MenuItem { Uri = Url.Action("Index", "Home"), Text = "Home" },
				new MenuItem { Uri = Url.Action("Index", "Account"), Text = "Account" },
				new MenuItem { Uri = Url.Action("Index", "Deck"), Text = "Deck" }
			};
		}

		/// <summary>
		/// GET /
		/// </summary>
		/// <returns>The view.</returns>
		public ActionResult Index() {
			return View();
		}
	}
}
