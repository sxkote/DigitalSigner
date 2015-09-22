using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace DigitalSigner.Classes
{
    /// <summary>
    /// Certificate Interface
    /// </summary>
    public interface ICertificate
    {
        X509Certificate2 CertificateX509 { get; }

        string Thumbprint { get; }
        string SerialNumber { get; }

        DateTime NotBefore { get; }
        DateTime NotAfter { get; }

        string Subject { get; }
        string SubjectCommonName { get; }

        string Organization { get; }
        string Locality { get; }
        string Email { get; }
        string INN { get; }
        string OGRN { get; }
        string SNILS { get; }

        string Issuer { get; }
        string IssuerCommonName { get; }
    }

    public sealed class Certificate : ICertificate
    {
        private X509Certificate2 _certificate = null;

        public X509Certificate2 CertificateX509
        { get { return _certificate; } }

        public string Thumbprint
        { get { return _certificate.Thumbprint; } }

        public string SerialNumber
        { get { return _certificate.SerialNumber; } }

        public DateTime NotBefore
        { get { return _certificate.NotBefore; } }

        public DateTime NotAfter
        { get { return _certificate.NotAfter; } }

        public string Subject
        { get { return _certificate.Subject; } }

        public string SubjectCommonName
        { get { return GetParam(this.Subject, "CN"); } }

        public string Organization
        { get { return this.GetProperty("O"); } }

        public string Locality
        { get { return this.GetProperty("L"); } }

        public string Email
        { get { return this.GetProperty("E"); } }

        public string INN
        {
            get
            {
                var result = this.GetProperty("ИНН");
                if (String.IsNullOrEmpty(result))
                    result = this.GetProperty("1.2.643.3.131.1.1");
                return result;
            }
        }

        public string OGRN
        {
            get
            {
                var result = this.GetProperty("ОГРН");
                if (String.IsNullOrEmpty(result))
                    result = this.GetProperty("1.2.643.100.1");
                return result;
            }
        }

        public string SNILS
        {
            get
            {
                var result = this.GetProperty("СНИЛС");
                if (String.IsNullOrEmpty(result))
                    result = this.GetProperty("1.2.643.100.3");
                return result;
            }
        }

        public string Issuer
        { get { return _certificate.Issuer; } }

        public string IssuerCommonName
        { get { return GetParam(this.Issuer, "CN"); } }

        public Certificate(X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            _certificate = certificate;
        }

        public override string ToString()
        {
            return this.SubjectCommonName + "  [" + this.IssuerCommonName + "]";
        }

        private string GetProperty(string pattern)
        {
            string result = "";
            string Name = _certificate.SubjectName.Name;

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

        static private string GetParam(string text, string param_name)
        {
            if (text == null || text == "")
                return "";

            var items = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (items == null || items.Length <= 0)
                return "";

            var item = items.FirstOrDefault(i => i.Trim().ToLower().StartsWith(param_name.Trim().ToLower()));
            if (item == null)
                return "";

            string[] args = item.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            return (args == null || args.Length < 2) ? "" : args[1].Trim();
        }
    }
}
