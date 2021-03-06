﻿using UnityEngine;
using System.Collections;



namespace Fresvii.AppSteroid.Gui
{
    public class FresviiGUIGroupMemberAddTop : MonoBehaviour
    {        
        private Rect baseRect;

        private Texture2D palette;
        private Rect texCoordsMenu;
        private Rect texCoordsBorderLine;

        public float height;

        public GUIStyle guiStyleTitle;
        private string title = "Add Contact";
        
        public float hMargin;
		public float vMargin = 8f;

        private FresviiGUIGroupMemberAdd frameGroupMemberAdd;

        public int GuiDepth;

        public float minusMargin;

        public GUIStyle guiStyleCancelButton;
        private GUIContent cancelLabelContent;
        private Rect cancelLabelPosition;
        private Rect cancelButtonHitPosition;

        public GUIStyle guiStyleDoneButton;
        private GUIContent doneLabelContent;
        private Rect doneLabelPosition;
        private Rect doneButtonHitPosition;
        private Vector2 doneLabelSize;

        public void Init(Texture2D appIcon, string postFix, float scaleFactor, int guiDepth, FresviiGUIGroupMemberAdd frameGroupMemberAdd)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                guiStyleTitle.font = null;
                guiStyleTitle.fontStyle = FontStyle.Bold;

                guiStyleDoneButton.font = null;
                guiStyleCancelButton.font = null;
            }

            this.title = FresviiGUIText.Get("AddContact");

            this.frameGroupMemberAdd = frameGroupMemberAdd;

            this.GuiDepth = guiDepth;

            palette = FresviiGUIColorPalette.Palette;

            guiStyleTitle.normal.textColor = FresviiGUIColorPalette.GetColor(FresviiGUIColorPalette.NavigationBarTitle);

            texCoordsMenu = FresviiGUIColorPalette.GetTextureCoods(FresviiGUIColorPalette.NavigationBarBackground);

            texCoordsBorderLine = FresviiGUIColorPalette.GetTextureCoods(FresviiGUIColorPalette.NavigationBarUnderLine);

            height *= scaleFactor;

            guiStyleTitle.fontSize = (int)(guiStyleTitle.fontSize * scaleFactor);
                      
            hMargin *= scaleFactor;
			
            vMargin *= scaleFactor;
            
            guiStyleCancelButton.fontSize = (int)(guiStyleCancelButton.fontSize * scaleFactor);

            guiStyleDoneButton.fontSize = (int)(guiStyleDoneButton.fontSize * scaleFactor);
            
            guiStyleCancelButton.normal.textColor = FresviiGUIColorPalette.GetColor(FresviiGUIColorPalette.NavigationBarNormal);

            guiStyleDoneButton.normal.textColor = FresviiGUIColorPalette.GetColor(FresviiGUIColorPalette.NavigationBarNormal);
            
            cancelLabelContent = new GUIContent(FresviiGUIText.Get("Cancel"));

            doneLabelContent = new GUIContent(FresviiGUIText.Get("Done"));

            doneLabelSize = guiStyleDoneButton.CalcSize(doneLabelContent);
        }

        void Update()
        {
            baseRect = new Rect(frameGroupMemberAdd.Position.x, frameGroupMemberAdd.Position.y, Screen.width, height);

            cancelButtonHitPosition = new Rect(0f, 0f, vMargin + guiStyleCancelButton.CalcSize(cancelLabelContent).x, height);
            
            cancelLabelPosition = new Rect(hMargin, 0f, baseRect.width, height);
         
            doneLabelPosition = new Rect(Screen.width - hMargin - doneLabelSize.x, 0f, doneLabelSize.x, height);
        }

        public void OnGUI()
        {
            GUI.depth = GuiDepth;

			Event e = Event.current;

            if (e.type == EventType.MouseUp && cancelButtonHitPosition.Contains(e.mousePosition) && !FASGesture.IsDragging && !frameGroupMemberAdd.ControlLock)
            {
                e.Use();

                frameGroupMemberAdd.Back();
            }

            if (e.type == EventType.MouseUp && doneLabelPosition.Contains(e.mousePosition) && !FASGesture.IsDragging && !frameGroupMemberAdd.ControlLock)
            {
                e.Use();

                frameGroupMemberAdd.Done();
            }

            //  Mat
            GUI.DrawTextureWithTexCoords(baseRect, palette, texCoordsMenu);

            GUI.DrawTextureWithTexCoords(new Rect(baseRect.x, baseRect.height + baseRect.y, Screen.width, 1), palette, texCoordsBorderLine);

            GUI.BeginGroup(baseRect);

            GUI.Label(cancelLabelPosition, cancelLabelContent, guiStyleCancelButton);

            GUI.Label(new Rect(0f,0f,Screen.width,height), title, guiStyleTitle);

            GUI.Label(doneLabelPosition, doneLabelContent, guiStyleDoneButton);

            GUI.EndGroup();
            
        }
    }   
}