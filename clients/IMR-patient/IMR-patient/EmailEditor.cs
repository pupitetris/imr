using System;
using Newtonsoft.Json.Linq;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmailEditor : Gtk.Bin
	{
		private AppConfig config;
		private Gtk.Window ParentWin;
		private Gtk.Container Cont;
		private JObject myData;
		
		public EmailEditor ()
		{
			this.Build ();
		}
		
		public EmailEditor (AppConfig config, Gtk.Window parent, Gtk.Container cont, JObject data = null) {
			this.Build ();
			
			this.config = config;
			ParentWin = parent;
			Cont = cont;
			
			buttonDelete.ConfirmClick += delegate (object sender, EventArgs e) { Cont.Remove (this); };
			
			LoadData (data);
		}
		
		public void LoadData (JObject data)
		{
			if (data != null) {
				myData = data;
				
				string val;
				if (Util.DictTryValue (data, "email", out val)) { entryEmail.Text = val; }
				if (Util.DictTryValue (data, "remarks", out val)) { textRemarks.Buffer.InsertAtCursor (val); }
				if (Util.DictTryValue (data, "e_type", out val)) {
					int active;
					switch (val) {
						case "PERSONAL": active = 0; break;
						case "WORK": active = 1; break;
						default: active = -1; break;
					}
					comboType.Active = active;
				}
				if (Util.DictTryValue (data, "system", out val)) {
					int active;
					switch (val) {
						case "STANDARD": active = 0; break;
						case "SKYPE": active = 1; break;
						default: active = -1; break;
					}
					comboSystem.Active = active;
				}
			} else
				myData = new JObject ();
		}
	}
}
