#include <stdafx.h>
#include <EdgeEffectTestVision.h>
#include <JobRequest.h>
#include <ListStrategy.h>
#include <ShellStrategy.h>
#include <OutdoorConnectionStrategy.h>
#include <RingHallwayStrategy.h>

namespace
{
  const JobRequest& createCustomRequest()
  {
    static JobRequest customRequest;
    customRequest.requirements.site.width = 6;
    customRequest.requirements.site.height = 3;
    customRequest.settings.grid = 1.0f;

    return customRequest;
  }
}

EdgeEffectTestVision::EdgeEffectTestVision(const JobRequest& /*jobRequest*/)
: XYVision(createCustomRequest())
{
  createPlan();
  createGenome();
}

void EdgeEffectTestVision::createPlan()
{
  unique_ptr<ListStrategy> list(new ListStrategy());
  list->addStrategy(make_unique<ShellStrategy>());

  unique_ptr<OutdoorConnectionStrategy> connect(new OutdoorConnectionStrategy(Rangei(1, 2)));
  connect->setSides({ 0 });
  list->addStrategy(Strategy::const_ref(connect));

  unique_ptr<EdgeEffectStrategy> edge(new RingHallwayStrategy(jobData->getHallwayWidth(basis)));
  edge->setSides({ 0 });
  list->addStrategy(Strategy::const_ref(edge));

  setRootStrategy(Strategy::const_ref(list));
}

void EdgeEffectTestVision::createGenome()
{
  static Genome::gene_structure genes {
    // Outdoor Connection
    Ribosome::fromBool(true),
    0.0f,
    Ribosome::fromAmount(Rangei(0, 4), 2),
    Ribosome::fromAmount(Rangei(0, 0), 0),

    // Edge Effect
    0.0f,
    Ribosome::fromAmount(Rangei(2, 2), 2)
  };

  setGenome(make_unique<Genome>(genes));
}
