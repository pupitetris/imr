using System;
using System.Net;
using System.Collections;
using System.Collections.Specialized;
using monoCharp;
using Mono.Unix;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AddressEditor : Gtk.Bin
	{
		public delegate void StateComboChangedDelegate (string state_id);

		private static uint DELETE_CONFIRM_TIMEOUT = 2000;

		public StateComboChangedDelegate StateComboChanged;

		private AppConfig config;
		private Gtk.Window ParentWin;
		private Gtk.Container Cont;
		private StringDictionary myData;
		private ArrayList states;
		private ArrayList zipcodes;
		private ArrayList counties;
		private ArrayList asentas;
		private ArrayList streets;
		private string DefaultSateID;
		private bool DeleteConfirm = false;

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

			LoadData (data);
		}

		public void LoadData (StringDictionary data)
		{
			myData = (data != null)? data: new StringDictionary ();

			config.charp.request ("get_states_by_inst", null, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				useCache = true,
				success = delegate (object dat, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						states = (ArrayList) dat;
						foreach (StringDictionary state in states) {
							comboState.AppendText (state["st_name"]);
						}
						Util.GtkComboActiveFromData (comboState, states, "state_id", 
						                             (data == null)? DefaultSateID: data["state_id"]);
						GLib.Signal.Emit (Cont, "check-resize");
					});
				}
			});
		}

		private void FieldsSetSensitive (bool sensitive)
		{
			labelZipcode.Sensitive = sensitive;
			labelCounty.Sensitive = sensitive;
			labelAsenta.Sensitive = sensitive;
			comboZipcode.Sensitive = sensitive;
			comboCounty.Sensitive = sensitive;
			comboAsenta.Sensitive = sensitive;
		}

		public void StateComboSetDefault (string state_id)
		{
			DefaultSateID = state_id;
			Util.GtkComboActiveFromData (comboState, states, "state_id", state_id);
		}

		protected void OnComboStateChanged (object sender, EventArgs e)
		{
			if (comboState.Active == 0) {
				FieldsSetSensitive (false);
				return;
			}

			FieldsSetSensitive (true);

			string state_id = ((StringDictionary) states[comboState.Active - 1])["state_id"];
			StateComboChanged (state_id);
			config.charp.request ("get_zipcodes_by_state", new object [] { state_id }, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				useCache = true,
				success = delegate (object dat, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						zipcodes = (ArrayList) dat;
						Util.GtkComboClear (comboZipcode);
						foreach (StringDictionary zipcode in zipcodes) {
							comboZipcode.AppendText (zipcode["z_code"]);
						}
					});
				}
			});
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
	}
}

