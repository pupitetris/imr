using System;
using System.Collections;
using Gtk;

namespace IMRpatient
{
	public class MyCombo
	{
		public delegate bool ActiveByDataDelegate (object obj);

		private Gtk.ComboBox _Combo;
		private ArrayList list;

		public Gtk.ComboBox Combo { get { return _Combo; } }
		public int Count { get { return list.Count; } }

		public MyCombo (Gtk.ComboBox combo)
		{
			_Combo = combo;
			list = new ArrayList ();
		}

		public void AppendText (string text, object data = null)
		{
			list.Add (data);
			_Combo.AppendText (text);
		}

		public void PrependText (string text, object data = null)
		{
			list.Insert (0, data);
			_Combo.PrependText (text);
		}

		public object ActiveData (int idx = -1)
		{
			if (idx < 0)
				idx = _Combo.Active;
			if (idx < 0)
				return null;
			return list[idx];
		}

		public void SetActiveByData (ActiveByDataDelegate del)
		{
			for (int i = 0; i < list.Count; i++) {
				if (del (list [i])) {
					_Combo.Active = i;
					return;
				}
			}
			_Combo.Active = -1;
		}

		public void Clear ()
		{
			_Combo.Active = -1;
			int count = list.Count;
			while (count > 0) {
				count --;
				_Combo.RemoveText (0);
			}
			list.Clear ();
		}
	}
}

