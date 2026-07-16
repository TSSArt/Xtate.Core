// Copyright © 2019-2026 Sergii Artemenko
// 
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

using Xtate.StateMachine;
using Xtate.StateMachine.Validator;
using Xtate.StateMachine.Validator.Services;

namespace Xtate.Test.UnitTests.StateMachine.Validator;

[TestClass]
public class StateMachineValidatorCoverageTest
{
	[TestMethod]
	public void AssignAndCancelReportMissingConflictingAndEmptyAttributes()
	{
		var (validator, errors) = CreateValidator();

		validator.ValidateAssign(Mock.Of<IAssign>());
		validator.ValidateAssign(
			Mock.Of<IAssign>(assign =>
								 assign.Location == Location() &&
								 assign.Expression == Value() &&
								 assign.InlineContent == InlineContent()));
		validator.ValidateCancel(Mock.Of<ICancel>());
		validator.ValidateCancel(Mock.Of<ICancel>(cancel => cancel.SendId == string.Empty && cancel.SendIdExpression == Value()));

		VerifyErrors(errors, count: 6);
	}

	[TestMethod]
	public void ContentDataAndParamReportInvalidCombinations()
	{
		var (validator, errors) = CreateValidator();
		var inline = InlineContent();
		var value = Value();
		var location = Location();

		validator.ValidateContent(Mock.Of<IContent>());
		validator.ValidateContent(Mock.Of<IContent>(content => content.Expression == value && content.Body == ContentBody()));
		validator.ValidateData(Mock.Of<IData>());
		validator.ValidateData(Mock.Of<IData>(data => data.Id == "data" && data.Expression == value && data.InlineContent == inline));
		validator.ValidateParam(Mock.Of<IParam>(param => param.Name == null! && param.Expression == value && param.Location == location));

		VerifyErrors(errors, count: 6);
	}

	[TestMethod]
	public void HistoryForEachAndIfReportStructuralErrors()
	{
		var (validator, errors) = CreateValidator();
		var condition = Condition();
		var elseIf = Mock.Of<IElseIf>(item => item.Condition == condition);
		var firstElse = Mock.Of<IElse>();
		var secondElse = Mock.Of<IElse>();

		validator.ValidateHistory(Mock.Of<IHistory>(history => history.Type == (HistoryType) 99));
		validator.ValidateForEach(Mock.Of<IForEach>());
		validator.ValidateIf(
			Mock.Of<IIf>(item =>
							 item.Condition == null! &&
							 item.Action == ImmutableArray.Create<IExecutableEntity>(firstElse, elseIf, secondElse)));

		VerifyErrors(errors, count: 7);
	}

	[TestMethod]
	public void InvokeReportsMissingTypeAndAllMutuallyExclusiveInputs()
	{
		var (validator, errors) = CreateValidator();
		var location = Location();
		var value = Value();
		var parameter = Mock.Of<IParam>(param => param.Name == "parameter");
		var conflicting = new Mock<IInvoke>();
		conflicting.SetupGet(invoke => invoke.Type).Returns(new FullUri("urn:test:type"));
		conflicting.SetupGet(invoke => invoke.TypeExpression).Returns(value);
		conflicting.SetupGet(invoke => invoke.Id).Returns("invoke");
		conflicting.SetupGet(invoke => invoke.IdLocation).Returns(location);
		conflicting.SetupGet(invoke => invoke.Source).Returns(new Uri("https://example.test/source"));
		conflicting.SetupGet(invoke => invoke.SourceExpression).Returns(value);
		conflicting.SetupGet(invoke => invoke.NameList).Returns([location]);
		conflicting.SetupGet(invoke => invoke.Parameters).Returns([parameter]);

		validator.ValidateInvoke(Mock.Of<IInvoke>());
		validator.ValidateInvoke(conflicting.Object);

		VerifyErrors(errors, count: 5);
	}

	[TestMethod]
	public void SendReportsMissingEventAndConflictingAlternatives()
	{
		var (validator, errors) = CreateValidator();
		var value = Value();
		var location = Location();
		var content = Mock.Of<IContent>(item => item.Body == ContentBody());
		var parameter = Mock.Of<IParam>(param => param.Name == "parameter");
		var conflicting = new Mock<ISend>();
		conflicting.SetupGet(send => send.EventName).Returns("event");
		conflicting.SetupGet(send => send.EventExpression).Returns(value);
		conflicting.SetupGet(send => send.Content).Returns(content);
		conflicting.SetupGet(send => send.Target).Returns(new FullUri("urn:test:target"));
		conflicting.SetupGet(send => send.TargetExpression).Returns(value);
		conflicting.SetupGet(send => send.Type).Returns(new FullUri("urn:test:type"));
		conflicting.SetupGet(send => send.TypeExpression).Returns(value);
		conflicting.SetupGet(send => send.Id).Returns("send");
		conflicting.SetupGet(send => send.IdLocation).Returns(location);
		conflicting.SetupGet(send => send.DelayMs).Returns(1);
		conflicting.SetupGet(send => send.DelayExpression).Returns(value);
		conflicting.SetupGet(send => send.NameList).Returns([location]);
		conflicting.SetupGet(send => send.Parameters).Returns([parameter]);

		validator.ValidateSend(Mock.Of<ISend>());
		validator.ValidateSend(conflicting.Object);

		VerifyErrors(errors, count: 8);
	}

	[TestMethod]
	public void FinalizeScriptStateMachineStateAndTransitionReportInvalidStructures()
	{
		var (validator, errors) = CreateValidator();
		var raise = Mock.Of<IRaise>(item => item.OutgoingEvent == Mock.Of<IOutgoingEvent>());
		var send = Mock.Of<ISend>(item => item.EventName == "event");
		var transition = Mock.Of<ITransition>(item => item.Condition == Condition());
		var initial = Mock.Of<IInitial>(item => item.Transition == transition);
		var machine = Mock.Of<IStateMachine>(item =>
												 item.Initial == initial &&
												 item.Binding == (BindingType) 99);
		var state = Mock.Of<IState>(item => item.Initial == initial);

		validator.ValidateFinalize(Mock.Of<IFinalize>(item => item.Action == ImmutableArray.Create<IExecutableEntity>(raise, send)));
		validator.ValidateScript(Mock.Of<IScript>(item => item.Source == Mock.Of<IExternalScriptExpression>() && item.Content == Script()));
		validator.Validate(machine);
		validator.ValidateState(state);
		validator.ValidateTransition(Mock.Of<ITransition>());

		VerifyErrors(errors, count: 7);
	}

	[TestMethod]
	public void SimpleRequiredValuesReportNullAndAcceptPopulatedValues()
	{
		var (validator, errors) = CreateValidator();
		var condition = Condition();
		var transition = Mock.Of<ITransition>(item => item.Condition == condition);

		validator.ValidateCondition(Mock.Of<IConditionExpression>());
		validator.ValidateCondition(condition);
		validator.ValidateLocation(Mock.Of<ILocationExpression>());
		validator.ValidateLocation(Location());
		validator.ValidateScriptExpression(Mock.Of<IScriptExpression>());
		validator.ValidateScriptExpression(Script());
		validator.ValidateContentBody(Mock.Of<IContentBody>());
		validator.ValidateContentBody(ContentBody());
		validator.ValidateInlineContent(Mock.Of<IInlineContent>());
		validator.ValidateInlineContent(InlineContent());
		validator.ValidateCustomAction(Mock.Of<ICustomAction>());
		validator.ValidateCustomAction(Mock.Of<ICustomAction>(item => item.Xml == "<action />"));
		validator.ValidateElseIf(Mock.Of<IElseIf>());
		validator.ValidateElseIf(Mock.Of<IElseIf>(item => item.Condition == condition));
		validator.ValidateInitial(Mock.Of<IInitial>());
		validator.ValidateInitial(Mock.Of<IInitial>(item => item.Transition == transition));
		validator.ValidateRaise(Mock.Of<IRaise>());
		validator.ValidateRaise(Mock.Of<IRaise>(item => item.OutgoingEvent == Mock.Of<IOutgoingEvent>()));

		VerifyErrors(errors, count: 9);
	}

	private static (ExposedValidator Validator, Mock<IErrorProcessorService<StateMachineValidator>> Errors) CreateValidator()
	{
		var errors = new Mock<IErrorProcessorService<StateMachineValidator>>(MockBehavior.Strict);
		errors.Setup(service => service.AddError(It.IsAny<object?>(), It.IsAny<string>(), It.IsAny<Exception?>()));

		return (new ExposedValidator { ErrorProcessorService = errors.Object }, errors);
	}

	private static void VerifyErrors(Mock<IErrorProcessorService<StateMachineValidator>> errors, int count) =>
		errors.Verify(service => service.AddError(It.IsAny<object?>(), It.IsAny<string>(), It.IsAny<Exception?>()), Times.Exactly(count));

	private static IValueExpression Value() => Mock.Of<IValueExpression>(expression => expression.Expression == "value");

	private static IConditionExpression Condition() => Mock.Of<IConditionExpression>(expression => expression.Expression == "condition");

	private static IScriptExpression Script() => Mock.Of<IScriptExpression>(expression => expression.Expression == "script");

	private static ILocationExpression Location() => Mock.Of<ILocationExpression>(expression => expression.Expression == "location");

	private static IInlineContent InlineContent() => Mock.Of<IInlineContent>(content => content.Value == "inline");

	private static IContentBody ContentBody() => Mock.Of<IContentBody>(content => content.Value == "body");

	private sealed class ExposedValidator : StateMachineValidator
	{
		public void ValidateAssign(IAssign entity) => Visit(ref entity);

		public void ValidateCancel(ICancel entity) => Visit(ref entity);

		public void ValidateCondition(IConditionExpression entity) => Visit(ref entity);

		public void ValidateContent(IContent entity) => Visit(ref entity);

		public void ValidateContentBody(IContentBody entity) => Visit(ref entity);

		public void ValidateCustomAction(ICustomAction entity) => Visit(ref entity);

		public void ValidateData(IData entity) => Visit(ref entity);

		public void ValidateElseIf(IElseIf entity) => Visit(ref entity);

		public void ValidateFinalize(IFinalize entity) => Visit(ref entity);

		public void ValidateForEach(IForEach entity) => Visit(ref entity);

		public void ValidateHistory(IHistory entity) => Visit(ref entity);

		public void ValidateIf(IIf entity) => Visit(ref entity);

		public void ValidateInitial(IInitial entity) => Visit(ref entity);

		public void ValidateInlineContent(IInlineContent entity) => Visit(ref entity);

		public void ValidateInvoke(IInvoke entity) => Visit(ref entity);

		public void ValidateLocation(ILocationExpression entity) => Visit(ref entity);

		public void ValidateParam(IParam entity) => Visit(ref entity);

		public void ValidateRaise(IRaise entity) => Visit(ref entity);

		public void ValidateScript(IScript entity) => Visit(ref entity);

		public void ValidateScriptExpression(IScriptExpression entity) => Visit(ref entity);

		public void ValidateSend(ISend entity) => Visit(ref entity);

		public void ValidateState(IState entity) => Visit(ref entity);

		public void ValidateTransition(ITransition entity) => Visit(ref entity);
	}
}