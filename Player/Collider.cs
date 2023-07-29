using System.Collections.Generic;
using MonoGame.Extended;

namespace FireInTheHole.Player;

public class Collider
{
    public Collider(GameEngine engine)
    {
        Engine = engine;
    }

    public void RegisterForCollision(Mole mole)
    {
        Moles.Add(mole);
    }

    public GameEngine Engine { get; init; }

    public List<Mole> Moles { get; init; } = new();

    public bool HasCollision(Mole mole, RectangleF bounds)
    {
        foreach (var otherMole in Moles)
        {
            if (otherMole == mole || otherMole.IsDead)
            {
                continue;
            }

            if (bounds.Intersects(otherMole.Bounds))
            {
                return true;
            }
        }

        return false;
    }
}