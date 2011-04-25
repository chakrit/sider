namespace Sider.Samples.Chat
{
  partial class ChatForm
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
      this.ParticipantList = new System.Windows.Forms.ListBox();
      this.MsgList = new System.Windows.Forms.ListBox();
      this.ChatBox = new System.Windows.Forms.TextBox();
      this.SayButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // ParticipantList
      // 
      this.ParticipantList.FormattingEnabled = true;
      this.ParticipantList.IntegralHeight = false;
      this.ParticipantList.Location = new System.Drawing.Point(197, 12);
      this.ParticipantList.Name = "ParticipantList";
      this.ParticipantList.Size = new System.Drawing.Size(75, 183);
      this.ParticipantList.TabIndex = 0;
      // 
      // MsgList
      // 
      this.MsgList.FormattingEnabled = true;
      this.MsgList.IntegralHeight = false;
      this.MsgList.Location = new System.Drawing.Point(12, 12);
      this.MsgList.Name = "MsgList";
      this.MsgList.Size = new System.Drawing.Size(179, 183);
      this.MsgList.TabIndex = 1;
      // 
      // ChatBox
      // 
      this.ChatBox.Location = new System.Drawing.Point(12, 201);
      this.ChatBox.Multiline = true;
      this.ChatBox.Name = "ChatBox";
      this.ChatBox.Size = new System.Drawing.Size(179, 49);
      this.ChatBox.TabIndex = 2;
      // 
      // SayButton
      // 
      this.SayButton.Location = new System.Drawing.Point(197, 201);
      this.SayButton.Name = "SayButton";
      this.SayButton.Size = new System.Drawing.Size(75, 49);
      this.SayButton.TabIndex = 3;
      this.SayButton.Text = "Say";
      this.SayButton.UseVisualStyleBackColor = true;
      // 
      // ChatForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.SayButton);
      this.Controls.Add(this.ChatBox);
      this.Controls.Add(this.MsgList);
      this.Controls.Add(this.ParticipantList);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "ChatForm";
      this.Text = "ChatForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListBox ParticipantList;
    private System.Windows.Forms.ListBox MsgList;
    private System.Windows.Forms.TextBox ChatBox;
    private System.Windows.Forms.Button SayButton;
  }
}