using ClickableTransparentOverlay;
using ClickableTransparentOverlay.Win32;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace wallhack_cs
{
    public class Renderer : Overlay
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        public Vector2 overlaySize = new Vector2(1920, 1080);
        public List<Entity> entitiesCopy = new List<Entity>();
        public Entity localPlayerCopy = new Entity();

        public bool esp = true;
        public bool drawBoxes = false;
        public bool drawSkeleton = true;
        public bool healthBar = true;
        public bool teammate = false;
        public bool antiflash = true;
        public bool trigger = false;
        public bool enableVisibility = true;
        public bool corner = true;
        public bool names = true;

        public float boneThickness = 4;
        public Vector4 teamColor = new Vector4(1f, 0.2431f, 0.1882f, 1.0f);
        public Vector4 enemyColor = new Vector4(1f, 1f, 1f, 1f);
        public Vector4 textColor = new Vector4(0f, 1f, 0f, 1f);
        public Vector4 skeletonColor = new Vector4(0.4941f, 0f, 1f, 1f);
        public Vector4 visibleColor = new Vector4(0.0117f, 0.8588f, 1.0f, 1.0f);
        public Vector4 namesColor = new Vector4(1f, 1f, 1f, 1f);

        public string KeyPressed = "";
        bool isButtonPressed = false;
        bool CapturInput = false;
        public int CaptureKey = -1;
        public ImGuiKey imKey = ImGuiKey.None;

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
            ImGui.Checkbox("Corner Box", ref corner);
            ImGui.Checkbox("Names", ref names); // Corrigido "Nmaes" para "Names"
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

            if (healthBar && ImGui.CollapsingHeader("Health Text color"))
                ImGui.ColorPicker4("##textcolor", ref textColor);

            if (drawSkeleton && ImGui.CollapsingHeader("Skeleton line color"))
                ImGui.ColorPicker4("##skeletoncolor", ref skeletonColor);

            if (enableVisibility && ImGui.CollapsingHeader("Visibility Color"))
                ImGui.ColorPicker4("##visiblecolor", ref visibleColor);
            if(names && ImGui.CollapsingHeader("Names Color"))
                ImGui.ColorPicker4("##textcolor", ref namesColor);

            if (drawBoxes)
                corner = false;
            if (corner)
                drawBoxes = false;

            if (trigger)
            {
                ImGui.Text("Trigger HotKey:");

                if (ImGui.Button("Press any key"))
                {
                    isButtonPressed = true;
                    CapturInput = true;
                    CaptureKey = -1;
                }

                if (isButtonPressed)
                {
                    if (CapturInput)
                    {
                        ImGui.Text("Press any key...");
                    }
                    else
                    {
                        if (CaptureKey != -1)
                            ImGui.Text("Hotkey Input: " + ImGui.GetKeyName(imKey));
                        else
                            ImGui.Text("Hotkey Input: None");
                    }
                }

                if (isButtonPressed && CapturInput)
                {
                    for (int vk = 0; vk < 256; vk++)
                    {
                        if ((GetAsyncKeyState(vk) & 0x8000) != 0)
                        {
                            CaptureKey = vk;
                            CapturInput = false;
                            KeyPressed = GlobalKey.GlobalKeyDetector.VKCodeToName(vk);
                            imKey = GlobalKey.GlobalKeyDetector.VKCodeToImGuiKey(vk);
                            break;
                        }
                    }
                }
            }

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

            var drawList = ImGui.GetWindowDrawList();

            foreach (var entity in entitiesCopy)
            {
                if (entity == null) continue;

                if (drawBoxes || corner)
                    DrawEntity(drawList, entity);
            }

            ImGui.End();
        }

        private void DrawEntity(ImDrawListPtr drawList, Entity entity)
        {
            if (entity.bones2d == null || entity.bones2d.Count < 13) return;

            Vector2 min = new Vector2(float.MaxValue), max = new Vector2(float.MinValue);
            foreach (var bone in entity.bones2d)
            {
                min = Vector2.Min(min, bone);
                max = Vector2.Max(max, bone);
            }

            Vector2 topLeft = new Vector2(min.X - 7, min.Y - 12);
            Vector2 bottomRight = max;

            uint color = GetEntityColor(entity);
            float thickness = CalculateThickness(entity.distance);

            if (drawSkeleton && ShouldDraw(entity))
                DrawSkeleton(drawList, entity, thickness);

            if (drawBoxes && ShouldDraw(entity))
                drawList.AddRect(topLeft, bottomRight, color, 0f, ImDrawFlags.None, thickness);

            if (corner && ShouldDraw(entity))
                DrawCornerBox(drawList, topLeft, bottomRight, color, thickness);

            if (healthBar && ShouldDraw(entity))
                DrawHealthBar(drawList, entity, topLeft, bottomRight, thickness);

            if (names && ShouldDraw(entity))
                DrawName(drawList, entity, topLeft, bottomRight);
        }

        private void DrawName(ImDrawListPtr drawList, Entity entity, Vector2 topLeft, Vector2 bottomRight)
        {
            if (string.IsNullOrWhiteSpace(entity.name)) return;

            string name = entity.name;
            Vector2 textSize = ImGui.CalcTextSize(name);
            Vector2 textPos = new Vector2(
                topLeft.X + (textSize.X > 0 ? ((bottomRight.X - topLeft.X) - textSize.X) / 2 : 0),
                topLeft.Y - textSize.Y - 4
            );

            drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(namesColor), name);
        }

        private void DrawCornerBox(ImDrawListPtr drawList, Vector2 topLeft, Vector2 bottomRight, uint color, float thickness)
        {
            float width = bottomRight.X - topLeft.X;
            float height = bottomRight.Y - topLeft.Y;
            float cornerLength = width * 0.2f;

            Vector2 topRight = new Vector2(bottomRight.X, topLeft.Y);
            Vector2 bottomLeft = new Vector2(topLeft.X, bottomRight.Y);

            // Top left
            drawList.AddLine(topLeft, new Vector2(topLeft.X + cornerLength, topLeft.Y), color, thickness);
            drawList.AddLine(topLeft, new Vector2(topLeft.X, topLeft.Y + cornerLength), color, thickness);

            // Top right
            drawList.AddLine(topRight, new Vector2(topRight.X - cornerLength, topRight.Y), color, thickness);
            drawList.AddLine(topRight, new Vector2(topRight.X, topRight.Y + cornerLength), color, thickness);

            // Bottom left
            drawList.AddLine(bottomLeft, new Vector2(bottomLeft.X + cornerLength, bottomLeft.Y), color, thickness);
            drawList.AddLine(bottomLeft, new Vector2(bottomLeft.X, bottomLeft.Y - cornerLength), color, thickness);

            // Bottom right
            drawList.AddLine(bottomRight, new Vector2(bottomRight.X - cornerLength, bottomRight.Y), color, thickness);
            drawList.AddLine(bottomRight, new Vector2(bottomRight.X, bottomRight.Y - cornerLength), color, thickness);
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

            drawList.AddLine(bones[1], bones[2], color, thickness);
            drawList.AddLine(bones[1], bones[3], color, thickness);
            drawList.AddLine(bones[1], bones[6], color, thickness);
            drawList.AddLine(bones[3], bones[4], color, thickness);
            drawList.AddLine(bones[6], bones[7], color, thickness);
            drawList.AddLine(bones[4], bones[5], color, thickness);
            drawList.AddLine(bones[7], bones[8], color, thickness);
            drawList.AddLine(bones[1], bones[0], color, thickness);
            drawList.AddLine(bones[0], bones[9], color, thickness);
            drawList.AddLine(bones[0], bones[11], color, thickness);
            drawList.AddLine(bones[9], bones[10], color, thickness);
            drawList.AddLine(bones[11], bones[12], color, thickness);

            drawList.AddCircle(bones[2], 3 + thickness, color);
        }

        private void DrawHealthBar(ImDrawListPtr drawList, Entity entity, Vector2 topLeft, Vector2 bottomRight, float thickness)
        {
            float healthPercent = Math.Clamp(entity.health, 0, 100) / 100f;
            float barHeight = bottomRight.Y - topLeft.Y;
            float barX = topLeft.X - 6f;
            float barThickness = thickness + 2.4f;

            drawList.AddRectFilled(
                new Vector2(barX, topLeft.Y),
                new Vector2(barX + 4f, bottomRight.Y),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1f))
            );

            drawList.AddRectFilled(
                new Vector2(barX, bottomRight.Y - barHeight * healthPercent),
                new Vector2(barX + 4f, bottomRight.Y),
                ImGui.ColorConvertFloat4ToU32(textColor)
            );
        }
    }
}
