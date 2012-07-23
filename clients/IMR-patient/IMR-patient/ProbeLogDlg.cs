using System;
using Mono.Unix;

namespace IMRpatient
{
	public partial class ProbeLogDlg : Gtk.Dialog
	{
		public string Port { get; set; }

		private Radionic radionic;
		private string[] probePorts;
		private int probeIdx;
		private Gtk.TextBuffer logBuff;

		public ProbeLogDlg (Radionic radionic)
		{
			this.Build ();

			logBuff = textviewLog.Buffer;
			Port = "";

			this.radionic = radionic;

			probeStart ();
		}

		private bool logScroll ()
		{
			textviewLog.ScrollToIter (logBuff.EndIter, 0.0, true, 0, 1);
			return false;
		}

		private void logAppend (string txt) 
		{
			logBuff.InsertAtCursor (txt);
			GLib.Timeout.Add (100, new GLib.TimeoutHandler (logScroll));
		}

		private void probeCB (Radionic.RESULT result, string reply)
		{
			string msg;

			switch (result) {
			case (Radionic.RESULT.SUCCESS):
				msg = Catalog.GetString ("Found.");
				Respond (Gtk.ResponseType.Apply);
				break;
			case (Radionic.RESULT.TIMEOUT):
				msg = Catalog.GetString ("Timeout.\n\n");
				break;
			default:
				msg = Catalog.GetString ("Bad reply.\n\n");
				break;
			}

			buttonRetry.Sensitive = true;
			logAppend (msg);

			if (result != Radionic.RESULT.SUCCESS) {
				probeIdx ++;
				probeNext ();
			}
		}

		private bool probeNextTimeout ()
		{
			radionic.Close ();
			radionic.Port = Port = probePorts[probeIdx];
			logAppend (String.Format (Catalog.GetString ("Trying port {0}.\n"), radionic.Port));
			try {
				logAppend (Catalog.GetString ("Open... "));
				radionic.Open ();
				logAppend (Catalog.GetString ("OK.\nProbe... "));
				radionic.Probe (probeCB);
			} catch (Exception e) {
				logAppend (e.Message + ".\n\n");
				probeIdx ++;
				probeNext ();
			}

			return false;
		}
		
		private void probeNext ()
		{
			if (probeIdx == probePorts.Length) {
				logAppend (Catalog.GetString ("Not found. Is equipment on and connected?"));
				buttonRetry.Sensitive = true;
				Respond (Gtk.ResponseType.None);
				Port = "";
				return;
			}

			GLib.Timeout.Add (125, new GLib.TimeoutHandler (probeNextTimeout));
		}

		private void probeStart ()
		{
			logBuff.Clear ();
			probePorts = Radionic.findPorts ();
			probeIdx = 0;
			probeNext ();
		}

		protected void OnButtonRetryClicked (object sender, EventArgs e)
		{
			buttonRetry.Sensitive = false;
			probeStart ();
		}

		protected void OnButtonCloseClicked (object sender, EventArgs e)
		{
			radionic.Close ();
			Destroy ();
		}
	}
}
