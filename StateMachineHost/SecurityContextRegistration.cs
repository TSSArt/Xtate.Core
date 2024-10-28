namespace Xtate.Core;

public sealed class SecurityContextRegistration : IAsyncDisposable
{
	private readonly AsyncLocal<SecurityContext> _asyncLocal;
	private readonly SecurityContext             _newContext;
	private readonly SecurityContext?            _parentContext;

	internal SecurityContextRegistration(AsyncLocal<SecurityContext> asyncLocal, SecurityContextType securityContextType)
	{
		_asyncLocal = asyncLocal;
		_parentContext = asyncLocal.Value;
		asyncLocal.Value = _newContext = (_parentContext ?? SecurityContext.FullAccess).CreateNested(securityContextType);
	}

#region Interface IAsyncDisposable

	public ValueTask DisposeAsync()
	{
		Infra.Assert(_asyncLocal.Value == _newContext);

		_asyncLocal.Value = _parentContext!;

		return default;
	}

#endregion
}