namespace ThirdTry
{
    partial class FrmThirdTry
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
            listeningFeedbackIndicator = new Panel();
            SuspendLayout();
            // 
            // console
            // 
            console.Location = new Point(12, 25);
            console.Multiline = true;
            console.Name = "console";
            console.Size = new Size(738, 413);
            console.TabIndex = 0;
            // 
            // listeningFeedbackIndicator
            // 
            listeningFeedbackIndicator.Location = new Point(756, 12);
            listeningFeedbackIndicator.Name = "listeningFeedbackIndicator";
            listeningFeedbackIndicator.Size = new Size(41, 45);
            listeningFeedbackIndicator.TabIndex = 1;
            listeningFeedbackIndicator.Paint += listeningFeedbackIndicator_Paint;
            // 
            // FrmThirdTry
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(listeningFeedbackIndicator);
            Controls.Add(console);
            Name = "FrmThirdTry";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox console;
        private Panel listeningFeedbackIndicator;
    }
}
