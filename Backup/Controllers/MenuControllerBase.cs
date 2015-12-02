using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MTGCards.Core;
using MTGCards.Core.Menus;

namespace MTGCards.Controllers {
	/// <summary>
	/// Base controller that allows easy creation of sidebar menus.
	/// </summary>
	public abstract class MenuControllerBase : Controller {
		protected override void OnActionExecuting(ActionExecutingContext filterContext) {
			base.OnActionExecuting(filterContext);

			ViewBag.Menu = MenuGenerator.GenerateMenu(CreateMenu());
		}


		/// <summary>
		/// Overload this method to create the menu
		/// </summary>
		/// <example>
		/// protected override MenuItem[] CreateMenu() {
		///		List<MenuItem> menuItems = new List<MenuItem>();
		///
		///		menuItems.Add(new MenuItem { Uri = "Uri to link to", Text = "Text to be shown" });
		///		menuItems.Add(new MenuItem { Uri = "Uri to link to", Text = "Menu Subitem Text", SubItem = true });
		///		
		///		return menuItems.ToArray();
		///	}
		/// </example>
		/// <returns>An array of MenuItems that represents the menu in the sidebar</returns>
		protected abstract MenuItem[] CreateMenu();
	}
}
