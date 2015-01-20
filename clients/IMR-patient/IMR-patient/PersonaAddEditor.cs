using System;
using System.IO;
using System.Text;
using Mono.Unix;
using monoCharp;
using Newtonsoft.Json.Linq;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PersonaAddEditor : Gtk.Bin
	{
		private JObject myData;
		private int personaId;
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
				success = delegate (object data, Charp.CharpCtx ctx) {
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
				success = delegate (object data, Charp.CharpCtx ctx) {
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
				success = delegate (object data, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						foreach (JObject email in (JArray) data)
							AddEmailEditor (email);
					});
				}
			});
		}
		
		public void LoadData (JObject data) {
			myData = data;
			personaId = (int) myData["persona_id"];

			string val;

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

		public void SetPersonaId (int id) {
			personaId = id;
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

		public bool Validate (StringBuilder b) {
			foreach (AddressEditor editor in vboxAddress) {
				editor.Validate (b);
			}

			foreach (PhoneEditor editor in vboxPhone) {
				editor.Validate (b);
			}

			foreach (EmailEditor editor in vboxEmails) {
				editor.Validate (b);
			}

			if (b.Length == 0)
				return true;
			return false;
		}

		private void CommitEmails (System.Collections.IEnumerator e, Charp.SuccessDelegate success, Charp.ErrorDelegate error) {
			if (e.MoveNext ()) {
				EmailEditor editor = (EmailEditor) e.Current;
				editor.Commit (delegate (object data, Charp.CharpCtx ctx) {
					CommitEmails (e, success, error);
				}, error);
			} else {
				success (null, null);
			}
		}

		private void CommitPhones (System.Collections.IEnumerator e, Charp.SuccessDelegate success, Charp.ErrorDelegate error) {
			if (e.MoveNext ()) {
				PhoneEditor editor = (PhoneEditor) e.Current;
				editor.Commit (delegate (object data, Charp.CharpCtx ctx) {
					CommitPhones (e, success, error);
				}, error);
			} else {
				System.Collections.IEnumerator e2 = vboxEmails.GetEnumerator ();
				e2.Reset ();
				CommitEmails (e2, success, error);
			}
		}

		private void CommitAddresses (System.Collections.IEnumerator e, Charp.SuccessDelegate success, Charp.ErrorDelegate error) {
			if (e.MoveNext ()) {
				AddressEditor editor = (AddressEditor) e.Current;
				editor.Commit (delegate (object data, Charp.CharpCtx ctx) {
					CommitAddresses (e, success, error);
				}, error);
			} else {
				System.Collections.IEnumerator e2 = vboxPhone.GetEnumerator ();
				e2.Reset ();
				CommitPhones (e2, success, error);
			}
		}

		public void Commit (Charp.SuccessDelegate success, Charp.ErrorDelegate error) {
			foreach (AddressEditor editor in vboxAddress)
				editor.SetPersonaId (personaId);
			foreach (PhoneEditor editor in vboxPhone)
				editor.SetPersonaId (personaId);
			foreach (EmailEditor editor in vboxEmails)
				editor.SetPersonaId (personaId);

			System.Collections.IEnumerator e = vboxAddress.GetEnumerator ();
			e.Reset ();
			CommitAddresses (e, success, error);
		}
	}
}

