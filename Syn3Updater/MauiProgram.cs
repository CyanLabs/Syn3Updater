using Microsoft.Maui.LifecycleEvents;

namespace Syn3Updater
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>();
#if WINDOWS
            builder.ConfigureLifecycleEvents(events => {

                events.AddWindows(wndLifeCycleBuilder => {
                    wndLifeCycleBuilder.OnWindowCreated(window => {
                        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        if (PInvoke.User32.GetWindowRect(hwnd, out PInvoke.RECT rect1))
                        {
                            //X = rect1.left;
                            //Y = rect1.top;
                            //appSettings.WindowsSettings.AppWindow.Width = rect1.right - rect1.left;
                            //appSettings.WindowsSettings.AppWindow.Height = rect1.bottom - rect1.top;
                        }
                        PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                        (Int32)rect1.left, (Int32)rect1.top, (Int32)800, (Int32)640,
                        PInvoke.User32.SetWindowPosFlags.SWP_SHOWWINDOW);
                    });
                });
            });
#endif
            return builder.Build();
        }
    }
}