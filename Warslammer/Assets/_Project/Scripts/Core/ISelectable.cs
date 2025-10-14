using UnityEngine;

namespace Warslammer.Core
{
    /// <summary>
    /// Interface for objects that can be selected by the player
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// Is this object currently selected?
        /// </summary>
        bool IsSelected { get; }

        /// <summary>
        /// Can this object be selected?
        /// </summary>
        bool CanBeSelected { get; }

        /// <summary>
        /// Transform of the selectable object
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Called when this object is selected
        /// </summary>
        void OnSelected();

        /// <summary>
        /// Called when this object is deselected
        /// </summary>
        void OnDeselected();

        /// <summary>
        /// Called when this object is hovered over
        /// </summary>
        void OnHoverEnter();

        /// <summary>
        /// Called when hover leaves this object
        /// </summary>
        void OnHoverExit();
    }
}