#include "stdafx.h"
#include "GeneratorListener.h"

#include "boost/program_options.hpp" 

using namespace web;
using namespace http;
using namespace utility;
using namespace http::experimental::listener;

std::unique_ptr<GeneratorListener> g_generatorListener;

void on_initialize(const string_t& address) {
    uri_builder uri(address);
    uri.append_path(U("generator"));

    auto addr = uri.to_uri().to_string();
    g_generatorListener = std::unique_ptr<GeneratorListener>(new GeneratorListener(addr));
    g_generatorListener->open().wait();
    
    ucout << utility::string_t(U("Listening for requests at: ")) << addr << std::endl;
    return;
}

void on_shutdown() {
    g_generatorListener->close().wait();
    return;
}

#ifdef _WIN32
int wmain(int argc, wchar_t *argv[]) {
#else
int main(int argc, char *argv[]) {
#endif

    //utility::string_t address = U("http://localhost:");
    utility::string_t address = U("localhost");
    utility::string_t port = U("34568");
    int num_iterations = 12;

    namespace po = boost::program_options; 
    po::options_description desc("Options"); 
    desc.add_options() 
      ("port,p", po::value< std::vector<std::string> >(), "Port to listen on (default 34568)") 
      ("address,a", po::value< std::vector<std::string> >(), "Interface address to listen on (default localhost)")
      ("iterations,i", po::value<int>(), "Number of L-systems iterations per design")
      ("one", "Generate one topology string then quit")
      ("help,h", "Print this message");
 
    po::variables_map vm; 
    try { 
        po::store(po::parse_command_line(argc, argv, desc), vm); // can throw 
 
        if (vm.count("port")) {
            auto ports = vm["port"].as< std::vector<std::string> >();
            port = ports[0];
        }

        if (vm.count("address")) {
            auto addresses = vm["address"].as< std::vector<std::string> >();
            address = addresses[0];
        }

        if (vm.count("iterations")) {
            num_iterations = vm["iterations"].as<int>();
        }

        if (vm.count("one")) { 
            address.append(port);
            g_generatorListener = std::unique_ptr<GeneratorListener>(new GeneratorListener(address));
            std::cout << g_generatorListener->testLSystem(num_iterations) << std::endl;
            return 0; 
        } 

        if (vm.count("help")) { 
            std::cout << "AkabaGeneratorService: REST listener for L-systems generation" << std::endl 
                  << desc << std::endl; 
            return 0; 
        } 

        po::notify(vm); 
    } catch(po::error& e) { 
      std::cerr << "ERROR: " << e.what() << std::endl << std::endl; 
      std::cerr << desc << std::endl; 
      return 1; 
    } 

    utility::string_t fully_qualified_address = U("http://") + address + U(":") + port;
    on_initialize(fully_qualified_address);
    g_generatorListener->setDefaultIterationCount(num_iterations);

    std::cout << "Press CRTL-C to exit." << std::endl;
    while(1) { sleep(10); }

    // TODO: configure a signal handler to call onExit()
    // when program receives STOP signal
    //
    //on_shutdown();
    return 0;
}
