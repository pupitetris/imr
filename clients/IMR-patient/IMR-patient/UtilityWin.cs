using System;

namespace IMRpatient
{
	public abstract class UtilityWin : Gtk.Window
	{
		protected AppConfig config;

		private delegate bool ActionDelegate ();
		protected delegate void VoidDelegate ();

		private static uint SEND_CLOSE_TIMEOUT = 50; // msecs to pass before closing the window.
		private static uint SEND_ACTION_TIMEOUT = 100; // msecs to pass before an action is performed.

		public UtilityWin (AppConfig config) :
			base(Gtk.WindowType.Toplevel)
		{
			this.config = config;
			this.DeleteEvent += delegate { SaveState ();	};
			this.MapEvent += delegate {	LoadState (); };
		}

		private string GetWindowPath ()
		{
			return config.LoginMD5 + "/" + Util.GtkGetWidgetPath (this);
		}

		protected void SaveKey (string key, int value)
		{
			config.SaveWindowKey (GetWindowPath (), key, value);
		}

		protected bool LoadKey (string key, out int value)
		{
			return config.LoadWindowKey (GetWindowPath (), key, out value);
		}

		protected virtual void LoadState ()
		{
			int x, y, w, h;
			if (config.LoadWindowGeom (GetWindowPath (), out x, out y, out w, out h)) {
				this.Move (x, y);
				this.Resize (w, h);
			}
		}

		protected virtual void SaveState ()
		{
			int x, y, w, h;
			this.GetPosition (out x, out y);
			this.GetSize (out w, out h);
			config.SaveWindowGeom (GetWindowPath (), x, y, w, h);
		}

		protected bool CloseWindow ()
		{
			SaveState ();
			Destroy ();
			return false;
		}
		
		protected void SendClose ()
		{
			GLib.Timeout.Add (SEND_CLOSE_TIMEOUT, new GLib.TimeoutHandler (CloseWindow));
		}

		protected void SendAction (Gtk.MenuBar menubar, VoidDelegate del)
		{
			ActionDelegate adel = delegate () {
				GLib.Signal.Emit (menubar, "cancel");
				if (del != null) {
					del ();
				}
				return false;
			};

			GLib.Timeout.Add (SEND_ACTION_TIMEOUT, new GLib.TimeoutHandler (adel));
		}

		protected void FinishAction (Gtk.MenuBar menubar) {
			SendAction (menubar, null);
		}
	}
}

