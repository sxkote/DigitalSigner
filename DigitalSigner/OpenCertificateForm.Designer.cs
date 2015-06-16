namespace DigitalSigner
{
    partial class OpenCertificateForm
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
            this.textBox_filename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button_open = new System.Windows.Forms.Button();
            this.openFileDialog_certificate = new System.Windows.Forms.OpenFileDialog();
            this.button_cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_filename
            // 
            this.textBox_filename.Location = new System.Drawing.Point(91, 17);
            this.textBox_filename.Name = "textBox_filename";
            this.textBox_filename.ReadOnly = true;
            this.textBox_filename.Size = new System.Drawing.Size(262, 22);
            this.textBox_filename.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "File:";
            // 
            // textBox_password
            // 
            this.textBox_password.Location = new System.Drawing.Point(91, 45);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(262, 22);
            this.textBox_password.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(373, 15);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 27);
            this.button1.TabIndex = 4;
            this.button1.Text = "Browse...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button_browse_Click);
            // 
            // button_open
            // 
            this.button_open.Location = new System.Drawing.Point(237, 73);
            this.button_open.Name = "button_open";
            this.button_open.Size = new System.Drawing.Size(116, 30);
            this.button_open.TabIndex = 5;
            this.button_open.Text = "Open";
            this.button_open.UseVisualStyleBackColor = true;
            this.button_open.Click += new System.EventHandler(this.button_open_Click);
            // 
            // openFileDialog_certificate
            // 
            this.openFileDialog_certificate.Filter = "CER|*.cer|P7B|*.p7b|PFX|*.pfx|All Files|*.*";
            // 
            // button_cancel
            // 
            this.button_cancel.Location = new System.Drawing.Point(91, 73);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(104, 30);
            this.button_cancel.TabIndex = 6;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // OpenCertificateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 108);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_open);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_filename);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "OpenCertificateForm";
            this.Text = "OpenCertificateForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_filename;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button_open;
        private System.Windows.Forms.OpenFileDialog openFileDialog_certificate;
        private System.Windows.Forms.Button button_cancel;
    }
}