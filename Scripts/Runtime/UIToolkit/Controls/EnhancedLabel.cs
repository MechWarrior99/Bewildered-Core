using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bewildered.UIToolkit
{
    public enum TextTruncationPosition { Start, Middle, End }

    public class EnhancedLabel : Label
    {
        private const string ellipsisText = @"...";

        private bool _ellipsText = true;
        private bool _displayTooltipWhenElided = true;

        [SerializeField] private string _sourceText = string.Empty;
        
        public override string text 
        {
            get { return base.text; }
            set 
            {
                _sourceText = value;
                base.text = value;
            }
        }

        public bool EllipsText
        {
            get { return _ellipsText; }
            set { _ellipsText = value; }
        }

        public TextTruncationPosition TruncationPosition
        {
            get;
            set;
        } = TextTruncationPosition.End;

        public EnhancedLabel() : this(string.Empty) { }

        public EnhancedLabel(string text) : base(text)
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private string TruncateText(string sourceText, float textContainerWidth, TextTruncationPosition truncationPosition)
        {
            if (float.IsNaN(textContainerWidth))
                return sourceText;

            Vector2 textSize = MeasureTextSize(sourceText, textContainerWidth, MeasureMode.Undefined, 0, MeasureMode.Undefined);
            if (textSize.x <= textContainerWidth)
                return sourceText;

            int sourceTextMaxIndex = sourceText.Length - 1;

            string truncatedText = sourceText;
            int minIndex = truncationPosition == TextTruncationPosition.Start ? 1 : 0;
            int maxIndex = (truncationPosition == TextTruncationPosition.Start || truncationPosition == TextTruncationPosition.Middle) ? sourceTextMaxIndex : sourceTextMaxIndex - 1;
            int midIndex = (minIndex + maxIndex) / 2;
            int previusFitMidIndex = -1;

            int exceptionHandlerCount = 0;

            while (minIndex <= maxIndex)
            {
                truncatedText = Truncate(sourceText, midIndex, truncationPosition);

                textSize = MeasureTextSize(truncatedText, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined);

                // Basically exactly the same size.
                if (Mathf.Abs(textSize.x - textContainerWidth) < Mathf.Epsilon)
                    return truncatedText;

                if (textSize.x > textContainerWidth)
                {
                    if (midIndex - 1 == previusFitMidIndex)
                        return Truncate(sourceText, previusFitMidIndex, truncationPosition);

                    if (truncationPosition == TextTruncationPosition.Start)
                        minIndex = midIndex + 1;
                    else
                        maxIndex = midIndex - 1;
                }
                else
                {
                    if (truncationPosition == TextTruncationPosition.Start)
                        maxIndex = midIndex - 1;
                    else
                        minIndex = midIndex + 1;

                    previusFitMidIndex = midIndex;
                }

                midIndex = (minIndex + maxIndex) / 2;

                if (exceptionHandlerCount > 200)
                    throw new TimeoutException();
                else
                    exceptionHandlerCount++;
            }

            return truncatedText;
        }

        private string Truncate(string sourceText, int midIndex, TextTruncationPosition truncationPosition)
        {
            int lengthFromMiddle = (sourceText.Length - 1) - (midIndex - 1);
            switch (truncationPosition)
            {
                case TextTruncationPosition.Start:
                    return ellipsisText + sourceText.Substring(midIndex, lengthFromMiddle);
                case TextTruncationPosition.Middle:
                    return sourceText.Substring(0, midIndex - 1) + ellipsisText + sourceText.Substring(lengthFromMiddle);
                case TextTruncationPosition.End:
                    return sourceText.Substring(0, midIndex) + ellipsisText;
            }

            return sourceText;
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            base.text = TruncateText(_sourceText, contentRect.width, TruncationPosition);
        }
    }

}