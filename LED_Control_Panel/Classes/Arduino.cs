using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace LED_Control_Panel
{
    class Arduino
    {
        private Color[] colors =new Color[4];
        private SerialPort port;

        private byte[] buf=new byte [13];

        private static Boolean initialized=false;

        public ledState[] ledStates = new ledState[4];


        private void sendColor()
        {
            if (initialized)
            {
                Program.window.SetPanelColors(this.colors);

                buf[1] = (byte)this.colors[3].R;
                buf[2] = (byte)this.colors[3].G;
                buf[3] = (byte)this.colors[3].B;

                buf[4] = (byte)this.colors[1].R;
                buf[5] = (byte)this.colors[1].G;
                buf[6] = (byte)this.colors[1].B;


                buf[7] = (byte)this.colors[0].R;
                buf[8] = (byte)this.colors[0].G;
                buf[9] = (byte)this.colors[0].B;

                buf[10] = (byte)this.colors[2].R;
                buf[11] = (byte)this.colors[2].G;
                buf[12] = (byte)this.colors[2].B;

                try
                {
                    port.Write(buf, 1, 12);
                    System.Threading.Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "COM Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }

            }
          //  else
             //   MessageBox.Show("Connection Error", "Initial Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            ;
        }

        public void init(String comPort)
        {
            try
            {
                port = new SerialPort(comPort, 115200);
                port.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "COM Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            initialized = true;

        }

        public void setColor(Color[] ncolors)
        {
            for (int i = 0; i < 4; i++)
            {
                if (ledStates[i] == ledState.CONTROL)
                    colors[i] = ncolors[i];
            }
            sendColor();

        }
        public void UpdateOff()
        {
            for (int i = 0; i < 4; i++)
            {
                if (ledStates[i] == ledState.OFF)
                    colors[i] = Color.Black;
            }
            sendColor();
        }

        public void setColor(Color ncolor)
        {
            for (int i = 0; i < 4; i++)
            {
                if (ledStates[i] == ledState.CONTROL)
                    colors[i] = ncolor;
            }
            sendColor();
        }

        
    }
}
