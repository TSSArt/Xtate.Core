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
using Xtate.StateMachine.Services;

namespace Xtate.Test.UnitTests.StateMachine;

[TestClass]
public class StateMachineVisitorCoverageTest
{
	[TestMethod]
	public void TrackedPathContainsRootCollectionAndNestedEntity()
	{
		// Expected: CurrentPath identifies the root, collection, and nested final-state entity.
		// Actual: SetRootPath and Enter store [] (an empty, nondefault array), so EntityName casts the entity instance to Type.
		// Enable this test when entity path entries use a default array or EntityName recognizes empty entity markers.
		IStateMachine machine = new MachineSource { States = [new FinalSource()] };
		var visitor = new PathVisitor(trackPath: true);

		visitor.Process(ref machine);

		Assert.AreEqual(expected: "MachineSource", visitor.RootPath);
		Assert.AreEqual(expected: "MachineSource", visitor.CompletedPath);
		Assert.AreEqual(expected: "MachineSource/IStateEntity[..]/FinalSource", visitor.FinalPath);
	}

	[TestMethod]
	public void UntrackedPathRemainsNullAndDoesNotRetainRoot()
	{
		var visitor = new PathVisitor(trackPath: false);
		var first = new object();
		var second = new object();

		visitor.SetRoot(first);
		visitor.SetRoot(second);

		Assert.IsNull(visitor.Path);
	}

	[TestMethod]
	public void TrackedRootRejectsNullAndReplacement()
	{
		var visitor = new PathVisitor(trackPath: true);

		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => visitor.SetRoot(root: null!));
		visitor.SetRoot(new object());
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => visitor.SetRoot(new object()));
	}

	[TestMethod]
	public void TrackedTraversalEntersAndExitsCollectionsAndEntitiesWithoutFormattingPath()
	{
		IStateMachine machine = new MachineSource { States = [new FinalSource()] };
		var visitor = new TrackedTraversalVisitor();

		visitor.Process(ref machine);

		Assert.AreEqual(expected: 1, visitor.FinalVisits);
	}

	[TestMethod]
	public void UnknownStateAndExecutableAreDispatchedToFallbacks()
	{
		var visitor = new DispatchVisitor();
		IStateEntity state = new UnknownState();
		IExecutableEntity executable = new UnknownExecutable();

		visitor.Process(ref state);
		visitor.Process(ref executable);

		Assert.AreEqual(expected: 1, visitor.UnknownStates);
		Assert.AreEqual(expected: 1, visitor.UnknownExecutables);
		Assert.AreSame(state, visitor.LastUnknownState);
		Assert.AreSame(executable, visitor.LastUnknownExecutable);
	}

	[TestMethod]
	public void ChangedInterfaceEntityIsRebuiltWhileUnchangedStructIsPreserved()
	{
		var visitor = new ExpressionVisitor("changed");
		IValueExpression source = new ExpressionSource { Expression = "original" };
		IValueExpression unchanged = new ValueExpression { Expression = "stable" };
		var originalBox = unchanged;

		visitor.Process(ref source);
		new ExpressionVisitor(replacement: null).Process(ref unchanged);

		Assert.IsInstanceOfType<ValueExpression>(source);
		Assert.AreEqual(expected: "changed", source.Expression);
		Assert.AreSame(originalBox, unchanged);
	}

	[TestMethod]
	public void DirectVisitorEntryPointsRejectNullEntities()
	{
		var visitor = new ArgumentGuardVisitor();
		IIdentifier identifier = null!;
		IEventDescriptor descriptor = null!;
		IOutgoingEvent outgoingEvent = null!;
		IExecutableEntity executable = null!;
		IStateEntity state = null!;
		IValueExpression expression = null!;

		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => visitor.Process(ref identifier));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => visitor.Process(ref descriptor));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => visitor.Process(ref outgoingEvent));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => visitor.Process(ref executable));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => visitor.Process(ref state));
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => visitor.Process(ref expression));
	}

	[TestMethod]
	public void ValueExpressionListBuildReplacesItemsAndRejectsDefaultList()
	{
		var first = new ExpressionSource { Expression = "first" };
		var second = new ExpressionSource { Expression = "second" };
		var list = ImmutableArray.Create<IValueExpression>(first, second);
		var visitor = new ExpressionVisitor("replacement");

		visitor.Process(ref list);

		Assert.HasCount(expected: 2, list);
		Assert.AreEqual(expected: "replacement", list[0].Expression);
		Assert.AreEqual(expected: "replacement", list[1].Expression);
		Assert.AreNotSame(first, list[0]);
		Assert.AreNotSame(second, list[1]);

		var defaultList = default(ImmutableArray<IValueExpression>);
		Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => visitor.Process(ref defaultList));
	}

	[TestMethod]
	public void TrackListSupportsIdentityMutationInsertionRemovalAdditionAndEnumeration()
	{
		var first = new ExpressionSource { Expression = "first" };
		var second = new ExpressionSource { Expression = "second" };
		var replacement = new ExpressionSource { Expression = "replacement" };
		var inserted = new ExpressionSource { Expression = "inserted" };
		var list = ImmutableArray.Create<IValueExpression>(first, second);
		var visitor = new TrackListVisitor(replacement, inserted);

		visitor.Process(ref list);

		Assert.IsTrue(visitor.ContainedFirstBeforeMutation);
		Assert.IsTrue(visitor.ContainedInsertedAfterMutation);
		Assert.AreEqual(expected: 2, visitor.InitialEnumerationCount);
		Assert.AreEqual(expected: 3, visitor.ModifiedEnumerationCount);
		CollectionAssert.AreEqual(new[] { replacement, inserted, first }, list.ToArray());
	}

	[TestMethod]
	public void TrackListInsertAndRemoveCreateBuildersAndCanRestoreOriginalSequence()
	{
		var first = new ExpressionSource { Expression = "first" };
		var second = new ExpressionSource { Expression = "second" };
		var inserted = ImmutableArray.Create<IValueExpression>(first);
		var removed = ImmutableArray.Create<IValueExpression>(first, second);
		var restored = ImmutableArray.Create<IValueExpression>(first);

		new InitialTrackListMutationVisitor(TrackMutation.Insert, second).Process(ref inserted);
		new InitialTrackListMutationVisitor(TrackMutation.Remove, second).Process(ref removed);
		new InitialTrackListMutationVisitor(TrackMutation.Restore, second).Process(ref restored);

		CollectionAssert.AreEqual(new[] { first, second }, inserted.ToArray());
		CollectionAssert.AreEqual(new[] { second }, removed.ToArray());
		CollectionAssert.AreEqual(new[] { first }, restored.ToArray());
	}

	[TestMethod]
	public void TrackListClearHandlesEmptyOriginalAndAlreadyModifiedBuilder()
	{
		var item = new ExpressionSource { Expression = "item" };
		var empty = ImmutableArray<IValueExpression>.Empty;
		var populated = ImmutableArray.Create<IValueExpression>(item);
		var modified = ImmutableArray.Create<IValueExpression>(item);

		new ClearTrackListVisitor(modifyFirst: false).Process(ref empty);
		new ClearTrackListVisitor(modifyFirst: false).Process(ref populated);
		new ClearTrackListVisitor(modifyFirst: true).Process(ref modified);

		Assert.IsEmpty(empty);
		Assert.IsEmpty(populated);
		Assert.IsEmpty(modified);
	}

	private sealed class PathVisitor(bool trackPath) : StateMachineVisitor(trackPath)
	{
		public string? Path => CurrentPath;

		public string? RootPath { get; private set; }

		public string? FinalPath { get; private set; }

		public string? CompletedPath { get; private set; }

		public void SetRoot(object root) => SetRootPath(root);

		public void Process(ref IStateMachine machine)
		{
			SetRootPath(machine);
			RootPath = CurrentPath;
			Visit(ref machine);
			CompletedPath = CurrentPath;
		}

		protected override void Visit(ref IFinal entity)
		{
			FinalPath = CurrentPath;
			base.Visit(ref entity);
		}
	}

	private sealed class DispatchVisitor : StateMachineVisitor
	{
		public int UnknownStates { get; private set; }

		public int UnknownExecutables { get; private set; }

		public IStateEntity? LastUnknownState { get; private set; }

		public IExecutableEntity? LastUnknownExecutable { get; private set; }

		public void Process(ref IStateEntity entity) => Visit(ref entity);

		public void Process(ref IExecutableEntity entity) => Visit(ref entity);

		protected override void VisitUnknown(ref IStateEntity entity)
		{
			UnknownStates ++;
			LastUnknownState = entity;
			base.VisitUnknown(ref entity);
		}

		protected override void VisitUnknown(ref IExecutableEntity entity)
		{
			UnknownExecutables ++;
			LastUnknownExecutable = entity;
			base.VisitUnknown(ref entity);
		}
	}

	private sealed class TrackedTraversalVisitor : StateMachineVisitor
	{
		public TrackedTraversalVisitor() : base(trackPath: true) { }

		public int FinalVisits { get; private set; }

		public void Process(ref IStateMachine machine)
		{
			SetRootPath(machine);
			Visit(ref machine);
		}

		protected override void Visit(ref IFinal entity)
		{
			FinalVisits ++;
			base.Visit(ref entity);
		}
	}

	private sealed class ArgumentGuardVisitor : StateMachineVisitor
	{
		public void Process(ref IIdentifier entity) => Visit(ref entity);

		public void Process(ref IEventDescriptor entity) => Visit(ref entity);

		public void Process(ref IOutgoingEvent entity) => Visit(ref entity);

		public void Process(ref IExecutableEntity entity) => Visit(ref entity);

		public void Process(ref IStateEntity entity) => Visit(ref entity);

		public void Process(ref IValueExpression entity) => Visit(ref entity);
	}

	private sealed class ExpressionVisitor(string? replacement) : StateMachineVisitor
	{
		public void Process(ref IValueExpression entity) => Visit(ref entity);

		public void Process(ref ImmutableArray<IValueExpression> list) => Visit(ref list);

		protected override void Build(ref ValueExpression properties)
		{
			if (replacement is not null)
			{
				properties.Expression = replacement;
			}
		}
	}

	private sealed class TrackListVisitor(IValueExpression replacement, IValueExpression inserted) : StateMachineVisitor
	{
		public bool ContainedFirstBeforeMutation { get; private set; }

		public bool ContainedInsertedAfterMutation { get; private set; }

		public int InitialEnumerationCount { get; private set; }

		public int ModifiedEnumerationCount { get; private set; }

		public void Process(ref ImmutableArray<IValueExpression> list) => Visit(ref list);

		protected override void Build(ref TrackList<IValueExpression> trackList)
		{
			var first = trackList[0]!;
			ContainedFirstBeforeMutation = trackList.Contains(first);

			foreach (var _ in trackList)
			{
				InitialEnumerationCount ++;
			}

			trackList[0] = first;
			trackList[0] = replacement;
			trackList.Insert(index: 1, inserted);
			trackList.RemoveAt(index: 2);
			trackList.Add(first);
			ContainedInsertedAfterMutation = trackList.Contains(inserted);

			foreach (var _ in trackList)
			{
				ModifiedEnumerationCount ++;
			}
		}
	}

	private sealed class InitialTrackListMutationVisitor(TrackMutation mutation, IValueExpression item) : StateMachineVisitor
	{
		public void Process(ref ImmutableArray<IValueExpression> list) => Visit(ref list);

		protected override void Build(ref TrackList<IValueExpression> trackList)
		{
			switch (mutation)
			{
				case TrackMutation.Insert:
					trackList.Insert(index: 1, item);

					break;

				case TrackMutation.Remove:
					trackList.RemoveAt(index: 0);

					break;

				case TrackMutation.Restore:
					trackList.Insert(index: 1, item);
					trackList.RemoveAt(index: 1);

					break;
			}
		}
	}

	private sealed class ClearTrackListVisitor(bool modifyFirst) : StateMachineVisitor
	{
		public void Process(ref ImmutableArray<IValueExpression> list) => Visit(ref list);

		protected override void Build(ref TrackList<IValueExpression> trackList)
		{
			if (modifyFirst && trackList.Count > 0)
			{
				trackList.Add(trackList[0]);
			}

			trackList.Clear();
		}
	}

	private sealed class MachineSource : IStateMachine
	{
	#region Interface IStateMachine

		public string? Name => null;

		public string? DataModelType => null;

		public BindingType Binding => BindingType.Early;

		public IInitial? Initial => null;

		public ImmutableArray<IStateEntity> States { get; init; }

		public IDataModel? DataModel => null;

		public IExecutableEntity? Script => null;

	#endregion
	}

	private sealed class FinalSource : IFinal
	{
	#region Interface IFinal

		public ImmutableArray<IOnEntry> OnEntry => default;

		public ImmutableArray<IOnExit> OnExit => default;

		public IDoneData? DoneData => null;

	#endregion

	#region Interface IStateEntity

		public IIdentifier? Id => null;

	#endregion
	}

	private sealed class UnknownState : IStateEntity
	{
	#region Interface IStateEntity

		public IIdentifier? Id => null;

	#endregion
	}

	private sealed class UnknownExecutable : IExecutableEntity;

	private sealed class ExpressionSource : IValueExpression
	{
	#region Interface IValueExpression

		public string? Expression { get; init; }

	#endregion
	}

	private enum TrackMutation
	{
		Insert,

		Remove,

		Restore
	}
}