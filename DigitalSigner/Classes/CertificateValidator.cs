using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSigner.Classes
{
    public interface ICertificateValidator
    {
        CertificateValidationResult Validate(CertificateInfo certificate);
    }

    public class CertificateValidator : ICertificateValidator
    {
        private CertificateInfo _certificate = null;

        public CertificateValidator(CertificateInfo certificate)
        {
            _certificate = certificate;

            if (_certificate == null || _certificate.Certificate == null)
                throw new ArgumentNullException("certificate");
        }

        public CertificateValidationResult Validate(CertificateInfo certificate)
        {
            var result = new CertificateValidationResult();

            if (certificate == null || certificate.Certificate == null) 
                return result;

            result.Add("Отпечаток сертификата", certificate.Thumbprint);
            result.Add("Серийный номер сертификата", certificate.SerialNumber);
            result.AddNewLine();

            #region //проверка валидности и отозванности с использованием базовой политики проверки
            if (certificate.Certificate.Verify())
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
            ch.Build(certificate.Certificate);

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
                result.Add(verify ? CertificateValidationMessage.MessageType.Info : CertificateValidationMessage.MessageType.Error, "Валиден", verify.ToString());
                result.AddNewLine();
            }

            result.Add(valid_result ? CertificateValidationMessage.MessageType.Info : CertificateValidationMessage.MessageType.Error, "Результат проверки цепочки", valid_result ? "Сертификат прошел проверку" : "Сертификат НЕ прошел проверку");
            result.AddNewLine();
            #endregion

            return result;
        }

        /// <summary>
        /// Проверка срока действия сертификата
        /// </summary>
        /// <returns>Ошибку либо информационное сообщение о валидности дат</returns>
        protected CertificateValidationResult ValidateDates()
        {
            var result = new CertificateValidationResult();

            if (_certificate.Certificate.NotBefore < DateTime.Now && DateTime.Now < _certificate.Certificate.NotAfter)
                result.AddInfo("Срок действия", "действует");
            else
               result.AddError("Срок действия", "истек");

            return result;
        }

        /// <summary>
        /// Проверка сертификата на квалифицированность
        /// </summary>
        /// <returns>Список сообщений/ошибок о квалифицированности сертификата</returns>
        protected CertificateValidationResult ValidateQualified()
        {
            var result = new CertificateValidationResult();

            #region //проверка квалифицированности сертификата
            bool isQualified = true;

            result.AddNewLine();
            result.AddNewLine();
            result.Add("Проверка квалифицированного сертификатов:");
            result.AddNewLine();

            string common_name = GetCertificateProperty(_certificate, "CN");
            if (common_name == "")
            {
                result.AddError("Не задано наименование (CN) Субъекта");
                isQualified = false;
            }

            if (GetCertificateProperty(_certificate, "O") == "")
            {
                result.AddError("Не задана организация (O) Субъекта");
                isQualified = false;
            }

            if (GetCertificateProperty(_certificate, "L") == "")
            {
                result.AddError("Не задана расположение (L) Субъекта");
                isQualified = false;
            }

            if (GetCertificateProperty(_certificate, "E") == "")
            {
                result.AddError("Не задан e-mail (E) Субъекта");
                isQualified = false;
            }

            string inn = GetCertificateProperty(_certificate, "ИНН");
            if (inn == "")
                inn = GetCertificateProperty(_certificate, "1.2.643.3.131.1.1");
            if (inn.Trim().Length != 12)
            {
                result.AddError("ИНН Субъекта должен состоять из 12 знаков");
                isQualified = false;
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

            if (common_name.ToLower().Contains("ооо") || common_name.ToLower().Contains("зао") || common_name.ToLower().Contains("оао") || common_name.ToLower().Contains("пао") || common_name.ToLower().StartsWith("ип"))
                CN_org += 2;

            if (CN_fio > CN_org && GetCertificateProperty(_certificate, "СНИЛС").Trim().Length == 0 && GetCertificateProperty(_certificate, "1.2.643.100.3").Trim().Length == 0)
            {
                result.AddError("Не задан СНИЛС Субъекта");
                isQualified = false;
            }

            if (GetCertificateProperty(_certificate, "ОГРН").Trim().Length == 0 && GetCertificateProperty(_certificate, "1.2.643.100.1").Trim().Length == 0)
            {
                result.AddError("Не задан ОГРН Субъекта");
                isQualified = false;
            }

            result.Add(isQualified ? CertificateValidationMessage.MessageType.Info : CertificateValidationMessage.MessageType.Error, "Результат проверки квалифицированного сертификата", isQualified ? "Сертификат является Квалифицированным" : "Сертификат НЕ является Квалифицированным");
            #endregion

            return result;
        }

        static private string GetCertificateProperty(CertificateInfo certificate, string pattern)
        {
            if (certificate == null || certificate.Certificate == null)
                return "";

            string result = "";
            string Name = certificate.Certificate.SubjectName.Name;

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
    }
}
