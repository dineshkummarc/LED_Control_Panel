using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace LED_Control_Panel
{
    partial class ledControlPanel
    {

        public int recChannel;
        private RECORDPROC myRecProc;
        private Un4seen.Bass.BASSTimer updateTimer;
        private bool inicialized=false;
        private Un4seen.Bass.Misc.Visuals bassVisuals = new Un4seen.Bass.Misc.Visuals();
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
                MessageBox.Show("Bass Error Code: "+error, "Music Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            Bass.BASS_RecordSetDevice(Device);
            if ((error = Bass.BASS_ErrorGetCode()) != 0)
                MessageBox.Show("Bass Error Code: " + error, "Music Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            

            myRecProc = new RECORDPROC(MyRecording);
            recChannel = Bass.BASS_RecordStart(22050, 2, BASSFlag.BASS_RECORD_PAUSE, myRecProc, IntPtr.Zero);
            Bass.BASS_ChannelPlay(recChannel, false);
            updateTimer = new Un4seen.Bass.BASSTimer(50);
            updateTimer.Tick += new EventHandler(timerUpdate_Tick);
            mySpectrumInit();
            inicialized = true;
        }

        public float[] fft = new float[1024];
        private void timerUpdate_Tick(object sender, System.EventArgs e)
        {
            Bass.BASS_ChannelGetData(recChannel, fft, (int)BASSData.BASS_DATA_FFT2048);
            //colorMusic2(fft);
            drawMySpectrum(fft);
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

       

      
        private void colorMusic2(float[] fft)
        {
            int x;
            int Bands=8;
            int b0 = (int)Math.Pow(2, 3 * 10.0 / (Bands - 1));
            double[] y=new double[Bands];
            for (x = 0; x < Bands; x++)
            {
                float peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (Bands - 1));//хз откуда они взяли такую формулу, но математикам-музыкантам виднее
                
                if (b1 > fft.Length-1) b1 = fft.Length-1;

                if (b1 <= b0) b1 = b0 + 1; // make sure it uses at least 1 FFT bin


                for (; b0 < b1; b0++)
                    if (peak < fft[1 + b0]) peak = fft[1 + b0];


                y[x] = Math.Sqrt(peak)*3*100-4; // scale it (sqrt to make low values more visible)

                if (y[x] > 100) y[x] = 100;

                if (y[x] < sensTR.Value) y[x] = 0;// cap it
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

                colors[i]= ColorHandler.HSVtoColor((int)hue,255,(int)y[i*2]*255/100);
                 
            }
            Elements.arduino.setColor(colors);
            
        }

//---------------------------------------------My Spectrum------------------------------------------------------------------------
        
        
        private Graphics graphics;
        private BufferedGraphics graphBuff;
        private BufferedGraphicsContext graphContext;
        private Bitmap mySpectrumBuffer;
        private int mySpectrumHieght;
        private int mySpectrumWidth;
        private LinearGradientBrush brush;

        private int[] barriers={60,60,60,50};//пороговые значения по-умолчанию 
        private int currentBarrier;

        private void mySpectrumInit()
        {
           

            mySpectrumHieght = mySpectrumPanel.Height;
            mySpectrumWidth = mySpectrumPanel.Width;

            graphContext = BufferedGraphicsManager.Current;
            graphContext.MaximumBuffer = new Size(mySpectrumWidth + 1, mySpectrumHieght + 1);
            graphBuff = graphContext.Allocate(mySpectrumPanel.CreateGraphics(), new Rectangle(0,0,mySpectrumWidth,mySpectrumHieght));

            mySpectrumBuffer = new Bitmap(mySpectrumWidth, mySpectrumHieght);
            graphics = mySpectrumPanel.CreateGraphics();
            brush = new LinearGradientBrush(new Point(0,0),new Point (0,mySpectrumHieght),Color.Red,Color.Lime);
            graphBuff.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed; //пофиг качество главное скорость

            barrierTb.Visible = true;
            
        }

        private void drawMySpectrum(float[] fft)
        {
            
            int Bands = 16;
            float scaleFactor = 2;
            int fadeSpeed = 5;
            
            int b0 = (int)Math.Pow(2, 3 * 10.0 / (Bands - 1));
            float[] peaks=new float[4];
            int[] y = new int[Bands];
            mySpectrumPanel.BackgroundImage = mySpectrumBuffer;
            graphBuff.Graphics.Clear(Color.Black);
            for (int x = 0; x < Bands; x++)
            {
                float peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (Bands - 1));//хз откуда они взяли такую формулу, но математикам-музыкантам виднее

                if (b1 > fft.Length - 1) b1 = fft.Length - 1;

                //if (b0 > fft.Length - 2) b0 = fft.Length - 2;

                if (b1 <= b0) b1 = b0 + 1; // make sure it uses at least 1 FFT bin


                for (; b0 < b1; b0++)
                {
                    if (peak < fft[1 + b0]) peak = fft[1 + b0];
                    if (peaks[x / (Bands / 4)] < fft[1 + b0]) peaks[x / (Bands / 4)] = fft[1 + b0];
                }


                y[x] = (int)(Math.Sqrt(peak) * scaleFactor * mySpectrumHieght - 4); // scale it (sqrt to make low values more visible)

                if (y[x] > mySpectrumHieght) y[x] = mySpectrumHieght;// cap it

                graphBuff.Graphics.FillRectangle(brush, 
                    new Rectangle(x * (mySpectrumWidth / Bands), mySpectrumHieght - y[x], mySpectrumWidth / Bands - 2, y[x]));

            }

            float hue;
            int debugHeight=0;
            for (int i = 0; i < 4; i++)
            {
                int height = (int)(Math.Sqrt(peaks[i]) * scaleFactor * mySpectrumHieght - 4);
                if (height > mySpectrumHieght) height = mySpectrumHieght;

                if (i == 0) debugHeight = height;

                //height = mySpectrumHieght - height;//инвертируем для нормального восприятия т.к. у визуализатора 0 сверху
                
                graphBuff.Graphics.DrawLine(new Pen(Color.Blue, 3), new Point(i * mySpectrumWidth / 4, mySpectrumHieght - height),
                    new Point((i + 1) * mySpectrumWidth / 4 - 2, mySpectrumHieght - height));//общие линии
             
                
                graphBuff.Graphics.DrawLine(new Pen(Color.Violet,3f),new Point(i * mySpectrumWidth/4,mySpectrumHieght-barriers[i]),
                    new Point((i + 1) * mySpectrumWidth / 4 - 2, mySpectrumHieght - barriers[i])); //барьеры


                 if (i == 0)
                    hue = Color.Red.GetHue();
                else if (i == 1)
                    hue = Color.Yellow.GetHue();
                else if (i == 2)
                    hue = Color.Green.GetHue();
                else
                    hue = Color.Blue.GetHue();

                 if (height > barriers[i])
                     colors[i] = ColorHandler.HSVtoColor((int)hue, 255, (int)(((double)height / (double)mySpectrumHieght) * 0.7 * 255));
                 else
                 {
                     ColorHandler.HSV nColor=ColorHandler.ColorToHSV(colors[i]);

                     nColor.value -= fadeSpeed;

                     if (nColor.value < 0) nColor.value = 0;

                     colors[i] = ColorHandler.HSVtoColor(nColor);
                 }
            }
            debugTextBox.Text = barriers[0] + "   " + debugHeight+"   "+((double)debugHeight/(double)mySpectrumHieght);
            graphBuff.Render(mySpectrumPanel.CreateGraphics());
            Elements.arduino.setColor(colors);
         }



//---------------------------------------------Default Spectrum-------------------------------------------------------------------
        private int specIdx = 15;
        private int voicePrintIdx = 0;
        private void DrawSpectrum()
        {
            switch (specIdx)
            {
                // normal spectrum (width = resolution)
                case 0:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrum(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Lime, Color.Red, Color.Black, false, false, false);
                    break;
                // normal spectrum (full resolution)
                case 1:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrum(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.SteelBlue, Color.Pink, Color.Black, false, true, true);
                    break;
                // line spectrum (width = resolution)
                case 2:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrumLine(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Lime, Color.Red, Color.Black, 2, 2, false, false, false);
                    break;
                // line spectrum (full resolution)
                case 3:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrumLine(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.SteelBlue, Color.Pink, Color.Black, 16, 4, false, true, true);
                    break;
                // ellipse spectrum (width = resolution)
                case 4:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrumEllipse(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Lime, Color.Red, Color.Black, 1, 2, false, false, false);
                    break;
                // ellipse spectrum (full resolution)
                case 5:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrumEllipse(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.SteelBlue, Color.Pink, Color.Black, 2, 4, false, true, true);
                    break;
                // dot spectrum (width = resolution)
                case 6:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrumDot(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Lime, Color.Red, Color.Black, 1, 0, false, false, false);
                    break;
                // dot spectrum (full resolution)
                case 7:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrumDot(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.SteelBlue, Color.Pink, Color.Black, 2, 1, false, false, true);
                    break;
                // peak spectrum (width = resolution)
                case 8:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrumLinePeak(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.SeaGreen, Color.LightGreen, Color.Orange, Color.Black, 2, 1, 2, 10, false, false, false);
                    break;
                // peak spectrum (full resolution)
                case 9:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrumLinePeak(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.GreenYellow, Color.RoyalBlue, Color.DarkOrange, Color.Black, 23, 5, 3, 5, false, true, true);
                    break;
                // wave spectrum (width = resolution)
                case 10:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrumWave(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Yellow, Color.Orange, Color.Black, 1, false, false, false);
                    break;
                // dancing beans spectrum (width = resolution)
                case 11:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrumBean(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Chocolate, Color.DarkGoldenrod, Color.Black, 4, false, false, true);
                    break;
                // dancing text spectrum (width = resolution)
                case 12:
                    this.spectrumBox.Image = bassVisuals.CreateSpectrumText(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.White, Color.Tomato, Color.Black, "BASS .NET IS GREAT PIECE! UN4SEEN ROCKS...", false, false, true);
                    break;
                // frequency detection
                case 13:
                    float amp = bassVisuals.DetectFrequency(recChannel, 10, 500, true);
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
                    bassVisuals.CreateSpectrum3DVoicePrint(recChannel, g, new Rectangle(0, 0, this.spectrumBox.Width, this.spectrumBox.Height), Color.Black, Color.White, voicePrintIdx, false, false);
                    g.Dispose();
                    // next call will be at the next pos
                    voicePrintIdx++;
                    if (voicePrintIdx > this.spectrumBox.Width - 1)
                        voicePrintIdx = 0;
                    break;
                // WaveForm
                case 15:
                    this.spectrumBox.Image = bassVisuals.CreateWaveForm(recChannel, this.spectrumBox.Width, this.spectrumBox.Height, Color.Green, Color.Red, Color.Gray, Color.Black, 1, true, false, true);
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
            bassVisuals.ClearPeaks();
        }

       
      

    }
}
