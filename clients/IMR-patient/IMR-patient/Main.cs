using System;
using System.Text;
using System.Runtime.InteropServices;
using Gtk;
using monoCharp;

namespace IMRpatient
{
	static class PlatformHacks
	{
		[DllImport("libc")] // Linux
		private static extern int prctl(int option, byte[] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);
		
		[DllImport("libc")] // BSD
		private static extern void setproctitle(byte[] fmt, byte[] str_arg);
		
		public static void SetProcessName(string name)
		{
			if (Environment.OSVersion.Platform != PlatformID.Unix) {
				return;
			}
			
			try	{
				if (prctl(15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes(name + "\0"),
				          IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0) {
					throw new ApplicationException("Error setting process name: " +
					                               Mono.Unix.Native.Stdlib.GetLastError());
				}
			} catch (EntryPointNotFoundException) {
				setproctitle(Encoding.ASCII.GetBytes("%s\0"),
				             Encoding.ASCII.GetBytes(name + "\0"));
			}
		}
		
		public static void TrySetProcessName(string name)
		{
			try	{
				SetProcessName(name);
			} catch	{
			}
		}
	}

	class MainClass
	{
		private static readonly string APP_NAME = "IMR-patient";
		private static readonly string GCONF_APP_BASE = "/apps/" + APP_NAME;
		private static readonly string GCONF_BASEURL = GCONF_APP_BASE + "/baseUrl";
		private static readonly string GCONF_PORT = GCONF_APP_BASE + "/port";
		private static readonly string DEFAULT_BASEURL = "http://www.imr.local/";

		public static void Main (string[] args)
		{
			PlatformHacks.TrySetProcessName(APP_NAME);

			Application.Init ();

			GConf.Client gconf = new GConf.Client ();
			Radionic radionic = new Radionic ();

			string baseUrl = null;
			try {
				baseUrl = (string) gconf.Get (GCONF_BASEURL);
				radionic.Port = (string) gconf.Get (GCONF_PORT);
			} catch (GConf.NoSuchKeyException) {
				WelcomeSetupDlg dlg = new WelcomeSetupDlg (DEFAULT_BASEURL, radionic);
				dlg.Response += delegate(object o, Gtk.ResponseArgs a) {
					if (a.ResponseId == Gtk.ResponseType.Ok) {
						baseUrl = dlg.baseUrl; 
					}
				};
				dlg.Run ();
			}
			if (baseUrl == null) {
				return;
			}

			Charp.BASE_URL = baseUrl;
			CharpGtk charp = new CharpGtk ();

			WelcomeDlg wDlg = new WelcomeDlg (charp, radionic);
			charp.parent = wDlg;
			wDlg.Show ();
			Application.Run ();

			gconf.Set (GCONF_BASEURL, charp.baseUrl);
			if (radionic.Port != null && radionic.Port != "") {
				gconf.Set (GCONF_PORT, radionic.Port);
			}

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
