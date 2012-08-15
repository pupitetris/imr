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
		}

		protected bool CloseWindow ()
		{
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
				GLib.Signal.Emit (menubar, "selection-done");
				del ();
				return false;
			};

			GLib.Timeout.Add (SEND_ACTION_TIMEOUT, new GLib.TimeoutHandler (adel));
		}
	}
}

