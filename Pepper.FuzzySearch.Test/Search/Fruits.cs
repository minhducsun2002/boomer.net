using System.Collections;
using NUnit.Framework;

namespace Pepper.FuzzySearch.Tests.Search
{
    public class FuzzySearchTest
    {
        public class Fruits
        {
            public static IEnumerable FruitTestCases
            {
                get
                {
                    yield return new TestCaseData(new[] { "Apple", "Banana", "Orange" }, "Apple", "Apple").SetName("Apple => Apple");
                    yield return new TestCaseData(new[] { "Apple", "Banana", "Orange" }, "ran", "Orange").SetName("ran => Orange");
                    yield return new TestCaseData(new[] { "Apple", "Banana", "Orange" }, "nan", "Banana").SetName("nan => Banana");
                }
            }
        }

        [Test]
        [TestCaseSource(typeof(Fruits), nameof(Fruits.FruitTestCases))]
        public void Search(string[] fruits, string query, string output)
        {
            var fuse = new Fuse<string>(fruits, false, new StringFuseField<string>(s => s));
            var results = fuse.Search(query);
            Assert.AreEqual(results[0].Element, output);
        }

        [Test]
        [TestCaseSource(typeof(Book), nameof(Book.BookTestCases))]
        public void Search(Book[] books, string query, int index)
        {
            var fuse = new Fuse<Book>(books, false,
                new StringFuseField<Book>(book => book.Title),
                new StringFuseField<Book>(book => book.AuthorFirstName),
                new StringFuseField<Book>(book => book.AuthorLastName)
            );

            var result = fuse.Search(query);
            Assert.AreEqual(result[0].Element, books[index]);
        }
    }
}