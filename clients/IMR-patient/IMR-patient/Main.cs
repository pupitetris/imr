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

			Radionic radionic = new Radionic ();
			CharpGtk charp = new CharpGtk ();

			AppConfig config = new AppConfig (charp, radionic);
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

			MainWindow win = new MainWindow (charp);
			charp.parent = win;
			win.Show ();
			Application.Run ();
		}
	}
}
