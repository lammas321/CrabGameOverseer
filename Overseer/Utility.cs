using SteamworksNative;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Overseer
{
    internal static class Utility
    {
        internal static int MaxChatMessageLength
            => GameUiChatBox.Instance != null ? GameUiChatBox.Instance.field_Private_Int32_0 : 80;

        internal static ulong HostClientId
            => SteamManager.Instance.field_Private_CSteamID_1.m_SteamID;

        internal static string FormatMessage(string str)
            => Regex.Replace(
                str,
                "(.)(?<=\\1{5})", // Remove repeating characters (5 or more will truncate to 4, allowing it to appear in chat)
                string.Empty,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled
             );

        internal enum MessageType
        {
            Normal,
            Server,
            Styled
        }
        internal static void SendMessage(ulong recipientClientId, string message, MessageType messageType = MessageType.Server, string displayName = null)
            => SendMessage(message, messageType, displayName, [recipientClientId]);
        internal static void SendMessage(string message, MessageType messageType = MessageType.Server, string displayName = null, IEnumerable<ulong> recipientClientIds = null)
        {
            ulong senderClientId = 0UL;
            message ??= string.Empty;
            message = FormatMessage(message);
            if (messageType == MessageType.Server)
            {
                displayName = string.Empty;
                senderClientId = 1UL;
            }
            else
                displayName ??= string.Empty;

            List<byte> bytes = [];
            bytes.AddRange(BitConverter.GetBytes((int)ServerSendType.sendMessage));
            bytes.AddRange(BitConverter.GetBytes(senderClientId));

            bytes.AddRange(BitConverter.GetBytes(displayName.Length));
            bytes.AddRange(Encoding.ASCII.GetBytes(displayName));

            bytes.AddRange(BitConverter.GetBytes(message.Length));
            bytes.AddRange(Encoding.ASCII.GetBytes(message));

            bytes.InsertRange(0, BitConverter.GetBytes(bytes.Count));

            Packet packet = new()
            {
                field_Private_List_1_Byte_0 = new()
            };
            foreach (byte b in bytes)
                packet.field_Private_List_1_Byte_0.Add(b);

            foreach (ulong clientId in recipientClientIds ?? [.. LobbyManager.steamIdToUID.Keys])
            {
                if (messageType == MessageType.Styled)
                {
                    byte[] clientIdBytes = BitConverter.GetBytes(clientId);
                    for (int i = 0; i < clientIdBytes.Length; i++)
                        packet.field_Private_List_1_Byte_0[i + 8] = clientIdBytes[i];
                }
                SteamPacketManager.SendPacket(new CSteamID(clientId), packet, 8, SteamPacketDestination.ToClient);
            }
        }

        internal static string Escape(string str)
           => string.IsNullOrEmpty(str) ? string.Empty : str
                .Replace("\\", "\\\\")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace(",", "\\,");
        internal static string Unescape(string str)
            => string.IsNullOrEmpty(str) ? string.Empty : str
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\,", ",");
    }
}