using System;
using monoCharp;
using System.Net;
using Mono.Unix;

namespace IMRpatient
{
	public partial class WelcomeDlg : Gtk.Window
	{
		private Charp charp;
		private Radionic radionic;
		private bool userFirstEdit;
		private bool passwdFirstEdit;
		private bool success;

		public WelcomeDlg (Charp charp, Radionic radionic) : base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.charp = charp;
			this.radionic = radionic;
			userFirstEdit = true;
			passwdFirstEdit = true;
			success = false;

			table1.FocusChain = new Gtk.Widget[] { entryUser, entryPasswd, buttonOk, buttonConf };
		}

		private void authTry ()
		{
			Sensitive = false;
			charp.credentialsSet (entryUser.Text, Charp.GetMD5HexHash (entryPasswd.Text));
			charp.request ("user_auth", null, new Charp.CharpCtx {
				success = delegate { 
					Gtk.Application.Invoke (delegate {
						success = true;
						Gtk.Main.Quit ();
						Destroy ();
					});
				},
				error = delegate (Charp.CharpError err, Charp.CharpCtx ctx) {
					if (err.key == "SQL:USERUNK" || err.key == "SQL:REPFAIL") {
						Gtk.Application.Invoke (delegate {
							Gtk.MessageDialog md = 
								new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Warning,
								                       Gtk.ButtonsType.Ok, Catalog.GetString ("\nIncorrect user or password."));
							md.Run ();
							md.Destroy ();
							Sensitive = true;
						});
						return false;
					}

					Gtk.Application.Invoke (delegate { Sensitive = true; });
					return true;
				}
			});
		}

		public bool authSuccess ()
		{
			return success;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			authTry ();
		}

		protected void OnButtonConfClicked (object sender, EventArgs e)
		{
			WelcomeSetupDlg dlg = new WelcomeSetupDlg (charp.baseUrl, radionic);
			dlg.Response += delegate(object o, Gtk.ResponseArgs args) {
				if (args.ResponseId == Gtk.ResponseType.Ok) {
					charp.baseUrl = dlg.baseUrl; 
				}
			};
			dlg.TransientFor = this;
			dlg.Run ();
		}

		protected void OnEntryUserChanged (object sender, EventArgs e)
		{
			userFirstEdit = false;
		}

		protected void OnEntryPasswdChanged (object sender, EventArgs e)
		{
			if (passwdFirstEdit) {
				passwdFirstEdit = false;
				entryPasswd.Visibility = false;
			}
		}

		protected void OnEntryPasswdActivated (object sender, EventArgs e)
		{
			authTry ();
		}

		protected void OnDeleteEvent (object sender, EventArgs e)
		{
			Gtk.Main.Quit ();
		}

		protected void OnEntryUserActivated (object sender, EventArgs e)
		{
			entryPasswd.GrabFocus ();
		}

		protected void OnEntryUserFocusGrabbed (object sender, EventArgs e)
		{
			if (userFirstEdit) {
				entryUser.Text = "";
			}
		}
	}
}
