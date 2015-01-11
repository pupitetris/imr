using System;
using System.IO;
using System.Collections.Specialized;
using System.Net;
using monoCharp;

namespace IMRpatient
{
	public class PictureCache
	{
		public delegate void FileReadyDelegate (System.IO.Stream stream);
		
		private Charp charp;
		private string dir;

		public PictureCache (Charp charp)
		{
			this.charp = charp;
		}

		public void Setup ()
		{
			dir = System.Environment.GetEnvironmentVariable ("HOME") + "/.imr";
			try {
				if (!Directory.Exists (dir))
					Directory.CreateDirectory (dir);

				dir += "/Pictures";
				if (!Directory.Exists (dir))
					Directory.CreateDirectory (dir);

			} catch (Exception e) {
				dir = null;
				Console.WriteLine (e.Message);
			}
		}

		public void CallDelegate (string fullname, FileReadyDelegate del)
		{
			FileStream stream = new FileStream (fullname, FileMode.Open);
			del ((Stream) stream);
		}

		public void LoadFile (Gtk.Window win, string fname, string resource, object [] parms, FileReadyDelegate del)
		{
			string fullname = dir + "/" + fname;
			if (File.Exists (fullname)) {
				CallDelegate (fullname, del);
				return;
			}

			charp.request (resource, parms, new CharpGtk.CharpGtkCtx {
				parent = win,
				reply_complete_handler = delegate (Charp.CharpCtx ctx) {
					if (ctx.wc.ResponseHeaders["Content-Type"].StartsWith ("application/json")) {
						// it's an error message.
						return false;
					}

					FileStream stream = new FileStream (fullname, FileMode.Create);
					byte[] result = ((UploadValuesCompletedEventArgs) ctx.status).Result;
					stream.Write (result, 0, result.Length);
					stream.Close ();

					CallDelegate (fullname, del);
					return true;
				},
				error = delegate {
					del (null);
					return true;
				}
			});
		}
	}
}

