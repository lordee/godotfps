public class Soldier : TFClass
{
    public Soldier()
    {
        _weapon1 = new RocketLauncher();
        _weapon2 = new SuperShotgun();
        _weapon3 = new Shotgun();
        _weapon4 = new Axe();
        _gren1 = Ammunition.FragGrenade;
        _gren2 = Ammunition.NailGrenade;
        _health = 100;
        _armour = 200;
        _maxShells = 100;
        _maxNails = 50;
        _maxRockets = 50;
        _maxCells = 50;
        _maxGren1 = 4;
        _maxGren2 = 200;
    }
}