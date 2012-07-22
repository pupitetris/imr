using System;
using Mono.Unix;

namespace IMRpatient
{
	public partial class WelcomeSetupDlg : Gtk.Dialog
	{
		public string baseUrl;
		private Radionic radionic;
		private int comboPortSize;

		private void findPorts ()
		{
			while (comboPortSize-- > 0) {
				comboPort.RemoveText (0);
			}

			string[] ports = Radionic.findPorts ();
			comboPortSize = ports.Length;
			for (int i = 0; i < comboPortSize; i++) {
				comboPort.AppendText (ports[i]);
			}
		}

		public WelcomeSetupDlg (string baseUrl, Radionic radionic)
		{
			this.Build ();
			this.baseUrl = baseUrl;
			this.radionic = radionic;

			comboPortSize = 0;

			baseUrlEntry.Text = baseUrl;
			findPorts ();
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

		protected void OnButtonPortRefreshClicked (object sender, EventArgs e)
		{
			findPorts ();
		}

		protected void OnButtonProbeClicked (object sender, EventArgs e)
		{
			ProbeLogDlg dlg = new ProbeLogDlg (radionic);
			dlg.TransientFor = this;
			dlg.Response += delegate(object o, Gtk.ResponseArgs args) {
				switch (args.ResponseId) {
				case Gtk.ResponseType.Apply:
					comboPort.Entry.Text = dlg.Port;
					break;
				case Gtk.ResponseType.None:
					comboPort.Entry.Text = "";
					break;
				}
			};
			dlg.Run ();
		}
	}
}

