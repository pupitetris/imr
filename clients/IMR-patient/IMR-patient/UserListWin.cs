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
		public UserListNode (Dictionary<string, string> data)
		{
			Data = data;
			Name = data["username"];
			Info = Util.StringPlusSpace (data["prefix"]) +	Util.StringPlusSpace (data["name"]) +
				Util.StringPlusSpace (data["paterno"]) + Util.StringPlusSpace (data["materno"]);
			Level = Util.UserLevelToString (data["type"]);
			Status = "(" + Util.StatusToString (data["status"]) + ")";
		}

		public Dictionary<string, string> Data;

		[Gtk.TreeNodeValue (Column=0)]
		public string Name;

		[Gtk.TreeNodeValue (Column=1)]
		public string Info;

		[Gtk.TreeNodeValue (Column=2)]
		public string Level;

		[Gtk.TreeNodeValue (Column=3)]
		public string Status;
	}

	public partial class UserListWin : UtilityWin
	{
		private Gtk.NodeStore store;
		private Gtk.NodeSelection selection;

		public UserListWin (AppConfig config) : 
				base(config)
		{
			this.Build ();

			store = new Gtk.NodeStore (typeof (UserListNode));
			nodeview.NodeStore = store;

			selection = nodeview.NodeSelection;
			selection.Mode = Gtk.SelectionMode.Multiple;
			selection.Changed += new System.EventHandler (this.OnSelectionChanged);

			Gtk.TreeViewColumn infoCol;

			nodeview.AppendColumn (Catalog.GetString ("Username"), new Gtk.CellRendererText (), "text", 0);
			infoCol = nodeview.AppendColumn (Catalog.GetString ("Info"), new Gtk.CellRendererText (), "text", 1);
			infoCol.Expand = true;
			nodeview.AppendColumn (Catalog.GetString ("Level"), new Gtk.CellRendererText (), "text", 2);
			nodeview.AppendColumn (Catalog.GetString ("Status"), new Gtk.CellRendererText (), "text", 3);

			Refresh ();
		}

		private void OnSelectionChanged (object o, System.EventArgs args)
		{
			switch (selection.SelectedNodes.Length) {
			case 0:
				DeleteAction.Sensitive = false;
				EditAction.Sensitive = false;
				break;
			case 1:
				DeleteAction.Sensitive = true;
				EditAction.Sensitive = true;
				break;
			default:
				DeleteAction.Sensitive = true;
				EditAction.Sensitive = false;
				break;
			}
		}

		private void Refresh ()
		{
			store.Clear ();
			config.charp.request ("user_list_get", null, new CharpGtk.CharpGtkCtx {
				parent = this,
				success = delegate (object data, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						ArrayList arr = (ArrayList) data;
						for (int i = 0; i < arr.Count; i++) {
							store.AddNode (new UserListNode ((Dictionary<string, string>) arr[i]));
						}
						nodeview.ShowNow ();
						FinishAction (menubar);
					});
				},
				error = delegate(Charp.CharpError err, Charp.CharpCtx ctx)  {
					Gtk.Application.Invoke (delegate { FinishAction (menubar); });
					return true;
				}
			});
		}

		protected void OnRefreshActionActivated (object sender, EventArgs e)
		{
			Refresh ();
		}

		private void DeleteAsync (int i)
		{
			Gtk.ITreeNode[] selected = selection.SelectedNodes;
			if (i < selected.Length) {
				UserListNode node = (UserListNode) selected[i];
				config.charp.request ("user_remove", new object[] {Convert.ToUInt32 (node.Data["persona_id"])}, 
				new CharpGtk.CharpGtkCtx {
					parent = this,
					success = delegate (object data, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx) {
						Gtk.Application.Invoke (delegate {
							store.RemoveNode (node);
							DeleteAsync (i);
						});
					},
					error = delegate(Charp.CharpError err, Charp.CharpCtx ctx)  {
						if (err.key == "SQL:USERPERM") {
							Gtk.Application.Invoke (delegate {
								DeleteAsync (i + 1);
							});
							return false;
						}

						Gtk.Application.Invoke (delegate {
							nodeview.ShowNow ();
							FinishAction (menubar);
						});
						return true;
					}
				});

			} else {
				nodeview.ShowNow ();
				FinishAction (menubar);
			}
		}

		private void DeleteSelected ()
		{
			SendAction (menubar, delegate {
				string msg = String.Format (
					Catalog.GetPluralString ("This will delete the user from the system.\n\nAre you sure you want to delete it?",
				                         "This will delete {0} users from the system.\n\nAre you sure you want to delete them?",
				                         selection.SelectedNodes.Length), selection.SelectedNodes.Length);
				Gtk.MessageDialog md = new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Question, 
				                                              Gtk.ButtonsType.YesNo, msg);
				int res = md.Run ();
				md.Destroy ();
				if (res == (int) Gtk.ResponseType.Yes) {
					DeleteAsync (0);
				}
			});
		}

		protected void OnDeleteActionActivated (object sender, EventArgs e)
		{
			DeleteSelected ();
		}

		private void EditSelected ()
		{
			Gtk.ITreeNode[] selected = selection.SelectedNodes;
			if (selected.Length != 1) {
				throw new Exception ("Selection must be 1");
			}
			
			UserListNode node = (UserListNode) selected[0];
			UserEditorWin win = new UserEditorWin (UserEditorWin.TYPE.EDIT, config, node.Data);
			win.Show ();
			win.Present ();
		}

		protected void OnEditActionActivated (object sender, EventArgs e)
		{
			SendAction (menubar, EditSelected);
		}

		protected void OnNodeviewRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			EditSelected ();
		}
	}
}

