using System;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Events;
using MHServerEmu.Core.Logging;

namespace MHServerEmu.Games.Conditions
{
    /// <summary>
    /// Represents a burn condition that deals damage over time to an entity.
    /// </summary>
    public class BurnCondition : Condition
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public float DamagePerTick { get; private set; }
        public TimeSpan Duration { get; private set; }
        public TimeSpan TickInterval { get; private set; }

        private DateTime _startTime;
        private readonly Entity _owner;

        public BurnCondition(float damagePerTick, TimeSpan duration, TimeSpan tickInterval, Entity owner)
        {
            DamagePerTick = damagePerTick;
            Duration = duration;
            TickInterval = tickInterval;
            _owner = owner;
        }

        /// <summary>
        /// Applies the burn condition to the target entity and schedules the first damage tick.
        /// </summary>
        /// <param name="target">The entity to apply the burn condition to.</param>
        public override void Apply(Entity target)
        {
            base.Apply(target);
            _startTime = DateTime.UtcNow;
            ScheduleNextTick(target);
            Logger.Info($"Applied BurnCondition to {target.Name}: {DamagePerTick} dmg/tick for {Duration.TotalSeconds} seconds.");
        }

        /// <summary>
        /// Schedules the next damage tick event.
        /// </summary>
        /// <param name="target">The target entity.</param>
        private void ScheduleNextTick(Entity target)
        {
            var scheduler = target.Game.GameEventScheduler;
            if (scheduler == null)
            {
                Logger.Warn("GameEventScheduler is null. Cannot schedule BurnCondition ticks.");
                return;
            }

            var burnTickEvent = new BurnTickEvent(this, target);
            scheduler.ScheduleEvent(burnTickEvent, TickInterval, null);
        }

        /// <summary>
        /// Applies damage to the target entity.
        /// </summary>
        /// <param name="target">The entity to damage.</param>
        private void ApplyDamage(Entity target)
        {
            if (target.IsAlive)
            {
                target.ApplyDamage(DamagePerTick, DamageType.Fire, _owner);
                Logger.Debug($"BurnCondition: Applied {DamagePerTick} Fire damage to {target.Name}.");
            }
        }

        /// <summary>
        /// Updates the condition, checking for expiration.
        /// </summary>
        /// <param name="target">The target entity.</param>
        public override void Update(Entity target)
        {
            base.Update(target);

            var elapsed = DateTime.UtcNow - _startTime;
            if (elapsed >= Duration)
            {
                Remove(target);
                Logger.Info($"BurnCondition expired on {target.Name}.");
            }
        }

        /// <summary>
        /// Represents a scheduled event for applying burn damage ticks.
        /// </summary>
        private class BurnTickEvent : ScheduledEvent
        {
            private BurnCondition _burnCondition;
            private Entity _target;

            public BurnTickEvent(BurnCondition burnCondition, Entity target)
            {
                _burnCondition = burnCondition;
                _target = target;
            }

            /// <summary>
            /// Triggered when the scheduled event occurs.
            /// Applies damage and reschedules if necessary.
            /// </summary>
            /// <returns>True to keep the event active, false to remove it.</returns>
            public override bool OnTriggered()
            {
                if (_burnCondition == null || _target == null || !_target.IsAlive)
                {
                    return false; // Do not reschedule
                }

                _burnCondition.ApplyDamage(_target);

                // Check if duration has been exceeded
                var elapsed = DateTime.UtcNow - _burnCondition._startTime;
                if (elapsed + _burnCondition.TickInterval >= _burnCondition.Duration)
                {
                    _burnCondition.Remove(_target);
                    return false; // Do not reschedule
                }

                // Schedule next tick
                _burnCondition.ScheduleNextTick(_target);
                return true;
            }
        }
    }
}
