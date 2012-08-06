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

		protected void OnDeleteActionActivated (object sender, EventArgs e)
		{
			throw new System.NotImplementedException ();
		}

		protected void OnCancelActionActivated (object sender, EventArgs e)
		{
			Destroy ();
		}
		
		protected void OnOKActionActivated (object sender, EventArgs e)
		{
			throw new System.NotImplementedException ();
		}
	}
}

