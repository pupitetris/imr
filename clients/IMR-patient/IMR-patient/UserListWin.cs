using System;
using System.Net;
using System.Collections; // for ArrayList
using System.Collections.Generic; // for Dictionary
using Mono.Unix;
using monoCharp;

namespace IMRpatient
{
	[Gtk.TreeNode (ListOnly=true)]
	public class UserListNode : Gtk.TreeNode {
		public UserListNode (object persona_id, object name, string info)
		{
			Name = (string) name;
			Info = info;
			PersonaId = (uint) persona_id;
		}

		[Gtk.TreeNodeValue (Column=0)]
		public string Name;

		[Gtk.TreeNodeValue (Column=1)]
		public string Info;

		public uint PersonaId;
	}

	public partial class UserListWin : UtilityWin
	{
		private Gtk.NodeStore store;

		public UserListWin (AppConfig config) : 
				base(config)
		{
			this.Build ();

			store = new Gtk.NodeStore (typeof (UserListNode));
			nodeview.NodeStore = store;

			nodeview.AppendColumn (Catalog.GetString ("Username"), new Gtk.CellRendererText (), "text", 0);
			nodeview.AppendColumn (Catalog.GetString ("Info"), new Gtk.CellRendererText (), "text", 1);
		}

		private void Refresh ()
		{
			store.Clear ();
			config.charp.request ("user_list_get", null, new CharpGtk.CharpGtkCtx {
				parent = this,
				success = delegate (object data, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					ArrayList arr = (ArrayList) data;
					for (int i = 0; i < arr.Count; i++) {
						Dictionary<string, object> o = (Dictionary<string, object>) arr[i];
						store.AddNode (new UserListNode (o["persona_id"], 
						                                 o["username"], 
						                                 (string) o["paterno"]));
					}
				}
			});
		}

		protected void OnRefreshActionActivated (object sender, EventArgs e)
		{
			SendAction (menubar, Refresh);
		}
	}
}

