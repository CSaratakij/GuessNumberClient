extern "C" {
#include <xdo.h>
}

extern "C" void SetWindowTitle(const char* name)
{
    xdo_t* xdo = xdo_new(NULL);
    Window window;

    int error = xdo_get_focused_window(xdo, &window);

    if (!error) {
        xdo_set_window_property(xdo, window, "WM_NAME", name);
        xdo_set_window_property(xdo, window, "_NET_WM_NAME", name);
    }
}

