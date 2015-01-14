using System;
using System.Text;
using Mono.Unix;
using monoCharp;
using Newtonsoft.Json.Linq;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PhoneEditor : Gtk.Bin
	{
		private AppConfig config;
		private Gtk.Window ParentWin;
		private Gtk.Container Cont;
		private JObject myData;
		private int phoneId = 0;
		private int personaId = 0;
		private bool isNew = true;
		
		public PhoneEditor ()
		{
			this.Build ();
		}

		public PhoneEditor (AppConfig config, Gtk.Window parent, Gtk.Container cont, JObject data = null) {
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
				phoneId = (int) myData["phone_id"];
				personaId = (int) myData["persona_id"];
				isNew = false;

				string val;
				if (Util.DictTryValue (data, "number", out val)) { entryNumber.Text = val; }
				if (Util.DictTryValue (data, "remarks", out val)) { textRemarks.Buffer.InsertAtCursor (val); }
				if (Util.DictTryValue (data, "p_type", out val)) {
					int active;
					switch (val) {
						case "MOBILE": active = 0; break;
						case "HOME": active = 1; break;
						case "WORK": active = 2; break;
						case "NEXTEL": active = 3; break;
						default: active = -1; break;
					}
					comboType.Active = active;
				}
			} else
				myData = new JObject ();
		}

		public void SetPersonaId (int id) {
			personaId = id;
		}
			
		public bool Validate (StringBuilder b)
		{
			if (entryNumber.Text.Length == 0) {
				b.Append (Catalog.GetString ("You have to set a number.\n"));
			}

			if (b.Length == 0)
				return true;
			return false;
		}

		public void Commit (Charp.SuccessDelegate success, Charp.ErrorDelegate error, Gtk.Window parent) {
			string[] types = { "MOBILE", "NEXTEL", "HOME", "WORK" };

			object[] parms = new object[] {
				personaId,
				entryNumber.Text,
				types[comboType.Active],
				textRemarks.Buffer.Text
			};

			if (isNew ||
				(string) parms[1] != (string) myData["number"] ||
				(string) parms[2] != (string) myData["p_type"] ||
				(string) parms[3] != (string) myData["remarks"]) {

				string resource;
				if (isNew) {
					resource = "phone_create";
				} else {
					resource = "phone_update";
					parms[4] = phoneId;
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

