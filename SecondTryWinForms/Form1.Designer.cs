using System.Windows.Forms;

namespace SecondTryWinForms
{
    partial class FrmSpeechTry2
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            console = new TextBox();
            SuspendLayout();
            // 
            // console
            // 
            console.Location = new Point(12, 23);
            console.Multiline = true;
            console.Name = "console";
            console.Size = new Size(765, 415);
            console.TabIndex = 0;
            // 
            // FrmSpeechTry2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(console);
            Name = "FrmSpeechTry2";
            Text = "SpeechTry2";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox console;
    }
}
