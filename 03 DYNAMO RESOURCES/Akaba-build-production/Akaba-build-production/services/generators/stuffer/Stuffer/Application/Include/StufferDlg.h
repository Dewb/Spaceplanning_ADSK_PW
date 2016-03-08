#pragma once

class DisplayAppAPI;
class GeneratorGraphicsAPI;

class CStufferDlg : public CDialog
{
public:
	CStufferDlg();
  ~CStufferDlg();

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);

protected:
	virtual BOOL OnInitDialog();

  afx_msg void OnSize(UINT nType, int cx, int cy);
  afx_msg void OnPaint();
  afx_msg void OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags);
  DECLARE_MESSAGE_MAP()

private:
  int m_width;
  int m_height;

  unique_ptr<thread> m_pListenThread;
  unique_ptr<DisplayAppAPI> m_pDisplay;
  unique_ptr<GeneratorGraphicsAPI> m_pGraphics;
  int m_targetIndex;

  void setUI();
    
  bool createTarget(HWND hwnd, int cx, int cy);
  void releaseResources();
};
