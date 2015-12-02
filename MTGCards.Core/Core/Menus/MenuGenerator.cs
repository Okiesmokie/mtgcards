using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MTGCards.Core.Menus {
	public class MenuGenerator {
		public static string GenerateMenu(MenuItem[] menuList) {
			string menuString = string.Empty;

			menuString += "<ul>\r\n";
			foreach(var menuItem in menuList) {
				menuString += string.Format("<li><a {2} href=\"{0}\">{1}</a></li>\r\n", menuItem.Uri, menuItem.Text, (menuItem.SubItem ? "class=\"subitem\"" : string.Empty));
			}
			menuString += "</ul>\r\n";

			return menuString;
		}
	}
}