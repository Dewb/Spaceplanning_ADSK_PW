#pragma once

#include <resource.h>

class CStufferApp : public CWinApp
{
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};

extern CStufferApp theApp;
