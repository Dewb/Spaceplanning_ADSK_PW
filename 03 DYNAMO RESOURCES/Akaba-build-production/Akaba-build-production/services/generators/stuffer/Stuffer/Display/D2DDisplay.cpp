#include <stdafx.h>

#ifdef USE_DIALOG

#include <D2DDisplay.h>
#include <D2DContext.h>

#include <d2d1.h>
#include <d2d1_1.h>

unique_ptr<DisplayAppAPI> D2DDisplay::create()
{
  unique_ptr<D2DDisplay> pDisplay(new D2DDisplay);
  if (!pDisplay->initialize())
    return nullptr;

  return move(pDisplay);
}

D2DDisplay::D2DDisplay()
: m_pFactory(nullptr),
  m_targetIndex(0),
  m_brushIndex(0)
{
}

bool D2DDisplay::initialize()
{
  HRESULT hr = D2D1CreateFactory(
    D2D1_FACTORY_TYPE_SINGLE_THREADED,
    &m_pFactory);

  // Square end-cap stroke
  m_pStrokes[0] = nullptr;
  ID2D1StrokeStyle*& pStroke0 = m_pStrokes[0];
  hr = m_pFactory->CreateStrokeStyle(
    D2D1::StrokeStyleProperties(
    D2D1_CAP_STYLE_SQUARE,
    D2D1_CAP_STYLE_SQUARE),
    nullptr, 0,
    &pStroke0);

  if (!SUCCEEDED(hr))
    return false;

  // Dashed line stroke
  m_pStrokes[1] = nullptr;
  ID2D1StrokeStyle*& pStroke1 = m_pStrokes[1];
  hr = m_pFactory->CreateStrokeStyle(
    D2D1::StrokeStyleProperties(
      D2D1_CAP_STYLE_FLAT,
      D2D1_CAP_STYLE_FLAT,
      D2D1_CAP_STYLE_FLAT,
      D2D1_LINE_JOIN_MITER,
      10.0f,
      D2D1_DASH_STYLE_DASH),
      nullptr, 0,
    &pStroke1);

  return SUCCEEDED(hr);
}

int D2DDisplay::createTarget(void* hwnd, int width, int height)
{
  D2D1_SIZE_U size = D2D1::SizeU(width, height);

  int index = m_targetIndex++;
  ID2D1HwndRenderTarget*& pTarget = get<0>(m_targets[index]);
  pTarget = nullptr;
  HRESULT hr = m_pFactory->CreateHwndRenderTarget(
    D2D1::RenderTargetProperties(),
    D2D1::HwndRenderTargetProperties(static_cast<HWND>(hwnd), size),
    &pTarget);

  return SUCCEEDED(hr) ? index : -1;
}

void D2DDisplay::getTargetSize(int index, int& width, int& height)
{
  width = 0;
  height = 0;
  auto targetIt = m_targets.find(index);
  if (targetIt == m_targets.end())
    return;

  D2D1_SIZE_F size = get<0>(targetIt->second)->GetSize();
  width = static_cast<int>(size.width);
  height = static_cast<int>(size.height);
}

int D2DDisplay::createSolidBrush(int index, const D2D1::ColorF& color)
{
  auto targetIt = m_targets.find(index);
  if (targetIt == m_targets.end())
    return -1;

  ID2D1SolidColorBrush* pBrush = nullptr;
  HRESULT hr = get<0>(targetIt->second)->CreateSolidColorBrush(color, &pBrush);
  if (SUCCEEDED(hr))
  {
    brush_map& brushes(get<1>(targetIt->second));
    int brushIndex = m_brushIndex++;
    brushes[brushIndex] = pBrush;

    return brushIndex;
  }

  return -1;
}

namespace
{
  template<class Interface>
  inline void SafeRelease(Interface **ppInterfaceToRelease)
  {
    if (*ppInterfaceToRelease != NULL)
    {
      (*ppInterfaceToRelease)->Release();
      (*ppInterfaceToRelease) = NULL;
    }
  }
}

void D2DDisplay::releaseResources()
{
  for (auto& targetInfo : m_targets)
  {
    SafeRelease(&get<0>(targetInfo.second));
    for (auto& brush : get<1>(targetInfo.second))
      SafeRelease(&brush.second);
  }

  m_targets.clear();
}

unique_ptr<DisplayContextAPI> D2DDisplay::createContext(int index)
{
  auto targetIt = m_targets.find(index);
  if (targetIt == m_targets.end())
    return nullptr;

  return move(unique_ptr<DisplayContextAPI>(
    new D2DContext(
      *this, 
      get<0>(targetIt->second),
      targetIt->first)));
}

ID2D1SolidColorBrush* D2DDisplay::getBrush(int targetIndex, int brushIndex) const
{
  auto targetIt = m_targets.find(targetIndex);
  if (targetIt == m_targets.end())
    return nullptr;

  const brush_map& brushes(get<1>(targetIt->second));
  auto brushIt = brushes.find(brushIndex);
  if (brushIt == brushes.end())
    return nullptr;

  return brushIt->second;
}

ID2D1StrokeStyle* D2DDisplay::getStroke(int strokeIndex) const
{
  const auto& it = m_pStrokes.find(strokeIndex);
  if (it == m_pStrokes.end())
    return nullptr;

  return it->second;
}

#endif
