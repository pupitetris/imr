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
			AppConfig.ThemeSetup ();
			
			Application.Init ();

			CharpGtk charp = new CharpGtk ();
			AppConfig config = new AppConfig (charp, new Radionic ());

			if (!config.LoadOrSetup ()) {
				return;
			}

			MainWindow win;
			do {
				win = null;

				WelcomeDlg wDlg = new WelcomeDlg (config);
				charp.parent = wDlg;
				
				wDlg.Show ();
				Application.Run ();
				
				config.Save ();
				
				if (!wDlg.authSuccess ()) {
					return;
				}

				wDlg = null;

				win = new MainWindow (config);
				charp.parent = win;
				win.Show ();
				Application.Run ();
			} while (win.IsLogout);
		}
	}
}
