# MapGen

2d Map generator for tile based games built in Unity3d. 

### Prerequisites

Unity 3d (MapGen was created with version 2017.1.0f3)
```
https://unity3d.com/
```

Zenject
```
https://github.com/modesttree/Zenject
or
https://www.assetstore.unity3d.com/en/#!/content/17758
```

Point Triangulation
```
https://github.com/adamgit/Unity-delaunay
This is outdated but works fine in this instance. Replace as required.
```

### Installing

Import MapGen into Unity3D project window.
```
Drag MapGen (folder) into Unity project window under Scripts
```

Create new SceneContext object in scene
```
Right click on Hierarchy > Zenject > Scene Context
Add GameInstaller to SceneContext
Drag added GameInstaller to "Installers" collection on Scene Context
```

Create Physical helper room prefab
```
Create empty game object in scene.

Add Rigidbody 2D
  Freeze Rotation Z
  
Add Box Collider 2D
Add MapRoomHolder (script)

Drag newly created object to Prefab folder in Project window.
Delete from scene.
```

Create MapGen
```
Create empty game object in scene.

Add MapGen(Script)
  Drag Physical helper room prefab (Created above) onto "Physical Room" property.
  Drag a MapSettings SO (Located @ MapGen>Scriptable Objects>MapSettings) onto "Map Settings" property.

Add MapGenVisualDebugger(Script)

Add ZenjectBinding(script)
  Drag MapGen(script) on same game object to Components.
```

## Built With

* [ZenJect](https://github.com/modesttree/Zenject) - Dependency injection for Unity3D
* [Unity-delaunay](https://github.com/adamgit/Unity-delaunay) - Point Triangulation
