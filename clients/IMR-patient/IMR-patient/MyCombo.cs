using System;
using Gtk;

namespace IMRpatient
{
	public class MyCombo
	{
		private Gtk.ComboBox _Combo;
		private int _Count;

		public Gtk.ComboBox Combo { get { return _Combo; } }
		public int Count { get { return _Count; } }

		public MyCombo (Gtk.ComboBox combo)
		{
			_Combo = combo;
		}

		public void AppendText (string text)
		{
			_Count ++;
			_Combo.AppendText (text);
		}

		public void Clear ()
		{
			while (_Count > 0) {
				_Count --;
				_Combo.RemoveText (0);
			}
			_Count = 0;
		}
	}
}

