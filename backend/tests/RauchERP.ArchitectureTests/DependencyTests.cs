using NetArchTest.Rules;
using RauchERP.Modules.Catalog.Domain;

namespace RauchERP.ArchitectureTests;
public sealed class DependencyTests
{
    private static readonly string[] ForbiddenDomainDependencies = ["Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", ".Infrastructure", ".Presentation"];
    [Fact] public void Catalog_domain_has_no_forbidden_dependencies() => Assert.True(Types.InAssembly(typeof(Product).Assembly).ShouldNot().HaveDependencyOnAny(ForbiddenDomainDependencies).GetResult().IsSuccessful);
    [Fact] public void Identity_domain_has_no_forbidden_dependencies() => Assert.True(Types.InAssembly(typeof(Modules.Identity.Domain.User).Assembly).ShouldNot().HaveDependencyOnAny(ForbiddenDomainDependencies).GetResult().IsSuccessful);
    [Fact] public void Organization_domain_has_no_forbidden_dependencies() => Assert.True(Types.InAssembly(typeof(Modules.Organization.Domain.Company).Assembly).ShouldNot().HaveDependencyOnAny(ForbiddenDomainDependencies).GetResult().IsSuccessful);
    [Fact] public void Inventory_domain_has_no_forbidden_dependencies() => Assert.True(Types.InAssembly(typeof(Modules.Inventory.Domain.StockBalance).Assembly).ShouldNot().HaveDependencyOnAny(ForbiddenDomainDependencies).GetResult().IsSuccessful);
}
