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
			//stream.Close ();
		}

		public void LoadFile (Gtk.Window win, string fname, FileReadyDelegate del)
		{
			string fullname = dir + "/" + fname + ".jpg";
			if (File.Exists (fullname)) {
				CallDelegate (fullname, del);
				return;
			}

			charp.request ("file_get_picture", new object[] { fname }, new CharpGtk.CharpGtkCtx {
				parent = win,
				reply_handler = delegate (Uri base_uri, NameValueCollection parms, Charp.CharpCtx ctx) {
					ctx.wc = new WebClient ();
					ctx.wc.UploadValuesCompleted += new UploadValuesCompletedEventHandler (delegate (object sender, UploadValuesCompletedEventArgs status) {
						if (!charp.resultHandleErrors (status, ctx)) {
							del (null);
							return;
						}

						FileStream stream = new FileStream (fullname, FileMode.Create);
						stream.Write (status.Result, 0, status.Result.Length);
						stream.Close ();

						CallDelegate (fullname, del);
					});
					ctx.wc.UploadValuesAsync (base_uri, "POST", parms, ctx);
				},
				error = delegate {
					del (null);
					return true;
				}
			});
		}
	}
}

