using Swed64;
using System.Numerics;
using wallhack_cs;

Swed swed = new Swed("cs2");
IntPtr client = swed.GetModuleBase("client.dll");
Reader reader = new Reader(swed);
Renderer renderer = new Renderer();
renderer.Start().Wait();

List<Entity> entities = new List<Entity>(64); // Pré-alocar capacidade
Entity localPlayer = new Entity();
Vector2 screen = new Vector2(1920, 1080);
renderer.overlaySize = screen;
object renderLock = new object();

while (true)
{
    entities.Clear();

    IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
    localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vOldOrigin);

    for (int i = 0; i < 64; i++)
    {
        if (listEntry == IntPtr.Zero) continue;

        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);
        if (currentController == IntPtr.Zero) continue;

        int pawnHandle = swed.ReadInt(currentController, Offsets.m_hPlayerPawn);
        if (pawnHandle == 0) continue;

        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));

        if (currentPawn == localPlayer.pawnAddress) continue;

        IntPtr sceneNode = swed.ReadPointer(currentPawn, Offsets.m_pGameSceneNode);
        IntPtr boneMatrix = swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);

        ViewMatrix viewMatrix = reader.readMatrix(client + Offsets.dwViewMatrix);

        int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        uint lifeState = swed.ReadUInt(currentPawn, Offsets.m_lifeState);
        if (lifeState != 256) continue;

        uint health = swed.ReadUInt(currentPawn, Offsets.m_iHealth);
        if (health > 100) continue;

        Entity entity = new Entity
        {
            pawnAddress = currentPawn,
            controllerAddress = currentController,
            team = team,
            spotted = swed.ReadBool(currentPawn, Offsets.m_entitySpottedState + Offsets.m_bSpotted),
            health = health,
            lifeState = lifeState,
            origin = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin)
        };

        entity.distance = Vector3.Distance(entity.origin, localPlayer.origin);
        entity.bones = reader.ReadBones(boneMatrix);
        entity.bones2d = reader.ReadBones2d(entity.bones, viewMatrix, screen);

        entities.Add(entity);
    }

    lock (renderLock)
    {
        renderer.entitiesCopy = new List<Entity>(entities); // Criar nova lista em vez de ToList()
        renderer.localPlayerCopy = localPlayer;
    }

    if (renderer.antiflash)
    {
        const int flashOffset = 0x13F8;
        float flashDuration = swed.ReadFloat(localPlayer.pawnAddress, flashOffset);
        if (flashDuration > 0)
        {
            swed.WriteFloat(localPlayer.pawnAddress, flashOffset, 0f);
        }
    }

    Thread.Sleep(2);
}