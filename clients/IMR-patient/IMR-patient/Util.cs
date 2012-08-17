using System;
using Gtk;
using Mono.Unix;

namespace IMRpatient
{
	public static class Util
	{
		public static void GtkLabelStyleAsError (Gtk.Label label) {
			label.UseMarkup = true;
			label.Markup = "<span color=\"red\">" + label.Text + "</span>";
		}

		public static void GtkLabelStyleRemove (Gtk.Label label) {
			string str = label.Text;
			label.UseMarkup = false;
			label.Text = str;
		}

		public static string GtkGetWidgetPath (Gtk.Widget w)
		{
			uint len;
			string name, rev;
			
			w.Path (out len, out name, out rev);
			return name;
		}
		
		public static string StringPlusSpace (string str) {
			if (str == null) {
				return "";
			}
			return str + " ";
		}

		public static string StringPlusSpace (object o) {
			return StringPlusSpace ((string) o);
		}
		
		public static string StatusToString (string status) {
			switch (status) {
			case "ACTIVE": return Catalog.GetString ("Active");
			case "DISABLED": return Catalog.GetString ("Disabled");
			case "DELETED": return Catalog.GetString ("Deleted");
			}
			throw new Exception ("Unsupported status `" + status + "'");
		}

		public static string StatusToString (object o) {
			return StatusToString ((string) o);
		}
	}
}

