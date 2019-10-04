using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace livegg_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool communication_ok = false;
        SerialPort serialPort1 = new SerialPort();
        public SerialPort SerialPort1 { get => serialPort1; set => serialPort1 = value; }
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();


        public MainWindow()
        {
            InitializeComponent();
            communication_procedure();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////// COMMUNICATION /////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void communication_procedure()
        {
            byte[] response = new byte[7];

            SerialPort1.BaudRate = 115200;
            SerialPort1.Handshake = System.IO.Ports.Handshake.None;
            SerialPort1.Parity = Parity.None;
            SerialPort1.DataBits = 8;
            SerialPort1.StopBits = StopBits.One;
            SerialPort1.ReadTimeout = 1000;
            SerialPort1.WriteTimeout = 1000;
            string[] port_name = System.IO.Ports.SerialPort.GetPortNames();
            string response_string;
            if (port_name.Length == 0)
            {
                communication_status_update(communication_ok, "NO OPEN COMM PORTS");
                return;
            }
            for (int comm_index = 0; comm_index < port_name.Length; comm_index++)
            {
                if (SerialPort1.IsOpen == true)
                    SerialPort1.Close();
                SerialPort1.PortName = port_name[comm_index];
                try
                {
                    SerialPort1.Open();
                }
                catch
                {
                    SerialPort1.Close();
                    continue;
                }
                try
                {
                    SerialPort1.WriteLine("COMM_ALIVE");
                    response_string = SerialPort1.ReadLine();
                    if (response_string == "AA")
                    {
                        communication_ok = true;
                        break;
                    }
                    else
                        SerialPort1.Close();
                    communication_status_update(communication_ok, "NO COMMUNICATION WITH DEVICE");
                }
                catch (Exception ex)
                {
                    SerialPort1.Close();
                    continue;
                }
            }
            communication_status_update(communication_ok, "NO COMMUNICATION WITH DEVICE");
        }

        private void communication_status_update(bool communication_status, string communication_error_message_string)
        {
            if (communication_status == true)
            {
                communication_status_message_textblock.Foreground = Brushes.Green;
                communication_status_message_textblock.Text = "CONNECTION SUCCEEDED";
            }
            else
            {
                communication_status_message_textblock.Foreground = Brushes.Red;
                communication_status_message_textblock.Text = communication_error_message_string;
            }
        }

        private Boolean is_tray_id_valid_string(string checked_string)
        {
            for (Byte character_index = 0; character_index < checked_string.Length; character_index++)
            {
                if (Char.IsDigit(checked_string[character_index]) != true && Char.IsUpper(checked_string[character_index]) != true)
                    return false;
            }
            return true;
        }

        private void timer_initialization()
        {
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            tray_id_error_label.Content = "";
            //PORT_NUMBER_ERROR_LABEL.Content = "";
            dispatcherTimer.Stop();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string tray_id = tray_id_textbox.Text;
            string port_number_string = port_textbox.Text;
            string response_string;
            UInt16 port_number = 0;
            uint record_number, number_of_records;

            if (communication_ok == true)
            {
                if (tray_id.Length != 12 || is_tray_id_valid_string(tray_id) == false || UInt16.TryParse(port_number_string, out port_number) == false || port_number < 9000 || port_number > 11000)
                {
                    tray_id_error_label.Content = "";
                    /*if (UInt16.TryParse(port_number_string, out port_number) == false)
                        PORT_NUMBER_ERROR_LABEL.Content = "ILLEGAL NUMBER";
                    else if (port_number < 9000 || port_number > 11000)
                        PORT_NUMBER_ERROR_LABEL.Content = "PORT NUMBER MUST BE BETWEEN 9000-11000";*/
                    if (tray_id.Length != 12)
                        tray_id_error_label.Content = "TRAY ID MUST BE 12 CHARACTERS";
                    else if (is_tray_id_valid_string(tray_id) == false)
                        tray_id_error_label.Content = "TRAY ID MUST HAVE ONLY DIGITS OR CAPITAL LETTERS";
                    timer_initialization();
                }
                else
                {
                    serialPort1.WriteLine("SET_TRAY " + tray_id_textbox.Text + " " + port_textbox.Text);
                    /*response_string = serialPort1.ReadLine();
                    string[] records = response_string.Split(' ');
                    if (uint.TryParse(records[0], out record_number) == true && uint.TryParse(records[1], out number_of_records) == true)
                        records_label.Content = "TRAY-ID RECORD NUMBER " + record_number.ToString() + " FROM " + number_of_records.ToString();*/
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UInt16 port_number;
            string port_number_and_records_response;
            Byte[] tray_response = new Byte[12];
            UInt32 ip_number;

            ///try
            ////{
            serialPort1.WriteLine("GET_TRAY");
            /////while (serialPort1.BytesToRead < 12) ;
            /////serialPort1.Read(tray_response,0,12);
            /*string tray_id_string = System.Text.Encoding.UTF8.GetString(tray_response, 0, tray_response.Length);
            if (is_tray_id_valid_string(tray_id_string) == true)
                textBox_tray_id.Text = tray_id_string;
            else
                textBox_tray_id.Text = "NON-VALID TRAY ID";*/
            port_number_and_records_response = serialPort1.ReadLine();
            string[] port_number_and_records_response_array = port_number_and_records_response.Split(' ');
            tray_id_textbox.Text = port_number_and_records_response_array[0];
            port_textbox.Text = port_number_and_records_response_array[1];

            ///   records_label.Content = "TRAY-ID RECORD NUMBER " + record_number.ToString() + " FROM " + number_of_records.ToString();
            if (UInt32.TryParse(port_number_and_records_response_array[2], out ip_number) == true)
            {
                uint bigEndian = (uint)IPAddress.HostToNetworkOrder((int)ip_number);
                byte[] ip_array = BitConverter.GetBytes(bigEndian);
                ip_textbox.Text = ip_array[3].ToString() + "." + ip_array[2].ToString() + "." + ip_array[1].ToString() + "." + ip_array[0].ToString();
            }
            /*}
            catch
            {
                //textBox_port_number.Text = "CANNOT READ PORT";
            }*/
        }

        private void Reconnect_button_Click(object sender, RoutedEventArgs e)
        {
            communication_procedure();
        }

        private void Main_tabcontrol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl) //if this event fired from TabControl then enter
            {
                if (device.IsSelected)
                {
                    if (communication_ok == true)
                    {
                        //////////////////////////////////////////////////
                        /////////////////   DEVICE ID   //////////////////
                        //////////////////////////////////////////////////
                        byte[] device_id_bytes = new byte[24];
                        UInt32 timeout = 100000;
                        SerialPort1.WriteLine("DEVICE_ID");
                        while (serialPort1.BytesToRead < 24 && --timeout > 0) ;
                        serialPort1.ReadTimeout = 250;
                        try
                        {
                            serialPort1.Read(device_id_bytes, 0, 24);
                        }
                        catch
                        {
                            return;
                        }
                        device_id_textbox.Text = System.Text.Encoding.Default.GetString(device_id_bytes);

                        //////////////////////////////////////////////////
                        /////////////////   VERSIONS   ///////////////////
                        //////////////////////////////////////////////////
                        byte[] version_parts = new byte[4];
                        serialPort1.WriteLine("VERSIONS");
                        string versions_string = serialPort1.ReadLine();

                        int version_int;
                        Int32.TryParse(versions_string , out version_int);
                        uint bigEndian = (uint)IPAddress.HostToNetworkOrder((int)version_int);
                        byte[] version_array = BitConverter.GetBytes(bigEndian);
                        boot_version_textbox.Text = version_array[0].ToString() + "." + version_array[1].ToString() + "." + version_array[2].ToString() + "." + version_array[3].ToString();


                        /*for( int parts_number_index = 0; parts_number_index < versions_string.Length ; parts_number_index++ )
                        {
                            version_parts[parts_number_index] = versions_string[]
                        }*/
                        /*boot_version_textbox.Text = versions_string;
                        byte[] ip_array = BitConverter.GetBytes(versions_string);
                        boot_version_textbox.Text = ip_array[3].ToString() + "." + ip_array[2].ToString() + "." + ip_array[1].ToString() + "." + ip_array[0].ToString();
                        */
                        //////////////////////////////////////////////////
                        /////////////////   DEVICE ID   //////////////////
                        //////////////////////////////////////////////////
                        string[] version = ((string)version_label.Content).Split(' ');
                        gui_version_textbox.Text = version[2];
                    }
                }
            }
        }
    }
}
