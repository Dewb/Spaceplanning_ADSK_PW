#include <stdafx.h>
#include <StufferListener.h>
#include <StufferJob.h>

using namespace web;
using namespace http;

void StufferListener::start(uri url, GeneratorGraphicsAPI* pGraphics)
{
  GeneratorListener::start(make_unique<StufferListener>(url, pGraphics));
}

StufferListener::StufferListener(uri url, GeneratorGraphicsAPI* pGraphics)
: GeneratorListener(url),
  m_pGraphics(pGraphics)
{
}

unique_ptr<LayoutJob> StufferListener::createJob(const json::object& jobData) const
{
  return make_unique<StufferJob>(m_pGraphics, jobData);
}
