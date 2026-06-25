using UnityEngine;

namespace InfiniteTileWorld
{
    public class BackgroundPanel : MonoBehaviour
    {
        public virtual void OnWarped(int playerGridX, int playerGridZ, float tileSize)
        {
            var pos = transform.position;
            pos.x = playerGridX * tileSize;
            pos.z = playerGridZ * tileSize;
            transform.position = pos;
        }
    }
}