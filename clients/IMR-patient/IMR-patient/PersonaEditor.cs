using System;
using System.IO;
using System.Net;
using Mono.Unix;
using monoCharp;
using Newtonsoft.Json.Linq;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PersonaEditor : Gtk.Bin
	{
		private bool imageSet = false;
		private bool imageChanged = false;
		private JObject myData;
		private AppConfig config;
		private Gtk.Window ParentWin;
		private Gdk.Pixbuf pixbufOrig;
		public string pictureFolder;

		private static int PICTURE_WIDTH = 128;
		private static int PICTURE_HEIGHT = 128;

		public PersonaEditor ()
		{
			this.Build ();
		}

		public void Setup (AppConfig config, Gtk.Window parent)
		{
			this.config = config;
			this.ParentWin = parent;
		}

		private void LoadPicture (string hash) {
			imagePicture.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.image_loading.png");
			imageSet = true;
			
			string fname = myData["persona_id"] + "_" + hash + ".jpg";
			config.pcache.LoadFile (ParentWin, fname, "file_persona_get_photo", new object[] { myData["persona_id"] }, 
				delegate (Stream stream) {
					Gtk.Application.Invoke (delegate {
						if (stream == null) {
							imagePicture.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.image_error.png");
						} else {
							try {
								imagePicture.Pixbuf = new Gdk.Pixbuf (stream);
							} catch {
								imagePicture.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.image_error.png");
							}
							stream.Close ();
						}
					});
				});
		}
		
		public void LoadData (JObject data) {
			myData = data;

			string val;
			if (Util.DictTryValue (data, "prefix", out val)) { entryPrefix.Text = val; }
			if (Util.DictTryValue (data, "name", out val)) { entryName.Text = val; }
			if (Util.DictTryValue (data, "paterno", out val)) { entryPaterno.Text = val; }
			if (Util.DictTryValue (data, "materno", out val)) { entryMaterno.Text = val; }
			if (Util.DictTryValue (data, "picture", out val)) { LoadPicture (val); }
			if (Util.DictTryValue (data, "gender", out val)) { SetGender (val); }

			if (Util.DictTryValue (data, "birth", out val)) {
				entryBirth.Text = val;
				hboxBirth.Show ();
			} else {
				hboxBirth.Hide ();
			}
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

		protected void OnButtonPictureClicked (object sender, EventArgs e)
		{
			Gtk.FileChooserDialog dlg = 
				new Gtk.FileChooserDialog (Catalog.GetString ("Select Image"), ParentWin, Gtk.FileChooserAction.Open,
				                           Catalog.GetString ("Cancel"), Gtk.ResponseType.Cancel,
				                           Catalog.GetString ("Open"), Gtk.ResponseType.Accept);
			dlg.Modal = true;
			dlg.TransientFor = ParentWin;

			int res;
			do {
				if (pictureFolder != null)
					dlg.SetCurrentFolder (pictureFolder);

				res = dlg.Run ();
				if (res == (int) Gtk.ResponseType.Cancel)
					break;

				pictureFolder = dlg.CurrentFolder;

				try {
					Gdk.Pixbuf load = new Gdk.Pixbuf (dlg.Filename);
					Gdk.Pixbuf orig = load.ApplyEmbeddedOrientation ();
					Gdk.Pixbuf big;

					int w = orig.Width;
					int h = orig.Height;
					if (w == h) {
						big = orig;
					} else if (w > h) {
						big = new Gdk.Pixbuf (orig, (w - h) / 2, 0, h, h);
					} else {
						big = new Gdk.Pixbuf (orig, 0, (h - w) / 2, w, w);
					}

					imagePicture.Pixbuf = big.ScaleSimple (PICTURE_WIDTH, PICTURE_HEIGHT, Gdk.InterpType.Hyper);
					pixbufOrig = orig;
					imageChanged = true;
				} catch (GLib.GException) {
					Gtk.MessageDialog msg = 
						new Gtk.MessageDialog (ParentWin, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok,
						                       Catalog.GetString ("Error opening image"));
					msg.Title = "Error";
					msg.Run ();
					msg.Destroy ();
					imagePicture.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.image_unknown.png");
					imageChanged = false;
					continue;
				}
			} while (res != (int) Gtk.ResponseType.Accept);
			dlg.Destroy ();
		}
	}
}

