using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalSigner
{
    public partial class OpenCertificateForm : Form
    {
        public string FileName { get { return this.textBox_filename.Text; } }

        public string Password { get { return this.textBox_password.Text; } }

        public OpenCertificateForm()
        {
            InitializeComponent();
        }

        private void button_open_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_browse_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog_certificate.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            this.textBox_filename.Text = this.openFileDialog_certificate.FileName;
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
