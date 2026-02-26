using UnityEngine;
using Verse;

namespace BoundWeapon
{
    public static class BW_Icons
    {
        public static readonly Texture2D Manager = Load("UI/Commands/BW_Manager");
        public static readonly Texture2D Bind = Load("UI/Commands/BW_Bind");
        public static readonly Texture2D Clear = Load("UI/Commands/BW_Clear");

        static Texture2D Load(string path)
        {
            Texture2D t = ContentFinder<Texture2D>.Get(path, false);
            return t ?? BaseContent.ClearTex;
        }
    }
}