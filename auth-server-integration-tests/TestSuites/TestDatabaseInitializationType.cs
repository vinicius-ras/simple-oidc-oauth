namespace SimpleOidcOauth.Tests.Integration.TestSuites
{
	/// <summary>The type of database initialization to be performed for a specific test suite during the Integration Tests.</summary>
	public enum TestDatabaseInitializationType
	{
		/// <summary>Indicates that the test suite doesn't require any kind of database initialization.</summary>
		None,
		/// <summary>
		///     Indicates that the test suite requires only a structural database initialization.
		///     This is meant to be used by test suites that require an empty database before tests are performed.
		/// </summary>
		StructureOnly,
		/// <summary>Indicates that the test suite requires full initialization of a database containing sample test data.</summary>
		StructureAndTestData,
	}
}