#include <stdafx.h>
#include <ShellStrategy.h>
#include <Args.h>
#include <JobData.h>
#include <JobRequest.h>
#include <BuildData.h>
#include <Phenotype.h>
#include <STLShell.h>
#include <STLxShell.h>
#include <RectShell.h>

#include <STL/STL_R_Shell.h>
#include <STL/STL_L_Shell.h>
#include <STL/STL_U_Shell.h>
#include <STL/STL_O_Shell.h>

const string_t& ShellStrategy::name() const
{
  static string_t name(U("ShellStrategy"));
  return name;
}

void ShellStrategy::execute(const Args& args) const
{
  auto designs(args.getJobData().getRequest().requirements.designs);
  auto design(designs.size() > 0 ? &designs[0] : nullptr);
  auto mesh(design && design->meshes.size() > 0 ? &design->meshes[0] : nullptr);

  args.getBuildData().setXYShell(
    createShell(mesh, args),
    args.getPhenotype());
}

unique_ptr<const Shell> ShellStrategy::createShell(const Mesh* mesh, const Args& args) const
{
  if (mesh && mesh->inlineData != U(""))
  {
    unique_ptr<const Shell> shell(
      STLxShell::load(
        mesh->inlineData,
        args.getBuildData().getBasis()));
    if (shell)
    {
      args.getPhenotype().addMesh(*mesh);
      return move(shell);
    }
  }

  const JobRequest& request(args.getJobData().getRequest());

  static bool useShell(false);
  if (useShell)
  {
    auto uri(request.requirements.shell);
    string stlData;
    //if (uri.is_empty())
    //{
    //  // Set to use canned STL here...
    //  //stlData = STL_20x20x1;
    //  //stlData = STL_20x20x2;
    //  //stlData = STL_TwoTowers;
    //}
    //else
    {
      auto stlDataU = uri.to_string();

      // DELETE THIS!!! <- This is only here until the shell info is delivered via the requirements...
      stlDataU = U("L");

      if (stlDataU.length() == 1)
      {
        auto type(stlDataU[0]);
        switch (type)
        {
        case U('R'):
          stlData = STL_R_Shell;
          break;

        case U('L'):
          stlData = STL_L_Shell;
          break;

        case U('U'):
          stlData = STL_U_Shell;
          break;

        case U('O'):
          stlData = STL_O_Shell;
          break;
        }
      }
    }

    unique_ptr<const Shell> shell(
      new STLShell(
        stlData,
        args.getBuildData().getBasis()));
    if (shell)
      return move(shell);
  }

  Point2f site(
    request.requirements.site.width,
    request.requirements.site.height);

  // TODO: What is the right thing to do here?
  float defaultMin(50.0f);
  if (site.x() == 0.0f)
    site.x() = defaultMin;
  if (site.y() == 0.0f)
    site.y() = defaultMin;

  float siteSqm(static_cast<float>(site.x()*site.y()));
  float requiredSqm(0.0f);
  for (const auto& it : request.requirements.spaces)
    requiredSqm += it.minimumArea;
  int floors(static_cast<int>(ceil(requiredSqm / siteSqm)));
  if (floors < 1)
    floors = 1;

  return make_unique<RectShell>(site, floors, args.getBuildData().getBasis());

  // NOTE: Test code, delete (or move) later
  //return make_unique<RectShell>(Point2ui(3, 2), 1, args.getBuildData().getBasis());
  //return make_unique<RectShell>(Point2ui(76, 42), 1, args.getBuildData().getBasis());
}
