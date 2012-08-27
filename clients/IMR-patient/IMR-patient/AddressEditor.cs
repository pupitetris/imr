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
		public delegate void StateComboChangedDelegate (string state_id);

		private static uint DELETE_CONFIRM_TIMEOUT = 2000;
		private static ArrayList states;
		private static Dictionary<string, StringDictionary> zipcodes_by_code;

		public StateComboChangedDelegate StateComboChanged;
		public StateComboChangedDelegate MuniComboChanged;
		public StateComboChangedDelegate AsentaComboChanged;

		private AppConfig config;
		private Gtk.Window ParentWin;
		private Gtk.Container Cont;
		private StringDictionary myData;
		private bool DeleteConfirm = false;

		private ArrayList zipcodes;
		private ArrayList munis;
		private ArrayList asentas;
		private ArrayList streets;
		private string DefaultStateID;
		private string DefaultMuniID;
		private string DefaultAsentaID;

		private MyCombo myComboState;
		private MyCombo myComboMuni;
		private MyCombo myComboAsenta;

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

			LoadData (data);
		}

		private bool ButtonDeleteRevert ()
		{
			DeleteConfirm = false;
			imageButtonDelete.Pixbuf = Stetic.IconLoader.LoadIcon (this, "gtk-remove", Gtk.IconSize.Menu);
			return false;
		}
		
		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			if (!DeleteConfirm) {
				DeleteConfirm = true;
				imageButtonDelete.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.trash_delete.png");
				GLib.Timeout.Add (DELETE_CONFIRM_TIMEOUT, this.ButtonDeleteRevert);
			} else {
				Cont.Remove (this);
			}
		}
		
		public void StateComboSetDefault (string state_id)
		{
			DefaultStateID = state_id;
			Util.GtkComboActiveFromData (comboState, states, "state_id", state_id, 1);
		}
		
		public void MuniComboSetDefault (string muni_id)
		{
			DefaultMuniID = muni_id;
			Util.GtkComboActiveFromData (comboMuni, munis, "muni_id", muni_id);
		}
		
		public void AsentaComboSetDefault (string asenta_id)
		{
			DefaultAsentaID = asenta_id;
			Util.GtkComboActiveFromData (comboAsenta, asentas, "asenta_id", asenta_id);
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

			string state_id = ((StringDictionary) states[comboState.Active - 1])["state_id"];
			StateComboChanged (state_id);

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

			string muni_id = ((StringDictionary) munis[comboMuni.Active])["muni_id"];
			MuniComboChanged (muni_id);

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

			string asenta_id = ((StringDictionary) asentas[comboAsenta.Active])["asenta_id"];
			AsentaComboChanged (asenta_id);
		}
	}
}

