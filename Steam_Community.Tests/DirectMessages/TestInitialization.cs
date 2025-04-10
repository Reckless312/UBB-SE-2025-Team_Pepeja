using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Steam_Community.Tests.DirectMessages
{
    [TestClass]
    public class TestInitialization
    {
        private static Thread? uiThread;
        private static ManualResetEventSlim threadStarted = new ManualResetEventSlim(false);

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            try
            {
                Console.WriteLine("Starting UI thread for WinUI components...");

                // Create and start a UI thread for tests that need UI components
                uiThread = new Thread(() =>
                {
                    try
                    {
                        // Initialize COM for this thread (required for WinUI)
                        CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);

                        // Signal that the thread has started
                        threadStarted.Set();

                        // Keep the thread alive for the duration of testing
                        // Process Windows messages
                        Message msg;
                        while (GetMessage(out msg, IntPtr.Zero, 0, 0))
                        {
                            TranslateMessage(ref msg);
                            DispatchMessage(ref msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UI thread exception: {ex}");
                    }
                    finally
                    {
                        CoUninitialize();
                    }
                });

                uiThread.IsBackground = true;
                uiThread.SetApartmentState(ApartmentState.STA); // Set STA for UI thread
                uiThread.Start();

                // Wait for the UI thread to start with a timeout
                if (!threadStarted.Wait(TimeSpan.FromSeconds(5)))
                {
                    Console.WriteLine("Warning: Timeout waiting for UI thread to start!");
                }
                else
                {
                    Console.WriteLine("UI thread started successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test initialization error: {ex}");
                throw;
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            // Clean up resources
            threadStarted.Dispose();

            // Post a WM_QUIT message to the UI thread to end the message loop
            if (uiThread != null && uiThread.IsAlive)
            {
                PostThreadMessage(GetThreadId(uiThread), WM_QUIT, UIntPtr.Zero, IntPtr.Zero);

                // Give the thread a chance to terminate cleanly
                if (!uiThread.Join(TimeSpan.FromSeconds(3)))
                {
                    Console.WriteLine("Warning: Had to abort UI thread");
                    uiThread.Interrupt();
                }
            }
        }

        #region Win32 API

        // Windows constants and P/Invoke functions for UI thread handling
        private const int WM_QUIT = 0x0012;
        private const int COINIT_APARTMENTTHREADED = 0x2;
        private const int COINIT_DISABLE_OLE1DDE = 0x4;

        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            public IntPtr hwnd;
            public uint message;
            public UIntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point point;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMessage(out Message lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage([In] ref Message lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage([In] ref Message lpmsg);

        [DllImport("user32.dll")]
        private static extern bool PostThreadMessage(uint threadId, int msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetThreadId(Thread thread);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(IntPtr pvReserved, int dwCoInit);

        [DllImport("ole32.dll")]
        private static extern void CoUninitialize();

        #endregion
    }
}