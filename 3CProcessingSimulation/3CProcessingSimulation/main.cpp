#include "gk2_scene.h"
#include "gk2_window.h"
#include "gk2_exceptions.h"

using namespace std;
using namespace gk2;

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE prevInstance, LPWSTR cmdLine, int cmdShow)
{
	UNREFERENCED_PARAMETER(prevInstance);
	UNREFERENCED_PARAMETER(cmdLine);
	shared_ptr<ApplicationBase> app;
	shared_ptr<Window> w;
	int exitCode = 0;
	try
	{
		app.reset(new Scene(hInstance));
		w.reset(new Window(hInstance, 800, 800, L"3C Processing Simulation"));
		exitCode = app->Run(w.get(), cmdShow);
	}
	catch (Exception& e)
	{
		MessageBoxW(NULL, e.getMessage().c_str(), L"Error", MB_OK);
		exitCode = e.getExitCode();
	}
	catch(...)
	{
		MessageBoxW(NULL, L"Unknown Error", L"Error", MB_OK);
		exitCode = -1;
	}
	w.reset();
	app.reset();
	return exitCode;
}