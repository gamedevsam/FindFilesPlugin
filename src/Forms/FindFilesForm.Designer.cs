namespace FindFilesPlugin
{
    partial class FindFilesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindFilesForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox = new System.Windows.Forms.TextBox();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.filenameHeader = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.filepathHeader = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.labelDirectoryInfo = new System.Windows.Forms.Label();
            this.labelSearchTime = new System.Windows.Forms.Label();
            this.btnReload = new System.Windows.Forms.Button();
            this.toolTipReload = new System.Windows.Forms.ToolTip(this.components);
            this.checkInProjectOnly = new System.Windows.Forms.CheckBox();
            this.toolTipInProjectOnly = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Search string:";
            // 
            // textBox
            // 
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox.Location = new System.Drawing.Point(12, 28);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(560, 22);
            this.textBox.TabIndex = 0;
            this.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown);
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToResizeRows = false;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView.BackgroundColor = System.Drawing.Color.OldLace;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.filenameHeader,
            this.filepathHeader});
            this.dataGridView.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.dataGridView.EnableHeadersVisualStyles = false;
            this.dataGridView.Location = new System.Drawing.Point(12, 56);
            this.dataGridView.MultiSelect = false;
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.ShowCellErrors = false;
            this.dataGridView.ShowCellToolTips = false;
            this.dataGridView.ShowEditingIcon = false;
            this.dataGridView.ShowRowErrors = false;
            this.dataGridView.Size = new System.Drawing.Size(560, 194);
            this.dataGridView.StandardTab = true;
            this.dataGridView.TabIndex = 3;
            this.dataGridView.TabStop = false;
            this.dataGridView.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_CellMouseDoubleClick);
            this.dataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView_KeyDown);
            // 
            // filenameHeader
            // 
            this.filenameHeader.FillWeight = 76.14215F;
            this.filenameHeader.HeaderText = "File Name";
            this.filenameHeader.MinimumWidth = 100;
            this.filenameHeader.Name = "filenameHeader";
            this.filenameHeader.ReadOnly = true;
            this.filenameHeader.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // filepathHeader
            // 
            this.filepathHeader.FillWeight = 123.8579F;
            this.filepathHeader.HeaderText = "File Path";
            this.filepathHeader.MinimumWidth = 100;
            this.filepathHeader.Name = "filepathHeader";
            this.filepathHeader.ReadOnly = true;
            this.filepathHeader.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // labelDirectoryInfo
            // 
            this.labelDirectoryInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDirectoryInfo.AutoSize = true;
            this.labelDirectoryInfo.Location = new System.Drawing.Point(9, 256);
            this.labelDirectoryInfo.Name = "labelDirectoryInfo";
            this.labelDirectoryInfo.Size = new System.Drawing.Size(70, 13);
            this.labelDirectoryInfo.TabIndex = 4;
            this.labelDirectoryInfo.Text = "Directory Info";
            // 
            // labelSearchTime
            // 
            this.labelSearchTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSearchTime.Location = new System.Drawing.Point(470, 256);
            this.labelSearchTime.Name = "labelSearchTime";
            this.labelSearchTime.Size = new System.Drawing.Size(122, 26);
            this.labelSearchTime.TabIndex = 6;
            this.labelSearchTime.Text = "Search Time";
            // 
            // btnReload
            // 
            this.btnReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReload.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnReload.BackgroundImage")));
            this.btnReload.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnReload.FlatAppearance.BorderSize = 0;
            this.btnReload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReload.Location = new System.Drawing.Point(547, 2);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(24, 24);
            this.btnReload.TabIndex = 7;
            this.btnReload.TabStop = false;
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // toolTipReload
            // 
            this.toolTipReload.AutoPopDelay = 15000;
            this.toolTipReload.InitialDelay = 250;
            this.toolTipReload.IsBalloon = true;
            this.toolTipReload.ReshowDelay = 100;
            this.toolTipReload.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTipReload.ToolTipTitle = "Reload Directories";
            // 
            // checkInProjectOnly
            // 
            this.checkInProjectOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkInProjectOnly.AutoSize = true;
            this.checkInProjectOnly.Location = new System.Drawing.Point(446, 8);
            this.checkInProjectOnly.Name = "checkInProjectOnly";
            this.checkInProjectOnly.Size = new System.Drawing.Size(95, 17);
            this.checkInProjectOnly.TabIndex = 8;
            this.checkInProjectOnly.Text = "In Project Only";
            this.checkInProjectOnly.UseVisualStyleBackColor = true;
            this.checkInProjectOnly.CheckedChanged += new System.EventHandler(this.checkInProjectOnly_CheckedChanged);
            // 
            // toolTipInProjectOnly
            // 
            this.toolTipInProjectOnly.AutoPopDelay = 15000;
            this.toolTipInProjectOnly.InitialDelay = 250;
            this.toolTipInProjectOnly.IsBalloon = true;
            this.toolTipInProjectOnly.ReshowDelay = 100;
            this.toolTipInProjectOnly.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTipInProjectOnly.ToolTipTitle = "In Project Only";
            // 
            // FindFilesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 275);
            this.Controls.Add(this.checkInProjectOnly);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.labelSearchTime);
            this.Controls.Add(this.labelDirectoryInfo);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.label1);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(350, 200);
            this.Name = "FindFilesForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Find Files";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpenResourceForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OpenResourceForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn filenameHeader;
        private System.Windows.Forms.DataGridViewTextBoxColumn filepathHeader;
        private System.Windows.Forms.Label labelDirectoryInfo;
        private System.Windows.Forms.Label labelSearchTime;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.ToolTip toolTipReload;
        private System.Windows.Forms.CheckBox checkInProjectOnly;
        private System.Windows.Forms.ToolTip toolTipInProjectOnly;
    }
}