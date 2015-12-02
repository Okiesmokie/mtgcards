using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MTGCards.Models {
	public class AccountUtils {
		public static bool isLoggedIn(Controller controller) {
			var cookie = controller.Request.Cookies["loginHash"];

			if(cookie == null) return false;

			var accountDB = new Models.AccountsDataContext();

			var account = (from x in accountDB.Accounts
						   where x.LoginHash == cookie.Value
						   select true).ToArray();

			if(account.Length > 0) return true;
			else return false;
		}

		public static int GetAccountID(string loginHash) {
			if(loginHash == null) return -1;
			if(loginHash == string.Empty) return -1;

			var accountDB = new Models.AccountsDataContext();
			var account = (from a in accountDB.Accounts
						   where a.LoginHash == loginHash
						   select a.Id).ToArray();

			if(account.Length <= 0) return -1;

			return account[0];
		}
	}
}