using System;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PersonaEditor : Gtk.Bin
	{
		private bool imageSet = false;

		public PersonaEditor ()
		{
			this.Build ();

			Gdk.Color white = new Gdk.Color (255, 255, 255);
			personaImgFrame.ModifyBg (Gtk.StateType.Prelight, white);
			personaImg.ModifyBg (Gtk.StateType.Prelight, white);
		}

		protected void OnRadioMaleToggled (object sender, EventArgs e)
		{
			if (!imageSet) {
				if (radioMale.Active) {
					personaImg.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.user_male.png");
				} else {
					personaImg.Pixbuf = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.user_female.png");
				}
			}
		}
	}
}

