#include <stdafx.h>
#include <STLxShell.h>

namespace
{
  void split(vector<string_t> &tokens, const string_t &text, char_t sep)
  {
    size_t start(0);
    size_t end(0);
    while ((end = text.find(sep, start)) != string_t::npos)
    {
      if (start != end)
        tokens.push_back(text.substr(start, end - start));
  
      start = end + 1;
    }
    tokens.push_back(text.substr(start));
  }

#ifdef _WIN32
  float strtof_t(string_t& str) { return static_cast<float>(wcstod(str.c_str(), 0)); }
  int strtoi_t(string_t& str) { return wcstol(str.c_str(), nullptr, 10); }
#else
  float strtof_t(string_t& str) { return static_cast<float>(strtod(str.c_str(), 0)); }
  int strtoi_t(string_t& str) { return strtol(str.c_str(), nullptr, 10); }
#endif
}

unique_ptr<STLxShell> STLxShell::load(const string_t& data, const GridBasis& basis)
{
  smatch_t match;
  static regex_t regexGrids(U("(grids\\s[a-z\\-0-9.\\s]*endgrids)"), regex_constants::icase);
  if (!regex_search(data, match, regexGrids))
    return nullptr;

  return unique_ptr<STLxShell>(new STLxShell(match[0], basis));
}

STLxShell::STLxShell(const string_t& data, const GridBasis& basis)
: Shell(basis)
{
  loadSTLxFromASCII(data);
}

void STLxShell::loadSTLxFromASCII(const string_t& data)
{
  vector<string_t> lines;
  split(lines, data, U('\n'));
  for (auto& line : lines)
  {
    auto pos(line.find_first_not_of(U(" \n\r\t")));
    if (pos == string_t::npos)
      line = U("");
    else
      line = line.substr(pos);
  }

  enum State
  {
    Begin,
    StartGrids,
    StartGrid,
    StartBasis,
    Ordinal,
    EndBasis,
    StartAxes,
    StartAxis,
    Segment,
    EndAxis,
    EndAxes,
    EndGrid
  } last = Begin;

  float height(0.00f);
  Grid* grid(nullptr);
  int ordinal(0);
  for (const auto& line : lines)
  {
    // NOTE: Making the canned STL can introduce an empty line at the beginning
    if (line == U(""))
      continue;

    vector<string_t> parts;
    split(parts, line, U(' '));

    switch (last)
    {
    case Begin:
      last = StartGrids;
      break;

    case EndGrid:
      if (parts[0].compare(U("endgrids")) == 0)
        return;
    case StartGrids:
      height = strtof_t(parts[2]);
      grid = &createLevel(basis.levelNum(height)).getGrid();
      last = StartGrid;
      break;

    case StartGrid:
      last = StartBasis;
      break;

    case StartBasis:
    case Ordinal:
      if (parts[0].compare(U("ordinal")) == 0)
      {
        last = Ordinal;
        break;
      }

      last = EndBasis;
      break;

    case EndBasis:
    case EndAxes:
      if (parts[0].compare(U("endgrid")) == 0)
      {
        last = EndGrid;
        break;
      }

      last = StartAxes;
      break;

    case StartAxes:
      ordinal = 0;
      last = StartAxis;
      break;

    case StartAxis:
    case Segment:
      if (parts[0].compare(U("seg")) == 0)
      {
        string_t tag(basis.axis()[ordinal]);
        int index(strtoi_t(parts[1]));
        Rangei range(strtoi_t(parts[2]), strtoi_t(parts[3]));
        grid->addLine(tag, index, range);
        last = Segment;
        break;
      }

      last = EndAxis;
      break;

    case EndAxis:
      if (parts[0].compare(U("endaxes")) == 0)
      {
        last = EndAxes;
        break;
      }

      ++ordinal;
      last = StartAxis;
      break;
    }
  }
}
