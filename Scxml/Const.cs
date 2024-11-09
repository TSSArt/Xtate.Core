// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate;

/// <summary>
///     Contains constant values used throughout the Xtate project.
/// </summary>
public static class Const
{
	public const string ScxmlNs = "http://www.w3.org/2005/07/scxml";

	public const string XtateScxmlNs = "http://xtate.net/scxml";

	public const string ScxmlIoProcessorInvokeIdPrefix = @"#_";

	public const string ScxmlIoProcessorSessionIdPrefix = "#_scxml_";

	/// <summary>
	///     #_internal
	/// </summary>
	public static readonly FullUri ScxmlIoProcessorInternalTarget = new(@"#_internal");

	/// <summary>
	///     http://www.w3.org/TR/scxml/#SCXMLEventProcessor
	/// </summary>
	public static readonly FullUri ScxmlIoProcessorId = new(@"http://www.w3.org/TR/scxml/#SCXMLEventProcessor");

	/// <summary>
	///     scxml
	/// </summary>
	public static readonly FullUri ScxmlIoProcessorAliasId = new(@"scxml");

	/// <summary>
	///     #_parent
	/// </summary>
	public static readonly FullUri ScxmlIoProcessorParentTarget = new(@"#_parent");

	/// <summary>
	///     _parent
	/// </summary>
	public static readonly FullUri ParentTarget = new(@"_parent");

	/// <summary>
	///     _internal
	/// </summary>
	public static readonly FullUri InternalTarget = new(@"_internal");

	/// <summary>
	///     ioprocessor:///
	/// </summary>
	public static readonly Uri ScxmlIoProcessorBaseUri = new(@"ioprocessor:///");
}