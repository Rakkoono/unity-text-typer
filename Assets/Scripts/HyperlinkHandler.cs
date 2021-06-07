using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TMP_Text))]
public class HyperlinkHandler : MonoBehaviour, IPointerClickHandler
{

    #region Serialized Fields
    [SerializeField]
    private bool changeColorOnHover = false;

    [SerializeField]
    private Color hoverColor = new Color(-.1f, -.1f, -.1f, .1f);
    #endregion

    #region Hidden Fields
    private TMP_Text tmp = null;
    private TMP_Text TMP => tmp ??= GetComponent<TMP_Text>();

    private Camera Camera => TMP.canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : TMP.canvas.worldCamera;
    private Vector3 MousePosition => Mouse.current.position.ReadValue();

    private int lastLinkIndex = -1;

    private List<Color32[]> previousVertexColors = new List<Color32[]>();
    #endregion

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(TMP, eventData.position, Camera);

        if (linkIndex != -1)
            Application.OpenURL(TMP.textInfo.linkInfo[linkIndex].GetLinkID());
    }

#if !UNITY_IOS || !UNITY_ANDROID
    private void Update()
    {
        bool hoveringOverText = TMP_TextUtilities.IsIntersectingRectTransform(TMP.rectTransform, MousePosition, null);
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(TMP, MousePosition, Camera);

        if (linkIndex != lastLinkIndex)
        {
            if (lastLinkIndex != -1)
            {
                if (changeColorOnHover)
                {
                    ColorLink(lastLinkIndex, (link, vert) => previousVertexColors[link][vert]);
                    previousVertexColors.Clear();
                    lastLinkIndex = -1;
                }
            }

            if (linkIndex != -1)
            {
                if (changeColorOnHover)
                {
                    lastLinkIndex = linkIndex;
                    previousVertexColors = ColorLink(linkIndex, (link, vert) => hoverColor);
                }
            }
        }
    }

    private List<Color32[]> ColorLink(int linkIndex, Func<int, int, Color32> ColorByLinkAndVertex)
    {
        var linkInfo = TMP.textInfo.linkInfo[linkIndex];
        var currentVertexColors = new List<Color32[]>(); // store the old character colors

        for (int i = 0; i < linkInfo.linkTextLength; i++)
        {
            // for each character in the link string
            int charIndex = linkInfo.linkTextfirstCharacterIndex + i; // the character index into the entire text
            var charInfo = TMP.textInfo.characterInfo[charIndex];
            int meshIndex = charInfo.materialReferenceIndex; // Get the index of the material / sub text object used by this character.
            int vertexIndex = charInfo.vertexIndex; // Get the index of the first vertex of this character.

            Color32[] vertexColors = TMP.textInfo.meshInfo[meshIndex].colors32; // the colors for this character
            currentVertexColors.Add(vertexColors.Clone() as Color32[]);

            if (charInfo.isVisible)
            {
                for (int j = 0; j < 4; j++)
                    vertexColors[vertexIndex + j] = ColorByLinkAndVertex(i, vertexIndex + j);
            }
        }

        // Update Geometry
        TMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        return currentVertexColors;
    }
#endif
}