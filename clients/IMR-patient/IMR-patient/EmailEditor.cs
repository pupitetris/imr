using System;
using System.Text;
using Mono.Unix;
using monoCharp;
using Newtonsoft.Json.Linq;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmailEditor : Gtk.Bin
	{
		private AppConfig config;
		private Gtk.Window ParentWin;
		private Gtk.Container Cont;
		private JObject myData;
		private int emailId = 0;
		private int personaId = 0;
		private bool isNew = true;
		
		public EmailEditor ()
		{
			this.Build ();
		}
		
		public EmailEditor (AppConfig config, Gtk.Window parent, Gtk.Container cont, JObject data = null) {
			this.Build ();
			
			this.config = config;
			ParentWin = parent;
			Cont = cont;
			
			buttonDelete.ConfirmClick += delegate (object sender, EventArgs e) { Cont.Remove (this); };
			
			LoadData (data);
		}
		
		public void LoadData (JObject data)
		{
			if (data != null) {
				myData = data;
				emailId = (int) myData["email_id"];
				personaId = (int) myData["persona_id"];
				isNew = false;
				
				string val;
				if (Util.DictTryValue (data, "email", out val)) { entryEmail.Text = val; }
				if (Util.DictTryValue (data, "remarks", out val)) { textRemarks.Buffer.InsertAtCursor (val); }
				if (Util.DictTryValue (data, "e_type", out val)) {
					int active;
					switch (val) {
						case "PERSONAL": active = 0; break;
						case "WORK": active = 1; break;
						default: active = -1; break;
					}
					comboType.Active = active;
				}
				if (Util.DictTryValue (data, "system", out val)) {
					int active;
					switch (val) {
						case "STANDARD": active = 0; break;
						case "SKYPE": active = 1; break;
						default: active = -1; break;
					}
					comboSystem.Active = active;
				}
			} else
				myData = new JObject ();
		}

		public void SetPersonaId (int id) {
			personaId = id;
		}

		public bool Validate (StringBuilder b)
		{
			if (entryEmail.Text.Length == 0) {
				b.Append (Catalog.GetString ("You have to set the address.\n"));
			}

			if (b.Length == 0)
				return true;
			return false;
		}

		public void Commit (Charp.SuccessDelegate success, Charp.ErrorDelegate error, Gtk.Window parent) {
			string[] types = { "PERSONAL", "WORK" };
			string[] systems = { "STANDARD", "SKYPE" };

			object[] parms = new object[] {
				personaId,
				entryEmail.Text,
				types[comboType.Active],
				systems[comboSystem.Active],
				textRemarks.Buffer.Text
			};

			if (isNew ||
				(string) parms[1] != (string) myData["email"] ||
				(string) parms[2] != (string) myData["e_type"] ||
				(string) parms[3] != (string) myData["system"] ||
				(string) parms[4] != (string) myData["remarks"]) {

				string resource;
				if (isNew) {
					resource = "email_create";
				} else {
					resource = "email_update";
					parms[4] = emailId;
				}

				config.charp.request (resource, parms, new CharpGtk.CharpGtkCtx {
					parent = parent,
					success = success,
					error = error
				});
			}
		}
	}
}
