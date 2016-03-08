#pragma once

#include <DisplayAppAPI.h>

struct ID2D1Factory1;

class D2DDisplay : public DisplayAppAPI
{
public:
  static unique_ptr<DisplayAppAPI> create();

  int createSolidBrush(int index, const D2D1::ColorF& color);
  ID2D1SolidColorBrush* getBrush(int targetIndex, int brushIndex) const;
  ID2D1StrokeStyle* getStroke(int strokeIndex) const;

protected:
  int createTarget(void* hwnd, int width, int height);
  void getTargetSize(int index, int& width, int& height);
  void releaseResources();

  unique_ptr<DisplayContextAPI> createContext(int index);

private:
  ID2D1Factory1* m_pFactory;
  map<int, ID2D1StrokeStyle*> m_pStrokes;

  int m_targetIndex;
  int m_brushIndex;
  using brush_map = map<int, ID2D1SolidColorBrush*>;
  using target_data = tuple<ID2D1HwndRenderTarget*, brush_map>;
  map<int, target_data> m_targets;

  D2DDisplay();
  bool initialize();
};
