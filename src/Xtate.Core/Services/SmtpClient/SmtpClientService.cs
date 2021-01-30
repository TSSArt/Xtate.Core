#region Copyright © 2019-2021 Sergii Artemenko

// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Xtate.Service
{
	public class SmtpClientService : ServiceBase
	{
		protected override async ValueTask<DataModelValue> Execute()
		{
			var parameters = Parameters.AsListOrEmpty();
			var host = parameters["server"].AsString();
			var port = parameters["port"].AsNumberOrDefault();
			using var client = new SmtpClient(host, port is { } p ? (int) p : 25);

			if (parameters["userName"].AsStringOrDefault() is { Length: >0 } userName)
			{
				var password = parameters["password"].AsStringOrDefault();
				client.Credentials = new NetworkCredential(userName, password);
			}

			if (parameters["deliveryFormat"].AsStringOrDefault() is { Length: > 0 } deliveryFormatString)
			{
				client.DeliveryFormat = (SmtpDeliveryFormat) Enum.Parse(typeof(SmtpDeliveryFormat), deliveryFormatString, parameters.CaseInsensitive);
			}

			client.EnableSsl = parameters["enableSsl"].AsBooleanOrDefault() ?? false;

			if (parameters["timeout"].AsStringOrDefault() is { Length: > 0 } timeoutString)
			{
				client.Timeout = int.Parse(timeoutString);
			}

			var textBody = parameters["body"].AsStringOrDefault();
			var htmlBody = parameters["htmlBody"].AsStringOrDefault();

			using var message = new MailMessage(parameters["from"].AsString(), parameters["to"].AsString())
								{
										Body = htmlBody ?? textBody,
										IsBodyHtml = htmlBody is not null,
										BodyEncoding = Encoding.UTF8,
										Subject = parameters["subject"].AsStringOrDefault() ?? string.Empty,
										SubjectEncoding = Encoding.UTF8
								};

			await client.SendMailAsync(message).ConfigureAwait(false);

			return default;
		}
	}
}