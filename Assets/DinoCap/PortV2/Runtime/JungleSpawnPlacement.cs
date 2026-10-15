using System;
using UnityEngine;

namespace DinoCap.PortV2
{
    public static class JungleSpawnPlacement
    {
        [Serializable] public sealed class BoundsTable {public JumpBounds[] bounds;}
        [Serializable] public struct JumpBounds {public string name;public float minX,maxX;}
        public struct Placement
        {
            public bool admitted,jump,facingRight;
            public float x,y,velocityX;
        }
        public static JumpBounds LoadNativeJumpBounds()
        {
            var data=Resources.Load<TextAsset>("DinoCapGenerated/jungle_jump_bounds");
            if(data==null)throw new InvalidOperationException("Missing original Jungle jump bounds.");
            var table=JsonUtility.FromJson<BoundsTable>(data.text);
            foreach(var row in table.bounds)if(row.name=="k3_jump")return row;
            throw new InvalidOperationException("Native18858 uses21B9BC k3_jump bounds for both jump types.");
        }
        static float Draw(int raw,float scale)
        {
            if(raw<0)throw new ArgumentOutOfRangeException(nameof(raw));
            return (float)((float)(raw*(1f/2147483648f))*scale);
        }
        // iPad18858: walk side from CURRENT player physics velocity; jump
        // side from raw player+0x60 facing. These are distinct owners.
        public static Placement Decide(int type,int playerState,float cameraLeft,float viewWidth,
            float playerVelocityX,bool playerFacingRight,float speed,JumpBounds bounds,Func<int> nextRaw31)
        {
            if(type<1 || type>4)throw new ArgumentOutOfRangeException(nameof(type));
            if(playerState==4 || playerState==6)return default;
            bool jump=type<=2 && (int)Draw(nextRaw31(),99f)>=75;
            bool left;
            float x,velocity;
            if(jump)
            {
                // NATIVE (PROVEN, ARM): sub_57BD8/sub_18858 test the player byte +0x60. The player movement update sub_65D1C
                // sets +0x60=1 (and a NEGATIVE x velocity) when the move-LEFT flag is held and clears it to 0 (positive velocity) for
                // move-RIGHT, so +0x60=1 means FACING LEFT. Jump-ins spawn at the left edge (moving +1024) when +0x60==1 and at the
                // right edge (moving -1024) when it is 0, i.e. ALWAYS on the side the player is facing - never behind a player who
                // keeps moving. (The earlier reading "Jump appears on the LEFT when facing right" inverted this flag.)
                left=!playerFacingRight;
                x=left ? cameraLeft-bounds.maxX : cameraLeft+viewWidth-bounds.minX;
                velocity=left ? 1024f : -1024f;
            }
            else
            {
                left=playerVelocityX<0 || playerVelocityX==0 && Draw(nextRaw31(),100f)>=50f;
                //1128792064 is float bits0x43480000 =200, not195.
                x=left ? cameraLeft-200f : cameraLeft+viewWidth+200f;
                velocity=left ? speed : -speed;
            }
            return new Placement {admitted=true,jump=jump,facingRight=left,x=x,y=136f,velocityX=velocity};
        }
    }
}
