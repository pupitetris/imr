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
		public UserListNode (object persona_id, object name, string info, string status)
		{
			PersonaId = Convert.ToUInt32 (persona_id);
			Name = (string) name;
			Info = info;
			Status = status;
		}

		public uint PersonaId;

		[Gtk.TreeNodeValue (Column=0)]
		public string Name;

		[Gtk.TreeNodeValue (Column=1)]
		public string Info;

		[Gtk.TreeNodeValue (Column=2)]
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
			nodeview.AppendColumn (Catalog.GetString ("Status"), new Gtk.CellRendererText (), "text", 2);

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
							Dictionary<string, object> o = (Dictionary<string, object>) arr[i];
							store.AddNode (new UserListNode (o["persona_id"], o["username"], 
							                                 Util.StringPlusSpace (o["prefix"]) +
							                                 Util.StringPlusSpace (o["name"]) +
							                                 Util.StringPlusSpace (o["paterno"]) +
							                                 Util.StringPlusSpace (o["materno"]),
							                                 "(" + Util.StatusToString (o["status"]) + ")"));
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
				config.charp.request ("user_remove", new object[] {node.PersonaId}, new CharpGtk.CharpGtkCtx {
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
			DeleteAsync (0);
		}

		protected void OnDeleteActionActivated (object sender, EventArgs e)
		{
			DeleteSelected ();
		}

		protected void OnEditActionActivated (object sender, EventArgs e)
		{
			throw new System.NotImplementedException ();
		}
	}
}

