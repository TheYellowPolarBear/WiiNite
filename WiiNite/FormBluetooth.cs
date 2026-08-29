using System;
using System.Collections;
using System.Windows.Forms;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using WiimoteLib;
using System.Media;

namespace WiiNite
{
    public partial class FormBluetooth : Form
    {
        public FormBluetooth() //could be removed(?)
        {
            InitializeComponent();

            try
            {
                using (var btClient = new BluetoothClient())
                {
                    var btPin = AddressToWiiPin(BluetoothRadio.PrimaryRadio.LocalAddress.ToString());
                    
                    BalanceWalker.FormMain.consoleBoxWriteLine(btPin.ToString());
                }
            }
            catch (Exception ex)
            {
                
            }
        }


        private string AddressToWiiPin(string bluetoothAddress)
        {
            if (bluetoothAddress.Length != 12) throw new Exception("Invalid Bluetooth Address: " + bluetoothAddress);

            var bluetoothPin = "";
            bool doubleZeroInAddr = false;
            for (int i = bluetoothAddress.Length - 2; i >= 0; i -= 2)
            {
                string hex = bluetoothAddress.Substring(i, 2);
                bluetoothPin += (char)Convert.ToInt32(hex, 16);
                if (hex == "00") doubleZeroInAddr = true;
            }
            if (doubleZeroInAddr)
            { 
                doubleZeroMsgBox(bluetoothAddress);
                return "Invalid bt MAC address";
            }
            return bluetoothPin;
        }
        private void doubleZeroMsgBox(string bluetoothAddress)
        {
            string address = bluetoothAddress.Substring(0,2);
            for (int i = 2; i <= bluetoothAddress.Length - 2; i += 2)
            {
                address = address + ":" + bluetoothAddress.Substring(i, 2);
            }
            string caption = "Permanent PIN code could not be created for device: " + address;
            string message = "Your bluetooth device MAC address (" + address +  ") contains \"00\" in it, which means permanent connection with special PIN code is not possible. (You can still connect to your wii balance board, but establishing the Bluetooth connection manually becomes annoying quickly.) To resolve this issue, you can change your bluetooth device MAC address. Use any address without \"00\" in it. I used this tool: https://macaddresschanger.com. click OK to visit URL.";
            MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
            DialogResult result;
            result = MessageBox.Show(message, caption, buttons);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                System.Diagnostics.Process.Start("https://macaddresschanger.com");
            }
        }

        private void RemoveExisting_Click(object sender, EventArgs e) //this has been removed in the form
        {
            ((Button)sender).Enabled = false;
            var btRemoved = 0;
            try
            {
                using (var btClient = new BluetoothClient())
                {
                    // Find remembered bluetooth devices.

                   
                    var btExistingList = btClient.DiscoverDevices(255, false, true, false);

                    foreach (var btItem in btExistingList)
                    {
                        if (!btItem.DeviceName.Contains("Nintendo")) continue;

                        btRemoved++;
                        BluetoothSecurity.RemoveDevice(btItem.DeviceAddress);
                        btItem.SetServiceState(BluetoothService.HumanInterfaceDevice, false);
                    }
                }
                
            }
            catch (Exception ex)
            {
                
            }
            ((Button)sender).Enabled = true;
            
        }

        private void dapBtn_Click(object sender, EventArgs e)
        {
            // XP & Vista use different control panel settings for Bluetooth pairing
            // NT 5.1/2 or 6.0 indicate XP/Vista
            string oldLink = "bthprops.cpl";
            string newLink = "shell:::{A8A91A66-3A7D-4424-8D24-04E180695C7A}";
            var osMajor = Environment.OSVersion.Version.Major;
            var osMinor = Environment.OSVersion.Version.Minor;
            bool isXPorVista = osMajor == 5 || (osMajor == 6 && osMinor == 0);
            string link = isXPorVista ? oldLink : newLink;

            try
            {
                System.Diagnostics.Process.Start(link);
            }
            catch (Exception ex)
            {
                DialogResult result;
                result = MessageBox.Show("Invalid Control Panel item or operating system. Please refer to the GitHub README.", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    System.Diagnostics.Process.Start("https://github.com/TheYellowPolarBear/WiiNite/blob/master/README.md");
                }
            }
        }

        private void FormBluetooth_Load(object sender, EventArgs e)
        {
             
            label1.Text = "You are using Windows NT " + Environment.OSVersion.Version.Major.ToString() + "." + Environment.OSVersion.Version.Minor.ToString();
        }
    }
}
