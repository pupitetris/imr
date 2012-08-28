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
		private static Dictionary<string, StringDictionary> zipcodes_by_code;

		private static string GlobalDefaultStateID;
		private static string GlobalDefaultMuniID;
		private static string GlobalDefaultAsentaID;

		private AppConfig config;
		private Gtk.Window ParentWin;
		private Gtk.Container Cont;
		private StringDictionary myData;

		private ArrayList zipcodes;
		private ArrayList munis;
		private ArrayList asentas;
		private string DefaultStateID;
		private string DefaultMuniID;
		private string DefaultAsentaID;

		private MyCombo myComboState;
		private MyCombo myComboMuni;
		private MyCombo myComboAsenta;

		private string WidgetPath;

		static AddressEditor ()
		{
			zipcodes_by_code = new Dictionary<string, StringDictionary> ();
		}

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
				myComboState.AppendText (state["st_name"]);

			Util.GtkComboActiveFromData (comboState, states, "state_id", 
			                             myData.ContainsKey ("state_id")? myData["state_id"]: DefaultStateID, 1);
			GLib.Signal.Emit (Cont, "check-resize");
		}

		public void LoadData (StringDictionary data)
		{
			myData = (data != null) ? data : new StringDictionary ();

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

		private void FieldsSetSensitive (bool sensitive)
		{
			labelZipcode.Sensitive = sensitive;
			labelMuni.Sensitive = sensitive;
			entryZipcode.Sensitive = sensitive;
			comboMuni.Sensitive = sensitive;
		}
		
		protected void OnComboStateChanged (object sender, EventArgs e)
		{
			if (comboState.Active < 1) {
				FieldsSetSensitive (false);
				return;
			}

			FieldsSetSensitive (true);

			string state_id = ((StringDictionary)states [comboState.Active - 1]) ["state_id"];
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

						if (zipcodes_by_code.ContainsKey (((StringDictionary) zipcodes[0])["z_code"]))
							return;

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
							myComboMuni.AppendText (muni["m_name"]);

						if (state_id == DefaultStateID)
							Util.GtkComboActiveFromData (comboMuni, munis, "muni_id", DefaultMuniID);

						GLib.Signal.Emit (Cont, "check-resize");
					});
				}
			});
		}

		protected void OnComboMuniChanged (object sender, EventArgs e)
		{
			if (comboMuni.Active < 0)
				return;

			labelAsenta.Sensitive = true;
			comboAsenta.Sensitive = true;

			string muni_id = ((StringDictionary)munis [comboMuni.Active]) ["muni_id"];
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
						
						myComboAsenta.Clear ();
						foreach (StringDictionary asenta in asentas)
							myComboAsenta.AppendText (asenta["fullname"]);
						
						if (muni_id == DefaultMuniID)
							Util.GtkComboActiveFromData (comboAsenta, asentas, "asenta_id", DefaultAsentaID);
						
						GLib.Signal.Emit (Cont, "check-resize");
					});
				}
			});
		}

		protected void OnComboAsentaChanged (object sender, EventArgs e)
		{
			if (comboAsenta.Active < 0)
				return;

			labelStreet.Sensitive = true;
			entryStreet.Sensitive = true;

			string asenta_id = ((StringDictionary)asentas [comboAsenta.Active]) ["asenta_id"];
			if (asenta_id != GlobalDefaultAsentaID) {
				config.SaveWindowKey (WidgetPath, "default_asenta_id", asenta_id);
				GlobalDefaultAsentaID = asenta_id;
			}
		}
	}
}

