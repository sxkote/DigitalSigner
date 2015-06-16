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
        public SignerForm()
        {
            InitializeComponent();
        }

        #region Работа с Сертификатами
        protected void FillCertificatesFromStore()
        {
            //открываем хранилище сертификатов (личные сертификаты для текущего пользователя)
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            //открываем это хранилище для чтения
            store.Open(OpenFlags.ReadOnly);

            //фильтруем все доступные сертификаты для выпадающего списка
            var certificates = store.Certificates.Cast<X509Certificate2>()
                                    .Where(cert => cert.NotBefore < DateTime.Now && DateTime.Now < cert.NotAfter)
                                    .ToArray();

            this.FillCertificatesList(certificates);
        }

        protected void FillCertificatesFromFile(string filename, string password)
        {
            //читаем данные из файла
            byte[] data = File.ReadAllBytes(filename);

            //проверяем, что данные не пусты
            if (data == null || data.Length <= 0)
            {
                MessageBox.Show("Не верные данные сертификата!");
                return;
            }

            //создаем новый сертификат из данных файла
            X509Certificate2 cert = new X509Certificate2(data, password);

            this.FillCertificatesList(cert);
        }

        protected void FillCertificatesList(params X509Certificate2[] certificates)
        {
            //очищаем список сертификатов в выпадающем списке
            this.comboBox_certificate_list.Items.Clear();

            //добавляем каждый сертификат в выпадающий список
            foreach (var cert in certificates)
                this.comboBox_certificate_list.Items.Add(new CertificateInfo(cert));

            //если ксть хотябы 1 сертификат, выбираем его в выпадающем списке
            if (this.comboBox_certificate_list.Items.Count > 0)
                this.comboBox_certificate_list.SelectedIndex = 0;
        }

        public CertificateValidationResult CheckCertificate(X509Certificate2 certificate)
        {
            var result = new CertificateValidationResult();

            if (certificate == null) return result;


            result.Add("Отпечаток сертификата", certificate.Thumbprint);
            result.Add("Серийный номер сертификата", certificate.SerialNumber);
            result.AddNewLine();

            #region //проверка сроков действия сертификата
            if (certificate.NotBefore < DateTime.Now && DateTime.Now < certificate.NotAfter)
                result.AddInfo("Срок действия", "действует");
            else
                result.AddError("Срок действия", "истек");
            #endregion

            #region //проверка валидности и отозванности с использованием базовой политики проверки
            if (certificate.Verify())
                result.AddInfo("Базовая проверка", "сертификат действителен и не отозван (прошел стандартную проверку)");
            else
                result.AddError("Базовая проверка", "сертификат НЕ валиден (НЕ прошел стандартную проверку)");
            #endregion

            #region //проверка валидности и отозванности с использованием пользовательской политики проверки
            //создаем цепочку сертификата
            X509Chain ch = new X509Chain();
            //отозванность сертификата хотим получать онлайн
            ch.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            //хотим проверить всю цепочку сертификатов
            ch.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            //проверка валидности самая полная 
            ch.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
            //строим цепочку на основе сертификата
            ch.Build(certificate);

            result.AddNewLine();
            result.AddNewLine();
            result.Add("Проверка цепочки сертификатов:");
            result.AddNewLine();

            bool valid_result = true;
            foreach (X509ChainElement element in ch.ChainElements)
            {
                bool verify = element.Certificate.Verify();

                valid_result = valid_result && verify;

                result.Add("Субъект", element.Certificate.Subject);
                result.Add("Издатель", element.Certificate.Issuer);
                result.Add("Отпечаток", element.Certificate.Thumbprint);
                result.Add("Серийный номер", element.Certificate.SerialNumber);
                result.Add("Срок действия", String.Format("c {0} по {1}", element.Certificate.NotBefore, element.Certificate.NotAfter));
                result.Add(verify ? MessageType.Info : MessageType.Error, "Валиден", verify.ToString());
                result.AddNewLine();
            }

            result.Add(valid_result ? MessageType.Info : MessageType.Error, "Результат проверки цепочки", valid_result ? "Сертификат прошел проверку" : "Сертификат НЕ прошел проверку");
            result.AddNewLine();
            #endregion

            #region //проверка квалифицированности сертификата

            bool qual_result = true;

            result.AddNewLine();
            result.AddNewLine();
            result.Add("Проверка квалифицированного сертификатов:");
            result.AddNewLine();

            string common_name = GetCertificateProperty(certificate, "CN");
            if (common_name == "")
            {
                result.AddError("Не задано наименование (CN) Субъекта");
                qual_result = false;
            }

            if (GetCertificateProperty(certificate, "O") == "")
            {
                result.AddError("Не задана организация (O) Субъекта");
                qual_result = false;
            }

            if (GetCertificateProperty(certificate, "L") == "")
            {
                result.AddError("Не задана расположение (L) Субъекта");
                qual_result = false;
            }

            if (GetCertificateProperty(certificate, "E") == "")
            {
                result.AddError("Не задан e-mail (E) Субъекта");
                qual_result = false;
            }

            string inn = GetCertificateProperty(certificate, "ИНН");
            if (inn == "")
                inn = GetCertificateProperty(certificate, "1.2.643.3.131.1.1");
            if (inn.Trim().Length != 12)
            {
                result.AddError("ИНН Субъекта должен состоять из 12 знаков");
                qual_result = false;
            }

            int CN_fio = 0;
            int CN_org = 0;

            string[] splits = common_name.Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (splits.Length == 3)
            {
                CN_fio += 3;

                if (splits[2].EndsWith("вич") || splits[2].EndsWith("вна"))
                    CN_fio += 1;
            }
            else CN_org += 2;

            if (common_name.Contains("\""))
                CN_org += 3;
            else CN_fio += 1;

            if (common_name.ToLower().Contains("ооо") || common_name.ToLower().Contains("зао") || common_name.ToLower().Contains("оао") || common_name.ToLower().StartsWith("ип"))
                CN_org += 2;

            if (CN_fio > CN_org && GetCertificateProperty(certificate, "СНИЛС").Trim().Length == 0 && GetCertificateProperty(certificate, "1.2.643.100.3").Trim().Length == 0)
            {
                result.AddError("Не задан СНИЛС Субъекта");
                qual_result = false;
            }

            if (GetCertificateProperty(certificate, "ОГРН").Trim().Length == 0 && GetCertificateProperty(certificate, "1.2.643.100.1").Trim().Length == 0)
            {
                result.AddError("Не задан ОГРН Субъекта");
                qual_result = false;
            }

            result.Add(qual_result ? MessageType.Info : MessageType.Error, "Результат проверки квалифицированного сертификата", qual_result ? "Сертификат является Квалифицированным" : "Сертификат НЕ является Квалифицированным");
            #endregion

            return result;
        }

        private void comboBox_certificate_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            //определяем выбранный в выпадающем списке сертификат
            CertificateInfo cert = this.comboBox_certificate_list.SelectedItem as CertificateInfo;

            //перезагружаем информацию о сертификате в поля формы
            if (cert != null)
            {
                this.textBox_cert_subject.Text = cert.Subject;
                this.textBox_cert_serial_number.Text = cert.SerialNumber;
                this.textBox_check_result.Text = this.CheckCertificate(cert.Certificate).ToString();
            }
        }

        private void button_certificate_show_Click(object sender, EventArgs e)
        {
            //определяем выбранный в выпадающем списке сертификат
            CertificateInfo cert = this.comboBox_certificate_list.SelectedItem as CertificateInfo;

            //показываем стандартное окно сертификата
            if (cert != null && cert.Certificate != null)
                X509Certificate2UI.DisplayCertificate(cert.Certificate);
        }

        private void button_certificate_check_Click(object sender, EventArgs e)
        {
            this.textBox_check_result.Text = "";

            //определяем выбранный в выпадающем списке сертификат
            CertificateInfo cert = this.comboBox_certificate_list.SelectedItem as CertificateInfo;

            //выводим информацию о проверке самого сертификата
            if (cert != null)
                this.textBox_check_result.Text = this.CheckCertificate(cert.Certificate) + Environment.NewLine;
        }

        private void button_certificate_from_store_Click(object sender, EventArgs e)
        {
            //перезагружаем список доступных сертификатов из личного хранилища
            this.FillCertificatesFromStore();
        }

        private void button_certificate_from_file_Click(object sender, EventArgs e)
        {
            var openCertForm = new OpenCertificateForm();
            if (openCertForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            //перезагружаем список доступных сертификатов из личного хранилища
            this.FillCertificatesFromFile(openCertForm.FileName, openCertForm.Password);
        }
        #endregion

        #region Формирование Подписи
        private void button_browse_to_sign_Click(object sender, EventArgs e)
        {
            //просто выбираем файл, который будем подписывать
            if (this.openFileDialog_file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                this.textBox_sign_file.Text = this.openFileDialog_file.FileName;

            this.openFileDialog_file.FileName = "";
        }

        private void button_sign_Click(object sender, EventArgs e)
        {
            //определяем выбранный в выпадающем списке сертификат
            CertificateInfo cert = this.comboBox_certificate_list.SelectedItem as CertificateInfo;

            //если по какой-то причине сертификат не выбран
            if (cert == null || cert.Certificate == null)
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

            //создаем контейнер с данными, которые будут подписываться
            ContentInfo content = new ContentInfo(data_to_sign);

            //создаем пакет, в который помещаем контейнер с данными и параметры подписи
            //это основной объект, в рамках которого формируются проверки и преобразования подписи
            SignedCms cms = new SignedCms(content, detached);

            //создаем подписанта (объект на основе сертификата, который будет подписывать)
            CmsSigner signer = new CmsSigner(cert.Certificate);

            //с помощью подписанта подписываем пакет так,
            //что теперь в пакете находятся не сами данные,
            //а именно подписанные данные, то есть:
            //  - сама подпись в случае отсоединенной подписи
            //  - подпись с оригинальными данными в случае присоединенной подписи
            cms.ComputeSignature(signer, false);

            // сохраняем подписанный пакет
            byte[] signed_data = cms.Encode();


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

            StringBuilder sb = new StringBuilder();

            //проверяем, что указан файл с подписью
            if (this.textBox_check_sign.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо выбрать файл, содержащий подпись");
                return;
            }

            //читаем файл с подписью
            byte[] sign_data = File.ReadAllBytes(this.textBox_check_sign.Text);
            if (sign_data == null)
            {
                MessageBox.Show("Выбран не корректный файл с подписью");
                return;
            }

            //определяем параметр подписания:
            // true - значит подпись отделенная (в файле с подписью есть только подпись и содержимое нужно указывать отдельно)
            // false - значит подпись будет присоединенная (в файле с подписью будет и сама подпись и сами данные, которые подписывались)
            bool detached = this.checkBox_check_detached.Checked;


            //Создаем пакет с подписью для проверки самой подписи
            SignedCms cms = null;

            if (detached)
            {//отсоединенная подпись

                //проверяем, что указан файл с оригинальными данными
                if (this.textBox_check_file.Text.Trim() == "")
                {
                    MessageBox.Show("Необходимо выбрать файл, содержащий оригинальные данные, которые были подписаны");
                    return;
                }

                //читаем файл с подписью
                byte[] original_data = File.ReadAllBytes(this.textBox_check_file.Text);
                if (original_data == null)
                {
                    MessageBox.Show("Выбран не корректный файл с оригинальными данными");
                    return;
                }

                //создаем контейнер с оригинальными данными, подпись которых будет проверяться
                ContentInfo content = new ContentInfo(original_data);

                //формируем пакет с оригинальными данными и параметрами подписи
                cms = new SignedCms(content, detached);
            }
            else
            {// присоединенная подпись 

                //формируем пустой пакет с данными
                //так как в случае присоединенной подписи 
                //данные содержатся в самом подписанном файле
                cms = new SignedCms();
            }


            try
            {
                //декодируем файл, содержащий подпись
                //если вылетает ошибка - значит подпись не верна!!!
                cms.Decode(sign_data);

                //возможно, информация о подписаниях отсутствует
                if (cms.SignerInfos.Count <= 0)
                {
                    sb.AppendLine("Нет информации о подписях (возможно файл не подписан)" + Environment.NewLine);
                    this.textBox_check_result.Text += "Нет информации о подписях (возможно файл не подписан)" + Environment.NewLine;
                    return;
                }

                sb.AppendLine("Электронная Подпись вернa.");
                sb.AppendLine("Файл '" + this.textBox_check_file.Text + "' подписан следующим сертификатом:" + Environment.NewLine);

                this.textBox_check_result.Text += "Электронная Подпись вернa." + Environment.NewLine;
                this.textBox_check_result.Text += "Файл '" + this.textBox_check_file.Text + "' подписан следующими сертификатами:" + Environment.NewLine + Environment.NewLine;

                //отображаем информацию о подписаниях документа и загружаем сертификаты в список
                this.comboBox_certificate_list.Items.Clear();

                foreach (SignerInfo si in cms.SignerInfos)
                {
                    this.comboBox_certificate_list.Items.Add(new CertificateInfo(si.Certificate));
                    this.textBox_check_result.Text += this.CheckCertificate(si.Certificate) + Environment.NewLine + Environment.NewLine;

                    sb.AppendLine("Сведения о сертификате:" + Environment.NewLine);

                    //дергаем время подписания документа текущей подписью 
                    DateTime sign_time = DateTime.Now;
                    for (int i = 0; i < si.SignedAttributes.Count; i++)
                    {
                        if (si.SignedAttributes[i].Oid.Value == "1.2.840.113549.1.9.5") // Oid время подписания
                        {
                            Pkcs9SigningTime pkcs9_time = new Pkcs9SigningTime(si.SignedAttributes[i].Values[0].RawData);
                            sign_time = pkcs9_time.SigningTime;
                            break;
                        }
                    }
                    sb.AppendLine("Дата и Время подписания:  " + sign_time.ToString());

                    sb.AppendLine(this.CheckCertificate(si.Certificate) + Environment.NewLine + Environment.NewLine);
                }

                if (comboBox_certificate_list.Items.Count > 0)
                    this.comboBox_certificate_list.SelectedIndex = 0;

                //KReport report = KReport.Generate_ActSignDocument(sb.ToString());
                //report.ShowReport(true);

            }
            catch
            {
                this.textBox_check_result.Text += "Подпись не верна." + Environment.NewLine;
                return;
            }
        }

        /// <summary>
        /// Возвращает поле сертификата (CN=..., O=...)
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="pattern">CN или O</param>
        /// <returns></returns>
        public string GetCertificateProperty(X509Certificate2 certificate, string pattern)
        {
            string result = "";
            string Name = certificate.SubjectName.Name;

            pattern = pattern.ToLower();

            string[] parts = Name.Split(new char[1] { ',' });
            for (int i = 0; i < parts.Length; i++)
                if (parts[i].Trim().ToLower().StartsWith(pattern))
                {
                    result = parts[i].Replace(pattern + "=", "").Replace(pattern.ToUpper() + "=", "").Trim();
                    break;
                }

            if (result.StartsWith("\"") && result.EndsWith("\""))
                result = result.Substring(1, result.Length - 2);

            result = result.Replace("\"\"", "\"");

            return result.Trim();
        }
        #endregion


    }
}
