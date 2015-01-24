using System;
using Mono.Unix;
using System.Text;
using Newtonsoft.Json.Linq;
using monoCharp;

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

			string val;
			if (Util.DictTryValue (data, "birth", out val)) { comboBirth.Entry.Text = val; }

			checkAlcohol.Active = (bool) data["alcohol"];
			checkTobacco.Active = (bool) data["tobacco"];
			checkDrugs.Active = (bool) data["drugs"];

			if (Util.DictTryValue (data, "sickness_remarks", out val))
				textSickness.Buffer.InsertAtCursor (val);
			if (Util.DictTryValue (data, "medication_remarks", out val))
				textMedication.Buffer.InsertAtCursor (val);
			if (Util.DictTryValue (data, "diet_remarks", out val))
				textDiet.Buffer.InsertAtCursor (val);
			if (Util.DictTryValue (data, "activity_remarks", out val))
				textActivity.Buffer.InsertAtCursor (val);
			if (Util.DictTryValue (data, "alcohol_remarks", out val))
				textAlcohol.Buffer.InsertAtCursor (val);
			if (Util.DictTryValue (data, "tobacco_remarks", out val))
				textTobacco.Buffer.InsertAtCursor (val);
			if (Util.DictTryValue (data, "drugs_remarks", out val))
				textDrugs.Buffer.InsertAtCursor (val);
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

			// TODO: validate comboBirth.Entry.Text

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
			SendAction (menubar, delegate {
//				if (config.mainwin.patientListWin != null)
//					config.mainwin.patientListWin.Refresh ();
				Destroy ();
			});
		}

		private void CommitPersonaSuccess (object data, Charp.CharpCtx ctx) {
			personaAddEditor.Commit (CommitSuccess, CommitError);
		}

		private void CommitPatientSuccess (object data, Charp.CharpCtx ctx) {
			LoadData ((JObject) data);
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
				comboBirth.Entry.Text,
				textSickness.Buffer.Text,
				textMedication.Buffer.Text,
				textDiet.Buffer.Text,
				textActivity.Buffer.Text,
				checkAlcohol.Active,
				textAlcohol.Buffer.Text,
				checkTobacco.Active,
				textTobacco.Buffer.Text,
				checkDrugs.Active,
				textDrugs.Buffer.Text
			};

			if (OpType == TYPE.NEW ||
				(string) parms[0] != (string) myData["birth"] ||
				(string) parms[1] != (string) myData["sickness_remarks"] ||
				(string) parms[2] != (string) myData["medication_remarks"] ||
				(string) parms[3] != (string) myData["diet_remarks"] ||
				(string) parms[4] != (string) myData["activity_remarks"] ||
				(string) parms[5] != (string) myData["alcohol"] ||
				(string) parms[6] != (string) myData["alcohol_remarks"] ||
				(string) parms[7] != (string) myData["tobacco"] ||
				(string) parms[8] != (string) myData["tobacco_remarks"] ||
				(string) parms[9] != (string) myData["drugs"] ||
				(string) parms[10] != (string) myData["drugs_remarks"]) {

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
			if (Validate ())
				Commit ();
		}
	}
}
