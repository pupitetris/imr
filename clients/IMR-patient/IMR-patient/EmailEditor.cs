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
			
			buttonDelete.ConfirmClick += OnDeleteConfirm;
			
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
				if (Util.DictTryValue (data, "type", out val)) {
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
			}
		}

		public void SetPersonaId (int id) {
			personaId = id;
		}

		private void OnDeleteConfirm (object sender, EventArgs e) 
		{
			if (isNew) {
				Cont.Remove (this);
				return;
			}

			config.charp.request ("email_delete", new object[] { personaId, emailId }, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				success = delegate (object data, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						Cont.Remove (this);
					});
				}
			});
		}

		public bool Validate (StringBuilder b)
		{
			if (entryEmail.Text.Length == 0) {
				b.Append (Catalog.GetString ("● You have to set the address.\n"));
			}

			if (b.Length == 0)
				return true;
			return false;
		}

		public void Commit (Charp.SuccessDelegate success, Charp.ErrorDelegate error) {
			string[] types = { "PERSONAL", "WORK" };
			string[] systems = { "STANDARD", "SKYPE" };

			object[] parms = {
				personaId,
				entryEmail.Text,
				types[comboType.Active],
				systems[comboSystem.Active],
				textRemarks.Buffer.Text
			};

			if (isNew ||
				(string) parms[1] != (string) myData["email"] ||
				(string) parms[2] != (string) myData["type"] ||
				(string) parms[3] != (string) myData["system"] ||
				!Util.StrEqNull ((string) parms[4], (string) myData["remarks"])) {

				string resource;
				if (isNew) {
					resource = "email_create";
				} else {
					resource = "email_update";
					parms = Util.ArrayUnshift (parms, emailId);
				}

				config.charp.request (resource, parms, new CharpGtk.CharpGtkCtx {
					asSingle = true,
					parent = ParentWin,
					success = delegate (object data, Charp.CharpCtx ctx) {
						LoadData ((JObject) data);
						success (null, null);
					},
					error = error
				});
			}
		}
	}
}
