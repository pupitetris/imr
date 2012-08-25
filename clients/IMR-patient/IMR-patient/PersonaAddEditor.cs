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

		private void LoadAddresses () {
			config.charp.request ("persona_get_addresses", new object[] { myData["persona_id"] }, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				success = delegate (object data, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						ArrayList arr = (ArrayList) data;
						for (int i = 0; i < arr.Count; i++) {
							vboxAddress.Add (new AddressEditor (ParentWin, vboxAddress, (StringDictionary) arr[i]));
						}
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
				hboxFiscal.Show ();
			} else {
				hboxFiscal.Hide ();
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
	}
}

