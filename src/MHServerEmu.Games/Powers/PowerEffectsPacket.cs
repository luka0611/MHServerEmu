﻿using MHServerEmu.Core.VectorMath;
using MHServerEmu.Games.GameData.Prototypes;
using MHServerEmu.Games.Properties;

namespace MHServerEmu.Games.Powers
{
    // Base abstract class for PowerPayload and PowerResults
    // TODO: Pooling?
    public abstract class PowerEffectsPacket
    {
        public ulong PowerOwnerId { get; protected set; }
        public ulong UltimateOwnerId { get; protected set; }
        public ulong TargetId { get; protected set; }
        public Vector3 PowerOwnerPosition { get; protected set; }
        public PowerPrototype PowerPrototype { get; protected set; }
        public KeywordsMask KeywordsMask { get; protected set; }

        public PropertyCollection Properties { get; } = new();

        // long - TimeSpan?

        public virtual void Clear()
        {
            PowerOwnerId = default;
            UltimateOwnerId = default;
            TargetId = default;
            PowerOwnerPosition = default;
            PowerPrototype = default;
            KeywordsMask = default;

            Properties.Clear();
        }
    }
}
