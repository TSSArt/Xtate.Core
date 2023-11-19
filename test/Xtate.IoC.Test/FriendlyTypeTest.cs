using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.IoC.Test
{
	[TestClass]
	public class FriendlyTypeTest
	{
		[TestMethod]
		[DataRow("int", typeof(int))]
		[DataRow("bool", typeof(bool))]
		[DataRow("byte", typeof(byte))]
		[DataRow("char", typeof(char))]
		[DataRow("decimal", typeof(decimal))]
		[DataRow("double", typeof(double))]
		[DataRow("short", typeof(short))]
		[DataRow("int", typeof(int))]
		[DataRow("long", typeof(long))]
		[DataRow("sbyte", typeof(sbyte))]
		[DataRow("float", typeof(float))]
		[DataRow("string", typeof(string))]
		[DataRow("ushort", typeof(ushort))]
		[DataRow("uint", typeof(uint))]
		[DataRow("ulong", typeof(ulong))]
		[DataRow("object", typeof(object))]
		[DataRow("void", typeof(void))]
		[DataRow("List<int>", typeof(List<int>))]
		[DataRow("(int,long)", typeof((int,long)))]
		[DataRow("(int,int,int)", typeof((int,int,int)))]
		[DataRow("(int,int,int,int)", typeof((int,int,int,int)))]
		[DataRow("(int,int,int,int,int)", typeof((int,int,int,int,int)))]
		[DataRow("(int,int,int,int,int,int)", typeof((int,int,int,int,int,int)))]
		[DataRow("(int,int,int,int,int,int,int)", typeof((int,int,int,int,int,int,int)))]
		[DataRow("(int,int,int,int,int,int,int,int)", typeof((int,int,int,int,int,int,int,int)))]
		[DataRow("(int,int,int,int,int,int,int,int,int)", typeof((int,int,int,int,int,int,int,int,int)))]
		[DataRow("(int,int,int,int,int,int,int,int,int,int)", typeof((int,int,int,int,int,int,int,int,int,int)))]
		public void ClassNameTest(string name, Type type)
		{
			Assert.AreEqual(name, type.FriendlyName());
		}
	}
}
