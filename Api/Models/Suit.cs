namespace Api.Models;

// Underlying values match the suit-product math from the brief:
// Diamonds=1, Hearts=2, Spades=3, Clubs=4.
// We use the int value directly when calculating the tie-break product,
// which keeps the rule visible in one place and avoids a lookup table.
public enum Suit
{
    Diamonds = 1,
    Hearts = 2,
    Spades = 3,
    Clubs = 4
}