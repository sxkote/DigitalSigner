using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSigner.Classes
{
    /// <summary>
    /// Список сообщений/ошибок о проверки или результате работы
    /// </summary>
    public class ValidationResult : List<ValidationMessage>
    {
        public ValidationResult() : base() { }
        public ValidationResult(IEnumerable<ValidationMessage> collection) : base(collection) { }

        public bool Correct
        { get { return this.Count <= 0 || !this.Any(m => m.Type == ValidationMessage.MessageType.Error); } }

        public void Add(ValidationMessage.MessageType type, string message)
        {
            this.Add(new ValidationMessage(type, message));
        }

        public void Add(ValidationMessage.MessageType type, string name, string value)
        {
            this.Add(new ValidationMessage(type, name, value));
        }

        public void Add(string message)
        {
            this.Add(new ValidationMessage(ValidationMessage.MessageType.None, message));
        }

        public void Add(string name, string value)
        {
            this.Add(new ValidationMessage(ValidationMessage.MessageType.None, name, value));
        }

        public void AddNewLine(int count = 1)
        {
            for (int i = 0; i < count; i++)
                this.Add(ValidationMessage.MessageType.None, "");
        }

        public void AddError(string error)
        {
            this.Add(ValidationMessage.Error(error));
        }

        public void AddError(string name, string error)
        {
            this.Add(ValidationMessage.Error(name, error));
        }

        public void AddInfo(string info)
        {
            this.Add(ValidationMessage.Info(info));
        }

        public void AddInfo(string name, string value)
        {
            this.Add(ValidationMessage.Info(name, value));
        }

        public void Add(bool correct, string message)
        {
            if (correct)
                this.AddInfo(message);
            else
                this.AddError(message);
        }

        public void Add(bool correct, string name, string value)
        {
            if (correct)
                this.AddInfo(name, value);
            else
                this.AddError(name, value);
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
