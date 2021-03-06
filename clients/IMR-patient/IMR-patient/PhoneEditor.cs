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

			buttonDelete.ConfirmClick += OnDeleteConfirm;

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
				if (Util.DictTryValue (data, "numbr", out val)) { entryNumber.Text = val; }
				Util.GtkTextSetFromDict (textRemarks, data, "remarks");
				if (Util.DictTryValue (data, "type", out val)) {
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

			config.charp.request ("phone_delete", new object[] { personaId, phoneId }, new CharpGtk.CharpGtkCtx {
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
			if (entryNumber.Text.Length == 0) {
				b.Append (Catalog.GetString ("● You have to set a number.\n"));
			}

			if (b.Length == 0)
				return true;
			return false;
		}

		public void Commit (Charp.SuccessDelegate success, Charp.ErrorDelegate error) {
			string[] types = { "MOBILE", "HOME", "WORK", "NEXTEL" };

			object[] parms = {
				personaId,
				entryNumber.Text,
				types[comboType.Active],
				textRemarks.Buffer.Text
			};

			if (isNew ||
				(string) parms[1] != (string) myData["numbr"] ||
				(string) parms[2] != (string) myData["type"] ||
				!Util.StrEqNull ((string) parms[3], (string) myData["remarks"])) {

				string resource;
				if (isNew) {
					resource = "phone_create";
				} else {
					resource = "phone_update";
					parms = Util.ArrayUnshift (parms, phoneId);
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
			} else
				success (null, null);
		}
	}
}

