#pragma once

#include <GeneratorListener.h>

class GeneratorGraphicsAPI;

class StufferListener : public GeneratorListener
{
public:
  static void start(web::http::uri url, GeneratorGraphicsAPI* pGraphics);

  StufferListener(web::http::uri url, GeneratorGraphicsAPI* pGraphics);

protected:
  GeneratorGraphicsAPI* m_pGraphics;

  unique_ptr<LayoutJob> createJob(const web::json::object& jobData) const;
};
