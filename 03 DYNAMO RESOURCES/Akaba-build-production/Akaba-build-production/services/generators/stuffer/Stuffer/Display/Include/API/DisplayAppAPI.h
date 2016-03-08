#pragma once

class DisplayContextAPI;

class DisplayAppAPI
{
public:
  virtual ~DisplayAppAPI() = default;

  virtual int createTarget(void* hwnd, int width, int height) = 0;
  virtual void getTargetSize(int index, int& width, int& height) = 0;
  virtual void releaseResources() = 0;

  virtual unique_ptr<DisplayContextAPI> createContext(int index) = 0;
};
