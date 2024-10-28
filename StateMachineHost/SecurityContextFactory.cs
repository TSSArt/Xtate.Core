namespace Xtate.Core;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public sealed class SecurityContextFactory
{
	private readonly AsyncLocal<SecurityContext> _securityContext = new();

	private SecurityContext CurrentSecurityContext => _securityContext.Value ?? SecurityContext.FullAccess;

	[UsedImplicitly]
	public IIoBoundTask GetIIoBoundTask() => CurrentSecurityContext;

	[UsedImplicitly]
	public SecurityContextRegistration GetRegistration(SecurityContextType securityContextType) => new(_securityContext, securityContextType);
}