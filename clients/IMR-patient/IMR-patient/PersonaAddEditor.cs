using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Specialized;
using Mono.Unix;
using monoCharp;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PersonaAddEditor : Gtk.Bin
	{
		private StringDictionary myData;
		private AppConfig config;
		private Gtk.Window ParentWin;
		private string DefaultAddressState;

		public PersonaAddEditor ()
		{
			this.Build ();

			textNotes.Hide ();
		}

		public void Setup (AppConfig config, Gtk.Window parent)
		{
			this.config = config;
			ParentWin = parent;
		}

		private void AddAddressEditor (StringDictionary data = null) {
			AddressEditor editor = new AddressEditor (config, ParentWin, vboxAddress, data);

			string path = Util.GtkGetWidgetPath (editor, config);
			editor.StateComboChanged += delegate (string state_id) {
				if (state_id != DefaultAddressState) {
					config.SaveWindowKey (path, "default_state_id", state_id);
					DefaultAddressState = state_id;
				}
			};

			if (DefaultAddressState == null)
				config.LoadWindowKey (path, "default_state_id", out DefaultAddressState);
			editor.StateComboSetDefault (DefaultAddressState);

			vboxAddress.Add (editor);
		}

		private void LoadAddresses () {
			config.charp.request ("persona_get_addresses", new object[] { myData["persona_id"] }, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				success = delegate (object data, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						foreach (StringDictionary address in (ArrayList) data)
							AddAddressEditor (address);
					});
				}
			});
		}

		private void LoadPhones () {
			
		}
		
		private void LoadEmails () {
			
		}
		
		public void LoadData (StringDictionary data) {
			myData = data;

			string val;

			if (Util.DictTryValue (data, "remarks", out val)) { 
				textNotes.Buffer.InsertAtCursor (val);
				expanderNotes.Expanded = true;
				textNotes.Show ();
			} else {
				expanderNotes.Expanded = false;
				textNotes.Hide ();
			}

			if (Util.DictTryValue (data, "fiscal_code", out val)) {
				entryFiscal.Text = val;
				contFiscal.Show ();
			} else {
				contFiscal.Hide ();
			}

			LoadAddresses ();
			LoadPhones ();
			LoadEmails ();
		}

		protected void OnExpanderNotesActivated (object sender, EventArgs e)
		{
			if (expanderNotes.Expanded) {
				textNotes.Show ();
			} else {
				textNotes.Hide ();
			}
		}

		protected void OnButtonAddAddressClicked (object sender, EventArgs e)
		{
			AddAddressEditor ();
		}
	}
}

