using Godot;
using System;

public class ParticleManager : Node
{
    string _puffResource = "res://Scenes/Weapons/Puff.tscn";
    string _bloodResource = "res://Scenes/Weapons/BloodPuff.tscn";
    string _flamethrowerResource = "res://Scenes/Weapons/Flame.tscn";

    PackedScene _puffScene;
    PackedScene _bloodScene;
    PackedScene _flamethrowerScene;
    Game _game;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _puffScene = (PackedScene)ResourceLoader.Load(_puffResource);
        _bloodScene = (PackedScene)ResourceLoader.Load(_bloodResource);
        _flamethrowerScene = (PackedScene)ResourceLoader.Load(_flamethrowerResource);
        _game = GetTree().Root.GetNode("Game") as Game;
    }

    public Particles MakePuff(PARTICLE puff, Vector3 pos, Node puffOwner)
    {
        Particles puffPart = null;
        switch (puff)
        {
            case PARTICLE.BLOOD:
                puffPart = (Particles)_bloodScene.Instance();
            break;
            case PARTICLE.PUFF:
                puffPart = (Particles)_puffScene.Instance();
            break;
        }

        if (puffOwner != null)
        {
            puffOwner.AddChild(puffPart);
        }
        else
        {
            this.AddChild(puffPart);
        }

        Transform t = puffPart.GlobalTransform;
        t.origin = pos;
        puffPart.GlobalTransform = t;
        
        puffPart.Emitting = true;

        // FIXME - is this necessary? Clients could emulate based off of initial shoot cmds, moving to no origin sent each frame for projectiles etc
        if (IsNetworkMaster())
        {
            foreach (Peer p in _game.Network.PeerList)
            {
                if (p.ID != 1)
                {
                    _game.Network.SendParticle(p.ID, puff, pos, puffOwner);
                }
            }
        }

        return puffPart;
    }

    public Particles SpawnParticle(PARTICLE partType, Transform trans, Player owner)
    {
        Flame p = null;
        Vector3 adjust = new Vector3(0,0,0);
        switch (partType)
        {
            case PARTICLE.FLAMETHROWER:
                p = (Flame)_flamethrowerScene.Instance();
                p.PlayerOwner = owner;
                p.WeaponType = WEAPONTYPE.FLAMETHROWER;
                adjust = new Vector3(0, 0, -1.5f);
                break;
        }
        
        _game.World.ParticleManager.AddChild(p);
        p.Transform = trans;
        p.Translate(adjust);
        

        return p;
    }
}
