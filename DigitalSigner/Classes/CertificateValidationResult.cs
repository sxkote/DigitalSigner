using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSigner.Classes
{
    /// <summary>
    /// Collection of Certificate Validation Messages (errors or info messages).
    /// </summary>
    public class CertificateValidationResult : List<CertificateValidationMessage>
    {
        public CertificateValidationResult() : base() { }
        public CertificateValidationResult(IEnumerable<CertificateValidationMessage> collection) : base(collection) { }

        public void Add(CertificateValidationMessage.MessageType type, string message)
        {
            this.Add(new CertificateValidationMessage(type, message));
        }

        public void Add(CertificateValidationMessage.MessageType type, string name, string value)
        {
            this.Add(new CertificateValidationMessage(type, name, value));
        }

        public void Add(string message)
        {
            this.Add(new CertificateValidationMessage(CertificateValidationMessage.MessageType.None, message));
        }

        public void Add(string name, string value)
        {
            this.Add(new CertificateValidationMessage(CertificateValidationMessage.MessageType.None, name, value));
        }

        public void AddNewLine()
        {
            this.Add(CertificateValidationMessage.MessageType.None, "");
        }

        public void AddError(string error)
        {
            this.Add(CertificateValidationMessage.MessageType.Error, error);
        }

        public void AddError(string name, string error)
        {
            this.Add(CertificateValidationMessage.MessageType.Error, name, error);
        }

        public void AddInfo(string info)
        {
            this.Add(CertificateValidationMessage.MessageType.Info, info);
        }

        public void AddInfo(string name, string value)
        {
            this.Add(CertificateValidationMessage.MessageType.Info, name, value);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in this)
                sb.Append(item.ToString());

            return sb.ToString();
        }
    }
}
