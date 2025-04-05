using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MatchmakingRoleCache
{
    public static Dictionary<ulong, PlayerState> clientRoles = new();
}
