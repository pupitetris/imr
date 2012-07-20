using System;
using Gtk;
using monoCharp;
using System.Net;

public partial class MainWindow: Gtk.Window
{
	private Charp charp;

	public MainWindow (Charp charp): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		this.charp = charp;
	}
	
	protected void OnWindowDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	private void testySuccess (object data, UploadValuesCompletedEventArgs status, Charp.CharpCtx ctx)
	{
		Console.WriteLine ("success " + entry1.Text);
	}

	protected void OnButton5Clicked (object sender, EventArgs e)
	{
		charp.request (entry1.Text, null, new Charp.CharpCtx () { success = testySuccess });
	}
}
