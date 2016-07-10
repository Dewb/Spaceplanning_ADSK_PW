using System.Collections.Generic;

namespace stuffer
{
  internal class Design
  {
    private List<Space> m_spaces;
    private List<Mesh> m_meshes;

    internal Design()
    {
      m_spaces = new List<Space>();
      m_meshes = new List<Mesh>();
    }

    public static Design Empty
    {
      get
      {
        return new Design();
      }
    }

    public void AddSpace(Space space)
    {
      m_spaces.Add(space);
    }

    public void AddMesh(Mesh mesh)
    {
      m_meshes.Add(mesh);
    }

    public List<Space> Spaces
    {
      get { return m_spaces; }
    }

    public List<Mesh> Meshes
    {
      get { return m_meshes; }
    }
  }
}
