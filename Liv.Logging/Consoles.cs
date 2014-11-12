using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Liv.Logging
{
	internal static class Consoles
	{
		[DllImport("kernel32.dll")]
		public static extern IntPtr GetConsoleWindow();

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool AllocConsole();

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		private const int STD_OUTPUT_HANDLE = -11;
		private const int MY_CODE_PAGE = 437;

		private static void SetConsoleOutput()
		{
			IntPtr defaultStdout = new IntPtr(7);
			IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);

			if (stdHandle != defaultStdout)
			{
				SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
				FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
				Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
				StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
				standardOutput.AutoFlush = true;
				Console.SetOut(standardOutput);
			}
			else
			{
				// reopen stdout.
				TextWriter writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
				Console.SetOut(writer);
			}
		}

		public static void ShowWindow(IntPtr hWnd)
		{
			ShowWindow(hWnd, 5);
		}

		public static void ShowConsole()
		{
			var handle = GetConsoleWindow();

			if (handle == IntPtr.Zero)
			{
				AllocConsole();
				SetConsoleOutput();
			}
			else
			{
				ShowWindow(handle);
			}
		}
	}
}
