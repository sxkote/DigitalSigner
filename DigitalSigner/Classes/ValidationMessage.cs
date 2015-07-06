using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSigner.Classes
{
    /// <summary>
    /// Сообщение или ошибка о проверке или результате работы
    /// </summary>
    public struct ValidationMessage
    {
        public enum MessageType { None, Info, Error };

        private string _message;
        private MessageType _type;

        public string Message { get { return _message; } }

        public MessageType Type { get { return _type; } }

        public ValidationMessage(MessageType type, string message = "")
        {
            _type = type;
            _message = message;
        }

        public ValidationMessage(MessageType type, string name, string value)
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

        static public ValidationMessage Info(string message)
        {
            return new ValidationMessage(MessageType.Info, message);
        }

        static public ValidationMessage Info(string name, string value)
        {
            return new ValidationMessage(MessageType.Info, name, value);
        }

        static public ValidationMessage Error(string message)
        {
            return new ValidationMessage(MessageType.Error, message);
        }

        static public ValidationMessage Error(string name, string value)
        {
            return new ValidationMessage(MessageType.Error, name, value);
        }
    }

   
}
