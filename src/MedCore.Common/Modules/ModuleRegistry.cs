namespace MedCore.Common.Modules;

public sealed class ModuleRegistry
{
    private readonly List<IModule> _modules = [];
    public IReadOnlyList<IModule> Modules => _modules;
    public void Register(IModule module) => _modules.Add(module);
}
