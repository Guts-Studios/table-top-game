using UnityEngine;

namespace Warslammer.Core
{
    /// <summary>
    /// Interface for objects that can move on the battlefield
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// Current position of the object
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Movement speed in inches
        /// </summary>
        float MovementSpeed { get; }

        /// <summary>
        /// Remaining movement for this turn
        /// </summary>
        float RemainingMovement { get; }

        /// <summary>
        /// Can this unit currently move?
        /// </summary>
        bool CanMove { get; }

        /// <summary>
        /// Has this unit moved this turn?
        /// </summary>
        bool HasMoved { get; }

        /// <summary>
        /// Move to a target position
        /// </summary>
        /// <param name="targetPosition">Destination position</param>
        void MoveTo(Vector3 targetPosition);

        /// <summary>
        /// Check if a position is within movement range
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>True if position is reachable</returns>
        bool IsPositionReachable(Vector3 position);

        /// <summary>
        /// Get the movement cost to reach a position
        /// </summary>
        /// <param name="position">Target position</param>
        /// <returns>Movement cost in inches</returns>
        float GetMovementCost(Vector3 position);

        /// <summary>
        /// Reset movement for a new turn
        /// </summary>
        void ResetMovement();
    }
}