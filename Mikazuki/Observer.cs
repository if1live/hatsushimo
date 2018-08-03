using System.Linq;
using System.Collections.Generic;
using Mikazuki.NetChan;


namespace Mikazuki
{
    public class Observer
    {
        public int ID { get; private set; }
        public Session Session { get; private set; }

        public Observer(int id, Session session)
        {
            this.ID = id;
            this.Session = session;
        }
    }

    public class ObserverManager
    {
        List<Observer> observers = new List<Observer>();

        public void Add(Observer o) { observers.Add(o); }
        public void Remove(int id)
        {
            var remains = observers.Where(o => o.ID != id);
            observers = remains.ToList();
        }

        public int GetObservers(ref List<Observer> retval)
        {
            retval.Clear();
            retval.AddRange(observers);
            return retval.Count;
        }

        public void Update() { }
    }
}
