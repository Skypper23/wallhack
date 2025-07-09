using System.Collections.Generic;
using System.Numerics;

namespace wallhack_cs
{
    public class Entity
    {
        public IntPtr pawnAddress;
        public IntPtr controllerAddress;
        public Vector3 origin;
        public int team;
        public string name;
        public uint health;
        public uint lifeState;
        public float distance;
        public List<Vector3> bones;
        public List<Vector2> bones2d;
        public bool spotted;
    }

    public enum BonesIds
    {
        Waist = 0,
        Neck = 5,
        Head = 6,
        ShoulderLeft = 8,
        ForeLeft = 9,
        HandLeft = 11,
        ShoulderRight = 13,
        ForeRight = 14,
        HandRight = 16,
        KneeLeft = 23,
        FeetLeft = 24,
        KneeRight = 26,
        FeetRight = 27
    }
}