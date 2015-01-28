using System;
using System.IO;
using System.Net;
using System.Text;
using Mono.Unix;
using monoCharp;
using Newtonsoft.Json.Linq;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PersonaEditor : Gtk.Bin
	{
		private bool imageSet = false;
		private bool photoChanged = false;
		private bool isNew = true;
		private JObject myData;
		private string filename;
		private int personaId = 0;
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

		private void LoadPicture (string fname) {
			imagePicture.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.image_loading.png");
			imageSet = true;
			
			config.pcache.LoadFile (ParentWin, "thumb_" + fname, "file_persona_get_photo", new object[] { myData["persona_id"], true }, 
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
			personaId = (int) myData["persona_id"];
			isNew = false;

			string val;
			if (Util.DictTryValue (data, "prefix", out val)) { entryPrefix.Text = val; }
			if (Util.DictTryValue (data, "name", out val)) { entryName.Text = val; }
			if (Util.DictTryValue (data, "paterno", out val)) { entryPaterno.Text = val; }
			if (Util.DictTryValue (data, "materno", out val)) { entryMaterno.Text = val; }
			if (Util.DictTryValue (data, "photo", out val)) { LoadPicture (val); }
			if (Util.DictTryValue (data, "gender", out val)) { SetGender (val); }

			if (Util.GtkTextSetFromDict (textNotes, data, "remarks"))
				expanderNotes.Expanded = true;
			else
				expanderNotes.Expanded = false;
		}

		public void SetPersonaId (int id) {
			personaId = id;
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
				filename = dlg.Filename;

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
					photoChanged = true;
				} catch (GLib.GException) {
					Gtk.MessageDialog msg = 
						new Gtk.MessageDialog (ParentWin, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Ok,
						                       Catalog.GetString ("Error opening image"));
					msg.Title = "Error";
					msg.Run ();
					msg.Destroy ();
					imagePicture.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.image_unknown.png");
					photoChanged = false;
					continue;
				}
			} while (res != (int) Gtk.ResponseType.Accept);
			dlg.Destroy ();
		}

		public bool Validate (StringBuilder b) {
			if (entryName.Text.Length == 0) {
				b.Append (Catalog.GetString ("● You have to set at least a name.\n"));
				Util.GtkLabelStyleAsError (labelName);
			} else {
				Util.GtkLabelStyleRemove (labelName);
			}

			if (b.Length == 0)
				return true;
			return false;
		}

		private void CommitPersonaSuccess (Charp.SuccessDelegate success, Charp.ErrorDelegate error) {
			if (photoChanged) {
				config.charp.request ("persona_add_photo", new object[] { personaId }, new CharpGtk.CharpGtkCtx {
					parent = ParentWin,
					success = success,
					error = error,
					fileName = filename
				});
			} else
				success (null, null);
		}

		public void Commit (Charp.SuccessDelegate success, Charp.ErrorDelegate error) {
			object[] parms = {
				personaId,
				entryPrefix.Text,
				entryName.Text,
				entryPaterno.Text,
				entryMaterno.Text,
				radioMale.Active? "MALE" : "FEMALE",
				textNotes.Buffer.Text
			};

			if (isNew ||
				!Util.StrEqNull ((string) parms[1], (string) myData["prefix"])  ||
				!Util.StrEqNull ((string) parms[2], (string) myData["name"])    ||
				!Util.StrEqNull ((string) parms[3], (string) myData["paterno"]) ||
				!Util.StrEqNull ((string) parms[4], (string) myData["materno"]) ||
				!Util.StrEqNull ((string) parms[5], (string) myData["gender"])  ||
				!Util.StrEqNull ((string) parms[6], (string) myData["remarks"])) {

				config.charp.request ("persona_update", parms, new CharpGtk.CharpGtkCtx {
					asSingle = true,
					parent = ParentWin,
					success = delegate (object data, Charp.CharpCtx ctx) {
						LoadData ((JObject) data);
						CommitPersonaSuccess (success, error);
					},
					error = error
				});
			} else {
				CommitPersonaSuccess (success, error);
			}
		}
	}
}

