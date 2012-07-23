using System;

namespace IMRpatient
{
	public partial class DeviceErrorDlg : Gtk.Dialog
	{
		private AppConfig config;

		public DeviceErrorDlg (AppConfig config, string message)
		{
			this.Build ();

			this.config = config;

			labelMessage.Text = message;
		}

		protected void OnButtonConfigureClicked (object sender, EventArgs e)
		{
			config.Setup (this);
		}

		protected void OnButtonRetryClicked (object sender, EventArgs e)
		{
			Destroy ();
		}

		protected void OnButtonIgnoreClicked (object sender, EventArgs e)
		{
			Destroy ();
		}
	}
}

