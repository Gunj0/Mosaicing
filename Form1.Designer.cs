namespace Mosaicing
{
  partial class Form1
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
      pictureBox1 = new PictureBox();
      ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
      SuspendLayout();
      // 
      // Form1
      // 
      TopMost = false;
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(515, 1026);
      Controls.Add(pictureBox1);
      KeyPreview = true;
      Name = "Form1";
      Text = "Form1";
      TopMost = true;
      KeyDown += Form1_KeyDown;
      ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
      ResumeLayout(false);
      // 
      // pictureBox1
      // 
      pictureBox1.Location = new Point(2, 0);
      pictureBox1.Name = "pictureBox1";
      pictureBox1.Size = new Size(512, 1024);
      pictureBox1.TabIndex = 0;
      pictureBox1.TabStop = false;
    }

    #endregion

    private PictureBox pictureBox1;
  }
}
