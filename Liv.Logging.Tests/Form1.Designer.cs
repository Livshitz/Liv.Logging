namespace Liv.Logging.Tests
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.logDisplayer1 = new Liv.Logging.LogDisplayer();
			this.SuspendLayout();
			// 
			// logDisplayer1
			// 
			this.logDisplayer1.Location = new System.Drawing.Point(13, 13);
			this.logDisplayer1.Name = "logDisplayer1";
			this.logDisplayer1.Size = new System.Drawing.Size(525, 275);
			this.logDisplayer1.TabIndex = 0;
			this.logDisplayer1.TraceFile = ".\\Trace\\Log.log";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(550, 300);
			this.Controls.Add(this.logDisplayer1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private LogDisplayer logDisplayer1;
	}
}

