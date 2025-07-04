using Swed64;
using System.Numerics;
using wallhack_cs;

Swed swed = new Swed("cs2");

IntPtr client = swed.GetModuleBase("client.dll");

Reader reader = new Reader(swed);

Renderer renderer = new Renderer();
renderer.Start().Wait();

List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();

Vector2 screen = new Vector2(1920, 1080);

renderer.overlaySize = screen;

object renderLock = new object();

while (true)
{
    entities.Clear();
    // Console.Clear(); // Remova ou comente esta linha

    IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
    localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vOldOrigin);
    

    for(int i = 0;i < 64; i++)
    {
        if(listEntry == IntPtr.Zero)
            continue;
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);
        
        if(currentController == IntPtr.Zero)
            continue;
        int pawnHandle = swed.ReadInt(currentController, Offsets.m_hPlayerPawn);
        
        if(pawnHandle == 0)
            continue;

        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);

        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));

        if(currentPawn == localPlayer.pawnAddress)
            continue;

        //IntPtr currentWeapon = swed.ReadPointer(currentPawn, Offsets.m_pClippingWeapon);

        //short weaponDefinitionIndex = swed.ReadShort(currentWeapon, Offsets.m_AttributeManager + Offsets.m_Item + Offsets.m_iItemDefinitionIndex);

        //if(weaponDefinitionIndex == -1)
            //continue;

        IntPtr sceneNode = swed.ReadPointer(currentPawn, Offsets.m_pGameSceneNode);
        IntPtr boneMatrix = swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);

        ViewMatrix viewMatrix = reader.readMatrix(client + Offsets.dwViewMatrix);

        int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        uint lifeState = swed.ReadUInt(currentPawn, Offsets.m_lifeState);

        if(lifeState != 256)
            continue;

        uint health = swed.ReadUInt(currentPawn, Offsets.m_iHealth);
        if (health > 100 || health < 0)
            continue;

        Entity entity = new Entity();

        entity.pawnAddress = currentPawn;
        //entity.currentWeaponIndex = weaponDefinitionIndex;
        //entity.currentWeaponName = Enum.GetName(typeof(Weapon), weaponDefinitionIndex) ?? "Unknown Weapon";
        entity.controllerAddress = currentController;
        entity.team = team;
        entity.health = health;
        entity.lifeState = lifeState;
        entity.origin = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin); // position
        entity.distance = Vector3.Distance(entity.origin, localPlayer.origin);
        entity.bones = reader.ReadBones(boneMatrix);
        entity.bones2d = reader.ReadBones2d(entity.bones, viewMatrix, screen);

        entities.Add(entity);

        //Console.ForegroundColor = ConsoleColor.Green;

        //if(team != localPlayer.team)
        //{
        //Console.ForegroundColor= ConsoleColor.Red;
        //}

        //Console.ResetColor();

    }
    lock (renderLock)
    {
        renderer.entitiesCopy = entities.ToList();
        renderer.localPlayerCopy = localPlayer;
    }
    
    Thread.Sleep(2);
}