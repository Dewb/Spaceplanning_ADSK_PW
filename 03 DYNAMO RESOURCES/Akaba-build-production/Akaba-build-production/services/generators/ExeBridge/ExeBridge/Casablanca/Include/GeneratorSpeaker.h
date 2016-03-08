#pragma once

class GeneratorSpeaker
{
public:
  static web::http::status_code postJob(const string_t& baseUri, const string_t& request, web::json::value& data);
};
