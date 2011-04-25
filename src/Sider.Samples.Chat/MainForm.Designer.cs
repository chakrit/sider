namespace Sider.Samples.Chat
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
      this.JoinButton = new System.Windows.Forms.Button();
      this.RoomsCombo = new System.Windows.Forms.ComboBox();
      this.NameBox = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // JoinButton
      // 
      this.JoinButton.Location = new System.Drawing.Point(197, 12);
      this.JoinButton.Name = "JoinButton";
      this.JoinButton.Size = new System.Drawing.Size(75, 49);
      this.JoinButton.TabIndex = 0;
      this.JoinButton.Text = "Join";
      this.JoinButton.UseVisualStyleBackColor = true;
      this.JoinButton.Click += new System.EventHandler(this.JoinButton_Click);
      // 
      // RoomsCombo
      // 
      this.RoomsCombo.FormattingEnabled = true;
      this.RoomsCombo.Location = new System.Drawing.Point(12, 40);
      this.RoomsCombo.Name = "RoomsCombo";
      this.RoomsCombo.Size = new System.Drawing.Size(179, 21);
      this.RoomsCombo.TabIndex = 1;
      this.RoomsCombo.Text = "Sider";
      // 
      // NameBox
      // 
      this.NameBox.Location = new System.Drawing.Point(12, 12);
      this.NameBox.Name = "NameBox";
      this.NameBox.Size = new System.Drawing.Size(179, 20);
      this.NameBox.TabIndex = 2;
      this.NameBox.Text = "(name)";
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 77);
      this.Controls.Add(this.NameBox);
      this.Controls.Add(this.RoomsCombo);
      this.Controls.Add(this.JoinButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Redis chat!";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button JoinButton;
    private System.Windows.Forms.ComboBox RoomsCombo;
    private System.Windows.Forms.TextBox NameBox;
  }
}

