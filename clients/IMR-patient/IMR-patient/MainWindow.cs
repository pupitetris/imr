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

		private Boolean fixedBgAllocateFlag;
		public Boolean IsLogout;

		public MainWindow (AppConfig config): base (Gtk.WindowType.Toplevel)
		{
			Build ();
			this.config = config;

			fixedBgAllocateFlag = false;
			IsLogout = false;

			config.LoadPermissions (delegate {
				ConfigureByPermissions ();
			});
		}

		private void ConfigureByPermissions ()
		{
			if (!config.CanPerform (IMR_PERM.USER_EDIT_YOURSELF)) {
				if (!config.CanPerform (IMR_PERM.USER_CREATE)) { UsersAction.Sensitive = false; }
			} else if (!config.CanPerform (IMR_PERM.USER_CREATE)) { UsersNew.Sensitive = false; }

			if (!config.CanPerform (IMR_PERM.SYSTEM_BACKUP)) { BackupAction.Sensitive = false; }
			if (!config.CanPerform (IMR_PERM.SYSTEM_RESTORE)) { RestoreAction.Sensitive = false; }
		}
		
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Gtk.Main.Quit ();
			a.RetVal = true;
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