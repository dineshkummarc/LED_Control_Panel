using System;
using System.Drawing;

public class ColorHandler 
{
	// Handle conversions between RGB and HSV    
	// (and Color types, as well).

	public struct RGB
	{
		// All values are between 0 and 255.
		public int Red;
		public int Green;
		public int Blue;

		public RGB(int R, int G, int B) 
		{
			Red = R;
			Green = G;
			Blue = B;
		}

	public override string  ToString() 
	{
		return String.Format("({0}, {1}, {2})", Red, Green, Blue);
	}
} 

	public struct HSV
	{
		// All values are between 0 and 255.
		public int Hue;
		public int Saturation;
		public int value;

		public HSV(int H, int S, int V) 
		{
			Hue = H;
			Saturation = S;
			value = V;
		}

		public override string  ToString() 
		{
			return String.Format("({0}, {1}, {2})", Hue, Saturation, value);
		}
	}

			public static RGB HSVtoRGB( int H, int S, int V) 
			{
				// H, S, and V must all be between 0 and 255.
				return HSVtoRGB(new HSV(H, S, V));
			}

			public static Color HSVtoColor(HSV hsv) 
			{
				RGB RGB = HSVtoRGB(hsv);
				return Color.FromArgb(RGB.Red, RGB.Green, RGB.Blue);
			}

			public static Color HSVtoColor( int H,  int S,  int V) 
			{
				return HSVtoColor(new HSV(H, S, V));
			}

			public static RGB HSVtoRGB(HSV HSV) 
			{
				// HSV contains values scaled as in the color wheel:
				// that is, all from 0 to 255. 

				// for ( this code to work, HSV.Hue needs
				// to be scaled from 0 to 360 (it//s the angle of the selected
				// point within the circle). HSV.Saturation and HSV.value must be 
				// scaled to be between 0 and 1.

				double h;
				double s;
				double v;

				double r = 0;
				double g = 0;
				double b = 0;

				// Scale Hue to be between 0 and 360. Saturation
				// and value scale to be between 0 and 1.
				h = ((double) HSV.Hue / 255 * 360) % 360;
				s = (double) HSV.Saturation / 255;
				v = (double) HSV.value / 255;

				if ( s == 0 ) 
				{
					// If s is 0, all colors are the same.
					// This is some flavor of gray.
					r = v;
					g = v;
					b = v;
				} 
				else 
				{
					double p;
					double q;
					double t;

					double fractionalSector;
					int sectorNumber;
					double sectorPos;

					// The color wheel consists of 6 sectors.
					// Figure out which sector you//re in.
					sectorPos = h / 60;
					sectorNumber = (int)(Math.Floor(sectorPos));

					// get the fractional part of the sector.
					// That is, how many degrees into the sector
					// are you?
					fractionalSector = sectorPos - sectorNumber;

					// Calculate values for the three axes
					// of the color. 
					p = v * (1 - s);
					q = v * (1 - (s * fractionalSector));
					t = v * (1 - (s * (1 - fractionalSector)));

					// Assign the fractional colors to r, g, and b
					// based on the sector the angle is in.
					switch (sectorNumber) 
					{
						case 0:
							r = v;
							g = t;
							b = p;
							break;

						case 1:
							r = q;
							g = v;
							b = p;
							break;

						case 2:
							r = p;
							g = v;
							b = t;
							break;

						case 3:
							r = p;
							g = q;
							b = v;
							break;

						case 4:
							r = t;
							g = p;
							b = v;
							break;

						case 5:
							r = v;
							g = p;
							b = q;
							break;
					}
				}
				// return an RGB structure, with values scaled
				// to be between 0 and 255.
				return new RGB((int)(r * 255), (int)(g * 255), (int)(b * 255));
			}

			public static HSV RGBtoHSV( RGB RGB) 
			{
				// In this function, R, G, and B values must be scaled 
				// to be between 0 and 1.
				// HSV.Hue will be a value between 0 and 360, and 
				// HSV.Saturation and value are between 0 and 1.
				// The code must scale these to be between 0 and 255 for
				// the purposes of this application.

				double min;
				double max;
				double delta;

				double r = (double) RGB.Red / 255;
				double g = (double) RGB.Green / 255;
				double b = (double) RGB.Blue / 255;

				double h;
				double s;
				double v;

				min = Math.Min(Math.Min(r, g), b);
				max = Math.Max(Math.Max(r, g), b);
				v = max;
				delta = max - min;
				if ( max == 0 || delta == 0 ) 
				{
					// R, G, and B must be 0, or all the same.
					// In this case, S is 0, and H is undefined.
					// Using H = 0 is as good as any...
					s = 0;
					h = 0;
				} 
				else 
				{
					s = delta / max;
					if ( r == max ) 
					{
						// Between Yellow and Magenta
						h = (g - b) / delta;
					} 
					else if ( g == max ) 
					{
						// Between Cyan and Yellow
						h = 2 + (b - r) / delta;
					} 
					else 
					{
						// Between Magenta and Cyan
						h = 4 + (r - g) / delta;
					}

				}
				// Scale h to be between 0 and 360. 
				// This may require adding 360, if the value
				// is negative.
				h *= 60;
				if ( h < 0 ) 
				{
					h += 360;
				}

				// Scale to the requirements of this 
				// application. All values are between 0 and 255.
				return new HSV((int)(h / 360 * 255), (int)(s * 255), (int)(v * 255));
			}


            /// <summary>
            /// Creates a Color from alpha, hue, saturation and brightness.
            /// </summary>
            /// <param name="alpha">The alpha channel value.</param>
            /// <param name="hue">The hue value.</param>
            /// <param name="saturation">The saturation value.</param>
            /// <param name="brightness">The brightness value.</param>
            /// <returns>A Color with the given values.</returns>
            public static Color FromAhsb(int alpha, float hue, float saturation, float brightness)
            {
                if (0 > alpha
                    || 255 < alpha)
                {
                    throw new ArgumentOutOfRangeException(
                        "alpha",
                        alpha,
                        "Value must be within a range of 0 - 255.");
                }

                if (0f > hue
                    || 360f < hue)
                {
                    throw new ArgumentOutOfRangeException(
                        "hue",
                        hue,
                        "Value must be within a range of 0 - 360.");
                }

                if (0f > saturation
                    || 1f < saturation)
                {
                    throw new ArgumentOutOfRangeException(
                        "saturation",
                        saturation,
                        "Value must be within a range of 0 - 1.");
                }

                if (0f > brightness
                    || 1f < brightness)
                {
                    throw new ArgumentOutOfRangeException(
                        "brightness",
                        brightness,
                        "Value must be within a range of 0 - 1.");
                }

                if (0 == saturation)
                {
                    return Color.FromArgb(
                                        alpha,
                                        Convert.ToInt32(brightness * 255),
                                        Convert.ToInt32(brightness * 255),
                                        Convert.ToInt32(brightness * 255));
                }

                float fMax, fMid, fMin;
                int iSextant, iMax, iMid, iMin;

                if (0.5 < brightness)
                {
                    fMax = brightness - (brightness * saturation) + saturation;
                    fMin = brightness + (brightness * saturation) - saturation;
                }
                else
                {
                    fMax = brightness + (brightness * saturation);
                    fMin = brightness - (brightness * saturation);
                }

                iSextant = (int)Math.Floor(hue / 60f);
                if (300f <= hue)
                {
                    hue -= 360f;
                }

                hue /= 60f;
                hue -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
                if (0 == iSextant % 2)
                {
                    fMid = (hue * (fMax - fMin)) + fMin;
                }
                else
                {
                    fMid = fMin - (hue * (fMax - fMin));
                }

                iMax = Convert.ToInt32(fMax * 255);
                iMid = Convert.ToInt32(fMid * 255);
                iMin = Convert.ToInt32(fMin * 255);

                switch (iSextant)
                {
                    case 1:
                        return Color.FromArgb(alpha, iMid, iMax, iMin);
                    case 2:
                        return Color.FromArgb(alpha, iMin, iMax, iMid);
                    case 3:
                        return Color.FromArgb(alpha, iMin, iMid, iMax);
                    case 4:
                        return Color.FromArgb(alpha, iMid, iMin, iMax);
                    case 5:
                        return Color.FromArgb(alpha, iMax, iMin, iMid);
                    default:
                        return Color.FromArgb(alpha, iMax, iMid, iMin);
                }
            }
		}
