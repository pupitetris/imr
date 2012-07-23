using System;
using System.IO.Ports;
using System.Text;

namespace IMRpatient
{
	public class Radionic
	{
		public enum RESULT {
			SUCCESS = 0,
			TIMEOUT,
			CANCEL,
			ASSERT_IDLE,
			BAD_RESPONSE
		}
			
		public delegate void CommandCompleteCB (RESULT result, string reply);

		private enum STATE {
			IDLE = 0,
			CMD_PROBE
		}
		
		private static readonly string REQUEST_PROBE = "W";
		private static readonly string REPLY_PROBE = "284a-er45-FG34-09%#-12w+q";

		private static readonly int MAX_POLLS = 10;
		private static readonly uint POLL_INTERVAL = 150;
		
		public string Port { get; set; }

		private SerialPort serial;
		private STATE state;
		private CommandCompleteCB cb;
		private StringBuilder builder;
		private string expectStr;
		private int polls;

		/* private void handleDataReceived (object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
		{
			processExisting ();
		} */

		public Radionic ()
		{
			Port = null;
			builder = new StringBuilder ();
			state = STATE.IDLE;
			polls = 0;

			serial = new SerialPort ();
			serial.BaudRate = 2400;
			serial.DataBits = 8;
			serial.Parity = Parity.None;
			serial.StopBits = StopBits.One;
			serial.Handshake = Handshake.None;

			// This is not supported by Mono.
			// serial.DataReceived += new SerialDataReceivedEventHandler (handleDataReceived);
		}

		public void Open ()
		{
			serial.PortName = Port;
			serial.Open ();
		}

		public void Close () 
		{
			if (serial.IsOpen) {
				state = STATE.IDLE;
				serial.Close ();
			}
		}

		private void finishCmd (RESULT result)
		{
			if (cb != null) {
				cb (result, builder.ToString ());
			}
			
			cb = null;
			polls = 0;
			state = STATE.IDLE;
			builder.Clear ();
		}
		
		private bool processExisting ()
		{
			builder.Append (serial.ReadExisting ());
			
			if (state == STATE.IDLE) {
				return true;
			}
			
			if (builder.ToString ().Length >= expectStr.Length) {
				finishCmd ((builder.ToString () == expectStr)?
				           RESULT.SUCCESS: RESULT.BAD_RESPONSE);
				return false;
			}
			return true;
		}
		
		private bool checkBytesToRead ()
		{
			polls ++;
			if (polls >= MAX_POLLS) {
				finishCmd (RESULT.TIMEOUT);
			}

			if (serial.BytesToRead > 0) {
				return processExisting ();
			}

			if (state == STATE.IDLE) {
				return false;
			}
			return true;
		}

		private void startCmd (string data, string expect, STATE cmd, CommandCompleteCB cb)
		{
			this.cb = cb;

			if (state != STATE.IDLE) {
				finishCmd (RESULT.ASSERT_IDLE);
				return;
			}

			state = cmd;
			expectStr = expect;
			polls = 0;

			serial.Write (data);
			if (expect != null) {
				GLib.Timeout.Add (POLL_INTERVAL, new GLib.TimeoutHandler (checkBytesToRead));
			} else {
				finishCmd (RESULT.SUCCESS);
			}
		}

		public void Cancel ()
		{
			finishCmd (RESULT.CANCEL);
		}

		public void Probe (CommandCompleteCB cb = null)
		{
			startCmd (REQUEST_PROBE, REPLY_PROBE, STATE.CMD_PROBE, cb);
		}

		public static string[] findPorts ()
		{
			return SerialPort.GetPortNames ();
		}
	}
}
