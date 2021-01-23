using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleOidcOauth.Extensions
{
	/// <summary>
	///     Extension methods for the <see cref="IQueryable{T}"/> interface.
	///     These methods usually provide useful extra features for LINQ in the form of query operator methods.
	/// </summary>
	public static class CustomIQueryableExtensions
	{
		// STATIC FIELDS
		/// <summary>
		///     <para>
		///         A <see cref="MethodInfo"/> describing the <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)"/> method.
		///     </para>
		///     <para>
		///         Notice that this <see cref="MethodInfo"/> refers to the generic version of the method.
		///         To be useful, it is usually necessary to generate a <see cref="MethodInfo"/> describing a closed generic version of this method,
		///         which can be done by calling <see cref="MethodInfo.MakeGenericMethod(Type[])"/>.
		///     </para>
		/// </summary>
		private static readonly MethodInfo _methodInfoEnumerableContains;





		// STATIC METHODS
		/// <summary>Static initializer.</summary>
		static CustomIQueryableExtensions()
		{
			_methodInfoEnumerableContains = typeof(Enumerable)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(
					method => method.Name == nameof(Enumerable.Contains)
					&& method.GetParameters().Length == 2)
				.Single();
		}


		/// <summary>
		///     <para>
		///         Filters a sequence of entities, keeping the ones which have specific key values contained in a given set.
		///         This method allows you to select a property ("key") contained in the entity and perform an SQL-like "IN" operation
		///         using that property.
		///     </para>
		///     <para>Example:</para>
		///     <code>
		///         // Obtain the list of users whose roles are in the set "super-user", "manager", or "sayajin"
		///         var adminUsers = await users.WhereIn(u => u.Role, new [] { "super-user", "manager", "sayajin" })
		///     </code>
		/// </summary>
		/// <param name="entities">The sequence of entities where the operation will be applied.</param>
		/// <param name="keys">
		///     A collection of acceptable key values.
		///     Entities whose key properties' values are present in this collection will be kept in the resulting sequence.
		/// </param>
		/// <param name="keySelector">An expression used to select the key of the entities which will be used for filtering.</param>
		/// <typeparam name="TEntity">The type of the objects being filtered.</typeparam>
		/// <typeparam name="TKey">The type of the keys that will be used for filtering the objects.</typeparam>
		/// <returns>An <see cref="IQueryable{T}"/> sequence that contains the elements from the input sequence whose keys are found to be contained in the <paramref name="keys"/> parameter.</returns>
		public static IQueryable<TEntity> WhereIn<TEntity, TKey>(this IQueryable<TEntity> entities, IEnumerable<TKey> keys, Expression<Func<TEntity, TKey>> keySelector)
		{
			const string wherePredicateParameterName = "entity";

			// Retrieve a MethodInfo for: Enumerable.Contains(this IEnumerable<TKey>, TKey)
			var methodInfoContains = _methodInfoEnumerableContains.MakeGenericMethod(typeof(TKey));

			// CONSIDERING: keySelector = entity => entity.SomeKeyProperty
			// So "keySelector" is a LambaExpression, whose body is in the form "entity.SomeKeyProperty".
			// Knowing that, we will use the "keySelector" expression tree to retrieve a PropertyInfo describing the property which represents
			// an entity key ("SomeKeyProperty")
			var memberExpression = (MemberExpression) keySelector.Body;
			var targetKeyPropertyInfo = (PropertyInfo) memberExpression.Member;


			// We must return: entities.Where(wherePredicate).
			// CONSIDERING:
			//     1: wherePredicate = entity => keys.Contains(keySelector(entity))
			//         1.1: the "wherePredicate" is of type Expression<Func<TEntity, bool>> -- that is, it is an Expression Tree describing the WHERE clause
			//     2: keySelector(entity) basically translates to "entity.SomeKeyProperty", where:
			//         2.1: "entity" is the entities.Where(entity => ...) lambda parameter.
			//         2.2: we already have a PropertyInfo for "SomeKeyProperty"
			// So let's build the "wherePredicate" using this information
			var whereLambdaParameter = Expression.Parameter(typeof(TEntity), wherePredicateParameterName);
			var wherePredicate = Expression.Lambda<Func<TEntity, bool>>(
				body: Expression.Call(
					method: methodInfoContains,
					arg0: Expression.Constant(keys),
					arg1: Expression.Property(whereLambdaParameter, targetKeyPropertyInfo)),
				parameters: whereLambdaParameter
			);
			return entities.Where(wherePredicate);
		}
	}
}