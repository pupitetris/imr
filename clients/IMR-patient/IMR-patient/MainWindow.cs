using System;
using Gtk;
using monoCharp;
using System.Net;

namespace IMRpatient {

	public partial class MainWindow: Gtk.Window
	{
		private AppConfig config;
		private Charp charp;

		public MainWindow (AppConfig config): base (Gtk.WindowType.Toplevel)
		{
			Build ();
			this.config = config;
			this.charp = config.charp;
		}
		
		protected void OnWindowDeleteEvent (object sender, DeleteEventArgs a)
		{
			Gtk.Main.Quit ();
			a.RetVal = true;
		}

		private void testySuccess (object data, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx)
		{
			Console.WriteLine ("success " + entry1.Text);
		}

		protected void OnButton5Clicked (object sender, EventArgs e)
		{
			charp.request (entry1.Text, null, new Charp.CharpCtx () { success = testySuccess });
		}

		protected void OnDestroyEvent (object sender, EventArgs e)
		{
			Gtk.Main.Quit ();
		}
	}
}