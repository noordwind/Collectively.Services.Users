using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coolector.Common.Extensions;
using Coolector.Services.Users.Services;

namespace Coolector.Services.Users.SendGrid
{
    public class SendGridEmailMessenger : IEmailMessenger
    {
        private readonly ISendGridClient _sendGridClient;

        public SendGridEmailMessenger(ISendGridClient sendGridClient)
        {
            _sendGridClient = sendGridClient;
        }

        public async Task SendPasswordResetAsync(string email, string token)
        {
            //TODO: Fetch template from database.
            var templateId = "4febd104-85b1-4b57-a07f-85805b4e4241";
            var templateParameters = new List<SendGridEmailTemplateParameter>
            {
                SendGridEmailTemplateParameter.Create("email", email),
                SendGridEmailTemplateParameter.Create("token", token)
            };
            var emailMessage = CreateMessage(email, "no-reply@coolector.tk", "Reset Coolector account password");
            ApplyTemplate(emailMessage, templateId, templateParameters);
            await _sendGridClient.SendMessageAsync(emailMessage);
        }

        private SendGridEmailMessage CreateMessage(string receiver, string sender,
            string subject, string message = null)
        {
            var emailMessage = new SendGridEmailMessage
            {
                From = new SendGridEmailMessage.Person
                {
                    Email = sender
                },
                Subject = subject,
                Personalizations = new List<SendGridEmailMessage.Personalization>()
            };
            emailMessage.Personalizations.Add(new SendGridEmailMessage.Personalization
            {
                To = new List<SendGridEmailMessage.Person>
                {
                    new SendGridEmailMessage.Person
                    {
                        Email = receiver
                    }
                },
                Substitutions = new Dictionary<string, List<string>>()
            });
            if (message.NotEmpty())
            {
                emailMessage.Content = new List<SendGridEmailMessage.MessageContent>()
                {
                    new SendGridEmailMessage.MessageContent
                    {
                        Value = message
                    }
                };
            }

            return emailMessage;
        }

        private void ApplyTemplate(SendGridEmailMessage emailMessage, string templateId,
            IEnumerable<SendGridEmailTemplateParameter> templateParameters)
        {
            emailMessage.Content = null;
            emailMessage.TemplateId = templateId;
            var personalization = emailMessage.Personalizations.First();
            foreach (var parameter in templateParameters)
            {
                personalization.Substitutions[parameter.ReplacementTag] = parameter.Values.ToList();
            }
        }
    }
}