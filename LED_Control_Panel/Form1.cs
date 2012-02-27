using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Un4seen.Bass;



namespace LED_Control_Panel
{

    public partial class ledControlPanel : Form
    {


        //private  Arduino arduino=new Arduino();
        //private  Music music = new Music();

        public ledControlPanel()
        {
            InitializeComponent();
            cbPortsRefresh();
            for (int i = 0; i < 4; i++)
                Elements.arduino.ledStates[i] = ledState.CONTROL;

            cbMusicRefresh();


            debugTextBox.Text = ColorHandler.HSVtoColor(85, 255, 50).ToString();
        }

        public void SetPanelColors(Color[] colors)
        {
            led11.BackColor = colors[0];
            led12.BackColor = colors[0];

            led21.BackColor = colors[1];
            led22.BackColor = colors[1];

            led31.BackColor = colors[2];
            led32.BackColor = colors[2];

            led41.BackColor = colors[3];
            led42.BackColor = colors[3];


        }

        private void cbPortsRefresh()
        {
            
            cbPorts.BeginUpdate();
            cbPorts.Items.Clear();
            string[] availablePorts = System.IO.Ports.SerialPort.GetPortNames();
            for (int i = 0; i < availablePorts.Length; i++)
                cbPorts.Items.Add(availablePorts[i]);
            cbPorts.EndUpdate();
        }

        private void cbPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            Elements.arduino.init(cbPorts.SelectedItem.ToString());
        }

        private void refreshComButt_Click(object sender, EventArgs e)
        {
            cbPortsRefresh();
        }

        private void rbPanelState(int number, String state)
        {
            switch (state)
            {
                case "Off":
                    {
                        Elements.arduino.ledStates[number] = ledState.OFF;
                        Elements.arduino.UpdateOff();
                        break;
                    }
                case "Control":
                    {
                        Elements.arduino.ledStates[number] = ledState.CONTROL;
                        break;
                    }
                case "Fix":
                    {
                        Elements.arduino.ledStates[number] = ledState.FIX;
                        break;
                    }

            }
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            Panel parent = (Panel)rb.Parent;
            switch (parent.Name)
            {
                case "led1Panel":
                    {
                        rbPanelState(0, rb.Tag.ToString());
                        break;
                    }
                case "led2Panel":
                    {
                        rbPanelState(1, rb.Tag.ToString());
                        break;
                    }
                case "led3Panel":
                    {
                        rbPanelState(2, rb.Tag.ToString());
                        break;
                    }
                case "led4Panel":
                    {
                        rbPanelState(3, rb.Tag.ToString());
                        break;
                    }

            }
        }

        private void allOff_Click(object sender, EventArgs e)
        {
            led1Off.Checked = true;
            led2Off.Checked = true;
            led3Off.Checked = true;
            led4Off.Checked = true;
        }

        private void allControl_Click(object sender, EventArgs e)
        {
            led1Control.Checked = true;
            led2Control.Checked = true;
            led3Control.Checked = true;
            led4Control.Checked = true;
        }

        private void allFix_Click(object sender, EventArgs e)
        {
            led1Fix.Checked = true;
            led2Fix.Checked = true;
            led3Fix.Checked = true;
            led4Fix.Checked = true;
        }

        //----------------------------Color-----------------------------------------------

        private enum ChangeStyle
        {
            MouseMove,
            RGB,
            HSV,
            None
        }

        private ChangeStyle changeType = ChangeStyle.None;
        private Point selectedPoint;

        private ColorWheel myColorWheel;
        private ColorHandler.RGB RGB;
        private ColorHandler.HSV HSV;

        private void ColorChooser2_Load(object sender, System.EventArgs e)
        {
            // Turn on double-buffering, so the form looks better. 
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);

            // These properties are set in design view, as well, but they
            // have to be set to false in order for the Paint
            // event to be able to display their contents.
            // Never hurts to make sure they're invisible.
            pnlSelectedColor.Visible = false;
            pnlBrightness.Visible = false;
            pnlColor.Visible = false;

            // Calculate the coordinates of the three
            // required regions on the form.
            Rectangle SelectedColorRectangle = new Rectangle(pnlSelectedColor.Location, pnlSelectedColor.Size);
            Rectangle BrightnessRectangle = new Rectangle(pnlBrightness.Location, pnlBrightness.Size);
            Rectangle ColorRectangle = new Rectangle(pnlColor.Location, pnlColor.Size);

            // Create the new ColorWheel class, indicating
            // the locations of the color wheel itself, the
            // brightness area, and the position of the selected color.
            myColorWheel = new ColorWheel(ColorRectangle, BrightnessRectangle, SelectedColorRectangle);
            myColorWheel.ColorChanged +=
                new ColorWheel.ColorChangedEventHandler(this.myColorWheel_ColorChanged);

            // Set the RGB and HSV values 
            // of the NumericUpDown controls.
            SetRGB(RGB);
            SetHSV(HSV);
        }

        private void HandleMouse(object sender, MouseEventArgs e)
        {
            // If you have the left mouse button down, 
            // then update the selectedPoint value and 
            // force a repaint of the color wheel.
            if (e.Button == MouseButtons.Left)
            {
                changeType = ChangeStyle.MouseMove;
                selectedPoint = new Point(e.X, e.Y);
                this.ColorTab.Invalidate();
            }
        }

        private void frmMain_MouseUp(object sender, MouseEventArgs e)
        {
            myColorWheel.SetMouseUp();
            changeType = ChangeStyle.None;
        }

        private void SetRGBLabels(ColorHandler.RGB RGB)
        {
            RefreshText(redlbl, RGB.Red);
            RefreshText(blueLbl, RGB.Blue);
            RefreshText(greenLbl, RGB.Green);
        }

        private void SetHSVLabels(ColorHandler.HSV HSV)
        {
            RefreshText(hueLbl, HSV.Hue);
            RefreshText(satLbl, HSV.Saturation);
            RefreshText(britlbl, HSV.value);
        }

        private void SetRGB(ColorHandler.RGB RGB)
        {
            // Update the RGB values on the form.
            RefreshValue(redSc, RGB.Red);
            RefreshValue(blueSC, RGB.Blue);
            RefreshValue(greenSc, RGB.Green);
            SetRGBLabels(RGB);
            Elements.arduino.setColor(this.Color);
        }

        private void SetHSV(ColorHandler.HSV HSV)
        {
            // Update the HSV values on the form.
            RefreshValue(Hue, HSV.Hue);
            RefreshValue(Saturation, HSV.Saturation);
            RefreshValue(Brightness, HSV.value);
            SetHSVLabels(HSV);
            Elements.arduino.setColor(this.Color);
        }

        private void RefreshValue(HScrollBar hsb, int value)
        {
            hsb.Value = value;
        }

        private void RefreshText(Label lbl, int value)
        {
            lbl.Text = value.ToString();
        }

        public Color Color
        {
            // Get or set the color to be
            // displayed in the color wheel.
            get
            {
                return myColorWheel.Color;
            }

            set
            {
                // Indicate the color change type. Either RGB or HSV
                // will cause the color wheel to update the position
                // of the pointer.
                changeType = ChangeStyle.RGB;
                RGB = new ColorHandler.RGB(value.R, value.G, value.B);
                HSV = ColorHandler.RGBtoHSV(RGB);
            }
        }

        private void myColorWheel_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            SetRGB(e.RGB);
            SetHSV(e.HSV);
        }

        private void HandleHSVScroll(object sender, ScrollEventArgs e)
        // If the H, S, or V values change, use this 
        // code to update the RGB values and invalidate
        // the color wheel (so it updates the pointers).
        // Check the isInUpdate flag to avoid recursive events
        // when you update the NumericUpdownControls.
        {
            changeType = ChangeStyle.HSV;
            HSV = new ColorHandler.HSV(Hue.Value, Saturation.Value, Brightness.Value);
            SetRGB(ColorHandler.HSVtoRGB(HSV));
            SetHSVLabels(HSV);
            this.ColorTab.Invalidate();
        }

        private void HandleRGBScroll(object sender, ScrollEventArgs e)
        {
            // If the R, G, or B values change, use this 
            // code to update the HSV values and invalidate
            // the color wheel (so it updates the pointers).
            // Check the isInUpdate flag to avoid recursive events
            // when you update the NumericUpdownControls.
            changeType = ChangeStyle.RGB;
            RGB = new ColorHandler.RGB(redSc.Value, greenSc.Value, blueSC.Value);
            SetHSV(ColorHandler.RGBtoHSV(RGB));
            SetRGBLabels(RGB);
            this.ColorTab.Invalidate();
        }

        private void ColorChooser2_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Depending on the circumstances, force a repaint
            // of the color wheel passing different information.
            switch (changeType)
            {
                case ChangeStyle.HSV:
                    myColorWheel.Draw(e.Graphics, HSV);
                    break;
                case ChangeStyle.MouseMove:
                case ChangeStyle.None:
                    myColorWheel.Draw(e.Graphics, selectedPoint);
                    break;
                case ChangeStyle.RGB:
                    myColorWheel.Draw(e.Graphics, RGB);
                    break;
            }

        }

        //---------------------------Music-------------------------------------

        private void cbMusicRefresh()
        {
            BASS_DEVICEINFO[] devs = GetDeviceList();
            cbMusicDevices.BeginUpdate();
            cbMusicDevices.Items.Clear();
            for (int i = 0; i < devs.Length; i++)
                cbMusicDevices.Items.Add(devs[i]);
            cbMusicDevices.DisplayMember = "name";
            cbMusicDevices.EndUpdate();
        }

        private void RefreshMusicDevices_Click(object sender, EventArgs e)
        {
            cbMusicRefresh();
        }

        private void cbMusicDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            init(cbMusicDevices.SelectedIndex);
        }

        private bool musicStarted = false;
        private void MusicStart_Click(object sender, EventArgs e)
        {
            if (musicStarted)
            {
                MusicStart.Text = "Start";
                updateTimer.Stop();
            }
            else
            {
                MusicStart.Text = "Stop";
                updateTimer.Start();
            }
        }


      
    }


    public enum ledState
    {
        CONTROL, OFF, FIX
    }
    static class Elements
    {
        public static Arduino arduino = new Arduino();
        public static ColorHandler colorHSV = new ColorHandler();
    }
}