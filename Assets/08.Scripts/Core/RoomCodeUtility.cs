using System;
using System.Text;

public static class RoomCodeUtility
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int RoomCodeLength = 4;
    private const ushort MinimumPort = 10000;
    private const ushort PortRange = 50000;

    public static string GenerateRoomCode()
    {
        Random random = new();
        StringBuilder builder = new(RoomCodeLength);

        for (int i = 0; i < RoomCodeLength; i++)
        {
            builder.Append(Alphabet[random.Next(Alphabet.Length)]);
        }

        return builder.ToString();
    }

    public static bool TryNormalize(string roomCode, out string normalizedCode)
    {
        normalizedCode = string.Empty;
        if (string.IsNullOrWhiteSpace(roomCode))
        {
            return false;
        }

        StringBuilder builder = new(RoomCodeLength);
        foreach (char character in roomCode)
        {
            if (char.IsWhiteSpace(character) || character == '-')
            {
                continue;
            }

            char upperCharacter = char.ToUpperInvariant(character);
            if (Alphabet.IndexOf(upperCharacter) < 0)
            {
                return false;
            }

            builder.Append(upperCharacter);
            if (builder.Length > RoomCodeLength)
            {
                return false;
            }
        }

        if (builder.Length != RoomCodeLength)
        {
            return false;
        }

        normalizedCode = builder.ToString();
        return true;
    }

    public static ushort GetPortForRoomCode(string roomCode)
    {
        if (!TryNormalize(roomCode, out string normalizedCode))
        {
            throw new ArgumentException("Room code must be a four-character code.", nameof(roomCode));
        }

        int value = 0;
        foreach (char character in normalizedCode)
        {
            value *= Alphabet.Length;
            value += Alphabet.IndexOf(character);
        }

        return (ushort)(MinimumPort + (value % PortRange));
    }
}
