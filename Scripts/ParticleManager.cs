using Godot;
using System;

public class ParticleManager : Node
{
    string _puffResource = "res://Scenes/Weapons/Puff.tscn";
    string _bloodResource = "res://Scenes/Weapons/BloodPuff.tscn";
    PackedScene _puffScene;
    PackedScene _bloodScene;
    Game _game;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _puffScene = (PackedScene)ResourceLoader.Load(_puffResource);
        _bloodScene = (PackedScene)ResourceLoader.Load(_bloodResource);
        _game = GetTree().Root.GetNode("Game") as Game;
    }

    public void MakePuff(PUFFTYPE puff, Vector3 pos, Node puffOwner)
    {
        Particles puffPart = null;
        switch (puff)
        {
            case PUFFTYPE.BLOOD:
                puffPart = (Particles)_bloodScene.Instance();
            break;
            case PUFFTYPE.PUFF:
                puffPart = (Particles)_puffScene.Instance();
            break;
        }

        puffPart.Translation = pos;
        if (puffOwner != null)
        {
            puffOwner.AddChild(puffPart);
        }
        else
        {
            this.AddChild(puffPart);
        }
        
        puffPart.Emitting = true;

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
    }
}
