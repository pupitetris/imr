using System;
using Mono.Unix;
using System.Text;
using Newtonsoft.Json.Linq;
using monoCharp;

namespace IMRpatient
{
	public partial class UserEditorWin : UtilityWin
	{
		public enum TYPE {
			NEW,
			EDIT,
			EDIT_SELF
		}

		private JObject myData;
		private int personaId;
		private TYPE OpType;

		private void SetupForNew ()
		{
			Title = Catalog.GetString ("New User");
			personaId = 0;

			DeleteAction.Visible = false;

			comboStatus.Active = 0;
			comboLevel.Active = 0;
			if (!config.CanPerform (IMR_PERM.USER_SET_ADMIN_LEVEL)) {
				comboLevel.Sensitive = false;
			}
		}

		private void SetupForEdit (JObject data)
		{
			Title = Catalog.GetString ("Edit User");
			myData = data;
			personaId = (int) myData["persona_id"];

			DeleteAction.Visible = true;

			entryUsername.Text = (string) data["username"];

			int idx = 0;
			switch ((string) data["type"]) {
			case "OPERATOR": idx = 0; break;
			case "ADMIN": idx = 1; break;
			case "SUPERUSER": idx = 2; break;
			}
			comboLevel.Active = idx;
			comboStatus.Active = ((string) data["status"] == "ACTIVE")? 0: 1;

			if (!config.CanPerform (IMR_PERM.USER_SET_ADMIN_LEVEL)) {
				comboLevel.Sensitive = false;
			}

			personaEditor.LoadData (data);
			personaAddEditor.LoadData (data);
		}

		public UserEditorWin (TYPE type, AppConfig config, JObject data = null) : 
				base(config)
		{
			this.Build ();

			personaEditor.Setup (config, this);
			personaAddEditor.Setup (config, this);

			OpType = type;

			tableUser.FocusChain = new Gtk.Widget[] { entryUsername, entryPassword, entryConfirm, comboStatus, comboLevel };

			switch (type) {
			case TYPE.NEW:
				SetupForNew ();
				break;
			case TYPE.EDIT:
				SetupForEdit (data);
				break;
			}
		}

		public override void SaveState ()
		{
			base.SaveState ();
			if (personaEditor.pictureFolder != null) {
				SaveKey ("pictureFolder", personaEditor.pictureFolder);
			}
		}

		protected override void LoadState ()
		{
			base.LoadState ();
			LoadKey ("pictureFolder", out personaEditor.pictureFolder);
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
			
			if (entryUsername.Text.Length == 0) {
				b.Append (Catalog.GetString ("You have to set an username.\n"));
				Util.GtkLabelStyleAsError (labelUsername);
			} else {
				Util.GtkLabelStyleRemove (labelUsername);
			}

			if (entryPassword.Text.Length == 0 && OpType == TYPE.NEW) {
				b.Append (Catalog.GetString ("You have to set a password.\n"));
				Util.GtkLabelStyleAsError (labelPassword);
			} else {
				Util.GtkLabelStyleRemove (labelPassword);
			}

			if (entryPassword.Text != entryConfirm.Text) {
				b.Append (Catalog.GetString ("Password and confirmation must be the same.\n"));
				Util.GtkLabelStyleAsError (labelConfirm);
			} else {
				Util.GtkLabelStyleRemove (labelConfirm);
			}

			personaEditor.Validate (b);
			personaAddEditor.Validate (b);

			int errors = b.Length;
			if (errors == 0)
				return true;

			b.Insert (0, String.Format (Catalog.GetPluralString ("You have {0} error:\n\n", 
			                                                     "You have {0} errors:\n\n", errors), errors));

			Gtk.MessageDialog dlg = new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, 
			                                               Gtk.ButtonsType.Ok, b.ToString ());
			dlg.Icon = Stetic.IconLoader.LoadIcon (dlg, "gtk-dialog-error", Gtk.IconSize.Dialog);
			dlg.Title = Catalog.GetString ("Validation");
			dlg.Run ();
			dlg.Destroy ();

			return false;
		}

		private bool CommitError (Charp.CharpError err, Charp.CharpCtx ctx) {
			Gtk.Application.Invoke (delegate { FinishAction (menubar); });
			return true;
		}

		private void CommitSuccess (object data, Charp.CharpCtx ctx) {
			if (OpType != TYPE.NEW && config.mainwin.userListWin != null) {
				config.mainwin.userListWin.Refresh ();
			}
			Destroy ();
		}

		private void CommitPersonaSuccess (object data, Charp.CharpCtx ctx) {
			personaAddEditor.Commit (CommitSuccess, CommitError, this);
		}

		private void CommitUserSuccess (object data, Charp.CharpCtx ctx) {
			if (OpType == TYPE.NEW) {
				personaId = (int) ((JValue) data).Value;
				personaEditor.SetPersonaId (personaId);
				personaAddEditor.SetPersonaId (personaId);
			}
			personaEditor.Commit (CommitPersonaSuccess, CommitError, this);
		}

		private void Commit ()
		{
			string[] types = { "OPERATOR", "ADMIN", "SUPERUSER" };

			object[] parms = new object[] {
				entryUsername.Text,
				entryPassword.Text,
				types[comboLevel.Active],
				(comboStatus.Active == 0)? "ACTIVE" : "DISABLED"
			};

			if (OpType == TYPE.NEW ||
				(string) parms[0] != (string) myData["username"] ||
				(string) parms[1] != "" ||
				(string) parms[2] != (string) myData["type"] ||
				(string) parms[3] != (string) myData["status"]) {

				if (parms[1] != "")
					parms[1] = Charp.GetMD5HexHash (entryPassword.Text);

				string resource;
				if (OpType == TYPE.NEW) {
					resource = "user_create";
				} else {
					resource = "user_update";
					parms[4] = personaId;
				}

				config.charp.request (resource, parms, new CharpGtk.CharpGtkCtx {
					parent = this,
					success = CommitUserSuccess,
					error = CommitError
				});
			} else {
				personaEditor.Commit (CommitPersonaSuccess, CommitError, this);
			}
		}

		protected void OnOKActionActivated (object sender, EventArgs e)
		{
			SendAction (menubar, delegate {
				if (Validate ())
					Commit ();
			});
		}
	}
}

