#region Copyright © 2019-2020 Sergii Artemenko
// This file is part of the Xtate project. <http://xtate.net>
// Copyright © 2019-2020 Sergii Artemenko
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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Xtate.CustomAction
{
	public class StartAction : ICustomActionExecutor
	{
		private const    string             Source     = "src";
		private const    string             SourceExpr = "srcexpr";
		private const    string             IdLocation = "idlocation";
		private readonly ILocationAssigner? _idLocation;
		private readonly Uri?               _source;

		private readonly IExpressionEvaluator? _sourceExpression;

		public StartAction(XmlReader xmlReader, ICustomActionContext access)
		{
			if (xmlReader == null) throw new ArgumentNullException(nameof(xmlReader));
			if (access == null) throw new ArgumentNullException(nameof(access));

			var source = xmlReader.GetAttribute(Source);
			var sourceExpression = xmlReader.GetAttribute(SourceExpr);
			var idLocation = xmlReader.GetAttribute(IdLocation);

			if (source == null && sourceExpression == null)
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_At_least_one_source_must_be_specified);
			}

			if (source != null && sourceExpression != null)
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_src_and_srcexpr_attributes_should_not_be_assigned_in_Start_element);
			}

			if (source != null && !Uri.TryCreate(source, UriKind.RelativeOrAbsolute, out _source))
			{
				access.AddValidationError<StartAction>(Resources.ErrorMessage_source__has_invalid_URI_format);
			}

			if (sourceExpression != null)
			{
				_sourceExpression = access.RegisterValueExpression(sourceExpression);
			}

			if (idLocation != null)
			{
				_idLocation = access.RegisterLocationExpression(idLocation);
			}
		}

	#region Interface ICustomActionExecutor

		public async ValueTask Execute(IExecutionContext executionContext, CancellationToken token)
		{
			if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

			var host = GetHost(executionContext);
			var baseUri = GetBaseUri(executionContext);
			var source = await GetSource(executionContext, token).ConfigureAwait(false);

			if (source == null)
			{
				throw new ProcessorException(Resources.StartAction_Execute_Source_not_specified);
			}

			var sessionId = SessionId.New();
			await host.StartStateMachineAsync(sessionId, new StateMachineOrigin(source, baseUri), parameters: default, token).ConfigureAwait(false);

			if (_idLocation != null)
			{
				await _idLocation.Assign(executionContext, new DataModelValue(sessionId), token).ConfigureAwait(false);
			}
		}

	#endregion

		private static Uri? GetBaseUri(IExecutionContext executionContext)
		{
			var val = executionContext.DataModel[key: "_x", caseInsensitive: false]
									  .AsObjectOrEmpty()[key: "host", caseInsensitive: false]
									  .AsObjectOrEmpty()[key: "location", caseInsensitive: false]
									  .AsStringOrDefault();

			return val != null ? new Uri(val) : null;
		}

		private static IHost GetHost(IExecutionContext executionContext)
		{
			if (executionContext.RuntimeItems[typeof(IHost)] is IHost host)
			{
				return host;
			}

			throw new ProcessorException(Resources.Exception_Can_t_get_access_to_IHost_interface);
		}

		private async ValueTask<Uri?> GetSource(IExecutionContext executionContext, CancellationToken token)
		{
			if (_source != null)
			{
				return _source;
			}

			if (_sourceExpression != null)
			{
				var val = await _sourceExpression.Evaluate(executionContext, token).ConfigureAwait(false);

				return new Uri(val.AsString(), UriKind.RelativeOrAbsolute);
			}

			return null;
		}
	}
}