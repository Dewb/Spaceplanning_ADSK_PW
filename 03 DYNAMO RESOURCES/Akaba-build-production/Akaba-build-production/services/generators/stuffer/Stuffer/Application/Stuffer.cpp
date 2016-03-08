#include <stdafx.h>

#ifdef USE_DIALOG

#include <Stuffer.h>
#include <StufferDlg.h>
#include <StufferListener.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

BEGIN_MESSAGE_MAP(CStufferApp, CWinApp)
END_MESSAGE_MAP()

CStufferApp theApp;

BOOL CStufferApp::InitInstance()
{
	CWinApp::InitInstance();
	CShellManager *pShellManager = new CShellManager;
	CMFCVisualManager::SetDefaultManager(RUNTIME_CLASS(CMFCVisualManagerWindows));
	SetRegistryKey(_T("Local AppWizard-Generated Applications"));

	CStufferDlg dlg;
	m_pMainWnd = &dlg;

  INT_PTR nResponse = dlg.DoModal();
	if (nResponse == IDOK)
	{
		// TODO: Place code here to handle when the dialog is
		//  dismissed with OK
	}
	else if (nResponse == IDCANCEL)
	{
		// TODO: Place code here to handle when the dialog is
		//  dismissed with Cancel
	}
	else if (nResponse == -1)
	{
		TRACE(traceAppMsg, 0, "Warning: dialog creation failed, so application is terminating unexpectedly.\n");
		TRACE(traceAppMsg, 0, "Warning: if you are using MFC controls on the dialog, you cannot #define _AFX_NO_MFC_CONTROLS_IN_DIALOGS.\n");
	}

	if (pShellManager != NULL)
		delete pShellManager;

	return FALSE;
}

#else

#include <StufferListener.h>

void onExit(void)
{
  GeneratorListener::stop();
}

// Program options handling
#ifdef _WIN32
int processProgramOptions(int, char_t *[], string_t&, string_t&){return -1;};
#else

#include <boost/program_options.hpp>

int processProgramOptions(int argc, char_t *argv[], string_t& address, string_t& port)
{
  namespace po = boost::program_options; 
  po::options_description desc("Options"); 
  desc.add_options() 
    ("address,a", po::value<vector<string>>(), "Address to listen on (default localhost)") 
    ("port,p", po::value<vector<string>>(), "Port to listen on (default 34570)") 
    ("help,h", "Print this message");

  po::variables_map vm; 
  try 
  {
    po::store(po::parse_command_line(argc, argv, desc), vm); // can throw 

    if (vm.count("address")) 
    {
      auto addresses(vm["address"].as<vector<string>>());
      address = addresses[0];
    }

    if (vm.count("port")) 
    {
      auto ports(vm["port"].as<vector<string>>());
      port = ports[0];
    }

    if (vm.count("help")) 
    {
      ucout << "ShellStufferService: Shell-constrained space generation" << endl << desc << endl; 
      return 0; 
    } 

    po::notify(vm); 
    return -1;
  } 
  catch(po::error& e) 
  { 
    ucerr << "ERROR: " << e.what() << endl << endl; 
    ucerr << desc << endl;
    return 1; 
  }
}
#endif

int main(int argc, char_t *argv[])
{
  atexit(onExit);

	string_t address(U("localhost"));
	string_t port(U("34570"));

  int ret(processProgramOptions(argc, argv, address, port));
  if (ret != -1)
    return ret;

  string_t uriText(U("http://"));
  uriText.append(address);
  uriText.append(U(":"));
  uriText.append(port);
  web::uri_builder uri(uriText);
  uri.append_path(U("generator"));
  ucout << U("Using URI: ") << uri.to_uri().to_string() << endl;

  StufferListener::start(uri.to_uri(), nullptr);

  TRACE(U("Press CTRL-C to exit.\n\n"));
#ifdef _WIN32
  auto flag(true);
  while (flag)
  {
    this_thread::sleep_for(std::chrono::milliseconds(10));
  }
#else
  while (1)
  {
    sleep(10);
  }
#endif

  // TODO: configure a signal handler to call onExit()
  // when program receives STOP signal
  //
  // onExit();

  return 0;
}

#endif
