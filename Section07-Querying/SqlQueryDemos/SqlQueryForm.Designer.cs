namespace JsonSqlQuery
{
    partial class SqlQueryForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlQueryForm));
			this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
			this.panel2 = new System.Windows.Forms.Panel();
			this.SqlTextBox = new ScintillaNET.Scintilla();
			this.JsonTextBox = new ScintillaNET.Scintilla();
			this.panel1 = new System.Windows.Forms.Panel();
			this.ResultsInfoLabel = new System.Windows.Forms.Label();
			this.toolStrip2 = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.ContainerToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.ExecuteToolStripButton = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.toolStrip2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = new System.Drawing.Point(0, 25);
			this.MainSplitContainer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MainSplitContainer.Name = "MainSplitContainer";
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.panel2);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.JsonTextBox);
			this.MainSplitContainer.Panel2.Controls.Add(this.panel1);
			this.MainSplitContainer.Size = new System.Drawing.Size(1008, 618);
			this.MainSplitContainer.SplitterDistance = 409;
			this.MainSplitContainer.TabIndex = 1;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.White;
			this.panel2.Controls.Add(this.SqlTextBox);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Padding = new System.Windows.Forms.Padding(4);
			this.panel2.Size = new System.Drawing.Size(409, 618);
			this.panel2.TabIndex = 4;
			// 
			// SqlTextBox
			// 
			this.SqlTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.SqlTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SqlTextBox.Location = new System.Drawing.Point(4, 4);
			this.SqlTextBox.Name = "SqlTextBox";
			this.SqlTextBox.Size = new System.Drawing.Size(401, 610);
			this.SqlTextBox.TabIndex = 0;
			// 
			// JsonTextBox
			// 
			this.JsonTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.JsonTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JsonTextBox.Location = new System.Drawing.Point(0, 25);
			this.JsonTextBox.Name = "JsonTextBox";
			this.JsonTextBox.ReadOnly = true;
			this.JsonTextBox.Size = new System.Drawing.Size(595, 593);
			this.JsonTextBox.TabIndex = 6;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.ResultsInfoLabel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(595, 25);
			this.panel1.TabIndex = 7;
			// 
			// ResultsInfoLabel
			// 
			this.ResultsInfoLabel.BackColor = System.Drawing.Color.White;
			this.ResultsInfoLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsInfoLabel.Location = new System.Drawing.Point(0, 0);
			this.ResultsInfoLabel.Name = "ResultsInfoLabel";
			this.ResultsInfoLabel.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
			this.ResultsInfoLabel.Size = new System.Drawing.Size(595, 25);
			this.ResultsInfoLabel.TabIndex = 0;
			this.ResultsInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStrip2
			// 
			this.toolStrip2.Font = new System.Drawing.Font("Roboto", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip2.ImageScalingSize = new System.Drawing.Size(32, 32);
			this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.ContainerToolStripComboBox,
            this.toolStripSeparator1,
            this.ExecuteToolStripButton});
			this.toolStrip2.Location = new System.Drawing.Point(0, 0);
			this.toolStrip2.Name = "toolStrip2";
			this.toolStrip2.Size = new System.Drawing.Size(1008, 25);
			this.toolStrip2.TabIndex = 3;
			this.toolStrip2.Text = "toolStrip2";
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(61, 22);
			this.toolStripLabel1.Text = "Container";
			// 
			// ContainerToolStripComboBox
			// 
			this.ContainerToolStripComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ContainerToolStripComboBox.Font = new System.Drawing.Font("Roboto", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ContainerToolStripComboBox.Name = "ContainerToolStripComboBox";
			this.ContainerToolStripComboBox.Size = new System.Drawing.Size(300, 25);
			this.ContainerToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.ContainerToolStripComboBox_SelectedIndexChanged);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// ExecuteToolStripButton
			// 
			this.ExecuteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.ExecuteToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("ExecuteToolStripButton.Image")));
			this.ExecuteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ExecuteToolStripButton.Name = "ExecuteToolStripButton";
			this.ExecuteToolStripButton.Size = new System.Drawing.Size(82, 22);
			this.ExecuteToolStripButton.Text = "Execute (F5)";
			this.ExecuteToolStripButton.Click += new System.EventHandler(this.ExecuteToolStripButton_Click);
			// 
			// SqlQueryForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Gray;
			this.ClientSize = new System.Drawing.Size(1008, 643);
			this.Controls.Add(this.MainSplitContainer);
			this.Controls.Add(this.toolStrip2);
			this.Font = new System.Drawing.Font("Roboto", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "SqlQueryForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Azure Cosmos DB - SQL Queries";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.toolStrip2.ResumeLayout(false);
			this.toolStrip2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.SplitContainer MainSplitContainer;
		private System.Windows.Forms.ToolStrip toolStrip2;
		private System.Windows.Forms.ToolStripButton ExecuteToolStripButton;
		private System.Windows.Forms.Panel panel2;
		private ScintillaNET.Scintilla SqlTextBox;
		private System.Windows.Forms.ToolStripComboBox ContainerToolStripComboBox;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private ScintillaNET.Scintilla JsonTextBox;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label ResultsInfoLabel;
	}
}