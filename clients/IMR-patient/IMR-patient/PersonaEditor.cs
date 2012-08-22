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

