using System;
using System.Collections;
using System.Collections.Specialized;
using Gtk;
using Mono.Unix;
using Newtonsoft.Json.Linq;

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

		public static string GtkGetWidgetPath (Gtk.Widget w, AppConfig config)
		{
			uint len;
			string name, rev;
			
			w.Path (out len, out name, out rev);
			return config.LoginMD5 + "/" + name;
		}

		public static void GtkComboActiveFromData (Gtk.ComboBox combo, ArrayList list, 
		                                           string key, string value, int offset = 0)
		{
			if (list != null && value != null) {
				for (int i = 0; i < list.Count; i++) {
					if (((StringDictionary) list[i])[key] == value) {
						combo.Active = i + offset;
						break;
					}
				}
			}
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
		
		public static string UserLevelToString (string level) {
			switch (level) {
			case "OPERATOR": return Catalog.GetString ("Operator");
			case "ADMIN": return Catalog.GetString ("Administrator");
			case "SUPERUSER": return Catalog.GetString ("Super User");
			}
			throw new Exception ("Unsupported level `" + level + "'");
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

		public static bool DictTryValue (StringDictionary dict, string key, out string value)
		{
			if (dict.ContainsKey (key)) {
				value = dict[key];
				if (value == null)
					return false;
				return true;
			}

			value = null;
			return false;
		}

		public static bool DictTryValue (JObject dict, string key, out string value)
		{
			if (dict[key] != null) {
				value = (string) dict[key];
				if (value == null)
					return false;
				return true;
			}

			value = null;
			return false;
		}

		public static bool DictTryValue (JObject dict, string key, out int value)
		{
			if (dict[key] != null) {
				value = (int) dict[key];
				return true;
			}
			value = -1;
			return false;
		}

		public static bool DictArrayKeyValueUnique (JArray arr, string key, string value) {
			bool first = false;
			foreach (JObject dict in arr) {
				if ((string) dict[key] == value) {
					if (first) return false;
					first = true;
				}
			}
			if (first)
				return true;
			return false; // not found
		}
	}
}

