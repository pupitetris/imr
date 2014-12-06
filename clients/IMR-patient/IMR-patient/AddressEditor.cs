using System;
using System.Net;
using System.Collections.Generic;
using monoCharp;
using Mono.Unix;
using Newtonsoft.Json.Linq;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AddressEditor : Gtk.Bin
	{
		private static JArray states;

		private static int GlobalDefaultStateID = -1;
		private static int GlobalDefaultMuniID = -1;
		private static int GlobalDefaultAsentaID = -1;

		private AppConfig config;
		private Gtk.Window ParentWin;
		private Gtk.Container Cont;
		private JObject myData;

		private JArray zipcodes;
		private Dictionary<string, JObject> zipcodes_by_code;
		private JArray munis;
		private JArray asentas;
		private int DefaultStateID;
		private int DefaultMuniID;
		private int DefaultAsentaID;

		private MyCombo myComboState;
		private MyCombo myComboMuni;
		private MyCombo myComboAsenta;

		private string WidgetPath;

		public AddressEditor ()
		{
			this.Build ();
		}

		public AddressEditor (AppConfig config, Gtk.Window parent, Gtk.Container cont, JObject data = null)
		{
			this.Build ();

			this.config = config;
			ParentWin = parent;
			Cont = cont;

			myComboState = new MyCombo (comboState);
			myComboMuni = new MyCombo (comboMuni);
			myComboAsenta = new MyCombo (comboAsenta);

			buttonDelete.ConfirmClick += delegate (object sender, EventArgs e) { Cont.Remove (this); };

			WidgetPath = Util.GtkGetWidgetPath (this, config);

			if (GlobalDefaultStateID == -1)
				config.LoadWindowKey (WidgetPath, "default_state_id", out GlobalDefaultStateID);
			DefaultStateID = GlobalDefaultStateID;

			if (GlobalDefaultMuniID == -1)
				config.LoadWindowKey (WidgetPath, "default_muni_id", out GlobalDefaultMuniID);
			DefaultMuniID = GlobalDefaultMuniID;

			if (GlobalDefaultAsentaID == -1)
				config.LoadWindowKey (WidgetPath, "default_asenta_id", out GlobalDefaultAsentaID);
			DefaultAsentaID = GlobalDefaultAsentaID;
			
			LoadData (data);
		}

		private void PopulateStates ()
		{
			foreach (JObject state in states)
				myComboState.AppendText ((string) state["st_name"], state);

			int state_id = myData["state_id"] != null? (int) myData["state_id"]: DefaultStateID;
			myComboState.SetActiveByData (delegate(object obj) {
				return ((int) ((JObject) obj)["state_id"] == state_id);
			});

			GLib.Signal.Emit (Cont, "check-resize");
		}

		public void LoadData (JObject data)
		{
			if (data != null) {
				myData = data;

				string val;
				if (Util.DictTryValue (data, "street", out val)) { entryStreet.Text = val; }
				if (Util.DictTryValue (data, "ad_type", out val)) {
					int active;
					switch (val) {
						case "HOME": active = 0; break;
						case "WORK": active = 1; break;
						case "FISCAL": active = 2; break;
						default: active = -1; break;
					}
					comboType.Active = active;
				}
			} else
				myData = new JObject ();

			if (states != null) {
				PopulateStates ();
				return;
			}

			config.charp.request ("get_states_by_inst", null, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				useCache = true,
				success = delegate (object dat, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						states = (JArray) dat;
						PopulateStates ();
					});
				}
			});
		}

		protected void OnComboStateChanged (object sender, EventArgs e)
		{
			labelMuni.Sensitive = false;
			comboMuni.Sensitive = false;
			comboMuni.Active = -1;
			entryZipcode.Text = "";

			if (comboState.Active < 0) {
				labelZipcode.Sensitive = false;
				entryZipcode.Sensitive = false;
				return;
			}

			labelZipcode.Sensitive = true;
			entryZipcode.Sensitive = true;

			JObject state = (JObject) myComboState.ActiveData ();
			int state_id = state != null? (int) state["state_id"]: -1;
			if (state_id != GlobalDefaultStateID) {
				config.SaveWindowKey (WidgetPath, "default_state_id", state_id);
				GlobalDefaultStateID = state_id;
			}

			config.charp.request ("get_zipcodes_by_state", new object [] { state_id }, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				useCache = true,
				asAnon = true,
				success = delegate (object dat, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						zipcodes = (JArray) dat;

						zipcodes_by_code = new Dictionary<string, JObject> ();
						foreach (JObject zipcode in zipcodes)
							zipcodes_by_code[(string) zipcode["z_code"]] = zipcode;
					});
				}
			});

			config.charp.request ("get_munis_by_state", new object [] { state_id }, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				useCache = true,
				asAnon = true,
				success = delegate (object dat, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						munis = (JArray) dat;

						myComboMuni.Clear ();
						foreach (JObject muni in munis)
							myComboMuni.AppendText ((string) muni["m_name"], muni);

						labelMuni.Sensitive = true;
						comboMuni.Sensitive = true;
						if (state_id != -1 && state_id == DefaultStateID)
							myComboMuni.SetActiveByData (delegate(object obj) {
								return ((int) ((JObject) obj)["muni_id"] == DefaultMuniID);
							});

						GLib.Signal.Emit (Cont, "check-resize");
					});
				}
			});
		}

		protected void OnComboMuniChanged (object sender, EventArgs e)
		{
			labelAsenta.Sensitive = false;
			comboAsenta.Sensitive = false;
			comboAsenta.Active = -1;

			if (comboMuni.Active < 0) {
				return;
			}

			JObject muni = (JObject) myComboMuni.ActiveData ();
			int muni_id = muni != null? (int) muni["muni_id"]: -1;
			if (muni_id != GlobalDefaultMuniID) {
				config.SaveWindowKey (WidgetPath, "default_muni_id", muni_id);
				GlobalDefaultMuniID = muni_id;
			}

			config.charp.request ("get_asentas_by_muni", new object [] { muni_id }, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				useCache = true,
				asAnon = true,
				success = delegate (object dat, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						asentas = (JArray) dat;

						string z_code = entryZipcode.Text;
						if (!zipcodes_by_code.ContainsKey (z_code))
							z_code = null;

						myComboAsenta.Clear ();
						foreach (JObject asenta in asentas) {
							if (z_code != null && (string) asenta["z_code"] == z_code)
								myComboAsenta.PrependText ((string) asenta["fullname"], asenta);
							else
								myComboAsenta.AppendText ((string) asenta["fullname"], asenta);
						}

						labelAsenta.Sensitive = true;
						comboAsenta.Sensitive = true;

						if (z_code != null && (int) zipcodes_by_code[z_code]["muni_id"] == muni_id)
							comboAsenta.Active = 0;
						else {
							entryZipcode.Text = "";
							if (muni_id != -1 && muni_id == DefaultMuniID)
								myComboAsenta.SetActiveByData (delegate(object obj) {
									return ((int) ((JObject) obj)["asenta_id"] == DefaultAsentaID);
								});
						}

						GLib.Signal.Emit (Cont, "check-resize");
					});
				}
			});
		}

		protected void OnComboAsentaChanged (object sender, EventArgs e)
		{
			if (comboAsenta.Active < 0) {
				labelStreet.Sensitive = false;
				entryStreet.Sensitive = false;
				return;
			}

			labelStreet.Sensitive = true;
			entryStreet.Sensitive = true;

			JObject asenta = (JObject) myComboAsenta.ActiveData ();
			int asenta_id = asenta != null? (int) asenta["asenta_id"]: -1;
			if (asenta_id != GlobalDefaultAsentaID) {
				config.SaveWindowKey (WidgetPath, "default_asenta_id", asenta_id);
				GlobalDefaultAsentaID = asenta_id;
			}

			entryZipcode.Text = (string) asenta["z_code"];
		}

		private void CheckZipcodeText ()
		{
			string z_code = entryZipcode.Text;

			if (zipcodes_by_code.ContainsKey (z_code)) {
				JObject asenta = (JObject) myComboAsenta.ActiveData ();
				if (asenta == null || (string) asenta["z_code"] != z_code) {
					comboMuni.Active = -1;
					JObject zipcode = zipcodes_by_code[z_code];
					myComboMuni.SetActiveByData (delegate(object obj) {
						return ((int) ((JObject) obj)["muni_id"] == (int) zipcode["muni_id"]);
					});
				}
				Util.GtkLabelStyleRemove (labelZipcode);
				return;
			}

			Util.GtkLabelStyleAsError (labelZipcode);
		}

		protected void OnEntryZipcodeBackspace (object sender, EventArgs e)
		{
			CheckZipcodeText ();
		}

		protected void OnEntryZipcodeTextInserted (object o, Gtk.TextInsertedArgs args)
		{
			CheckZipcodeText ();
		}
	}
}
