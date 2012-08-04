using System;
using System.Net;
using Gtk;
using monoCharp;
using Mono.Unix;

namespace IMRpatient {

	public partial class MainWindow: Gtk.Window
	{
		private static readonly int IMAGE_BG_OFFSSETY = 41;

		private AppConfig config;
		private Charp charp;

		private Boolean fixedBgAllocateFlag;
		public Boolean IsLogout;

		public MainWindow (AppConfig config): base (Gtk.WindowType.Toplevel)
		{
			Build ();
			this.config = config;
			this.charp = config.charp;

			fixedBgAllocateFlag = false;
			IsLogout = false;
		}
		
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Gtk.Main.Quit ();
			a.RetVal = true;
		}

		private void testySuccess (object data, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx)
		{
			//Console.WriteLine ("success " + entry1.Text);

		}

		protected void OnButton5Clicked (object sender, EventArgs e)
		{
			//charp.request (entry1.Text, null, new Charp.CharpCtx () { success = testySuccess });
		}

		protected void OnFixedBgSizeAllocated (object o, SizeAllocatedArgs args)
		{
			if (fixedBgAllocateFlag) {
				fixedBgAllocateFlag = false;
				return;
			}
			fixedBgAllocateFlag = true;

			Gtk.Requisition ireq = mainImageBg.SizeRequest ();
			Gtk.Fixed.FixedChild w = ((global::Gtk.Fixed.FixedChild) (fixedBg [mainImageBg]));
			w.X = args.Allocation.Width - ireq.Width;
			w.Y = args.Allocation.Height - ireq.Height + IMAGE_BG_OFFSSETY;
		}

		protected void OnLogOutActionActivated (object sender, EventArgs e)
		{
			Gtk.MessageDialog md = 
				new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Question, Gtk.ButtonsType.YesNo,
				                       Catalog.GetString ("This will take you back to the welcome screen.\n\nAre you sure you want to log out?"));
			int res = md.Run ();
			md.Destroy ();
			if (res == (int) Gtk.ResponseType.Yes) {
				IsLogout = true;
				Destroy ();
				Gtk.Main.Quit ();
			}
		}

		protected void OnAboutActionActivated (object sender, EventArgs e)
		{
			AboutDlg dlg = new AboutDlg ();
			dlg.TransientFor = this;
			dlg.Run ();
		}
	}
}