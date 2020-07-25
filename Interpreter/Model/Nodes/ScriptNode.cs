using Xtate.Persistence;

namespace Xtate
{
	internal sealed class ScriptNode : ExecutableEntityNode, IScript, IAncestorProvider
	{
		private readonly ScriptEntity _entity;

		public ScriptNode(in DocumentIdRecord documentIdNode, in ScriptEntity entity) : base(documentIdNode, (IScript?) entity.Ancestor) => _entity = entity;

	#region Interface IAncestorProvider

		object? IAncestorProvider.Ancestor => _entity.Ancestor;

	#endregion

	#region Interface IScript

		public IScriptExpression? Content => _entity.Content;

		public IExternalScriptExpression? Source => _entity.Source;

	#endregion

		protected override void Store(Bucket bucket)
		{
			bucket.Add(Key.TypeInfo, TypeInfo.ScriptNode);
			bucket.Add(Key.DocumentId, DocumentId);
			bucket.AddEntity(Key.Content, Content);
			bucket.AddEntity(Key.Source, Source);
		}
	}
}