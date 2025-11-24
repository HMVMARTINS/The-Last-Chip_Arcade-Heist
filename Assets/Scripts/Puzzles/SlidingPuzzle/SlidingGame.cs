using System.Collections.Generic;
using UnityEngine;

public class SlidingGame : InteractableGame
{
    List<SlidingPiece> pieces = new List<SlidingPiece>();

    void OnEnable() { }

    private void InitializePieces() { }

    public override void ForceFinish() { }
}
