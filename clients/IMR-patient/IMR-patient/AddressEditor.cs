using System;
using System.Net;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using monoCharp;
using Mono.Unix;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AddressEditor : Gtk.Bin
	{
		private static ArrayList states;

		private static string GlobalDefaultStateID;
		private static string GlobalDefaultMuniID;
		private static string GlobalDefaultAsentaID;

		private AppConfig config;
		private Gtk.Window ParentWin;
		private Gtk.Container Cont;
		private StringDictionary myData;

		private ArrayList zipcodes;
		private Dictionary<string, StringDictionary> zipcodes_by_code;
		private ArrayList munis;
		private ArrayList asentas;
		private string DefaultStateID;
		private string DefaultMuniID;
		private string DefaultAsentaID;

		private MyCombo myComboState;
		private MyCombo myComboMuni;
		private MyCombo myComboAsenta;

		private string WidgetPath;

		public AddressEditor ()
		{
			this.Build ();
		}

		public AddressEditor (AppConfig config, Gtk.Window parent, Gtk.Container cont, StringDictionary data = null)
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

			if (GlobalDefaultStateID == null)
				config.LoadWindowKey (WidgetPath, "default_state_id", out GlobalDefaultStateID);
			DefaultStateID = GlobalDefaultStateID;

			if (GlobalDefaultMuniID == null)
				config.LoadWindowKey (WidgetPath, "default_muni_id", out GlobalDefaultMuniID);
			DefaultMuniID = GlobalDefaultMuniID;

			if (GlobalDefaultAsentaID == null)
				config.LoadWindowKey (WidgetPath, "default_asenta_id", out GlobalDefaultAsentaID);
			DefaultAsentaID = GlobalDefaultAsentaID;
			
			LoadData (data);
		}

		private void PopulateStates ()
		{
			foreach (StringDictionary state in states)
				myComboState.AppendText (state["st_name"], state);

			string state_id = myData.ContainsKey ("state_id")? myData["state_id"]: DefaultStateID;
			myComboState.SetActiveByData (delegate(object obj) {
				return (((StringDictionary) obj)["state_id"] == state_id);
			});

			GLib.Signal.Emit (Cont, "check-resize");
		}

		public void LoadData (StringDictionary data)
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
				myData = new StringDictionary ();

			if (states != null) {
				PopulateStates ();
				return;
			}

			config.charp.request ("get_states_by_inst", null, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				useCache = true,
				success = delegate (object dat, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						states = (ArrayList) dat;
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

			StringDictionary state = (StringDictionary) myComboState.ActiveData ();
			string state_id = state != null? state["state_id"]: null;
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
						zipcodes = (ArrayList) dat;

						zipcodes_by_code = new Dictionary<string, StringDictionary> ();
						foreach (StringDictionary zipcode in zipcodes)
							zipcodes_by_code[zipcode["z_code"]] = zipcode;
					});
				}
			});

			config.charp.request ("get_munis_by_state", new object [] { state_id }, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				useCache = true,
				asAnon = true,
				success = delegate (object dat, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						munis = (ArrayList) dat;

						myComboMuni.Clear ();
						foreach (StringDictionary muni in munis)
							myComboMuni.AppendText (muni["m_name"], muni);

						labelMuni.Sensitive = true;
						comboMuni.Sensitive = true;
						if (state_id != null && state_id == DefaultStateID)
							myComboMuni.SetActiveByData (delegate(object obj) {
								return (((StringDictionary) obj)["muni_id"] == DefaultMuniID);
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

			StringDictionary muni = (StringDictionary) myComboMuni.ActiveData ();
			string muni_id = muni != null? muni["muni_id"]: null;
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
						asentas = (ArrayList) dat;

						string z_code = entryZipcode.Text;
						if (!zipcodes_by_code.ContainsKey (z_code))
							z_code = null;

						myComboAsenta.Clear ();
						foreach (StringDictionary asenta in asentas) {
							if (z_code != null && asenta["z_code"] == z_code)
								myComboAsenta.PrependText (asenta["fullname"], asenta);
							else
								myComboAsenta.AppendText (asenta["fullname"], asenta);
						}

						labelAsenta.Sensitive = true;
						comboAsenta.Sensitive = true;

						if (z_code != null && zipcodes_by_code[z_code]["muni_id"] == muni_id)
							comboAsenta.Active = 0;
						else {
							entryZipcode.Text = "";
							if (muni_id != null && muni_id == DefaultMuniID)
								myComboAsenta.SetActiveByData (delegate(object obj) {
									return (((StringDictionary) obj)["asenta_id"] == DefaultAsentaID);
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

			StringDictionary asenta = (StringDictionary) myComboAsenta.ActiveData ();
			string asenta_id = asenta != null? asenta["asenta_id"]: null;
			if (asenta_id != GlobalDefaultAsentaID) {
				config.SaveWindowKey (WidgetPath, "default_asenta_id", asenta_id);
				GlobalDefaultAsentaID = asenta_id;
			}

			entryZipcode.Text = asenta["z_code"];
		}

		private void CheckZipcodeText ()
		{
			string z_code = entryZipcode.Text;

			if (zipcodes_by_code.ContainsKey (z_code)) {
				StringDictionary asenta = (StringDictionary) myComboAsenta.ActiveData ();
				if (asenta == null || asenta["z_code"] != z_code) {
					comboMuni.Active = -1;
					StringDictionary zipcode = zipcodes_by_code[z_code];
					myComboMuni.SetActiveByData (delegate(object obj) {
						return (((StringDictionary) obj)["muni_id"] == zipcode["muni_id"]);
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
