using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    public sealed class JunglePickupActor : MonoBehaviour
    {
        [Serializable] public sealed class GeometryTable {public Geometry[] rows;}
        [Serializable] public struct Geometry {public string frame;public float width,height;}
        readonly List<Sprite> ownedSprites=new List<Sprite>();
        NativeSceneTimerQueue timers;
        SpriteRenderer item,beam;
        int firstTimer,beamTimer,healTimer;
        public JunglePickupState State {get;private set;}
        public JungleDropSelection.Decision Decision {get;private set;}
        public bool BeamActive=>beam!=null && beam.enabled;
        public float ItemY=>item.transform.position.y;
        public static Geometry NativeGeometry(string frame)
        {
            var raw=Resources.Load<TextAsset>("DinoCapGenerated/jungle_pickup_geometry");
            if(raw==null)throw new InvalidOperationException("Missing native pickup draw geometry.");
            foreach(var row in JsonUtility.FromJson<GeometryTable>(raw.text).rows)if(row.frame==frame)return row;
            throw new InvalidOperationException("No native pickup geometry for "+frame);
        }
        public void Initialize(JungleDropSelection.Decision decision,float x,float deathY,NativeSceneTimerQueue queue)
        {
            if(!decision.HasDrop)throw new ArgumentException("Cannot create an absent native drop.");
            Decision=decision;timers=queue;
            State=new JunglePickupState(decision.kind,x,deathY,true);
            var geometry=NativeGeometry(decision.frame);
            item=CreateVisual("JunglePickup",decision.frame,geometry.width,geometry.height,310);
            item.transform.position=new Vector3(x,State.SpawnY,1);
            firstTimer=timers.Arm(10,_=>{if(this!=null){State.Expire();item.enabled=false;RetireBeam();RetireIfDone();}});
            beam=CreateVisual("JunglePickupBeam","L07Frame",200,200,299);
            //1AD48 raw1AE2C/2E overwrites saved width/height slots with
            //spawnX/spawnY+20;1AF2E/30 uses those for the stationary beam.
            beam.transform.position=item.transform.position;
            beamTimer=timers.Arm(10,_=>{if(this!=null){beam.enabled=false;RetireIfDone();}});
        }
        SpriteRenderer CreateVisual(string name,string frame,float width,float height,int order)
        {
            var texture=Resources.Load<Texture2D>("DinoCapGenerated/JungleBlitz/"+frame);
            if(texture==null)throw new InvalidOperationException("Missing original pickup frame "+frame);
            texture.filterMode=FilterMode.Point;
            var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),1,0,SpriteMeshType.FullRect);
            sprite.name=frame;ownedSprites.Add(sprite);
            var go=new GameObject(name);go.transform.SetParent(transform,false);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=order;
            go.transform.localScale=new Vector3(width/texture.width,height/texture.height,1);
            return renderer;
        }
        public void Tick(float dt,float playerX,ref float hp,
            Func<JungleDropSelection.Decision,JunglePickupInventory.Outcome> apply,
            Action<JungleDropSelection.Decision,JunglePickupInventory.Outcome,float,float> collected)
        {
            if(!State.Active)return;
            //Native17BA5C moves physics before actor update; bob callback
            //changes velocity for the next integration, not the old step.
            item.transform.position+=Vector3.up*(State.VelocityY*dt);
            if(State.InPickupRange(playerX))
            {
                if(Decision.kind==JunglePickupState.Kind.Health)
                {
                    if(State.TryCollectHealth(playerX,ref hp))
                    {
                        timers.Cancel(firstTimer);
                        var replacement=Resources.Load<Texture2D>("DinoCapGenerated/JungleBlitz/LaddhpFrame");
                        if(replacement==null)throw new InvalidOperationException("Missing original heal indicator.");
                        item.enabled=false;
                        var indicator=CreateVisual("JungleHealIndicator","LaddhpFrame",192,60,310);
                        indicator.transform.position=item.transform.position;item=indicator;
                        RetireBeam();
                        healTimer=timers.Arm(5,_=>{if(this!=null){State.Expire();item.enabled=false;RetireIfDone();}});
                        collected?.Invoke(Decision,new JunglePickupInventory.Outcome {itemAudio=true},State.X,item.transform.position.y);
                    }
                }
                else
                {
                    var outcome=apply(Decision);
                    State.FinishInventoryCollection(playerX);item.enabled=false;timers.Cancel(firstTimer);
                    //1AD48 puts L07 in this pickup's owned child container.
                    //1724CC/1719C4 recursively retire its children and timers
                    //when1B13C removes the collected pickup. Separate timer
                    //does not confer an independent lifetime (N255 supersession).
                    RetireBeam();
                    collected?.Invoke(Decision,outcome,State.X,item.transform.position.y);
                    RetireIfDone();
                }
            }
            State.Tick(dt);
        }
        void RetireIfDone(){if(!State.Active && !BeamActive)Destroy(gameObject);}
        void RetireBeam(){if(beam!=null)beam.enabled=false;timers.Cancel(beamTimer);}
        void OnDestroy()
        {
            if(timers!=null){timers.Cancel(firstTimer);timers.Cancel(beamTimer);timers.Cancel(healTimer);}
            foreach(var sprite in ownedSprites)if(sprite!=null)Destroy(sprite);
        }
    }
}
