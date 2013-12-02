namespace Spring.Mvc5QuickStart
{
    //intended to be instantiated as a singleton by the parent ApplicationContext so that it can keep track of its hit-count
    //  as the page is visited (NOTE: this impl. is for demo purposes only and is obviously NOT threadsafe!)
    public class Counter
    {
        private int _count;
        public int Count
        {
            get { return _count++; }
            set { _count = value; }
        }
    }
}