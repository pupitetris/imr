using System;
using Mono.Unix;

namespace IMRpatient
{
	public partial class UserEditorWin : Gtk.Window
	{
		public enum TYPE {
			NEW,
			EDIT
		}

		public UserEditorWin (TYPE type) : 
				base(Gtk.WindowType.Toplevel)
		{
			Build ();

			switch (type) {
			case TYPE.NEW:
				Title = Catalog.GetString ("New User");
				DeleteAction.Visible = false;
				break;
			case TYPE.EDIT:
				Title = Catalog.GetString ("Edit User");
				break;
			}
		}

		private bool CloseWindow ()
		{
			Destroy ();
			return false;
		}
		
		private void SendClose ()
		{
			GLib.Timeout.Add (50, new GLib.TimeoutHandler (CloseWindow));
		}

		protected void OnDeleteActionActivated (object sender, EventArgs e)
		{
			SendClose ();
		}

		protected void OnCancelActionActivated (object sender, EventArgs e)
		{
			SendClose ();
		}
		
		protected void OnOKActionActivated (object sender, EventArgs e)
		{
			SendClose ();
		}
	}
}

