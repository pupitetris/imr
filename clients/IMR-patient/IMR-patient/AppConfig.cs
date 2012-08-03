using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

using Gtk;
using monoCharp;

namespace IMRpatient
{
	public class AppConfig
	{
		private static readonly string APP_NAME = "IMR-patient";
		private static readonly string GCONF_APP_BASE = "/apps/" + APP_NAME;
		private static readonly string GCONF_BASEURL = GCONF_APP_BASE + "/baseUrl";
		private static readonly string GCONF_PORT = GCONF_APP_BASE + "/port";
		private static readonly string DEFAULT_BASEURL = "http://www.imr.local/";

		public Charp charp;
		public Radionic radionic;
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

		public static void CopyResource (string dir, string resource)
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
			Stream input = assembly.GetManifestResourceStream (resource);
			FileStream outfile = new FileStream (dir + "/" + resource, FileMode.Create, FileAccess.ReadWrite);

			byte [] buffer = new byte [8192];
			int n;

			while ((n = input.Read (buffer, 0, 8192)) != 0)
				outfile.Write (buffer, 0, n);

			input.Close ();
			outfile.Close ();
		}

		public static void ThemeSetup ()
		{
			string dir = System.Environment.GetEnvironmentVariable ("HOME") + "/.imr";
			if (!Directory.Exists (dir))
			{
				try {
					Directory.CreateDirectory (dir);
					CopyResource (dir, "gtkrc");
					CopyResource (dir, "menu_bg.png");
					CopyResource (dir, "menu_prelight_bg.png");
				} catch (Exception e) {
					Console.WriteLine (e.Message);
				}
			}
			Gtk.Rc.AddDefaultFile (dir + "/gtkrc");
		}

		private string SetupDo (string defaultBaseUrl, Gtk.Window parent = null)
		{
			string baseUrl = null;

			WelcomeSetupDlg dlg = new WelcomeSetupDlg (defaultBaseUrl, radionic);
			dlg.Response += delegate(object o, Gtk.ResponseArgs args) {
				if (args.ResponseId == Gtk.ResponseType.Ok) {
					baseUrl = dlg.baseUrl; 
				}
			};
			if (parent != null) {
				dlg.TransientFor = parent;
			}
			dlg.Run ();
			return baseUrl;
		}

		public void Setup (Gtk.Window parent = null)
		{
			string baseUrl = SetupDo (charp.baseUrl, parent);
			if (baseUrl != null) {
				charp.baseUrl = baseUrl;
			}
		}

		public bool LoadOrSetup () 
		{
			string baseUrl;
			try {
				baseUrl = (string) gconf.Get (GCONF_BASEURL);
				radionic.Port = (string) gconf.Get (GCONF_PORT);
			} catch (GConf.NoSuchKeyException) {
				baseUrl = SetupDo (DEFAULT_BASEURL);
			}

			if (baseUrl == null) {
				return false;
			}
			Charp.BASE_URL = charp.baseUrl = baseUrl;
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

