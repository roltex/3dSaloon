namespace JohnStairs.RPG.Character {
    public interface IMotor {
        /// <summary>
        /// Moves the character forward/backwards
        /// </summary>
        /// <param name="units">Units to move forward/backwards</param>
        void Move(float units);

        /// <summary>
        /// Moves the character left/right without turning it
        /// </summary>
        /// <param name="units"></param>
        void Strafe(float units);

        /// <summary>
        /// Lets the character rotate
        /// </summary>
        /// <param name="degrees">Degrees to rotate</param>
        /// <returns>Rotated angle in degrees</returns>
        float Rotate(float degrees);

        /// <summary>
        /// Lets the character jump if possible
        /// </summary>
        void Jump();
    }
}
