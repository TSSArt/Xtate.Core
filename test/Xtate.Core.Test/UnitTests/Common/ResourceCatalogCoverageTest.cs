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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.


using System.Globalization;
using System.Reflection;

namespace Xtate.Core.Test.UnitTests.Common;

[TestClass]
public class ResourceCatalogCoverageTest
{
	[TestMethod]
	[DoNotParallelize]
	public void ExplicitResourceCultureFallsBackToInvariantCatalog()
	{
		var original = Resources.Culture;
		try
		{
			Resources.Culture = CultureInfo.GetCultureInfo("pl-PL");
			Assert.AreEqual("pl-PL", Resources.Culture.Name);
			Assert.AreEqual(Resources.ResourceManager.GetString(nameof(Resources.Exception_UnsupportedObjectType), CultureInfo.InvariantCulture), Resources.Exception_UnsupportedObjectType);
		}
		finally
		{
			Resources.Culture = original;
		}
	}

	public static IEnumerable<object[]> ResourceNames =>
		typeof(Resources).GetProperties(BindingFlags.Static | BindingFlags.NonPublic)
			.Where(property => property.PropertyType == typeof(string))
			.Select(property => new object[] { property.Name });

	[TestMethod]
	[DynamicData(nameof(ResourceNames))]
	public void DiagnosticResourceHasNonEmptyTextAndValidFormat(string name)
	{
		var property = typeof(Resources).GetProperty(name, BindingFlags.Static | BindingFlags.NonPublic)!;
		var text = (string?)property.GetValue(null);

		Assert.IsFalse(string.IsNullOrWhiteSpace(text), name);
		Assert.AreEqual(Resources.ResourceManager.GetString(name, CultureInfo.CurrentUICulture), text, name);
		var arguments = Enumerable.Range(0, 32).Select(index => (object)$"argument{index}").ToArray();
		if (System.Text.RegularExpressions.Regex.IsMatch(text!, @"\{\d+"))
		{
			Assert.IsFalse(string.IsNullOrWhiteSpace(string.Format(CultureInfo.InvariantCulture, text!, arguments)), name);
		}
	}
}
