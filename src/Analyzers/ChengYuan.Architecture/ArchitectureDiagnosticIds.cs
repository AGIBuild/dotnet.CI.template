namespace ChengYuan.Architecture;

internal static class ArchitectureDiagnosticIds
{
    public const string WebFacetMustNotReferencePersistence = "CYARCH001";
    public const string PersistenceFacetMustBeProviderNeutral = "CYARCH002";
    public const string HostMustOwnRuntimePersistenceComposition = "CYARCH003";
    public const string ModuleDependencyMustRespectCategoryOrder = "CYARCH004";
    public const string EfStoreMustUseScopedDbContext = "CYARCH005";
    public const string HostDiagnosticsMustBeExplicitlyEnabled = "CYARCH006";
}
