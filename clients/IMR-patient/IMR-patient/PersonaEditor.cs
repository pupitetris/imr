using System;
using System.IO;
using System.Collections.Generic;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PersonaEditor : Gtk.Bin
	{
		private bool imageSet = false;
		private Dictionary<string, string> myData;
		private AppConfig config;
		private Gtk.Window parent;

		public PersonaEditor ()
		{
			this.Build ();
		}

		private void LoadPicture (string hash) {
			imagePicture.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.image_loading.png");
			imageSet = true;

			string fname = myData["persona_id"] + "_" + hash;
			config.pcache.LoadFile (parent, fname, delegate (Stream stream) {
				Gtk.Application.Invoke (delegate {
					if (stream == null) {
						imagePicture.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.image_error.png");
					} else {
						imagePicture.Pixbuf = new Gdk.Pixbuf (stream);
						stream.Close ();
					}
				});
			});
		}

		public void Setup (AppConfig config, Gtk.Window parent)
		{
			this.config = config;
			this.parent = parent;
		}

		public void SetData (Dictionary<string, string> data) {
			myData = data;

			string val;
			if (Util.DictTryValue (data, "prefix", out val)) { entryPrefix.Text = val; }
			if (Util.DictTryValue (data, "name", out val)) { entryName.Text = val; }
			if (Util.DictTryValue (data, "paterno", out val)) { entryPaterno.Text = val; }
			if (Util.DictTryValue (data, "materno", out val)) { entryMaterno.Text = val; }
			if (Util.DictTryValue (data, "picture", out val)) { LoadPicture (val); }
			if (Util.DictTryValue (data, "gender", out val)) { SetGender (val); }
			if (Util.DictTryValue (data, "remarks", out val)) { textNotes.Buffer.InsertAtCursor (val); }
		}

		private void SetImageByGender (string gender)
		{
			if (gender == "MALE") {
				imagePicture.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.user_male.png");
			} else {
				imagePicture.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.user_female.png");
			}
		}

		private void SetGender (string gender)
		{
			if (gender == "MALE") {
				radioMale.Active = true;
			} else {
				radioFemale.Active = true;
			}

			if (!imageSet) {
				SetImageByGender (gender);
			}
		}

		protected void OnRadioMaleToggled (object sender, EventArgs e)
		{
			if (!imageSet) {
				SetImageByGender (radioMale.Active? "MALE": "FEMALE");
			}
		}
	}
}

