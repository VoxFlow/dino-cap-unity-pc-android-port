using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    // Scene-owned itemClassiPad reused by F154; EFB4 grows the display
    // while root physics rises50. No unproven fade/despawn timer added.
    public sealed class JungleAmmoNotification : MonoBehaviour
    {
        readonly List<Sprite> sprites=new List<Sprite>();
        readonly List<SpriteRenderer> digits=new List<SpriteRenderer>();
        readonly List<float> digitWidths=new List<float>();
        SpriteRenderer icon,multiply;
        Vector2 iconOffset,iconSize;
        public float Progress {get;private set;}
        public int Family {get;private set;}
        public int Amount {get;private set;}
        public bool Displaying {get;private set;}
        static readonly Vector2[] offsets={new Vector2(-25,20),new Vector2(-29,25),new Vector2(-31,25),new Vector2(-35,28),new Vector2(-38,25),new Vector2(-40,25)};
        static readonly Vector2[] sizes={new Vector2(38,44),new Vector2(61,52),new Vector2(52,55),new Vector2(65,70),new Vector2(56,62),new Vector2(66,62)};
        static readonly float[] nativeDigitWidths={40,23,36,36,40,37,38,33,38,39};
        public void Show(int family,int amount,Vector2 position)
        {
            if(family<0 || family>5 || amount<0)throw new ArgumentOutOfRangeException();
            ClearVisuals();Family=family;Amount=amount;Progress=.5f;Displaying=true;
            transform.position=new Vector3(position.x,position.y,1);
            iconOffset=offsets[family];iconSize=sizes[family];
            icon=Create("icon_item"+family+"Frame");multiply=Create("icon_itemxFrame");
            foreach(char c in amount.ToString(System.Globalization.CultureInfo.InvariantCulture))
            {
                digits.Add(Create("icon_num"+c+"Frame"));digitWidths.Add(nativeDigitWidths[c-'0']*.5f);
            }
            Layout();
        }
        SpriteRenderer Create(string frame)
        {
            var texture=Resources.Load<Texture2D>((frame=="icon_itemxFrame"?"DinoCapGenerated/JungleBlitz/":"DinoCapGenerated/CityGrind/")+frame);
            if(texture==null)throw new InvalidOperationException("Missing original notification frame "+frame);
            texture.filterMode=FilterMode.Point;
            var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),1,0,SpriteMeshType.FullRect);
            sprite.name=frame;sprites.Add(sprite);
            var go=new GameObject(frame);go.transform.SetParent(transform,false);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=320;return sr;
        }
        public void Tick(float dt)
        {
            if(!Displaying)return;
            transform.position+=Vector3.up*(50f*dt);
            Progress=Mathf.Min(1,Progress+dt*5);
            Layout();
        }
        void Layout()
        {
            Place(icon,iconOffset,iconSize);
            Place(multiply,new Vector2(-5,10),new Vector2(19,18));
            float x=15;
            for(int i=0;i<digits.Count;i++)
            {
                Place(digits[i],new Vector2(x,15),new Vector2(digitWidths[i],26.5f));
                x+=digitWidths[i];
            }
        }
        void Place(SpriteRenderer sr,Vector2 offset,Vector2 size)
        {
            sr.transform.localPosition=new Vector3(offset.x*Progress,offset.y*Progress,0);
            sr.transform.localScale=new Vector3(size.x*Progress/sr.sprite.rect.width,size.y*Progress/sr.sprite.rect.height,1);
        }
        void ClearVisuals()
        {
            foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            foreach(var sprite in sprites)Destroy(sprite);
            sprites.Clear();digits.Clear();digitWidths.Clear();
        }
        void OnDestroy(){foreach(var sprite in sprites)if(sprite!=null)Destroy(sprite);}
    }
}
