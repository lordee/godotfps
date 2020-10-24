public class Debuff
{
    public WEAPONTYPE Type;
    public float TimeLeft;
    public float NextThink;
    public float NextThinkInterval = 1;
    public Player Owner;
    public Player Attacker;

    public void ApplyDebuff()
    {
        switch (Type)
        {
            case WEAPONTYPE.CONCUSSION:
                ApplyConcussion();
                break;
            case WEAPONTYPE.FLASH:
                ApplyFlash();
                break;
            case WEAPONTYPE.SYRINGE:
                ApplySyringe();
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

    private void ApplySyringe()
    {
        // heals are taken care of in initial swing, so this is only ever damage
        // TODO - on player touch (so not this think function), spread disease
        Owner.TakeDamage(Attacker, Owner.GlobalTransform.origin, Syringe.BioDamage);
    }
}