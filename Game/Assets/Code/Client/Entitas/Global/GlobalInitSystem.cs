using Entitas;
using JetBrains.Annotations;

namespace Client.Entitas.Global
{
    [UsedImplicitly]
    public sealed class GlobalInitSystem : IInitializeSystem
    {
        private readonly GlobalContext _context;

        public GlobalInitSystem(GlobalContext context)
        {
            _context = context;
        }

        public void Initialize()
        {
            if (!_context.hasTimeScale) _context.ReplaceTimeScale(1.0f);
        }
    }
}