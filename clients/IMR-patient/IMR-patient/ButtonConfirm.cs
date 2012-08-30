using System;

namespace IMRpatient
{
	[System.ComponentModel.ToolboxItem(true)]
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

		public ConfirmedClickDelegate ConfirmClick;

		public ButtonConfirm ()
		{
			this.Build ();

			pixbufNormal = image.Pixbuf;
			pixbufConfirming = Gdk.Pixbuf.LoadFromResource ("IMRpatient.img.trash_delete.png");
		}

		public void SetPixbuf (ConfirmState state, Gdk.Pixbuf pixbuf) {
			switch (state) {
			case ConfirmState.NORMAL:
				pixbufNormal = pixbuf;
				break;
			case ConfirmState.CONFIRMING:
				pixbufConfirming = pixbuf;
				break;
			}

			if (state == currState)
				image.Pixbuf = pixbuf;
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

