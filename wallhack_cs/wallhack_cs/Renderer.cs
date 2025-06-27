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
        ImDrawListPtr drawListPtr;
        public bool esp = true;
        Vector4 teamColor = new Vector4(14, 255, 0, 255);
        Vector4 enemyColor = new Vector4(104, 0, 175, 255);
        float boneThickness = 4;

        protected override void Render()
        {
            // Janela de configuração do ESP
            ImGui.Begin("ESP Menu");
            ImGui.Checkbox("ESP", ref esp);
            ImGui.SliderFloat("Bone Thickness", ref boneThickness, 4, 500);
            if (ImGui.CollapsingHeader("Team Color"))
            {
                ImGui.ColorPicker4("##teamcolor", ref teamColor);
            }
            if (ImGui.CollapsingHeader("Enemy Color"))
            {
                ImGui.ColorPicker4("##enemycolor", ref enemyColor);
            }
            ImGui.End();

            // Se o ESP estiver ativado, desenha o overlay e os esqueleto das entidades
            if (esp)
            {
                DrawOverlay();   // Abre a janela overlay
                DrawSkeletons(); // Desenha linhas e círculos para os ossos
                ImGui.End();     // Fecha a janela overlay
            }
        }

        void DrawSkeletons()
        {
            // Verifica se a lista não é nula e não está vazia
            if (entitiesCopy == null || entitiesCopy.Count == 0)
                return;

            // Cria uma cópia da lista para evitar conflitos durante a iteração
            List<Entity> tempEntities = new List<Entity>(entitiesCopy).ToList();

            drawListPtr = ImGui.GetWindowDrawList();

            uint uintColor;

            foreach (Entity entity in tempEntities)
            {
                if (entity == null)
                    continue;

                // Define a cor com base no time: mesmo time do jogador local ou inimigo
                uintColor = localPlayerCopy.team == entity.team
                              ? ImGui.ColorConvertFloat4ToU32(teamColor)
                              : ImGui.ColorConvertFloat4ToU32(enemyColor);

                // Verifica se o ponto central do esqueleto está dentro dos limites de exibição
                if (entity.bones2d[2].X > 0 && entity.bones2d[2].Y > 0 &&
                    entity.bones2d[2].X < overlaySize.X && entity.bones2d[2].Y < overlaySize.Y)
                {
                    // Protege contra divisão por zero (ou valores muito baixos)
                    float safeDistance = entity.distance <= 0 ? 1 : entity.distance;
                    float currentBoneThickness = boneThickness / safeDistance;

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
            // A janela Overlay será fechada em Render() após desenhar os elementos.
        }
    }
}