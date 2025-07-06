using ClickableTransparentOverlay;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace wallhack_cs
{
    public class Renderer : Overlay
    {
        public Vector2 overlaySize = new Vector2(1920, 1080);
        public List<Entity> entitiesCopy = new List<Entity>();
        public Entity localPlayerCopy = new Entity();

        // Configurações
        public bool esp = true;
        public bool drawBoxes = true;
        public bool drawSkeleton = true;
        public bool healthBar = true;
        public bool teammate = false;
        public bool antiflash = true;
        public bool trigger = false;
        public bool enableVisibility = true;

        // Estilos
        public float boneThickness = 4;
        public Vector4 teamColor = new Vector4(1f, 0.2431f, 0.1882f, 1.0f);
        public Vector4 enemyColor = new Vector4(1f, 1f, 1f, 1f);
        public Vector4 textColor = new Vector4(1f, 1f, 1f, 1f);
        public Vector4 skeletonColor = new Vector4(0.4941f, 0f, 1f, 1f);
        public Vector4 visibleColor = new Vector4(0.0117f, 0.8588f, 1.0f, 1.0f);

        protected override void Render()
        {
            RenderMenu();
            if (esp) RenderOverlay();
        }

        private void RenderMenu()
        {
            ImGui.Begin("Nebula Guard");
            ImGui.Text("ESP");
            ImGui.Checkbox("Wallhack", ref esp);
            ImGui.Checkbox("On teammate", ref teammate);
            ImGui.Checkbox("Draw Skeleton", ref drawSkeleton);
            ImGui.Checkbox("Draw Boxes", ref drawBoxes);
            ImGui.Checkbox("Visibility", ref enableVisibility);
            ImGui.Checkbox("Health Bar", ref healthBar);

            ImGui.Text("Utils");
            ImGui.Checkbox("AntiFlash", ref antiflash);
            ImGui.Checkbox("TriggerBot", ref trigger);

            ImGui.SliderFloat("Line Thickness", ref boneThickness, 4, 500);

            if (teammate && ImGui.CollapsingHeader("Team Lines Color"))
                ImGui.ColorPicker4("##teamcolor", ref teamColor);

            if (ImGui.CollapsingHeader("Enemy Lines Color"))
                ImGui.ColorPicker4("##enemycolor", ref enemyColor);

            if (healthBar && ImGui.CollapsingHeader("Health text color"))
                ImGui.ColorPicker4("##textcolor", ref textColor);

            if (drawSkeleton && ImGui.CollapsingHeader("Skeleton line color"))
                ImGui.ColorPicker4("##textcolor", ref skeletonColor);

            if (enableVisibility && ImGui.CollapsingHeader("Visibility Color"))
                ImGui.ColorPicker4("##textcolor", ref visibleColor);

            ImGui.End();
        }

        private void RenderOverlay()
        {
            ImGui.SetNextWindowSize(overlaySize);
            ImGui.SetNextWindowPos(Vector2.Zero);

            ImGui.Begin("Overlay",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoInputs |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse);

            if (entitiesCopy.Count > 0)
            {
                var drawList = ImGui.GetWindowDrawList();
                foreach (var entity in entitiesCopy)
                {
                    if (entity == null) continue;
                    DrawEntity(drawList, entity);
                }
            }

            ImGui.End();
        }

        private void DrawEntity(ImDrawListPtr drawList, Entity entity)
        {
            if (entity.bones2d == null || entity.bones2d.Count < 13) return;

            // Calcular bounding box
            Vector2 min = new Vector2(float.MaxValue), max = new Vector2(float.MinValue);
            foreach (var bone in entity.bones2d)
            {
                min = Vector2.Min(min, bone);
                max = Vector2.Max(max, bone);
            }

            Vector2 topLeft = new Vector2(min.X - 7, min.Y - 12);
            Vector2 bottomRight = max;

            // Determinar cor
            uint color = GetEntityColor(entity);

            // Desenhar elementos
            float thickness = CalculateThickness(entity.distance);

            if (drawSkeleton && ShouldDraw(entity))
                DrawSkeleton(drawList, entity, thickness);

            if (drawBoxes && ShouldDraw(entity))
                drawList.AddRect(topLeft, bottomRight, color, 0f, ImDrawFlags.None, thickness);

            if (healthBar && ShouldDraw(entity))
                DrawHealthBar(drawList, entity, topLeft, bottomRight, thickness);
        }

        private uint GetEntityColor(Entity entity)
        {
            if (localPlayerCopy.team == entity.team)
                return ImGui.ColorConvertFloat4ToU32(teamColor);

            if (enableVisibility)
                return ImGui.ColorConvertFloat4ToU32(entity.spotted ? visibleColor : enemyColor);

            return ImGui.ColorConvertFloat4ToU32(enemyColor);
        }

        private float CalculateThickness(float distance)
        {
            return distance <= 0 ? boneThickness :
                   distance <= 1 ? boneThickness :
                   boneThickness / distance;
        }

        private bool ShouldDraw(Entity entity)
        {
            return entity.team != localPlayerCopy.team || teammate;
        }

        private void DrawSkeleton(ImDrawListPtr drawList, Entity entity, float thickness)
        {
            uint color = ImGui.ColorConvertFloat4ToU32(skeletonColor);
            var bones = entity.bones2d;

            // Cabeça e pescoço
            drawList.AddLine(bones[1], bones[2], color, thickness);

            // Torso e braços
            drawList.AddLine(bones[1], bones[3], color, thickness);
            drawList.AddLine(bones[1], bones[6], color, thickness);
            drawList.AddLine(bones[3], bones[4], color, thickness);
            drawList.AddLine(bones[6], bones[7], color, thickness);

            // Mãos
            drawList.AddLine(bones[4], bones[5], color, thickness);
            drawList.AddLine(bones[7], bones[8], color, thickness);

            // Pernas
            drawList.AddLine(bones[1], bones[0], color, thickness);
            drawList.AddLine(bones[0], bones[9], color, thickness);
            drawList.AddLine(bones[0], bones[11], color, thickness);
            drawList.AddLine(bones[9], bones[10], color, thickness);
            drawList.AddLine(bones[11], bones[12], color, thickness);

            // Cabeça
            drawList.AddCircle(bones[2], 3 + thickness, color);
        }

        private void DrawHealthBar(ImDrawListPtr drawList, Entity entity, Vector2 topLeft, Vector2 bottomRight, float thickness)
        {
            float healthPercent = Math.Clamp(entity.health, 0, 100) / 100f;
            float barHeight = bottomRight.Y - topLeft.Y;
            float barX = topLeft.X - 6f;
            float barThickness = thickness + 2.4f;

            // Fundo
            drawList.AddRectFilled(
                new Vector2(barX, topLeft.Y),
                new Vector2(barX + barThickness, bottomRight.Y),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1f))
            );

            // Barra de vida
            float filledTopY = bottomRight.Y - (barHeight * healthPercent);
            drawList.AddRectFilled(
                new Vector2(barX, filledTopY),
                new Vector2(barX + barThickness, bottomRight.Y),
                ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f - healthPercent, healthPercent, 0f, 1f))
            );

            // Texto de vida
            if (entity.health < 100 && entity.health > 0)
            {
                string healthText = entity.health.ToString();
                float fontScale = Math.Clamp(barHeight / 60f, 0.4f, 1.0f);
                Vector2 textSize = ImGui.CalcTextSize(healthText) * fontScale;
                Vector2 textPos = new Vector2(barX + (barThickness / 2f) - (textSize.X / 2f), topLeft.Y - textSize.Y - 2f);

                drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(textColor), healthText);
            }
        }
    }
}