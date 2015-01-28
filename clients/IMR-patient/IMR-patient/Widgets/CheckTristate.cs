using System;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CheckTristate : Gtk.CheckButton
	{
		private bool IgnoreChange = false;

		public CheckTristate ()
		{
			this.Build ();
		}

		public bool? TriState {
			set {
				if (value == null)
					Inconsistent = true;
				else {
					Inconsistent = false;
					if (value == true)
						Active = true;
					else
						Active = false;
				}
			}

			get {
				if (Inconsistent)
					return null;
				return Active;
			}
		}

		protected void OnTristateToggled (object sender, EventArgs e)
		{
			if (IgnoreChange)
				return;

			if (Inconsistent) {
				Inconsistent = false;
				IgnoreChange = true;
				Active = true;
				IgnoreChange = false;
				return;
			}

			if (Active) {
				Inconsistent = true;
			}
		}
	}
}

