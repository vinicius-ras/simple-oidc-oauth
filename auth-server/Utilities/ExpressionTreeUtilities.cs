using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleOidcOauth.Utilities
{
	/// <summary>Provides utility methods for working with Expression Trees (<see cref="System.Linq.Expressions"/>).</summary>
	public static class ExpressionTreeUtilities
	{
		// STATIC METHODS
		/// <summary>Retrieves the whole path used to access a specific member of a class.</summary>
		/// <remarks>
		///     This method requires client code to provide a simple Lambda Expression which accesses the target member.
		///     The given Lambda Expression will be compiled by the C# Compiler into an Expression Tree describing the member access.
		///     This Expression Tree which will then be processed by this method at runtime.
		/// </remarks>
		/// <param name="obj">
		///     <para>An object of the type whose member needs to be accessed.</para>
		///     <para>
		///         This object is used for the sole purpose of infering the class/struct type from which
		///         we will extract the target member. Thus, a <c>null</c> (for classes) or <c>default</c> (for structs) reference can
		///         be safely provided, as long as it is cast to the target type (e.g., you can use <c>(MyClassType>) null</c>).
		///     </para>
		///     <para>
		///         Notice that for structs, using the <c>(MyStructType) default</c> value will cause the construction of an empty struct
		///         instance of the given target type. This is unavoidable, but will not affect the method's execution in any ways (besides
		///         temporarilly allocating a small amount of extra uneeded memory for the struct object).
		///     </para>
		/// </param>
		/// <param name="memberAccessLambda">A Lambda Expression whose body simply accesses a target member, whose access path will be formatted into a string.</param>
		/// <param name="membersSeparator">A string to be used as a path separator between nested members.</param>
		/// <param name="ignoreRootObject">Flag indicating if the root object used to access the target member should be included in the path result.</param>
		/// <typeparam name="TInputExpression">The root type used to access a target member.</typeparam>
		/// <typeparam name="TReturn">The type of the target member that will be accessed.</typeparam>
		/// <returns>Returns a string containing the path to the target member.</returns>
		/// <exception cref="ArgumentException">
		///     Thrown when the body of the Lamda Expression passed into the <paramref name="memberAccessLambda"/> parameter
		///     does not represent a member access. Expressions representing anything other than property or field accesses cannot be processed
		///     by this method.
		/// </exception>
		public static string GetMemberAccessPath<TInputExpression, TReturn>(
			TInputExpression obj,
			Expression<Func<TInputExpression, TReturn>> memberAccessLambda,
			string membersSeparator,
			bool ignoreRootObject)
		{
			// Lambda Expressions describing something other than a member (property//field) access will be rejected
			if (memberAccessLambda.Body is MemberExpression == false)
				throw new ArgumentException($"This method only accepts simple lambda expressions which access a property or field of the given object.");


			// Place member access expressions and their nested member accesses in a stack.
			// Also place the last expression node after all member access expressions: that node will describe the
			// name of the variable being used to access the target member.
			var baseBodyMemberExpr = memberAccessLambda.Body;
			var expressionNodesStack = new Stack<Expression>();
			while (true)
			{
				expressionNodesStack.Push(baseBodyMemberExpr);
				if (baseBodyMemberExpr is MemberExpression memberExpr)
					baseBodyMemberExpr = memberExpr.Expression;
				else
					break;
			}
			if (ignoreRootObject)
				expressionNodesStack.TryPop(out _);


			// Join everything in the stack into a string result, using the given separator
			var expressionNames = expressionNodesStack.Select(expression => expression switch {
				ParameterExpression paramExpr => paramExpr.Name,
				MemberExpression memberExpr => memberExpr.Member.Name,
				_ => throw new ArgumentException($@"Found a node of type ""{expression.GetType()}"" in the Lambda Expression - it must be formed by property/field access ({nameof(MemberExpression)}) nodes only."),
			});
			var result = string.Join(membersSeparator, expressionNames);
			return result;
		}
	}
}