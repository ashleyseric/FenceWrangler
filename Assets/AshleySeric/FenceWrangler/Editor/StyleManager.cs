using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AshleySeric.Styles
{
    public class StyleManager
    {
        public static GUIStyle FoldoutHeading
        {
            get
            {
                GUIStyle style = new GUIStyle("ShurikenModuleTitle");
                //style.fixedHeight = 20;
                style.margin = new RectOffset(0, 0, 0, 0);
                style.imagePosition = ImagePosition.TextOnly;
                return style;
            }
        }

        public static GUIStyle FoldoutSetHeading
        {
            get
            {
                GUIStyle style = new GUIStyle("ShurikenEmitterTitle");
                //style.fixedHeight = 20;
                style.margin = new RectOffset(0, 0, 0, 0);
                style.imagePosition = ImagePosition.TextOnly;
                return style;
            }
        }

        public static GUIStyle FoldoutBg
        {
            get
            {
                GUIStyle style = new GUIStyle();
                GUIStyle eStyle = new GUIStyle("ShurikenModuleBg");
                style.normal.background = eStyle.normal.background;
                style.border = eStyle.border;
                style.margin = new RectOffset(1, 1, 1, 3);
                style.padding = new RectOffset(0, 0, 0, 0);
                return style;
            }
        }

        public static GUIStyle FoldoutBgCollapsed
        {
            get
            {
                GUIStyle style = new GUIStyle();
                style.margin = new RectOffset(1, 1, 1, 0);
                style.padding = new RectOffset(0, 0, 0, 0);
                return style;
            }
        }

        public static GUIStyle FoldoutSetBg
        {
            get
            {
                GUIStyle style = new GUIStyle("ShurikenEffectBg");
                style.stretchHeight = false;
                style.padding = new RectOffset(1, 1, 4, 1);
                style.margin = new RectOffset(0, 0, 3, 3);
                return style;
            }
        }

        public static GUIStyle toggle
        {
            get
            {
                GUIStyle style = new GUIStyle("ShurikenCheckMark");
                //style.padding = new RectOffset(1, 1, 4, 1);
                //style.margin = new RectOffset(0, 0, 3, 3);
                return style;
            }
        }
    }
}
