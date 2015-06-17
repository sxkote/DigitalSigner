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
        ValidationResult Validate(ICertificate certificate);
    }

    public class CertificateCommonValidator : ICertificateValidator
    {
        /// <summary>
        /// Проверка валидности и отозванности с использованием базовой политики проверки
        /// </summary>
        /// <returns>Список сообщений/ошибок о валидности и/или отозвонности сертификата</returns>
        public ValidationResult Validate(ICertificate certificate)
        {
            var result = new ValidationResult();

            if (certificate == null)
                return result;

            bool isValid = certificate.NotBefore < DateTime.Now && DateTime.Now < certificate.NotAfter;
            result.Add(isValid, "Срок действия", String.Format("{0} ({1} - {2})", isValid ? "действителен" : "истек", certificate.NotBefore.ToShortDateString(), certificate.NotAfter.ToShortDateString()));

            result.AddNewLine();

            bool isVerified = certificate.CertificateX509.Verify();
            result.Add(isVerified, "Базовая проверка", isVerified ? "сертификат действителен и не отозван (прошел стандартную проверку)" : "сертификат НЕ валиден (НЕ прошел стандартную проверку)");

            return result;
        }
    }

    public class CertificateChainValidator : ICertificateValidator
    {
        /// <summary>
        /// Проверка валидности и отозванности с использованием пользовательской политики проверки
        /// </summary>
        /// <returns>Список сообщений/ошибок о валидности и/или отозвонности сертификата</returns>
        public ValidationResult Validate(ICertificate certificate)
        {
            var result = new ValidationResult();

            if (certificate == null)
                return result;

            //создаем цепочку сертификата
            X509Chain ch = new X509Chain();
            //отозванность сертификата хотим получать онлайн
            ch.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            //хотим проверить всю цепочку сертификатов
            ch.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            //проверка валидности самая полная 
            ch.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
            //строим цепочку на основе сертификата
            ch.Build(certificate.CertificateX509);

            result.Add("Проверка цепочки сертификатов:");
            result.AddNewLine();

            bool isValid = true;
            foreach (X509ChainElement element in ch.ChainElements)
            {
                bool verify = element.Certificate.Verify();

                isValid = isValid && verify;

                result.Add("  Субъект", element.Certificate.Subject);
                result.Add("  Издатель", element.Certificate.Issuer);
                result.Add("  Отпечаток", element.Certificate.Thumbprint);
                result.Add("  Серийный номер", element.Certificate.SerialNumber);
                result.Add("  Срок действия", String.Format("c {0} по {1}", element.Certificate.NotBefore, element.Certificate.NotAfter));
                result.Add(verify, "  Валиден", verify.ToString());
                result.AddNewLine();
            }

            result.Add(isValid, "Результат проверки цепочки", isValid ? "Сертификат прошел проверку" : "Сертификат НЕ прошел проверку");

            return result;
        }
    }

    public class CertificateQualifiedValidator : ICertificateValidator
    {
        /// <summary>
        /// Проверка сертификата на квалифицированность
        /// </summary>
        /// <returns>Список сообщений/ошибок о квалифицированности сертификата</returns>
        public ValidationResult Validate(ICertificate certificate)
        {
            var result = new ValidationResult();

            if (certificate == null)
                return result;

            bool isQualified = true;

            result.Add("Проверка квалифицированного сертификатов:");
            result.AddNewLine();

            string subjectCommonName = certificate.SubjectCommonName;

            if (subjectCommonName == "")
            {
                result.AddError("  Не задано наименование (CN) Субъекта");
                isQualified = false;
            }

            if (certificate.Organization == "")
            {
                result.AddError("  Не задана организация (O) Субъекта");
                isQualified = false;
            }

            if (certificate.Locality == "")
            {
                result.AddError("  Не задана расположение (L) Субъекта");
                isQualified = false;
            }

            if (certificate.Email == "")
            {
                result.AddError("  Не задан e-mail (E) Субъекта");
                isQualified = false;
            }

            if (certificate.INN.Trim().Length != 12)
            {
                result.AddError("  ИНН Субъекта должен состоять из 12 знаков");
                isQualified = false;
            }

            if (String.IsNullOrEmpty(certificate.OGRN))
            {
                result.AddError("  Не задан ОГРН Субъекта");
                isQualified = false;
            }

            int CN_fio = 0;
            int CN_org = 0;

            string[] splits = subjectCommonName.Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (splits.Length == 3)
            {
                CN_fio += 3;

                if (splits[2].EndsWith("вич") || splits[2].EndsWith("вна"))
                    CN_fio += 1;
            }
            else CN_org += 2;

            if (subjectCommonName.Contains("\""))
                CN_org += 3;
            else CN_fio += 1;

            if (subjectCommonName.ToLower().Contains("ооо") || subjectCommonName.ToLower().Contains("зао") || subjectCommonName.ToLower().Contains("оао") || subjectCommonName.ToLower().Contains("пао") || subjectCommonName.ToLower().StartsWith("ип"))
                CN_org += 2;

            if (CN_fio > CN_org && String.IsNullOrEmpty(certificate.SNILS))
            {
                result.AddError("  Не задан СНИЛС Субъекта");
                isQualified = false;
            }

            result.Add(isQualified, "Результат проверки квалифицированного сертификата", isQualified ? "Сертификат является Квалифицированным" : "Сертификат НЕ является Квалифицированным");

            return result;
        }
    }

    public class CertificateValidator:ICertificateValidator
    {
        public ValidationResult Validate(ICertificate certificate)
        {
            var result = new ValidationResult();

            if (certificate == null)
                return result;

            result.Add("Отпечаток сертификата", certificate.Thumbprint);
            result.Add("Серийный номер сертификата", certificate.SerialNumber);

            result.AddNewLine(2);
            result.AddRange(new CertificateCommonValidator().Validate(certificate));

            result.AddNewLine(2);
            result.AddRange(new CertificateChainValidator().Validate(certificate));

            result.AddNewLine(2);
            result.AddRange(new CertificateQualifiedValidator().Validate(certificate));

            return result;
        }
    }
}
