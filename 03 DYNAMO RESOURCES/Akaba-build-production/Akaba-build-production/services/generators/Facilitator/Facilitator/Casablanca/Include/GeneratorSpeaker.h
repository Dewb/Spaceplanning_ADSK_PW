#pragma once

class GeneratorSpeaker
{
public:
  static web::http::status_code postJob(const string_t& baseUri, web::json::value& data);
};
