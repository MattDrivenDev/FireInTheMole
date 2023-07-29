using System.Linq;
using FireInTheHole.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FireInTheHole.Objects
{
    public class DynamiteBuff : Pickup
    {
        public DynamiteBuff(GameEngine engine) : base(engine)
        {
            Texture = engine.Content.Load<Texture2D>("s_pickup_dynamite");
        }

        public override void Apply(Mole player)
        {
            player.Dynamites.Add(
                new Dynamite(_engine, player)
                {                    
                    Position = new Vector2(-100, -100),
                    FuseLit = false,
                    Size = player.Dynamites.First().Size
                });
        }
    }
}