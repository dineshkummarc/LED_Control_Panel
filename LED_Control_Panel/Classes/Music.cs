using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;
using System.Windows.Forms;
using System.Drawing;


namespace LED_Control_Panel
{
    partial class ledControlPanel
    {

        public int recChannel;
        private RECORDPROC myRecProc;
        private Un4seen.Bass.BASSTimer updateTimer;
        private bool inicialized=false;
        private Un4seen.Bass.Misc.Visuals temp = new Un4seen.Bass.Misc.Visuals();
        private Color[] colors = new Color[4];

        public BASS_DEVICEINFO[] GetDeviceList()
        {
             return Bass.BASS_RecordGetDeviceInfos();
        }

        private bool MyRecording(int handle, IntPtr buffer, int length, IntPtr user)
        {
            return true;
        }

        public void init(int Device)
        {
            if(inicialized)
            {
                Bass.BASS_RecordFree();
            }

            Bass.BASS_RecordInit(-1);
            Un4seen.Bass.BASSError error;
            if((error=Bass.BASS_ErrorGetCode())!=0)
                MessageBox.Show("Bass Error Code: "+error, "Misuc Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            Bass.BASS_RecordSetDevice(Device);
            if ((error = Bass.BASS_ErrorGetCode()) != 0)
                MessageBox.Show("Bass Error Code: " + error, "Misuc Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            

            myRecProc = new RECORDPROC(MyRecording);
            recChannel = Bass.BASS_RecordStart(22050, 2, BASSFlag.BASS_RECORD_PAUSE, myRecProc, IntPtr.Zero);
            Bass.BASS_ChannelPlay(recChannel, false);
            updateTimer = new Un4seen.Bass.BASSTimer(50);
            updateTimer.Tick += new EventHandler(timerUpdate_Tick);
            inicialized = true;
        }

        public float[] fft = new float[256];
        private void timerUpdate_Tick(object sender, System.EventArgs e)
        {
            Bass.BASS_ChannelGetData(recChannel, fft, (int)BASSData.BASS_DATA_FFT512);
            colorMusic2(fft);
            DrawSpectrum();
        }

        private double calculateMax(float[] arr, int begin, int end)
        {
            double result=0;
            //int counter = 0;
            for (int i=begin;i<=end;i++)
            {
                //counter++
                if(result<arr[i])
                    result=arr[i];
            }
            return result;
        }


        private double calculateMiddle(float[] arr, int begin, int end)
        {
            double result = 0;
            int counter = 0;
            for (int i = begin; i <= end; i++)
            {
                counter++;
                result += arr[i];
            }
            return result/counter;
        }

        private void colorMusicClassic(float[] fft)
        {
            
            double ampR = Math.Sqrt(calculateMax(fft,0,5)) * 4;
            double ampY = Math.Sqrt(calculateMax(fft, 6, 19)) * 4;
            double ampG = Math.Sqrt(calculateMax(fft, 20, 81)) * 4;
            double ampB = Math.Sqrt(calculateMax(fft, 82, 254)) * 4;
            
            

            if (ampR > 4*ampRtrackbar.Value/100)
                colors[0] = Color.Red;
            else
                colors[0] = Color.Black;

            if (ampY > 4 * ampYtrackbar.Value / 100)
                colors[1] = Color.Yellow;
            else
                colors[1] = Color.Black;

            if (ampG > 4 * ampGtrackbar.Value / 100)
                colors[2] = Color.Green;
            else
                colors[2] = Color.Black;

            if (ampB > 4 * ampBtrackbar.Value / 100)
                colors[3] = Color.Blue;
            else
                colors[3] = Color.Black;

            Elements.arduino.setColor(colors);
            Application.DoEvents();
            debugTextBox.Text = ampR.ToString() + "\n" + ampY.ToString() + "\n" + ampG.ToString() + "\n" + ampB.ToString()+"\n"+specIdx;
 
        }

        private void colorMusic1(float[] fft)
        {
            float[] amps = new float[4];

            if ((amps[0] = (float)Math.Pow(calculateMiddle(fft, 0, 5), 1)) > ampRtrackbar.Value / 100)
                colors[0] = ColorHandler.FromAhsb(255, Color.Red.GetHue(), 1f, amps[0]);
            else
                colors[0] = Color.Black;

            if ((amps[1] = (float)Math.Pow(calculateMiddle(fft, 6, 19), 1)) > ampYtrackbar.Value/100)
                colors[1] = ColorHandler.FromAhsb(255, Color.Yellow.GetHue(), 1f, amps[1]);
            else
                colors[1] = Color.Black;

            if ((amps[2] = (float)Math.Pow(calculateMiddle(fft, 20, 81), 1)) > ampGtrackbar.Value / 100)
                colors[2] = ColorHandler.FromAhsb(255, Color.Green.GetHue(), 1f, amps[2]);
            else
                colors[2] = Color.Black;

            if ((amps[3] = (float)Math.Pow(calculateMiddle(fft, 82, 116), 1)) > ampBtrackbar.Value / 100)
                colors[3] = ColorHandler.FromAhsb(255, Color.Blue.GetHue(), 1f, amps[3]);
            else
                colors[3] = Color.Black;


            Elements.arduino.setColor(colors);
            Application.DoEvents();
            debugTextBox.Text = (ColorHandler.HSVtoColor(0, 255,(int)calculateMiddle(fft, 0, 5) * 255).ToString() + '\n' + (calculateMiddle(fft, 6, 19) * 255).ToString() + "\n" +
                ((int)(calculateMiddle(fft, 20, 81) * 255 )).ToString() + '\n' + (calculateMiddle(fft, 82, 116) * 255)).ToString() +
                "\n" + colors[0].ToString() + "\n" + colors[1].ToString() + "\n" + colors[2].ToString() + "\n" + colors[3].ToString();


        }


        private void colorMusic2(float[] fft)
        {
            int x;
            double[] y=new double[8];
            int b0=0;
            int Bands=8;
            for (x = 0; x < Bands; x++)
            {
                float peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (Bands - 1));
                if (b1 > 254) b1 = 254;
                if (b1 <= b0) b1 = b0 + 1; // make sure it uses at least 1 FFT bin
                for (; b0 < b1; b0++)
                    if (peak < fft[1 + b0]) peak = fft[1 + b0];
                y[x] = Math.Sqrt(peak)*3*100-4; // scale it (sqrt to make low values more visible)
                if (y[x] > 100) y[x] = 100;

                if (y[x] < 60) y[x] = 0;// cap it
            }
            debugTextBox.Text = y[0].ToString()+'\n'+y[2].ToString()+'\n'+y[4].ToString()+'\n'+y[6].ToString();

            float hue;
            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                    hue = Color.Red.GetHue();
                else if (i == 1)
                    hue = Color.Yellow.GetHue();
                else if (i == 2)
                    hue = Color.Green.GetHue();
                else
                    hue = Color.Blue.GetHue();

                colors[i] = ColorHandler.FromAhsb(255, hue, 1f, (float)((y[2*i]/100)*0.7));
            }
            Elements.arduino.setColor(colors);
            for (int i = 0; i < 4; i++)
                colors[i] = Color.Black;

        }

        private int specIdx = 15;
        private int voicePrintIdx = 0;
        private void DrawSpectrum()
        {
            switch (specIdx)
            {
                // normal spectrum (width = resolution)
                case 0:
                    this.spectrumBox.Image = temp.CreateSpectrum(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Lime, Color.Red, Color.Black, false, false, false);
                    break;
                // normal spectrum (full resolution)
                case 1:
                    this.spectrumBox.Image = temp.CreateSpectrum(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.SteelBlue, Color.Pink, Color.Black, false, true, true);
                    break;
                // line spectrum (width = resolution)
                case 2:
                    this.spectrumBox.Image = temp.CreateSpectrumLine(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Lime, Color.Red, Color.Black, 2, 2, false, false, false);
                    break;
                // line spectrum (full resolution)
                case 3:
                    this.spectrumBox.Image = temp.CreateSpectrumLine(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.SteelBlue, Color.Pink, Color.Black, 16, 4, false, true, true);
                    break;
                // ellipse spectrum (width = resolution)
                case 4:
                    this.spectrumBox.Image = temp.CreateSpectrumEllipse(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Lime, Color.Red, Color.Black, 1, 2, false, false, false);
                    break;
                // ellipse spectrum (full resolution)
                case 5:
                    this.spectrumBox.Image = temp.CreateSpectrumEllipse(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.SteelBlue, Color.Pink, Color.Black, 2, 4, false, true, true);
                    break;
                // dot spectrum (width = resolution)
                case 6:
                    this.spectrumBox.Image = temp.CreateSpectrumDot(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Lime, Color.Red, Color.Black, 1, 0, false, false, false);
                    break;
                // dot spectrum (full resolution)
                case 7:
                    this.spectrumBox.Image = temp.CreateSpectrumDot(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.SteelBlue, Color.Pink, Color.Black, 2, 1, false, false, true);
                    break;
                // peak spectrum (width = resolution)
                case 8:
                    this.spectrumBox.Image = temp.CreateSpectrumLinePeak(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.SeaGreen, Color.LightGreen, Color.Orange, Color.Black, 2, 1, 2, 10, false, false, false);
                    break;
                // peak spectrum (full resolution)
                case 9:
                    this.spectrumBox.Image = temp.CreateSpectrumLinePeak(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.GreenYellow, Color.RoyalBlue, Color.DarkOrange, Color.Black, 23, 5, 3, 5, false, true, true);
                    break;
                // wave spectrum (width = resolution)
                case 10:
                    this.spectrumBox.Image = temp.CreateSpectrumWave(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Yellow, Color.Orange, Color.Black, 1, false, false, false);
                    break;
                // dancing beans spectrum (width = resolution)
                case 11:
                    this.spectrumBox.Image = temp.CreateSpectrumBean(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Chocolate, Color.DarkGoldenrod, Color.Black, 4, false, false, true);
                    break;
                // dancing text spectrum (width = resolution)
                case 12:
                    this.spectrumBox.Image = temp.CreateSpectrumText(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.White, Color.Tomato, Color.Black, "BASS .NET IS GREAT PIECE! UN4SEEN ROCKS...", false, false, true);
                    break;
                // frequency detection
                case 13:
                    float amp = temp.DetectFrequency(recChannel, 10, 500, true);
                    if (amp > 0.3)
                        this.spectrumBox.BackColor = Color.Red;
                    else
                        this.spectrumBox.BackColor = Color.Black;
                    break;
                // 3D voice print
                case 14:
                    // we need to draw directly directly on the picture box...
                    // normally you would encapsulate this in your own custom control
                    Graphics g = Graphics.FromHwnd(this.spectrumBox.Handle);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    temp.CreateSpectrum3DVoicePrint(recChannel, g, new Rectangle(0, 0, this.spectrumBox.Width, this.spectrumBox.Height), Color.Black, Color.White, voicePrintIdx, false, false);
                    g.Dispose();
                    // next call will be at the next pos
                    voicePrintIdx++;
                    if (voicePrintIdx > this.spectrumBox.Width - 1)
                        voicePrintIdx = 0;
                    break;
                // WaveForm
                case 15:
                    this.spectrumBox.Image = temp.CreateWaveForm(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Green, Color.Red, Color.Gray, Color.Black, 1, true, false, true);
                    break;
            }

        }

        private void spectrumBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                specIdx++;
            else
                specIdx--;

            if (specIdx > 15)
                specIdx = 0;
            if (specIdx < 0)
                specIdx = 15;
            this.spectrumBox.Image = null;
            //debugTextBox.Text += "\n" + specIdx;
            temp.ClearPeaks();
        }

       
      

    }
}
