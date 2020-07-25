using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace Xtate
{
	internal class MetaObject : DynamicMetaObject
	{
		private readonly DynamicMetaObject _metaObject;

		public MetaObject(Expression expression, object val, Func<Expression, DynamicMetaObject> metaObjectCreator) : base(expression, BindingRestrictions.Empty, val) =>
				_metaObject = metaObjectCreator(expression);

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
}