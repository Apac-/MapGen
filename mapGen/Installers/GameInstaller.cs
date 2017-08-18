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
        // MapGen is bound using a ZenjectBinding component object in scene.

        // Singles
        Container.Bind<IMapRoomFactory>().To<MapRoomFactory>().AsSingle();

        // Transients
        Container.Bind<IMapDataFactory>().To<MapDataFactory>().AsTransient();

        Container.Bind<IHallwayFactory>().To<HallwayFactory>().AsTransient();

        Container.Bind<IMapRoomTools>().To<MapRoomTools>().AsTransient();

        Container.Bind<IPhysicalMapRoomTools>().To<PhysicalMapRoomTools>().AsTransient();

        Container.Bind<IPointTriangulation>().To<DelaunayGrapher>().AsTransient();
    }
}