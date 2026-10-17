using UnityEngine;

public static class GamePhysics
{
    private static PhysicsMaterial2D material;
    public static PhysicsMaterial2D NoFriction
    {
        get
        {
            if (material == null)
                material = new PhysicsMaterial2D("Arena sem atrito") { friction = 0f, bounciness = 0f };
            return material;
        }
    }
}
