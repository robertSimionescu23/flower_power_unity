using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class GroundWithBorderTiles : RuleTile<GroundWithBorderTiles.Neighbor> {
    public List<TileBase> siblings = new();

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int NewRule1 = 1;
        public const int NewRule2 = 2;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.NewRule1 : return tile == this || siblings.Contains(tile) ;
            case Neighbor.NewRule2 : return tile != this && !siblings.Contains(tile) ;
        }
        return base.RuleMatch(neighbor, tile);
    }
}
