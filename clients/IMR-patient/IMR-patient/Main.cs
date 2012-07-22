using System;
using Gtk;
using monoCharp;

namespace IMRpatient
{
	class MainClass
	{
		private static readonly string GCONF_APP_BASE = "/apps/IMR-patient";
		private static readonly string GCONF_BASEURL = GCONF_APP_BASE + "/baseUrl";
		private static readonly string DEFAULT_BASEURL = "http://www.imr.local/";

		public static void Main (string[] args)
		{
			Application.Init ();

			GConf.Client gconf = new GConf.Client ();

			string baseUrl = null;
			try {
				baseUrl = (string) gconf.Get (GCONF_BASEURL);
			} catch (GConf.NoSuchKeyException) {
				WelcomeSetupDlg dlg = new WelcomeSetupDlg (DEFAULT_BASEURL);
				dlg.Response += delegate { baseUrl = dlg.baseUrl; };
				dlg.Run ();
			}
			if (baseUrl == null) {
				return;
			}
			gconf.Set (GCONF_BASEURL, baseUrl);

			Charp.BASE_URL = baseUrl;
			CharpGtk charp = new CharpGtk ();
			Radionic radionic = new Radionic ();

			WelcomeDlg wDlg = new WelcomeDlg (charp);
			charp.parent = wDlg;
			wDlg.Show ();
			Application.Run ();

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
