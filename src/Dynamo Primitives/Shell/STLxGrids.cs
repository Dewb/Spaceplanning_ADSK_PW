using System;
using System.Collections.Generic;

namespace stuffer
{
  public class STLxGrids : STLx
  {
    List<double> m_heights;
    List<Grid2d> m_grids;

    internal STLxGrids(List<double> heights, List<Grid2d> grids)
    {
      m_heights = heights;
      m_grids = grids;
    }

    public static STLxGrids ByHeightAndGrid2d(List<double> heights, List<Grid2d> grids)
    {
      return new STLxGrids(heights, grids);
    }

    internal void OutputBasis(ref string text, GridBasis basis)
    {
      string tag = "basis";
      OutputHeader(ref text, tag);
      IndentIn();

      List<Vector2d> vectors = basis.Vectors;
      for (int index = 0; index < vectors.Count;++index)
        OutputTextLine(ref text, "ordinal " + EncodeDouble(basis.GridSize(index)) + " " + EncodePoint2d(vectors[index]));

      IndentOut();
      OutputFooter(ref text, tag);
    }

    internal void OutputAxis(ref string text, GridBasis basis, int ordinal, RangeAxis2d axis)
    {
      string tag = "axis";
      OutputHeader(ref text, tag);
      IndentIn();

      foreach (KeyValuePair<int, List<Range1d>> lineData in axis.Lines)
      {
        foreach (Range1d range in lineData.Value)
        {
          int v0 = basis.ToGrid(ordinal, range.Min);
          int v1 = basis.ToGrid(ordinal, range.Max);
          OutputTextLine(ref text, "seg " + EncodeInt(lineData.Key) + " " + EncodeInt(v0) + " " + EncodeInt(v1));
        }
      }

      IndentOut();
      OutputFooter(ref text, tag);
    }

    internal void OutputAxes(ref string text, GridBasis basis, List<RangeAxis2d> axes)
    {
      if (axes == null)
        return;

      string tag = "axes";
      OutputHeader(ref text, tag);
      IndentIn();

      for (int index = 0; index < axes.Count; ++index)
        OutputAxis(ref text, basis, index, axes[index]);

      IndentOut();
      OutputFooter(ref text, tag);
    }

    internal void OutputGrid(ref string text, double height, Grid2d grid)
    {
      string tag = "grid";
      OutputHeader(ref text, tag, "height " + EncodeDouble(height));
      IndentIn();

      OutputBasis(ref text, grid.Basis);
      OutputAxes(ref text, grid.Basis, grid.GetGridAxes());

      IndentOut();
      OutputFooter(ref text, tag);
    }

    public string ToASCII()
    {
      return ToString();
    }

    public override string ToString()
    {
      string text = "";
      if (m_heights.Count == m_grids.Count)
      {
        OutputHeader(ref text, "grids", m_name);
        IndentIn();

        int count = m_heights.Count;
        for (int index = 0; index < count; ++index)
          OutputGrid(ref text, m_heights[index], m_grids[index]);

        IndentOut();
        OutputFooter(ref text, "grids", m_name);
      }

      return text;
    }
  }
}
