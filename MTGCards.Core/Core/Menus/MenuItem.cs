using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MTGCards.Core.Menus {
	public class MenuItem {
		public string Uri { get; set; }
		
		public string Text { get; set; }

		private bool _SubItem = false;
		public bool SubItem {
			get {
				return this._SubItem;
			}

			set {
				this._SubItem = value;
			}
		}
	}
}