using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ControlsLibrary;

public class NaturalStringComparer : IComparer<string> {
    private static readonly Regex _re = new(@"(\d+)|(\D+)", RegexOptions.Compiled);

    public int Compare(string? x, string? y) {
        if (x == null) return y == null ? 0 : -1;
        if (y == null) return 1;

        var matchesX = _re.Matches(x);
        var matchesY = _re.Matches(y);
        int iX = 0, iY = 0;

        while (iX < matchesX.Count || iY < matchesY.Count) {
            if (iX >= matchesX.Count) return -1; // x ran out of parts
            if (iY >= matchesY.Count) return 1;  // y ran out of parts

            var partX = matchesX[iX].Value;
            var partY = matchesY[iY].Value;

            if (char.IsDigit(partX[0]) && char.IsDigit(partY[0])) {
                int numX = int.Parse(partX);
                int numY = int.Parse(partY);

                if (numX != numY) return numX.CompareTo(numY);
            }
            else {
                int comp = string.Compare(partX, partY, StringComparison.Ordinal);
                if (comp != 0) return comp;
            }

            iX++;
            iY++;
        }

        return 0; // They matched completely
    }
}
