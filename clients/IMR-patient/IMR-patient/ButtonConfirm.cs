using System;
using System.ComponentModel;

namespace IMRpatient
{
	[ToolboxItem(true)]
	public partial class ButtonConfirm : Gtk.Bin
	{
		public delegate void ConfirmedClickDelegate (object sender, EventArgs e);

		public enum ConfirmState {
			NORMAL,
			CONFIRMING
		}

		private static uint CONFIRM_TIMEOUT = 2000;

		private ConfirmState currState = ConfirmState.NORMAL;
		private uint confirmTimeoutHandle = 0;
		private Gdk.Pixbuf pixbufNormal;
		private Gdk.Pixbuf pixbufConfirming;

		public event EventHandler ConfirmClick;

		public ButtonConfirm ()
		{
			this.Build ();

			pixbufNormal = image.Pixbuf;
			pixbufConfirming = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.trash_delete.png");
		}

		[GLib.Property ("pixbuf")]
		public Gdk.Pixbuf PixbufNormal {
			set {
				pixbufNormal = value;
				if (currState == ConfirmState.NORMAL)
					image.Pixbuf = value;
			}
			get {
				return pixbufNormal;
			}
		}

		[GLib.Property ("pixbuf")]
		public Gdk.Pixbuf PixbufConfirming {
			set {
				pixbufConfirming = value;
				if (currState == ConfirmState.CONFIRMING)
					image.Pixbuf = value;
			}
			get {
				return pixbufConfirming;
			}
		}
		
		private void RemoveTimeout ()
		{
			if (confirmTimeoutHandle > 0)
				GLib.Source.Remove (confirmTimeoutHandle);
			confirmTimeoutHandle = 0;
		}

		protected override void OnDestroyed ()
		{
			RemoveTimeout ();
			base.OnDestroyed ();
		}

		private bool ButtonRevert ()
		{
			currState = ConfirmState.NORMAL;
			confirmTimeoutHandle = 0;
			image.Pixbuf = pixbufNormal;
			return false;
		}
		
		protected void OnButtonClicked (object sender, EventArgs e)
		{
			if (currState == ConfirmState.CONFIRMING) {
				RemoveTimeout ();
				ConfirmClick (sender, e);
			} else {
				currState = ConfirmState.CONFIRMING;
				confirmTimeoutHandle = GLib.Timeout.Add (CONFIRM_TIMEOUT, this.ButtonRevert);
				image.Pixbuf = pixbufConfirming;
			}
		}
	}
}
