namespace Solid.Integrations.PlayHQ.WebhookClient
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.configurationPathTextBox = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.browseButton = new System.Windows.Forms.Button();
            this.recentEventsGridView = new System.Windows.Forms.DataGridView();
            this.receivedDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.payloadDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.eventEntryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.clearButton = new System.Windows.Forms.Button();
            this.payloadTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.recentEventsGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventEntryBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // configurationPathTextBox
            // 
            this.configurationPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.configurationPathTextBox.Location = new System.Drawing.Point(12, 12);
            this.configurationPathTextBox.Name = "configurationPathTextBox";
            this.configurationPathTextBox.PlaceholderText = "Select configuration file";
            this.configurationPathTextBox.ReadOnly = true;
            this.configurationPathTextBox.Size = new System.Drawing.Size(463, 23);
            this.configurationPathTextBox.TabIndex = 1;
            // 
            // startButton
            // 
            this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.startButton.Location = new System.Drawing.Point(575, 12);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(88, 23);
            this.startButton.TabIndex = 6;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.stopButton.Location = new System.Drawing.Point(669, 12);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(88, 23);
            this.stopButton.TabIndex = 7;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 497);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(863, 22);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton.Location = new System.Drawing.Point(481, 12);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(88, 23);
            this.browseButton.TabIndex = 9;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // recentEventsGridView
            // 
            this.recentEventsGridView.AllowUserToAddRows = false;
            this.recentEventsGridView.AllowUserToDeleteRows = false;
            this.recentEventsGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.recentEventsGridView.AutoGenerateColumns = false;
            this.recentEventsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.recentEventsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.receivedDataGridViewTextBoxColumn,
            this.payloadDataGridViewTextBoxColumn});
            this.recentEventsGridView.DataSource = this.eventEntryBindingSource;
            this.recentEventsGridView.Location = new System.Drawing.Point(12, 41);
            this.recentEventsGridView.Name = "recentEventsGridView";
            this.recentEventsGridView.ReadOnly = true;
            this.recentEventsGridView.RowTemplate.Height = 25;
            this.recentEventsGridView.Size = new System.Drawing.Size(839, 271);
            this.recentEventsGridView.TabIndex = 10;
            this.recentEventsGridView.SelectionChanged += new System.EventHandler(this.recentEventsGridView_SelectionChanged);
            // 
            // receivedDataGridViewTextBoxColumn
            // 
            this.receivedDataGridViewTextBoxColumn.DataPropertyName = "Received";
            this.receivedDataGridViewTextBoxColumn.HeaderText = "Received";
            this.receivedDataGridViewTextBoxColumn.Name = "receivedDataGridViewTextBoxColumn";
            this.receivedDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // payloadDataGridViewTextBoxColumn
            // 
            this.payloadDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.payloadDataGridViewTextBoxColumn.DataPropertyName = "Payload";
            this.payloadDataGridViewTextBoxColumn.HeaderText = "Payload";
            this.payloadDataGridViewTextBoxColumn.Name = "payloadDataGridViewTextBoxColumn";
            this.payloadDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // eventEntryBindingSource
            // 
            this.eventEntryBindingSource.DataSource = typeof(Solid.Integrations.PlayHQ.WebhookClient.EventEntry);
            // 
            // clearButton
            // 
            this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.clearButton.Location = new System.Drawing.Point(763, 12);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(88, 23);
            this.clearButton.TabIndex = 11;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            // 
            // payloadTextBox
            // 
            this.payloadTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.payloadTextBox.Location = new System.Drawing.Point(12, 318);
            this.payloadTextBox.Multiline = true;
            this.payloadTextBox.Name = "payloadTextBox";
            this.payloadTextBox.PlaceholderText = "Select event above to view payload";
            this.payloadTextBox.ReadOnly = true;
            this.payloadTextBox.Size = new System.Drawing.Size(839, 176);
            this.payloadTextBox.TabIndex = 12;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(863, 519);
            this.Controls.Add(this.payloadTextBox);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.recentEventsGridView);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.configurationPathTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Solid Displays PlayHQ Connector";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.recentEventsGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventEntryBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private TextBox configurationPathTextBox;
        private Button startButton;
        private Button stopButton;
        private StatusStrip statusStrip1;
        private Button browseButton;
        private DataGridView recentEventsGridView;
        private Button clearButton;
        private TextBox payloadTextBox;
        private BindingSource eventEntryBindingSource;
        private DataGridViewTextBoxColumn receivedDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn payloadDataGridViewTextBoxColumn;
    }
}