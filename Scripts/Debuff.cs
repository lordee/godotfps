public class Debuff
{
    public WEAPON Type;
    public float TimeLeft;
    public float NextThink;
    public Player Owner;
    public Player Attacker;

    public void ApplyDebuff()
    {
        switch (Type)
        {
            case WEAPON.CONCUSSION:
                ApplyConcussion();
                break;
            case WEAPON.FLASH:
                ApplyFlash();
                break;
        }
    }

    private void ApplyConcussion()
    {
        // TODO - figure out what effect we want
    }

    private void ApplyFlash()
    {
        // TODO - shaders changing in 4.0 I believe
    }
}