using System;
using Newtonsoft.Json.Linq;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PhoneEditor : Gtk.Bin
	{
		private AppConfig config;
		private Gtk.Window ParentWin;
		private Gtk.Container Cont;
		private JObject myData;
		
		public PhoneEditor ()
		{
			this.Build ();
		}

		public PhoneEditor (AppConfig config, Gtk.Window parent, Gtk.Container cont, JObject data = null) {
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
				if (Util.DictTryValue (data, "number", out val)) { entryNumber.Text = val; }
				if (Util.DictTryValue (data, "remarks", out val)) { textRemarks.Buffer.InsertAtCursor (val); }
				if (Util.DictTryValue (data, "p_type", out val)) {
					int active;
					switch (val) {
						case "MOBILE": active = 0; break;
						case "HOME": active = 1; break;
						case "WORK": active = 2; break;
						case "NEXTEL": active = 3; break;
						default: active = -1; break;
					}
					comboType.Active = active;
				}
			} else
				myData = new JObject ();
		}
	}
}

