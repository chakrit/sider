namespace Sider.GUI
{
  partial class MainForm
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
      if (disposing && (components != null)) {
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
      this.pingButton = new System.Windows.Forms.Button();
      this.pingLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // pingButton
      // 
      this.pingButton.Location = new System.Drawing.Point(12, 12);
      this.pingButton.Name = "pingButton";
      this.pingButton.Size = new System.Drawing.Size(75, 23);
      this.pingButton.TabIndex = 0;
      this.pingButton.Text = "PING";
      this.pingButton.UseVisualStyleBackColor = true;
      this.pingButton.Click += new System.EventHandler(this.pingButton_Click);
      // 
      // pingLabel
      // 
      this.pingLabel.AutoSize = true;
      this.pingLabel.Location = new System.Drawing.Point(93, 17);
      this.pingLabel.Name = "pingLabel";
      this.pingLabel.Size = new System.Drawing.Size(38, 13);
      this.pingLabel.TabIndex = 1;
      this.pingLabel.Text = "(result)";
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(672, 395);
      this.Controls.Add(this.pingLabel);
      this.Controls.Add(this.pingButton);
      this.Name = "MainForm";
      this.Text = "Console";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    protected System.Windows.Forms.Button pingButton;
    protected System.Windows.Forms.Label pingLabel;
  }
}

