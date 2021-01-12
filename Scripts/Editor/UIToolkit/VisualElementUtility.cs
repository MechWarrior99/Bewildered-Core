using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bewildered.Core.Editor
{
    public static class VisualElementUtility
    {
        /// <summary>
        /// Create a <see cref="Image"/> with the icon from the specified <see cref="GUIContent"/>.
        /// </summary>
        public static Image CreateIconImage(GUIContent icon)
        {
            return CreateIconImage(icon.image);
        }

        /// <summary>
        /// Create a <see cref="Image"/> with the icon of the specified <see cref="Texture"/>.
        /// </summary>
        public static Image CreateIconImage(Texture icon)
        {
            Image iconImage = new Image();
            iconImage.image = icon;
            iconImage.style.flexGrow = 0;
            iconImage.style.width = icon.width;
            iconImage.style.height = icon.height;
            return iconImage;
        }
    }

}