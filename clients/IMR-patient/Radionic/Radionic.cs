using System;
using System.IO.Ports;

namespace IMRpatient
{
	public abstract class Radionic
	{
		private enum STATE {
			IDLE = 0,
			CMD_PROBE
		}

		public string Port { get; set; }
		private SerialPort serial;
		private STATE state;

		public delegate void CommandComplete (bool success);

		private void handleDataReceived (System.IO.Ports.SerialDataReceivedEventArgs e)
		{
			switch (state) {
			case STATE.IDLE:
				return;
			case STATE.CMD_PROBE:
				return;
			}
		}

		public Radionic ()
		{
			Port = null;
			state = STATE.IDLE;
			serial = new SerialPort ();
			serial.BaudRate = 2400;
			serial.DataBits = 8;
			serial.Parity = Parity.None;
			serial.StopBits = StopBits.One;
			serial.Handshake = Handshake.None;
			serial.DataReceived += delegate(object sender, System.IO.Ports.SerialDataReceivedEventArgs e) {
				this.handleDataReceived (e);
			};
		}

		public void Open ()
		{
			serial.PortName = Port;
			serial.Open ();
		}

		public void Close () 
		{
			serial.Close ();
		}

		public static string[] findPorts ()
		{
			return SerialPort.GetPortNames ();
		}
	}
}

