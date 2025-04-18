namespace CorporateSystem.SharedDocs.Tests.IntegrationTests;

[CollectionDefinition("PostgresCollection")]
public class PostgresTestCollection : ICollectionFixture<PostgresContainer>;