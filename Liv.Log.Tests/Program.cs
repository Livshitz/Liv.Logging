using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Liv.Log.Tests
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			#region Log usage demo
			// Create console window and auto listen to logger
			Log.SetConsoleTracing();
			// Or manually add to listeners
			/*
			var ctl = new ConsoleTraceListener();
			ctl.Filter = new EventTypeFilter(SourceLevels.Verbose);
			Trace.Listeners.Add(ctl);
			Trace.AutoFlush = true;
			*/

			// Set write log into file
			Log.SetWriteToFile("myLog.log");

			Log.Info("Hello there!");
			Log.Verbose("This is verbose, and look at this {0} & fast way to write to log", "awsome");
			Log.SetLogLevel(Log.TraceLevel.Info);
			Log.Verbose("This you should not see");
			Log.SetLogLevel(Log.TraceLevel.Debug);
			Log.Debug(Compunentes.CompenentA, "This is debug message for CompenentA");
			Log.TraceEntity(new MyCustomClass() { Title = "This is the title", Number = 5 });
			#endregion


			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}

		enum Compunentes
		{
			CompenentA,
			CompenentB,
		}

		class MyCustomClass
		{
			public string Title { get; set; }
			public int Number { get; set; }
		}

	}
}
