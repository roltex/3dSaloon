namespace JohnStairs.RPG {
    public interface ICursorHandler {
        /// <summary>
        /// Shows the cursor
        /// </summary>
        void ShowCursor();

        /// <summary>
        /// Hides the cursor
        /// </summary>
        void HideCursor();

        /// <summary>
        /// Checks if the cursor is over a UI element
        /// </summary>
        /// <returns>True if the cursor is over a UI element, otherwise false</returns>
        bool IsCursorOverUI();
    }
}
