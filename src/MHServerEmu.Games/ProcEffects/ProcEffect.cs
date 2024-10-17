using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Events;

namespace MHServerEmu.Games.ProcEffects
{
    /// <summary>
    /// Base class for all proc effects.
    /// </summary>
    public abstract class ProcEffect
    {
        /// <summary>
        /// Initializes the proc effect with the owning entity.
        /// </summary>
        /// <param name="owner">The entity that owns this proc effect.</param>
        public virtual void Initialize(Entity owner)
        {
            // Initialize if necessary
        }

        /// <summary>
        /// Called when an attack hits a target.
        /// </summary>
        /// <param name="attacker">The entity performing the attack.</param>
        /// <param name="target">The entity being hit by the attack.</param>
        /// <param name="attackData">Additional data about the attack.</param>
        public abstract void OnHit(Entity attacker, Entity target, AttackData attackData);
    }
}
