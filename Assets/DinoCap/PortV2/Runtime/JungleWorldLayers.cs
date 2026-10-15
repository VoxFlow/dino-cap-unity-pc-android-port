using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    // Original registered sprites, native13FD4 sizes/origins and D308 wrap.
    // Scene adapter supplies painter-order mapping for the shared actors.
    public sealed class JungleWorldLayers : MonoBehaviour
    {
        readonly NativeJungleLayerWrap wrap=new NativeJungleLayerWrap();
        readonly List<Transform> back=new List<Transform>(),front=new List<Transform>();
        Sprite backSprite,frontSprite;
        Transform leftBack,leftFront;
        int backOrder,frontOrder;
        public int TileCount=>back.Count;
        public NativeJungleLayerWrap Layout=>wrap;
        public bool ExpandedLeftCoverageActive=>leftBack!=null && leftBack.gameObject.activeSelf;
        public void Initialize(int backgroundOrder,int foregroundOrder)
        {
            backOrder=backgroundOrder;frontOrder=foregroundOrder;
            backSprite=LoadSprite("back_endFrame");frontSprite=LoadSprite("back_frontFrame");
        }
        static Sprite LoadSprite(string name)
        {
            var texture=Resources.Load<Texture2D>("DinoCapGenerated/JungleBlitz/"+name);
            if(texture==null)throw new InvalidOperationException("Missing original Jungle frame "+name);
            texture.filterMode=FilterMode.Point;texture.mipMapBias=0;
            var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),1,0,SpriteMeshType.FullRect);
            sprite.name=name;return sprite;
        }
        Transform CreateTile(string name,Sprite sprite,float height,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=order;
            go.transform.localScale=new Vector3(1023f/sprite.rect.width,height/sprite.rect.height,1);
            return go.transform;
        }
        public void Tick(float cameraLeft,float viewWidth)
        {
            if(backSprite==null || frontSprite==null)throw new InvalidOperationException("Initialize Jungle world layers first.");
            int count=NativeJungleLayerWrap.TileCountForView(viewWidth);
            while(back.Count<count)
            {
                int i=back.Count;
                back.Add(CreateTile("JungleBackground_"+i,backSprite,768,backOrder));
                front.Add(CreateTile("JungleForeground_"+i,frontSprite,312,frontOrder));
            }
            wrap.Tick(cameraLeft);
            for(int i=0;i<back.Count;i++)
            {
                back[i].gameObject.SetActive(i<count);front[i].gameObject.SetActive(i<count);
                back[i].position=new Vector3(wrap.CentreX(i),0,10);
                front[i].position=new Vector3(wrap.CentreX(i),-250,5);
            }
            // USER-ACCEPTED DEFENSIVE WORKAROUND, N255 runtime follow-up:
            // an extra LEFT copy covers retained-fraction reversal gaps in
            // expanded views. Do not alter D308 or the native three copies.
            bool expanded=viewWidth>1024f;
            if(expanded && leftBack==null)
            {
                leftBack=CreateTile("JungleBackground_ExpandedLeftCoverage",backSprite,768,backOrder);
                leftFront=CreateTile("JungleForeground_ExpandedLeftCoverage",frontSprite,312,frontOrder);
            }
            if(leftBack!=null)
            {
                leftBack.gameObject.SetActive(expanded);leftFront.gameObject.SetActive(expanded);
                float x=wrap.CentreX(0)-NativeJungleLayerWrap.TileWidth;
                leftBack.position=new Vector3(x,0,10);leftFront.position=new Vector3(x,-250,5);
            }
        }
        void OnDestroy()
        {
            if(backSprite!=null)Destroy(backSprite);
            if(frontSprite!=null)Destroy(frontSprite);
        }
    }
}
