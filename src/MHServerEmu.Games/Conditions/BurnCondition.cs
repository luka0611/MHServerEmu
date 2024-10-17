using System;
using MHServerEmu.Games.Entities;
using MHServerEmu.Games.Events;
using MHServerEmu.Core.Logging;

namespace MHServerEmu.Games.Conditions
{
    public class BurnCondition : Condition
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public float DamagePerTick { get; private set; }
        public TimeSpan Duration { get; private set; }
        public TimeSpan TickInterval { get; private set; }

        private int _ticksApplied;
        private DateTime _startTime;

        public BurnCondition(float damagePerTick, TimeSpan duration, TimeSpan tickInterval)
        {
            DamagePerTick = damagePerTick;
            Duration = duration;
            TickInterval = tickInterval;
            _ticksApplied = 0;
        }

        public override void Apply(Entity target)
        {
            base.Apply(target);
            _startTime = DateTime.UtcNow;
            ScheduleNextTick(target);
            Logger.Info($"Applied BurnCondition to {target.Name}: {DamagePerTick} dmg/tick for {Duration.TotalSeconds} seconds.");
        }

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

        private void ApplyDamage(Entity target)
        {
            if (target.IsAlive)
            {
                target.ApplyDamage(DamagePerTick, DamageType.Fire, Owner);
                Logger.Debug($"BurnCondition: Applied {DamagePerTick} Fire damage to {target.Name}.");
                _ticksApplied++;
            }
        }

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

        private class BurnTickEvent : ScheduledEvent
        {
            private BurnCondition _burnCondition;
            private Entity _target;

            public BurnTickEvent(BurnCondition burnCondition, Entity target)
            {
                _burnCondition = burnCondition;
                _target = target;
            }

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
