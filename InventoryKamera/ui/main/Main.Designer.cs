namespace InventoryKamera.ui.main
{
    partial class Main
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.scanLabel = new System.Windows.Forms.Label();
            this.customCharNamesLabel = new System.Windows.Forms.Label();
            this.travelersName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // scanLabel
            // 
            this.scanLabel.AutoSize = true;
            this.scanLabel.Font = new System.Drawing.Font("Corbel", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scanLabel.Location = new System.Drawing.Point(740, 0);
            this.scanLabel.Name = "scanLabel";
            this.scanLabel.Size = new System.Drawing.Size(260, 39);
            this.scanLabel.TabIndex = 0;
            this.scanLabel.Text = "Pick items to scan:";
            // 
            // customCharNamesLabel
            // 
            this.customCharNamesLabel.AutoSize = true;
            this.customCharNamesLabel.Font = new System.Drawing.Font("Corbel", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.customCharNamesLabel.Location = new System.Drawing.Point(463, 0);
            this.customCharNamesLabel.Name = "customCharNamesLabel";
            this.customCharNamesLabel.Size = new System.Drawing.Size(265, 39);
            this.customCharNamesLabel.TabIndex = 1;
            this.customCharNamesLabel.Text = "Set correct names:";
            // 
            // travelersName
            // 
            this.travelersName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.travelersName.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.travelersName.Location = new System.Drawing.Point(470, 43);
            this.travelersName.Name = "travelersName";
            this.travelersName.Size = new System.Drawing.Size(257, 29);
            this.travelersName.TabIndex = 2;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.travelersName);
            this.Controls.Add(this.customCharNamesLabel);
            this.Controls.Add(this.scanLabel);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.MaximumSize = new System.Drawing.Size(1000, 500);
            this.MinimumSize = new System.Drawing.Size(1000, 500);
            this.Name = "Main";
            this.Size = new System.Drawing.Size(1000, 500);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label scanLabel;
        private System.Windows.Forms.Label customCharNamesLabel;
        private System.Windows.Forms.TextBox travelersName;
    }
}
