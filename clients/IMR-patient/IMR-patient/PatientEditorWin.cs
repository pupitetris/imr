using System;
using Mono.Unix;
using System.Text;
using Newtonsoft.Json.Linq;
using monoCharp;
using System.Linq;

namespace IMRpatient
{
	public partial class PatientEditorWin : UtilityWin
	{
		public enum TYPE {
			NEW,
			EDIT
		}

		private JObject myData;
		private JObject myDetails;
		private int personaId;
		private TYPE OpType;
		private bool renewDetails = true;

		private void SetupForNew ()
		{
			Title = Catalog.GetString ("New Patient");
			personaId = 0;
			renewDetails = false;

			DeletePatientAction.Visible = false;
		}

		private void SetupForEdit (JObject data)
		{
			LoadData (data);

			Title = Catalog.GetString ("Edit Patient");
			DeletePatientAction.Visible = true;

			personaEditor.LoadData (data);
			personaAddEditor.LoadData (data);
		}

		private void LoadDetails (JObject data) {
			myDetails = data;
			renewDetails = false;
			if (data["persona_id"] != null)
				personaId = (int) data["persona_id"];

			string val;
			if (Util.DictTryValue (data, "birth", out val)) {  dateButtonBirth.Date = DateTime.Parse (val); }

			checkAlcohol.TriState = (bool?) data["alcohol"];
			checkTobacco.TriState = (bool?) data["tobacco"];
			checkDrugs.TriState = (bool?) data["drugs"];

			Util.GtkTextSetFromDict (textSickness, data, "sickness_remarks");
			Util.GtkTextSetFromDict (textMedic, data, "medic_remarks");
			Util.GtkTextSetFromDict (textHereditary, data, "hereditary_remarks");
			Util.GtkTextSetFromDict (textDiet, data, "diet_remarks");
			Util.GtkTextSetFromDict (textAlcohol, data, "activity_remarks");
			Util.GtkTextSetFromDict (textAlcohol, data, "alcohol_remarks");
			Util.GtkTextSetFromDict (textTobacco, data, "tobacco_remarks");
			Util.GtkTextSetFromDict (textDrugs, data, "drugs_remarks");
		}

		private void LoadData (JObject data) 
		{
			myData = data;
			personaId = (int) myData["persona_id"];

			if (renewDetails) {
				config.charp.request ("patient_get_details", new object[] { myData["persona_id"] }, new CharpGtk.CharpGtkCtx {
					asSingle = true,
					parent = this,
					success = delegate (object dat, Charp.CharpCtx ctx) {
						Gtk.Application.Invoke (delegate {
							LoadDetails ((JObject) dat);
						});
					}
				});
			}
		}

		public PatientEditorWin (TYPE type, AppConfig config, JObject data = null) : 
		base(config)
		{
			this.Build ();

			personaEditor.Setup (config, this);
			personaAddEditor.Setup (config, this);

			OpType = type;

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
				SaveKey ("patient_pictureFolder", personaEditor.pictureFolder);
			}
		}

		protected override void LoadState ()
		{
			base.LoadState ();
			LoadKey ("patient_pictureFolder", out personaEditor.pictureFolder);
		}

		protected void OnDeleteActionActivated (object sender, EventArgs e)
		{
			LockMenu (menubar, (Gtk.Action) sender);

			config.charp.request ("patient_delete", new object[] { personaId }, new CharpGtk.CharpGtkCtx {
				parent = this,
				success = delegate (object data, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						if (config.mainwin.patientListWin != null)
							config.mainwin.patientListWin.Refresh ();
						SendClose ();
					});
				},
				error = delegate(Charp.CharpError err, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate { FinishAction (menubar); });
					return true;
				}
			});
		}

		protected void OnCancelActionActivated (object sender, EventArgs e)
		{
			SendClose ();
		}

		private bool Validate () {
			StringBuilder b = new StringBuilder ();

			personaEditor.Validate (b);
			personaAddEditor.Validate (b);

			string msg = b.ToString ();
			int errors = msg.Count (c => c == '●');
			if (errors == 0)
				return true;

			SendAction (menubar, delegate {
				b.Insert (0, String.Format (Catalog.GetPluralString ("You have {0} error:\n\n", 
					"You have {0} errors:\n\n", errors), errors));

				Gtk.MessageDialog dlg = new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, 
					Gtk.ButtonsType.Ok, msg);
				dlg.Icon = Stetic.IconLoader.LoadIcon (dlg, "gtk-dialog-error", Gtk.IconSize.Dialog);
				dlg.Title = Catalog.GetString ("Validation");
				dlg.Run ();
				dlg.Destroy ();
			});

			return false;
		}

		private bool CommitError (Charp.CharpError err, Charp.CharpCtx ctx) {
			Gtk.Application.Invoke (delegate { FinishAction (menubar); });
			return true;
		}

		private void CommitSuccess (object data, Charp.CharpCtx ctx) {
			SendAction (menubar, delegate {
				if (config.mainwin.patientListWin != null)
					config.mainwin.patientListWin.Refresh ();
				Destroy ();
			});
		}

		private void CommitPersonaSuccess (object data, Charp.CharpCtx ctx) {
			personaAddEditor.Commit (CommitSuccess, CommitError);
		}

		private void CommitPatientSuccess (object data, Charp.CharpCtx ctx) {
			LoadDetails ((JObject) data);
			if (OpType == TYPE.NEW) {
				personaEditor.SetPersonaId (personaId);
				personaAddEditor.SetPersonaId (personaId);
			}
			personaEditor.Commit (CommitPersonaSuccess, CommitError);
		}

		private void Commit ()
		{
			string[] types = { "OPERATOR", "ADMIN", "SUPERUSER" };

			object[] parms = {
				null,
				textSickness.Buffer.Text,
				textMedic.Buffer.Text,
				textHereditary.Buffer.Text,
				textDiet.Buffer.Text,
				textActivity.Buffer.Text,
				checkAlcohol.TriState,
				textAlcohol.Buffer.Text,
				checkTobacco.TriState,
				textTobacco.Buffer.Text,
				checkDrugs.TriState,
				textDrugs.Buffer.Text
			};

			if (OpType == TYPE.EDIT || dateButtonBirth.HasChanged)
				parms[0] = dateButtonBirth.Date.ToString ("yyyy-MM-dd");

			if (OpType == TYPE.NEW ||
				!Util.StrEqNull ((string) parms[0], (string) myDetails["birth"])  ||
				!Util.StrEqNull ((string) parms[1], (string) myDetails["sickness_remarks"])  ||
				!Util.StrEqNull ((string) parms[2], (string) myDetails["medic_remarks"])  ||
				!Util.StrEqNull ((string) parms[3], (string) myDetails["hereditary_remarks"])  ||
				!Util.StrEqNull ((string) parms[4], (string) myDetails["diet_remarks"])  ||
				!Util.StrEqNull ((string) parms[5], (string) myDetails["activity_remarks"])  ||
				!Util.StrEqNull ((string) parms[6], (string) myDetails["alcohol"])  ||
				!Util.StrEqNull ((string) parms[7], (string) myDetails["alcohol_remarks"])  ||
				!Util.StrEqNull ((string) parms[8], (string) myDetails["tobacco"])  ||
				!Util.StrEqNull ((string) parms[9], (string) myDetails["tobacco_remarks"])  ||
				!Util.StrEqNull ((string) parms[10], (string) myDetails["drugs"])  ||
				!Util.StrEqNull ((string) parms[11], (string) myDetails["drugs_remarks"])) {

				string resource;
				if (OpType == TYPE.NEW) {
					resource = "patient_create";
				} else {
					resource = "patient_update";
					parms = Util.ArrayUnshift (parms, personaId);
				}

				config.charp.request (resource, parms, new CharpGtk.CharpGtkCtx {
					asSingle = true,
					parent = this,
					success = CommitPatientSuccess,
					error = CommitError
				});
			} else {
				personaEditor.Commit (CommitPersonaSuccess, CommitError);
			}
		}

		protected void OnOKActionActivated (object sender, EventArgs e)
		{
			LockMenu (menubar, (Gtk.Action) sender);

			if (Validate ())
				Commit ();
		}

	}
}
