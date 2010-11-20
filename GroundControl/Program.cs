
using System;
using System.Threading;
using System.Text;
using System.IO.Ports;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.USBHost;
using GHIElectronics.NETMF.Hardware;

namespace GroundControl
{
    public class Program
    {
        enum sticks { left = 0, right = 1 };

        const short DEADZONE = 50;
        const short MINCHANGE = 10; // minimum change for a joystick movement to be used

        static USBH_Joystick[] joysticks; // all the currently connected joysticks
        
        static short jCount = 0; 

        static SerialPort Radio = null; // zigbee radio
        public static byte[] tx_data;

        
        static short leftPower = 0;
        static short rightPower = 0;
        static bool leftEnabled = true; // gets cleared after the
        static bool rightEnabled = true;
        public static void Main()
        {
            Init();

            while(true){
                SendCommands();
                Thread.Sleep(20);
            }

            Thread.Sleep(Timeout.Infinite); 
        }

        public static void Init()
        {
            joysticks = new USBH_Joystick[8];       
            Radio = new SerialPort("COM1", 9600);
            Radio.Open();
            USBHostController.DeviceConnectedEvent += DeviceConnectedEvent;
        }

        public static void checkDevices()
        {
        }

        static void DeviceConnectedEvent(USBH_Device device)
        {
            if ((int)device.TYPE == 2)
            {
                joysticks[jCount] = new USBH_Joystick(device);
                joysticks[jCount].JoystickXYMove += JoystickXYMove;
                jCount++;
            }
        }

        static void JoystickXYMove(USBH_Joystick sender, USBH_JoystickEventArgs args)
        {
            int x = 0, y = 0;

            y = (int)(sender.Cursor.Y / 10.24);
            if (y < 0) y -= 50;
            else if (y > 0) y += 50;

            if (y > 100) y = 100;
            else if (y < -100) y = -100;

            if (y < 55 && y > 0)
                y = 0;
            else if (y < 0 && y > -55)
                y = 0;

            if (sender == joysticks[(int)sticks.left])
            {
                joysticks[(int)sticks.left].JoystickXYMove -= JoystickXYMove;
                leftPower = (short)y;
                leftEnabled = false;
            }
            else if (sender == joysticks[(int)sticks.right])
            {
                joysticks[(int)sticks.right].JoystickXYMove -= JoystickXYMove;
                rightPower = (short)y;
                rightEnabled = false;
            }


            //Debug.Print("(x, y) = (" + x + ", " + y + ")");
        }

        static void SendCommands()
        {
            EnableJoysticks();
            String outStr = "$" + leftPower.ToString() + "," + rightPower.ToString() + "*";
            tx_data = Encoding.UTF8.GetBytes(outStr);
            Debug.Print(outStr);
            Radio.Write(tx_data, 0, tx_data.Length);
        }

        static void EnableJoysticks()
        {
            if(joysticks[(int)sticks.left] != null)
                if (!leftEnabled)
                {
                    joysticks[(int)sticks.left].JoystickXYMove += JoystickXYMove;
                    leftEnabled = true;
                }
            if (joysticks[(int)sticks.right] != null)
                if (!rightEnabled)
                {
                    joysticks[(int)sticks.right].JoystickXYMove += JoystickXYMove;
                    rightEnabled = true;
                }
        }
    }
}
