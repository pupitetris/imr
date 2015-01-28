using System;
using System.Collections.Specialized;

namespace IMRpatient
{
	public abstract class UtilityWin : Gtk.Window
	{
		protected delegate void VoidDelegate ();
		private delegate bool ActionDelegate ();

		protected AppConfig config;
		
		private const uint SEND_CLOSE_TIMEOUT = 50; // msecs to pass before closing the window.
		private const uint SEND_ACTION_TIMEOUT = 100; // msecs to pass before an action is performed.
		private const uint PRESENT_TIMEOUT = 100; // msecs to pass before the window is presented after initial show.

		public UtilityWin (AppConfig config) :
			base(Gtk.WindowType.Toplevel)
		{
			this.config = config;
			this.DeleteEvent += delegate { SaveState (); };
			this.MapEvent += delegate {	
				LoadState (); 
				GLib.Timeout.Add (PRESENT_TIMEOUT, new GLib.TimeoutHandler (delegate () { Present (); return false; }));
			};
		}

		protected void SaveKey (string key, string value)
		{
			config.SaveWindowKey (Util.GtkGetWidgetPath (this, config), key, value);
		}
		
		protected void SaveKey (string key, int value)
		{
			config.SaveWindowKey (Util.GtkGetWidgetPath (this, config), key, value);
		}

		protected bool LoadKey (string key, out string value)
		{
			return config.LoadWindowKey (Util.GtkGetWidgetPath (this, config), key, out value);
		}
		
		protected bool LoadKey (string key, out int value)
		{
			return config.LoadWindowKey (Util.GtkGetWidgetPath (this, config), key, out value);
		}

		protected virtual void LoadState ()
		{
			int x, y, w, h;
			if (config.LoadWindowGeom (Util.GtkGetWidgetPath (this, config), out x, out y, out w, out h)) {
				this.Move (x, y);
				this.Resize (w, h);
			}
		}

		public virtual void SaveState ()
		{
			int x, y, w, h;
			this.GetPosition (out x, out y);
			this.GetSize (out w, out h);
			config.SaveWindowGeom (Util.GtkGetWidgetPath (this, config), x, y, w, h);
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
				foreach (Gtk.MenuItem child in menubar.Children)
					child.Show ();

				menubar.Cancel ();

				if (del != null)
					del ();

				return false;
			};

			GLib.Timeout.Add (SEND_ACTION_TIMEOUT, new GLib.TimeoutHandler (adel));
		}

		protected void FinishAction (Gtk.MenuBar menubar) {
			SendAction (menubar, null);
		}

		protected void LockMenu (Gtk.MenuBar menubar, Gtk.Action action) {
			foreach (Gtk.MenuItem child in menubar.Children) {
				if (child.Action != action)
					child.Hide ();
			}
		}
	}
}

