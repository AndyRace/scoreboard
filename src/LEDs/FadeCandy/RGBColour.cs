using System;
using Windows.UI;

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

    public static RGBColour FromBytes(byte r, byte g, byte b)
    {
      return new RGBColour(r / 255.0, g / 255.0, b / 255.0);
    }

    public static RGBColour FromColor(Color onColour)
    {
      return FromBytes(onColour.R, onColour.G, onColour.B);
    }

    public void Clear()
    {
      R = G = B = 0;
    }

    //private static RGBColour FromHSVOld(double h, double s, double v)
    //{
    //  double r, g, b;
    //  r = g = b = 0;
    //  if (h < 0.5) r = 1 - h * 3; else r = h * 3 - 2;
    //  g = 1 - Math.Abs(h * 3 - 1);
    //  b = 1 - Math.Abs(h * 3 - 2);
    //  if (r > 1) r = 1;
    //  if (r < 0) r = 0;
    //  if (g > 1) g = 1;
    //  if (g < 0) g = 0;
    //  if (b > 1) b = 1;
    //  if (b < 0) b = 0;

    //  r = (r * s + (1 - s)) * v;
    //  g = (g * s + (1 - s)) * v;
    //  b = (b * s + (1 - s)) * v;

    //  return new RGBColour(r, g, b);
    //}

    public static RGBColour FromHSV(double h, double s, double v)
    {
      var calc = new Func<double, double>((rgb) =>
        {
          if (rgb > 1) return v;

          if (rgb < 0) rgb = 0;
          return (rgb * s + (1 - s)) * v;
        });

      var result = new RGBColour(
        calc((h < 0.5) ? 1 - h * 3 : h * 3 - 2),
        calc(1 - Math.Abs(h * 3 - 1)),
        calc(1 - Math.Abs(h * 3 - 2)));

      // var oldResult = FromHSVOld(h, s, v);
      // Debug.Assert(result.Equals(oldResult), "Unexpected RGB from HSV");
      // return oldResult;

      return result;
    }

    public override bool Equals(object other)
    {
      if (other == null) return false;
      if (!(other is RGBColour)) return false;
      if (GetHashCode() != other.GetHashCode()) return false;
      return true;
    }

    public override int GetHashCode()
    {
      return (RByte << 16) + (GByte << 8) + BByte;
    }
  }
}
