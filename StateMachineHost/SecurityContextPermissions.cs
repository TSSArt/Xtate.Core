namespace Xtate.Core;

[Flags]
public enum SecurityContextPermissions
{
	None                      = 0x0000_0000,
	RunIoBoundTask            = 0x0000_0001,
	CreateStateMachine        = 0x0000_0002,
	CreateTrustedStateMachine = 0x0000_0004,
	Full                      = 0x7FFF_FFFF
}