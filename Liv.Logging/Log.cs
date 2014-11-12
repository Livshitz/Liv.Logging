using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection;
using System.Configuration;
using System.Xml;

namespace Liv.Logging
{
    public static class Log
    {
        private static TraceSwitch TraceSwitch = new TraceSwitch("GeneralSwitch", "Switch in config file");
        private static readonly object _Lock = new object();
        public static bool IsDebugMode = false;
        public static bool IsShowAt = false;
        public static bool IsAddDate = false;

        // Summary:
        //     Specifies what messages to output for the System.Diagnostics.Debug, System.Diagnostics.Trace
        //     and System.Diagnostics.TraceSwitch classes.
        public enum TraceLevel
        {
            // Summary:
            //     Output no tracing and debugging messages.
            Off = 0,
            //
            // Summary:
            //     Output error-handling messages.
            Error = 1,
            //
            // Summary:
            //     Output warnings and error-handling messages.
            Warning = 2,
            //
            // Summary:
            //     Output informational messages, warnings, and error-handling messages.
            Info = 3,
            //
            // Summary:
            //     Output debug messages, warnings, and error-handling messages.
            Debug = 32,
            //
            // Summary:
            //     Output all debugging and tracing messages.
            Verbose = 4,
        }

        public static void SetLogLevel(TraceLevel traceLevel)
        {
            if (traceLevel == TraceLevel.Debug)
            {
                IsDebugMode = true;
                TraceSwitch.Level = System.Diagnostics.TraceLevel.Verbose;
                Log.Debug("Log:SetLogLevel: Debug mode is on.");
            }
            else
            {
                TraceSwitch.Level = (System.Diagnostics.TraceLevel)traceLevel;
            }
        }

        private static void WriteLineIfWrapper(bool condition, object value, TraceLevel level)
        {
            Trace.WriteLineIf(condition, FormatMessage(level, value.ToString()));
        }

        public static Exception Exception(string format, params object[] args)
        {
            Exception newEx = new Exception(String.Format(format, args));
            WriteLineIfWrapper(TraceSwitch.TraceError, newEx.Message, TraceLevel.Error);
            return newEx;
        }

        public static Exception Exception(string msg, Exception ex)
        {
            string innerEx = "";
            if (ex != null && ex.InnerException != null) innerEx = String.Format("\n\tInner Exception: {0}", ex.InnerException.Message);
            if (innerEx == "" && ex != null && ex.Message != null) innerEx = String.Format("\n\tInner Exception: {0}", ex.Message);

            Exception newEx = new Exception(msg + innerEx);
            WriteLineIfWrapper(TraceSwitch.TraceError, newEx.Message, TraceLevel.Error);
            return newEx;
        }

        public static Exception Exception(string format, Exception ex, params object[] args)
        {
            string innerEx = "";
            string msg = String.Format(format, args);
            if (innerEx == "" && ex != null) innerEx = String.Format("\n\tException: {0}", ex.Message);
            if (ex != null && ex.InnerException != null) innerEx += String.Format("\n\tInner Exception: {0}", ex.InnerException.Message);

            Exception newEx = new Exception(msg + innerEx);
            WriteLineIfWrapper(TraceSwitch.TraceError, newEx.Message, TraceLevel.Error);
            return newEx;
        }

        public static string Error(string message)
        {
            WriteLineIfWrapper(TraceSwitch.TraceError, message, Log.TraceLevel.Error);
            return message;
        }

        public static string Error(string format, params object[] args)
        {
            string message = String.Format(format, args);
            WriteLineIfWrapper(TraceSwitch.TraceError, message, Log.TraceLevel.Error);
            return message;
        }

        public static string Error(string message, Exception ex)
        {
            if (ex != null)
            {
                string exceptionText = ex.Message;
                if (ex.InnerException != null)
                    exceptionText += "\n\tInner:" + ex.InnerException.Message;

                string stackTrace = ex.StackTrace;
                if (ex.InnerException != null)
                    stackTrace += "\n\tInner:" + ex.InnerException.StackTrace;

                message = String.Format("{0} \n\tex: {1}. \n\tStackTrace: {2}", message, exceptionText, stackTrace);

            }
            WriteLineIfWrapper(TraceSwitch.TraceError, message, Log.TraceLevel.Error);
            return message;
        }

        public static string Error(Exception ex)
        {
            string message = "";
            if (ex != null)
                message = String.Format("{0} \n\tex: {1}. \n\tStackTrace: {2}", message, ex.Message, ex.StackTrace);
            WriteLineIfWrapper(TraceSwitch.TraceError, message, Log.TraceLevel.Error);
            return message;
        }

        public static string Error(string format, Exception ex, params object[] args)
        {
            string innerEx = "";
            if (ex != null && ex.InnerException != null) innerEx = String.Format("\n\tInner Exception: {0}", ex.InnerException.Message);
            return Error(String.Format(format, args) + innerEx, ex);
        }

        public static string Debug(string message, params object[] args)
        {
            WriteLineIfWrapper(IsDebugMode, String.Format(message, args), TraceLevel.Debug);
            return message;
        }

        public static string Debug(Enum aComponent, string message)
        {
            string newMsg = GetEnumName(aComponent.GetType(), aComponent) + ":" + message;
            WriteLineIfWrapper(IsDebugMode, newMsg, TraceLevel.Debug);
            return newMsg;
        }

        public static string Debug(Enum aComponent, string format, params object[] args)
        {
            string newMsg = GetEnumName(aComponent.GetType(), aComponent) + ":" + String.Format(format, args);
            WriteLineIfWrapper(IsDebugMode, newMsg, TraceLevel.Debug);
            return newMsg;
        }

        public static string Info(string message)
        {
            WriteLineIfWrapper(TraceSwitch.TraceInfo, message, Log.TraceLevel.Info);
            if (Trace.Listeners.Count > 0) Trace.Listeners[0].Flush();
            return message;
        }

        public static string Info(string format, params object[] args)
        {
            return Info(String.Format(format, args));
        }

        public static string Verbose(string message)
        {
            WriteLineIfWrapper(TraceSwitch.TraceVerbose, message, Log.TraceLevel.Verbose);
            return message;
        }

        public static string Verbose(string format, params object[] args)
        {
            return Verbose(String.Format(format, args));
        }

        public static string Warning(string message)
        {
            WriteLineIfWrapper(TraceSwitch.TraceWarning, message, Log.TraceLevel.Warning);
            return message;
        }

        public static string Warning(string format, params object[] args)
        {
            return Warning(String.Format(format, args));
        }

        private static string GetEnumName(Type aEnum, object aValue)
        {
            return Enum.GetName(aEnum, aValue);
        }

        public delegate string ToStringDlg(object aObj);
        public static string TraceEntity<T>(T aEntity)
        {
            try
            {
                if (!IsDebugMode && !TraceSwitch.TraceVerbose) return null;

                return TraceEntity<T>(aEntity, new ToStringDlg(delegate(object aObj)
                {
                    if (aObj != null) return aObj.ToString();
                    return "null";
                }));
            }
            catch (Exception) { }

            return null;
        }

        public static string TraceEntity<T>(T aEntity, ToStringDlg aToStringDlg)
        {
            return TraceEntity(aEntity, typeof(T), aToStringDlg);
        }

        public static string TraceEntity(object aEntity, Type aType, ToStringDlg aToStringDlg)
        {
            StringBuilder ret = new StringBuilder();
            Type t = aType;

            ret.Append(Info("[#] TraceEntity ({0}):", t.FullName) + "\n\r");

            PropertyInfo[] pInfo = t.GetProperties();

            // run threw all the object properties
            foreach (PropertyInfo info in pInfo)
            {
                string val = "";
                val = aToStringDlg(info.GetValue(aEntity, null));
                ret.Append(Info("|\t" + info.Name + ": \"" + val + "\" \t") + "\n\r");

                bool isStruct = info.PropertyType.IsValueType;
                if (!isStruct && info.PropertyType != typeof(String)) TraceEntity(val, info.PropertyType, aToStringDlg);
            }

            // If no properties, try to get members
            if (pInfo.Length == 0)
            {
                FieldInfo[] fInfo = t.GetFields();
                foreach (FieldInfo info in fInfo)
                {
                    string val = "";
                    val = aToStringDlg(info.GetValue(aEntity));
                    ret.Append(Info("|\t" + info.Name + ": \"" + val + "\" \t") + "\n\r");

                    bool isStruct = info.FieldType.IsValueType;
                    if (!isStruct) TraceEntity(val, info.FieldType, aToStringDlg);
                }
            }

            return ret.ToString();
        }

        private static string FormatMessage(Log.TraceLevel traceLevel, string msg)
        {
            var ret = msg;
            var _Prefix = "";
            switch (traceLevel)
            {
                case Log.TraceLevel.Verbose:
                    _Prefix = "-";
                    break;
                case Log.TraceLevel.Info:
                    _Prefix = ">";
                    break;
                case Log.TraceLevel.Debug:
                    _Prefix = "D";
                    break;
                case Log.TraceLevel.Warning:
                    _Prefix = "*";
                    break;
                case Log.TraceLevel.Error:
                    _Prefix = "!";
                    break;
            }

            if (IsAddDate)
            {
                ret = string.Format("{1}|{2}[{0}] {3}", _Prefix, DateTime.Now.ToString("dd/MM/yy|HH:mm:ss.fff"), AppDomain.GetCurrentThreadId(), msg);
            }
            else
            {
                ret = string.Format("{1}[{0}] {2}", _Prefix, AppDomain.GetCurrentThreadId(), msg);
            }

            if (IsShowAt && traceLevel != Log.TraceLevel.Error)
            {
                ret += String.Format(" (at:{0})", GetCallingMethod());
            }

            return ret;
        }

        private static string GetCallingMethod()
        {
            string ret = "";

            try
            {
                StackTrace stackTrace = new StackTrace(true);
                StackFrame stackFrame = stackTrace.GetFrame(8);
                System.Reflection.MethodBase methodBase = stackFrame.GetMethod();

                string _FilePath = stackFrame.GetFileName();
                if (!String.IsNullOrEmpty(_FilePath))
                {
                    _FilePath = _FilePath.ToLower();
                    //_FilePath = _FilePath.Replace(Feedox.Common.SharedScope.Instance.ServerRootPath.ToLower(), "");
                    if (_FilePath[0] == '\\') _FilePath = _FilePath.Remove(0, 1);
                    _FilePath = "-" + _FilePath;
                }
                else
                {
                    _FilePath = "";
                }

                ret = methodBase.Name + _FilePath;
            }
            catch { }
            return ret;
        }

        public static void SetConsoleTracing(bool isAddDate = true)
        {
            try
            {
                IsAddDate = isAddDate;
                Consoles.ShowConsole();
                Console.WriteLine("");
                Console.WriteLine("--------------------");
                Console.WriteLine("Console was alocated");
            }
            catch(Exception ex)
            {
                Console.WriteLine("SetConsoleTracing: Error:" + ex.Message);
            }
            var ctl = new ConsoleTraceListener();
            ctl.Filter = new EventTypeFilter(SourceLevels.Verbose);

            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;

            Log.TraceSwitch.Level = System.Diagnostics.TraceLevel.Verbose;
            Log.Info("Log:SetConsoleTracing: Done");
        }

        public static void Write(string text)
        {
            Trace.WriteLine(text);
        }

        public static void SetWriteToFile(string logFilePath, TraceLevel traceLevel = TraceLevel.Verbose)
        {
            var ctl = new TextWriterTraceListener(logFilePath);
            ctl.Filter = new EventTypeFilter(SourceLevels.Verbose);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
            SetLogLevel(traceLevel);
            Log.Verbose("Log:SetWriteToFile: Setting log file: {0}", logFilePath);
        }
    }

    /// <summary>
    /// This class is mechanism for collecting and recording the messages that are sent,
    /// For debug Goal.    
    /// The purpose of a listener is to collect, store, and route those messages.
    /// Listeners direct the tracing output to an appropriate target,
    /// That declares in the *.config xml-file under the sub node - "initializeData".
    /// This class is the TextWriterTraceListener listener type. 
    /// </summary>
    public class CustomTraceListener : TextWriterTraceListener
    {
        public CustomTraceListener(string aFileName)
            : base(ModifyFileName(aFileName))
        {

        }

        private static string ModifyFileName(string aRelativePath)
        {
            // Map the relative path given in the web.cofig (you cant by default).
            return aRelativePath.Replace("~", System.AppDomain.CurrentDomain.BaseDirectory);
        }

        public override void WriteLine(string message)
        {
            WriteLine(message, Log.TraceLevel.Verbose.ToString());
        }

        public override void WriteLine(string message, string amyTraceLevel)
        {
            string _Prefix = "";

            Log.TraceLevel traceLevel = (Log.TraceLevel)Enum.Parse(typeof(Log.TraceLevel), amyTraceLevel);

            switch (traceLevel)
            {
                case Log.TraceLevel.Verbose:
                    _Prefix = "-";
                    break;
                case Log.TraceLevel.Info:
                    _Prefix = ">";
                    break;
                case Log.TraceLevel.Debug:
                    _Prefix = "D";
                    break;
                case Log.TraceLevel.Warning:
                    _Prefix = "*";
                    break;
                case Log.TraceLevel.Error:
                    _Prefix = "!";
                    break;
            }

            if (Writer == null) return;

            Writer.Write("{1}[{2}]-[{0}]", _Prefix, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"), AppDomain.GetCurrentThreadId());
            base.Write(message);

            if (traceLevel != Log.TraceLevel.Error)
            {
                Writer.Write(" (at:{0})", GetCallingMethod());
            }

            base.WriteLine("");
        }

        private string GetCallingMethod()
        {
            string ret = "";

            try
            {
                StackTrace stackTrace = new StackTrace(true);
                StackFrame stackFrame = stackTrace.GetFrame(8);
                System.Reflection.MethodBase methodBase = stackFrame.GetMethod();

                string _FilePath = stackFrame.GetFileName();
                if (!String.IsNullOrEmpty(_FilePath))
                {
                    _FilePath = _FilePath.ToLower();
                    //_FilePath = _FilePath.Replace(Feedox.Common.SharedScope.Instance.ServerRootPath.ToLower(), "");
                    if (_FilePath[0] == '\\') _FilePath = _FilePath.Remove(0, 1);
                    _FilePath = "-" + _FilePath;
                }
                else
                {
                    _FilePath = "";
                }

                ret = methodBase.Name + _FilePath;
            }
            catch { }
            return ret;
        }
    }
}
