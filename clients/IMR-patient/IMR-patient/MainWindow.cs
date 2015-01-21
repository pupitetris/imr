using System;
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

		public UserListWin userListWin;

		public MainWindow (AppConfig config): base (Gtk.WindowType.Toplevel)
		{
			Build ();
			this.config = config;

			fixedBgAllocateFlag = false;
			IsLogout = false;

			config.LoadPermissions (delegate {
				ConfigureByPermissions ();
			});

			Maximize ();
		}

		private void ConfigureByPermissions ()
		{
			if (!config.CanPerform (IMR_PERM.USER_EDIT_SELF)) {
				if (!config.CanPerform (IMR_PERM.USER_CREATE)) { UsersAction.Sensitive = false; }
			} else if (!config.CanPerform (IMR_PERM.USER_CREATE)) { UsersNew.Sensitive = false; }

			if (!config.CanPerform (IMR_PERM.SYSTEM_BACKUP)) { BackupAction.Sensitive = false; }
			if (!config.CanPerform (IMR_PERM.SYSTEM_RESTORE)) { RestoreAction.Sensitive = false; }
		}

		private void Quit ()
		{
			if (userListWin != null)
				userListWin.SaveState ();
			config.radionic.Close ();
			Gtk.Main.Quit ();
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Gtk.MessageDialog md = 
				new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Question, Gtk.ButtonsType.YesNo,
					Catalog.GetString ("This will close the application.\n\nAre you sure you want to exit?"));
			md.Title = Catalog.GetString ("Exit");
			int res = md.Run ();
			md.Destroy ();
			if (res == (int) Gtk.ResponseType.Yes) {
				IsLogout = false;
				Quit ();
				a.RetVal = false;
			} else
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
			md.Title = Catalog.GetString ("Log out");
			int res = md.Run ();
			md.Destroy ();
			if (res == (int) Gtk.ResponseType.Yes) {
				IsLogout = true;
				Destroy ();
				Quit ();
			}
		}

		protected void OnAboutActionActivated (object sender, EventArgs e)
		{
			AboutDlg dlg = new AboutDlg ();
			dlg.TransientFor = this;
			dlg.Run ();
		}

		protected void OnUsersNewActivated (object sender, EventArgs e)
		{
			UserEditorWin win = new UserEditorWin (UserEditorWin.TYPE.NEW, config);
			win.TransientFor = this;
			win.Show ();
		}

		protected void OnUsersEditActivated (object sender, EventArgs e)
		{
			if (config.CanPerform (IMR_PERM.USER_EDIT)) {
				if (userListWin == null) {
					UserListWin win = new UserListWin (config);
					win.TransientFor = this;
					win.DeleteEvent += delegate { userListWin = null; };
					win.DestroyEvent += delegate { userListWin = null; };
					win.Show ();
					userListWin = win;
				} else {
					userListWin.Present ();
				}
			} else if (config.CanPerform (IMR_PERM.USER_EDIT_SELF)) {
				UserEditorWin win = new UserEditorWin (UserEditorWin.TYPE.EDIT_SELF, config);
				win.Show ();
			}
		}

		protected void OnRefreshThemeActivated (object sender, EventArgs e)
		{
			Gtk.Application.Invoke (delegate {
				if (Gtk.Rc.ReparseAll ())
					Console.WriteLine ("Theme reloaded");
			});
		}


		protected void OnPatientsNewActivated (object sender, EventArgs e)
		{
			PatientEditorWin win = new PatientEditorWin (PatientEditorWin.TYPE.NEW, config);
			win.TransientFor = this;
			win.Show ();
		}
	}
}