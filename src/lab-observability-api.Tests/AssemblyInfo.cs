using Xunit;

// All test collections run sequentially so multiple WebApplicationFactory instances
// do not race on the shared entry point during initialization.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
