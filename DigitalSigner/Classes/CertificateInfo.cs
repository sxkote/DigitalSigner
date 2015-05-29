using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace DigitalSigner.Classes
{
    public sealed class CertificateInfo
    {
        private X509Certificate2 _certificate = null;

        public X509Certificate2 Certificate
        { get { return _certificate; } }

        public string Thumbprint
        { get { return _certificate == null ? "" : _certificate.Thumbprint; } }

        public string SerialNumber
        { get { return _certificate == null ? "" : _certificate.SerialNumber; } }

        public string Subject
        { get { return _certificate == null ? "" : _certificate.Subject; } }

        public string SubjectCN
        { get { return GetParam(this.Subject, "CN"); } }

        public string Issuer
        { get { return _certificate == null ? "" : _certificate.Issuer; } }

        public string IssuerCN
        { get { return GetParam(this.Issuer, "CN"); } }

        public CertificateInfo() { }

        public CertificateInfo(X509Certificate2 cert)
        { _certificate = cert; }

        public override string ToString()
        {
            if (this.Certificate == null)
                return "NULL";

            return this.SubjectCN + "  [" + this.IssuerCN + "]";
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
