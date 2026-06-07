namespace ChengYuan.Architecture;

internal static class ArchitectureDiagnosticCatalog
{
    public static IReadOnlyList<ArchitectureDiagnosticRule> Rules { get; } =
    [
        new(
            ArchitectureDiagnosticIds.WebFacetMustNotReferencePersistence,
            "Web facets must not reference persistence facets",
            "ModuleBoundaries",
            "Web project '{0}' must not reference persistence project '{1}'.",
            "Web modules expose HTTP endpoints and depend on application contracts; host composition owns persistence wiring."),
        new(
            ArchitectureDiagnosticIds.PersistenceFacetMustBeProviderNeutral,
            "Persistence facets must be provider-neutral",
            "DataAccess",
            "Persistence project '{0}' must not reference provider project or package '{1}'.",
            "Database provider choice belongs to host/runtime tooling, not reusable persistence facets."),
        new(
            ArchitectureDiagnosticIds.HostMustOwnRuntimePersistenceComposition,
            "Host must own runtime persistence composition",
            "Composition",
            "Persistence module '{0}' must be composed by a host composition module, not by web module '{1}'.",
            "Application web facets stay portable while host modules decide concrete runtime infrastructure."),
        new(
            ArchitectureDiagnosticIds.ModuleDependencyMustRespectCategoryOrder,
            "Module dependency must respect category order",
            "Modularity",
            "Module '{0}' cannot depend on '{1}' because it violates the framework/application/extension/host layering order.",
            "Layered module categories keep dependencies acyclic and aligned with ABP-style modular composition."),
        new(
            ArchitectureDiagnosticIds.EfStoreMustUseScopedDbContext,
            "EF-backed stores must use scoped DbContext",
            "DataAccess",
            "Store '{0}' should consume scoped DbContext '{1}' instead of IDbContextFactory<TContext> or singleton lifetime.",
            "EF stores participate in the current request scope and share the same transaction semantics as repositories and UoW."),
        new(
            ArchitectureDiagnosticIds.HostDiagnosticsMustBeExplicitlyEnabled,
            "Host diagnostics must be explicitly enabled",
            "Security",
            "Diagnostic endpoint or OpenAPI mapping '{0}' must be gated by environment or explicit configuration.",
            "Templates should not expose module catalogs, OpenAPI, or detailed health output by default in production.")
    ];
}
