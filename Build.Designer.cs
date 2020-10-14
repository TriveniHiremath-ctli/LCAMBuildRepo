namespace LCAMBuild
{
    partial class frmBuild
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lstBuildOutputs = new System.Windows.Forms.CheckedListBox();
            this.btnBuild = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.lblSelection = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.btnDeleteArchives = new System.Windows.Forms.Button();
            this.ddReleases = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(94, 34);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(250, 24);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "LCAMWEB Application Build";
            // 
            // lstBuildOutputs
            // 
            this.lstBuildOutputs.CheckOnClick = true;
            this.lstBuildOutputs.FormattingEnabled = true;
            this.lstBuildOutputs.Location = new System.Drawing.Point(98, 181);
            this.lstBuildOutputs.Name = "lstBuildOutputs";
            this.lstBuildOutputs.Size = new System.Drawing.Size(367, 289);
            this.lstBuildOutputs.TabIndex = 1;
            // 
            // btnBuild
            // 
            this.btnBuild.Location = new System.Drawing.Point(210, 487);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Size = new System.Drawing.Size(75, 23);
            this.btnBuild.TabIndex = 2;
            this.btnBuild.Text = "Build";
            this.btnBuild.UseVisualStyleBackColor = true;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(95, 516);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(35, 13);
            this.lblMessage.TabIndex = 3;
            this.lblMessage.Text = "label2";
            this.lblMessage.Visible = false;
            // 
            // lblSelection
            // 
            this.lblSelection.AutoSize = true;
            this.lblSelection.Location = new System.Drawing.Point(95, 165);
            this.lblSelection.Name = "lblSelection";
            this.lblSelection.Size = new System.Drawing.Size(157, 13);
            this.lblSelection.TabIndex = 4;
            this.lblSelection.Text = "Select the components to Build:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(95, 99);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Release Number";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(381, 165);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(84, 13);
            this.linkLabel1.TabIndex = 7;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Clear All Checks";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // btnDeleteArchives
            // 
            this.btnDeleteArchives.Location = new System.Drawing.Point(469, 88);
            this.btnDeleteArchives.Name = "btnDeleteArchives";
            this.btnDeleteArchives.Size = new System.Drawing.Size(142, 23);
            this.btnDeleteArchives.TabIndex = 8;
            this.btnDeleteArchives.Text = "Delete Archive Files";
            this.btnDeleteArchives.UseVisualStyleBackColor = true;
            this.btnDeleteArchives.Click += new System.EventHandler(this.btnDeleteArchives_Click);
            // 
            // ddReleases
            // 
            this.ddReleases.FormattingEnabled = true;
            this.ddReleases.Location = new System.Drawing.Point(98, 115);
            this.ddReleases.Name = "ddReleases";
            this.ddReleases.Size = new System.Drawing.Size(121, 21);
            this.ddReleases.TabIndex = 9;
            // 
            // frmBuild
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(979, 693);
            this.Controls.Add(this.ddReleases);
            this.Controls.Add(this.btnDeleteArchives);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblSelection);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.btnBuild);
            this.Controls.Add(this.lstBuildOutputs);
            this.Controls.Add(this.lblTitle);
            this.Name = "frmBuild";
            this.Text = "LCAMWEB Build";
            this.Load += new System.EventHandler(this.frmBuild_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.CheckedListBox lstBuildOutputs;
        private System.Windows.Forms.Button btnBuild;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label lblSelection;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button btnDeleteArchives;
        private System.Windows.Forms.ComboBox ddReleases;
    }
}

