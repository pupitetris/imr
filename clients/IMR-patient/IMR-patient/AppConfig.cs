using System;
using System.Text;
using monoCharp;
using System.Runtime.InteropServices;

namespace IMRpatient
{
	public class AppConfig
	{
		private static readonly string APP_NAME = "IMR-patient";
		private static readonly string GCONF_APP_BASE = "/apps/" + APP_NAME;
		private static readonly string GCONF_BASEURL = GCONF_APP_BASE + "/baseUrl";
		private static readonly string GCONF_PORT = GCONF_APP_BASE + "/port";
		private static readonly string DEFAULT_BASEURL = "http://www.imr.local/";

		private Charp charp;
		private Radionic radionic;
		private GConf.Client gconf;
		
		public AppConfig (Charp charp, Radionic radionic)
		{
			this.charp = charp;
			this.radionic = radionic;

			gconf = new GConf.Client ();
		}

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
		
		public static void TrySetProcessName()
		{
			try	{
				SetProcessName(APP_NAME);
			} catch	{
			}
		}

		public string Setup (string defaultBaseUrl)
		{
			string baseUrl = null;

			WelcomeSetupDlg dlg = new WelcomeSetupDlg (defaultBaseUrl, radionic);
			dlg.Response += delegate(object o, Gtk.ResponseArgs a) {
				if (a.ResponseId == Gtk.ResponseType.Ok) {
					baseUrl = dlg.baseUrl; 
				}
			};
			dlg.Run ();
			return baseUrl;
		}

		public bool LoadOrSetup () 
		{
			string baseUrl;
			try {
				baseUrl = (string) gconf.Get (GCONF_BASEURL);
				radionic.Port = (string) gconf.Get (GCONF_PORT);
			} catch (GConf.NoSuchKeyException) {
				baseUrl = Setup (DEFAULT_BASEURL);
			}

			if (baseUrl == null) {
				return false;
			}
			Charp.BASE_URL = baseUrl;
			return true;
		}

		public void Save () 
		{
			gconf.Set (GCONF_BASEURL, charp.baseUrl);
			if (radionic.Port != null && radionic.Port != "") {
				gconf.Set (GCONF_PORT, radionic.Port);
			}
		}
	}
}

