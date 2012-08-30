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
		}

		public void Setup (AppConfig config, Gtk.Window parent)
		{
			this.config = config;
			ParentWin = parent;
		}

		private void AddAddressEditor (StringDictionary data = null) {
			AddressEditor editor = new AddressEditor (config, ParentWin, vboxAddress, data);
			vboxAddress.Add (editor);
			Gtk.Box.BoxChild bc = (Gtk.Box.BoxChild) (vboxAddress[editor]);
			bc.Expand = false;
			bc.Fill = false;
		}

		private void AddPhoneEditor (StringDictionary data = null) {
			PhoneEditor editor = new PhoneEditor (config, ParentWin, vboxPhone, data);
			vboxPhone.Add (editor);
			Gtk.Box.BoxChild bc = (Gtk.Box.BoxChild) (vboxPhone[editor]);
			bc.Expand = false;
			bc.Fill = false;
		}
		
		private void AddEmailEditor (StringDictionary data = null) {
			EmailEditor editor = new EmailEditor (config, ParentWin, vboxEmails, data);
			vboxEmails.Add (editor);
			Gtk.Box.BoxChild bc = (Gtk.Box.BoxChild) (vboxEmails[editor]);
			bc.Expand = false;
			bc.Fill = false;
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
			config.charp.request ("persona_get_phones", new object[] { myData["persona_id"] }, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				success = delegate (object data, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						foreach (StringDictionary phone in (ArrayList) data)
							AddPhoneEditor (phone);
					});
				}
			});
		}
		
		private void LoadEmails () {
			config.charp.request ("persona_get_emails", new object[] { myData["persona_id"] }, new CharpGtk.CharpGtkCtx {
				parent = ParentWin,
				success = delegate (object data, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						foreach (StringDictionary email in (ArrayList) data)
							AddEmailEditor (email);
					});
				}
			});
		}
		
		public void LoadData (StringDictionary data) {
			myData = data;

			string val;

			if (Util.DictTryValue (data, "remarks", out val)) { 
				textNotes.Buffer.InsertAtCursor (val);
				expanderNotes.Expanded = true;
			} else {
				expanderNotes.Expanded = false;
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

		protected void OnButtonAddAddressClicked (object sender, EventArgs e)
		{
			AddAddressEditor ();
		}

		protected void OnButtonAddPhoneClicked (object sender, EventArgs e)
		{
			AddPhoneEditor ();
		}

		protected void OnButtonAddEmailClicked (object sender, EventArgs e)
		{
			AddEmailEditor ();
		}
	}
}

