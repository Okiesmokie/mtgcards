using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using MTGCards.Core;
using MTGCards.Core.Menus;

namespace MTGCards.Controllers {
	public class AccountController : MenuControllerBase {
		protected override MenuItem[] CreateMenu() {
			List<MenuItem> menuItems = new List<MenuItem>();

			menuItems.Add(new MenuItem { Uri = Url.Action("Index", "Home"), Text = "Home" });
			menuItems.Add(new MenuItem { Uri = Url.Action("Index", "Account"), Text = "Account" });
			if(Models.AccountUtils.isLoggedIn(this)) {
				menuItems.Add(new MenuItem { Uri = Url.Action("LogOut", "Account"), Text = "Log out", SubItem = true });
			} else {
				menuItems.Add(new MenuItem { Uri = Url.Action("Register", "Account"), Text = "Register", SubItem = true });
				menuItems.Add(new MenuItem { Uri = Url.Action("LogIn", "Account"), Text = "Log in", SubItem = true });
			}
			menuItems.Add(new MenuItem { Uri = Url.Action("Index", "Deck"), Text = "Deck" });

			return menuItems.ToArray();
		}


		/// <summary>
		/// GET /Account/
		/// </summary>
		/// <returns>The view.</returns>
		public ActionResult Index() {
			return View();
		}


		/// <summary>
		/// GET /Account/Register
		/// </summary>
		/// <param name="errorCode">The error code.</param>
		/// <returns>The view.</returns>
		public ActionResult Register(int errorCode = 0) {
			if(errorCode != 0) {
				/* Error Codes:
				 *   100 - A required field is empty
				 *   200 - Invalid email address
				 *   300 - Username in use
				 */
				ViewBag.ErrorCode = errorCode;
			}

			return View();
		}

		/// <summary>
		/// POST /Account/Register
		/// Creates an account with the specified username and password.
		/// </summary>
		/// <param name="Username">The username.</param>
		/// <param name="Password">The password.</param>
		/// <param name="Email">The email address.</param>
		/// <returns>A redirect to /Account/Register on failure, /Account/LogIn on success.</returns>
		[HttpPost]
		public ActionResult Register(string Username = "", string Password = "", string Email = "") {
			if(Username == string.Empty || Password == string.Empty || Email == string.Empty) return RedirectToAction("Register", new { errorCode = 100 });
			if(!Regex.Match(Email, "\\b[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,4}\\b").Success) return RedirectToAction("Register", new { errorCode = 200 });

			// Check for existing username
			var accountDB = new Models.AccountsDataContext();
			var exisiting = (from a in accountDB.Accounts
							 where a.Username == Username
							 select true).ToArray();

			if(exisiting.Length > 0) {
				return RedirectToAction("Register", new { errorCode = 300 });
			}

			var LoginHash = Util.GenerateMD5(string.Format("{0}{1}", Username, Password));

			accountDB.Accounts.InsertOnSubmit(new Models.Account {
				Username = Username,
				Password = Core.Util.GenerateMD5(Password),
				Email = Email,
				LoginHash = LoginHash
			});

			accountDB.SubmitChanges();

			return RedirectToAction("LogIn");
		}

		/// <summary>
		/// GET /Account/LogIn
		/// The login page.
		/// </summary>
		/// <param name="errorCode">The error code.</param>
		/// <returns>The view.</returns>
		public ActionResult LogIn(int errorCode = 0) {
			if(Models.AccountUtils.isLoggedIn(this)) return RedirectToAction("Index");

			if(errorCode != 0) {
				/* Error Codes:
				 *   100 - A required field is empty
				 *   500 - Invalid username/password combination
				 */
				ViewBag.ErrorCode = errorCode;
			}

			return View();
		}

		/// <summary>
		/// POST /Account/LogIn
		/// Logs the user in with the specified credentials
		/// </summary>
		/// <param name="Username">The username.</param>
		/// <param name="Password">The password.</param>
		/// <returns>A redirect to LogIn with an error code on failure, /Account/ on success.</returns>
		[HttpPost]
		public ActionResult LogIn(string Username = "", string Password = "") {
			if(Username == "" || Password == "") return RedirectToAction("LogIn", new { errorCode = 100 });

			var accountDB = new Models.AccountsDataContext();
			var loginHash = (from a in accountDB.Accounts
							 where (a.Username.ToLower() == Username.ToLower()) &&
							       (a.Password == Core.Util.GenerateMD5(Password))
							 select a.LoginHash).ToArray();

			if(loginHash.Length == 0) {
				return RedirectToAction("LogIn", new { errorCode = 500 });
			}

			var cookie = new HttpCookie("loginHash");
			cookie.Value = loginHash[0];
			cookie.Expires = DateTime.Now.AddDays(365);
			Response.Cookies.Add(cookie);

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Logs the user out.
		/// </summary>
		/// <returns>A redirect to /Account/</returns>
		public ActionResult LogOut() {
			if(Request.Cookies["loginHash"] != null) {
				var cookie = new HttpCookie("loginHash");
				cookie.Expires = DateTime.Now.AddDays(-1);
				Response.Cookies.Add(cookie);
			}

			return RedirectToAction("Index");
		}
	}
}
