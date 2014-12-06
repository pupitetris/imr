using System;
using System.IO;
using System.Net;
using Mono.Unix;
using monoCharp;
using Newtonsoft.Json.Linq;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PersonaAddEditor : Gtk.Bin
	{
		private JObject myData;
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

		private void AddAddressEditor (JObject data = null) {
			AddressEditor editor = new AddressEditor (config, ParentWin, vboxAddress, data);
			vboxAddress.Add (editor);
			Gtk.Box.BoxChild bc = (Gtk.Box.BoxChild) (vboxAddress[editor]);
			bc.Expand = false;
			bc.Fill = false;
		}

		private void AddPhoneEditor (JObject data = null) {
			PhoneEditor editor = new PhoneEditor (config, ParentWin, vboxPhone, data);
			vboxPhone.Add (editor);
			Gtk.Box.BoxChild bc = (Gtk.Box.BoxChild) (vboxPhone[editor]);
			bc.Expand = false;
			bc.Fill = false;
		}
		
		private void AddEmailEditor (JObject data = null) {
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
						foreach (JObject address in (JArray) data)
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
						foreach (JObject phone in (JArray) data)
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
						foreach (JObject email in (JArray) data)
							AddEmailEditor (email);
					});
				}
			});
		}
		
		public void LoadData (JObject data) {
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

