using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace wallhack_cs
{
    public class Renderer : Overlay
    {
        public Vector2 overlaySize = new Vector2(1920, 1080); // Tamanho da tela
        Vector2 windowLocation = new Vector2(0, 0);
        public List<Entity> entitiesCopy = new List<Entity>();
        public Entity localPlayerCopy = new Entity();
        //ImDrawListPtr drawListPtr;
        public bool esp = true;
        public bool drawBoxes = true;
        public bool drawSkeleton = false;
        public bool healthBar = true;
        public bool teammate = false;
        //public bool weapon = true;
        float boneThickness = 4;
        Vector4 teamColor = new Vector4(1f, 0.2431f, 0.1882f, 1.0f);
        Vector4 enemyColor = new Vector4(1f, 1f, 1f, 1f);
        Vector4 textColor = new Vector4(1f, 1f, 1f, 1f);

        protected override void Render()
        {   
            ImGui.Begin("ESP Menu");
            ImGui.Checkbox("ESP", ref esp);
            ImGui.Checkbox("On teammate", ref teammate);
            ImGui.Checkbox("Draw Skeleton", ref drawSkeleton);
            ImGui.Checkbox("Draw Boxes", ref drawBoxes);
            ImGui.Checkbox("Health Bar", ref healthBar);
            //ImGui.Checkbox("Weapon names", ref weapon);
            ImGui.SliderFloat("Bone Thickness", ref boneThickness, 4, 500);
            if (teammate)
            {
                if (ImGui.CollapsingHeader("Team Lines Color"))
                {
                    ImGui.ColorPicker4("##teamcolor", ref teamColor);
                }
            }
            if (ImGui.CollapsingHeader("Enemy Lines Color"))
                ImGui.ColorPicker4("##enemycolor", ref enemyColor);
            if (healthBar)
            {
                if (ImGui.CollapsingHeader("Health text color"))
                    ImGui.ColorPicker4("##textcolor", ref textColor);
            }

            ImGui.End();

            if (esp)
            {
                DrawOverlay();
                DrawSkeletons();
                ImGui.End();
            }
        }

        void DrawSkeletons()
        {
            if (entitiesCopy == null || entitiesCopy.Count == 0)
                return;

            // Criar uma cópia do array para evitar modificação da lista original durante a iteração
            Entity[] tempEntities = entitiesCopy.ToArray();

            var drawListPtr = ImGui.GetWindowDrawList();
            uint uintColor;

            foreach (Entity entity in tempEntities)
            {
                if (entity == null)
                    continue;

                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;

                foreach (var bone in entity.bones2d)
                {
                    minX = MathF.Min(minX, bone.X);
                    minY = MathF.Min(minY, bone.Y);
                    maxX = MathF.Max(maxX, bone.X);
                    maxY = MathF.Max(maxY, bone.Y);
                }

                Vector2 topLeft = new Vector2(minX - 7, minY - 12);
                Vector2 bottomRight = new Vector2(maxX, maxY);

                uintColor = localPlayerCopy.team == entity.team
                            ? ImGui.ColorConvertFloat4ToU32(teamColor)
                            : ImGui.ColorConvertFloat4ToU32(enemyColor);

                if (entity.bones2d[2].X > 0 && entity.bones2d[2].Y > 0 &&
                    entity.bones2d[2].X < overlaySize.X && entity.bones2d[2].Y < overlaySize.Y)
                {
                    float currentBoneThickness = boneThickness;
                    if (entity.distance >= 0)
                        currentBoneThickness = boneThickness;
                    if (entity.distance >= 1)
                        currentBoneThickness = boneThickness / entity.distance;

                    if ((drawSkeleton && entity.team != localPlayerCopy.team) || (drawSkeleton && teammate))
                    {
                        drawListPtr.AddLine(entity.bones2d[1], entity.bones2d[2], uintColor, currentBoneThickness);
                        drawListPtr.AddLine(entity.bones2d[1], entity.bones2d[3], uintColor, currentBoneThickness);
                        drawListPtr.AddLine(entity.bones2d[1], entity.bones2d[6], uintColor, currentBoneThickness);
                        drawListPtr.AddLine(entity.bones2d[3], entity.bones2d[4], uintColor, currentBoneThickness);
                        drawListPtr.AddLine(entity.bones2d[6], entity.bones2d[7], uintColor, currentBoneThickness);
                        drawListPtr.AddLine(entity.bones2d[4], entity.bones2d[5], uintColor, currentBoneThickness);
                        drawListPtr.AddLine(entity.bones2d[7], entity.bones2d[8], uintColor, currentBoneThickness);
                        drawListPtr.AddLine(entity.bones2d[1], entity.bones2d[0], uintColor, currentBoneThickness);
                        drawListPtr.AddLine(entity.bones2d[0], entity.bones2d[9], uintColor, currentBoneThickness);
                        drawListPtr.AddLine(entity.bones2d[0], entity.bones2d[11], uintColor, currentBoneThickness);
                        drawListPtr.AddLine(entity.bones2d[9], entity.bones2d[10], uintColor, currentBoneThickness);
                        drawListPtr.AddLine(entity.bones2d[11], entity.bones2d[12], uintColor, currentBoneThickness);
                        drawListPtr.AddCircle(entity.bones2d[2], 3 + currentBoneThickness, uintColor);
                    }

                    if ((drawBoxes && entity.team != localPlayerCopy.team) || (drawBoxes && teammate))
                    {
                        drawListPtr.AddRect(topLeft, bottomRight, uintColor, 0f, ImDrawFlags.None, currentBoneThickness);
                    }

                    if ((healthBar && entity.team != localPlayerCopy.team) || (healthBar && teammate))
                    {
                        float healthPercent = Math.Clamp(entity.health, 0, 100) / 100f;
                        float barThickness = currentBoneThickness + 2.4f;
                        float barHeight = bottomRight.Y - topLeft.Y;
                        float barTopY = topLeft.Y;
                        float barBottomY = bottomRight.Y;
                        float barX = topLeft.X - 6f;

                        Vector4 healthColor = new Vector4(1.0f - healthPercent, healthPercent, 0f, 1f);
                        uint healthColorU32 = ImGui.ColorConvertFloat4ToU32(healthColor);
                        uint black = ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1f));

                        drawListPtr.AddRectFilled(
                            new Vector2(barX, barTopY),
                            new Vector2(barX + barThickness, barBottomY),
                            black
                        );

                        float filledTopY = barBottomY - (barHeight * healthPercent);

                        drawListPtr.AddRectFilled(
                            new Vector2(barX, filledTopY),
                            new Vector2(barX + barThickness, barBottomY),
                            healthColorU32
                        );

                        if (entity.health < 100 && entity.health > 0)
                        {
                            string healthText = $"{entity.health}";

                            float fontScale = Math.Clamp(barHeight / 60f, 0.4f, 1.0f);

                            Vector2 textSize = ImGui.CalcTextSize(healthText) * fontScale;

                            Vector2 textPos = new Vector2(
                                barX + (barThickness / 2f) - (textSize.X / 2f),
                                barTopY - textSize.Y - 2f
                            );

                            drawListPtr.AddText(
                                textPos,
                                ImGui.ColorConvertFloat4ToU32(textColor),
                                healthText
                            );
                        }
                    }
                }
            }
        }


        void DrawOverlay()
        {
            ImGui.SetNextWindowSize(overlaySize);
            ImGui.SetNextWindowPos(windowLocation);
            ImGui.Begin("Overlay", ImGuiWindowFlags.NoDecoration |
                      ImGuiWindowFlags.NoBackground |
                      ImGuiWindowFlags.NoBringToFrontOnFocus |
                      ImGuiWindowFlags.NoMove |
                      ImGuiWindowFlags.NoInputs |
                      ImGuiWindowFlags.NoCollapse |
                      ImGuiWindowFlags.NoScrollbar |
                      ImGuiWindowFlags.NoScrollWithMouse);
        }
    }
}
