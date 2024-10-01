using System.IO.Ports;
using System.Windows.Forms;
using WindowsMotors.DataClasses;



namespace WindowsMotors
{
	public partial class Form1 : Form
	{
		MSPClient client;


		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			cmbCommand.SelectedIndex = 0;
			client = new MSPClient();
			client.onData += Client_onData;
		}

		private void Client_onData(MSPClient sender, byte[] rawData)
		{
			MSPResponse response = MSPClient.ParseResponse(rawData);
			this.Invoke(new Action(() =>
			{
				txtOutput.AppendText(response.ToString());
			}));

		}

		private void button1_Click(object sender, EventArgs e)
		{
			client.SendCommand(textBox1.Text);
		}



		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			client.Close();
		}



		private void SimplifyLog()
		{
			string[] lines = File.ReadAllLines(@"c:\log.txt");
			lines = lines.Where(l => !l.Contains("Write ")).ToArray();
			lines = lines.Where(l => l.Length > 0).ToArray();
			lines = lines.Select(l => l.Trim().Split("$M<")[0].Trim()).ToArray();
			File.WriteAllLines(@"c:\log2.txt", lines);
		}


		private void button2_Click(object sender, EventArgs e)
		{


			client.SendCommand(MSPClient.MSPCommand.MSP_SET_MOTOR, new MotorSpeedRequest() { motor1Speed = 1500, motor2Speed = 1500, motor3Speed = 1500, motor4Speed = 1500 });
			Thread.Sleep(5000);

			client.SendCommand(MSPClient.MSPCommand.MSP_SET_MOTOR, new MotorSpeedRequest() { motor1Speed = 1000, motor2Speed = 1000, motor3Speed = 1000, motor4Speed = 1000 });


		}


		private void button3_Click_1(object sender, EventArgs e)
		{
			int command = int.Parse(cmbCommand.Text.Split("(")[1].Replace(")", ""));
			switch ((MSPClient.MSPCommand)command)
			{
				case MSPClient.MSPCommand.MSP_STATUS:
					client.SendCommand(MSPClient.MSPCommand.MSP_STATUS, new byte[] { });
					break;
				case MSPClient.MSPCommand.MSP_RAW_IMU:
					client.SendCommand(MSPClient.MSPCommand.MSP_RAW_IMU, new byte[] { });
					break;
				case MSPClient.MSPCommand.MSP_SERVO:
					client.SendCommand(MSPClient.MSPCommand.MSP_SERVO, new byte[] { });
					break;
				case MSPClient.MSPCommand.MSP_MOTOR:
					client.SendCommand(MSPClient.MSPCommand.MSP_MOTOR, new byte[] { });
					break;
				case MSPClient.MSPCommand.MSP_RC:
					client.SendCommand(MSPClient.MSPCommand.MSP_MOTOR, new byte[] { });
					break;
				case MSPClient.MSPCommand.MSP_ATTITUDE:
					client.SendCommand(MSPClient.MSPCommand.MSP_ATTITUDE, new byte[] { });
					break;
				case MSPClient.MSPCommand.MSP_ALTITUDE:
					client.SendCommand(MSPClient.MSPCommand.MSP_ALTITUDE, new byte[] { });
					break;
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{

		}
	}
}