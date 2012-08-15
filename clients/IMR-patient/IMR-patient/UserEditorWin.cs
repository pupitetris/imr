using System;
using Mono.Unix;
using System.Text;

namespace IMRpatient
{
	public partial class UserEditorWin : UtilityWin
	{
		public enum TYPE {
			NEW,
			EDIT,
			EDIT_SELF
		}

		private void SetupForNew ()
		{
			Title = Catalog.GetString ("New User");

			DeleteAction.Visible = false;

			comboStatus.Active = 0;
			comboLevel.Active = 0;
			if (!config.CanPerform (IMR_PERM.USER_SET_ADMIN_LEVEL)) {
				comboLevel.Sensitive = false;
			}
		}

		private void SetupForEdit ()
		{
			Title = Catalog.GetString ("Edit User");
		}

		public UserEditorWin (TYPE type, AppConfig config) : 
				base(config)
		{
			this.Build ();

			switch (type) {
			case TYPE.NEW:
				SetupForNew ();
				break;
			case TYPE.EDIT:
				SetupForEdit ();
				break;
			}
		}

		protected void OnDeleteActionActivated (object sender, EventArgs e)
		{
			SendClose ();
		}

		protected void OnCancelActionActivated (object sender, EventArgs e)
		{
			SendClose ();
		}

		private bool Validate () {
			StringBuilder b = new StringBuilder ();
			int errors = 0;

			if (entryUsername.Text.Length == 0) {
				errors ++;
				b.Append ("You have to set an username.\n");
				Util.GtkLabelStyleAsError (labelUsername);
			} else {
				Util.GtkLabelStyleRemove (labelUsername);
			}

			if (entryPassword.Text.Length == 0) {
				errors ++;
				b.Append ("You have to set a password.\n");
				Util.GtkLabelStyleAsError (labelPassword);
			} else {
				Util.GtkLabelStyleRemove (labelPassword);
			}

			if (entryPassword.Text != entryConfirm.Text) {
				errors ++;
				b.Append ("Password and confirmation must be the same.\n");
				Util.GtkLabelStyleAsError (labelConfirm);
			} else {
				Util.GtkLabelStyleRemove (labelConfirm);
			}

			if (errors == 0) {
				return true;
			}

			b.Insert (0, String.Format (Catalog.GetPluralString ("You have {0} error:\n\n", 
			                                                     "You have {0} errors:\n\n", errors), errors));

			Gtk.MessageDialog dlg = new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, 
			                                               Gtk.ButtonsType.Ok, b.ToString ());
			dlg.Run ();
			dlg.Destroy ();

			return false;
		}

		protected void OnOKActionActivated (object sender, EventArgs e)
		{
			SendAction (menubar, delegate {
				if (Validate ()) {
					Destroy ();
				}
			});
		}
	}
}

