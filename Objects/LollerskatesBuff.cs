using FireInTheHole.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FireInTheHole.Objects
{
    public class LollerskatesBuff : Pickup
    {
        public LollerskatesBuff(GameEngine engine) : base(engine)
        {
            Texture = engine.Content.Load<Texture2D>("s_pickup_lollerskates");
        }

        public override void Apply(Mole player)
        {
            player.SpeedBonus += 0.25f;
        }
    }
}