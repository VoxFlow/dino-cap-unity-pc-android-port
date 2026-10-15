using System;
using System.Collections.Generic;

namespace DinoCap.PortV2
{
    // Power2D Timer181FF8/182058, one-shot owner used by17A08/1802C.
    // Integer handles adapt native pointer identity. Equal deadline ordering
    // follows the original heap, deliberately without a stable-sort key.
    public sealed class NativeSceneTimerQueue
    {
        sealed class Entry {public int handle;public float deadline;public Action<int> callback;}
        readonly Queue<Entry> pending=new Queue<Entry>();
        readonly List<Entry> heap=new List<Entry>();
        readonly Queue<int> cancelled=new Queue<int>();
        int nextHandle;
        public float Clock {get;private set;}
        public int Count=>pending.Count+heap.Count;
        public void Cancel(int handle)=>cancelled.Enqueue(handle);
        public int Arm(float delay,Action<int> callback)
        {
            if(callback==null)throw new ArgumentNullException(nameof(callback));
            var e=new Entry {handle=++nextHandle,deadline=(float)(Clock+delay),callback=callback};
            pending.Enqueue(e);return e.handle;
        }
        public void Tick(float delta)
        {
            // Native processes additions before the positive-delta gate.
            while(pending.Count!=0)Insert(pending.Dequeue());
            while(cancelled.Count!=0)
            {
                int handle=cancelled.Dequeue();
                int i=heap.FindIndex(e=>e.handle==handle);
                if(i>=0)RemoveAt(i);
            }
            if(delta<=0)return;
            Clock=(float)(Clock+delta);
            while(heap.Count!=0 && heap[0].deadline<=Clock)
            {
                var e=heap[0];
                e.callback(e.handle);
                RemoveAt(0);
            }
        }
        void Insert(Entry e)
        {
            int i=heap.Count;heap.Add(e);
            while(i!=0)
            {
                int parent=(i-1)/2;
                if(heap[i].deadline>=heap[parent].deadline)break;
                Swap(i,parent);i=parent;
            }
        }
        void RemoveAt(int i)
        {
            int last=heap.Count-1;
            if(i==last){heap.RemoveAt(last);return;}
            heap[i]=heap[last];heap.RemoveAt(last);
            //1822A0 invokes PercolateUp then PercolateDown at the original
            //index after replacing it with the last node.
            int up=i;
            while(up!=0)
            {
                int parent=(up-1)/2;
                if(heap[up].deadline>=heap[parent].deadline)break;
                Swap(up,parent);up=parent;
            }
            while(i*2+1<heap.Count)
            {
                int child=i*2+1,right=child+1;
                if(right<heap.Count && heap[right].deadline<heap[child].deadline)child=right;
                if(heap[child].deadline>=heap[i].deadline)break;
                Swap(i,child);i=child;
            }
        }
        void Swap(int a,int b){var e=heap[a];heap[a]=heap[b];heap[b]=e;}
    }
}
