using UnityEngine.UIElements;

namespace Bewildered.Editor
{
    /// <inheritdoc/>
    /// <remarks>Adds some additional Funcinality that is hidden by default in the base <see cref="ListView"/>.</remarks>
    public class EnhancedListView : ListView
    {
        public new void ClearSelection()
        {
            base.ClearSelection();
        }
    } 
}
