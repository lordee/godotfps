using Godot;

abstract public class TFClass
{
    
    protected int _health;
    protected int _armour;
    protected Weapon _weapon1;
    protected Weapon _weapon2;
    protected Weapon _weapon3;
    protected Weapon _weapon4;
    protected Ammunition _gren1;
    protected Ammunition _gren2;
    //public HandGrenadeManager HandGrenadeManager = new HandGrenadeManager();
    protected int _maxShells;
    protected int _maxNails;
    protected int _maxRockets;
    protected int _maxCells;
    protected int _maxGren1;
    protected int _maxGren2;

    public int Health {
        get {
            return _health;           
        }
    }
    public int Armour {
        get {
            return _armour;           
        }
    }
    public Weapon Weapon1 {
        get {
            return _weapon1;           
        }
    }
    public Weapon Weapon2 {
        get {
            return _weapon2;           
        }
    }
    public Weapon Weapon3 {
        get {
            return _weapon3;           
        }
    }
    public Weapon Weapon4 {
        get {
            return _weapon4;           
        }
    }
    public Ammunition Gren1 {
        get {
            return _gren1;           
        }   
    }
    public Ammunition Gren2 {
        get {
            return _gren2;           
        }   
    }
    public int MaxShells {
        get {
            return _maxShells;           
        }   
    }
    public int MaxNails {
        get {
            return _maxNails;           
        }   
    }
    public int MaxRockets {
        get {
            return _maxRockets;           
        }   
    }
    public int MaxCells {
        get {
            return _maxCells;           
        }   
    }
    public int MaxGren1 {
        get {
            return _maxGren1;           
        }   
    }
    public int MaxGren2 {
        get {
            return _maxGren2;           
        }   
    }

    public void SpawnWeapons(Node camera)
    {
        if (Weapon1 != null)
        {
            Weapon1.Spawn(camera, "Weapon1");
        }

        if (Weapon2 != null)
        {
            Weapon2.Spawn(camera, "Weapon2");
        }

        if (Weapon3 != null)
        {
            Weapon3.Spawn(camera, "Weapon3");
        }

        if (Weapon4 != null)
        {
            Weapon4.Spawn(camera, "Weapon4");
        }
    }
}
