using System;

namespace stuffer
{
    public class STLx
  {
    protected string m_name;
    protected int m_precision;
    private int m_indent;

    protected STLx()
    {
      m_name = "Home";
      m_precision = 2;
      m_indent = 0;
    }

    protected void IndentIn()
    {
      ++m_indent;
    }

    protected void IndentOut()
    {
      --m_indent;
    }

    protected string Indent
    {
      get
      {
        if (m_indent <= 0)
          return "";

        return '\t'.Repeat(m_indent);
      }
    }

    protected string EncodeDouble(double value)
    {
      string specifier = "{0:F" + m_precision + "}";
      return String.Format(specifier, value);
    }

    protected string EncodeInt(int value)
    {
      return String.Format("{0}", value);
    }

    protected string EncodePoint2d(Point2d point)
    {
      return EncodeDouble(point.X) + " " + EncodeDouble(point.Y);
    }

    protected void OutputPoint2d(ref string text, Point2d point)
    {
      text += Indent + "vertex " + EncodePoint2d(point) + Environment.NewLine;
    }

    protected string EncodePoint3d(Point3d point)
    {
      return EncodeDouble(point.X) + " " + EncodeDouble(point.Y) + " " + EncodeDouble(point.Z);
    }

    protected void OutputPoint3d(ref string text, Point3d point)
    {
      text += Indent + "vertex " + EncodePoint3d(point) + Environment.NewLine;
    }

    protected void OutputTextLine(ref string text, string line)
    {
      text += Indent + line + Environment.NewLine;
    }

    internal void OutputHeader(ref string text, string tag, string extra = "")
    {
      if (extra != "")
        OutputTextLine(ref text, tag + " " + extra);
      else
        OutputTextLine(ref text, tag);
    }

    internal void OutputFooter(ref string text, string tag, string extra = "")
    {
      OutputHeader(ref text, "end" + tag, extra);
    }
  }
}
