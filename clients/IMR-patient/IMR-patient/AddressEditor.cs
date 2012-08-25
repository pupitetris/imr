using System;
using System.Collections.Specialized;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AddressEditor : Gtk.Bin
	{
		private Gtk.Window ParentWin;
		private Gtk.Container Cont;
		private StringDictionary myData;

		public AddressEditor ()
		{
			this.Build ();
		}

		public AddressEditor (Gtk.Window parent, Gtk.Container cont, StringDictionary data = null)
		{
			this.Build ();

			ParentWin = parent;
			Cont = cont;

			if (data != null) {
				LoadData (data);
			}
		}

		public void LoadData (StringDictionary data)
		{
			myData = data;
		}
	}
}

