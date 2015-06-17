using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DigitalSigner.Classes;

namespace DigitalSigner
{
    public partial class SignerForm : Form
    {
        protected ICertificateReader _reader = new CertificateReader();
        protected ICertificateValidator _validator = new CertificateValidator();

        public SignerForm()
        {
            InitializeComponent();
        }

        #region Вспомогательные функции
        private ICertificate SelectedCertificate
        { get { return this.comboBox_certificate_list.SelectedItem as ICertificate; } }

        private void FillCertificatesList(params ICertificate[] certificates)
        {
            //очищаем список сертификатов в выпадающем списке
            this.comboBox_certificate_list.Items.Clear();

            //добавляем каждый сертификат в выпадающий список
            foreach (var cert in certificates)
                this.comboBox_certificate_list.Items.Add(cert);

            //если ксть хотябы 1 сертификат, выбираем его в выпадающем списке
            if (this.comboBox_certificate_list.Items.Count > 0)
                this.comboBox_certificate_list.SelectedIndex = 0;
        }

        private void ValidateCertificate(ICertificate certificate)
        {
            //очищаем поле проверки сертификата
            this.textBox_check_result.Text = "";

            //выводим информацию о проверке самого сертификата
            if (certificate != null)
                this.textBox_check_result.Text = _validator.Validate(certificate).ToString();
        }
        #endregion

        #region Работа с Сертификатами
        private void comboBox_certificate_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            //определяем выбранный в выпадающем списке сертификат
            var certificate = this.SelectedCertificate;

            //перезагружаем информацию о сертификате в поля формы
            this.textBox_cert_subject.Text = certificate == null ? "" : certificate.Subject;
            this.textBox_cert_serial_number.Text = certificate == null ? "" : certificate.SerialNumber;

            //запускаем проверку сертификата
            this.ValidateCertificate(certificate);
        }

        private void button_certificate_show_Click(object sender, EventArgs e)
        {
            //определяем выбранный в выпадающем списке сертификат
            var certificate = this.SelectedCertificate;

            //показываем стандартное (windows) окно сертификата
            if (certificate != null && certificate.CertificateX509 != null)
                X509Certificate2UI.DisplayCertificate(certificate.CertificateX509);
        }

        private void button_certificate_check_Click(object sender, EventArgs e)
        {
            //определяем выбранный в выпадающем списке сертификат
            var certificate = this.SelectedCertificate;

            //запускаем проверку сертификата
            this.ValidateCertificate(certificate);
        }

        private void button_certificate_from_store_Click(object sender, EventArgs e)
        {
            //перезагружаем список доступных сертификатов из личного хранилища
            var certificates = _reader.GetCertificatesFromStore();

            this.FillCertificatesList(certificates.ToArray());
        }

        private void button_certificate_from_file_Click(object sender, EventArgs e)
        {
            var openCertForm = new OpenCertificateForm();
            if (openCertForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            if (!File.Exists(openCertForm.FileName))
            {
                MessageBox.Show("Указанный файл не существует");
                return;
            }

            byte[] data = File.ReadAllBytes(openCertForm.FileName);
            if (data == null || data.Length<=0)
            {
                MessageBox.Show("Указан пустой файл");
                return;
            }

            var cert = _reader.GetCertificateFromFile(File.ReadAllBytes(openCertForm.FileName), openCertForm.Password);
            if (cert == null)
            {
                MessageBox.Show("Невозможно открыть файл с сертификатом");
                return;
            }

            this.FillCertificatesList(cert);
        }
        #endregion

        #region Формирование Подписи
        private void button_browse_to_sign_Click(object sender, EventArgs e)
        {
            //просто выбираем файл, который будем подписывать
            if (this.openFileDialog_file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                this.textBox_sign_file.Text = this.openFileDialog_file.FileName;

            //очищаем имя файла в диалоговом окне (чтобы каждый раз выбирать новый файл)
            this.openFileDialog_file.FileName = "";
        }

        private void button_sign_Click(object sender, EventArgs e)
        {
            //определяем выбранный в выпадающем списке сертификат
            var certificate = this.SelectedCertificate;

            //если по какой-то причине сертификат не выбран
            if (certificate == null || certificate.CertificateX509 == null)
            {
                MessageBox.Show("Необходимо выбрать сертификат!");
                return;
            }

            //проверяем, что указан файл, который будем подписывать
            if (this.textBox_sign_file.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо выбрать файл для подписи");
                return;
            }

            //читаем файл, который будем подписывать
            byte[] data_to_sign = File.ReadAllBytes(this.textBox_sign_file.Text);
            if (data_to_sign == null)
            {
                MessageBox.Show("Выбран не корректный файл для подписи");
                return;
            }

            //определяем параметр подписания:
            // true - значит подпись будет отделенная (в файле с подписью будет только подпись)
            // false - значит подпись будет присоединенная (в файле с подписью будет и сама подпись и сами данные, которые подписывались)
            bool detached = this.checkBox_sign_detached.Checked;

            var signer = new CertificateSigner(data_to_sign, detached);
            
            // подписываем файл и сохраняем подпись
            byte[] signed_data = signer.Sign(certificate);

            //сохраняем файл с подпсиью
            if (this.saveFileDialog_sign.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(this.saveFileDialog_sign.FileName, signed_data);
                MessageBox.Show("Файл с подписью сохранен успешно!");
                return;
            }
        }
        #endregion

        #region Проверка Подписи
        private void checkBox_check_detached_CheckedChanged(object sender, EventArgs e)
        {
            this.label_check_file.Visible = this.checkBox_check_detached.Checked;
            this.textBox_check_file.Visible = this.checkBox_check_detached.Checked;
            this.button_browse_to_check_file.Visible = this.checkBox_check_detached.Checked;
        }

        private void button_browse_to_check_sign_Click(object sender, EventArgs e)
        {
            //выбираем файл содержащий подпись
            if (this.openFileDialog_sign.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                this.textBox_check_sign.Text = this.openFileDialog_sign.FileName;

            this.openFileDialog_sign.FileName = "";
        }

        private void button_browse_to_check_file_Click(object sender, EventArgs e)
        {
            //выбираем файл содержащий оригинальные данные 
            //только в случае отсоединенной подписи
            if (this.openFileDialog_file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                this.textBox_check_file.Text = this.openFileDialog_file.FileName;

            this.openFileDialog_file.FileName = "";
        }

        private void button_check_Click(object sender, EventArgs e)
        {
            this.textBox_check_result.Text = "";

            //проверяем, что указан файл с подписью
            if (this.textBox_check_sign.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо выбрать файл, содержащий подпись");
                return;
            }

            //читаем файл с подписью
            byte[] signature = File.ReadAllBytes(this.textBox_check_sign.Text);
            if (signature == null)
            {
                MessageBox.Show("Выбран не корректный файл с подписью");
                return;
            }

            //определяем параметр подписания:
            // true - значит подпись отделенная (в файле с подписью есть только подпись и содержимое нужно указывать отдельно)
            // false - значит подпись будет присоединенная (в файле с подписью будет и сама подпись и сами данные, которые подписывались)
            bool detached = this.checkBox_check_detached.Checked;

            byte[] original = null;

            if (detached)
            {//отсоединенная подпись

                //проверяем, что указан файл с оригинальными данными
                if (this.textBox_check_file.Text.Trim() == "")
                {
                    MessageBox.Show("Необходимо выбрать файл, содержащий оригинальные данные, которые были подписаны");
                    return;
                }

                //читаем файл с подписью
                original = File.ReadAllBytes(this.textBox_check_file.Text);
                if (original == null)
                {
                    MessageBox.Show("Выбран не корректный файл с оригинальными данными");
                    return;
                }
            }

            var signer = new CertificateSigner(signature, original);

            var verification = signer.Verify();

            if (signer.Certificate != null)
            {
                this.FillCertificatesList(signer.Certificate);

                verification.AddNewLine(2);
                verification.AddRange(_validator.Validate(signer.Certificate));
            }

            this.textBox_check_result.Text = verification.ToString();
        }
        #endregion


    }
}
