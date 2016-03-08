#include <stdafx.h>

#ifdef USE_DIALOG

#include <StufferDlg.h>
#include <Resource.h>
#include <StufferListener.h>
#include <D2DDisplay.h>
#include <GeneratorGraphics.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

CStufferDlg::CStufferDlg()
: CDialog(IDD_STUFFER_DIALOG, nullptr),
  m_width(0),
  m_height(0),
  m_targetIndex(-1)
{
}

CStufferDlg::~CStufferDlg()
{
  if (m_pListenThread)
  {
    GeneratorListener::stop();
    m_pListenThread->join();
    //m_pListenThread.reset();
  }

  //m_pDisplay.reset();
  //m_pGraphics.reset();
}

void CStufferDlg::DoDataExchange(CDataExchange* pDX)
{
  CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CStufferDlg, CDialog)
  ON_WM_PAINT()
  ON_WM_SIZE()
  ON_WM_KEYDOWN()
  ON_WM_LBUTTONDOWN()
END_MESSAGE_MAP()

void CStufferDlg::setUI()
{
  CRect rc;
  GetClientRect(&rc);

  m_width = rc.Width();
  m_height = rc.Height();
}


BOOL CStufferDlg::OnInitDialog()
{
  CDialog::OnInitDialog();

  m_pDisplay = move(D2DDisplay::create());
  m_pGraphics.reset(new GeneratorGraphics(*m_pDisplay));

  m_pListenThread.reset(new thread(StufferListener::start, U("http://localhost:34570/generator"), m_pGraphics.get()));

  setUI();

  return TRUE;
}

void CStufferDlg::releaseResources()
{
  m_pDisplay->releaseResources();
  m_targetIndex = -1;
  m_pGraphics->releaseResources();
}

bool CStufferDlg::createTarget(HWND hwnd, int cx, int cy)
{
  if (m_targetIndex != -1)
  {
    int width;
    int height;
    m_pDisplay->getTargetSize(m_targetIndex, width, height);
    if (cx != width || cy != height)
      releaseResources();
  }

  if (m_targetIndex == -1)
  {
    RECT rc;
    GetClientRect(&rc);
    m_targetIndex = m_pDisplay->createTarget(hwnd, rc.right - rc.left, rc.bottom - rc.top);
  }

  if (m_targetIndex != -1)
  {
    if (m_pGraphics)
      m_pGraphics->setTargetIndex(m_targetIndex);

    return true;
  }

  releaseResources();
  
  return false;
}

void CStufferDlg::OnSize(UINT nType, int cx, int cy)
{
  CDialog::OnSize(nType, cx, cy);
  setUI();
  Invalidate();
  UpdateWindow();
}

void CStufferDlg::OnPaint()
{
  CDialog::OnPaint();

  if (!m_pDisplay)
    return;

  createTarget(m_hWnd, m_width, m_height);
  m_pGraphics->displayPlanState(nullptr);
}

void CStufferDlg::OnKeyDown(UINT nChar, UINT /*nRepCnt*/, UINT /*nFlags*/)
{
  auto delta(0);
  if (nChar == 189)
    delta = -1;
  else if (nChar == 187)
    delta = 1;

  if (delta != 0)
  {
    m_pGraphics->setDisplayLevel(m_pGraphics->getDisplayLevel() + delta);
    m_pGraphics->displayPlanState(nullptr);
  }
}
#endif
