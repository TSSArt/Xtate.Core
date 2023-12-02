#define INTERNAL_NULLABLE_ATTRIBUTES

#if NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NET45 || NET451 || NET452 || NET6 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48
// https://github.com/dotnet/corefx/blob/48363ac826ccf66fbe31a5dcb1dc2aab9a7dd768/src/Common/src/CoreLib/System/Diagnostics/CodeAnalysis/NullableAttributes.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InconsistentNaming
// ReSharper disable InheritdocConsiderUsage
// ReSharper disable UnusedType.Global

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>Specifies that null is allowed as an input even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
#if INTERNAL_NULLABLE_ATTRIBUTES
    internal
#else
    public
#endif
        sealed class AllowNullAttribute : Attribute
    { }

    /// <summary>Specifies that null is disallowed as an input even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
#if INTERNAL_NULLABLE_ATTRIBUTES
    internal
#else
    public
#endif
        sealed class DisallowNullAttribute : Attribute
    { }

    /// <summary>Specifies that an output may be null even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
#if INTERNAL_NULLABLE_ATTRIBUTES
    internal
#else
    public
#endif
        sealed class MaybeNullAttribute : Attribute
    { }

    /// <summary>Specifies that an output will not be null even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
#if INTERNAL_NULLABLE_ATTRIBUTES
    internal
#else
    public
#endif
        sealed class NotNullAttribute : Attribute
    { }

	/// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter may be null even if the corresponding type disallows it.</summary>
	/// <remarks>Initializes the attribute with the specified return value condition.</remarks>
	/// <param name="returnValue">
	/// The return value condition. If the method returns this value, the associated parameter may be null.
	/// </param>
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false), ExcludeFromCodeCoverage]
#if INTERNAL_NULLABLE_ATTRIBUTES
    internal
#else
    public
#endif
        sealed class MaybeNullWhenAttribute(bool returnValue) : Attribute
    {

		/// <summary>Gets the return value condition.</summary>
		public bool ReturnValue { get; } = returnValue;
	}

	/// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be null even if the corresponding type allows it.</summary>
	/// <remarks>Initializes the attribute with the specified return value condition.</remarks>
	/// <param name="returnValue">
	/// The return value condition. If the method returns this value, the associated parameter will not be null.
	/// </param>
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false), ExcludeFromCodeCoverage]
#if INTERNAL_NULLABLE_ATTRIBUTES
    internal
#else
    public
#endif
        sealed class NotNullWhenAttribute(bool returnValue) : Attribute
    {

		/// <summary>Gets the return value condition.</summary>
		public bool ReturnValue { get; } = returnValue;
	}

	/// <summary>Specifies that the output will be non-null if the named parameter is non-null.</summary>
	/// <remarks>Initializes the attribute with the associated parameter name.</remarks>
	/// <param name="parameterName">
	/// The associated parameter name.  The output will be non-null if the argument to the parameter specified is non-null.
	/// </param>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
	[ExcludeFromCodeCoverage]
#if INTERNAL_NULLABLE_ATTRIBUTES
    internal
#else
    public
#endif
        sealed class NotNullIfNotNullAttribute(string parameterName) : Attribute
    {

		/// <summary>Gets the associated parameter name.</summary>
		public string ParameterName { get; } = parameterName;
	}

    /// <summary>Applied to a method that will never return under any circumstance.</summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false), ExcludeFromCodeCoverage]
#if INTERNAL_NULLABLE_ATTRIBUTES
    internal
#else
    public
#endif
        sealed class DoesNotReturnAttribute : Attribute
    { }

	/// <summary>Specifies that the method will not return if the associated Boolean parameter is passed the specified value.</summary>
	/// <remarks>Initializes the attribute with the specified parameter value.</remarks>
	/// <param name="parameterValue">
	/// The condition parameter value. Code after the method will be considered unreachable by diagnostics if the argument to
	/// the associated parameter matches this value.
	/// </param>
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false), ExcludeFromCodeCoverage]
#if INTERNAL_NULLABLE_ATTRIBUTES
    internal
#else
    public
#endif
        sealed class DoesNotReturnIfAttribute(bool parameterValue) : Attribute
    {

		/// <summary>Gets the condition parameter value.</summary>
		public bool ParameterValue { get; } = parameterValue;
	}
}
#endif