using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Liv.Logging
{
    public partial class LogDisplayer : UserControl
    {
        private string _traceFile = @".\Trace\Log.log";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public string TraceFile { get { return _traceFile; } set { _traceFile = value; } }
        System.Threading.Timer mTimer = null;
        int mLastSize = 0;

        public LogDisplayer()
        {
            InitializeComponent();

            Trace.Listeners.Add(new TextBoxTraceListener(txtLog));

            this.Load += new EventHandler(LogDisplayer_Load);
        }

        void LogDisplayer_Load(object sender, EventArgs e)
        {
            if (this.DesignMode) return;

            if (!File.Exists(TraceFile))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(TraceFile));
                File.Create(TraceFile);
            }
        }

        public class TextBoxTraceListener : TraceListener
        {
            private TextBox _target;
            private StringSendDelegate _invokeWrite;

            public TextBoxTraceListener(TextBox target)
            {
                _target = target;
                _invokeWrite = new StringSendDelegate(SendString);
            }

            public override void Write(string message)
            {
                if (!_target.IsHandleCreated) return;
				message = Log.UnescapeColors(message);
                _target.Invoke(_invokeWrite, new object[] { message });
            }

            public override void WriteLine(string message)
            {
                if (!_target.IsHandleCreated) return;
				message = Log.UnescapeColors(message);
				_target.Invoke(_invokeWrite, new object[] { message + System.Environment.NewLine });
            }

            private delegate void StringSendDelegate(string message);
            private void SendString(string message)
            {
                // No need to lock text box as this function will only 
                // ever be executed from the UI thread
                _target.Text += message;
            }
        }
    }
}
