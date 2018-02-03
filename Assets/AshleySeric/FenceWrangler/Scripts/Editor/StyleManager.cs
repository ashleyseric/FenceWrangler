/// Copyright 2018 Ashley Seric [contact@ashleyseric.com]
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
/// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
/// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
/// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
/// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
/// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR 
/// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;

namespace AshleySeric.FenceWrangler.Editor
{
    public class StyleManager
    {
        public static GUIStyle FoldoutHeading
        {
            get
            {
                GUIStyle style = new GUIStyle("ShurikenModuleTitle");
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
                return style;
            }
        }
    }
}
