using System;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller<GameInstaller>
{
    public override void InstallBindings()
    {
        InstallMapGen();
    }

    private void InstallMapGen()
    {
        // MapGen is bound using ZenjectBinding component object in scene.

        Container.Bind<IMapRoomFactory>().To<MapRoomFactory>().AsSingle();
        Container.Bind<IPhysicalMapRoomTools>().To<PhysicalMapRoomTools>().AsSingle();
    }
}