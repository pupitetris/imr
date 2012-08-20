using System;
using monoCharp;
using System.Net;
using Mono.Unix;

namespace IMRpatient
{
	public partial class WelcomeDlg : Gtk.Window
	{
		private AppConfig config;
		private bool userFirstEdit;
		private bool passwdFirstEdit;
		private bool success;

		public WelcomeDlg (AppConfig config) : base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.config = config;
			userFirstEdit = true;
			passwdFirstEdit = true;
			success = false;

			table1.FocusChain = new Gtk.Widget[] { entryUser, entryPasswd, buttonOk, buttonConf };

			string ver = AppConfig.GetAppVersion ();
			labelVersion.Markup = String.Format ("<span color=\"white\"><b>v. {0}</b></span>", ver);
			labelVersionShadow.Markup = String.Format ("<span color=\"black\"><b>v. {0}</b></span>", ver);
		}

		private void devError (string message)
		{
			DeviceErrorDlg dlg = new DeviceErrorDlg (config, message);
			dlg.TransientFor = this;
			dlg.Response += delegate(object o, Gtk.ResponseArgs args) {
				dlg.Destroy ();
				switch (args.ResponseId) {
				case Gtk.ResponseType.Yes:
					// Retry
					devProbeTry ();
					break;
				case Gtk.ResponseType.Accept:
					// Ignore
					success = true;
					Gtk.Main.Quit ();
					Destroy ();
					break;
				}
			};
			dlg.Run ();
			Sensitive = true;
		}

		private void devProbeTry ()
		{
			try {
				config.radionic.Open ();
				config.radionic.Probe (delegate (Radionic.RESULT result, string reply) {
					switch (result) {
					case Radionic.RESULT.SUCCESS:
						success = true;
						Gtk.Main.Quit ();
						Destroy ();
						break;
					case Radionic.RESULT.TIMEOUT:
						config.radionic.Close ();
						devError (Catalog.GetString ("Timeout."));
						break;
					default:
						config.radionic.Close ();
						devError (Catalog.GetString ("Read error."));
						break;
					}
				});
			} catch (Exception e) {
				devError (String.Format (Catalog.GetString ("Open: {0}"), e.Message));
			}
		}

		private void authTry ()
		{
			Sensitive = false;
			success = false;

			config.charp.credentialsSet (entryUser.Text, Charp.GetMD5HexHash (entryPasswd.Text));
			config.charp.request ("user_auth", null, new Charp.CharpCtx {
				success = delegate { 
					Gtk.Application.Invoke (delegate {
						devProbeTry ();
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
			config.Setup (this);
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
