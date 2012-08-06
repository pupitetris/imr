using System;
using Gtk;

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
	}
}

