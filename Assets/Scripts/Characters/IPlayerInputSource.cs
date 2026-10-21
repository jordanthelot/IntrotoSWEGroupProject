// Lets something other than the keyboard drive PlayerController (used by the automated
// movement test). When PlayerController.inputOverride is null, the keyboard is used as normal.
public interface IPlayerInputSource
{
    /// <summary>-1 = left, 0 = none, 1 = right (same as Input.GetAxisRaw("Horizontal")).</summary>
    float Horizontal { get; }

    /// <summary>True once per press (same as Input.GetButtonDown("Jump")).</summary>
    bool ConsumeJumpPressed();
}
