class GeneratorListener {
public:
    GeneratorListener() {}
    GeneratorListener(utility::string_t url);

    pplx::task<void> open() { return m_listener.open(); }
    pplx::task<void> close() { return m_listener.close(); }

    std::string testLSystem(int iterations);
    void setDefaultIterationCount(int iterations);

private:
    void handle_get(web::http::http_request message);
    void handle_put(web::http::http_request message);
    void handle_post(web::http::http_request message);
    void handle_delete(web::http::http_request message);
    void handle_options(web::http::http_request message);

    web::http::experimental::listener::http_listener m_listener;  

    int defaultIterationCount; 
};

