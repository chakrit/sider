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
      System.Windows.Forms.Label hostLabel;
      System.Windows.Forms.Label portLabel;
      System.Windows.Forms.GroupBox connectionGroup;
      System.Windows.Forms.GroupBox commandsGroup;
      System.Windows.Forms.Label controlLabel;
      System.Windows.Forms.Label label1;
      System.Windows.Forms.Label pingLabel;
      this.hostBox = new System.Windows.Forms.TextBox();
      this.testButton = new System.Windows.Forms.Button();
      this.portBox = new System.Windows.Forms.NumericUpDown();
      this.shutdownButton = new System.Windows.Forms.Button();
      this.quitButton = new System.Windows.Forms.Button();
      this.bgRewriteAOFButton = new System.Windows.Forms.Button();
      this.bgSaveButton = new System.Windows.Forms.Button();
      this.saveButton = new System.Windows.Forms.Button();
      this.pingResultLabel = new System.Windows.Forms.Label();
      this.pingButton = new System.Windows.Forms.Button();
      this.browseLabel = new System.Windows.Forms.Label();
      this.browseButton = new System.Windows.Forms.Button();
      hostLabel = new System.Windows.Forms.Label();
      portLabel = new System.Windows.Forms.Label();
      connectionGroup = new System.Windows.Forms.GroupBox();
      commandsGroup = new System.Windows.Forms.GroupBox();
      controlLabel = new System.Windows.Forms.Label();
      label1 = new System.Windows.Forms.Label();
      pingLabel = new System.Windows.Forms.Label();
      connectionGroup.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.portBox)).BeginInit();
      commandsGroup.SuspendLayout();
      this.SuspendLayout();
      // 
      // hostLabel
      // 
      hostLabel.AutoSize = true;
      hostLabel.Location = new System.Drawing.Point(47, 27);
      hostLabel.Name = "hostLabel";
      hostLabel.Size = new System.Drawing.Size(38, 17);
      hostLabel.TabIndex = 0;
      hostLabel.Text = "Host:";
      // 
      // portLabel
      // 
      portLabel.AutoSize = true;
      portLabel.Location = new System.Drawing.Point(50, 57);
      portLabel.Name = "portLabel";
      portLabel.Size = new System.Drawing.Size(35, 17);
      portLabel.TabIndex = 0;
      portLabel.Text = "Port:";
      // 
      // connectionGroup
      // 
      connectionGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      connectionGroup.AutoSize = true;
      connectionGroup.Controls.Add(this.hostBox);
      connectionGroup.Controls.Add(this.testButton);
      connectionGroup.Controls.Add(hostLabel);
      connectionGroup.Controls.Add(this.portBox);
      connectionGroup.Controls.Add(portLabel);
      connectionGroup.Location = new System.Drawing.Point(12, 12);
      connectionGroup.Name = "connectionGroup";
      connectionGroup.Size = new System.Drawing.Size(418, 104);
      connectionGroup.TabIndex = 4;
      connectionGroup.TabStop = false;
      connectionGroup.Text = " Connection ";
      // 
      // hostBox
      // 
      this.hostBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.hostBox.Location = new System.Drawing.Point(91, 24);
      this.hostBox.Name = "hostBox";
      this.hostBox.Size = new System.Drawing.Size(321, 25);
      this.hostBox.TabIndex = 1;
      this.hostBox.Text = "localhost";
      this.hostBox.TextChanged += new System.EventHandler(this.hostBox_TextChanged);
      // 
      // testButton
      // 
      this.testButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.testButton.Location = new System.Drawing.Point(253, 55);
      this.testButton.Name = "testButton";
      this.testButton.Size = new System.Drawing.Size(156, 25);
      this.testButton.TabIndex = 3;
      this.testButton.Text = "Test Connection";
      this.testButton.UseVisualStyleBackColor = true;
      this.testButton.Click += new System.EventHandler(this.testButton_Click);
      // 
      // portBox
      // 
      this.portBox.Location = new System.Drawing.Point(91, 55);
      this.portBox.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
      this.portBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.portBox.Name = "portBox";
      this.portBox.Size = new System.Drawing.Size(120, 25);
      this.portBox.TabIndex = 2;
      this.portBox.Value = new decimal(new int[] {
            6379,
            0,
            0,
            0});
      this.portBox.ValueChanged += new System.EventHandler(this.portBox_ValueChanged);
      // 
      // commandsGroup
      // 
      commandsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      commandsGroup.AutoSize = true;
      commandsGroup.Controls.Add(this.browseButton);
      commandsGroup.Controls.Add(this.browseLabel);
      commandsGroup.Controls.Add(this.shutdownButton);
      commandsGroup.Controls.Add(this.quitButton);
      commandsGroup.Controls.Add(controlLabel);
      commandsGroup.Controls.Add(this.bgRewriteAOFButton);
      commandsGroup.Controls.Add(this.bgSaveButton);
      commandsGroup.Controls.Add(this.saveButton);
      commandsGroup.Controls.Add(this.pingResultLabel);
      commandsGroup.Controls.Add(this.pingButton);
      commandsGroup.Controls.Add(label1);
      commandsGroup.Controls.Add(pingLabel);
      commandsGroup.Location = new System.Drawing.Point(12, 122);
      commandsGroup.Name = "commandsGroup";
      commandsGroup.Size = new System.Drawing.Size(418, 166);
      commandsGroup.TabIndex = 5;
      commandsGroup.TabStop = false;
      commandsGroup.Text = " Commands ";
      // 
      // shutdownButton
      // 
      this.shutdownButton.Location = new System.Drawing.Point(91, 117);
      this.shutdownButton.Name = "shutdownButton";
      this.shutdownButton.Size = new System.Drawing.Size(156, 25);
      this.shutdownButton.TabIndex = 9;
      this.shutdownButton.Text = "SHUTDOWN";
      this.shutdownButton.UseVisualStyleBackColor = true;
      this.shutdownButton.Click += new System.EventHandler(this.shutdownButton_Click);
      // 
      // quitButton
      // 
      this.quitButton.Location = new System.Drawing.Point(253, 117);
      this.quitButton.Name = "quitButton";
      this.quitButton.Size = new System.Drawing.Size(75, 25);
      this.quitButton.TabIndex = 8;
      this.quitButton.Text = "QUIT";
      this.quitButton.UseVisualStyleBackColor = true;
      this.quitButton.Click += new System.EventHandler(this.quitButton_Click);
      // 
      // controlLabel
      // 
      controlLabel.AutoSize = true;
      controlLabel.Location = new System.Drawing.Point(31, 121);
      controlLabel.Name = "controlLabel";
      controlLabel.Size = new System.Drawing.Size(54, 17);
      controlLabel.TabIndex = 7;
      controlLabel.Text = "Control:";
      // 
      // bgRewriteAOFButton
      // 
      this.bgRewriteAOFButton.Location = new System.Drawing.Point(253, 86);
      this.bgRewriteAOFButton.Name = "bgRewriteAOFButton";
      this.bgRewriteAOFButton.Size = new System.Drawing.Size(156, 25);
      this.bgRewriteAOFButton.TabIndex = 6;
      this.bgRewriteAOFButton.Text = "BGREWRITEAOF";
      this.bgRewriteAOFButton.UseVisualStyleBackColor = true;
      this.bgRewriteAOFButton.Click += new System.EventHandler(this.bgRewriteAOFButton_Click);
      // 
      // bgSaveButton
      // 
      this.bgSaveButton.Location = new System.Drawing.Point(91, 86);
      this.bgSaveButton.Name = "bgSaveButton";
      this.bgSaveButton.Size = new System.Drawing.Size(75, 25);
      this.bgSaveButton.TabIndex = 5;
      this.bgSaveButton.Text = "BGSAVE";
      this.bgSaveButton.UseVisualStyleBackColor = true;
      this.bgSaveButton.Click += new System.EventHandler(this.bgSaveButton_Click);
      // 
      // saveButton
      // 
      this.saveButton.Location = new System.Drawing.Point(172, 86);
      this.saveButton.Name = "saveButton";
      this.saveButton.Size = new System.Drawing.Size(75, 25);
      this.saveButton.TabIndex = 4;
      this.saveButton.Text = "SAVE";
      this.saveButton.UseVisualStyleBackColor = true;
      this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
      // 
      // pingResultLabel
      // 
      this.pingResultLabel.AutoSize = true;
      this.pingResultLabel.Location = new System.Drawing.Point(172, 28);
      this.pingResultLabel.Name = "pingResultLabel";
      this.pingResultLabel.Size = new System.Drawing.Size(53, 17);
      this.pingResultLabel.TabIndex = 3;
      this.pingResultLabel.Text = "+PONG";
      // 
      // pingButton
      // 
      this.pingButton.Location = new System.Drawing.Point(91, 24);
      this.pingButton.Name = "pingButton";
      this.pingButton.Size = new System.Drawing.Size(75, 25);
      this.pingButton.TabIndex = 2;
      this.pingButton.Text = "PING";
      this.pingButton.UseVisualStyleBackColor = true;
      this.pingButton.Click += new System.EventHandler(this.pingButton_Click);
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(9, 90);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(76, 17);
      label1.TabIndex = 0;
      label1.Text = "Persistence:";
      // 
      // pingLabel
      // 
      pingLabel.AutoSize = true;
      pingLabel.Location = new System.Drawing.Point(49, 28);
      pingLabel.Name = "pingLabel";
      pingLabel.Size = new System.Drawing.Size(36, 17);
      pingLabel.TabIndex = 0;
      pingLabel.Text = "Ping:";
      // 
      // browseLabel
      // 
      this.browseLabel.AutoSize = true;
      this.browseLabel.Location = new System.Drawing.Point(32, 59);
      this.browseLabel.Name = "browseLabel";
      this.browseLabel.Size = new System.Drawing.Size(53, 17);
      this.browseLabel.TabIndex = 10;
      this.browseLabel.Text = "Browse:";
      // 
      // browseButton
      // 
      this.browseButton.Location = new System.Drawing.Point(91, 55);
      this.browseButton.Name = "browseButton";
      this.browseButton.Size = new System.Drawing.Size(75, 25);
      this.browseButton.TabIndex = 6;
      this.browseButton.Text = "BROWSE";
      this.browseButton.UseVisualStyleBackColor = true;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(442, 305);
      this.Controls.Add(commandsGroup);
      this.Controls.Add(connectionGroup);
      this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.Name = "MainForm";
      this.Text = "Redis console";
      connectionGroup.ResumeLayout(false);
      connectionGroup.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.portBox)).EndInit();
      commandsGroup.ResumeLayout(false);
      commandsGroup.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox hostBox;
    private System.Windows.Forms.NumericUpDown portBox;
    private System.Windows.Forms.Button testButton;
    private System.Windows.Forms.Button pingButton;
    private System.Windows.Forms.Button saveButton;
    private System.Windows.Forms.Label pingResultLabel;
    private System.Windows.Forms.Button bgSaveButton;
    private System.Windows.Forms.Button bgRewriteAOFButton;
    private System.Windows.Forms.Button quitButton;
    private System.Windows.Forms.Button shutdownButton;
    private System.Windows.Forms.Button browseButton;
    private System.Windows.Forms.Label browseLabel;


  }
}

