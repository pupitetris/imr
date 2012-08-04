using System;

namespace IMRpatient
{
	public partial class AboutDlg : Gtk.Dialog
	{
		public AboutDlg ()
		{
			this.Build ();

			string ver = AppConfig.GetAppVersion ();
			labelCaption.Markup = 
				String.Format ("<span color=\"white\"><b>Instituto Mexicano de Radiónica v. {0}</b></span>", ver);
			labelCaptionShadow.Markup = 
				String.Format ("<span color=\"black\"><b>Instituto Mexicano de Radiónica v. {0}</b></span>", ver);
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			Destroy ();
		}
	}
}

