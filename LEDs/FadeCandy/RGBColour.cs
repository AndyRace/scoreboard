using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FadeCandy
{
  public struct RGBColour
  {
    public double R, G, B;

    byte ConvertValue(double source)
    {
      if (source < 0) source = 0;
      if (source > 1) source = 1;
      return (byte)(source * 255);
    }
    public byte RByte { get { return ConvertValue(R); } }
    public byte GByte { get { return ConvertValue(G); } }
    public byte BByte { get { return ConvertValue(B); } }

    public RGBColour(double r, double g, double b)
    {
      R = r; G = g; B = b;
    }

    public static RGBColour FromHSV(double h, double s, double v)
    {
      double r, g, b;
      r = g = b = 0;
      if (h < 0.5) r = 1 - h * 3; else r = h * 3 - 2;
      g = 1 - Math.Abs(h * 3 - 1);
      b = 1 - Math.Abs(h * 3 - 2);
      if (r > 1) r = 1;
      if (r < 0) r = 0;
      if (g > 1) g = 1;
      if (g < 0) g = 0;
      if (b > 1) b = 1;
      if (b < 0) b = 0;

      r = (r * s + (1 - s)) * v;
      g = (g * s + (1 - s)) * v;
      b = (b * s + (1 - s)) * v;

      return new RGBColour(r, g, b);
    }
  }
}
