using System;
using System.IO.Ports;
using System.Text;

namespace IMRpatient
{
	public class Radionic
	{
		private enum STATE {
			IDLE = 0,
			CMD_PROBE
		}

		public delegate void CommandComplete (bool success);

		private static readonly string REPLY_PROBE = "284a-er45-FG34-09%#-12w+q";
		
		public string Port { get; set; }
		private SerialPort serial;
		private STATE state;
		private CommandComplete cb;
		private StringBuilder builder;

		private void handleDataReceived (object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
		{
			string data = serial.ReadExisting ();
			Console.WriteLine (data);

			switch (state) {
			case STATE.IDLE:
				return;
			case STATE.CMD_PROBE:
				builder.Append (data);
				if (builder.ToString ().Length >= REPLY_PROBE.Length) {
					if (builder.ToString () == REPLY_PROBE) {
						cb (true);
					} else {
						cb (false);
					}
					builder.Clear ();
				}
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
			serial.DataReceived += new SerialDataReceivedEventHandler (handleDataReceived);
		}

		public void Open ()
		{
			serial.PortName = Port;
			serial.Open ();
		}

		public void Close () 
		{
			if (serial.IsOpen) {
				serial.Close ();
			}
		}

		public void Probe (CommandComplete cb)
		{
			this.cb = cb;
			state = STATE.CMD_PROBE;
			serial.Write ("W");
		}

		public static string[] findPorts ()
		{
			return SerialPort.GetPortNames ();
		}
	}
}
