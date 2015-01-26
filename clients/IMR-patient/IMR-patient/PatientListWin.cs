using System;
using Mono.Unix;
using monoCharp;
using Newtonsoft.Json.Linq;

namespace IMRpatient
{
	[Gtk.TreeNode (ListOnly=true)]
	public class PatientListNode : Gtk.TreeNode {
		public PatientListNode (JObject data)
		{
			Data = data;
			Prefix = (string) data["prefix"];
			Name = (string) data["name"];
			Paterno = (string) data["paterno"];
			Materno = (string) data["materno"];
		}

		public JObject Data;

		[Gtk.TreeNodeValue (Column=0)]
		public string Prefix;

		[Gtk.TreeNodeValue (Column=1)]
		public string Name;

		[Gtk.TreeNodeValue (Column=2)]
		public string Paterno;

		[Gtk.TreeNodeValue (Column=3)]
		public string Materno;
	}

	public partial class PatientListWin : UtilityWin
	{
		private Gtk.NodeStore store;
		private Gtk.NodeSelection selection;

		public PatientListWin (AppConfig config) : 
		base(config)
		{
			this.Build ();

			store = new Gtk.NodeStore (typeof (PatientListNode));
			nodeview.NodeStore = store;

			selection = nodeview.NodeSelection;
			selection.Mode = Gtk.SelectionMode.Multiple;
			selection.Changed += new System.EventHandler (this.OnSelectionChanged);

			Gtk.TreeViewColumn infoCol;

			nodeview.AppendColumn (Catalog.GetString ("Prefix"), new Gtk.CellRendererText (), "text", 0);
			infoCol = nodeview.AppendColumn (Catalog.GetString ("Name"), new Gtk.CellRendererText (), "text", 1);
			infoCol.Expand = true;
			nodeview.AppendColumn (Catalog.GetString ("Paterno"), new Gtk.CellRendererText (), "text", 2);
			nodeview.AppendColumn (Catalog.GetString ("Materno"), new Gtk.CellRendererText (), "text", 3);

			Refresh ();
		}

		private void OnSelectionChanged (object o, System.EventArgs args)
		{
			switch (selection.SelectedNodes.Length) {
				case 0:
					DeletePatientAction.Sensitive = false;
					EditPatientAction.Sensitive = false;
					break;
				case 1:
					DeletePatientAction.Sensitive = true;
					EditPatientAction.Sensitive = true;
					break;
				default:
					DeletePatientAction.Sensitive = true;
					EditPatientAction.Sensitive = false;
					break;
			}
		}

		public void Refresh ()
		{
			store.Clear ();
			config.charp.request ("patient_list_get", null, new CharpGtk.CharpGtkCtx {
				parent = this,
				success = delegate (object data, Charp.CharpCtx ctx) {
					Gtk.Application.Invoke (delegate {
						foreach (JObject dat in (JArray) data) {
							store.AddNode (new PatientListNode (dat));
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
				PatientListNode node = (PatientListNode) selected[i];
				config.charp.request ("patient_delete", new object[] {Convert.ToUInt32 (node.Data["persona_id"])}, 
					new CharpGtk.CharpGtkCtx {
						parent = this,
						success = delegate (object data, Charp.CharpCtx ctx) {
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
					Catalog.GetPluralString ("This will delete the patient from the system.\n\nAre you sure you want to delete it?",
						"This will delete {0} patients from the system.\n\nAre you sure you want to delete them?",
						selection.SelectedNodes.Length), selection.SelectedNodes.Length);
				Gtk.MessageDialog md = new Gtk.MessageDialog (this, Gtk.DialogFlags.Modal, Gtk.MessageType.Question, 
					Gtk.ButtonsType.YesNo, msg);
				md.Title = Catalog.GetString ("Delete patient");
				md.Icon = Stetic.IconLoader.LoadIcon (md, "gtk-dialog-question", Gtk.IconSize.Dialog);
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

			PatientListNode node = (PatientListNode) selected[0];
			PatientEditorWin win = new PatientEditorWin (PatientEditorWin.TYPE.EDIT, config, node.Data);
			win.Show ();
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
