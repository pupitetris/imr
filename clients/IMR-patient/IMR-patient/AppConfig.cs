using System;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

using Gtk;
using monoCharp;

namespace IMRpatient
{
	public enum IMR_PERM {
		PATIENT_CREATE = 1000, // Operator
		PATIENT_EDIT,
		PATIENT_DELETE,
		USER_EDIT_SELF,
		USER_CREATE = 2000, // Admin
		USER_EDIT,
		USER_DELETE,
		SYSTEM_BACKUP = 3000, // Superuser
		SYSTEM_RESTORE,
		USER_SET_STATUS,
		USER_SET_ADMIN_LEVEL
	}

	public delegate void VoidDelegate ();

	public class AppConfig
	{
		private static readonly string APP_NAME = "IMRpatient";
		private static readonly string CONF_BASEURL = "baseUrl";
		private static readonly string CONF_PORT = "port";
		private static readonly string CONF_APP_WINDOWS = "windows";
		private static readonly string DEFAULT_BASEURL = "http://www.imr.local/";

		private enum AccountType {
			UNKNOWN = 0,
			OPERATOR,
			ADMIN,
			SUPERUSER
		}
		
		public Charp charp;
		public Radionic radionic;
		public PictureCache pcache;
		public MainWindow mainwin;
		private Charp.Config conf;
		private AccountType account_type;

		public AppConfig (Charp charp, Radionic radionic)
		{
			this.charp = charp;
			this.radionic = radionic;
			pcache = new PictureCache (charp);

			#if CHARP_WINDOWS
			conf = new CharpGtk.MSConfig (DEFAULT_BASEURL);
			#else
			conf = new CharpGtk.GConfConfig (DEFAULT_BASEURL);
			#endif
			conf.SetApp (APP_NAME);

			account_type = AccountType.UNKNOWN;
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

		private string loginMD5;
		public string LoginMD5 {
			set {
				loginMD5 = Charp.GetMD5HexHash (value);
			}
			get {
				return loginMD5;
			}
		}

		private static void CopyResource (string dir, string resource)
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

		public static string GetAppVersion ()
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
			return assembly.GetName ().Version.ToString ();
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
			string baseUrl = SetupDo (charp.BaseUrl, parent);
			if (baseUrl != null) {
				charp.BaseUrl = baseUrl;
			}
		}

		public bool LoadOrSetup () 
		{
			pcache.Setup ();
			
			string baseUrl;
			try {
				baseUrl = conf.Get (CONF_BASEURL);
				radionic.Port = conf.Get (CONF_PORT);
			} catch (Charp.Config.NoSuchKeyException) {
				baseUrl = SetupDo (DEFAULT_BASEURL);
			}

			if (baseUrl == null) {
				return false;
			}
			Charp.BASE_URL = charp.BaseUrl = baseUrl;

			return true;
		}

		public void Save () 
		{
			conf.Set (CONF_BASEURL, charp.BaseUrl);
			if (radionic.Port != null && radionic.Port != "") {
				conf.Set (CONF_PORT, radionic.Port);
			}
			conf.SuggestSync ();
		}

		public void SaveWindowGeom (string name, int x, int y, int w, int h) {
			try {
				string path = CONF_APP_WINDOWS + "/" + name + "/";
				conf.Set (path + "x", x);
				conf.Set (path + "y", y);
				conf.Set (path + "w", w);
				conf.Set (path + "h", h);
				conf.SuggestSync ();
			} catch (Exception) {
			}
		}

		public void SaveWindowKey (string name, string key, int value) {
			try {
				conf.Set (CONF_APP_WINDOWS + "/" + name + "/" + key, value);
				conf.SuggestSync ();
			} catch (Exception) {
			}
		}

		public void SaveWindowKey (string name, string key, string value) {
			try {
				conf.Set (CONF_APP_WINDOWS + "/" + name + "/" + key, value);
				conf.SuggestSync ();
			} catch (Exception) {
			}
		}
		
		public bool LoadWindowGeom (string name, out int x, out int y, out int w, out int h) {
			try {
				string path = CONF_APP_WINDOWS + "/" + name + "/";
				x = conf.GetInt (path + "x");
				y = conf.GetInt (path + "y");
				w = conf.GetInt (path + "w");
				h = conf.GetInt (path + "h");
			} catch (Exception) {
				x = y = w = h = 0;
				return false;
			}
			return true;
		}

		public bool LoadWindowKey (string name, string key, out int value) {
			try {
				value = conf.GetInt (CONF_APP_WINDOWS + "/" + name + "/" + key);
			} catch (Exception) {
				value = 0;
				return false;
			}
			return true;
		}
		
		public bool LoadWindowKey (string name, string key, out string value) {
			try {
				value = conf.Get (CONF_APP_WINDOWS + "/" + name + "/" + key);
			} catch (Exception) {
				value = null;
				return false;
			}
			return true;
		}
		
		public void LoadPermissions (VoidDelegate del)
		{
			account_type = AccountType.UNKNOWN;
			charp.request ("user_get_type", null, new Charp.CharpCtx {
				success = delegate (object data, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx){
					string typestr = (string) ((JValue) data).Value;
					switch (typestr) {
						case "ADMIN":
							account_type = AccountType.ADMIN;
							break;
						case "OPERATOR":
							account_type = AccountType.OPERATOR;
							break;
						case "SUPERUSER":
							account_type = AccountType.SUPERUSER;
							break;
					}
					del ();
				}
			});
		}

		public bool CanPerform (IMR_PERM perm) {
			if (((int) account_type) < ((int) perm) / 1000) {
				return false;
			}
			return true;
		}
	}
}

