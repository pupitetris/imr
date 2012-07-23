using System;
using Gtk;
using monoCharp;

namespace IMRpatient
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			AppConfig.TrySetProcessName ();

			Application.Init ();

			CharpGtk charp = new CharpGtk ();
			AppConfig config = new AppConfig (charp, new Radionic ());

			if (!config.LoadOrSetup ()) {
				return;
			}

			WelcomeDlg wDlg = new WelcomeDlg (config);
			charp.parent = wDlg;
			wDlg.Show ();
			Application.Run ();

			config.Save ();

			if (!wDlg.authSuccess ()) {
				return;
			}

			MainWindow win = new MainWindow (config);
			charp.parent = win;
			win.Show ();
			Application.Run ();
		}
	}
}
