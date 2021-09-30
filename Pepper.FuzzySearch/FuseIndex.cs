using System;
using System.Collections.Generic;
using System.Linq;

namespace Pepper.FuzzySearch
{
    public class FuseField<T>
    {
        public delegate string StringExtractor(T value);

        internal StringExtractor Extractor;
        internal double Weight;
        public FuseField(StringExtractor extractor, double weight = 1F)
        {
            Extractor = extractor;
            Weight = weight;
        }
    }
    
    public class FuseIndex<T>
    {
        internal struct Record
        {
            public Dictionary<FuseField<T>, (string, double)> SubRecords;
            public T Element;
        }
        
        private bool isCreated = false;
        private readonly NormGenerator norm = new();
        public readonly List<FuseField<T>> Keys;
        internal Dictionary<int, Record> index = new(); 
        internal FuseIndex () {}

        public void AddElement(T element)
        {
            var values = Keys
                .Select(key => new { value = key.Extractor(element), key })
                .ToDictionary(_ => _.key, _ => (_.value, norm.Get(_.value)));
            index[(index.Count + 1) % int.MaxValue] = new Record { SubRecords = values, Element = element };
        }

        public FuseIndex(IEnumerable<FuseField<T>> keys)
        {
            Keys = keys.ToList();
        }

        public FuseIndex<T> Create(IEnumerable<T> elements)
        {
            if (!isCreated)
            {
                foreach (var element in elements)
                {
                    AddElement(element);
                }

                isCreated = true;
            }

            return this;
        }
    }
}