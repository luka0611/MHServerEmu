using System;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Events;
using MHServerEmu.Core.Logging;
using MHServerEmu.Core.System.Random;
using MHServerEmu.Games.Conditions;

namespace MHServerEmu.Games.ProcEffects
{
    /// <summary>
    /// Represents a proc effect that applies a burn condition to targets upon hit.
    /// </summary>
    public class BurningProcEffect : ProcEffect
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        private readonly float _burnDamagePerTick;
        private readonly TimeSpan _burnDuration;
        private readonly TimeSpan _burnTickInterval;
        private readonly float _procChance;
        private readonly Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="BurningProcEffect"/> class.
        /// </summary>
        /// <param name="burnDamagePerTick">Damage dealt each tick of the burn.</param>
        /// <param name="burnDuration">Total duration of the burn effect.</param>
        /// <param name="burnTickInterval">Time between each damage tick.</param>
        /// <param name="procChance">Chance to apply the burn effect upon hit.</param>
        public BurningProcEffect(float burnDamagePerTick, TimeSpan burnDuration, TimeSpan burnTickInterval, float procChance)
        {
            _burnDamagePerTick = burnDamagePerTick;
            _burnDuration = burnDuration;
            _burnTickInterval = burnTickInterval;
            _procChance = procChance;
            _random = new Random();
        }

        /// <summary>
        /// Called when an attack hits a target. Determines whether to apply the burn condition.
        /// </summary>
        /// <param name="attacker">The entity performing the attack.</param>
        /// <param name="target">The entity being hit by the attack.</param>
        /// <param name="attackData">Additional data about the attack.</param>
        public override void OnHit(Entity attacker, Entity target, AttackData attackData)
        {
            base.OnHit(attacker, target, attackData);

            if (ShouldProc())
            {
                ApplyBurn(attacker, target);
            }
            else
            {
                Logger.Debug($"BurningProcEffect did not proc on {target.Name}.");
            }
        }

        /// <summary>
        /// Determines whether the proc effect should trigger based on the proc chance.
        /// </summary>
        /// <returns>True if the proc should trigger; otherwise, false.</returns>
        private bool ShouldProc()
        {
            double roll = _random.NextDouble();
            bool proc = roll <= _procChance;
            Logger.Debug($"BurningProcEffect Roll: {roll} <= {_procChance} => Proc: {proc}");
            return proc;
        }

        /// <summary>
        /// Applies the burn condition to the target.
        /// </summary>
        /// <param name="attacker">The entity performing the attack.</param>
        /// <param name="target">The entity being hit by the attack.</param>
        private void ApplyBurn(Entity attacker, Entity target)
        {
            var burnCondition = new BurnCondition(_burnDamagePerTick, _burnDuration, _burnTickInterval, attacker);
            target.ApplyCondition(burnCondition);
            Logger.Info($"BurningProcEffect applied BurnCondition to {target.Name}: {_burnDamagePerTick} dmg/tick for {_burnDuration.TotalSeconds} seconds.");
        }
    }
}
