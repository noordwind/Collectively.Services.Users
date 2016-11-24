using System;
using System.Collections.Generic;
using System.Linq;

namespace Coolector.Services.Users.SendGrid
{
    public class SendGridEmailTemplateParameter
    {
        public string ReplacementTag { get; }
        public IEnumerable<string> Values { get; }

        public SendGridEmailTemplateParameter(string replacementTag, params string[] values)
        {
            if (string.IsNullOrWhiteSpace(replacementTag))
                throw new ArgumentException("Replacement tag can not be empty", nameof(replacementTag));
            if (values?.Any() == false)
                throw new ArgumentException("Replacement tag values can not be empty", nameof(replacementTag));

            ReplacementTag = replacementTag;
            Values = values;
        }

        public static SendGridEmailTemplateParameter Create(string replacementTag, params string[] values)
            => new SendGridEmailTemplateParameter(replacementTag, values);
    }
}