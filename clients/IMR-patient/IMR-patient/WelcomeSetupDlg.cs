using System;

namespace IMRpatient
{
	public partial class WelcomeSetupDlg : Gtk.Dialog
	{
		public string baseUrl;

		public WelcomeSetupDlg (string baseUrl)
		{
			this.Build ();
			this.baseUrl = baseUrl;
			baseUrlEntry.Text = baseUrl;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			baseUrl = baseUrlEntry.Text;
			Respond (1);
			Destroy ();
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			Destroy ();
		}
	}
}

