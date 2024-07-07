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

using System.Dynamic;

namespace Xtate.Core;

internal class MetaObject(Expression expression, object value, Func<Expression, DynamicMetaObject> metaObjectCreator) : DynamicMetaObject(expression, BindingRestrictions.Empty, value)
{
	private readonly DynamicMetaObject _metaObject = metaObjectCreator(expression);

	public override DynamicMetaObject BindGetMember(GetMemberBinder binder) => _metaObject.BindGetMember(binder);

	public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) => _metaObject.BindSetMember(binder, value);

	public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) => _metaObject.BindDeleteMember(binder);

	public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) => _metaObject.BindGetIndex(binder, indexes);

	public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) => _metaObject.BindSetIndex(binder, indexes, value);

	public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes) => _metaObject.BindDeleteIndex(binder, indexes);

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) => _metaObject.BindInvokeMember(binder, args);

	public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) => _metaObject.BindInvoke(binder, args);

	public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) => _metaObject.BindCreateInstance(binder, args);

	public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) => _metaObject.BindUnaryOperation(binder);

	public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) => _metaObject.BindBinaryOperation(binder, arg);

	public override DynamicMetaObject BindConvert(ConvertBinder binder) => _metaObject.BindConvert(binder);
}