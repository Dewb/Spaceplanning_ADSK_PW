#include <stdafx.h>
#include <BuildingCoreVision.h>
#include <JobRequest.h>

#include <locale>

BuildingCoreVision::BuildingCoreVision(const JobRequest& jobRequest)
: Vision(jobRequest)
{
}

namespace
{
  Point3f parsePoint(ifstream& file)
  {
    string strX;
    getline(file, strX, ',');

    string strY;
    getline(file, strY, ',');

    string strZ;
    getline(file, strZ, '\n');

    char* end;
    float x = static_cast<float>(std::strtod(strX.c_str(), &end));
    float y = static_cast<float>(std::strtod(strY.c_str(), &end));
    float z = static_cast<float>(std::strtod(strZ.c_str(), &end));

    return Point3f(x, y, z);
  }

  void extractSpaces(string fileLoc, const string_t& usage, Design& design)
  {
    ifstream file(fileLoc);
    while (file.good())
    {
      Point3f p0(parsePoint(file));
      Point3f p1(parsePoint(file));

      Rangef xr(p0.x(), p1.x());
      Rangef yr(p0.y(), p1.y());
      Rangef zr(p0.z(), p1.z());

      float px(xr.l() + (xr.h() - xr.l()) / 2.0f);
      float py(yr.l() + (yr.h() - yr.l()) / 2.0f);
      Point3f origin(px, py, 0.0f);

      Point3f dim(
        xr.h() - xr.l(),
        yr.h() - yr.l(),
        zr.h() - zr.l());

      Space space;
      space.usage = usage;
      space.origin = origin;
      space.dimensions = dim;
      space.isCirculation = false;
      design.spaces.push_back(space);
    }
  }

  void extractShell(string shellFileLoc, string outlinesFileLoc, string gridsFileLoc, Design& design)
  {
    ifstream shellFile(shellFileLoc);
    stringstream shellStream;
    shellStream << shellFile.rdbuf();
    string shellText(shellStream.str());

    ifstream outlinesFile(outlinesFileLoc);
    stringstream outlinesStream;
    outlinesStream << outlinesFile.rdbuf();
    string outlinesText(outlinesStream.str());

    ifstream gridsFile(gridsFileLoc);
    stringstream gridsStream;
    gridsStream << gridsFile.rdbuf();
    string gridsText(gridsStream.str());

    Mesh mesh;
    locale loc;
    for (auto it = shellText.begin(); it != shellText.end(); ++it)
      mesh.inlineData.push_back(use_facet<ctype<wchar_t>>(loc).widen(*it));
    for (auto it = outlinesText.begin(); it != outlinesText.end(); ++it)
      mesh.inlineData.push_back(use_facet<ctype<wchar_t>>(loc).widen(*it));
    for (auto it = gridsText.begin(); it != gridsText.end(); ++it)
      mesh.inlineData.push_back(use_facet<ctype<wchar_t>>(loc).widen(*it));

    design.meshes.push_back(mesh);
  }
}

unique_ptr<Design> BuildingCoreVision::execute(std::function<void(unique_ptr<Design>)> callback)
{
  string exeLoc("\"C:\\Program Files\\Autodesk\\Dynamo Studio 2016\\DynamoStudio.exe\"");
  string dataLoc("C:\\Users\\Colin\\Documents\\GitHub\\Akaba\\services\\generators\\ExeBridge\\ExeBridge\\Generator\\Vision\\Dynamo\\");
  string dynFile("CORE_DAP.dyn");
  string elevatorsFile("elevators.csv");
  string serviceModulesFile("serviceModules.csv");
  string shellFile("shell.stl");
  string outlinesFile("outlines.stlx");
  string gridsFile("grids.stlx");

  std::stringstream stream;
  stream << exeLoc << " " << dataLoc << dynFile;
  int ret = system(stream.str().c_str());
  // TODO: Stop system call after files have been written...

  unique_ptr<Design> design(new Design());
  extractSpaces(dataLoc + elevatorsFile, U("Core"), *design);
  //extractSpaces(dataLoc + serviceModulesFile, U("ServiceModule"), *design);
  extractShell(
    dataLoc + shellFile,
    dataLoc + outlinesFile,
    dataLoc + gridsFile,
    *design);

  callback(move(design));

  return nullptr;
}
