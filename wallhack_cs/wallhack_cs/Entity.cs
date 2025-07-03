using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace wallhack_cs
{
    public class Entity
    {
        public IntPtr pawnAddress { get; set; }
        public IntPtr controllerAddress { get; set; }
        public Vector3 origin { get; set; }
        public int team { get; set; }
        public uint health { get; set; }
        public uint lifeState { get; set; }
        public float distance { get; set; }
        public List<Vector3> bones {  get; set; }
        public List<Vector2> bones2d { get; set; }
        //public short currentWeaponIndex { get; set; }
        //public string currentWeaponName { get; set; }
    }

    public enum BonesIds
    {
        Waist = 0, // 0
        Neck = 5, // 1
        Head = 6, // 2
        ShoulderLeft = 8, // 3
        ForeLeft = 9, // 4
        HandLeft = 11, // 5
        ShoulderRight = 13, // 6
        ForeRight = 14, // 7
        HandRight = 16, // 8
        KneeLeft = 23, // 9
        FeetLeft = 24, // 10
        KneeRight = 26, // 11
        FeetRight = 27 // 12
    }

    //public enum Weapon
    //{
    //Deagle = 1,
    //Elite = 2,
    //Fiveseven = 3,
    //Glock = 4,
    //Ak47 = 7,
    //Aug = 8,
    //Awp = 9,
    //Famas = 10,
    //G3Sg1 = 11,
    //M249 = 14,
    //Mac10 = 17,
    //P90 = 19,
    //Ump45 = 24,
    //Xm1014 = 25,
    //Bizon = 26,
    //Mag7 = 27,
    //Negev = 28,
    //Sawedoff = 29,
    //Tec9 = 30,
    //Zeus = 31,
    //P2000 = 0,
    //Mp7 = 33,
    //Mp9 = 34,
    //Nova = 35,
    //P250 = 36,
    //Scar20 = 38,
    //Sg556 = 39,
    //Ssg08 = 40,
    //CtKnife = 42,
    //Flashbang = 43,
    //Hegrenade = 44,
    //Smokegrenade = 45,
    //Molotov = 46,
    //Decoy = 47,
    //Incgrenade = 48,
    //C4 = 49,
    //M4A4 = 16,
    //UspS = 61,
    //M4A1Silencer = 60,
    //Cz75A = 63,
    //Revolver = 64,
    //TKnife = 59
    //}

}
