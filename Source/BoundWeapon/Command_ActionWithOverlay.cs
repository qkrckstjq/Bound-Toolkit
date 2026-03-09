using UnityEngine;
using Verse;

namespace BoundWeapon
{
    public class Command_ActionWithOverlay : Command_Action
    {
        public Texture2D overlayTex;
        public float overlayScale = 0.45f;
        public Color overlayColor = Color.white;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUI(topLeft, maxWidth, parms);

            if (overlayTex == null) return result;

            var rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            const float iconSize = 40f;

            var iconRect = new Rect(
                rect.x + (rect.width - iconSize) * 0.5f,
                rect.y + 3f,
                iconSize,
                iconSize
            );

            var size = iconSize * overlayScale;
            var overlayRect = new Rect(
                iconRect.x + (iconRect.width - size) * 0.5f,
                iconRect.y + (iconRect.height - size) * 0.5f + 14f,
                size,
                size
            );

            var prev = GUI.color;
            GUI.color = overlayColor;
            GUI.DrawTexture(overlayRect, overlayTex);
            GUI.color = prev;

            return result;
        }
    }
}