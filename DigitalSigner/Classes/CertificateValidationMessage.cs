using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSigner.Classes
{
    /// <summary>
    /// Certificate Verification Message is used to hold the Error or Info Message about the Certificate.
    /// </summary>
    public struct CertificateValidationMessage
    {
        public enum MessageType { None, Info, Error };

        private string _message;
        private MessageType _type;

        public string Message { get { return _message; } }

        public MessageType Type { get { return _type; } }

        public CertificateValidationMessage(MessageType type, string message = "")
        {
            _type = type;
            _message = message;
        }

        public CertificateValidationMessage(MessageType type, string name, string value)
        {
            _type = type;
            _message = String.Format("{0}: {1}", name, value);
        }

        public override string ToString()
        {
            string prefix = "";
            switch (this.Type)
            {
                case MessageType.Info:
                    prefix = " + ";
                    break;
                case MessageType.Error:
                    prefix = " - ";
                    break;
                default: prefix = "";
                    break;
            }

            return prefix + this.Message + Environment.NewLine;
        }

        static public CertificateValidationMessage Info(string message)
        {
            return new CertificateValidationMessage(MessageType.Info, message);
        }

        static public CertificateValidationMessage Info(string name, string value)
        {
            return new CertificateValidationMessage(MessageType.Info, name, value);
        }

        static public CertificateValidationMessage Error(string message)
        {
            return new CertificateValidationMessage(MessageType.Error, message);
        }

        static public CertificateValidationMessage Error(string name, string value)
        {
            return new CertificateValidationMessage(MessageType.Error, name, value);
        }
    }

   
}
