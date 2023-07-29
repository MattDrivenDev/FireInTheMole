using FireInTheHole.Player;
using Microsoft.Xna.Framework.Graphics;

namespace FireInTheHole.Objects;

public class ExplosionBuff : Pickup
{
    public ExplosionBuff(GameEngine engine) : base(engine)
    {
        Texture = engine.Content.Load<Texture2D>("s_pickup_explosion");
    }

    public override void Apply(Mole player)
    {
        for (var i = 0; i < player.Dynamites.Count; i++)
        {
            player.Dynamites[i].Size += 1;
        }
    }
}