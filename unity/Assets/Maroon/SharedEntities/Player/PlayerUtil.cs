using UnityEngine;
using VRTK;

namespace Util
{
    public static class PlayerUtil
    {
        public static bool IsPlayer(GameObject gameObject)
        {
            if (gameObject.CompareTag("Player"))
                return true;

            return gameObject.GetComponent<VRTK_PlayerObject>() != null && gameObject.name.Contains("Body");
        }
    }
}
