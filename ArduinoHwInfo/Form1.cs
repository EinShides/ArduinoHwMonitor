using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using LibreHardwareMonitor.Hardware;

namespace ArduinoHwInfo
{
    public partial class Form1 : Form
    {
        //Libre Hardware Monitor declaration for CPU and GPU only
        Computer c = new Computer()
        {
            IsGpuEnabled = true,
            IsCpuEnabled = true,
        };


        //Variables and arrays declaration
        double CPUTemp, CPULoad, GPUCoreClock, GPUMemTemp, GPUHotSpotTemp, GPUVramSize, GPUVramUsage, GPUVramUsagePercent;
        float GPUCoreTemp, GPULoad;
        int[] cpumhzvalue = { 0, 0, 0, 0, 0, 0, 0, 0 };


        //New serial port
        private SerialPort port = new SerialPort();
        
        //Form Main process
        public Form1()
        {
            InitializeComponent();
            Init();
            c.Open();
            Start();
        }

        //Form Inizialization
        private void Init()
        {
            try
            {
                notifyIcon1.Visible = false;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.DataBits = 8;
                port.Handshake = Handshake.None;
                port.RtsEnable = true;
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    comboBox1.Items.Add(port);
                }
                port.BaudRate = 115200;                    //IMPORTANT: Change this to match your arduino serial rate

                //Load saved COM port
                string savedValue = Properties.Settings.Default.COMPortValue;
                if (!string.IsNullOrEmpty(savedValue) && comboBox1.Items.Contains(savedValue))
                {
                    comboBox1.SelectedItem = savedValue;
                    port.PortName = savedValue;
                }

                //Load saved update interval value
                string savedUpdateValue = Properties.Settings.Default.UpdateValue;
                if (!string.IsNullOrEmpty(savedUpdateValue))
                {
                    comboBox2.SelectedItem = savedUpdateValue;
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Start Monitoring
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null && comboBox2.SelectedItem != null)
            {
                // Saves the selected COM port value and update interval
                Properties.Settings.Default.COMPortValue = comboBox1.SelectedItem.ToString();
                Properties.Settings.Default.UpdateValue = comboBox2.SelectedItem.ToString();
                Properties.Settings.Default.Save();

                port.PortName = comboBox1.SelectedItem.ToString();
                Start();
            }

            else
            {
                MessageBox.Show("Please select a COM port or update interval.");
            }
        }
        

        //Minimize form
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        //Restore form 1
        private void notifyIcon1_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
                Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
        }

        //Stop Monitoring
        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                port.Write("DISa");
                port.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            toolStripStatusLabel1.Text = "Disconnected";
            timer1.Enabled = false;
        }

        //Restore Form
        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            this.notifyIcon1.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        //Timer Execution
        private void timer1_Tick(object sender, EventArgs e)
        {
            Status();
        }

        //Serial selection and start monitoring
        private void Start()
        {
            try
            {
                if (!port.IsOpen)
                {
                    port.PortName = comboBox1.Text;
                    port.Open();
                    timer1.Interval = Convert.ToInt32(comboBox2.Text);
                    timer1.Enabled = true;
                    timer1.Tick += new EventHandler(timer1_Tick);
                    toolStripStatusLabel1.Text = "Connected";
                    notifyIcon1.BalloonTipText = "Connected";
                    notifyIcon1.ShowBalloonTip(5);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        
        //Saves the selected COM port Value on close
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.COMPortValue = comboBox1.SelectedItem.ToString();
            Properties.Settings.Default.UpdateValue = comboBox2.SelectedItem.ToString();
            Properties.Settings.Default.Save();
        }


        //Loads com port value
        private void Form1_Load(object sender, EventArgs e)
        {
            string savedCOMPortValue = Properties.Settings.Default.COMPortValue;
            if (!string.IsNullOrEmpty(savedCOMPortValue))
            {
                comboBox1.SelectedItem = savedCOMPortValue;
            }

            string savedUpdateValue = Properties.Settings.Default.COMPortValue;
            if (!string.IsNullOrEmpty(savedUpdateValue))
            {
                comboBox2.SelectedItem = savedUpdateValue;
            }
        }


        //Monitoring Function
        private void Status()
        {
            foreach (var hardware in c.Hardware)
            {
                //TEST GPU
                if (hardware.Name.Contains("NVIDIA GeForce RTX 3080"))
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        //Get GPU Core Temp
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("GPU Core"))
                        {
                            GPUCoreTemp = sensor.Value.GetValueOrDefault();
                        }
                        //Get GPU Core Clock
                        if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("GPU Core"))
                        {
                            GPUCoreClock = Math.Round(sensor.Value.GetValueOrDefault(), 0);
                        }
                        //Get GPU Memory Junction Temp
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("GPU Memory Junction"))
                        {
                            GPUMemTemp = Math.Round(sensor.Value.GetValueOrDefault(), 0);
                        }
                        //Get GPU HotSpot Temp
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("GPU Hot Spot"))
                        {
                            GPUHotSpotTemp = Math.Round(sensor.Value.GetValueOrDefault(), 0);
                        }
                        //Get GPU Load
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("GPU Core"))
                        {
                            GPULoad = sensor.Value.GetValueOrDefault();
                        }
                        //Get GPU Total VRAM Size
                        if (sensor.SensorType == SensorType.SmallData && sensor.Name.Contains("GPU Memory Total"))
                        {
                            GPUVramSize = sensor.Value.GetValueOrDefault();
                        }
                        //Get GPU VRAM Usage
                        if (sensor.SensorType == SensorType.SmallData && sensor.Name.Contains("GPU Memory Used"))
                        {
                            GPUVramUsage = sensor.Value.GetValueOrDefault();
                        }
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("GPU Memory"))
                        {
                            GPUVramUsagePercent = Math.Round(sensor.Value.GetValueOrDefault(), 0);
                        }

                    }

                }

                //TEST CPU
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors) {

                        //Get CPU Frequency Array
                        for (int i = 0; i < 8; i++)
                        {
                            if (sensor.SensorType == SensorType.Clock && sensor.Name.Equals("Core #" + (i + 1)))
                            {
                                cpumhzvalue[i] = (int)Math.Round(sensor.Value.GetValueOrDefault(), 0);
                            }
                        }

                        //Get CPU Temp
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core (Tctl/Tdie)"))
                        {
                            CPUTemp = Math.Round(sensor.Value.GetValueOrDefault(), 0);
                        }

                        //Get CPU Total Utilization
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("CPU Total"))
                        {
                            CPULoad = Math.Round(sensor.Value.GetValueOrDefault(), 0);
                        }
                    }
                }


                //Average Ghz Core
                double mhzAvg = Math.Round(cpumhzvalue.Average(), 0);

                //VRAM Usage %
                int vramPercentage = (int)(GPUVramUsage * 100 / GPUVramSize);


                try
                {
                    port.Write(CPUTemp + "a" + mhzAvg.ToString() + "b" + GPUCoreTemp + "c" + GPUMemTemp + "d" + GPUHotSpotTemp + "e" + GPUCoreClock + "f" + GPUVramUsagePercent + "g" + CPULoad + "h" + GPULoad + "i");
                }
                catch (Exception ex)
                {
                    timer1.Stop();
                    MessageBox.Show(ex.Message);
                }

            }
        }
    }
}
