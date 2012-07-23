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

			AppConfig conf = new AppConfig (charp, radionic);
			if (!conf.LoadOrSetup ()) {
				return;
			}

			WelcomeDlg wDlg = new WelcomeDlg (charp, radionic);
			charp.parent = wDlg;
			wDlg.Show ();
			Application.Run ();

			conf.Save ();

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
